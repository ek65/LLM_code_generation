from api_key import client
import json
import openai
import time
import re


def queryLLM(system_prompt, user_prompt, temperature=0, model="gpt-4", json_bool=False, max_retries=3):
    retries = 0
    while retries < max_retries:
        try:
            params = {
                "model": model,
                "messages": [
                    {"role": "system", "content": system_prompt},
                    {"role": "user", "content": user_prompt}
                ],
                "temperature": temperature,
                "timeout": 30,  # Increase timeout
            }

            # Only include response_format for gpt-4o
            if model == "gpt-4o" and json_bool:
                params["response_format"] = {"type": "json_object"}

            chat = client.chat.completions.create(**params)
            output = chat.choices[0].message.content

            if json_bool:
                try:
                    return json.loads(output)
                except json.JSONDecodeError as e:
                    print("scenic_translator.py")
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
    in the provided library are 'Orange' and 'Basket', then you should output a json, {{'orange1': 'Orange', 'basket1':'Basket'}}.
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

    **Example 6 (Place object somewhere that doesn't have info)**
    instructions: ["1": "Place the pitcher to the table"]
    object list : [Pitcher]
    Since we do not know where the table is, we can just have buffer that will give 3 seconds for patient to do the task.
    {{ 0: "Buffer()"}}

    '''

    return queryLLM(system_prompt, user_prompt, json_bool=True)


def instruction_generator(functions, instruction_list):
    """
    Given a dictionary where the key is an integer and the value is a function call, 
    return the key that represents the index at which the instruction should play, 
    along with the corresponding value, which is the instruction.

    Input Arguments:
    1. functions (dicts):
        key (int) - index which represents order of function calls
        value (str) - function calls contains name of function as well as parameters
    2. instruction_list (list): a list of descriptions of metrics to monitor

    Return:
    Dictionary:
        key (int) - index at which the instruction should play
        value (str) - str which is a instruciton
    """

    system_prompt = f'''
    You are a helpful coding assistant. 
    You are given a Python dictionary where the keys represent the order (index) 
    in which specific function calls should be executed, and the values represent 
    functions that monitor particular physical conditions: {functions}.
    Your task is to return a JSON dictionary where each key represents the index at which 
    the instruction should be played, and the corresponding value is the instruction 
    that should be executed at that index. 
    Again, return just JSON without explanation or any text.
    '''

    user_prompt = f'''
    Here is the list of instructions used to create the function calls: {instruction_list}.
    Your output should be in the form of a JSON object containing a dictionary. 
    The dictionary should have keys representing the index at which the instruction should be played, 
    and values representing the instructions (in English) that will be executed. 
    The instructions should be phrased as if a clinician is speaking directly to a patient and there should be no special character
    like underscore.

    
    To avoid redundancy, ensure that no similar instruction is repeated more than twice. So, you do not need to map all functions with instructions.

    Here are examples:

    Example #1
    function calls: 
    {{
        "0": "EgoSeated(ego)",
        "1": "MoveRealObjectWithRightHand(ego, real_obj_placeholder1)",
        "2": "BringRealObjectToMouthWithRight(ego)",
        "3": "MoveRealObjectWithRightHand(ego, real_obj_placeholder1)",
        "4": "MoveRealObjectWithRightHand(ego, real_obj_placeholder2)",
        "5": "BringRealObjectToMouthWithRight(ego)",
        "6": "MoveRealObjectWithRightHand(ego, real_obj_placeholder2)",
        "7": "MoveRealObjectWithRightHand(ego, real_obj_placeholder3)",
        "8": "BringRealObjectToMouthWithRight(ego)",
        "9": "MoveRealObjectWithRightHand(ego, real_obj_placeholder3)",
        "10": "MoveRealObjectWithRightHand(ego, real_obj_placeholder4)",
        "11": "BringRealObjectToMouthWithRight(ego)",
        "12": "MoveRealObjectWithRightHand(ego, real_obj_placeholder4)",
    }}
    list of instructions:
    [
    "1. Sit at the table.", 
    "2. Place real object on the real_object_placeholder1 with your right hand", 
    "3. Bring the real object toward your mouth with your right hand.", 
    "4. Return the real object to its real object placeholder position with your right hand.", 
    "5. Repeat steps 2-4 for placeholder 2 to 5."
    ]
    Returned JSON should be: 
    {{
    "0": "Sit at the table.",
    "1": "Place real object on the first placeholder with your right hand",
    "2": "Bring the real object toward your mouth with your right hand.",
    "3": "Return the real object to its first placeholder position with your right hand.",
    "4": "Place real object on the second placeholder with your right hand",
    "5": "Bring the real object toward your mouth with your right hand.",
    "6": "Return the real object to its second placeholder position with your right hand.",
    "7": "Repeat the previous steps for the remaining exercises"
    }}

    Again, return just JSON without explanation or any text.
    '''
    return queryLLM(system_prompt, user_prompt, json_bool=True)


def program_synthesis(annotations):
    """
    given the therapist's annotations from the WebGL, this function synthesizes a Scenic program

    Input Arguments:
    annotations (dict): 

    """


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

    def __init__(self, annotations, save_file_path, model_file_path, api_file_path, log_file_path):
        if "ego" not in annotations["setup"]:
            annotations["setup"]["ego"] = [[0, 0, 0], [0, 0, 0]]
        self.obj_dict = {obj: {"position": obj_data[0], "rotation": obj_data[1]}
                         for obj, obj_data in annotations["setup"].items()}
        self.instruction_list = annotations['instructions']
        self.save_file_path = save_file_path
        self.model_file_path = model_file_path
        self.api_file_path = api_file_path
        self.log_file_path = log_file_path
        self.obj_list = list(self.obj_dict.keys())
        self.env_object_list = ["Shelf", "Box", "ego"]

    def synthesize(self):
        obj_scenic_dict = obj_model_finder(self.obj_list, self.model_file_path)
        print(f"obj_scenic_dict: {json.dumps(obj_scenic_dict, indent=4)}")
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
                logs[key] = [value, 
                             instruction_dict[key] if key in instruction_dict else "", 
                             False]
            json.dump(logs, file, indent=4)

        # Write Scenic file
        with open(self.save_file_path, "w") as file:

            if "ego" in obj_scenic_dict:
                del obj_scenic_dict["ego"]

            # import necessary library
            file.write("from scenic.simulators.unity.actions import *\n")
            file.write("from scenic.simulators.unity.behaviors import *\n")
            file.write("model scenic.simulators.unity.model\n\n")
            
            file.write(f"log_file_path = r\"{self.log_file_path}\"\n\n")

            # Write the behavior for Instructions:
            file.write(
                f"behavior Instruction({', '.join(obj_scenic_dict.keys())}):\n")
            file.write(f"\tlog = 0\n\n")

            # Hide all objects that are generated in the scene
            for object in self.obj_list:
                if re.sub(r'[0-9]', '', object) not in self.env_object_list:
                    file.write(f"\ttake HideAction('{object}')\n")
            file.write("\n")

            # Sort fucntion dict in Key order
            sorted_apis_dict = sorted(
                metric_api_dict.items(), key=lambda x: int(x[0]))

            # Initialize a set to keep track of objects that have been shown
            shown_objects = set()

            # loop through sorted dictionary
            for i, item in enumerate(sorted_apis_dict):
                key, api = item
                
                # Activate Shine, object is being used in the API, shine the object
                last_parameter = api.split(",")
                if (len(last_parameter) > 1):
                    file.write(
                        f"\ttake ActivateShineAction('{last_parameter[-1][1:-1]}')\n")
                    

                if key in instruction_dict:
                    # Instruction gets played from inst_dict using key
                    file.write(
                        f"\ttake SpeakAction('{instruction_dict[key]}')\n")

                # Add Hide and Show actions
                for obj in self.obj_list:
                    # if the object is not an environmental object
                    object_name = re.sub(r'[0-9]', '', obj)
                    if object_name not in self.env_object_list:
                        # If the object appears in the current API call and hasn't been shown yet, show it.
                        if obj in api:
                            if obj not in shown_objects:
                                file.write(f"\ttake ShowAction('{obj}')\n")
                                shown_objects.add(obj)
                        else:
                            # For objects already shown, check if they appear in any future API call.
                            future_apis = [sorted_apis_dict[k][1]
                                           for k in range(i + 1, len(sorted_apis_dict))]
                            if obj in shown_objects and not any(obj in future_api for future_api in future_apis):
                                file.write(f"\ttake HideAction('{obj}')\n")
                                shown_objects.remove(obj)


                # Done aciton must be executed at the end of the actions
                file.write(f"\ttake DoneAction()\n")

                # Adding a 2 second time buffer
                file.write(f"\tfor _ in range(30):\n")
                file.write(f"\t\twait\n\n")

                # write monitoring condition with while not.
                file.write(f"\twhile not {api}:\n")
                file.write(f"\t\twait\n\n")
                file.write(f"\tlog = LogAction(log, log_file_path)\n\n")
            file.write(f"\ttake DoneAction()\n")

            # Instantiate Objects in the Scenic Program
            for obj in obj_scenic_dict.keys():
                obj_data = self.obj_dict[obj]
                pos, rot = obj_data["position"], obj_data["rotation"]
                file.write(f"{obj} = new {obj_scenic_dict[obj]} at {pos},\n")
                file.write(f"\t with pitch {-float(rot[0])} deg,\n")
                file.write(f"\t with yaw {-float(rot[1])} deg,\n")
                file.write(f"\t with roll {-float(rot[2])} deg\n\n")
            file.write(f"\n\n")

            # Instantiate ego that calls Instruction behavior
            file.write(
                f"ego = new Scenicavatar at (0,0,0),\n\t with behavior Instruction({', '.join(obj_scenic_dict.keys())})\n")
