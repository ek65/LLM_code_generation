using System;
using System.Reflection;
using UnityEngine;

namespace Rehab.Scripts.Scenic
{
    /// <summary>
    /// Acts as a bridge between Scenic movement data and Unity action execution
    /// Dynamically invokes actions based on received movement commands
    /// </summary>
    public class PlayerInterface : MonoBehaviour
    {
        public ActionAPI actionAPI;

        /// <summary>
        /// Dynamically invokes the specified action method on ActionAPI with provided arguments
        /// </summary>
        /// <param name="data">Movement data containing action function name and arguments</param>
        public void ApplyMovement(ScenicMovementData data)
        {
            if (data.actionFunc != null)
            {
                Type type = actionAPI.GetType();
                MethodInfo method = type.GetMethod(data.actionFunc);
                if (method != null) method.Invoke(actionAPI, data.actionArgs.ToArray());
            }
        }
    }
}
