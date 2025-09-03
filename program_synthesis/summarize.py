import json
import re
import os

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
        if not num_key_pattern.match(k) and k not in ["Pain", "Fatigue", "Dizziness"]:      
            summary[k] = v
            
    if "Pain" in logs_dict:
        summary["Pain Detection"] = logs_dict["Pain"]
    else:
        summary["Pain Detection"] = [False, "", "", ""]
    if "Fatigue" in logs_dict:
        summary["Fatigue"] = logs_dict["Fatigue"]
    else:
        summary["Fatigue"] = [False, ""]
    if "Dizziness" in logs_dict:
        summary["Dizziness"] = logs_dict["Dizziness"]
    else:
        summary["Fatigue"] = [False, ""]
        
    return summary

def create_report(json_id):
    ### Summarize the logs and Upload to the Cloud
    current_dir = current_dir = os.getcwd()
    file_name = "exercise"+str(json_id)
    json_name = os.path.join(current_dir, "user_study", "logs", f"{file_name}" + ".json")

    with open(json_name, 'r', encoding='utf-8') as f:
        data = json.load(f)

    summary_json = summarize_logs(data)

    save_path = f"summaries/{file_name}.json"
    with open(save_path, "w") as f:
        json.dump(summary_json, f, indent=4)

    print(f"Summarized successfully and saved to {save_path}")
    print(summary_json)