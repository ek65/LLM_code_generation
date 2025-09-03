import time
import json

from scenic.simulators.unity.actions import *
from scenic.simulators.unity.behaviors import *

model scenic.simulators.unity.model

log_file_path = "program_synthesis/logs/finger_separation_exercise.json"

# Create log json file 
with open(log_file_path, 'w') as f:
    json.dump({}, f, indent=4)

# Instruction logs
logs = {
    0: {
        "ActionAPI": "",
        "Instruction": "Separate fingers as wide as you can on Right hand.",
        "Time_Taken": 0,
        "Completeness": False
    },
    1: {
        "ActionAPI": "",
        "Instruction": "Bring fingers back together.",
        "Time_Taken": 0,
        "Completeness": False
    },
    2: {
        "ActionAPI": "",
        "Instruction": "Repeat the first and second steps for 3 reps. Let me guide you through the steps.",
        "Time_Taken": 0,
        "Completeness": False
    }
}

behavior Instruction():
    take SpeakAction("Let's start a new exercise.")
    take SpeakAction("Remove any items from the table that you could knock off.")
    take DoneAction()

    while WaitForIntroduction(ego):
        wait

    log_idx = 0
    speak_idx = 1

    # ---------- Instruction 0 ----------
    take SpeakAction("Separate fingers as wide as you can on Right hand.")
    take DoneAction()

    while WaitForSpeakAction(ego, speak_idx):
        wait
    speak_idx += 1

    start_time, count = time.time(), 0
    take DoneAction()
    while not CheckFingerSpread(ego, "Right"):
        if count > 175:
            break
        count += 1
        wait
    end_time = time.time()
    UpdateLogs(logs, log_idx, "CheckFingerSpread", end_time - start_time, count < 175)
    with open(log_file_path, "w") as file:
        json.dump(logs, file, indent=4)
    log_idx += 1

    # ---------- Instruction 1 ----------
    take SpeakAction("Bring fingers back together.")
    take DoneAction()

    while WaitForSpeakAction(ego, speak_idx):
        wait
    speak_idx += 1

    start_time, count = time.time(), 0
    take DoneAction()
    while not CheckFingerAdduction(ego, "Right"):
        if count > 175:
            break
        count += 1
        wait
    end_time = time.time()
    UpdateLogs(logs, log_idx, "CheckFingerAdduction", end_time - start_time, count < 175)
    with open(log_file_path, "w") as file:
        json.dump(logs, file, indent=4)
    log_idx += 1

    # ---------- Instruction 2 (Repeat Loop) ----------
    take SpeakAction("Repeat the first and second steps for 3 reps. Let me guide you through the steps.")
    take DoneAction()

    while WaitForSpeakAction(ego, speak_idx):
        wait
    speak_idx += 1

    rep_start_time = time.time()
    correctness = True

    for i in range(3):
        # Repeat Instruction 0 without logging
        take SpeakAction("Separate fingers as wide as you can on Right hand.")
        take DoneAction()

        while WaitForSpeakAction(ego, speak_idx):
            wait
        speak_idx += 1

        count = 0
        while not CheckFingerSpread(ego, "Right"):
            if count > 175:
                break
            count += 1
            wait
        correctness = correctness and (count < 175)

        # Repeat Instruction 1 without logging
        take SpeakAction("Bring fingers back together.")
        take DoneAction()

        while WaitForSpeakAction(ego, speak_idx):
            wait
        speak_idx += 1

        count = 0
        while not CheckFingerAdduction(ego, "Right"):
            if count > 175:
                break
            count += 1
            wait
        correctness = correctness and (count < 175)

    rep_end_time = time.time()
    UpdateLogs(logs, log_idx, "Repeat Loop (3x)", rep_end_time - rep_start_time, correctness)
    with open(log_file_path, "w") as file:
        json.dump(logs, file, indent=4)
    log_idx += 1

    # ---------- Inform the Patient that the Exercise is Completed ----------
    take SpeakAction("Great job! You finished this exercise.")
    take DoneAction()

    while WaitForSpeakAction(ego, speak_idx):
        wait
    speak_idx += 1
    take DisposeQueriesAction()

ego = new Scenicavatar at (0, 0, 0), with behavior Instruction()