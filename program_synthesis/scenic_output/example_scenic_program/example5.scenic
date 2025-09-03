###################### Example 5 ######################

###################### Input Dictionary example ######################
{
    "precaution": "The precaution is to ensure the area around your feet and the kibble box is clear of any tripping hazards. Make sure to stand steadily with feet shoulder-width apart. If you feel pain, fatigue, or dizziness, please inform the system so we can stop the exercise for your safety.", 
    "instruction": [
        "grab the spoon with your right hand",
        "grab the bowl with your left hand",
        "Stand in front of the kibble box",
        "Scoop the kibbles"
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


log_file_path = "program_synthesis/logs/scoop_and_transfer_kibbles.json"

# Create log json file 
with open(log_file_path, 'w') as f:
    json.dump({}, f, indent=4)

# Instruction logs
logs = {
    0: {
        "ActionAPI": "",
        "Instruction": "grab the spoon with your right hand",
        "Time_Taken": 0,
        "Completeness": False
    },
    1: {
        "ActionAPI": "",
        "Instruction": "grab the bowl with your left hand",
        "Time_Taken": 0,
        "Completeness": False
    },
    2: {
        "ActionAPI": "",
        "Instruction": "Stand in front of the kibble box",
        "Time_Taken": 0,
        "Completeness": False
    },
    3: {
        "ActionAPI": "",
        "Instruction": "Scoop the kibbles",
        "Time_Taken": 0,
        "Completeness": False
    }
}

behavior Instruction():
    take SpeakAction("Let's begin new exercise.") # Always start with a greeting
    take SpeakAction('The precaution is to ensure the area around your feet and the kibble box is clear of any tripping hazards. Make sure to stand steadily with feet shoulder-width apart. If you feel pain, fatigue, or dizziness, please inform the system so we can stop the exercise for your safety.')
    take DoneAction()

    while WaitForIntroduction(ego):
        wait

    log_idx = 0
    speak_idx = 1

    # ---------- Instruction 0 ----------
    take SpeakAction('grab the spoon with your right hand')
    take DoneAction()

    while WaitForSpeakAction(ego, speak_idx): 
        wait
    speak_idx += 1 

    start_time, count = time.time(), 0
    # When invoking SendImageAndTextRequestAction or RecordVideoAndEvaluateAction, provide the current instruction and prior instructions.
    take SendImageAndTextRequestAction(f"Current Instruction: {logs[log_idx]['Instruction']}. Prior Instructions: {[logs[i]['Instruction'] for i in range(log_idx)]}")
    take DoneAction()
    while not RequestActionResult(ego):
        if count > 175:
            break
        count += 1
        wait
    end_time = time.time()
    UpdateLogs(logs, log_idx, "SendImageAndTextRequestAction", end_time - start_time, count < 175)
    with open(log_file_path, "w") as file:
        json.dump(logs, file, indent=4)
    log_idx += 1
    # Whenever SendImageAndTextRequestAction() or RecordVideoandEvaulate() are called, 
    # you must call "take DisposeQueriesAction()" to flush out remaining web requests before the next instruction.
    take DisposeQueriesAction()

    # ---------- Instruction 1 ----------
    take SpeakAction('grab the bowl with your left hand')
    take DoneAction()

    while WaitForSpeakAction(ego, speak_idx): 
        wait
    speak_idx += 1 

    start_time, count = time.time(), 0
    take SendImageAndTextRequestAction(f"Current Instruction: {logs[log_idx]['Instruction']}. Prior Instructions: {[logs[i]['Instruction'] for i in range(log_idx)]}")
    take DoneAction()
    while not RequestActionResult(ego):
        if count > 175:
            take DisposeQueriesAction()
            break
        count += 1
        wait
    end_time = time.time()
    UpdateLogs(logs, log_idx, "SendImageAndTextRequestAction", end_time - start_time, count < 175)
    with open(log_file_path, "w") as file:
        json.dump(logs, file, indent=4)
    log_idx += 1
    # Whenever SendImageAndTextRequestAction() or RecordVideoandEvaulate() are called, 
    # you must call "take DisposeQueriesAction()" to flush out remaining web requests before the next instruction.
    take DisposeQueriesAction()

    # ---------- Instruction 2 ----------
    take SpeakAction('Stand in front of the kibble box')
    take DoneAction()

    while WaitForSpeakAction(ego, speak_idx): 
        wait
    speak_idx += 1 

    start_time, count = time.time(), 0
    # Make sure to remove the reference to standing in the input to SendImageAndTextRequestAction() since it is being checked with CheckStanding API.
    take SendImageAndTextRequestAction(f"Current Instruction: be in front of the kibble box. Prior Instructions: {[logs[i]['Instruction'] for i in range(log_idx)]}")
    take DoneAction()
    # *** as shown below, when conjuncting with RequestActionResult(ego), make sure to state RequestActionResult(ego) at the end of the condition, so that the system can check if the action is completed.
    while not (CheckStanding(ego) and RequestActionResult(ego)):
        if count > 175:
            take DisposeQueriesAction()
            break
        count += 1
        wait
    end_time = time.time()
    UpdateLogs(logs, log_idx, "SendImageAndTextRequestAction+CheckStanding", end_time - start_time, count < 175)
    with open(log_file_path, "w") as file:
        json.dump(logs, file, indent=4)
    log_idx += 1
    # Whenever SendImageAndTextRequestAction() or RecordVideoandEvaulate() are called, 
    # you must call "take DisposeQueriesAction()" to flush out remaining web requests before the next instruction.
    take DisposeQueriesAction()

    # ---------- Instruction 3 ----------
    take SpeakAction('Scoop the kibbles')
    take DoneAction()

    while WaitForSpeakAction(ego, speak_idx): 
        wait
    speak_idx += 1 

    start_time, count = time.time(), 0
    # The completed state of the instruction can be checked with an image of a hand holding a spoon with kibbles in it,
    # so you should invoke SendImageAndTextRequestAction over RecordVideoAndEvaluateAction.
    take SendImageAndTextRequestAction(f"Current Instruction: {logs[log_idx]['Instruction']}. Prior Instructions: {[logs[i]['Instruction'] for i in range(log_idx)]}")
    take DoneAction()
    while not RequestActionResult(ego):
        if count > 175:
            take DisposeQueriesAction()
            break
        count += 1
        wait
    end_time = time.time()
    UpdateLogs(logs, log_idx, "SendImageAndTextRequestAction", end_time - start_time, count < 175)
    with open(log_file_path, "w") as file:
        json.dump(logs, file, indent=4)
    log_idx += 1
    # Whenever SendImageAndTextRequestAction() or RecordVideoandEvaulate() are called, 
    # you must call "take DisposeQueriesAction()" to flush out remaining web requests before the next instruction.
    take DisposeQueriesAction()

    # ---------- Inform the Patient that the Exercise is Completed ----------
    take SpeakAction("Wonderful! You finished this exercise.")
    take DoneAction()

    while WaitForSpeakAction(ego, speak_idx): 
        wait
    speak_idx += 1 # increment speak_idx by 1 every time a new instruction is spoken
    take DisposeQueriesAction() # call this at the end of the program to flush out any remaining queries, if exists. 

ego = new Scenicavatar at (0, 0, 0), with behavior Instruction()



