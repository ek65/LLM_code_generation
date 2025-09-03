###################### Input Dictionary example ######################
{
  "precaution": "If you feel pain, fatigue, or dizziness, please tell the system so that we can terminate the exercise for your safety.",
  "instruction": [
    "Pick up the duster with your right hand.",
    "Move the duster left and right across the wall to wipe it, repeating the motion at least 3 times.",
    "Place the duster back on the table"
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

# Path to save logs and snapshots
log_file_path = "program_synthesis/logs/wipe_wall_with_duster.json"

# Create log json file 
with open(log_file_path, 'w') as f:
    json.dump({}, f, indent=4)

# Instruction logs
logs = {
    0: {
        "ActionAPI": "",
        "Instruction": "Pick up the duster with your right hand.",
        "Time_Taken": 0,
        "Completeness": False
    },
    1: {
        "ActionAPI": "",
        "Instruction": "Move the duster left and right across the wall to wipe it, repeating the motion at least 3 times.",
        "Time_Taken": 0,
        "Completeness": False
    },
    2: {
        "ActionAPI": "",
        "Instruction": "Place the duster back on the table",
        "Time_Taken": 0,
        "Completeness": False
    }
}

behavior Instruction():
    # Introduction
    # ** You must always start with two speak actions: one for greeting and one for precaution.
    take SpeakAction("Let's start a new exercise.") # Always start with a greeting
    take SpeakAction("Physical precaution is: If you feel pain, fatigue, or dizziness, please tell the system so that we can terminate the exercise for your safety.")
    take DoneAction()

    while WaitForIntroduction(ego):
        wait

    log_idx = 0
    speak_idx = 1

    # ---------- Step 0: Pick up the duster ----------
    take SpeakAction("Pick up the duster with your right hand.")
    take DoneAction()

    while WaitForSpeakAction(ego, speak_idx): 
        wait
    speak_idx += 1 

    start_time, count = time.time(), 0
    take SendImageAndTextRequestAction(f"Current Instruction: {logs[log_idx]['Instruction']}. Prior Instructions: {[logs[i]['Instruction'] for i in range(log_idx)]}")
    take DoneAction()
    while not RequestActionResult(ego):
        if count > 175:
            break
        count += 1
        wait
    end_time = time.time()

    UpdateLogs(logs, log_idx, "SendImageAndTextRequestAction", end_time - start_time, count < 175)
    with open(log_file_path, "w") as f:
        json.dump(logs, f, indent=4)
    log_idx += 1
    take DisposeQueriesAction()

    # ---------- Step 1: Wipe the wall ----------
    take SpeakAction("Move the duster left and right across the wall to wipe it, repeating the motion at least 3 times.")
    take DoneAction()

    while WaitForSpeakAction(ego, speak_idx): 
        wait
    speak_idx += 1 

    start_time, count = time.time(), 0
    # Note a dynamic instruction needs to be checked here, i.e. "Move the duster left and right across the wall to wipe it, repeating the motion at least 3 times."
    # Hence, RecordVideoAndEvaluateAction needs to be used instead of SendImageAndTextRequestAction.
    take RecordVideoAndEvaluateAction(f"Current Instruction: {logs[log_idx]['Instruction']}. Prior Instructions: {[logs[i]['Instruction'] for i in range(log_idx)]}")
    take DoneAction()
    while not RequestActionResult(ego):
        if count > 175:
            break
        count += 1
        wait
    end_time = time.time()

    UpdateLogs(logs, log_idx, "RecordVideoAndEvaluateAction", end_time - start_time, count < 175)
    with open(log_file_path, "w") as f:
        json.dump(logs, f, indent=4)
    log_idx += 1
    take DisposeQueriesAction()

    # ---------- Step 2: Place back the duster ----------
    take SpeakAction("Place the duster back on the table")
    take DoneAction()

    while WaitForSpeakAction(ego, speak_idx): 
        wait
    speak_idx += 1 

    start_time, count = time.time(), 0
    take SendImageAndTextRequestAction(f"Current Instruction: {logs[log_idx]['Instruction']}. Prior Instructions: {[logs[i]['Instruction'] for i in range(log_idx)]}")
    take DoneAction()
    while not RequestActionResult(ego):
        if count > 175:
            break
        count += 1
        wait
    end_time = time.time()

    UpdateLogs(logs, log_idx, "SendImageAndTextRequestAction", end_time - start_time, count < 175)
    with open(log_file_path, "w") as f:
        json.dump(logs, f, indent=4)

    take DisposeQueriesAction()

    # ---------- Inform the Patient that the Exercise is Completed ----------
    take SpeakAction("Yayyy, you did it! This is the end of this exercise.")
    take DoneAction()

    while WaitForSpeakAction(ego, speak_idx): 
        wait
    speak_idx += 1 # increment speak_idx by 1 every time a new instruction is spoken
    take DisposeQueriesAction() # call this at the end of the program to flush out any remaining queries, if exists. 

# Instantiate avatar
ego = new Scenicavatar at (0, 0, 0), with behavior Instruction()
