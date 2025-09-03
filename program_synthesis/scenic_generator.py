import json
import re         
import requests 
from scenic_writer import *
from summarize import *

def summarize_logs(logs_dict):
    summary = {
        "Exercise Step":           [],
        "Successful Instructions": [],
        "Failed Instructions":     [],
        "Omitted Instructions":    [],
        "Duration of Completion":  []
    }
    
    num_key_pattern = re.compile(r"^\d+$")  
    
    for k in sorted(
            (key for key in logs_dict if num_key_pattern.match(key)),
            key=lambda x: int(x)):
        
        rec         = logs_dict[k]
        instr       = rec.get("Instruction", "")
        time_taken  = rec.get("Time_Taken", 0)
        complete    = rec.get("Completeness", False)
        
        summary["Exercise Step"].append(instr)
        summary["Duration of Completion"].append(time_taken)
        
        summary["Successful Instructions"].append(complete)
        summary["Failed Instructions"].append(not complete and int(time_taken) != 0)
        summary["Omitted Instructions"].append(not complete and int(time_taken) == 0)

    
    for k, v in logs_dict.items():
        if not num_key_pattern.match(k):      
            summary[k] = v
    if "Pain" in logs_dict:
        summary["Pain Detection"] = logs_dict["Pain"]
    if "Fatigue" in logs_dict:
        summary["Fatigue"] = logs_dict["Fatigue"]
    if "Dizziness" in logs_dict:
        summary["Dizziness"] = logs_dict["Dizziness"]
        
    return summary

def download_and_merge_json(json_id, save_path, merge=False):
    # url = f"https://caduceus-test-754616842718.us-west1.run.app/download/file/{json_id}/"
    url = f"https://api.reia-rehab.com/download/file/{json_id}/"
    headers = {
        "Accept": "application/json"
    }

    response = requests.get(url, headers=headers)
    
    if response.status_code == 200:
        try:
            # Parse the JSON response
            data = response.json()
            
            # Save the parsed JSON data
            with open(save_path, "w") as f:
                json.dump(data, f, indent=4)
            
            merge_with_object_info(f"json/{json_id}", save_path, merge)
            print(f"DOWNLOAD SUCCESS: {json_id} saved to {save_path}")
            return True
        except json.JSONDecodeError as e:
            print(f"Error parsing JSON response: {e}")
            return False
    else:
        print(f"Failed to download JSON. Status code: {response.status_code}")
        return False
    

def merge_with_object_info(local_json_path, output_path, merge):
    """
    Download object_info from API and merge it with a local JSON file.
    
    Args:
        local_json_path (str): Path to the local JSON file to merge with object_info
        output_path (str): Path where the merged JSON will be saved
    """
    try:
        # First download object_info from API
        # url = "https://caduceus-test-754616842718.us-west1.run.app/download/file/object_info.json/"
        url = "https://api.reia-rehab.com/download/file/object_info.json/"
        headers = {"Accept": "application/json"}
        
        response = requests.get(url, headers=headers)
        if response.status_code != 200:
            print(f"Failed to download object_info. Status code: {response.status_code}")
            return False
            
        object_info = response.json()
        
        # Read local JSON file
        with open(local_json_path, 'r', encoding='utf-8') as f:
            local_data = json.load(f)
            
        # Merge the data
        if not merge:
            setup_info = {"setup": {}}
            setup_info["setup"]["ego"] = [[0, 0, 0],[0, 0, 0]]
            merged_data = {**local_data, **setup_info}
        elif isinstance(object_info, dict) and isinstance(local_data, dict):
            setup_info = {"setup": object_info["setup"]}
            setup_info["setup"]["ego"] = [[0, 0, 0],[0, 0, 0]]
            # Merge the data
            if isinstance(local_data, dict):
                merged_data = {**local_data, **setup_info}  # setup info takes precedence
                print(f"Successfully merged object_info with {local_json_path} into {output_path}")
            else:
                raise ValueError("Local file must contain a dictionary")
        else:
            raise ValueError("Both object_info and local file must contain dictionaries")
            
        # Save the merged data
        with open(output_path, 'w', encoding='utf-8') as f:
            json.dump(merged_data, f, indent=4)
        print(f"Merged data saved to {output_path}")
            
        return True
        
    except FileNotFoundError as e:
        print(f"Error: Could not find local JSON file - {e}")
        return False
    except json.JSONDecodeError as e:
        print(f"Error: Invalid JSON in local file - {e}")
        return False
    except Exception as e:
        print(f"Error during merge: {e}")
        return False

def generate_scenic_program(file_name):
    current_dir = current_dir = os.getcwd()
    parent_dir = os.path.dirname(current_dir)
    json_name = f"json/{file_name}.json"
    json_file_path = os.path.join(current_dir, json_name)
    print("Generating Scenic program", json_file_path)
    
    with open(os.path.join(current_dir, json_name), 'r') as file:
        annotations = json.load(file)
        model_file_path = os.path.join(
            parent_dir, "Scenic-main", "Scenic", "src", "scenic", "simulators", "unity", "model.scenic")
        api_file_path = os.path.join(
            parent_dir, "Scenic-main", "Scenic", "src", "scenic", "simulators", "unity", "actions.py")
        save_file_path = os.path.join(current_dir, "scenic_output", f"{file_name}" + ".scenic")
        example_scenic_programs_path = os.path.join(current_dir, "scenic_output", "example_scenic_program")
        log_file_path = os.path.join(current_dir, "logs", f"{file_name}" + ".json")
    
        synth = Synth(annotations, 
                      model_file_path, 
                      api_file_path,
                      example_scenic_programs_path)
        program = synth.synthesize()
    
        with open(save_file_path, 'w') as scenic_file:
            scenic_file.write(program)
        print("Done generating Scenic program")


def generate_and_upload_report(json_id, save_file_name):
    """
    Generate a report using summarize.py functionality and upload it to Google Cloud.
    
    Args:
        json_id (str): The ID of the exercise (e.g., "exercise1")
    """
    try:
        # Generate the report using summarize.py functionality
        current_dir = os.getcwd()
        file_name = json_id
        json_name = os.path.join(current_dir, "logs", f"{file_name}.json")

        with open(json_name, 'r', encoding='utf-8') as f:
            data = json.load(f)

        summary_json = summarize_logs(data)

        # Save the summary locally first
        save_path = f"summaries/{file_name}.json"
        os.makedirs("summaries", exist_ok=True)
        with open(save_path, "w") as f:
            json.dump(summary_json, f, indent=4)

        # Upload to Google Cloud
        # upload_url = "https://caduceus-test-754616842718.us-west1.run.app/upload/file/?upload_id=" + save_file_name
        upload_url = "https://api.reia-rehab.com/upload/json/?upload_id=" + save_file_name
        headers = {
            "Content-Type": "application/json"
        }
        
        # Create payload with the summary data
        payload = summary_json
        print(f"payload: {payload}")

        response = requests.post(upload_url, json=payload, headers=headers)
        print(f"Response: {response.text}")
    except Exception as e:
        print(f"Error during report generation and upload: {e}")
        return False