from scenic_writer import *
import json
import os
import requests

### Download the Instruction Json File
json_id = 1 
file_name = "exercise"+str(json_id)
download_and_print_json(file_name+".json")

current_dir = current_dir = os.getcwd()
parent_dir = os.path.dirname(current_dir)
json_name = f"user_study/instructions/{file_name}.json"
json_file_path = os.path.join(current_dir, json_name)
# print(json_file_path)

with open(os.path.join(current_dir, json_name), 'r') as file:
    annotations = json.load(file)
    model_file_path = os.path.join(
        parent_dir, "Scenic-main", "Scenic", "src", "scenic", "simulators", "unity", "model.scenic")
    api_file_path = os.path.join(
        parent_dir, "Scenic-main", "Scenic", "src", "scenic", "simulators", "unity", "actions.py")
    save_file_path = os.path.join(current_dir, "user_study", "scenic_programs", f"{file_name}" + ".scenic")
    example_scenic_programs_path = os.path.join(current_dir, "scenic_output", "example_scenic_program")
    log_file_path = os.path.join(current_dir, "user_study", "logs", f"{file_name}" + ".json")

    synth = Synth(annotations, 
                  model_file_path, 
                  api_file_path,
                  example_scenic_programs_path)
    program = synth.synthesize()

    with open(save_file_path, 'w') as scenic_file:
        scenic_file.write(program)

    print(f"Scenic program saved to {save_file_path}")