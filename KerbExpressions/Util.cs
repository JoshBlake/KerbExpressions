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

        public static string GetGameObjectTree(GameObject gameObject)
        {
            if (gameObject == null)
            {
                return "GameObject is null";
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
            tree += " root\n";
            return tree;
        }

        public static string GetGameObjectBehaviors(GameObject gameObject)
        {
            if (gameObject == null)
            {
                return "GameObject is null";
            }

            var behaviors = gameObject.GetComponents<Behaviour>();
            string list = "";
            foreach (var behavior in behaviors)
            {
                string disabledString = "";
                if (!behavior.enabled)
                {
                    disabledString = " (disabled)";
                }
                list += behavior.GetType() + disabledString + "\n";
            }
            return list;
        }

    }
}
