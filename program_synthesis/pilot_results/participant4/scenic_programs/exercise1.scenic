import time
import json

from scenic.simulators.unity.actions import *
from scenic.simulators.unity.behaviors import *

model scenic.simulators.unity.model

# Path to save logs
log_file_path = "program_synthesis/logs/reach_into_cabinet.json"
# Create/initialize log file
with open(log_file_path, 'w') as f:
    json.dump({}, f, indent=4)

# Instruction logs
logs = {
    0: {"ActionAPI": "", "Instruction": "Slide your chair closer to the wall if needed.", "Time_Taken": 0, "Completeness": False},
    1: {"ActionAPI": "", "Instruction": "Look towards the ceiling, there should be a little circle on the right side in front of you.", "Time_Taken": 0, "Completeness": False},
    2: {"ActionAPI": "", "Instruction": "Look below the circle, there should be a square.", "Time_Taken": 0, "Completeness": False},
    3: {"ActionAPI": "", "Instruction": "Look below the square, there should be a triangle.", "Time_Taken": 0, "Completeness": False},
    4: {"ActionAPI": "", "Instruction": "With your right hand, place your hand on the white circle on the wall.", "Time_Taken": 0, "Completeness": False},
    5: {"ActionAPI": "", "Instruction": "Move your hand from the circle", "Time_Taken": 0, "Completeness": False},
    6: {"ActionAPI": "", "Instruction": "Reach towards the square with your right hand.", "Time_Taken": 0, "Completeness": False},
    7: {"ActionAPI": "", "Instruction": "Reach towards the triangle with your right hand, go as far as you can comfortably go.", "Time_Taken": 0, "Completeness": False},
    8: {"ActionAPI": "", "Instruction": "Slowly glide your hand down the wall.", "Time_Taken": 0, "Completeness": False},
    9: {"ActionAPI": "", "Instruction": "Repeat steps 5 to 9, with placing your right hand on the circle", "Time_Taken": 0, "Completeness": False},
    10: {"ActionAPI": "", "Instruction": "Then move your hand from the circle", "Time_Taken": 0, "Completeness": False},
    11: {"ActionAPI": "", "Instruction": "Reach towards the square with your right hand.", "Time_Taken": 0, "Completeness": False},
    12: {"ActionAPI": "", "Instruction": "Then reach overhead to the triangle with your right hand", "Time_Taken": 0, "Completeness": False},
    13: {"ActionAPI": "", "Instruction": "Finish by slowly bringing your hand down to each shape again.", "Time_Taken": 0, "Completeness": False}
}

behavior Instruction(circle1, square1, triangle1):
    try:
        # Introduction: goal and precaution
        take SpeakAction("The goal of the exercise is to Improve the patient's ability to reach over like reaching into a cabinet.")
        take SpeakAction("Ensure you're in an environment where it isn't too clustered, where you might trip over anything. If you feel pain, fatigue, or dizziness, please tell the system so that we can terminate the exercise for your safety.")
        take DoneAction()

        while WaitForIntroduction(ego):
            wait

        log_idx = 0

        # ---------- Step 0 ----------
        take SpeakAction(logs[0]["Instruction"])
        take DoneAction()
        while WaitForSpeakAction(ego, 1):
            wait
        start_time, count = time.time(), 0
        take SendImageAndTextRequestAction(f"Current Instruction: {logs[log_idx]['Instruction']}. Prior Instructions: {[logs[i]['Instruction'] for i in range(log_idx)]}")
        take DoneAction()
        while not RequestActionResult(ego):
            if count > 100:
                break
            count += 1
            wait
        end_time = time.time()
        UpdateLogs(logs, log_idx, "SendImageAndTextRequestAction", end_time - start_time, count < 250)
        with open(log_file_path, "w") as f:
            json.dump(logs, f, indent=4)
        log_idx += 1

        # ---------- Step 1 ----------
        take SpeakAction(logs[1]["Instruction"])
        take DoneAction()
        while WaitForSpeakAction(ego, 2):
            wait
        start_time, count = time.time(), 0
        take SendImageAndTextRequestAction(f"Current Instruction: {logs[log_idx]['Instruction']}. Prior Instructions: {[logs[i]['Instruction'] for i in range(log_idx)]}")
        take DoneAction()
        while not RequestActionResult(ego):
            if count > 100:
                break
            count += 1
            wait
        end_time = time.time()
        UpdateLogs(logs, log_idx, "SendImageAndTextRequestAction", end_time - start_time, count < 250)
        with open(log_file_path, "w") as f:
            json.dump(logs, f, indent=4)
        log_idx += 1

        # ---------- Step 2 ----------
        take SpeakAction(logs[2]["Instruction"])
        take DoneAction()
        while WaitForSpeakAction(ego, 3):
            wait
        start_time, count = time.time(), 0
        take SendImageAndTextRequestAction(f"Current Instruction: {logs[log_idx]['Instruction']}. Prior Instructions: {[logs[i]['Instruction'] for i in range(log_idx)]}")
        take DoneAction()
        while not RequestActionResult(ego):
            if count > 100:
                break
            count += 1
            wait
        end_time = time.time()
        UpdateLogs(logs, log_idx, "SendImageAndTextRequestAction", end_time - start_time, count < 250)
        with open(log_file_path, "w") as f:
            json.dump(logs, f, indent=4)
        log_idx += 1

        # ---------- Step 3 ----------
        take SpeakAction(logs[3]["Instruction"])
        take DoneAction()
        while WaitForSpeakAction(ego, 4):
            wait
        start_time, count = time.time(), 0
        take SendImageAndTextRequestAction(f"Current Instruction: {logs[log_idx]['Instruction']}. Prior Instructions: {[logs[i]['Instruction'] for i in range(log_idx)]}")
        take DoneAction()
        while not RequestActionResult(ego):
            if count > 100:
                break
            count += 1
            wait
        end_time = time.time()
        UpdateLogs(logs, log_idx, "SendImageAndTextRequestAction", end_time - start_time, count < 250)
        with open(log_file_path, "w") as f:
            json.dump(logs, f, indent=4)
        log_idx += 1

        # ---------- Step 4 ----------
        take SpeakAction(logs[4]["Instruction"])
        take DoneAction()
        while WaitForSpeakAction(ego, 5):
            wait
        start_time, count = time.time(), 0
        while not CheckDistanceBetweenHandAndObject(ego, circle1, "Right"):
            if count > 250:
                break
            count += 1
            wait
        end_time = time.time()
        UpdateLogs(logs, log_idx, "CheckDistanceBetweenHandAndObject", end_time - start_time, count < 250)
        with open(log_file_path, "w") as f:
            json.dump(logs, f, indent=4)
        log_idx += 1

        # ---------- Step 5 ----------
        take SpeakAction(logs[5]["Instruction"])
        take DoneAction()
        while WaitForSpeakAction(ego, 6):
            wait
        start_time, count = time.time(), 0
        take RecordVideoAndEvaluateAction(f"Current Instruction: {logs[log_idx]['Instruction']}. Prior Instructions: {[logs[i]['Instruction'] for i in range(log_idx)]}")
        take DoneAction()
        while not RequestActionResult(ego):
            if count > 250:
                break
            count += 1
            wait
        end_time = time.time()
        UpdateLogs(logs, log_idx, "RecordVideoAndEvaluateAction", end_time - start_time, count < 250)
        with open(log_file_path, "w") as f:
            json.dump(logs, f, indent=4)
        log_idx += 1

        # ---------- Step 6 ----------
        take SpeakAction(logs[6]["Instruction"])
        take DoneAction()
        while WaitForSpeakAction(ego, 7):
            wait
        start_time, count = time.time(), 0
        while not CheckDistanceBetweenHandAndObject(ego, square1, "Right"):
            if count > 250:
                break
            count += 1
            wait
        end_time = time.time()
        UpdateLogs(logs, log_idx, "CheckDistanceBetweenHandAndObject", end_time - start_time, count < 250)
        with open(log_file_path, "w") as f:
            json.dump(logs, f, indent=4)
        log_idx += 1

        # ---------- Step 7 ----------
        take SpeakAction(logs[7]["Instruction"])
        take DoneAction()
        while WaitForSpeakAction(ego, 8):
            wait
        start_time, count = time.time(), 0
        while not CheckDistanceBetweenHandAndObject(ego, triangle1, "Right"):
            if count > 250:
                break
            count += 1
            wait
        end_time = time.time()
        UpdateLogs(logs, log_idx, "CheckDistanceBetweenHandAndObject", end_time - start_time, count < 250)
        with open(log_file_path, "w") as f:
            json.dump(logs, f, indent=4)
        log_idx += 1

        # ---------- Step 8 ----------
        take SpeakAction(logs[8]["Instruction"])
        take DoneAction()
        while WaitForSpeakAction(ego, 9):
            wait
        start_time, count = time.time(), 0
        take RecordVideoAndEvaluateAction(f"Current Instruction: {logs[log_idx]['Instruction']}. Prior Instructions: {[logs[i]['Instruction'] for i in range(log_idx)]}")
        take DoneAction()
        while not RequestActionResult(ego):
            if count > 250:
                break
            count += 1
            wait
        end_time = time.time()
        UpdateLogs(logs, log_idx, "RecordVideoAndEvaluateAction", end_time - start_time, count < 250)
        with open(log_file_path, "w") as f:
            json.dump(logs, f, indent=4)
        log_idx += 1

        # ---------- Step 9: Repeat loop ----------
        take SpeakAction(logs[9]["Instruction"])
        take DoneAction()
        while WaitForSpeakAction(ego, 10):
            wait
        rep_start, correctness = time.time(), True
        for _ in range(1):  # repeat once as instructed
            # repeat Step 4
            take SpeakAction(logs[4]["Instruction"])
            take DoneAction()
            count = 0
            while not CheckDistanceBetweenHandAndObject(ego, circle1, "Right"):
                if count > 250:
                    break
                count += 1
                wait
            correctness = correctness and (count < 250)
            # repeat Step 5
            take SpeakAction(logs[5]["Instruction"])
            take DoneAction()
            count = 0
            take RecordVideoAndEvaluateAction(f"Current Instruction: {logs[5]['Instruction']}.")
            take DoneAction()
            while not RequestActionResult(ego):
                if count > 250:
                    break
                count += 1
                wait
            correctness = correctness and (count < 250)
            # repeat Step 6
            take SpeakAction(logs[6]["Instruction"])
            take DoneAction()
            count = 0
            while not CheckDistanceBetweenHandAndObject(ego, square1, "Right"):
                if count > 250:
                    break
                count += 1
                wait
            correctness = correctness and (count < 250)
            # repeat Step 7
            take SpeakAction(logs[7]["Instruction"])
            take DoneAction()
            count = 0
            while not CheckDistanceBetweenHandAndObject(ego, triangle1, "Right"):
                if count > 250:
                    break
                count += 1
                wait
            correctness = correctness and (count < 250)
            # repeat Step 8
            take SpeakAction(logs[8]["Instruction"])
            take DoneAction()
            count = 0
            take RecordVideoAndEvaluateAction(f"Current Instruction: {logs[8]['Instruction']}.")
            take DoneAction()
            while not RequestActionResult(ego):
                if count > 250:
                    break
                count += 1
                wait
            correctness = correctness and (count < 250)
        rep_end = time.time()
        UpdateLogs(logs, log_idx, "Repeat Loop (1x)", rep_end - rep_start, correctness)
        with open(log_file_path, "w") as f:
            json.dump(logs, f, indent=4)
        log_idx += 1

        # ---------- Step 10 ----------
        take SpeakAction(logs[10]["Instruction"])
        take DoneAction()
        while WaitForSpeakAction(ego, 11):
            wait
        start_time, count = time.time(), 0
        take RecordVideoAndEvaluateAction(f"Current Instruction: {logs[10]['Instruction']}.")
        take DoneAction()
        while not RequestActionResult(ego):
            if count > 250:
                break
            count += 1
            wait
        end_time = time.time()
        UpdateLogs(logs, log_idx, "RecordVideoAndEvaluateAction", end_time - start_time, count < 250)
        with open(log_file_path, "w") as f:
            json.dump(logs, f, indent=4)
        log_idx += 1

        # ---------- Step 11 ----------
        take SpeakAction(logs[11]["Instruction"])
        take DoneAction()
        while WaitForSpeakAction(ego, 12):
            wait
        start_time, count = time.time(), 0
        while not CheckDistanceBetweenHandAndObject(ego, square1, "Right"):
            if count > 250:
                break
            count += 1
            wait
        end_time = time.time()
        UpdateLogs(logs, log_idx, "CheckDistanceBetweenHandAndObject", end_time - start_time, count < 250)
        with open(log_file_path, "w") as f:
            json.dump(logs, f, indent=4)
        log_idx += 1

        # ---------- Step 12 ----------
        take SpeakAction(logs[12]["Instruction"])
        take DoneAction()
        while WaitForSpeakAction(ego, 13):
            wait
        start_time, count = time.time(), 0
        while not CheckDistanceBetweenHandAndObject(ego, triangle1, "Right"):
            if count > 250:
                break
            count += 1
            wait
        end_time = time.time()
        UpdateLogs(logs, log_idx, "CheckDistanceBetweenHandAndObject", end_time - start_time, count < 250)
        with open(log_file_path, "w") as f:
            json.dump(logs, f, indent=4)
        log_idx += 1

        # ---------- Step 13 ----------
        take SpeakAction(logs[13]["Instruction"])
        take DoneAction()
        while WaitForSpeakAction(ego, 14):
            wait
        start_time, count = time.time(), 0
        take RecordVideoAndEvaluateAction(f"Current Instruction: {logs[13]['Instruction']}.")
        take DoneAction()
        while not RequestActionResult(ego):
            if count > 250:
                break
            count += 1
            wait
        end_time = time.time()
        UpdateLogs(logs, log_idx, "RecordVideoAndEvaluateAction", end_time - start_time, count < 250)
        with open(log_file_path, "w") as f:
            json.dump(logs, f, indent=4)

        # Finalize
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

# Spawn virtual objects
circle1 = new Circle at (-0.4918002, -0.371269166, -0.972810864), \
    with pitch -1.41717865e-05 deg, \
    with yaw 185.567383 deg, \
    with roll -1.27222191e-12 deg

square1 = new Square at (-0.486675739, -0.155810729, -0.973310351), \
    with pitch -1.41717865e-05 deg, \
    with yaw 185.567383 deg, \
    with roll -1.27222191e-12 deg

triangle1 = new Triangle at (-0.49137333, 0.07607846, -0.9728524), \
    with pitch -1.41717865e-05 deg, \
    with yaw 185.567383 deg, \
    with roll -1.27222191e-12 deg

ego = new Scenicavatar at (0, 0, 0), with behavior Instruction(circle1, square1, triangle1)