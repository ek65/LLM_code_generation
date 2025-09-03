import time
import json

from scenic.simulators.unity.actions import *
from scenic.simulators.unity.behaviors import *

model scenic.simulators.unity.model

# Log file for this exercise
log_file_path = "program_synthesis/logs/remove_milk_bottle_cap.json"

# Instruction logs
logs = {
    0: {
        "ActionAPI": "",
        "Instruction": "Reach for the milk bottle.",
        "Time_Taken": 0,
        "Completeness": False
    },
    1: {
        "ActionAPI": "",
        "Instruction": "Stabilize it with your left hand and bring it towards you.",
        "Time_Taken": 0,
        "Completeness": False
    },
    2: {
        "ActionAPI": "",
        "Instruction": "Use your right hand to unscrew the cap on the milk bottle.",
        "Time_Taken": 0,
        "Completeness": False
    }
}

behavior Instruction():
    # Introduction / overview
    take SpeakAction('To reach for the milk bottle and independently remove cap with your right hand.')
    take SpeakAction('Improve fine motor planning with right hand.')
    take SpeakAction('If you feel pain, fatigue, or dizziness, please tell the system so that we can terminate the exercise for your safety.')
    take SpeakAction('Stabilize the bottle to avoid tipping the bottle.')
    take DoneAction()

    # Wait until all four intro lines have been spoken
    while WaitForIntroduction(ego):
        wait

    log_idx = 0

    # Snapshot before starting
    take SnapshotAction('Before', log_file_path)

    # Instruction 0: Reach for the bottle
    take SpeakAction('Reach for the milk bottle.')
    take DoneAction()
    while WaitForSpeakAction(ego, 1):
        wait
    start_time = time.time()
    take SendImageAndTextRequestAction("Reach for the milk bottle.")
    take DoneAction()
    while not TaskIsDone(ego):
        wait
    end_time = time.time()
    UpdateLogs(logs, log_idx, "SendImageAndTextRequestAction", end_time - start_time, True)
    with open(log_file_path, "w") as f:
        json.dump(logs, f, indent=4)

    # Instruction 1: Stabilize with left hand and bring it close
    log_idx += 1
    take SpeakAction('Stabilize it with your left hand and bring it towards you.')
    take DoneAction()
    while WaitForSpeakAction(ego, 2):
        wait
    start_time = time.time()
    take SendImageAndTextRequestAction("Stabilize it with your left hand and bring it towards you.")
    take DoneAction()
    while not TaskIsDone(ego):
        wait
    end_time = time.time()
    UpdateLogs(logs, log_idx, "SendImageAndTextRequestAction", end_time - start_time, True)
    with open(log_file_path, "w") as f:
        json.dump(logs, f, indent=4)

    # Instruction 2: Unscrew the cap with right hand
    log_idx += 1
    take SpeakAction('Use your right hand to unscrew the cap on the milk bottle.')
    take DoneAction()
    while WaitForSpeakAction(ego, 3):
        wait
    start_time = time.time()
    take SendImageAndTextRequestAction("Use your right hand to unscrew the cap on the milk bottle.")
    take DoneAction()
    while not TaskIsDone(ego):
        wait
    end_time = time.time()
    UpdateLogs(logs, log_idx, "SendImageAndTextRequestAction", end_time - start_time, True)
    with open(log_file_path, "w") as f:
        json.dump(logs, f, indent=4)

    # Final log entry and snapshot
    log = LogAction(ego, log_idx, log_file_path, end_time - start_time)
    take SnapshotAction('After', log_file_path)
    take DoneAction()

# Instantiate the avatar at origin with this behavior
ego = new Scenicavatar at (0, 0, 0), with behavior Instruction()