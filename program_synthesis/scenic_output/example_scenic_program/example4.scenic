###################### Example 4  ######################
###################### Input Dictionary example ######################
{
    "precaution": "Make sure to be seated and stay seated while doing this exercise. If you feel pain, fatigue, or dizziness, please tell the system so that we can terminate the exercise for your safety.", 
    "instruction": [
        "Use your right hand to grasp the milk bottle", 
        "Open the lid of the milk bottle", 
        "Close the lid of the milk bottle"
        ], 
    "setup": {
        "ego": [
            [
                0,
                0,
                0
            ],
            [
                0,
                0,
                0
            ]
        ]
    }
}

# Suppose you are given a dictionary above. Generate a Scenic program in the following structure.
###################### Output Scenic file example ######################

import time
import json

from scenic.simulators.unity.actions import *
from scenic.simulators.unity.behaviors import *

model scenic.simulators.unity.model


log_file_path = "program_synthesis/open_milk_bottle_lid.json"

# Create log json file 
with open(log_file_path, 'w') as f:
    json.dump({}, f, indent=4)


# Logs uses instrctions from the dictionary, so you have to define this first. Only instructions are different

logs = {
    0 : {
        "ActionAPI" : "",
        "Instruction": "Use your right hand to grasp the milk bottle",
        "Time_Taken" : 0,
        "Completeness": False
        },
    1 :{
        "ActionAPI" : "",
        "Instruction": "Open the lid of the milk bottle", 
        "Time_Taken" : 0,
        "Completeness": False
        },
    2 : {
        "ActionAPI" : "",
        "Instruction": "Close the lid of the milk bottle", 
        "Time_Taken" : 0,
        "Completeness": False
        }
}


behavior Instruction():
    take SpeakAction("Let's start a new exercise.") # Always start with a greeting
    take SpeakAction('The precaution is make sure to be seated and stay seated while doing this exercise. If you feel pain, fatigue, or dizziness, please tell the system so that we can terminate the exercise for your safety.')
    take DoneAction()

    while WaitForIntroduction(ego):
        wait

    log_idx = 0
    speak_idx = 1

    # ---------- STEP 0 ----------
    take SpeakAction("Use your right hand to grasp the milk bottle")
    take DoneAction()

    while WaitForSpeakAction(ego, speak_idx): 
        wait
    speak_idx += 1 # increment speak_idx by 1 every time a new instruction is spoken

    start_time, count = time.time(), 0

    # None of the condition in the current instruction can be checked with BPE APIs.
    # So, we use a visual API. The completed state of the instruction can be checked with an image of the right hand
    # grapsing a milk bottle, so you should invoke SendImageAndTextRequestAction over RecordVideoAndEvaluateAction
    # Use the current instruction without modification to the SendImageAndTextRequestAction, as no other condition in the instruction
    # is checked with any BPE APIs.
    take SendImageAndTextRequestAction(f"Current Instruction: {logs[log_idx]['Instruction']}. Prior Instructions: {[logs[i]['Instruction'] for i in range(log_idx)]}")
    take DoneAction()
    while not RequestActionResult(ego):
        if count > 175:
            break
        count += 1
        wait

    end_time = time.time()
    UpdateLogs(logs, log_idx, "SendImageAndTextRequestAction", end_time - start_time, count < 175)
    with open(self.log_file_path, "w") as file:
        json.dump(logs, file, indent=4)
    log_idx += 1
    # Whenever SendImageAndTextRequestAction() or RecordVideoandEvaulate() are called, 
    # you must call "take DisposeQueriesAction()" to flush out remaining web requests before the next instruction.
    take DisposeQueriesAction()

    # ---------- STEP 1 ----------
    take SpeakAction("Open the lid of the milk bottle")
    take DoneAction()

    while WaitForSpeakAction(ego, speak_idx): 
        wait
    speak_idx += 1 # increment speak_idx by 1 every time a new instruction is spoken

    start_time, count = time.time(), 0

    # The completed state of the instruction can be checked with an image of a hand holding the lid of the milk bottle
    # so you should invoke SendImageAndTextRequestAction over RecordVideoAndEvaluateAction.
    take SendImageAndTextRequestAction(f"Current Instruction: {logs[log_idx]['Instruction']}. Prior Instructions: {[logs[i]['Instruction'] for i in range(log_idx)]}")
    take DoneAction()
    while not RequestActionResult(ego):
        if count > 175:
            break
        count += 1
        wait

    end_time = time.time()
    UpdateLogs(logs, log_idx, "SendImageAndTextRequestAction", end_time - start_time, count < 175)
    with open(self.log_file_path, "w") as file:
        json.dump(logs, file, indent=4)
    log_idx += 1
    # Whenever SendImageAndTextRequestAction() or RecordVideoandEvaulate() are called, 
    # you must call "take DisposeQueriesAction()" to flush out remaining web requests before the next instruction.
    take DisposeQueriesAction()

    # ---------- STEP 2 ----------
    take SpeakAction("Close the lid of the milk bottle")
    take DoneAction()

    while WaitForSpeakAction(ego, speak_idx):
        wait
    speak_idx += 1 # increment speak_idx by 1 every time a new instruction is spoken

    start_time, count = time.time(), 0

    # The completed state of the instruction can be checked with an image of a the milk bottle with a closed lid
    # so you should invoke SendImageAndTextRequestAction over RecordVideoAndEvaluateAction.
    take SendImageAndTextRequestAction(f"Current Instruction: {logs[log_idx]['Instruction']}. Prior Instructions: {[logs[i]['Instruction'] for i in range(log_idx)]}")
    take DoneAction()
    while not RequestActionResult(ego):
        if count > 175:
            break
        count += 1
        wait

    end_time = time.time()
    UpdateLogs(logs, log_idx, "SendImageAndTextRequestAction", end_time - start_time, count < 175)
    with open(self.log_file_path, "w") as file:
        json.dump(logs, file, indent=4)
    log_idx += 1
    # Whenever SendImageAndTextRequestAction() or RecordVideoandEvaulate() are called, 
    # you must call "take DisposeQueriesAction()" to flush out remaining web requests before the next instruction.
    take DisposeQueriesAction()

    # ---------- Inform the Patient that the Exercise is Completed ----------
    take SpeakAction("Fabulous! You reached the end of this exercise.")
    take DoneAction()

    while WaitForSpeakAction(ego, speak_idx): 
        wait
    speak_idx += 1 # increment speak_idx by 1 every time a new instruction is spoken
    take DisposeQueriesAction() # call this at the end of the program to flush out any remaining queries, if exists. 


ego = new Scenicavatar at (0,0,0), with behavior Instruction()


