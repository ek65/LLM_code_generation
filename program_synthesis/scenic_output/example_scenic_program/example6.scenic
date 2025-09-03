###################### Example 8 ######################
###################### Input Dictionary example ######################
{
  "precaution": "If you feel pain, fatigue, or dizziness, please tell the system so that we can terminate the exercise for your safety.",
  "instruction": [
    "Sit in front of a table.",
    "Place both your hands on the table and sit upright in a chair.",
    "Tap your fingers on the table as a warm-up for 3 seconds.",
    "Lean forward to touch the cup on the table.",
    "Sit up straight again and place both your hands on the table.",
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
log_file_path = "program_synthesis/logs/lean_forward_and_touch.json"

# Create log json file 
with open(log_file_path, 'w') as f:
    json.dump({}, f, indent=4)


# Instruction logs
logs = {
    0: {"ActionAPI": "", "Instruction": "Sit in front of a table.", "Time_Taken": 0, "Completeness": False},
    1: {"ActionAPI": "", "Instruction": "Place both your hands on the table and sit upright in a chair.", "Time_Taken": 0, "Completeness": False},
    2: {"ActionAPI": "", "Instruction": "Tap your fingers on the table as a warm-up for 3 seconds.", "Time_Taken": 0, "Completeness": False},
    3: {"ActionAPI": "", "Instruction": "Lean forward to touch the cup on the table.", "Time_Taken": 0, "Completeness": False},
    4: {"ActionAPI": "", "Instruction": "Sit up straight again and place both your hands on the table.", "Time_Taken": 0, "Completeness": False},
}

behavior Instruction():
    # Introduction: goal and precaution
    take SpeakAction("Let's do a new exercise.") # Always start with a greeting
    take SpeakAction("If you feel pain, fatigue, or dizziness, please tell the system so that we can terminate the exercise for your safety.")
    take DoneAction()

    while WaitForIntroduction(ego):
        wait

    log_idx = 0
    speak_idx = 1
    
    # ---------- Step 0----------
    take SpeakAction("Sit in front of a table.")
    take DoneAction()

    while WaitForSpeakAction(ego, speak_idx): 
        wait
    speak_idx += 1 

    start_time, count = time.time(), 0
    take SendImageAndTextRequestAction(f"Current Instruction: {logs[log_idx]['Instruction']}. Prior Instructions: {[logs[i]['Instruction'] for i in range(log_idx)]}")
    take DoneAction()
    # *** as shown below, when conjuncting with RequestActionResult(ego), you must state RequestActionResult(ego) at the end of the condition, so that the system can check if the action is completed.
    while not (CheckSeated(ego) and RequestActionResult(ego)):
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

    # ---------- Step 1----------
    take SpeakAction("Place both your hands on the table and sit upright in a chair.")
    take DoneAction()

    while WaitForSpeakAction(ego, speak_idx): 
        wait
    speak_idx += 1 

    start_time, count = time.time(), 0
    ## Note that sitting up straight cannot be checked in the camera view,
    # so we should remove the reference to sitting up straight in the current instruction to SendImageAndTextRequestAction.
    # And, instead, the sitting up straight action should be checked using SitUpStraight API
    take SendImageAndTextRequestAction(f"Current Instruction: Place your hands on the table. Prior Instructions: {[logs[i]['Instruction'] for i in range(log_idx)]}")
    take DoneAction()
    # conjunct the two conditions to check the instruction completion
    while not (SitUpStraight(ego) and RequestActionResult(ego)):
        if count > 175:
            break
        count += 1
        wait
    end_time = time.time()

    UpdateLogs(logs, log_idx, "SendImageAndTextRequestAction+SitUpStraight", end_time - start_time, count < 175)
    with open(log_file_path, "w") as f:
        json.dump(logs, f, indent=4)
    log_idx += 1
    take DisposeQueriesAction()

    # ---------- Step 2----------
    take SpeakAction("Tap your fingers on the table as a warm-up for 3 seconds.")
    take DoneAction()

    while WaitForSpeakAction(ego, speak_idx): 
        wait
    speak_idx += 1 

    start_time, count = time.time(), 0
    # Any condition that cannot be checked with BPE APIs, and therefore requires a visual API to check the action completion,
    # AND requires a temporal condition (like "for 3 seconds") must be checked with RecordVideoAndEvaluateAction.
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

    # ---------- Step 3----------
    take SpeakAction("Lean forward to touch the cup on the table.")
    take DoneAction()

    while WaitForSpeakAction(ego, speak_idx): 
        wait
    speak_idx += 1 

    start_time, count = time.time(), 0
    ## Note that because leaning forward cannot be checked in the camera view, 
    # we should remove the reference to leaning forward in the current instruction to SendImageAndTextRequestAction
    # And, instead, the leaning forward action should be checked using CheckLeanForward API
    take SendImageAndTextRequestAction(f"Current Instruction: Touch the cup on the table. Prior Instructions: {[logs[i]['Instruction'] for i in range(log_idx)]}")
    take DoneAction()
    # conjunct the two cconditions to check the instruction completion
    # *** as shown below, when conjuncting with RequestActionResult(ego), you must state RequestActionResult(ego) at the end of the condition, so that the system can check if the action is completed.
    while not (CheckLeanForward(ego) and RequestActionResult(ego)):
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

    # ---------- Step 4: Sit up straight again ----------
    take SpeakAction("Sit up straight again and place both your hands on the table.")
    take DoneAction()

    while WaitForSpeakAction(ego, speak_idx): 
        wait
    speak_idx += 1 

    start_time, count = time.time(), 0
    ## Note that sitting up straight cannot be checked in the camera view,
    # so we should remove the reference to sitting up straight in the current instruction to SendImageAndTextRequestAction.
    # And, instead, the sitting up straight action should be checked using SitUpStraight API
    take SendImageAndTextRequestAction(f"Current Instruction: Place both your hands on the table. Prior Instructions: {[logs[i]['Instruction'] for i in range(log_idx)]}")
    take DoneAction()
    # *** as shown below, when conjuncting with RequestActionResult(ego), you must state RequestActionResult(ego) at the end of the condition, so that the system can check if the action is completed.
    while not (SitUpStraight(ego) and RequestActionResult(ego)):
        if count > 175:
            break
        count += 1
        wait
    end_time = time.time()

    UpdateLogs(logs, log_idx, "SendImageAndTextRequestAction+SitUpStraight", end_time - start_time, count < 175)
    with open(log_file_path, "w") as f:
        json.dump(logs, f, indent=4)
    log_idx += 1
    take DisposeQueriesAction()

    # ---------- Inform the Patient that the Exercise is Completed ----------
    take SpeakAction("Wonderful! You finished this exercise.")
    take DoneAction()

    while WaitForSpeakAction(ego, speak_idx): 
        wait
    speak_idx += 1 # increment speak_idx by 1 every time a new instruction is spoken
    take DisposeQueriesAction() # call this at the end of the program to flush out any remaining queries, if exists. 


# Instantiate the avatar at the origin with this behavior
ego = new Scenicavatar at (0, 0, 0), with behavior Instruction()