import time
import json

from scenic.simulators.unity.actions import *
from scenic.simulators.unity.behaviors import *

model scenic.simulators.unity.model

# Path to save logs
log_file_path = "program_synthesis/logs/three_coins_on_table.json"

# Create log json file 
with open(log_file_path, 'w') as f:
    json.dump({}, f, indent=4)

# Instruction logs
logs = {
    0: {"ActionAPI": "", "Instruction": "Erie, look for the three coins on the table in front of you.", "Time_Taken": 0, "Completeness": False},
    1: {"ActionAPI": "", "Instruction": "Use your thumb and your index finger to pick up the first coin.", "Time_Taken": 0, "Completeness": False},
    2: {"ActionAPI": "", "Instruction": "Erie, place the first coin in the bowl.", "Time_Taken": 0, "Completeness": False},
    3: {"ActionAPI": "", "Instruction": "Use your thumb and your index finger to pick up the second coin.", "Time_Taken": 0, "Completeness": False},
    4: {"ActionAPI": "", "Instruction": "Erie, place the second coin in the bowl.", "Time_Taken": 0, "Completeness": False},
    5: {"ActionAPI": "", "Instruction": "Use your thumb and index finger to pick up the third coin", "Time_Taken": 0, "Completeness": False},
    6: {"ActionAPI": "", "Instruction": "Erie, put the third coin in the bowl", "Time_Taken": 0, "Completeness": False}
}

behavior Instruction():
    try:
        # Introduction: goal, precaution
        take SpeakAction("The goal of the exercise is None.")
        take SpeakAction("Precaution is: If you feel pain, fatigue, or dizziness, please tell the system so that we can terminate the exercise for your safety.")
        take DoneAction()

        while WaitForIntroduction(ego):
            wait

        log_idx = 0

        # ---------- Instruction 0 ----------
        take SpeakAction(logs[0]["Instruction"])
        take DoneAction()
        while WaitForSpeakAction(ego, 1):
            wait

        start_time, count = time.time(), 0
        take SendImageAndTextRequestAction(f"Current Instruction: {logs[log_idx]['Instruction']}. Prior Instructions: {[logs[i]['Instruction'] for i in range(log_idx)]}")
        take DoneAction()
        while not RequestActionResult(ego):
            if count > 250:
                take DisposeQueriesAction()
                break
            count += 1
            wait
        end_time = time.time()
        UpdateLogs(logs, log_idx, "SendImageAndTextRequestAction", end_time - start_time, count < 250)
        with open(log_file_path, "w") as f:
            json.dump(logs, f, indent=4)
        log_idx += 1
        take DisposeQueriesAction()

        # ---------- Instruction 1 ----------
        take SpeakAction(logs[1]["Instruction"])
        take DoneAction()
        while WaitForSpeakAction(ego, 2):
            wait

        start_time, count = time.time(), 0
        take SendImageAndTextRequestAction(f"Current Instruction: {logs[log_idx]['Instruction']}. Prior Instructions: {[logs[i]['Instruction'] for i in range(log_idx)]}")
        take DoneAction()
        while not RequestActionResult(ego):
            if count > 250:
                take DisposeQueriesAction()
                break
            count += 1
            wait
        end_time = time.time()
        UpdateLogs(logs, log_idx, "SendImageAndTextRequestAction", end_time - start_time, count < 250)
        with open(log_file_path, "w") as f:
            json.dump(logs, f, indent=4)
        log_idx += 1
        take DisposeQueriesAction()

        # ---------- Instruction 2 ----------
        take SpeakAction(logs[2]["Instruction"])
        take DoneAction()
        while WaitForSpeakAction(ego, 3):
            wait

        start_time, count = time.time(), 0
        take SendImageAndTextRequestAction(f"Current Instruction: {logs[log_idx]['Instruction']}. Prior Instructions: {[logs[i]['Instruction'] for i in range(log_idx)]}")
        take DoneAction()
        while not RequestActionResult(ego):
            if count > 250:
                take DisposeQueriesAction()
                break
            count += 1
            wait
        end_time = time.time()
        UpdateLogs(logs, log_idx, "SendImageAndTextRequestAction", end_time - start_time, count < 250)
        with open(log_file_path, "w") as f:
            json.dump(logs, f, indent=4)
        log_idx += 1
        take DisposeQueriesAction()

        # ---------- Instruction 3 ----------
        take SpeakAction(logs[3]["Instruction"])
        take DoneAction()
        while WaitForSpeakAction(ego, 4):
            wait

        start_time, count = time.time(), 0
        take SendImageAndTextRequestAction(f"Current Instruction: {logs[log_idx]['Instruction']}. Prior Instructions: {[logs[i]['Instruction'] for i in range(log_idx)]}")
        take DoneAction()
        while not RequestActionResult(ego):
            if count > 250:
                take DisposeQueriesAction()
                break
            count += 1
            wait
        end_time = time.time()
        UpdateLogs(logs, log_idx, "SendImageAndTextRequestAction", end_time - start_time, count < 250)
        with open(log_file_path, "w") as f:
            json.dump(logs, f, indent=4)
        log_idx += 1
        take DisposeQueriesAction()

        # ---------- Instruction 4 ----------
        take SpeakAction(logs[4]["Instruction"])
        take DoneAction()
        while WaitForSpeakAction(ego, 5):
            wait

        start_time, count = time.time(), 0
        take SendImageAndTextRequestAction(f"Current Instruction: {logs[log_idx]['Instruction']}. Prior Instructions: {[logs[i]['Instruction'] for i in range(log_idx)]}")
        take DoneAction()
        while not RequestActionResult(ego):
            if count > 250:
                take DisposeQueriesAction()
                break
            count += 1
            wait
        end_time = time.time()
        UpdateLogs(logs, log_idx, "SendImageAndTextRequestAction", end_time - start_time, count < 250)
        with open(log_file_path, "w") as f:
            json.dump(logs, f, indent=4)
        log_idx += 1
        take DisposeQueriesAction()

        # ---------- Instruction 5 ----------
        take SpeakAction(logs[5]["Instruction"])
        take DoneAction()
        while WaitForSpeakAction(ego, 6):
            wait

        start_time, count = time.time(), 0
        take SendImageAndTextRequestAction(f"Current Instruction: {logs[log_idx]['Instruction']}. Prior Instructions: {[logs[i]['Instruction'] for i in range(log_idx)]}")
        take DoneAction()
        while not RequestActionResult(ego):
            if count > 250:
                take DisposeQueriesAction()
                break
            count += 1
            wait
        end_time = time.time()
        UpdateLogs(logs, log_idx, "SendImageAndTextRequestAction", end_time - start_time, count < 250)
        with open(log_file_path, "w") as f:
            json.dump(logs, f, indent=4)
        log_idx += 1
        take DisposeQueriesAction()

        # ---------- Instruction 6 ----------
        take SpeakAction(logs[6]["Instruction"])
        take DoneAction()
        while WaitForSpeakAction(ego, 7):
            wait

        start_time, count = time.time(), 0
        take SendImageAndTextRequestAction(f"Current Instruction: {logs[log_idx]['Instruction']}. Prior Instructions: {[logs[i]['Instruction'] for i in range(log_idx)]}")
        take DoneAction()
        while not RequestActionResult(ego):
            if count > 250:
                take DisposeQueriesAction()
                break
            count += 1
            wait
        end_time = time.time()
        UpdateLogs(logs, log_idx, "SendImageAndTextRequestAction", end_time - start_time, count < 250)
        with open(log_file_path, "w") as f:
            json.dump(logs, f, indent=4)
        log_idx += 1

        # Final done and pain check
        take DoneAction()
        take AskQuestionAction("Did you feel any physical pain or discomfort during or after the exercise? For example, things like pain, fatigue, dizziness, or anything unusual?")
        take DoneAction()
        while not PainRecorded(ego):
            wait
        LogPain(ego, logs)
        with open(log_file_path, "w") as f:
            json.dump(logs, f, indent=4)
        take DoneAction()

    interrupt when DetectedPain(ego):
        take AskQuestionAction("Did you feel any physical pain or discomfort during or after the exercise? For example, things like pain, fatigue, dizziness, or anything unusual?")
        take DoneAction()
        while not PainRecorded(ego):
            wait
        LogPain(ego, logs)
        with open(log_file_path, "w") as f:
            json.dump(logs, f, indent=4)
        take DoneAction()

ego = new Scenicavatar at (0, 0, 0), with behavior Instruction()