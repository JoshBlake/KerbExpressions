using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using UnityEngine;

namespace KerbExpressions
{
    static class Util
    {
        public static void Log(string s, params object[] args)
        {
            Type callerClass = new StackTrace(1, false).GetFrame(0).GetMethod().DeclaringType;
            UnityEngine.Debug.Log("[" + UnityEngine.Time.realtimeSinceStartup + "] KerbExpressions." + callerClass.Name + "] " + String.Format(s, args));
        }

        public static void PrintGameObjectTree(GameObject gameObject)
        {
            if (gameObject == null)
            {
                Util.Log("GameObject is null");
                return;
            }

            var transform = gameObject.transform;
            string activeString = "active";
            if (!gameObject.activeSelf)
            {
                activeString = "inactive";
            }
            if (!gameObject.activeInHierarchy)
            {
                activeString += ", inactive hierarchy";
            }
            string tree = "GameObject list for " + gameObject.name + " (" + activeString + ") : ";
            while (transform != null)
            {
                tree += transform.gameObject.name + " -> ";
                transform = transform.parent;
            }
            tree += " root";
            Util.Log(tree);
        }

        public static void PrintGameObjectBehaviors(GameObject gameObject)
        {
            if (gameObject == null)
            {
                Util.Log("GameObject is null");
                return;
            }

            var behaviors = gameObject.GetComponentsInChildren<Behaviour>(true);
            string list = "GameObject behaviors for " + gameObject.name + ": ";
            foreach (var behavior in behaviors)
            {
                string disabledString = "";
                if (!behavior.enabled)
                {
                    disabledString = "disabled ";
                }
                list += behavior.name + " has a " + disabledString + behavior.GetType() + ", ";
            }
            Util.Log(list);
        }

    }
}
