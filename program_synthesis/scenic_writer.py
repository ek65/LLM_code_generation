from api_key import client
import json
import openai
import time
import requests
import re
import os
import api_key as key
from openai import OpenAI


def queryLLM(system_prompt, user_prompt, temperature=0, model="gpt-4", json_bool=False, max_retries=3):
    retries = 0
    while retries < max_retries:
        try:
            client = OpenAI(
            api_key= key.GROK,
            base_url="https://api.x.ai/v1",
            )
            
            chat = client.chat.completions.create(
            model="grok-3-beta",
            messages= [ 
            {
                "role": "system",
                "content": system_prompt
            },
            {
                "role": "user",
                "content": user_prompt
            }]
            )
            output = chat.choices[0].message.content

            if json_bool:
                try:
                    return json.loads(output)
                except json.JSONDecodeError as e:
                    print("scenic_writer.py")
                    print("Error decoding JSON response:", e)
                    # Print raw response for debugging
                    print("Raw output:", output)
                    return None
            return output

        except json.JSONDecodeError:
            print("Warning: JSON decoding failed, retrying...")
        except openai.OpenAIError as e:
            print(f"OpenAI API error: {e}, retrying...")
        except Exception as e:
            print(f"Unexpected error: {e}, retrying...")

        retries += 1
        time.sleep(2 ** retries)  # Exponential backoff

    raise RuntimeError("queryLLM failed after multiple retries")


def obj_model_finder(obj_list, file_path):
    """ 
    To instantiate objects in Scenic program, we need to identify which Scenic objects to reference 
    from model.scenic. 

    Inputs:
    1. obj_name (list): a list of string names of objects
    2. file_path (str): path to the model.scenic file

    Return:
    dictionary: (key: obj name, value: corresponding Python Object name as defined in the model.scenic)
    """

    # load in the scenic.model file as a string
    with open(file_path, "r") as file:
        scenic_models = file.read()

    system_prompt = f'''You are given a library of Python objects which semantically represent physical objects (e.g. orange, basket). 
    You will be given a name of physical object. Your task is to reference the library here: {scenic_models} and 
    return the name of the Python object that represents it. Make sure your output is case-sensitive. '''

    user_prompt = f'''Here is a list of object names: {obj_list}. Please output the corresponding python object name from the provided library.
    Your output should be a json, which consists of a dictionary whose key is a string name of the object in the obj_list 
    and its value is the string name of the corresponding Python class object name defined in the given library. 
    For example, if the given list of objects is ['orange1', 'basket1'], and the names of the corresponding python class object 
    in the provided library are 'Orange' and 'Basket', then you should output a json, {{'orange1': 'Orange', 'basket1':'Basket', 'ego': \"Scenicavatar\"}}.
    '''

    return queryLLM(system_prompt, user_prompt, json_bool=True)


def api_retriever(instruction_list, object_list, file_path):
    """ 
    Given a description of the metrics to monitor, return the corresponding APIs needed to monitor each metric.

    Inputs:
    1. instruction_list (list): a list of descriptions of metrics to monitor
    2. object_list (list): a list contains names of objects
    2. file_path (str): path to the python script with the library of APIs

    Return:
    dictionary:
        1) key (int): the index of the description in the given list of metrics descriptions
        2) val (str): corresponding API to monitor the metric with input args filled out (e.g. ObjectGrasped(sponge1))
    """
    # load in the scenic.model file as a string
    with open(file_path, "r") as file:
        apis = file.read()

    system_prompt = f'''
    You are a helpful coding assistant. 
    You are given a library of Python APIs which can be used to monitor particular physical conditions: {apis}.
    Your task is to analyze a list of instructions and return a JSON dictionary mapping each relevant description index
    to the appropriate API call with input arguments filled in. 
    **Output ONLY the JSON dictionary in valid JSON format (use double quotes for keys and string values), with no additional text, explanations, or comments.**
    '''

    user_prompt = f'''
    Here is the list of descriptions of steps of exercise: {instruction_list}. 
    Here are the list of objects that will be present in the scene: {object_list}.
    
    Each description may contain Python variable names corresponding to objects in the scene.  
    Your task is to generate a JSON dictionary where:  
    - The key is the integer index of the step that requires evaluation.  
    - The value is the appropriate API call to monitor the metric, with all required input arguments filled in.  

    Refer to the provided Python API library when determining the appropriate API.  
    Note that the number of steps does not necessarily match the number of API calls in the dictionary.  

    **Example 1 (Direct API Mapping):**  
    instructions:["1. Grab apple1"]
    Upon referring to the API library, if the corresponding API is `"ObjectGrasped(obj)"`, which requires an input argument `"obj"`,  
    then the output should be following json: 
    {{0: "ObjectGrasped(apple1)"}}. 

    **Example 2 (Handling Real-World Object Instructions)**
    instructions:["1. Place real object on the real_object_placeholder1 with the right hand.", 
    "2. Move the real object toward the target."]
    Since we cannot track the real object's position, we instead track the right hand's position between real_object_placeholder1 and target.
    Then you should output following json: 
    {{0: "MoveRealObjectWithRightHand(ego, real_object_placeholder1)", 
    1: "MoveRealObjectWithRightHand(ego, target)"}}

    **Example 3 (Handling distance checking)**
    instructions:["1. Bring obj1 and obj2 together", "2. Walk towards target"]
    This can be checked using either distance checking API or object collision API. 
    Then the output should be following json:
    {{0: "CheckDistance(obj1, obj2)",
    1: "CheckDistance(ego, target)"}}

    **Example 4 (Repeating with advanced task)**
    instructions: ["1. Move obj1 to the target1", "2. repeat 1 for 3 times with obj 2 to 4"]
    This requires to call the same API multiple times with different object as well as hiding and showing previous task.
    Then the output should be following json:
    {{
        0: "CheckDistance(obj1, target1)",
        1: "CheckDistance(obj2, target1)",
        2: "CheckDistance(obj3, target1)",
        3: "CheckDistance(obj4, target1)"
    }}

    **Example 5 (Walk towards or Move the ego itself)**
    instructions: ["1. Move to the target1", "2. Walk towards the target2"]
    This can be checked using AvatarMoveOrWalkTowardsSomething API that checks distance between ego and the object.
    Then the output should be following json:
    {{ 0: "AvatarMoveOrWalkTowardsSomething(ego, target1)",
    1: "AvatarMoveOrWalkTowardsSomething(ego, target2)"
    }}
    
    **Example 5 (objects that are not in object list)**
    It means that we are using phyiscal object, so we need to take a picture and send to openai for evaluation. For this
    we can use "SendImageAndTextRequestAction"
    instructions: ["1. "Open the lid of the milk bottle", 
        "2.Close the lid of the milk bottle",
        "3.Repeat open and closing the milk bottle for three more times.""]
    Then the output should be following json:
    {{ 0: "OpenLid(ego, target1)",
    1: "AvatarMoveOrWalkTowardsSomething(ego, target2)"
    }}
    

    **Example 6 (Place object somewhere that doesn't have info)**
    instructions: ["1": "Place the pitcher to the table"]
    object list : [Pitcher]
    Since we do not know where the table is, we can just have buffer that will give 3 seconds for patient to do the task.
    {{ 0: "Buffer()"}}

     The dictionary must contain all numbers in the range, starting from the first key to the last key, without skipping any numbers.
    '''

    return queryLLM(system_prompt, user_prompt, json_bool=True)


def instruction_generator(instruction_list):
    """
    Given a dictionary where the key is an integer and the value is a function call, 
    return the key that represents the index at which the instruction should play, 
    along with the corresponding value, which is the instruction.

    Input Arguments:
    1. instruction_list (list): a list of descriptions of metrics to monitor

    Return:
    Dictionary:
        key (int) - index at which the instruction should play
        value (str) - str which is a instruciton
    """

    system_prompt = f'''
    You are a helpful coding assistant. 
    Your task is to return a JSON dictionary where each key represents the index at which 
    the instruction should be played, and the corresponding value is the instruction 
    that should be executed at that index.
     
    Again, return just JSON without explanation or any text.
    '''

    user_prompt = f'''
    Here is the list of instructions that was generated by clinician: {instruction_list}.
    Your output should be in the form of a JSON object containing a dictionary. 
    The dictionary should have keys representing the index at which the instruction should be played, 
    and values representing the instructions (in English) that will be executed. 
    The instructions should be phrased as if a clinician is speaking directly to a patient and there should be no special character
    like underscore.

    Again, return just JSON without explanation or any text.
    '''
    return queryLLM(system_prompt, user_prompt, json_bool=True)


def direct_scenic_generator(json_file, actions_path, scenic_example_files, model_file_path):
    """
    Prompts an LLM to generate a Scenic program from annotations and therapist's instructions. 

    Inputs:
    1. transcript (str): The transcript of therapist which includes verbal instructions
    2. actions_path (str): path to the python script with the library of APIs
    3. scenic_examples_path (str): path to the scenic examples
    """

    with open(actions_path, "r") as file:
        apis = file.read()

    file_contents = []

    for file_path in scenic_example_files:
        with open(file_path, 'r') as file:
            content = file.read()
            file_contents.append(content)

    models = None
    with open(model_file_path, "r") as file:
        models = file.read()

    system_prompt = f'''
    You are a helpful coding assistant with knowledge in physical and occupational therapy. 
    Your overall task is to output a program that can (a) instruct exercise, (b) monitor patient movement, and (c) log the patient's performance. 
    You need to program using a domain-specific programming language called Scenic whose syntax and semantics are embedded in Python. 
    
    You are given:\n
    1) A json containing a transcript of instructions from a physical or occupational therapist to a patient regarding a personalized physical
    rehabilitation exercise. \n
    2) URLs to tutorials on Scenic programming language with which you need to generate the output program, \n
    3) A library of APIs which can be used to instruct, monitor, and log information about patient's exercises. \n
    4) Examples of Scenic programs whose structure you need to reference when programming,\n
    
    *** Context of the Program Use: \n
    Note that the Scenic program you output will be executed on an augmented reality (AR) headset. 
    The patient will be wearing the headset as your program instruct, monitor, and log the patient's exercises. 
    The headset has a built-in body pose estimation (BPE) model which tracks the patient's joint positions, e.g. finger, wrist, elbow positions,
    Also, the headset provides joint angle information, finger, wrist, elbow flexion/extension angles. 
    The headset also has an embedded camera which the provided two APIs, i.e. 'SendImageAndTextRequestAction' or 'RecordVideoAndEvaluateAction',
    can access to capture an image or a video and query a vision language model to monitor whether an instruction is followed. 
    Since the camera is on the headset, facing away from the patient, the camera only partially captures the patient's body, namely
    hand, wrist, and forearm, but may likely not capture elbows, shoulders, and upper torso. 

    *** Regarding APIs for Monitoring Patient's Exercise:
    The provided APIs related to monitoring utilize either the BPE or the camera data from the AR headset. 
    The APIs accessing the BPE information, or 'BPE APIs' for brevity, are much more accurate in monitoring 'static' conditions related to
    joint angles and positions than the APIs accessing the camera data, or 'visual APIs' for brevity. Note that the BPE APIs
    do not reason about a history of BPE information. Therefore, you should only use visual APIs to monitor conditions that 
    cannot be checked by the BPE APIs.

    Meanwhile, the BPE APIs cannot reason about the patient's interaction with any objects in the environment, e.g. table, cup.
    Thus, you should select the APIs with this context in mind to maximize the correctness of the program's monitoring.
    Make sure you 'separate' the conditions in each instruction step according to the strengths of each of the APIs you invoke.
    For example, suppose the instruction to monitor is 'extend your right arm to grasp a pen.' 
    Then, the most optimal way is to use SendImageAndTextRequestAction('grasp a pen with the right hand') 
    to check the condition on the patient's interaction with the pen, 
    and use 'CheckElbowExtension()' API to check right arm extension. The conjunction of these two conditions 
    should be monitored for the best monitoring outcome. *** It is important that you remove the elbow extension condition from the instruction
    to SendImageAndTextRequestAction() to only check conditions that BPE APIs cannot monitor to maximize monitoring correctness. 
    Otherwise, the conjunction of the two conditions may result in a sub-optimal monitoring outcome. \n
    
    Note that there are two types of visual APIs: 'SendImageAndTextRequestAction' or 'RecordVideoAndEvaluateAction.'
    One captures an image while the other captures a video and query a vision language model to monitor an instruction. 
    You will need to select which visual API to use based on the condition you need to monitor. 
    If the instruction cannot be checked with a single image, e.g. \n
    Example 1: 'move your hand in a circular motion' where you need to observe a sequence of images to determine the shape of the motion \n
    Example 2: 'hold your hand in the air for 5 seconds' where you should observe for a duration of time\n
    then use the 'RecordVideoAndEvaluateAction'; otherwise, by default, use 'SendImageAndTextRequestAction.' \n
    *** Try to use the 'SendImageAndTextRequestAction' API as much as possible, since it is 
    more efficient and less resource-intensive than the 'RecordVideoAndEvaluateAction' API.
    *** It is important to note that the objective of monitoring is to check whether the patient "completed" each instruction step. 
    This means, often times, you simply want to check the "end state" of an instruction to check its completion. 
    Example 1: if the instruction states 'pick up a pen,' then you just need a snapshot of an image of a hand holding a pen in the air. 
    Example 2: if the instruction states 'open your bag,' then you just need a snapshot of an image of an open bag.
    Therefore, many of the conditions can be monitored using 'SendImageAndTextRequestAction.'

    *** When you generate the Scenic program:\n
    1. You should use the exact same instructions as provided in the json. Do not change the wording or phrasing of the instructions.
       And, use the same order of instructions as in the json.\n
    2. It is very important that, whenever the therapist instructs the patient to 'repeat' certain steps, 
       you should identify the scope of 'which' steps are to be repeated and convert that instruction into a for-loop in the Scenic program. 
       And, within the for-loop, make sure you instruct each step again as shown in the example Scenic programs. \n
    
    Please reference the provided examples of Scenic programs to understand how to instruct, monitor, and log the patient's performance.
    '''
    scene_tutorial = "https://docs.scenic-lang.org/en/latest/tutorials/fundamentals.html"
    behavior_tutorial = "https://docs.scenic-lang.org/en/latest/tutorials/dynamics.html"

    user_prompt = f'''
    Please return a Scenic program after referencing the following: \n\n
    1. Here is the transcript of step-by-step instructions on a physical rehabilitation exercise: {json_file}, \n
    2. URL to a tutorial on setting up a static environment in Scenic: {scene_tutorial}. 
    It is very important to specify a property (e.g. pitch, yaw, roll) of an object using `with` syntax! \n 
    3. URL to a tutorial on constructing a behavior in Scenic: {behavior_tutorial}, \n
    4. A library of APIs: {apis}, \n
    5. Examples of Scenic programs: {file_contents}, \n

    You should return a Scenic program in string, without any additional text or comments. 
    Do not include any header like ```python. 
    Just return the Scenic program as a string such that it can be directly written to a file and be executed.
    '''

    return queryLLM(system_prompt, user_prompt)


def instruction_transcript_generator(exercise_title, example_json_path):
    examples = [os.path.join(example_json_path, f)
                for f in os.listdir(example_json_path)
                if os.path.isfile(os.path.join(example_json_path, f))]
    example_json_files = []

    for file_name in examples:
        with open(file_name, 'r') as file:
            content = file.read()
            example_json_files.append(content)

    patient_deficit = '''
    The patient presents with mild right upper extremity weakness.
    Active range of motion of right side is restricted due to the application of an elbow cast and a thumb spica splint.

    Range of Motion (Right Side):
    Elbow Flexion: 80–120 degrees (AROM: 40°)
    Wrist Flexion/Extension: 0°
    Thumb Flexion/Extension: 0°

    Muscle Strength (Manual Muscle Testing of Right Side):
    Shoulder: 3/5
    Elbow: 3/5
    Wrist: 3/5
    Fingers: 3/5
    '''
    system_prompt = f'''
    You are an expert physical and occupational therapist specializing in stroke rehabilitation and exercise design.
    Your task is to generate clear, safe, and realistic instruction transcripts for rehabilitation exercises,
    based on prior examples provided by therapists and patient needs.

    You are given:
    1. a description of our standardized patient's physical deficit: {patient_deficit}
    2. Example transcript output: {example_json_files}
    3. Exercise title (will be provided by the user)

    Your output should follow the same JSON format given in the examples.
    Output only the JSON, WITHOUT any explaination or comments.
    '''

    user_prompt = f'''
    Here is the exercise title you will be generating instruction JSON with: {exercise_title}
    '''
    return queryLLM(system_prompt, user_prompt, model='o4-mini')


def escape_quotes(text):
    return text.replace("'", "\\'").replace('"', '\\"')


class Synth:
    """
    The class object's functions are used:
    1) to parse the given the therapist's annotations from the WebGL
    2) synthesize a Scenic program

    Input Arguments:
    1. annotations from the WebGL
        We assume that the annotations are given as a nested dictionary of the following form:
        {
            "setup" (list) : dictionary of the form {"obj name": coordinate} ,
            "instruction" (str) : a list of descriptions of instructions 
                                    (the indices of this list represents the order of instructions)
            "monitor" (str) : a list of physical conditions to monitor for each instruction
                                (the indices of this list correspond to each description in the instruction list)
        }

    2. save_file_path (str): the path to save the synthesized Scenic program
    3. model_file_path (str): the path to model.scenic
    4. api_file_path (str): the path to python script with a library of APIs
    """

    def __init__(self, annotations, model_file_path, api_file_path, example_scenic_programs_path):
        if "ego" not in annotations["setup"]:
            annotations["setup"]["ego"] = [[0, 0, 0], [0, 0, 0]]
        self.annotations = annotations
        self.others = {}
        for key in annotations:
            if key == "setup":
                self.obj_dict = {obj: {"position": obj_data[0], "rotation": obj_data[1]}
                                 for obj, obj_data in annotations["setup"].items()}
            elif key == 'instruction':
                self.instruction_list = annotations['instruction']
            else:
                self.others[key] = annotations[key]
        # self.save_file_path = save_file_path
        self.model_file_path = model_file_path
        self.api_file_path = api_file_path
        # self.log_file_path = log_file_path
        self.obj_list = list(self.obj_dict.keys())
        self.env_object_list = ["Shelf", "Box", "ego"]
        self.scenic_files = [
            os.path.join(example_scenic_programs_path, f)
            for f in os.listdir(example_scenic_programs_path)
            if os.path.isfile(os.path.join(example_scenic_programs_path, f))
        ]

    def synthesize(self):
        # # write scenic program
        program = direct_scenic_generator(
            self.annotations, self.api_file_path, self.scenic_files, self.model_file_path)
        # print(program)
        return program
        obj_scenic_dict = obj_model_finder(self.obj_list, self.model_file_path)
        print(f"obj_scenic_dict: {json.dumps(obj_scenic_dict, indent=4)}")
        if obj_scenic_dict is None:
            obj_scenic_dict = {}
        metric_api_dict = api_retriever(
            self.instruction_list, self.obj_list, self.api_file_path)
        print(f"metric_api_dict: {json.dumps(metric_api_dict, indent=4)}")
        instruction_dict = instruction_generator(
            metric_api_dict, self.instruction_list)
        print(f"instruction_dict: {json.dumps(instruction_dict, indent=4)}")

        # Write log file
        logs = {}
        with open(self.log_file_path, "w") as file:
            for key, value in metric_api_dict.items():
                '''
                Dictionary content:
                - action API
                - instruction
                - task Completion 
                - Time taken
                - unique image id
                '''
                logs[key] = [value,
                             instruction_dict[key],
                             False,
                             0.0,
                             ""]
            # Write them into json file (goal, precautions, purpose)
            logs.update(self.others)
            json.dump(logs, file, indent=4)

        # Write Scenic file
        with open(self.save_file_path, "w") as file:

            if obj_scenic_dict is not None and "ego" in obj_scenic_dict:
                del obj_scenic_dict["ego"]
                if obj_scenic_dict is None:
                    obj_scenic_dict = {}

            # import necessary library
            file.write("from scenic.simulators.unity.actions import *\n")
            file.write("from scenic.simulators.unity.behaviors import *\n")
            file.write("model scenic.simulators.unity.model\n\n")

            file.write(f"log_file_path = r\"{self.log_file_path}\"\n\n")

            # Write the behavior for Instructions
            file.write(
                f"behavior Instruction({', '.join(obj_scenic_dict.keys())}):\n")

            file.write(f"\ttry:\n\n")

            file.write(
                f"\t\ttake SpeakAction('The goal of this exercise is {escape_quotes(self.others['goal'])}')\n")
            file.write(
                f"\t\ttake SpeakAction('Purpose is {escape_quotes(self.others['purpose'])}.')\n")
            file.write(
                f"\t\ttake SpeakAction('Physical precaution is {escape_quotes(self.others['physicalPrecaution'])}.')\n")
            file.write(
                f"\t\ttake SpeakAction('Environmental precaution is {escape_quotes(self.others['environmentalPrecaution'])}.')\n")
            file.write("\t\ttake DoneAction()\n\n")
            file.write(f"\t\twhile not WaitForIntroduction(ego):\n")
            file.write(f"\t\t\twait\n\n")

            file.write(f"\t\tlog = 0\n\n")

            # Take "Before" Snapshot
            file.write("\t\ttake SnapshotAction('Before', log_file_path)\n\n")

            # # Hide all objects that are generated in the scene
            # for object in self.obj_list:
            #     if re.sub(r'[0-9]', '', object) not in self.env_object_list:
            #         file.write(f"\t\ttake HideAction('{object}')\n")
            # file.write("\n")

            # Sort fucntion dict in Key order
            sorted_apis_dict = sorted(
                metric_api_dict.items(), key=lambda x: int(x[0]))

            # Initialize a set to keep track of objects that have been shown
            shown_objects = set()

            # loop through sorted dictionary
            for i, item in enumerate(sorted_apis_dict):
                key, api = item

                if key in instruction_dict:
                    # Instruction gets played from inst_dict using key
                    file.write(
                        f"\t\ttake SpeakAction('{escape_quotes(instruction_dict[key])}')\n")

                # Add Hide and Show actions
                # for obj in self.obj_list:
                #     # if the object is not an environmental object
                #     object_name = re.sub(r'[0-9]', '', obj)
                #     if object_name not in self.env_object_list:
                #         # If the object appears in the current API call and hasn't been shown yet, show it.
                #         if obj in api:
                #             if obj not in shown_objects:
                #                 file.write(f"\t\ttake ShowAction('{obj}')\n")
                #                 shown_objects.add(obj)
                #         else:
                #             # For objects already shown, check if they appear in any future API call.
                #             future_apis = [sorted_apis_dict[k][1]
                #                            for k in range(i + 1, len(sorted_apis_dict))]
                #             if obj in shown_objects and not any(obj in future_api for future_api in future_apis):
                #                 file.write(f"\t\ttake HideAction('{obj}')\n")
                #                 shown_objects.remove(obj)

                # Done aciton must be executed at the end of the actions
                file.write(f"\t\ttake DoneAction()\n")

                # Adding a 4 second time buffer
                file.write(f"\t\tfor _ in range(40):\n")
                file.write(f"\t\t\twait\n\n")

                # Recording time
                file.write(f"\t\tstart_time = time.time()\n\n")

                # # write monitoring condition with while not.a
                # file.write(f"\t\twhile not {api}:\n")
                # file.write(f"\t\t\twait\n\n")

                file.write(
                    f"\t\ttake SendImageAndTextRequestAction(\"{escape_quotes(instruction_dict[key])}\")\n")
                file.write(f"\t\ttake DoneAction()\n\n")

                file.write(f"\t\twhile not TaskIsDone(ego):\n")
                file.write(f"\t\t\twait\n")

                file.write(f"\t\tend_time = time.time()\n\n")
                file.write(
                    f"\t\tlog = LogAction(ego, log, log_file_path, end_time - start_time)\n\n")

            # End ofthe exerices
            file.write("\t\ttake SnapshotAction('After', log_file_path)\n")
            file.write(f"\t\ttake DoneAction()\n\n")

            file.write(f"\t\ttake AskQuestionAction(\"Did you feel any physical pain or discomfort during or after the exercise? For example, things like pain, fatigue, dizziness, or anything unusual?\")\n")
            file.write(f"\t\ttake DoneAction()\n\n")

            file.write(f"\t\twhile not PainRecorded(ego):\n")
            file.write(f"\t\t\twait\n\n")

            file.write(f"\t\tLogPain(ego, log_file_path)\n")
            file.write(f"\t\ttake DoneAction()\n\n")

            # Interrupt when the pain is detected
            file.write(f"\tinterrupt when DetectedPain(ego):\n")
            # Take a snapshot since the patient failed
            file.write("\t\ttake SnapshotAction('After', log_file_path)\n")
            file.write(f"\t\ttake AskQuestionAction(\"Did you feel any physical pain or discomfort during or after the exercise? For example, things like pain, fatigue, dizziness, or anything unusual?\")\n")
            file.write(f"\t\ttake DoneAction()\n\n")

            file.write(f"\t\twhile not PainRecorded(ego):\n")
            file.write(f"\t\t\twait\n\n")

            file.write(f"\t\tLogPain(ego, log_file_path)\n")
            file.write(f"\t\ttake DoneAction()\n\n")

            if obj_scenic_dict is not None:
                # Instantiate Objects in the Scenic Program
                for obj in obj_scenic_dict.keys():
                    obj_data = self.obj_dict[obj]
                    pos, rot = obj_data["position"], obj_data["rotation"]
                    file.write(
                        f"{obj} = new {obj_scenic_dict[obj]} at {pos},\n")
                    file.write(f"\t with pitch {-float(rot[0])} deg,\n")
                    file.write(f"\t with yaw {-float(rot[1])} deg,\n")
                    file.write(f"\t with roll {-float(rot[2])} deg\n\n")
            file.write(f"\n\n")

            # Instantiate ego that calls Instruction behavior
            file.write(
                f"ego = new Scenicavatar at (0,0,0),\n\t with behavior Instruction({', '.join(obj_scenic_dict.keys())})\n")

def download_and_print_json(json_id):
    save_path = f"user_study/instructions/{json_id}"
    url = f"https://caduceus-test-754616842718.us-west1.run.app/download/file/{json_id}/"
    headers = {
        "Accept": "application/json"
    }

    response = requests.get(url, headers=headers)
    text = response.text
    text = text.replace("\"", "")
    
    if response.status_code == 200:
        with open(save_path, "w") as f:
            json.dump(response.text, f, indent=4)
            
        print(f"DOWNLOAD SUCCESS: {json_id} saved to {save_path}")

    else:
        assert False, f"Failed to download JSON. Status code: {response.status_code}"

    return response.text