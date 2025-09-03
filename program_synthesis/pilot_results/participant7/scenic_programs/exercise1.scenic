import time
import json

from scenic.simulators.unity.actions import *
from scenic.simulators.unity.behaviors import *

model scenic.simulators.unity.model

log_file_path = "program_synthesis/logs/slide_right_hand.json"

# Create log json file 
with open(log_file_path, 'w') as f:
    json.dump({}, f, indent=4)

# Instruction logs
logs = {
    0: {
        "ActionAPI": "",
        "Instruction": "Sit upright at the table with your right arm resting comfortably on the surface.",
        "Time_Taken": 0,
        "Completeness": False
    },
    1: {
        "ActionAPI": "",
        "Instruction": "Place your right hand palm-down or neutral on the table.",
        "Time_Taken": 0,
        "Completeness": False
    },
    2: {
        "ActionAPI": "",
        "Instruction": "Slowly slide your right hand forward, straightening your elbow as much as is comfortable.",
        "Time_Taken": 0,
        "Completeness": False
    },
    3: {
        "ActionAPI": "",
        "Instruction": "Pause briefly",
        "Time_Taken": 0,
        "Completeness": False
    },
    4: {
        "ActionAPI": "",
        "Instruction": "Then slide your hand back toward your body, bending the elbow.",
        "Time_Taken": 0,
        "Completeness": False
    },
    5: {
        "ActionAPI": "",
        "Instruction": "Repeat the forward and backward movement 3-5 times in a smooth, controlled manner.",
        "Time_Taken": 0,
        "Completeness": False
    },
    6: {
        "ActionAPI": "",
        "Instruction": "Rest your right arm.",
        "Time_Taken": 0,
        "Completeness": False
    }
}

behavior Instruction():
    try:
        take SpeakAction("Let's start a new exercise.")
        take SpeakAction("Make sure there is no object on the table that you can knock over.")
        take DoneAction()

        while WaitForIntroduction(ego):
            wait

        log_idx = 0
        speak_idx = 1

        # ---------- Instruction 0 ----------
        take SpeakAction("Sit upright at the table with your right arm resting comfortably on the surface.")
        take DoneAction()

        while WaitForSpeakAction(ego, speak_idx): 
            wait
        speak_idx += 1 

        start_time, count = time.time(), 0
        take SendImageAndTextRequestAction(f"Current Instruction: Sit at the table with your right arm resting on the surface. Prior Instructions: {[logs[i]['Instruction'] for i in range(log_idx)]}")
        take DoneAction()
        while not (RequestActionResult(ego) and SitUpStraight(ego)):
            if count > 150:
                break
            count += 1
            wait
        end_time = time.time()
        UpdateLogs(logs, log_idx, "SendImageAndTextRequestAction+SitUpStraight", end_time - start_time, count < 150)
        with open(log_file_path, "w") as file:
            json.dump(logs, file, indent=4)
        log_idx += 1
        take DisposeQueriesAction()

        # ---------- Instruction 1 ----------
        take SpeakAction("Place your right hand palm-down or neutral on the table.")
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
        take SpeakAction("Slowly slide your right hand forward, straightening your elbow as much as is comfortable.")
        take DoneAction()

        while WaitForSpeakAction(ego, speak_idx): 
            wait
        speak_idx += 1 

        start_time, count = time.time(), 0
        take RecordVideoAndEvaluateAction(f"Current Instruction: {logs[log_idx]['Instruction']}. Prior Instructions: {[logs[i]['Instruction'] for i in range(log_idx)]}")
        take DoneAction()
        while not RequestActionResult(ego):
            if count > 150:
                break
            count += 1
            wait
        end_time = time.time()
        UpdateLogs(logs, log_idx, "RecordVideoAndEvaluateAction", end_time - start_time, count < 150)
        with open(log_file_path, "w") as file:
            json.dump(logs, file, indent=4)
        log_idx += 1
        take DisposeQueriesAction()

        # ---------- Instruction 3 ----------
        take SpeakAction("Pause briefly")
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

        # ---------- Instruction 4 ----------
        take SpeakAction("Then slide your hand back toward your body, bending the elbow.")
        take DoneAction()

        while WaitForSpeakAction(ego, speak_idx): 
            wait
        speak_idx += 1 

        start_time, count = time.time(), 0
        take RecordVideoAndEvaluateAction(f"Current Instruction: {logs[log_idx]['Instruction']}. Prior Instructions: {[logs[i]['Instruction'] for i in range(log_idx)]}")
        take DoneAction()
        while not RequestActionResult(ego):
            if count > 150:
                break
            count += 1
            wait
        end_time = time.time()
        UpdateLogs(logs, log_idx, "RecordVideoAndEvaluateAction", end_time - start_time, count < 150)
        with open(log_file_path, "w") as file:
            json.dump(logs, file, indent=4)
        log_idx += 1
        take DisposeQueriesAction()

        # ---------- Instruction 5 (Repeat Loop) ----------
        take SpeakAction("Repeat the forward and backward movement 3-5 times in a smooth, controlled manner.")
        take DoneAction()

        while WaitForSpeakAction(ego, speak_idx): 
            wait
        speak_idx += 1 

        rep_start_time = time.time()
        correctness = True

        for i in range(4):  # Assuming an average of 4 repetitions (between 3-5)
            # Repeat Instruction 2 without logging
            take SpeakAction("Slowly slide your right hand forward, straightening your elbow as much as is comfortable.")
            take DoneAction()

            while WaitForSpeakAction(ego, speak_idx): 
                wait
            speak_idx += 1 

            count = 0
            take RecordVideoAndEvaluateAction(f"Current Instruction: Slowly slide your right hand forward, straightening your elbow as much as is comfortable. Prior Instructions: {[logs[i]['Instruction'] for i in range(log_idx)]}")
            take DoneAction()
            while not RequestActionResult(ego):
                if count > 150:
                    break
                count += 1
                wait
            take DisposeQueriesAction()
            correctness = correctness and (count < 150)

            # Repeat Instruction 3 without logging
            take SpeakAction("Pause briefly")
            take DoneAction()

            while WaitForSpeakAction(ego, speak_idx): 
                wait
            speak_idx += 1 

            count = 0
            take SendImageAndTextRequestAction(f"Current Instruction: Pause briefly. Prior Instructions: {[logs[i]['Instruction'] for i in range(log_idx)]}")
            take DoneAction()
            while not RequestActionResult(ego):
                if count > 150:
                    break
                count += 1
                wait
            take DisposeQueriesAction()
            correctness = correctness and (count < 150)

            # Repeat Instruction 4 without logging
            take SpeakAction("Then slide your hand back toward your body, bending the elbow.")
            take DoneAction()

            while WaitForSpeakAction(ego, speak_idx): 
                wait
            speak_idx += 1 

            count = 0
            take RecordVideoAndEvaluateAction(f"Current Instruction: Then slide your hand back toward your body, bending the elbow. Prior Instructions: {[logs[i]['Instruction'] for i in range(log_idx)]}")
            take DoneAction()
            while not RequestActionResult(ego):
                if count > 150:
                    break
                count += 1
                wait
            take DisposeQueriesAction()
            correctness = correctness and (count < 150)

        rep_end_time = time.time()
        UpdateLogs(logs, log_idx, "Repeat Loop (4x)", rep_end_time - rep_start_time, correctness)
        with open(log_file_path, "w") as file:
            json.dump(logs, file, indent=4)
        log_idx += 1

        # ---------- Instruction 6 ----------
        take SpeakAction("Rest your right arm.")
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