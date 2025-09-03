import time
import json

from scenic.simulators.unity.actions import *
from scenic.simulators.unity.behaviors import *

model scenic.simulators.unity.model

# Path to save logs
log_file_path = "program_synthesis/logs/stack_plates.json"

# Initialize log file
with open(log_file_path, 'w') as f:
    json.dump({}, f, indent=4)

# Instruction logs
logs = {
    0: {"ActionAPI": "", "Instruction": "Pull the bowls closer towards you with both of your hands.", "Time_Taken": 0, "Completeness": False},
    1: {"ActionAPI": "", "Instruction": "With your right arm, pull the bowl on the right closer.", "Time_Taken": 0, "Completeness": False},
    2: {"ActionAPI": "", "Instruction": "With your right arm, reach diagonally over to the last pole.", "Time_Taken": 0, "Completeness": False},
    3: {"ActionAPI": "", "Instruction": "Use your left hand to grab the bowl on the side.", "Time_Taken": 0, "Completeness": False},
    4: {"ActionAPI": "", "Instruction": "Use your right hand to grab the bowl on the side.", "Time_Taken": 0, "Completeness": False},
    5: {"ActionAPI": "", "Instruction": "Stack the grabbed bowl on top of the one in the middle.", "Time_Taken": 0, "Completeness": False},
    6: {"ActionAPI": "", "Instruction": "With your left hand, grab the bowl to the right.", "Time_Taken": 0, "Completeness": False},
    7: {"ActionAPI": "", "Instruction": "With your right hand, grab the bowl to the right.", "Time_Taken": 0, "Completeness": False},
    8: {"ActionAPI": "", "Instruction": "Place the bowl grabbed from the right to the middle.", "Time_Taken": 0, "Completeness": False},
    9: {"ActionAPI": "", "Instruction": "Repeat the process of stacking bowls for two times in a row.", "Time_Taken": 0, "Completeness": False},
    10: {"ActionAPI": "", "Instruction": "Place the stacked bowls anywhere on the table.", "Time_Taken": 0, "Completeness": False},
    11: {"ActionAPI": "", "Instruction": "Repeat the entire process one more time.", "Time_Taken": 0, "Completeness": False}
}

behavior Instruction():
    try:
        # Introduction: goal and precaution
        take SpeakAction("The goal of the exercise is To improve the ability to stack plates on one another after a meal is finished.")
        take SpeakAction("The patient should be seated in a sturdy chair and be in front of a table that won't slide. If you feel pain, fatigue, or dizziness, please tell the system so that we can terminate the exercise for your safety.")
        take DoneAction()

        # Wait for introduction to finish
        while WaitForIntroduction(ego):
            wait

        log_idx = 0

        # ---------- Instruction 1 ----------
        take SpeakAction(logs[log_idx]["Instruction"])
        take DoneAction()
        while WaitForSpeakAction(ego, 1):
            wait
        start_time, count = time.time(), 0
        take SendImageAndTextRequestAction(f"Current Instruction: {logs[log_idx]['Instruction']}. Prior Instructions: {[logs[i]['Instruction'] for i in range(log_idx)]}")
        take DoneAction()
        while not RequestActionResult(ego):
            if count > 250:
                break
            count += 1
            wait
        end_time = time.time()
        UpdateLogs(logs, log_idx, "SendImageAndTextRequestAction", end_time - start_time, count < 250)
        with open(log_file_path, "w") as file:
            json.dump(logs, file, indent=4)
        log_idx += 1

        # ---------- Instruction 2 ----------
        take SpeakAction(logs[log_idx]["Instruction"])
        take DoneAction()
        while WaitForSpeakAction(ego, 2):
            wait
        start_time, count = time.time(), 0
        take SendImageAndTextRequestAction(f"Current Instruction: {logs[log_idx]['Instruction']}. Prior Instructions: {[logs[i]['Instruction'] for i in range(log_idx)]}")
        take DoneAction()
        while not RequestActionResult(ego):
            if count > 250:
                break
            count += 1
            wait
        end_time = time.time()
        UpdateLogs(logs, log_idx, "SendImageAndTextRequestAction", end_time - start_time, count < 250)
        with open(log_file_path, "w") as file:
            json.dump(logs, file, indent=4)
        log_idx += 1

        # ---------- Instruction 3 ----------
        take SpeakAction(logs[log_idx]["Instruction"])
        take DoneAction()
        while WaitForSpeakAction(ego, 3):
            wait
        start_time, count = time.time(), 0
        take SendImageAndTextRequestAction(f"Current Instruction: {logs[log_idx]['Instruction']}. Prior Instructions: {[logs[i]['Instruction'] for i in range(log_idx)]}")
        take DoneAction()
        while not RequestActionResult(ego):
            if count > 250:
                break
            count += 1
            wait
        end_time = time.time()
        UpdateLogs(logs, log_idx, "SendImageAndTextRequestAction", end_time - start_time, count < 250)
        with open(log_file_path, "w") as file:
            json.dump(logs, file, indent=4)
        log_idx += 1

        # ---------- Instruction 4 ----------
        take SpeakAction(logs[log_idx]["Instruction"])
        take DoneAction()
        while WaitForSpeakAction(ego, 4):
            wait
        start_time, count = time.time(), 0
        take SendImageAndTextRequestAction(f"Current Instruction: {logs[log_idx]['Instruction']}. Prior Instructions: {[logs[i]['Instruction'] for i in range(log_idx)]}")
        take DoneAction()
        while not RequestActionResult(ego):
            if count > 250:
                break
            count += 1
            wait
        end_time = time.time()
        UpdateLogs(logs, log_idx, "SendImageAndTextRequestAction", end_time - start_time, count < 250)
        with open(log_file_path, "w") as file:
            json.dump(logs, file, indent=4)
        log_idx += 1

        # ---------- Instruction 5 ----------
        take SpeakAction(logs[log_idx]["Instruction"])
        take DoneAction()
        while WaitForSpeakAction(ego, 5):
            wait
        start_time, count = time.time(), 0
        take SendImageAndTextRequestAction(f"Current Instruction: {logs[log_idx]['Instruction']}. Prior Instructions: {[logs[i]['Instruction'] for i in range(log_idx)]}")
        take DoneAction()
        while not RequestActionResult(ego):
            if count > 250:
                break
            count += 1
            wait
        end_time = time.time()
        UpdateLogs(logs, log_idx, "SendImageAndTextRequestAction", end_time - start_time, count < 250)
        with open(log_file_path, "w") as file:
            json.dump(logs, file, indent=4)
        log_idx += 1

        # ---------- Instruction 6 ----------
        take SpeakAction(logs[log_idx]["Instruction"])
        take DoneAction()
        while WaitForSpeakAction(ego, 6):
            wait
        start_time, count = time.time(), 0
        take SendImageAndTextRequestAction(f"Current Instruction: {logs[log_idx]['Instruction']}. Prior Instructions: {[logs[i]['Instruction'] for i in range(log_idx)]}")
        take DoneAction()
        while not RequestActionResult(ego):
            if count > 250:
                break
            count += 1
            wait
        end_time = time.time()
        UpdateLogs(logs, log_idx, "SendImageAndTextRequestAction", end_time - start_time, count < 250)
        with open(log_file_path, "w") as file:
            json.dump(logs, file, indent=4)
        log_idx += 1

        # ---------- Instruction 7 ----------
        take SpeakAction(logs[log_idx]["Instruction"])
        take DoneAction()
        while WaitForSpeakAction(ego, 7):
            wait
        start_time, count = time.time(), 0
        take SendImageAndTextRequestAction(f"Current Instruction: {logs[log_idx]['Instruction']}. Prior Instructions: {[logs[i]['Instruction'] for i in range(log_idx)]}")
        take DoneAction()
        while not RequestActionResult(ego):
            if count > 250:
                break
            count += 1
            wait
        end_time = time.time()
        UpdateLogs(logs, log_idx, "SendImageAndTextRequestAction", end_time - start_time, count < 250)
        with open(log_file_path, "w") as file:
            json.dump(logs, file, indent=4)
        log_idx += 1

        # ---------- Instruction 8 ----------
        take SpeakAction(logs[log_idx]["Instruction"])
        take DoneAction()
        while WaitForSpeakAction(ego, 8):
            wait
        start_time, count = time.time(), 0
        take SendImageAndTextRequestAction(f"Current Instruction: {logs[log_idx]['Instruction']}. Prior Instructions: {[logs[i]['Instruction'] for i in range(log_idx)]}")
        take DoneAction()
        while not RequestActionResult(ego):
            if count > 250:
                break
            count += 1
            wait
        end_time = time.time()
        UpdateLogs(logs, log_idx, "SendImageAndTextRequestAction", end_time - start_time, count < 250)
        with open(log_file_path, "w") as file:
            json.dump(logs, file, indent=4)
        log_idx += 1

        # ---------- Instruction 9 ----------
        take SpeakAction(logs[log_idx]["Instruction"])
        take DoneAction()
        while WaitForSpeakAction(ego, 9):
            wait
        start_time, count = time.time(), 0
        take SendImageAndTextRequestAction(f"Current Instruction: {logs[log_idx]['Instruction']}. Prior Instructions: {[logs[i]['Instruction'] for i in range(log_idx)]}")
        take DoneAction()
        while not RequestActionResult(ego):
            if count > 250:
                break
            count += 1
            wait
        end_time = time.time()
        UpdateLogs(logs, log_idx, "SendImageAndTextRequestAction", end_time - start_time, count < 250)
        with open(log_file_path, "w") as file:
            json.dump(logs, file, indent=4)
        log_idx += 1

        # ---------- Instruction 10: Repeat stacking twice ----------
        take SpeakAction(logs[log_idx]["Instruction"])
        take DoneAction()
        while WaitForSpeakAction(ego, 10):
            wait
        rep_start, correctness = time.time(), True
        for _ in range(2):
            for j in range(3, 9):  # steps 4–9
                take SpeakAction(logs[j]["Instruction"])
                take DoneAction()
                count = 0
                take SendImageAndTextRequestAction(f"Current Instruction: {logs[j]['Instruction']}. Prior Instructions: {[logs[i]['Instruction'] for i in range(log_idx)]}")
                take DoneAction()
                while not RequestActionResult(ego):
                    if count > 250:
                        break
                    count += 1
                    wait
                correctness = correctness and (count < 250)
        rep_end = time.time()
        UpdateLogs(logs, log_idx, "Repeat Loop (2x)", rep_end - rep_start, correctness)
        with open(log_file_path, "w") as file:
            json.dump(logs, file, indent=4)
        log_idx += 1

        # ---------- Instruction 11 ----------
        take SpeakAction(logs[log_idx]["Instruction"])
        take DoneAction()
        while WaitForSpeakAction(ego, 11):
            wait
        start_time, count = time.time(), 0
        take SendImageAndTextRequestAction(f"Current Instruction: {logs[log_idx]['Instruction']}. Prior Instructions: {[logs[i]['Instruction'] for i in range(log_idx)]}")
        take DoneAction()
        while not RequestActionResult(ego):
            if count > 250:
                break
            count += 1
            wait
        end_time = time.time()
        UpdateLogs(logs, log_idx, "SendImageAndTextRequestAction", end_time - start_time, count < 250)
        with open(log_file_path, "w") as file:
            json.dump(logs, file, indent=4)
        log_idx += 1

        # ---------- Instruction 12: Repeat entire process once ----------
        take SpeakAction(logs[log_idx]["Instruction"])
        take DoneAction()
        while WaitForSpeakAction(ego, 12):
            wait
        rep_start, correctness = time.time(), True
        for _ in range(1):
            for j in range(0, 11):  # steps 1–11
                take SpeakAction(logs[j]["Instruction"])
                take DoneAction()
                count = 0
                take SendImageAndTextRequestAction(f"Current Instruction: {logs[j]['Instruction']}. Prior Instructions: {[logs[i]['Instruction'] for i in range(log_idx)]}")
                take DoneAction()
                while not RequestActionResult(ego):
                    if count > 250:
                        break
                    count += 1
                    wait
                correctness = correctness and (count < 250)
        rep_end = time.time()
        UpdateLogs(logs, log_idx, "Repeat Loop (1x)", rep_end - rep_start, correctness)
        with open(log_file_path, "w") as file:
            json.dump(logs, file, indent=4)
        log_idx += 1

        # Final done and pain check
        take DoneAction()
        take AskQuestionAction("Did you feel any physical pain or discomfort during or after the exercise? For example, things like pain, fatigue, dizziness, or anything unusual?")
        take DoneAction()
        while not PainRecorded(ego):
            wait
        LogPain(ego, logs)
        with open(log_file_path, "w") as file:
            json.dump(logs, file, indent=4)
        take DoneAction()

    interrupt when DetectedPain(ego):
        take AskQuestionAction("Did you feel any physical pain or discomfort during or after the exercise? For example, things like pain, fatigue, dizziness, or anything unusual?")
        take DoneAction()
        while not PainRecorded(ego):
            wait
        LogPain(ego, logs)
        with open(log_file_path, "w") as file:
            json.dump(logs, file, indent=4)
        take DoneAction()

# Instantiate avatar
ego = new Scenicavatar at (0, 0, 0), with behavior Instruction()