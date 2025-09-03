using System.Collections;
using System.IO;
using System.Net.Http;
using UnityEngine;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using Rehab.Scripts.Scenic;

/// <summary>
/// Response structure for Whisper API transcription results
/// </summary>
[Serializable]
public class TranscriptionResponse
{
    public string text;
}

/// <summary>
/// Response structure for OpenAI Chat API completions
/// </summary>
[Serializable]
public class ChatCompletionResponse
{
    public Choice[] choices;
}

[Serializable]
public class Choice
{
    public Message message;
}

[Serializable]
public class Message
{
    public string role;
    public string content;
}

/// <summary>
/// Utility class for WAV audio file processing
/// </summary>
public class WAV
{
    public float[] LeftChannel;

    public WAV(byte[] wav)
    {
        int channels = wav[22]; // Usually 1 or 2
        int sampleRate = BitConverter.ToInt32(wav, 24);
        int pos = 44; // Start of data chunk
        int samples = (wav.Length - pos) / 2;

        LeftChannel = new float[samples];
        for (int i = 0; i < samples; i++)
        {
            short sample = BitConverter.ToInt16(wav, pos);
            LeftChannel[i] = sample / 32768f;
            pos += 2;
        }
    }
}

/// <summary>
/// Structure for storing patient condition analysis results
/// </summary>
[Serializable]
public class AnalysisResult
{
    public string pain;
    public string fatigue;
    public string dizziness;
    public string anything;
}

/// <summary>
/// Manages patient safety monitoring through audio analysis and AI-powered condition assessment
/// Handles continuous audio recording, transcription, and analysis of patient responses
/// </summary>
public class PatientSafetyManager : MonoBehaviour
{
    private string apiKey = API_KEY.openai_api_key;
    
    // AI prompt templates for different analysis stages
    public string painPrompt = "You are a helpful analyzer. We will provide transcriptions, and your task is to determine whether there is any indication of pain, fatigue, or dizziness." +
                               "\nLook for both direct and indirect expressions related to these categories:" +
                               "\n- Pain: expressions of physical or emotional suffering (e.g., 'it hurts', 'in agony', 'can't take this anymore')." +
                               "\n- Fatigue: signs of tiredness or low energy (e.g., 'I'm exhausted', 'so tired', 'worn out')." +
                               "\n- Dizziness: signs of imbalance or feeling lightheaded (e.g., 'dizzy', 'light-headed', 'the room is spinning')." +
                               "\n- anything related to body or physical condition are detected indicating the transcription do not feel good, return true. Otherwise, return false." +
                               "\n\nRespond only with a single boolean value: true or false." +
                               "\nDo not add any explanation, apology, or conversational text. Just return the boolean. For example: true" +
                               "\n Here is the transcription:";

    // Audio recording configuration
    private AudioClip recordedClip;
    private string micDevice;
    private float startTime;
    private int sampleRate = 16000;
    private int lengthSec = 3599;
    private int interval = 3;
    private float _lastResposneTime = 0f;
    public bool isRecording = false;

    // Patient interaction state
    public int questionCount = 0;
    public List<String> transcriptions = new List<string>();
    public List<string> bodyPartList = new List<string>();
    public Dictionary<string, string> painBodyPartsDict = new Dictionary<string, string>();
    private string prevBodyPart; 
    
    // Coroutine management
    private Coroutine patientStatusCoroutine = null;
    private Coroutine captureAudioAtIntervalsCoroutine = null;
    public AudioSource audioSource;

    private void Awake()
    {
        apiKey = API_KEY.openai_api_key;
    }
    
    /// <summary>
    /// Initiates continuous patient monitoring through audio recording and analysis
    /// </summary>
    public void StartRecording()
    {
        if (patientStatusCoroutine != null)
        {
            StopCoroutine(patientStatusCoroutine);
            patientStatusCoroutine = null;
        }
        
        if (questionCount == 0)
        {
            patientStatusCoroutine = StartCoroutine(CheckPatientStatusRoutine());
        }

        if (captureAudioAtIntervalsCoroutine != null)
        {
            StopCoroutine(captureAudioAtIntervalsCoroutine);
            captureAudioAtIntervalsCoroutine = null;
        }

        captureAudioAtIntervalsCoroutine = StartCoroutine(CaptureAudioAtIntervals());

        Debug.Log("StartRecording called");
        transcriptions.Clear();
        isRecording = true;
        _lastResposneTime = Time.time;
    }
    
    /// <summary>
    /// Continuously monitors patient status by analyzing recent transcriptions
    /// </summary>
    IEnumerator CheckPatientStatusRoutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(8f);
            
            int count = transcriptions.Count;
            if (count > 0)
            {
                Debug.Log("Checking patient status last 4 transcriptions");
                int start = Mathf.Max(0, count - 4);
                string lastFourCombined = string.Join(" ", transcriptions.GetRange(start, count - start));
                AnalyzeTranscriptionResult(lastFourCombined);
            }
        }
    }

    void Start()
    {
        SetupRecording();
    }
    
    /// <summary>
    /// Stops all recording and analysis processes
    /// </summary>
    public void StopRecording()
    {
        Debug.Log("Stop recording");
        isRecording = false;
        transcriptions.Clear();
        
        if (captureAudioAtIntervalsCoroutine != null)
        {
            StopCoroutine(captureAudioAtIntervalsCoroutine);
            captureAudioAtIntervalsCoroutine = null;
        }
        if (patientStatusCoroutine != null)
        {
            StopCoroutine(patientStatusCoroutine);
            patientStatusCoroutine = null;
        }
    }
    
    /// <summary>
    /// Initializes audio recording device and settings
    /// </summary>
    public void SetupRecording()
    {
#if !UNITY_WEBGL
        if (Microphone.devices.Length <= 0)
        {
            Debug.LogError("[SafetyManager] No microphone found.");
            return;
        }

        micDevice = Microphone.devices[0];
        if (micDevice == null)
        {
            Debug.LogError("[SafetyManager] Microphone found is null.");
            return;
        }

        Debug.Log("[SafetyManager] Start recording with device: " + micDevice);
        recordedClip = Microphone.Start(micDevice, true, lengthSec, sampleRate);
        startTime = Time.realtimeSinceStartup;
#endif
    }

    /// <summary>
    /// Checks if audio file contains significant audio content above threshold
    /// </summary>
    public bool IsWavFileLoud(string path, float threshold = 0.01f)
    {
        byte[] wavData = File.ReadAllBytes(path);
        WAV wav = new WAV(wavData);
        float[] samples = wav.LeftChannel;

        float peak = samples.Max(x => Mathf.Abs(x));

        return peak > threshold;
    }
    
    /// <summary>
    /// Captures audio at regular intervals and processes for analysis
    /// </summary>
    IEnumerator CaptureAudioAtIntervals()
    {
        while (true)
        {
            yield return new WaitForSeconds(interval);
            string filePath = SaveRecording("recording.wav");
            if (audioSource && audioSource.isPlaying)
            {
                _lastResposneTime = Time.time;
            }
            Debug.Log($"{_lastResposneTime} + {Time.time - _lastResposneTime} + {interval} seconds");
            
            // Handle exercise-specific recording logic
            if (questionCount != 0 && Time.time - _lastResposneTime > interval * 1.5 )
            {
                AnalyzeTranscriptionResult(string.Join(" ", transcriptions));
                StopRecording();
                yield break;
            }

            if (!string.IsNullOrEmpty(filePath))
            {
                if (IsWavFileLoud(filePath))
                {
                    SendAudioClipForAnalysis(filePath);
                    _lastResposneTime = Time.time;
                }
                else
                    Debug.Log("[SafetyManager] Wav file louder failed.");
            }
        }
    }

    /// <summary>
    /// Sends audio clip to Whisper API for transcription
    /// </summary>
    async void SendAudioClipForAnalysis(string filePath)
    {
#if !UNITY_WEBGL
        using (var client = new HttpClient())
        {
            var formData = new MultipartFormDataContent();
            byte[] audioData = File.ReadAllBytes(filePath);
            formData.Add(new ByteArrayContent(audioData), "file", Path.GetFileName(filePath));
            formData.Add(new StringContent("whisper-1"), "model");

            client.DefaultRequestHeaders.Add("Authorization", $"Bearer {apiKey}");

            try
            {
                var response = await client.PostAsync("https://api.openai.com/v1/audio/transcriptions", formData);
                var result = await response.Content.ReadAsStringAsync();

                if (!string.IsNullOrEmpty(result))
                {
                    TranscriptionResponse transcription = JsonConvert.DeserializeObject<TranscriptionResponse>(result);
                    Debug.Log("[safety transcriptioon] " + transcription);
                    this.transcriptions.Add(transcription.text);
                }
                else
                {
                    Debug.LogError("[SafetyManager] Unexpected response format: " + result);
                }
            }
            catch (HttpRequestException e)
            {
                Debug.LogError("[SafetyManager] Error transcribing audio: " + e.Message);
            }
        }
#endif
    }
    
    /// <summary>
    /// Analyzes transcribed text for patient condition indicators using GPT-4
    /// </summary>
    async void AnalyzeTranscriptionResult(string transcript)
    {
#if !UNITY_WEBGL
        using (var client = new HttpClient())
        {
            // Configure prompt based on question stage
            if (questionCount == 0)
            {
                painPrompt = "You are a helpful analyzer. We will provide transcriptions, and your task is to determine whether there is any indication of pain, fatigue, or dizziness." +
                               "\nLook for both direct and indirect expressions related to these categories:" +
                               "\n- Pain: expressions of physical or emotional suffering (e.g., 'it hurts', 'in agony', 'can't take this anymore')." +
                               "\n- Fatigue: signs of tiredness or low energy (e.g., 'I'm exhausted', 'so tired', 'worn out')." +
                               "\n- Dizziness: signs of imbalance or feeling lightheaded (e.g., 'dizzy', 'light-headed', 'the room is spinning')." +
                               "\n- anything related to body or physical condition are detected indicating the transcription do not feel good, return true. Otherwise, return false." +
                               "\n\nRespond only with a single boolean value: true or false." +
                               "\nDo not add any explanation, apology, or conversational text. Just return the boolean. For example: true" +
                               "\n Here is the transcription:";
            } else if (questionCount == 1)
            {
                painPrompt =
                    "You are a health assistant analyzing a patient's response to a question about pain.\n\n" +
                    "The question asked was:\n" +
                    "\"Did you feel any physical pain or discomfort during or after the exercise? " +
                    "For example, things like pain, fatigue, dizziness, or anything unusual?\"\n\n" +
                    "Your task is to analyze the patient's response and determine if they reported any physical discomfort.\n\n" +
                    "If the patient mentions any discomfort — such as pain, fatigue, dizziness, tightness, nausea, or anything unusual or even Yes— return:\n" +
                    "true\n\n" +
                    "If the patient clearly indicates no discomfort or gives a neutral/positive response, return:\n" +
                    "false\n\n" +
                    "Respond with only one word in all lower letters, no capital:\n" +
                    "true or false";
            } else if (questionCount == 2)
            {
                painPrompt =
                    "You are a health assistant analyzing a patient's response to identify specific body parts where they felt pain or discomfort.\n\n" +
                    "The patient was asked:\n" +
                    "\"Can you tell where you felt the pain or discomfort in your body?\"\n\n" +
                    "Your task is to extract all body parts mentioned by the patient where pain or discomfort was reported.\n\n" +
                    "If the patient specifies a side, such as 'left shoulder' or 'right knee', make sure to include the side in the result.\n" +
                    "If both sides are mentioned, include both (e.g., 'left shoulder|right shoulder').\n\n" +
                    "Return the names of body parts in lowercase, and separate each with a vertical bar '|'.\n" +
                    "For example:\n" +
                    "- 'My left shoulder and lower back hurt' → left shoulder|lower back\n" +
                    "- 'Pain in both knees and right elbow' → left knee|right knee|right elbow\n\n" +
                    "If the patient uses phrases like 'both knees' or 'each shoulder', interpret and split them accordingly.\n" +
                    "If no body part is mentioned or it's unclear, return an empty string.\n\n" +
                    "Output only the list of body parts separated by '|', with no extra text.";
            }
            else
            {
                painPrompt =
                    "You are a health assistant analyzing a patient's response to a question about their pain.\n\n" +
                    "The patient was asked:\n" +
                    $"\"\"Can you please describe when and what kind of pain you feel in your [prevBodyPart]? On a scale from 1 to 10, where 1 means no pain and 10 means the worst pain imaginable, how would you rate it?\"\"\n\n" +
                    "Your task is to extract the patient's own description of the pain, keeping the meaning and tone as close as possible to their original words.\n\n" +
                    "Ignore any parts of the response that are not in English. If there are mixed languages, only consider the English portion.\n\n" +
                    "Rephrase only to improve grammar or clarity if needed — do NOT exaggerate, add information, or speculate beyond what the patient said.\n\n" +
                    "You must return the following format: description of the pain|scale. For example: sharp pain on joint and muscle, minorly hurt|6. Another example: minor pain|2.";
            }

            // Send request to GPT-4
            var requestBody = new
            {
                model = "gpt-4o-mini",
                messages = new[]
                {
                    new { role = "user", content = $"{painPrompt} : {transcript}" },
                }
            };

            string jsonBody = JsonConvert.SerializeObject(requestBody);
            var content = new StringContent(jsonBody, Encoding.UTF8, "application/json");
            client.DefaultRequestHeaders.Add("Authorization", $"Bearer {apiKey}");

            try
            {
                var response = await client.PostAsync("https://api.openai.com/v1/chat/completions", content);
                string result = await response.Content.ReadAsStringAsync();

                if (!string.IsNullOrEmpty(result))
                {
                    ChatCompletionResponse parsedResponse = JsonUtility.FromJson<ChatCompletionResponse>(result);
                    if (parsedResponse != null && parsedResponse.choices != null && parsedResponse.choices.Length > 0)
                    {
                        GameObject ScenicAvatar = GameObject.FindWithTag("ScenicAvatar");
                        
                        // Handle different stages of patient interaction
                        if (questionCount == 0)
                        {
                            string painStatus = parsedResponse.choices[0].message.content;
                            Debug.Log("[SafetyManager] check patient status Transcript: " + transcript);
                            
                            if (ScenicAvatar && painStatus == "true")
                            {
                                CheckAvatarStatus checkAvatarStatus = ScenicAvatar.GetComponent<CheckAvatarStatus>();
                                if (checkAvatarStatus != null)
                                {
                                    checkAvatarStatus.stopProgram = true;
                                }
                                Debug.Log("[SafetyManager] Patient felt pain" + ScenicAvatar);
                                transcriptions.Clear();
                                StopRecording();
                            }
                        }
                        else if (questionCount == 1)
                        {
                            string painStatus = parsedResponse.choices[0].message.content;
                            Debug.Log("[SafetyManager] check patient status Transcript: " + transcript);
                            if (painStatus == "true")
                            {
                                Debug.Log("[SafetyManager] Patient indeed felt pain" + ScenicAvatar);
                                ScenicAvatar.GetComponent<ActionAPI>().AskQuestion("Which body part you feel pain?");
                                StopRecording();
                            }
                            else
                            {
                                ScenicAvatar.GetComponent<ActionAPI>().Speak("Okay, that's end of the exercise.");
                                AnalysisResult analysisResult = new AnalysisResult();
                                analysisResult.dizziness = "Not mentioned";
                                analysisResult.fatigue = "Not mentioned";
                                analysisResult.anything = "Not mentioned";
                                analysisResult.pain = "Not mentioned";
                                ScenicAvatar.GetComponent<CheckAvatarStatus>().UpdateStatus(analysisResult);
                                StopRecording();
                            }
                        }
                        else if (questionCount == 2)
                        {
                            Debug.Log("question count 2");
                            string bodyParts = parsedResponse.choices[0].message.content;
                            bodyPartList = bodyParts
                                .Split('|')
                                .Where(part => !string.IsNullOrWhiteSpace(part))
                                .Select(part => part.Trim())
                                .ToList();
                            if (bodyPartList.Count > 0)
                            {
                                prevBodyPart = bodyPartList[0];
                                bodyPartList.RemoveAt(0);
                                ScenicAvatar.GetComponent<ActionAPI>().AskQuestion($"Tell me when you felt pain, and on a scale from 1 to 10, where 10 means the worst pain, how would you rate it?");
                                StopRecording();
                            }
                            else
                            {
                                ScenicAvatar.GetComponent<ActionAPI>().Speak("Okay. That's end of the exercise.");
                                AnalysisResult analysisResult = new AnalysisResult();
                                analysisResult.dizziness = "Not mentioned";
                                analysisResult.fatigue = "Not mentioned";
                                analysisResult.anything = "Not mentioned";
                                analysisResult.pain = "Not mentioned";
                                ScenicAvatar.GetComponent<CheckAvatarStatus>().UpdateStatus(analysisResult);
                                StopRecording();
                            }
                        }
                        else if (questionCount >= 2)
                        {
                            Debug.Log($"question count {questionCount}");
                            string painReport = parsedResponse.choices[0].message.content;
                            painBodyPartsDict[prevBodyPart] = painReport;
                            if (bodyPartList.Count > 0)
                            {
                            prevBodyPart = bodyPartList[0];
                            bodyPartList.RemoveAt(0);
                            StopRecording();
                            ScenicAvatar.GetComponent<ActionAPI>().AskQuestion($"\"Can you describe when and what kind of pain you feel in your {prevBodyPart}? also on a scale from 1 to 10, where 1 means no pain and 10 means the worst pain, how would you rate it?\"");
                            }
                            else
                            { 
                                Debug.Log($"Done asking question and record");
                                ScenicAvatar.GetComponent<ActionAPI>().Speak("Okay. That's end of the exercise.");
                                AnalysisResult analysisResult = new AnalysisResult();
                                analysisResult.dizziness = "Not mentioned";
                                analysisResult.fatigue = "Not mentioned";
                                analysisResult.anything = "Not mentioned";
                                analysisResult.pain = string.Join("\n", painBodyPartsDict.Select(kv => $"{kv.Key}| {kv.Value}"));
                                if (analysisResult.pain == "") analysisResult.pain = "Not mentioned";
                                ScenicAvatar.GetComponent<CheckAvatarStatus>().UpdateStatus(analysisResult);
                                StopRecording();
                            }
                        }
                    }
                }
                else
                {
                    Debug.LogError("[SafetyManager] Empty response from OpenAI API.");
                }
            }
            catch (HttpRequestException e)
            {
                Debug.LogError("[SafetyManager] Error communicating with GPT: " + e.Message);
            }
        }
#endif
    }

    /// <summary>
    /// Saves the current audio recording to a WAV file
    /// </summary>
    public string SaveRecording(string fileName)
    {
        if (recordedClip == null)
        {
            Debug.LogError("[SafetyManager] No recording found to save.");
            return null;
        }

        string filePath = Path.Combine(Application.persistentDataPath, fileName);
        AudioClip lastClip = GetLastIntervalClip(recordedClip, interval);
        WavUtility.Save(filePath, lastClip);

        return filePath;
    }

    /// <summary>
    /// Extracts the last N seconds from an audio clip
    /// </summary>
    private AudioClip GetLastIntervalClip(AudioClip clip, int lastSeconds)
    {
        int position = Microphone.GetPosition(micDevice);
        int samples = sampleRate * lastSeconds;

        if (position - samples < 0)
            return clip; // If there aren't enough samples, return the whole clip

        float[] data = new float[samples];
        clip.GetData(data, position - samples);

        AudioClip trimmedClip = AudioClip.Create("trimmed", samples, clip.channels, clip.frequency, false);
        trimmedClip.SetData(data, 0);

        return trimmedClip;
    }

    /// <summary>
    /// Cleans up resources when application quits
    /// </summary>
    void OnApplicationQuit()
    {
#if !UNITY_WEBGL
        StopRecording();
        DeleteRecordingFile();
#endif
    }

    /// <summary>
    /// Removes temporary recording file
    /// </summary>
    private void DeleteRecordingFile()
    {
#if !UNITY_WEBGL
        string filePath = Path.Combine(Application.persistentDataPath, "recording.wav");

        if (File.Exists(filePath))
        {
            File.Delete(filePath);
            Debug.Log("[SafetyManager] Deleted recording file: " + filePath);
        }
#endif
    }
}