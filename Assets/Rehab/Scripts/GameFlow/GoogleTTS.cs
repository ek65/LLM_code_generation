using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using System.Text;
using System.IO;

public class GoogleTTS : MonoBehaviour
{
    private string apikey = API_KEY.google_api_key; // Replace with your API key
    private const string API_URL = "https://texttospeech.googleapis.com/v1/text:synthesize?key=";

    public AudioSource audioSource; // Assign an AudioSource in Unity Editor

    private Queue<string> ttsQueue = new Queue<string>();
    private bool isPlaying = false;
    
    private CheckAvatarStatus _checkAvatarStatus;

    private void Awake()
    {
        _checkAvatarStatus = GetComponent<CheckAvatarStatus>();
    }

    public void ConvertTextToSpeech(string text)
    {
        if (!string.IsNullOrEmpty(text))
        {
            ttsQueue.Enqueue(text);

            if (!isPlaying)
            {
                StartCoroutine(ProcessQueue());
            }
        }
    }

    private IEnumerator ProcessQueue()
    {
        isPlaying = true;

        while (ttsQueue.Count > 0)
        {
            string nextText = ttsQueue.Dequeue();
            yield return SendTextToSpeechRequest(nextText);
            _checkAvatarStatus.speakActionCount += 1;
            yield return new WaitForSeconds(1f); // Wait 2 seconds between audio clips
        }

        isPlaying = false;
    }

    private IEnumerator SendTextToSpeechRequest(string text)
    {
        // Create the request JSON
        string jsonPayload = "{\"input\":{\"text\":\"" + text + "\"}," +
                             "\"voice\":{\"languageCode\":\"en-US\",\"name\":\"en-US-Chirp-HD-O\"}," +
                             "\"audioConfig\":{\"audioEncoding\":\"MP3\", \"pitch\":\"0\", \"speakingRate\":\"0.9\"}}";

        // Convert JSON to byte array
        byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonPayload);

        // Send the request
        using (UnityWebRequest request = new UnityWebRequest(API_URL + apikey, "POST"))
        {
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");

            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                string responseJson = request.downloadHandler.text;
                yield return PlayAudioResponse(responseJson);
            }
            else
            {
                Debug.LogError("TTS API Error: " + request.error);
            }
        }
    }

    private IEnumerator PlayAudioResponse(string jsonResponse)
    {
        // Extract the base64 audio string from the JSON response
        string base64Audio = JsonUtility.FromJson<AudioResponse>(jsonResponse).audioContent;

        // Convert base64 to audio clip
        byte[] audioBytes = System.Convert.FromBase64String(base64Audio);
        yield return PlayAudioClip(audioBytes);
    }

    private IEnumerator PlayAudioClip(byte[] audioData)
    {
        string filePath = Path.Combine(Application.persistentDataPath, "tts_audio.mp3");
        File.WriteAllBytes(filePath, audioData);

        using (UnityWebRequest request = UnityWebRequestMultimedia.GetAudioClip("file://" + filePath, AudioType.MPEG))
        {
            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                audioSource.clip = DownloadHandlerAudioClip.GetContent(request);
                audioSource.Play();

                yield return new WaitUntil(() => !audioSource.isPlaying);
                File.Delete(filePath);
            }
            else
            {
                Debug.LogError("Failed to load audio clip: " + request.error);
            }
        }
    }

    [System.Serializable]
    private class AudioResponse
    {
        public string audioContent;
    }
}
