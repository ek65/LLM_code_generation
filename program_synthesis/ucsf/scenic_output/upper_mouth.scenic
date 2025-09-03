import time
import json

from scenic.simulators.unity.actions import *
from scenic.simulators.unity.behaviors import *

model scenic.simulators.unity.model

log_file_path = "program_synthesis/logs/hand_to_mouth_exercise.json"

# Create log json file 
with open(log_file_path, 'w') as f:
    json.dump({}, f, indent=4)

# Instruction logs
logs = {
    0: {
        "ActionAPI": "",
        "Instruction": "1. place the right hand at the placeholder1",
        "Time_Taken": 0,
        "Completeness": False
    },
    1: {
        "ActionAPI": "",
        "Instruction": "2. bring your right hand to your mouth",
        "Time_Taken": 0,
        "Completeness": False
    },
    2: {
        "ActionAPI": "",
        "Instruction": "3. return the hand to its initial position",
        "Time_Taken": 0,
        "Completeness": False
    },
    3: {
        "ActionAPI": "",
        "Instruction": "4. repeat steps 1-3 4 more times for placeholders 2-5",
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
    take SpeakAction("1. using your right hand, place the cup at circle1")
    take DoneAction()

    while WaitForSpeakAction(ego, speak_idx): 
        wait
    speak_idx += 1 

    start_time, count = time.time(), 0
    take DoneAction()
    while not CheckObjectTouch(ego, "right", circle1):
        if count > 175:
            break
        count += 1
        wait
    end_time = time.time()
    UpdateLogs(logs, log_idx, "CheckObjectTouch", end_time - start_time, count < 175)
    with open(log_file_path, "w") as file:
        json.dump(logs, file, indent=4)
    log_idx += 1

    # ---------- Instruction 1 ----------
    take SpeakAction("2. using your right hand, bring the cup to your mouth")
    take DoneAction()

    while WaitForSpeakAction(ego, speak_idx): 
        wait
    speak_idx += 1 

    start_time, count = time.time(), 0
    while not (CheckFaceTouch(ego, "Right")):
        if count > 175:
            break
        count += 1
        wait
    end_time = time.time()
    UpdateLogs(logs, log_idx, "CheckFaceTouch", end_time - start_time, count < 175)
    with open(log_file_path, "w") as file:
        json.dump(logs, file, indent=4)
    log_idx += 1

    # ---------- Instruction 2 ----------
    take SpeakAction("3. return the cup to its initial position")
    take DoneAction()

    while WaitForSpeakAction(ego, speak_idx): 
        wait
    speak_idx += 1 

    start_time, count = time.time(), 0
    take DoneAction()
    while not CheckObjectTouch(ego, "right", circle1):
        if count > 175:
            break
        count += 1
        wait
    end_time = time.time()
    UpdateLogs(logs, log_idx, "CheckObjectTouch", end_time - start_time, count < 175)
    with open(log_file_path, "w") as file:
        json.dump(logs, file, indent=4)
    log_idx += 1

    

    # ---------- Inform the Patient that the Exercise is Completed ----------
    take SpeakAction("Great job! You finished this exercise.")
    take DoneAction()

    while WaitForSpeakAction(ego, speak_idx): 
        wait
    speak_idx += 1

circle1 = new Circle at (0.326971948,
                -0.537628233,
                0.4644565),
	 with pitch -0.0 deg,
	 with yaw -0.0 deg,
	 with roll -0.0 deg

circle2 = new Circle at (0.256738126,
                -0.537628233,
                0.633601069),
	 with pitch -0.0 deg,
	 with yaw -0.0 deg,
	 with roll -0.0 deg

circle3 = new Circle at (0.05147794,
                -0.537628233,
                0.7394304),
	 with pitch -0.0 deg,
	 with yaw -0.0 deg,
	 with roll -0.0 deg

circle4 = new Circle at (-0.173487455,
                -0.537628233,
                0.6151452),
	 with pitch -0.0 deg,
	 with yaw -0.0 deg,
	 with roll -0.0 deg

circle5 = new Circle at (-0.253140241,
                -0.537628233,
                0.430084854),
	 with pitch -0.0 deg,
	 with yaw -0.0 deg,
	 with roll -0.0 deg

ego = new Scenicavatar at (0, 0, 0), with behavior Instruction()