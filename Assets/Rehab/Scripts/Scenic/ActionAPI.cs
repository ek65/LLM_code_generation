using System;
using System.Collections.Generic;
using QuestCameraKit.OpenAI;
using UnityEngine;
using System.Collections;
using System.Threading;
using System.Threading.Tasks;
using Unity.VisualScripting;
using UnityEngine.Serialization;

namespace Rehab.Scripts.Scenic
{
    /// <summary>
    /// Manages rehabilitation actions and interactions between the patient and the system
    /// Handles voice commands, video recording, and safety monitoring
    /// </summary>
    public class ActionAPI : MonoBehaviour
    {
        public bool stopMovement;
        public AudioSource audioSource;
        private CheckAvatarStatus _checkAvatarStatus;
        private PatientSafetyManager _patientSafetyManager;
        private VoiceCommandHandler _voiceCommandHandler;
        private ImageOpenAIConnector _imageOpenAIConnector;
        private VideoRecordingManager _videoRecordingManager;
        
        // Actions should correspond to the actions in actions.py in the Scenic repo
        // The actionName string in actions.py should correspond to the name of these functions exactly
        // The actionArgs should be the same as the arguments in actions.py
        // make stopMovement true and stop all animations/actions whenever an action is stopped or finished just in case
        void Awake()
        {
            _checkAvatarStatus = gameObject.GetComponent<CheckAvatarStatus>();
            audioSource = gameObject.GetComponent<AudioSource>();
            GameObject openAIManager = GameObject.FindWithTag("OpenAIManager");
            if (openAIManager) {
                _patientSafetyManager = openAIManager.GetComponent<PatientSafetyManager>();
                _voiceCommandHandler = openAIManager.GetComponent<VoiceCommandHandler>();
                _imageOpenAIConnector = openAIManager.GetComponent<ImageOpenAIConnector>();
                _videoRecordingManager = openAIManager.GetComponent<VideoRecordingManager>();
            } else
            {
                Debug.Log("ActionAPI : Unable to Find openAI Manager");
            }

            _patientSafetyManager.questionCount = 0;
            _patientSafetyManager.StartRecording();
        }

        /// <summary>
        /// Retrieves a GameObject from the ScenicManager's object list by name
        /// </summary>
        private GameObject GetGameObject(string name)
        {
            List<GameObject> gameObjects = GameObject.FindGameObjectWithTag("ScenicManager").GetComponent<ObjectsList>().scenicObjects;
            foreach (GameObject child in gameObjects)
            {
                if (child.name == name)
                {
                    return child;
                }
            }
            return null;
        }

        // Basic interaction methods
        public void Text(string textString)
        {
            Debug.LogError("Text received from Scenic: " + textString + gameObject.name);
        }

        public void MoveToPos(Vector3 pos)
        {
            Debug.LogError("moving to pos " + pos);
            gameObject.transform.position = pos;
        }

        public void Dialogue(string trigger)
        {
            Debug.Log("scenic dialogue connected");
        }

        /// <summary>
        /// Converts text to speech using Google TTS service
        /// </summary>
        public void Speak(string textString)
        {
            GoogleTTS tts = gameObject.GetComponent<GoogleTTS>();
            if (tts != null)
            {
                tts.ConvertTextToSpeech(textString);
            }
            Debug.Log("speaking " + textString);
        }

        /// <summary>
        /// Records video and evaluates patient's performance based on given instruction
        /// </summary>
        public void RecordVideoAndEvaluate(string instruction)
        {
            _videoRecordingManager.SetInstruction(instruction);
            _videoRecordingManager.StopAllImageSending();
            _videoRecordingManager.StopCapturing();
            _videoRecordingManager.StartTakeImages();
        }

        /// <summary>
        /// Cleans up all active image processing coroutines and resets status
        /// </summary>
        public void DisposeQueries()
        {
            StartCoroutine(DisposeQueriesCoroutine());
        }

        public IEnumerator DisposeQueriesCoroutine()
        {
            yield return new WaitForSeconds(0.2f);
            foreach (Coroutine co in _imageCoroutines)
            {  
                if (co != null) 
                    StopCoroutine(co);
            }
            _imageCoroutines.Clear();
            _videoRecordingManager.StopAllImageSending();
            _checkAvatarStatus.taskDone = false;
            _checkAvatarStatus.feedback = "";
            Debug.Log("Disposing all queries");
        }
        
        // Video recording control methods
        public void StartRecording()
        {  
            Debug.Log("starting recording");
            _videoRecordingManager.StartTakeImages();
        }

        public void StopRecording()
        {
            Debug.Log("Stop recording");
            _videoRecordingManager.StopTakeImages();
            _videoRecordingManager.StopAllImageSending();
        }

        public void Done()
        {
            Debug.Log("done action");
        }

        // Object visibility control
        public void Hide(string objectName)
        {
            GameObject obj = GetGameObject(objectName);
            if (obj != null)
            {
                if (!obj.activeInHierarchy) return;
                obj.SetActive(false);
            }
            Debug.Log("HIDE :" + objectName);
        }

        public void Show(string objectName)
        {
            GameObject obj = GetGameObject(objectName);
            if (obj != null)
            {
                if (obj.activeInHierarchy) return;
                obj.SetActive(true);
            }
            Debug.Log("SHOW :" + objectName);
        }

        /// <summary>
        /// Waits for audio to finish before starting recording for patient safety monitoring
        /// </summary>
        private IEnumerator WaitForAudioToFinishAndStartRecording(string question)
        {
            yield return new WaitForSeconds(1.5f);
            
            yield return new WaitWhile(() => audioSource.isPlaying);
            PatientSafetyManager patientSafetyManager = GameObject.FindWithTag("OpenAIManager")?.GetComponent<PatientSafetyManager>();
            if (patientSafetyManager)
            {
                patientSafetyManager.audioSource = audioSource;
                patientSafetyManager.questionCount += 1;
                patientSafetyManager.StartRecording();
            }
        }

        /// <summary>
        /// Asks a question to the patient and starts recording their response
        /// </summary>
        public void AskQuestion(string question)
        {
            Speak(question);
            StartCoroutine(WaitForAudioToFinishAndStartRecording(question));
        }

        /// <summary>
        /// Captures and saves a snapshot to Google Cloud with specified ID
        /// </summary>
        public void TakeSnapshot(string image_id)
        {
            Texture2D snapshot = _voiceCommandHandler.CaptureImage();
            _imageOpenAIConnector.SaveImageToGoogleCloud(snapshot, image_id);
        }

        private CancellationTokenSource _cancellationTokenSource;
        private List<Coroutine> _imageCoroutines = new List<Coroutine>();

        /// <summary>
        /// Initiates periodic image capture and analysis for exercise evaluation
        /// </summary>
        public void SendImageAndTextRequest(string instruction)
        {
            Debug.Log("SendImageAndTextRequest called with instruction 1: " + instruction);
            GameObject openAI = GameObject.Find("OpenAI Manager");
            if (_checkAvatarStatus == null) return;
            _checkAvatarStatus.feedback = "";
            _checkAvatarStatus.taskDone = false;
            
            _imageCoroutines.Add(StartCoroutine(PeriodicImageRequestCoroutine(instruction))); 
        }

        /// <summary>
        /// Periodically captures and analyzes images until task is complete
        /// </summary>
        private IEnumerator PeriodicImageRequestCoroutine(string instruction)
        {
            while (!_checkAvatarStatus.taskDone)
            {
                SendSingleImageRequest(instruction);
                yield return new WaitForSeconds(2f);
            }
        }

        /// <summary>
        /// Sends a single image request with exercise evaluation prompt
        /// </summary>
        private void SendSingleImageRequest(string instruction)
        {

            string command = $"You are a helpful assistant who can understand and answer questions about an image." +
                             $"You are provided with an image and an instruction. The video shows a patient conducting a physical" +
                             $"rehabilitation exercise at home while wearing orthonic splints, following the given instruction from a physical or an occupational therapist." +
                             $"The image is taken from a first-person perspective using a camera mounted on the patient's head. The instruction is given by a physical or an occupational therapist." +
                             $"So, the image will contain only a partial view of the patient, e.g. hand and arm, but not the upper torso." +
                             $"Regarding the camera perspective, it is aligned with the patient's perspective, meaning" +
                             $"the right side of the image is the right side of the patient, and the left side of the image is the the left side of the patient." +

                             $"The instruction consists of the current and the prior instructions." +
                             $"Given the limited view of the patient's body, your task is to evaluate whether the current instruction is completed in the image, not the prior instructions."+
                             $"Only use the prior instructions to clarify any ambiguity in the current instruction."+

                             $"It is important to be PRECISE! Make sure that the instruction is strictly followed." +
                             $"This means that you should check for the completion of the instruction, not in progression. " +
                             $"Example 1: if the instruction is to pick up an object. Make sure that the object is fully grasped and lifted up, not just grasping an object." +
                             $"Example 2: if the instruction is to take an object out of a container, make sure that the object is fully taken out of the container, not still partially in the container." +
                             $"Thus, you should check whether the image shows the fully completed end state of the instruction, not the state in progress." +

                             $"Also, make sure that the instruction is 'literally' followed." +
                             $"It is important that you check the instruction literally and do not add your own interpretation." +
                             $"Example 4: If the instruction is to move the right hand 'toward' your body, as long as the person is moving the hand towards the body, it is completed." +
                             $"Unless the instruction explicitly asked for the patient to touch one's body, simply moving the hand 'towards' the body as the instruction literally states is sufficient." +
                             $"Example 5: If the instruction is to 'try to straighten' your elbow, then you should check if the patient makes any elbow extension movement."+
                             $"The patient does not need to fully straighten the elbow since the instruction states 'try to' straighten the elbow." +

                             $"If an instruction is ambiguous despite referencing the prior instructions, then as long as the instruction can be interpreted as completed given the image, you should return 'Yes'." +
                             $"As long as the instruction is completed in the image, return 'Yes' even if there are other additional activities that are not instructed in the image." +
                             $"Return only 2 sentences, in the first sentence return either 'Yes' or 'No,' and in the second, provide the reasoning behind your decision." +
                             $"Here are the current instruction and the list of prior instructions: \"{instruction}\" ";

            // string command = $"You are a helpful assistant who can understand and answer questions about images. " +
            //                  $"Your are given an image and a instruction. The image contains a scene of a patient conducting a physical" +  
            //                  $"rehabilitation exercise at home while wearing orthonic splints, following the given instruction from a physical therapist." + 
            //                  $"The image is taken from a first-person perspective using a camera mounted on the patient's head." + 
            //                  $"The instruction contains information about (1) the current instruction to follow and (2) the history of" + 
            //                  $"prior instructions given to the patient. Your task is to analyze the image and output whether" + 
            //                  $"the patient is following the CURRENT instruction (Yes or No) and the rationale behind your answer." + 
            //                  $"It is important that you ONLY focus on the CURRENT instruction and ignore the history of prior instructions." + 
            //                  $"The history of prior instructions is only provided to help you understand the context of the current instruction." +
            //                  $"In particular, check with which hand the instruction is to be performed. If it is completed using the wrong hand," +
            //                  $"you should answer 'No' and explain why it is incorrect. However, if the task is performed by the correct hand, but" +
            //                  $"with the help of the other hand, you should answer 'Yes'. If the instruction is unclear which hand should be used, then consider that either hand can be used. "+
            //                  $"However, if it is unclear which hand to use, then the action can be performed with either hand." +
            //                  $"As long as the instruction is completed in the image, return 'Yes' even if there are other additional activities that are not instructed in the image." +
            //                  $"Return only 2 sentences, in the first sentence return either 'Yes' or 'No,' and in the second, provide the reasoning behind your decision." +
            //                  $"For your reasoning, if your answer is 'No', then describe what you are seeing in the image and why it is not following the instruction. " +
            //                  $"If your answer is 'Yes', then describe why the instruction is completed."+
            //                  $"Here are the current instruction and the list of the history of instructions: \"{instruction}\" ";
            Coroutine captureImage = _voiceCommandHandler.CaptureAndSendImage(command);
            if (captureImage != null) {
                _imageCoroutines.Add(captureImage);
                Debug.Log("SendImageAndTextRequest called with instruction 2: " + instruction);
            }
        }

        private void OnDestroy()
        {
            if (_patientSafetyManager)
            {
                _patientSafetyManager.StopRecording();
            }
            if (_imageCoroutines != null)
                _imageCoroutines.Clear();
            if (_videoRecordingManager)
                _videoRecordingManager.StopAllImageSending();
            if (_checkAvatarStatus)
            {
                _checkAvatarStatus.taskDone = false;
                _checkAvatarStatus.feedback = "";
            }

            Debug.Log("Disposing all queries");
        }
    }
}