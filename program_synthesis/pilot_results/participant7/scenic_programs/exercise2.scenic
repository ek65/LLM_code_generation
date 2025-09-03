import time
import json

from scenic.simulators.unity.actions import *
from scenic.simulators.unity.behaviors import *

model scenic.simulators.unity.model

log_file_path = "program_synthesis/logs/stack_cups.json"

# Create log json file 
with open(log_file_path, 'w') as f:
    json.dump({}, f, indent=4)

# Instruction logs
logs = {
    0: {
        "ActionAPI": "",
        "Instruction": "Start by stacking the cup labeled #1 on top of the cup labeled #2, using your right hand.",
        "Time_Taken": 0,
        "Completeness": False
    },
    1: {
        "ActionAPI": "",
        "Instruction": "Then, using your right hand, stack cup #3 on top of cup #2.",
        "Time_Taken": 0,
        "Completeness": False
    },
    2: {
        "ActionAPI": "",
        "Instruction": "Once you are done, return your hands to resting on the table.",
        "Time_Taken": 0,
        "Completeness": False
    }
}

behavior Instruction():
    try:
        take SpeakAction("Let's start a new exercise.") # Always start with a greeting
        take SpeakAction("If you feel pain, fatigue, or dizziness, please tell the system so that we can terminate the exercise for your safety.")
        take DoneAction()

        while WaitForIntroduction(ego):
            wait

        log_idx = 0
        speak_idx = 1

        # ---------- Instruction 0 ----------
        take SpeakAction("Start by stacking the cup labeled #1 on top of the cup labeled #2, using your right hand.")
        take DoneAction()

        while WaitForSpeakAction(ego, speak_idx): 
            wait
        speak_idx += 1 

        start_time, count = time.time(), 0
        take SendImageAndTextRequestAction(f"Current Instruction: {logs[log_idx]['Instruction']}. Prior Instructions: {[logs[i]['Instruction'] for i in range(log_idx)]}")
        take DoneAction()
        while not RequestActionResult(ego):
            if count > 150:
                break
            count += 1
            wait
        end_time = time.time()
        UpdateLogs(logs, log_idx, "SendImageAndTextRequestAction", end_time - start_time, count < 150)
        with open(log_file_path, "w") as file:
            json.dump(logs, file, indent=4)
        log_idx += 1
        take DisposeQueriesAction()

        # ---------- Instruction 1 ----------
        take SpeakAction("Then, using your right hand, stack cup #3 on top of cup #2.")
        take DoneAction()

        while WaitForSpeakAction(ego, speak_idx): 
            wait
        speak_idx += 1 

        start_time, count = time.time(), 0
        take SendImageAndTextRequestAction(f"Current Instruction: {logs[log_idx]['Instruction']}. Prior Instructions: {[logs[i]['Instruction'] for i in range(log_idx)]}")
        take DoneAction()
        while not RequestActionResult(ego):
            if count > 150:
                break
            count += 1
            wait
        end_time = time.time()
        UpdateLogs(logs, log_idx, "SendImageAndTextRequestAction", end_time - start_time, count < 150)
        with open(log_file_path, "w") as file:
            json.dump(logs, file, indent=4)
        log_idx += 1
        take DisposeQueriesAction()

        # ---------- Instruction 2 ----------
        take SpeakAction("Once you are done, return your hands to resting on the table.")
        take DoneAction()

        while WaitForSpeakAction(ego, speak_idx): 
            wait
        speak_idx += 1 

        start_time, count = time.time(), 0
        take SendImageAndTextRequestAction(f"Current Instruction: {logs[log_idx]['Instruction']}. Prior Instructions: {[logs[i]['Instruction'] for i in range(log_idx)]}")
        take DoneAction()
        while not RequestActionResult(ego):
            if count > 150:
                break
            count += 1
            wait
        end_time = time.time()
        UpdateLogs(logs, log_idx, "SendImageAndTextRequestAction", end_time - start_time, count < 150)
        with open(log_file_path, "w") as file:
            json.dump(logs, file, indent=4)
        log_idx += 1
        take DisposeQueriesAction()

        take DoneAction()

        # ---------- Exercise Completed, Now Check for Any Pain ----------
        take AskQuestionAction("Did you feel any pain or discomfort?")
        take DoneAction()

        while not PainRecorded(ego):
            wait

        LogPain(ego, logs)
        with open(log_file_path, "w") as file:
            json.dump(logs, file, indent=4)
        take DoneAction()

    interrupt when DetectedPain(ego):
        take AskQuestionAction("Did you feel any pain or discomfort?")
        take DoneAction()

        while not PainRecorded(ego):
            wait

        LogPain(ego, logs)
        with open(log_file_path, "w") as file:
            json.dump(logs, file, indent=4)
        take DoneAction()

ego = new Scenicavatar at (0, 0, 0), with behavior Instruction()