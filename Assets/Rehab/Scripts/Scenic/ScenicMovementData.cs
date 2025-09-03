using System.Collections.Generic;
using UnityEngine;

namespace Rehab.Scripts.Scenic
{
    public class ScenicMovementData
    {
        public Model model;
        public Vector3 position;
    
        public string actionFunc;
        public List<object> actionArgs;
    
        public bool stopButton;
        
        public ScenicMovementData (Vector3 position, string modelType)
        {
            this.position = position;
            this.model = new Model(modelType);

        }

        // override function for when there is an action
        public ScenicMovementData(Vector3 position, string modelType, string actionFunc, List<object> actionArgs)
        {
            this.position = position;
            this.model = new Model(modelType);
            this.actionFunc = actionFunc;
            this.actionArgs = actionArgs;
        }
    }



    public class Model
    {
        public string modelType;
        public Model(string mType)
        {
            this.modelType = mType;
        }
    }
}