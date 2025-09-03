using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.IO;
using GameFlow;
using PassthroughCameraSamples;
using UnityEngine;
using UnityEngine.Networking;
using QuestCameraKit.OpenAI;
using Rehab.Scripts.Scenic;
using VideoKit;
using VideoKit.Clocks;
using VideoKit.Sources;

using Newtonsoft.Json;

/// <summary>
/// Manages video recording, frame capture, and AI-based exercise evaluation
/// Handles communication with Caduceus API for rehabilitation assessment
/// </summary>
[RequireComponent(typeof(VoiceCommandHandler))]
public class VideoRecordingManager : MonoBehaviour
{
    private VoiceCommandHandler _voiceCommandHandler;
    private ImageOpenAIConnector _imageOpenAIConnector;
    
    private Coroutine recordingCoroutine;
    private List<string> listOfImages = new List<string>();
    
    
    public List<Texture2D> textures = new List<Texture2D>();
    public string geminiApiKey;

    private VideoKitRecorder _videoKitRecorder;

    
    private void Start()
    {
        _voiceCommandHandler = GetComponent<VoiceCommandHandler>();
        _imageOpenAIConnector = GetComponent<ImageOpenAIConnector>();
        
    }
    
  

    #region ImagesQuery

    
    private Coroutine _takeImagesCoroutine;
    private string _instruction = "what is the action being performed";
    private List<Coroutine> _coroutines = new List<Coroutine>();

    /// <summary>
    /// Response structure from Caduceus API for exercise evaluation
    /// </summary>
    [System.Serializable]
    public class CaduceusResponse
    {
        public bool output;
        public string reasoning;
        public string compression_duration;
        public string gemini_duration;
    }

    /// <summary>
    /// Sets the current exercise instruction for evaluation
    /// </summary>
    public void SetInstruction(string instruction)
    {
        this._instruction = instruction;
    }

    /// <summary>
    /// Starts capturing frames at 2Hz and schedules multiple evaluation attempts
    /// </summary>
    [ContextMenu("TakeImages")]
    public void StartTakeImages()
    {
        textures.Clear();
        // Takes images on 10 hertz
        _takeImagesCoroutine = StartCoroutine(CaptureFramesAt2Hz());
        
        float[] delays = { 3f, 5f, 7f, 9f, 11f};

        foreach (float delay in delays)
        {
            Coroutine coroutine = StartCoroutine(SendImagesAfterDelay(delay));
            _coroutines.Add(coroutine);
        }
    }

    /// <summary>
    /// Stops the frame capture process
    /// </summary>
    public void StopCapturing()
    {
        
        if (_takeImagesCoroutine != null) StopCoroutine(_takeImagesCoroutine);
        _takeImagesCoroutine = null;
        textures.Clear();
    }

    /// <summary>
    /// Stops all ongoing image capture and evaluation processes
    /// </summary>
    public void StopAllImageSending()
    {

        foreach (Coroutine coroutine in _coroutines)
        {
            if (coroutine != null)
            {
                StopCoroutine(coroutine);
            }
        }

        _coroutines.Clear();
        textures.Clear();
    }

    /// <summary>
    /// Schedules an evaluation of captured frames after specified delay
    /// </summary>
    private IEnumerator SendImagesAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        Debug.Log("Start evaluating video for delay: " + delay + " with length texture " + textures.Count + " with instruction  " + _instruction);
        CheckAvatarStatus checkAvatarStatus = GameObject.FindWithTag("ScenicAvatar")?.GetComponent<CheckAvatarStatus>();
        if (checkAvatarStatus != null && !checkAvatarStatus.taskDone)
        {
            Coroutine sending = StartCoroutine(SendImagesToCaduceus(textures, _instruction));
            _coroutines.Add(sending);
        }
    }

    
    /// <summary>
    /// Stops the image capture process and all scheduled evaluations
    /// </summary>
    [ContextMenu("StopTakeImages")]
    public void StopTakeImages()
    {
        Debug.Log("Stop Taking images");
        StopCoroutine(_takeImagesCoroutine);
        _takeImagesCoroutine = null;
        StopAllImageSending();
    }
    
    
    
    
    /// <summary>
    /// Captures frames at 2Hz until reaching maximum count
    /// </summary>
    private IEnumerator CaptureFramesAt2Hz()
    {
        int count = 0;
        while (true)
        {
            count += 1;
            if (count > 100) break; 
            Texture2D frame = _voiceCommandHandler.CaptureImage();
            SaveFrame(frame);
            yield return new WaitForSeconds(1f);
        }
        yield return null;
    }
    
    /// <summary>
    /// Saves a captured frame to the textures list
    /// </summary>
    private void SaveFrame(Texture2D tex)
    {
        tex = _imageOpenAIConnector.ConvertToTextureReadable(tex);
        textures.Add(tex);
    }

    /// <summary>
    /// Sends captured frames to Caduceus API for exercise evaluation
    /// </summary>
    public IEnumerator SendImagesToCaduceus(List<Texture2D> textures, string instructions)
    {
        // Create multipart form
        List<IMultipartFormSection> formData = new List<IMultipartFormSection>();

        int totalBytes = 0;
        
        // Convert each texture to PNG bytes and add to form data
        for (int i = 0; i < textures.Count; i++)
        {
            byte[] imageBytes = textures[i].EncodeToPNG();
            totalBytes += imageBytes.Length;
            
            string fileName = $"frame_{i + 1:D2}.jpg"; // frame_01.jpg, frame_02.jpg, ...
            formData.Add(new MultipartFormFileSection("images", imageBytes, fileName, "image/jpeg"));
        }

        float totalMB = totalBytes / (800f * 800f);
        Debug.Log($"Total image size: {totalMB:F2} MB");
        
        // Add instructions as a form field
        formData.Add(new MultipartFormDataSection("instructions", instructions));
        
        Debug.Log("Sent to Caduceus at " + Time.time);
        // Send the request
        UnityWebRequest req = UnityWebRequest.Post(
            "https://api.reia-rehab.com/evaluate-video/",
            formData
        );
        Debug.Log("after send to Caduceus at " + Time.time);
        yield return req.SendWebRequest();
        Debug.Log("received from Caduceus at " + Time.time);
        if (req.result == UnityWebRequest.Result.Success)
        {
            string responseText = req.downloadHandler.text;
            Debug.Log("Caduceus API response:\n" + responseText);
            
            CaduceusResponse response = JsonConvert.DeserializeObject<CaduceusResponse>(responseText);
            Debug.Log("Output: " + response.output);
            Debug.Log("Reasoning: " + response.reasoning);
            
            CheckAvatarStatus checkAvatarStatus = GameObject.FindWithTag("ScenicAvatar")?.GetComponent<CheckAvatarStatus>();
            if (checkAvatarStatus != null)
            {
                checkAvatarStatus.feedback = $"output {response.output} | + {response.reasoning}";
                
                if (response.output)
                {
                    if (_takeImagesCoroutine != null)
                    {
                        StopCoroutine(_takeImagesCoroutine);
                        _takeImagesCoroutine = null;
                    }
                    checkAvatarStatus.taskDone = response.output;
                    yield return new WaitForSeconds(0.2f);
                    checkAvatarStatus.taskDone = false;
                    StopAllImageSending();    
                    Debug.Log("Stopped image capture coroutine because task is done.");
                }
            }

        }
        else
        {
            Debug.LogError("Failed to send to Caduceus API: " + req.error);
        }
        Resources.UnloadUnusedAssets();
    }

    private void OnDestroy()
    {
        if (_takeImagesCoroutine != null)
        {
            StopCoroutine(_takeImagesCoroutine);
        }
    }

    #endregion

}
