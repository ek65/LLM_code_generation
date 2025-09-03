from synth import queryLLM

def obj_model_finder(obj_list, file_path):
    """ 
    To instantiate objects in Scenic program, we need to identify which Scenic objects to reference 
    from model.scenic. 

    Inputs:
    1. obj_name (list): a list of string names of objects
    2. file_path (str): path to the model.scenic file

    Return:
    list: string names of the class object which represents the given object
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

    return queryLLM(system_prompt, user_prompt, json=True)



class Synth:
    def __init__(self, annotations, save_file_path):
        self.obj_pos_dict = annotations['objects']
        self.instruction_list = annotations['instructions']
        self.metric_list = annotations['metrics']
        self.save_file_path = save_file_path

    def synthesis(self, file_path):
        obj_list = [obj_name for obj_name in self.obj_pos_dict.keys()]
        scenic_obj_list = obj_model_finder(obj_list, file_path)

        # with open(self.save_file_path, "w") as file:
        #     for 
