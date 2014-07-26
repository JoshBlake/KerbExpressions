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

            var components = gameObject.GetComponents<Component>();
            string list = "";
            foreach (var component in components)
            {
                string behaviorString = "";
                UnityEngine.Behaviour behavior = component as UnityEngine.Behaviour;
                if (behavior != null)
                {
                    behaviorString = " (Behavior)";
                    if (!behavior.enabled)
                    {
                        behaviorString += " (disabled)";
                    }
                }
                list += "Type: " + component.GetType() + behaviorString + "\n";
            }
            return list;
        }

        public static void WriteObjectCatalog()
        {
            var gameObjects = Resources.FindObjectsOfTypeAll<GameObject>();

            string catalog = "";

            foreach (var gameObject in gameObjects)
            {
                var transform = gameObject.transform;
                if (transform.parent == null)
                {
                    catalog += GetRootChildren("root", transform);
                    catalog += "=======================\n\n";
                }
            }

            System.IO.File.WriteAllText("catalog.txt", catalog);
            Util.Log("Catalog written");
        }

        public static string GetRootChildren(string tree, Transform transform)
        {
            if (transform == null)
            {
                return "";
            }

            var gameObject = transform.gameObject;
            tree += " -> " + gameObject.name;

            string activeString = "active";
            if (!gameObject.activeSelf)
            {
                activeString = "inactive";
            }
            if (!gameObject.activeInHierarchy)
            {
                activeString += ", inactive hierarchy";
            }

            string ret = tree + " (" + activeString + ")\n";

            ret += Util.GetGameObjectBehaviors(gameObject);
            ret += "\n";

            int childCount = transform.childCount;
            for (int i = 0; i < childCount; i++)
            {
                var childTransform = transform.GetChild(i);
                ret += GetRootChildren(tree, childTransform);
            }

            return ret;
        }
    }
}
