using System;
using System.Text;
using UnityEngine;
using OVRSimpleJSON;
using UnityEngine.Events;
using System.Collections;
using UnityEngine.Networking;
using System.Collections.Generic;
using Newtonsoft.Json;
using Rehab.Scripts.Scenic;
using Newtonsoft.Json.Linq;
using System.Linq;

namespace QuestCameraKit.OpenAI
{
    #region Enums and Extensions

    public enum TtsModel
    {
        Tts1,
        Tts1Hd,
    }

    public static class TtsModelExtensions
    {
        public static string EnumToString(this TtsModel model)
        {
            switch (model)
            {
                case TtsModel.Tts1:
                    return "tts-1";
                case TtsModel.Tts1Hd:
                    return "tts-1-hd";
                default:
                    Debug.Log(model + " is not a valid TTSModel.");
                    return "tts-1";
            }
        }
    }

    public enum TtsVoice
    {
        Alloy,
        Echo,
        Fable,
        Onyx,
        Nova,
        Shimmer
    }

    public enum OpenAIVisionModel
    {
        O1,
        Gpt4O,
        Gpt4OMini,
        Gpt4Turbo,
        Gpt3OMini,
        GptO4Mini
    }

    public static class OpenAIVisionModelExtensions
    {
        public static string ToModelString(this OpenAIVisionModel model)
        {
            return model switch
            {
                OpenAIVisionModel.O1 => "o1",
                OpenAIVisionModel.Gpt4O => "gpt-4o",
                OpenAIVisionModel.Gpt4OMini => "gpt-4o-mini",
                OpenAIVisionModel.Gpt4Turbo => "gpt-4-turbo",
                OpenAIVisionModel.Gpt3OMini => "o3-mini",
                OpenAIVisionModel.GptO4Mini => "o4-mini",
                _ => "gpt-4o"
            };
        }
    }

    public enum OpenAICommandMode
    {
        ImageAndText,
        ImageOnly,
        TextOnly
    }

    #endregion

    public class ImageOpenAIConnector : MonoBehaviour
    {
        [Header("API & Model Settings")] 
        [Tooltip("Your OpenAI API key.")]
        private string apiKey = "";
        [Tooltip("Your gemini API key.")]
        private string geminiApiKey = "AIzaSyChh5bRpx-sa62pvxhuUspSRxk4WoDsfMI";
        // [SerializeField] private string geminiApiKey;

        [SerializeField] private OpenAIVisionModel selectedModel = OpenAIVisionModel.GptO4Mini;

        [Header("Command Settings")] public OpenAICommandMode commandMode = OpenAICommandMode.ImageAndText;
        [SerializeField] private string instructions = "";
        [SerializeField] private TtsManager ttsManager;
        /*[SerializeField] private SttManager sttManager;*/

        [Header("Events")] public UnityEvent<string> onResponseReceived;

        [Header("Processing Sound")]
        [Tooltip("Assign an AudioSource that plays a waiting sound.")]
        [SerializeField]
        private AudioSource processingAudioSource;

        private const string OutputFormat = "mp3";

        [Serializable]
        private class TtsPayload
        {
            public string model;
            public string input;
            public string voice;
            public string responseFormat;
            public float speed;
        }

        #region Audio Feedback

        private void Awake()
        {
            apiKey = API_KEY.openai_api_key;
            /*sttManager.onRequestSent.AddListener(StartProcessingSound);*/
            onResponseReceived.AddListener(StopProcessingSound);
        }

        private void OnDestroy()
        {
            /*sttManager.onRequestSent.RemoveListener(StartProcessingSound);*/
            onResponseReceived.RemoveListener(StopProcessingSound);
        }

        private void StartProcessingSound()
        {
            if (!processingAudioSource)
            {
                return;
            }

            processingAudioSource.loop = true;
            processingAudioSource.Play();
        }

        private void StopProcessingSound(string response)
        {
            if (processingAudioSource)
            {
                processingAudioSource.Stop();
            }
        }

        #endregion

        #region Text-To-Speech Functionality

        /// <summary>
        /// Requests text-to-speech audio from OpenAI.
        /// </summary>
        /// <param name="text">Text to be synthesized.</param>
        /// <param name="onSuccess">Callback with the resulting audio data.</param>
        /// <param name="onError">Callback with error message.</param>
        /// <param name="model">TTS model to use.</param>
        /// <param name="voice">Voice to use.</param>
        /// <param name="speed">Speed factor for the speech.</param>
        public IEnumerator RequestTextToSpeech(string text, Action<byte[]> onSuccess, Action<string> onError,
            TtsModel model = TtsModel.Tts1, TtsVoice voice = TtsVoice.Alloy, float speed = 1f)
        {
            Debug.Log("Sending new request to OpenAI TTS.");

            var payload = new TtsPayload
            {
                model = model.EnumToString(),
                input = text,
                voice = voice.ToString().ToLower(),
                responseFormat = OutputFormat,
                speed = speed
            };

            var jsonPayload = JsonUtility.ToJson(payload);

            using var request = new UnityWebRequest("https://api.openai.com/v1/audio/speech", "POST");
            var bodyRaw = Encoding.UTF8.GetBytes(jsonPayload);

            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");
            request.SetRequestHeader("Authorization", "Bearer " + apiKey);

            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.ConnectionError ||
                request.result == UnityWebRequest.Result.ProtocolError)
            {
                Debug.LogError("TTS Request Error: " + request.error);
                onError?.Invoke(request.error);
            }
            else
            {
                var audioData = request.downloadHandler.data;
                onSuccess?.Invoke(audioData);
            }
        }

        #endregion

        #region Image and Chat Functionality

        /// <summary>
        /// Sends an image (and optionally text) to OpenAI.
        /// </summary>
        /// <param name="image">The image to send.</param>
        /// <param name="command">The command text.</param>


        public Texture2D ConvertToTextureReadable(Texture2D sourceTexture)
        {
            // Ensure the texture is uncompressed and readable (RGBA32 is a safe uncompressed format)
            Texture2D readableTexture = new Texture2D(sourceTexture.width, sourceTexture.height, TextureFormat.RGBA32, false);

            // Copy the texture to a RenderTexture (required to access non-readable textures)
            RenderTexture renderTexture = RenderTexture.GetTemporary(sourceTexture.width, sourceTexture.height, 0, RenderTextureFormat.ARGB32);
            Graphics.Blit(sourceTexture, renderTexture);

            // Read pixels from the RenderTexture into the new Texture2D
            RenderTexture.active = renderTexture;
            readableTexture.ReadPixels(new Rect(0, 0, sourceTexture.width, sourceTexture.height), 0, 0);
            readableTexture.Apply();

            // Clean up
            RenderTexture.active = null;
            RenderTexture.ReleaseTemporary(renderTexture);

            return readableTexture; // Now it's readable and uncompressed
        }

        private IEnumerator UploadImage(Texture2D image, string apiUrl, System.Action<Dictionary<string, object>> callback)
        {
            image = ConvertToTextureReadable(image);
            byte[] imageBytes = image.EncodeToJPG();

            // Create a form and add the image as a file
            WWWForm form = new WWWForm();
            form.AddBinaryData("file", imageBytes, "image.jpg", "image/jpeg");

            using (UnityWebRequest request = UnityWebRequest.Post(apiUrl, form))
            {

                // image = ConvertToTextureReadable(image);

                // using (UnityWebRequest request = new UnityWebRequest(apiUrl, "POST"))
                // {
                //     byte[] bodyRaw = image.EncodeToJPG();
                //     request.uploadHandler = new UploadHandlerRaw(bodyRaw);
                //     request.downloadHandler = new DownloadHandlerBuffer();
                //     //request.SetRequestHeader("Content-Type", "image/jpeg");
                //     //request.SetRequestHeader("Content-Type", "image/jpeg");
                //     request.SetRequestHeader("Content-Type", "application/json");

                // Debug.Log($"JPEG size: {bodyRaw.Length} bytes");
                // Debug.Log(bodyRaw);

                yield return request.SendWebRequest();

                if (request.result == UnityWebRequest.Result.Success)
                {
                    Debug.Log($"{request.error}\nResponse: {request.downloadHandler.text}");

                    try
                    {
                        var jsonResponse = JsonConvert.DeserializeObject<Dictionary<string, object>>(request.downloadHandler.text);
                        callback?.Invoke(jsonResponse);
                    }
                    catch (Exception ex)
                    {
                        Debug.LogError("Error parsing Image response: " + ex.Message);
                        callback?.Invoke(null);
                    }
                }
                else
                {
                    Debug.LogError("Error uploading Image: " + request.error);
                    callback?.Invoke(null);
                }
            }
        }

        public string SaveImageToGoogleCloud(Texture2D image, string custom_id = null)
        {
            string apiUrl = "https://api.reia-rehab.com/upload/file/";
            if (custom_id != null)
            {
                apiUrl = "https://api.reia-rehab.com/upload/file/?upload_id=" + custom_id;
            }

            string image_id = "";
            StartCoroutine(UploadImage(image, apiUrl, (response) =>
                                {
                                    if (response != null)
                                    {
                                        string data = $"{{ \"file_id\": \"{response["file_id"].ToString()}\" }}";
                                        image_id = response["file_id"].ToString();
                                    }
                                    else
                                    {
                                        Debug.LogError("Upload failed.");
                                        image_id = "";
                                    }
                                }));
            return image_id;

        }

        public Coroutine SendImage(Texture2D image, string command)
        {
            Coroutine temp = StartCoroutine(SendImageRequest(image, command));
            // Debug.Log("IMAGEQWEQW ");
            return temp;
        }



    private IEnumerator SendImageRequest(Texture2D image, string command)
        {
            GameObject scenicAvatar = GameObject.FindWithTag("ScenicAvatar");
            if (scenicAvatar)
            {
                CheckAvatarStatus checkAvatarStatus = scenicAvatar.GetComponent<CheckAvatarStatus>();
                if (checkAvatarStatus == null)
                {
                    yield break;
                }
            }
            else
            {
                Debug.LogError("ScenicAvatar not found in the scene.");
                yield break;
            }

            var base64Image = "";
            if (commandMode == OpenAICommandMode.ImageAndText || commandMode == OpenAICommandMode.ImageOnly)
            {
                var processedImage = (image.width == 512 && image.height == 512 && image.format == TextureFormat.RGBA32)
                    ? image
                    : ResizeTexture(image, 512, 512);
                var imageBytes = processedImage.EncodeToJPG();
                if (imageBytes == null || imageBytes.Length == 0)
                {
                    Debug.LogError(
                        "Failed to encode image to JPG. Check that the texture is readable and uncompressed.");
                    yield break;
                }

                base64Image = Convert.ToBase64String(imageBytes);
            }

            var contentElements = new List<string>();

            if (!string.IsNullOrEmpty(instructions))
            {
                contentElements.Add($"{{\"type\":\"text\",\"text\":\"{EscapeJson(instructions)}\"}}");
            }
            switch (commandMode)
            {
                case OpenAICommandMode.ImageAndText:
                    contentElements.Add($"{{\"type\":\"text\",\"text\":\"{EscapeJson(command)}\"}}");
                    contentElements.Add(
                        $"{{\"type\":\"image_url\",\"image_url\":{{\"url\":\"data:image/jpeg;base64,{base64Image}\"}}}}");
                    break;
                case OpenAICommandMode.ImageOnly:
                    contentElements.Add(
                        $"{{\"type\":\"image_url\",\"image_url\":{{\"url\":\"data:image/jpeg;base64,{base64Image}\"}}}}");
                    break;
                case OpenAICommandMode.TextOnly:
                    contentElements.Add($"{{\"type\":\"text\",\"text\":\"{EscapeJson(command)}\"}}");
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            
            var contentJson = string.Join(",", contentElements);
            var payloadJson = "{" +
                              $"\"model\":\"{selectedModel.ToModelString()}\"," +
                              "\"messages\":[{" +
                              $"\"role\":\"user\",\"content\":[{contentJson}]" +
                              "}]," +
                              "\"max_tokens\":3000" +
                              "}";

            using var request = new UnityWebRequest("https://api.openai.com/v1/chat/completions", "POST");
            var bodyRaw = Encoding.UTF8.GetBytes(payloadJson);

            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");
            request.SetRequestHeader("Authorization", "Bearer " + apiKey);

            StartProcessingSound();
            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.ConnectionError ||
                request.result == UnityWebRequest.Result.ProtocolError)
            {
                Debug.LogError($"Error sending request: {request.error} (Response code: {request.responseCode})");
            }
            else
            {
                var jsonResponse = JSON.Parse(request.downloadHandler.text);
                var responseContent = jsonResponse["choices"][0]["message"]["content"].Value;
                var dummy = request.downloadHandler.text;


                onResponseReceived?.Invoke(responseContent);
                StopProcessingSound("");

                Debug.Log(responseContent);
                Debug.Log(dummy);
                if (scenicAvatar)
                {
                    CheckAvatarStatus checkAvatarStatus = scenicAvatar.GetComponent<CheckAvatarStatus>();
                    ActionAPI actionAPI = scenicAvatar.GetComponent<ActionAPI>();
                    if (checkAvatarStatus != null)
                    {
                        responseContent = responseContent.Trim();
                        // Split into sentences 
                        string[] sentences =
                            responseContent.Split(new[] { '.', '!', '?' }, StringSplitOptions.RemoveEmptyEntries);

                        string decision = "Unknown";
                        string feedback = responseContent; // Default to the full response

                        if (sentences.Length > 0)
                        {

                            string firstSentence = sentences[0].Trim(); // Get first sentence and trim spaces

                            if (firstSentence.Contains("Yes", StringComparison.OrdinalIgnoreCase))
                            {
                                decision = "Yes";
                                feedback = responseContent.Replace(firstSentence, "").Trim(); // Remove first sentence

                                // Use a thread-safe approach to update the status

                                // string image_id = "";

                                // // TODO: send image to google cloud
                                // image_id = SaveImageToGoogleCloud(image);

                                // checkAvatarStatus.taskDone = true;
                                // checkAvatarStatus.SetImageID(image_id);
                                // actionAPI.DisposeQueries();

                            }
                            else if (firstSentence.Contains("No", StringComparison.OrdinalIgnoreCase))
                            {
                                decision = "No";
                                feedback = responseContent.Replace(firstSentence, "").Trim(); // Remove first sentence

                                checkAvatarStatus.taskDone = false;
                                checkAvatarStatus.SetImageID("");
                            }

                            checkAvatarStatus.feedback = responseContent;

                        }

                    }

                }


            }
        }


        #endregion

        #region Utility Methods

        /// <summary>
        /// Resizes or converts the given texture to a 512x512 uncompressed RGBA32 texture.
        /// </summary>
        private Texture2D ResizeTexture(Texture2D source, int targetWidth, int targetHeight)
        {
            var rt = RenderTexture.GetTemporary(targetWidth, targetHeight);
            rt.filterMode = FilterMode.Bilinear;
            var previous = RenderTexture.active;
            RenderTexture.active = rt;
            Graphics.Blit(source, rt);

            var result = new Texture2D(targetWidth, targetHeight, TextureFormat.RGBA32, false);
            result.ReadPixels(new Rect(0, 0, targetWidth, targetHeight), 0, 0);
            result.Apply();

            RenderTexture.active = previous;
            RenderTexture.ReleaseTemporary(rt);
            return result;
        }

        /// <summary>
        /// Escapes double quotes in strings so that they can be safely embedded in JSON.
        /// </summary>
        private string EscapeJson(string input)
        {
            return input.Replace("\"", "\\\"");
        }

        private string EscapeJsonString(string input)
        {
            return input.Replace("\\", "\\\\").Replace("\"", "\\\"");
        }
        #endregion
    }
}