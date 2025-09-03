###################### Example 1 ######################

###################### Input Dictionary example ######################

{
    "precaution": "Maintain your balance. If you think you are tilting to the side, stabilize yourself. Make your standing stable by holding on to the kitchen table with your left hand. If you feel pain, fatigue, or dizziness, please tell the system so that we can terminate the exercise for your safety.", 
    "instruction": [
        "Place your right hand on the table in a neutral position.",
        "Place your right hand on top of a cup.",
        "Lean forward to grasp a bowl for three seconds.",
        "Then, place your hand back to the neutral position.",
        "Repeat these steps 5 times."
    ], 
    "setup": {
            "ego": [
                [
                    0,
                    0,
                    0
                ],
                [
                    0,
                    0,
                    0
                ]
            ]
        }
}

# Suppose you are given a dictionary above. Generate a Scenic program in the following structure. 
###################### Output Scenic file example ######################
import time
import json

from scenic.simulators.unity.actions import *
from scenic.simulators.unity.behaviors import *

model scenic.simulators.unity.model

#set log_file_path to "program_synthesis/logs/name_of_the_exercise.json"
log_file_path = "program_synthesis/logs/reaching_for_circle.json"

logs = {
    0 : {
        "ActionAPI" : "",
        "Instruction": "Place your right hand on the table in a neutral position.",
        "Time_Taken" : 0,
        "Completeness": False
        },
    1 : {
        "ActionAPI" : "",
        "Instruction": "Place your right hand on top of a cup.",
        "Time_Taken" : 0,
        "Completeness": False
        },
    2 :{
        "ActionAPI" : "",
        "Instruction": "Lean forward to grasp a bowl for three seconds.", 
        "Time_Taken" : 0,
        "Completeness": False
        },
    3 : {
        "ActionAPI" : "",
        "Instruction": "Then, place your hand back to the neutral position.", 
        "Time_Taken" : 0,
        "Completeness": False
        },
    4 : {
        "ActionAPI" : "",
        "Instruction": "Repeat these steps 5 times.",
        "Time_Taken" : 0,
        "Completeness": False
        }
}

log_file_path = "program_synthesis/logs/summary_name_of_the_exercise.json" 
# Create log json file 
with open(log_file_path, 'w') as f:
    json.dump({}, f, indent=4)

behavior Instruction():
    # ** You must always start with two speak actions: one for greeting and one for precaution.
    take SpeakAction("Let's start a new exercise.") # Always start with a greeting
    take SpeakAction("The precaution to take is to maintain your balance. If you think you are tilting to the side, stabilize yourself. Make your standing stable by holding on to the kitchen table with your left hand. Also, if you feel pain, fatigue, or dizziness, please tell the system so that we can terminate the exercise for your safety.")
    take DoneAction() # take DoneAction() should be invoked right before any loop (as below)

    while WaitForIntroduction(ego):
        wait

    # Log count integer, which gets incremented by 1 every time new log is recorded
    log_idx = 0
    speak_idx = 1

    # ---------- INSTRUCTION STEP 1 ----------
    take SpeakAction("Place your right hand on the table in a neutral position.")
    take DoneAction()# take DoneAction() should be invoked right before any loop (as below)

    while WaitForSpeakAction(ego, speak_idx): 
        wait
    speak_idx += 1 # increment speak_idx by 1 every time a new instruction is spoken

    start_time, count = time.time(), 0
    # Use SendImageAndTextRequestAction() rather than RecordVideoAndEvaluateAction() because
    # checking for a static hand position is enough. This does not require a video. 
    take SendImageAndTextRequestAction(f"Current Instruction: {logs[log_idx]['Instruction']}. Prior Instructions: {[logs[i]['Instruction'] for i in range(log_idx)]}")
    take DoneAction()
    while not RequestActionResult(ego):
        if count > 175:
            break
        count += 1
        wait

    end_time = time.time()
    UpdateLogs(logs, log_idx, "SendImageAndTextRequestAction", end_time - start_time, count < 175)
    with open(self.log_file_path, "w") as file:
        json.dump(logs, file, indent=4)
    log_idx += 1 # increment log_idx by 1 every time a new log is recorded
    # Whenever SendImageAndTextRequestAction() or RecordVideoandEvaulate() are called, 
    # you must call "take DisposeQueriesAction()" to flush out the queries to a vision language model.
    take DisposeQueriesAction()
    
    # ---------- Instruction Step 2 ----------
    take SpeakAction("Place your right hand on top of a cup.")
    take DoneAction() # take DoneAction() should be invoked right before any loop (as below)

    while WaitForSpeakAction(ego, speak_idx): 
        wait
    speak_idx += 1 # increment speak_idx by 1 every time a new instruction is spoken

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
    with open(self.log_file_path, "w") as file:
        json.dump(logs, file, indent=4)
    log_idx += 1 # increment log_idx by 1 every time a new log is recorded
    # Whenever SendImageAndTextRequestAction() or RecordVideoandEvaulate() are called, 
    # you must call "take DisposeQueriesAction()" to flush out the queries to a vision language model.
    take DisposeQueriesAction()

    # ---------- Instruction Step 3 ----------
    take SpeakAction("Lean forward to grasp a bowl for three seconds.")
    take DoneAction()

    while WaitForSpeakAction(ego, speak_idx): 
        wait
    speak_idx += 1 # increment speak_idx by 1 every time a new instruction is spoken

    start_time, count = time.time(), 0
    # In this case, RecordVideoAndEvaluateAction() should be invoked, not SendImageAndTextRequestAction() because
    # we are checking for a dynamic action (gliding the hand across the table), which requires a video.
    # Leaning forward should be checked using LeanForward() which accesses body pose estimation data that is more accurate than the video.
    # The rest of the instruction, involving the patient's interaction with the table should be checked using RecordVideoAndEvaluateAction().
    # Make sure to exclude the "leaning forward" condition from the instruction in RecordVideoAndEvaluateAction() below, 
    take RecordVideoAndEvaluateAction(f"Current Instruction: grasp a bowl for three seconds. Prior Instructions: {[logs[i]['Instruction'] for i in range(log_idx)]}")
    take DoneAction()

    # like below, you conjunct the two conditions to check if the hand is on top of a cup and if the hand is gliding across the table to a bowl.
    # *** when conjuncting with RequestActionResult(ego), make sure to state RequestActionResult(ego) at the end of the condition, so that the system can check if the action is completed.
    # for example, below, LeanForward(ego) is checked first, then RequestActionResult(ego) is checked at the end.
    while not (LeanForward(ego) and RequestActionResult(ego)):
        if count > 175:
            break
        count += 1
        wait

    end_time = time.time()

    UpdateLogs(logs, log_idx, "LeanForward+RecordVideoAndEvaluateAction", end_time - start_time, count < 175)
    with open(log_file_path, "w") as file:
        json.dump(logs, file, indent=4)
    log_idx += 1
    take DisposeQueriesAction() # whenever SendImageAndTextRequestAction() or RecordVideoandEvaulateAction() are called,
    # you must call "take DisposeQueriesAction()" to flush out the queries to a vision language model.

    # ---------- Instruction Step 4 ----------
    take SpeakAction("Then, place your hand back to the neutral position.")
    take DoneAction()

    while WaitForSpeakAction(ego, speak_idx): 
        wait
    speak_idx += 1 # increment speak_idx by 1 every time a new instruction is spoken

    start_time, count = time.time(), 0
    # In this case, you should invoke SendImageAndTextRequestAction() over RecordVideoAndEvaluateAction() because
    # whether a hand is back to the neutral position can be checked using a static image, not a video.
    take SendImageAndTextRequestAction(f"Current Instruction: {logs[log_idx]['Instruction']}. Prior Instructions: {[logs[i]['Instruction'] for i in range(log_idx)]}")
    take DoneAction()
    while not RequestActionResult(ego):
        if count > 175:
            break
        count += 1
        wait

    end_time = time.time()
    UpdateLogs(logs, log_idx, "SendImageAndTextRequestAction", end_time - start_time, count < 175)
    with open(self.log_file_path, "w") as file:
        json.dump(logs, file, indent=4)
    log_idx += 1 
    take DisposeQueriesAction()

    # ---------- Instruction Step 5 ----------
    take SpeakAction("Repeat these steps 5 times.")
    take DoneAction()

    while WaitForSpeakAction(ego, speak_idx): 
        wait
    speak_idx += 1 # increment speak_idx by 1 every time a new instruction is spoken

    rep_start_time = time.time()

    # To check if this step is completed, we need to check if ALL the steps in the following for-loop are completed. We will use the variable 'correctness' to check if all the steps are completed.
    correctness = True

    # ---------- Repeat Loop ---------- 
    # in the following loop, we are repeating the steps 1-4 for 5 times.
    # you should NOT log the steps within this loop, unlike the steps above.
    # you should log after the loop is completed, i.e. after the 5 repetitions are done.
    for i in range(5): # for loop repeats for the number of instructed repetitions.
        # it is important that you identify the 'scope' of repetitions (i.e. which steps you are repeating) then, you basically repeat the same steps as above within this for loop. 
        # within this loop, you should NOT log 
        
        #### Repeat Instruction Step 1 without logging
        take SpeakAction("Place your right hand on the table in a neutral position.")
        take DoneAction()

        while WaitForSpeakAction(ego, speak_idx): 
            wait
        speak_idx += 1 # increment speak_idx by 1 every time a new instruction is spoken

        count = 0 
        take SendImageAndTextRequestAction(f"Current Instruction: {logs[log_idx]['Instruction']}. Prior Instructions: {[logs[i]['Instruction'] for i in range(log_idx)]}")
        take DoneAction()
        while not RequestActionResult(ego):
            if count > 175:
                break
            count += 1
            wait
        take DisposeQueriesAction()

        correctness = correctness and (count < 175) # check if the step is completed
        
        #### Repeat Instruction Step 2 without logging
        take SpeakAction("Place your right hand on top of a cup.")
        take DoneAction()
        while WaitForSpeakAction(ego, speak_idx): 
            wait
        speak_idx += 1 # increment speak_idx by 1 every time a new instruction is spoken

        count = 0 
        take SendImageAndTextRequestAction(f"Current Instruction: {logs[log_idx]['Instruction']}. Prior Instructions: {[logs[i]['Instruction'] for i in range(log_idx)]}")
        take DoneAction()
        while not RequestActionResult(ego):
            if count > 175:
                break
            count += 1
            wait
        take DisposeQueriesAction()

        correctness = correctness and (count < 175) # check if the step is completed

        #### Repeat Instruction Step 3 without logging
        take SpeakAction("Lean forward to grasp a bowl for three seconds.")
        take DoneAction()
        while WaitForSpeakAction(ego, speak_idx): 
            wait
        speak_idx += 1 # increment speak_idx by 1 every time a new instruction is spoken

        take RecordVideoAndEvaluateAction(f"Current Instruction: grasp a bowl for three seconds. Prior Instructions: {[logs[i]['Instruction'] for i in range(log_idx)]}")
        take DoneAction()

        count = 0
        while not (LeanForward(ego) and RequestActionResult(ego)):
            if count > 175:
                break
            count += 1
            wait

        log_idx += 1
        take DisposeQueriesAction()
        
        correctness = correctness and (count < 175)

        #### Repeat Instruction Step 4 without logging
        take SpeakAction("Then, place your hand back to the neutral position.")
        take DoneAction()
        while WaitForSpeakAction(ego, speak_idx): 
            wait
        speak_idx += 1

        count = 0
        take SendImageAndTextRequestAction(f"Current Instruction: {logs[log_idx]['Instruction']}. Prior Instructions: {[logs[i]['Instruction'] for i in range(log_idx)]}")
        take DoneAction()
        while not RequestActionResult(ego):
            if count > 175:
                break
            count += 1
            wait
        take DisposeQueriesAction()

        correctness = correctness and (count < 175)

    rep_end_time = time.time()

    UpdateLogs(logs, log_idx, "Repeat Loop (5x)", rep_end_time - rep_start_time, correctness) # Here, correctness is True if all the steps are completed.
    with open(log_file_path, "w") as file:
        json.dump(logs, file, indent=4)
    log_idx += 1

    # ---------- Inform the Patient that the Exercise is Completed ----------
    take SpeakAction("Excellent! You completed this exercise.")
    take DoneAction()

    while WaitForSpeakAction(ego, speak_idx): 
        wait
    speak_idx += 1 # increment speak_idx by 1 every time a new instruction is spoken
    take DisposeQueriesAction() # call this at the end of the program to flush out any remaining queries, if exists. 

ego = new Scenicavatar at (0,0,0),
    with behavior Instruction() 

