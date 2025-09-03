import time
import json

from scenic.simulators.unity.actions import *
from scenic.simulators.unity.behaviors import *

model scenic.simulators.unity.model

log_file_path = "program_synthesis/logs/shelf_reaching_exercise.json"

# Create log json file
with open(log_file_path, 'w') as f:
    json.dump({}, f, indent=4)

# Instruction logs
logs = {
    0: {
        "ActionAPI": "",
        "Instruction": "Stand in front of the shelf with [cup, toothbrush, plate] resting on it.",
        "Time_Taken": 0,
        "Completeness": False
    },
    1: {
        "ActionAPI": "",
        "Instruction": "Place object(s) within X inches from shelf edge.",
        "Time_Taken": 0,
        "Completeness": False
    },
    2: {
        "ActionAPI": "",
        "Instruction": "Reach for object and try to grasp with just the affected arm. Move object to X. Replace object at original starting point. Repeat X times.",
        "Time_Taken": 0,
        "Completeness": False
    },
    3: {
        "ActionAPI": "",
        "Instruction": "Place object within X inches from table edge (vs 'farther away'), repeat step 3 X times.",
        "Time_Taken": 0,
        "Completeness": False
    },
    4: {
        "ActionAPI": "",
        "Instruction": "Place object at farthest reachable point by unaffected limb. Repeat step 3 with affected limb X times.",
        "Time_Taken": 0,
        "Completeness": False
    }
}

behavior Instruction():
    take SpeakAction("Let's start a new exercise.")
    take SpeakAction("If you feel pain, fatigue, or dizziness, please tell the system so that we can terminate the exercise for your safety.")
    take DoneAction()

    while WaitForIntroduction(ego):
        wait

    log_idx = 0
    speak_idx = 1

    # ---------- Instruction 0 ----------
    take SpeakAction("Stand in front of the shelf with [cup, toothbrush, plate] resting on it.")
    take DoneAction()

    while WaitForSpeakAction(ego, speak_idx):
        wait
    speak_idx += 1

    start_time, count = time.time(), 0
    take SendImageAndTextRequestAction(f"Current Instruction: be in front of the shelf with [cup, toothbrush, plate] resting on it. Prior Instructions: {[logs[i]['Instruction'] for i in range(log_idx)]}")
    take DoneAction()
    while not (CheckStanding(ego) and RequestActionResult(ego)):
        if count > 175:
            break
        count += 1
        wait
    end_time = time.time()
    UpdateLogs(logs, log_idx, "SendImageAndTextRequestAction+CheckStanding", end_time - start_time, count < 175)
    with open(log_file_path, "w") as file:
        json.dump(logs, file, indent=4)
    log_idx += 1
    take DisposeQueriesAction()

    # ---------- Instruction 1 ----------
    take SpeakAction("Place object(s) within X inches from shelf edge.")
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
    with open(log_file_path, "w") as file:
        json.dump(logs, file, indent=4)
    log_idx += 1
    take DisposeQueriesAction()

    # ---------- Instruction 2 ----------
    take SpeakAction("Reach for object and try to grasp with just the affected arm. Move object to X. Replace object at original starting point. Repeat X times.")
    take DoneAction()

    while WaitForSpeakAction(ego, speak_idx):
        wait
    speak_idx += 1

    start_time, count = time.time(), 0
    ext = CheckElbowExtension()
    take SendImageAndTextRequestAction(f"Current Instruction: grasp the object with the affected arm, move object to X, and replace object at original starting point. Prior Instructions: {[logs[i]['Instruction'] for i in range(log_idx)]}")
    take DoneAction()
    while not (ext.checkCompleted(ego, arm='Right') and RequestActionResult(ego)):
        if count > 175:
            break
        count += 1
        wait
    end_time = time.time()
    UpdateLogs(logs, log_idx, "CheckElbowExtension+SendImageAndTextRequestAction", end_time - start_time, count < 175)
    with open(log_file_path, "w") as file:
        json.dump(logs, file, indent=4)
    log_idx += 1
    take DisposeQueriesAction()

    # ---------- Instruction 3 ----------
    take SpeakAction("Place object within X inches from table edge (vs 'farther away'), repeat step 3 X times.")
    take DoneAction()

    while WaitForSpeakAction(ego, speak_idx):
        wait
    speak_idx += 1

    rep_start_time = time.time()
    correctness = True

    for i in range(5):  # Assuming X is 5 for repetition
        # Repeat placing object
        take SpeakAction("Place object within X inches from table edge (vs 'farther away').")
        take DoneAction()

        while WaitForSpeakAction(ego, speak_idx):
            wait
        speak_idx += 1

        count = 0
        take SendImageAndTextRequestAction(f"Current Instruction: Place object within X inches from table edge. Prior Instructions: {[logs[i]['Instruction'] for i in range(log_idx)]}")
        take DoneAction()
        while not RequestActionResult(ego):
            if count > 175:
                break
            count += 1
            wait
        take DisposeQueriesAction()
        correctness = correctness and (count < 175)

        # Repeat step 3 actions
        take SpeakAction("Reach for object and try to grasp with just the affected arm. Move object to X. Replace object at original starting point.")
        take DoneAction()

        while WaitForSpeakAction(ego, speak_idx):
            wait
        speak_idx += 1

        count = 0
        ext = CheckElbowExtension()
        take SendImageAndTextRequestAction(f"Current Instruction: grasp the object with the affected arm, move object to X, and replace object at original starting point. Prior Instructions: {[logs[i]['Instruction'] for i in range(log_idx)]}")
        take DoneAction()
        while not (ext.checkCompleted(ego, arm='Right') and RequestActionResult(ego)):
            if count > 175:
                break
            count += 1
            wait
        take DisposeQueriesAction()
        correctness = correctness and (count < 175)

    rep_end_time = time.time()
    UpdateLogs(logs, log_idx, "Repeat Loop (5x)", rep_end_time - rep_start_time, correctness)
    with open(log_file_path, "w") as file:
        json.dump(logs, file, indent=4)
    log_idx += 1

    # ---------- Instruction 4 ----------
    take SpeakAction("Place object at farthest reachable point by unaffected limb. Repeat step 3 with affected limb X times.")
    take DoneAction()

    while WaitForSpeakAction(ego, speak_idx):
        wait
    speak_idx += 1

    rep_start_time = time.time()
    correctness = True

    for i in range(5):  # Assuming X is 5 for repetition
        # Repeat placing object at farthest point
        take SpeakAction("Place object at farthest reachable point by unaffected limb.")
        take DoneAction()

        while WaitForSpeakAction(ego, speak_idx):
            wait
        speak_idx += 1

        count = 0
        take SendImageAndTextRequestAction(f"Current Instruction: Place object at farthest reachable point by unaffected limb. Prior Instructions: {[logs[i]['Instruction'] for i in range(log_idx)]}")
        take DoneAction()
        while not RequestActionResult(ego):
            if count > 175:
                break
            count += 1
            wait
        take DisposeQueriesAction()
        correctness = correctness and (count < 175)

        # Repeat step 3 actions with affected limb
        take SpeakAction("Reach for object and try to grasp with just the affected arm. Move object to X. Replace object at original starting point.")
        take DoneAction()

        while WaitForSpeakAction(ego, speak_idx):
            wait
        speak_idx += 1

        count = 0
        ext = CheckElbowExtension()
        take SendImageAndTextRequestAction(f"Current Instruction: grasp the object with the affected arm, move object to X, and replace object at original starting point. Prior Instructions: {[logs[i]['Instruction'] for i in range(log_idx)]}")
        take DoneAction()
        while not (ext.checkCompleted(ego, arm='Right') and RequestActionResult(ego)):
            if count > 175:
                break
            count += 1
            wait
        take DisposeQueriesAction()
        correctness = correctness and (count < 175)

    rep_end_time = time.time()
    UpdateLogs(logs, log_idx, "Repeat Loop (5x)", rep_end_time - rep_start_time, correctness)
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