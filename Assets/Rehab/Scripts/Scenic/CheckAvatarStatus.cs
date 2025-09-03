using System;
using System.Collections;
using System.Collections.Generic;
using Rehab.Scripts.Scenic;
using Unity.Mathematics;
using UnityEngine;

/// <summary>
/// Tracks and manages the patient's status during rehabilitation exercises
/// Monitors pain, fatigue, dizziness, and task completion states
/// </summary>
public class CheckAvatarStatus : MonoBehaviour
{
    // Patient condition indicators
    public string pain = "";
    public int speakActionCount = 0; 
    public string fatigue = "";
    public string dizziness = "";
    public string anything = "";
    
    // Task state flags
    public bool taskDone = false;
    public bool taskCompleted = true;
    public bool inProgress = false;
    public bool stopProgram = false;
    
    // Feedback and identification
    public string feedback = "";
    public string imageID = "";

    /// <summary>
    /// Marks the current task as completed
    /// </summary>
    public void CompletedTask()
    {
        taskDone = true;
    }

    /// <summary>
    /// Updates patient status with new analysis results
    /// </summary>
    public void UpdateStatus(AnalysisResult analysisResult)
    {
        this.pain = analysisResult.pain;
        this.fatigue = analysisResult.fatigue; 
        this.dizziness = analysisResult.dizziness;
        this.anything = analysisResult.anything;
    }

    /// <summary>
    /// Sets the identifier for the current image being analyzed
    /// </summary>
    public void SetImageID(string i)
    {
        imageID = i;
    }
}
