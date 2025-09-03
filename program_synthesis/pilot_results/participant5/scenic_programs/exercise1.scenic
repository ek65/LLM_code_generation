import time
import json

from scenic.simulators.unity.actions import *
from scenic.simulators.unity.behaviors import *

model scenic.simulators.unity.model

# Path to save logs
log_file_path = "program_synthesis/logs/look_and_touch_circle.json"

# Initialize log file
with open(log_file_path, 'w') as f:
    json.dump({}, f, indent=4)

# Instruction logs
logs = {
    0: {"ActionAPI": "", "Instruction": "1. Look at the circle in front of you.", "Time_Taken": 0, "Completeness": False},
    1: {"ActionAPI": "", "Instruction": "2. Reach out with your right arm and touch the circle.", "Time_Taken": 0, "Completeness": False},
    2: {"ActionAPI": "", "Instruction": "3. Bend your right elbow as far as you can as if you're trying to touch your face.", "Time_Taken": 0, "Completeness": False},
    3: {"ActionAPI": "", "Instruction": "4. Touch the circle again with your right arm.", "Time_Taken": 0, "Completeness": False},
    4: {"ActionAPI": "", "Instruction": "5. Repeat the process of touching the circle and bending your right elbow three times.", "Time_Taken": 0, "Completeness": False}
}

behavior Instruction(a_circle_1):
    try:
        # Introduction: goal and precaution
        take SpeakAction("The goal of the exercise is None.")
        take SpeakAction("The precaution is: If you feel pain, fatigue, or dizziness, please tell the system so that we can terminate the exercise for your safety.")
        take DoneAction()

        while WaitForIntroduction(ego):
            wait

        log_idx = 0

        # ---------- Instruction 0: Look at the circle ----------
        take SpeakAction("1. Look at the circle in front of you.")
        take DoneAction()
        while WaitForSpeakAction(ego, 1):
            wait

        start_time, count = time.time(), 0
        take SendImageAndTextRequestAction(
            f"Current Instruction: {logs[log_idx]['Instruction']}. "
            f"Prior Instructions: {[logs[i]['Instruction'] for i in range(log_idx)]}"
        )
        take DoneAction()
        while not RequestActionResult(ego):
            if count > 250:
                take DisposeQueriesAction()
                break
            count += 1
            wait
        end_time = time.time()
        UpdateLogs(logs, log_idx, "SendImageAndTextRequestAction", end_time - start_time, count < 250)
        with open(log_file_path, 'w') as file:
            json.dump(logs, file, indent=4)
        log_idx += 1
        take DisposeQueriesAction()

        # ---------- Instruction 1: Reach out and touch the circle ----------
        take SpeakAction("2. Reach out with your right arm and touch the circle.")
        take DoneAction()
        while WaitForSpeakAction(ego, 2):
            wait

        start_time, count = time.time(), 0
        while not CheckDistanceBetweenHandAndObject(ego, a_circle_1, "Right"):
            if count > 250:
                break
            count += 1
            wait
        end_time = time.time()
        UpdateLogs(logs, log_idx, "CheckDistanceBetweenHandAndObject", end_time - start_time, count < 250)
        with open(log_file_path, 'w') as file:
            json.dump(logs, file, indent=4)
        log_idx += 1

        # ---------- Instruction 2: Bend your right elbow ----------
        take SpeakAction("3. Bend your right elbow as far as you can as if you're trying to touch your face.")
        take DoneAction()
        while WaitForSpeakAction(ego, 3):
            wait

        start_time, count = time.time(), 0
        while not CheckElbowBend(ego, "Right"):
            if count > 250:
                break
            count += 1
            wait
        end_time = time.time()
        UpdateLogs(logs, log_idx, "CheckElbowBend", end_time - start_time, count < 250)
        with open(log_file_path, 'w') as file:
            json.dump(logs, file, indent=4)
        log_idx += 1

        # ---------- Instruction 3: Touch the circle again ----------
        take SpeakAction("4. Touch the circle again with your right arm.")
        take DoneAction()
        while WaitForSpeakAction(ego, 4):
            wait

        start_time, count = time.time(), 0
        while not CheckDistanceBetweenHandAndObject(ego, a_circle_1, "Right"):
            if count > 250:
                break
            count += 1
            wait
        end_time = time.time()
        UpdateLogs(logs, log_idx, "CheckDistanceBetweenHandAndObject", end_time - start_time, count < 250)
        with open(log_file_path, 'w') as file:
            json.dump(logs, file, indent=4)
        log_idx += 1

        # ---------- Instruction 4: Repeat the process three times ----------
        take SpeakAction("5. Repeat the process of touching the circle and bending your right elbow three times.")
        take DoneAction()
        while WaitForSpeakAction(ego, 5):
            wait

        rep_start = time.time()
        correctness = True
        for _ in range(3):
            take SpeakAction("4. Touch the circle again with your right arm.")
            take DoneAction()
            count = 0
            while not CheckDistanceBetweenHandAndObject(ego, a_circle_1, "Right"):
                if count > 250:
                    break
                count += 1
                wait
            correctness = correctness and (count < 250)

            take SpeakAction("3. Bend your right elbow as far as you can as if you're trying to touch your face.")
            take DoneAction()
            count = 0
            while not CheckElbowBend(ego, "Right"):
                if count > 250:
                    break
                count += 1
                wait
            correctness = correctness and (count < 250)

        rep_end = time.time()
        UpdateLogs(logs, log_idx, "Repeat Loop (3x)", rep_end - rep_start, correctness)
        with open(log_file_path, 'w') as file:
            json.dump(logs, file, indent=4)
        log_idx += 1

        take DoneAction()

        # Ask about pain or discomfort
        take AskQuestionAction(
            "Did you feel any physical pain or discomfort during or after the exercise? "
            "For example, things like pain, fatigue, dizziness, or anything unusual?"
        )
        take DoneAction()
        while not PainRecorded(ego):
            wait

        LogPain(ego, logs)
        with open(log_file_path, 'w') as file:
            json.dump(logs, file, indent=4)
        take DoneAction()

    interrupt when DetectedPain(ego):
        take AskQuestionAction(
            "Did you feel any physical pain or discomfort during or after the exercise? "
            "For example, things like pain, fatigue, dizziness, or anything unusual?"
        )
        take DoneAction()
        while not PainRecorded(ego):
            wait

        LogPain(ego, logs)
        with open(log_file_path, 'w') as file:
            json.dump(logs, file, indent=4)
        take DoneAction()

# Instantiate objects
a_circle_1 = new Circle at (0.07704203, -0.7813944, 0.16032058), with pitch 270.0 deg, with yaw 0.0 deg, with roll 0.0 deg
ego = new Scenicavatar at (0, 0, 0), with behavior Instruction(a_circle_1)