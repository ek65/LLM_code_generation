###################### Example  ######################

###################### Input Dictionary example ######################
{
  "precaution": "If you feel pain, fatigue, or dizziness, please tell the system so that we can terminate the exercise for your safety.",
  "instruction": [
    "Extend your right arm.",
    "Now bend your arm",
    "Extend your right arm to touch a cup for 3 seconds.",
    "Now bend your arm to touch the edge of the table for 5 seconds.",
  ],
  "setup": {
    "ego": [[0, 0, 0], [0, 0, 0]]
  }
}

# Suppose you are given a dictionary above. Generate a Scenic program in the following structure. 
###################### Output Scenic file example ######################

import time
import json

from scenic.simulators.unity.actions import *
from scenic.simulators.unity.behaviors import *

model scenic.simulators.unity.model

log_file_path = "program_synthesis/logs/elbow_extension_flexion.json"

# Create log json file
with open(log_file_path, 'w') as f:
    json.dump({}, f, indent=4)

# Instruction logs
logs = {
    0: {
        "ActionAPI": "",
        "Instruction": "Extend your right arm.",
        "Time_Taken": 0,
        "Completeness": False
    },
    1: {
        "ActionAPI": "",
        "Instruction": "Now bend your arm",
        "Time_Taken": 0,
        "Completeness": False
    },
    2: {
        "ActionAPI": "",
        "Instruction": "Extend your right arm to touch a cup for 3 seconds.",
        "Time_Taken": 0,
        "Completeness": False
    },
    3: {
        "ActionAPI": "",
        "Instruction": "Now bend your arm to touch the edge of the table for 5 seconds.",
        "Time_Taken": 0,
        "Completeness": False
    }
}

behavior Instruction():
    # ** You must always start with two speak actions: one for greeting and one for precaution.
    take SpeakAction("Let's start a new exercise.")
    take SpeakAction("If you feel pain, fatigue, or dizziness, please tell the system so that we can terminate the exercise for your safety.")
    take DoneAction()

    while WaitForIntroduction(ego):
        wait

    log_idx = 0
    speak_idx = 1

    # ---------- Instruction 0 ----------
    take SpeakAction("Extend your right arm.")
    take DoneAction()

    while WaitForSpeakAction(ego, speak_idx):
        wait
    speak_idx += 1

    start_time, count = time.time(), 0
    take DoneAction()
    # For elbow extension check, you need to instantiate the CheckElbowExtension class and then call its checkCompleted method.
    ext = CheckElbowExtension()
    while not ext.checkCompleted(ego, arm='Right'):
        if count > 175:
            break
        count += 1
        wait

    end_time = time.time()
    UpdateLogs(logs, log_idx, "CheckElbowExtension", end_time - start_time, count < 175)
    with open(log_file_path, "w") as file:
        json.dump(logs, file, indent=4)
    log_idx += 1

    # ---------- Instruction 1 ----------
    take SpeakAction("Now bend your arm")
    take DoneAction()

    while WaitForSpeakAction(ego, speak_idx):
        wait
    speak_idx += 1

    start_time, count = time.time(), 0
    take DoneAction()
    # For elbow flexion check, you need to instantiate the CheckElbowBend class and then call its checkCompleted method.
    flx = CheckElbowBend()
    while not flx.checkCompleted(ego, arm='Right'):
        if count > 175:
            break
        count += 1
        wait
    end_time = time.time()
    UpdateLogs(logs, log_idx, "CheckElbowBend", end_time - start_time, count < 175)
    with open(log_file_path, "w") as file:
        json.dump(logs, file, indent=4)
    log_idx += 1
    take DoneAction()

    # ---------- Instruction 2 ----------
    take SpeakAction("Extend your right arm to touch a cup for 3 seconds.")
    take DoneAction()

    while WaitForSpeakAction(ego, speak_idx):
        wait
    speak_idx += 1

    start_time, count = time.time(), 0
    take DoneAction()
    # For elbow extension check, you need to instantiate the CheckElbowExtension class and then call its checkCompleted method.
    # To check duration of the elbow extension or any BPE APIs, you should use the CheckDuration() API.
    # You should not use RecordVideoAndEvaluateAction here, since checking duration for BPE API must use CheckDuration() API.
    cd = CheckDuration("CheckElbowExtension", 3, ego, "Right")
    take SendImageAndTextRequestAction(f"Current Instruction: Touch a cup with your right hand. Prior Instructions: {[logs[i]['Instruction'] for i in range(log_idx)]}")
    take DoneAction()
    while not (cd.checkCompleted() and RequestActionResult(ego)):
        if count > 175:
            break
        count += 1
        wait
    end_time = time.time()
    UpdateLogs(logs, log_idx, "CheckElbowExtension", end_time - start_time, count < 175)
    with open(log_file_path, "w") as file:
        json.dump(logs, file, indent=4)
    log_idx += 1
    take DoneAction()
    take DisposeQueriesAction()

    # ---------- Instruction 3 ----------
    take SpeakAction("Now bend your arm to touch the edge of the table for 5 seconds.")
    take DoneAction()

    while WaitForSpeakAction(ego, speak_idx):
        wait
    speak_idx += 1

    start_time, count = time.time(), 0
    take DoneAction()
    # For elbow flexion check, you need to instantiate the CheckElbowBend class and then call its checkCompleted method.
    # To check duration of the elbow bend or any BPE APIs, you should use the CheckDuration() API.
    # You should not use RecordVideoAndEvaluateAction here, since checking duration for BPE API must use CheckDuration() API.
    cd = CheckDuration("CheckElbowBend", 3, ego, "Right")
    take SendImageAndTextRequestAction(f"Current Instruction: Touch a cup with your right hand. Prior Instructions: {[logs[i]['Instruction'] for i in range(log_idx)]}")
    take DoneAction()
    while not (cd.checkCompleted() and RequestActionResult(ego)):
        if count > 175:
            break
        count += 1
        wait
    end_time = time.time()
    UpdateLogs(logs, log_idx, "CheckElbowBend", end_time - start_time, count < 175)
    with open(log_file_path, "w") as file:
        json.dump(logs, file, indent=4)
    log_idx += 1
    take DoneAction()
    take DisposeQueriesAction()

    # ---------- Inform the Patient that the Exercise is Completed ----------
    take SpeakAction("Great job! You finished this exercise.")
    take DoneAction()

    while WaitForSpeakAction(ego, speak_idx): 
        wait
    speak_idx += 1 # increment speak_idx by 1 every time a new instruction is spoken
    take DisposeQueriesAction() # call this at the end of the program to flush out any remaining queries, if exists. 


ego = new Scenicavatar at (0, 0, 0),
    with behavior Instruction()