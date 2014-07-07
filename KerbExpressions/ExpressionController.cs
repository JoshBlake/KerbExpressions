using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;

namespace KerbExpressions
{
    class ExpressionController
    {
        bool _wasEVAActive = false;

        Vessel _lastActiveVessel = null;

        KerbalActor _actor = null;
        kerbalExpressionSystem[] _expressionSystems;

        public void Start()
        {
            UpdateExpressionSystems();
        }

        public void Update()
        {
            try
            {
                if (!HighLogic.LoadedSceneIsFlight)
                {
                    //TODO cleanup stuff?
                    return;
                }

                //foreach (Vessel vessel in FlightGlobals.Vessels)
                Vessel vessel = FlightGlobals.ActiveVessel;

                if (_lastActiveVessel != vessel)
                {
                    _lastActiveVessel = vessel;
                    Util.Log("Saw new active vessel: " + vessel.vesselName);

                    var newActor = GetActorForVessel(vessel);

                    if (newActor == null)
                    {
                        //Try command seats
                        var seatActors = GetCommandSeatEVAs(vessel);
                        if (seatActors != null)
                        {
                            newActor = seatActors.FirstOrDefault();
                        }
                    }

                    if (_actor == null && newActor != null)
                    {
                        Util.Log("Entered EVA: {0}", newActor.CrewMember.name);
                    }
                    else if (_actor != null && newActor == null)
                    {
                        Util.Log("Left EVA, was with {0}", _actor.CrewMember.name);
                    }
                    else if (_actor != newActor)
                    {
                        Util.Log("Switched to new EVA: {0}", newActor.CrewMember.name);
                    }
                    _actor = newActor;
                }

                if (_actor != null)
                {
                    UpdateKerbal(_actor);
                }
            }
            catch (Exception ex)
            {
                Util.Log("{0}: {1}\n{2}", ex.GetType(), ex.Message, ex.StackTrace);
            }
        }

        private KerbalActor GetActorForVessel(Vessel vessel)
        {
            if (vessel == null)
            {
                return null;
            }

            var eva = vessel.GetComponent<KerbalEVA>();
            if (eva != null)
            {
                List<ProtoCrewMember> crew = vessel.rootPart.protoModuleCrew;
                if (crew.Count > 0)
                {
                    return new KerbalActor(vessel.rootPart, crew[0]);
                }
            }

            return null;
        }

        private static List<KerbalActor> GetCommandSeatEVAs(Vessel vessel)
        {
            var actors = new List<KerbalActor>();

            // Vessel is a ship. Update Kerbals on external seats.
            foreach (Part part in vessel.parts)
            {
                KerbalSeat seat = part.GetComponent<KerbalSeat>();
                if (seat == null || seat.Occupant == null)
                    continue;

                List<ProtoCrewMember> crew = seat.Occupant.protoModuleCrew;
                if (crew.Count > 0)
                {
                    var seatEva = seat.Occupant.GetComponent<KerbalEVA>();
                    if (seatEva != null)
                    {
                        actors.Add(new KerbalActor(seat.Occupant, crew[0]));
                    }
                }
            }

            return actors;
        }

        private void UpdateKerbal(KerbalActor actor)
        {
            if (actor == null)
            {
                throw new ArgumentNullException("actor");
            }

            var eva = actor.EVA;
            if (Input.GetKeyDown(KeyCode.Alpha1))
            {
                Util.Log("Alpha1");

                Util.Log("Set animator expression to 1.0");

                string expressionName = "Expression";
                int expressionhash = Animator.StringToHash(expressionName);
                actor.Animator.SetFloat(expressionhash, 1.0f);

            }
            else if (Input.GetKeyDown(KeyCode.Alpha2))
            {
                Util.Log("Alpha2");

                //Resources.FindObjectsOfTypeAll(typeof(SkinnedMeshRenderer))

                //eva.fsm.RunEvent(_startRun);

                Util.Log("Set animator expression to -1.0");

                string expressionName = "Expression";
                int expressionhash = Animator.StringToHash(expressionName);
                actor.Animator.SetFloat(expressionhash, -1.0f);

            }
            else if (Input.GetKeyDown(KeyCode.Alpha3))
            {
                Util.Log("Alpha3");

                actor.ShowHelmet = !actor.ShowHelmet;
                Util.Log("Toggled hasHelmet to " + actor.ShowHelmet);

                //string hasHelmetName = "hasHelmet";
                //int helmetHash = Animator.StringToHash(hasHelmetName);
                //actor.Animator.SetBool(helmetHash, _hasHelmet);
            }
            else if (Input.GetKeyDown(KeyCode.Alpha4))
            {
                Util.Log("Alpha4");

                if (actor.ExpressionSystem != null)
                {
                    actor.ExpressionSystem.fearFactor = 5.0f;
                    Util.Log("Set " + actor.CrewMember.name + " fear to 5.0");
                }
                else
                {
                    Util.Log("No expression system for " + actor.CrewMember.name);
                }
            }
            else if (Input.GetKeyDown(KeyCode.Alpha5))
            {
                Util.Log("Alpha5");
                if (actor.ExpressionSystem != null)
                {
                    actor.ExpressionSystem.fearFactor = -5.0f;
                    Util.Log("Set " + actor.CrewMember.name + " fear to -5.0");
                }
                else
                {
                    Util.Log("No expression system for " + actor.CrewMember.name);
                }
            }
            else if (Input.GetKeyDown(KeyCode.Alpha6))
            {
                Util.Log("Alpha6");
                actor.IsExpressionEnabled = !actor.IsExpressionEnabled;
                Util.Log(actor.CrewMember.name + " IsExpressionEnabled is now: " + actor.IsExpressionEnabled);                
            }
            else if (Input.GetKeyDown(KeyCode.Alpha7))
            {
                Util.Log("Alpha7");

                actor.Stumble();
            }
            else if (Input.GetKeyDown(KeyCode.Alpha8))
            {
                Util.Log("Alpha8");

                float variance = UnityEngine.Random.Range((float)0f, (float)1f);

                string varianceName = "Variance";
                int variancehash = Animator.StringToHash(varianceName);
                actor.Animator.SetFloat(variancehash, variance);

                Util.Log("Variance set to " + variance);
                //GetKerbalInfo(actor.Part.gameObject);
            }
            else if (Input.GetKeyDown(KeyCode.Alpha9))
            {
                Util.Log("Alpha9");
                float variance = UnityEngine.Random.Range((float)0f, (float)1f);

                string varianceName = "SecondaryVariance";
                int variancehash = Animator.StringToHash(varianceName);
                actor.Animator.SetFloat(variancehash, variance);

                Util.Log("SecondaryVariance set to " + variance);
            }
            else if (Input.GetKeyDown(KeyCode.Alpha0))
            {
                Util.Log("Alpha0");

                Util.Log("Set animator expression to 0.0");

                string expressionName = "Expression";
                int expressionhash = Animator.StringToHash(expressionName);
                actor.Animator.SetFloat(expressionhash, 0.0f);
            }
        }

        private static void ReadAnimations(KerbalActor actor)
        {
            var animations = actor.EVA.Animations.GetAllAnimations();
            string data = "Reading crew " + actor.CrewMember.name + " animations";
            foreach (var anim in animations)
            {
                data += anim.animationName + "\n";
            }
            Util.Log(data);
        }

        private static string GetPropertyValues(object obj)
        {
            var type = obj.GetType();
            string ret = "Properties for " + type.ToString() + " instance:\n";

            try
            {
                var properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);

                foreach (var propertyInfo in properties)
                {
                    var propType = propertyInfo.PropertyType;
                    object value = propertyInfo.GetValue(obj, null);

                    ret += propertyInfo.Name + " (" + propType.ToString() + ") :" + value.ToString() + "\n";
                }
            }
            catch (Exception ex)
            {
                ret += ex.ToString() + "\n";
            }

            return ret;
        }

        private static void GetKerbalInfo(GameObject gameObject)
        {
            var anim = gameObject.GetComponent<Animator>();
            var expr = gameObject.GetComponent<kerbalExpressionSystem>();

            string data = "Kerbal GameObject info '" + gameObject.name + "':\n";

            if (anim == null)
            {
                data += "Animator is null\n";
            }
            else
            {
                data += "Animator is present\n";
                var avatar = anim.avatar;
                if (avatar == null)
                {
                    data += "Avatar is null\n";
                }
                else
                {
                    data += "Avatar is present: '" + avatar.name + "'\n";
                    if (avatar.isValid)
                    {
                        data += "Avatar is valid\n";
                    }
                }

                var controller = anim.runtimeAnimatorController;
                if (controller == null)
                {
                    data += "Controller is null\n";
                }
                else
                {
                    data += "Controller is present: '" + controller.name + "'\n";
                }
            }
            data += "\n";
            if (expr == null)
            {
                data += "kerbalExpressionSystem is null\n";
            }
            else
            {
                data += "kerbalExpressionSystem is present\n";

                if (expr.kerbal == null)
                {
                    data += "kerbal is null\n";
                }
                else
                {
                    data += "kerbal is present\n";
                    data += "kerbal name: " + expr.kerbal.crewMemberName + "\n";
                }

                if (expr.evaPart == null)
                {
                    data += "evaPart is null\n";
                }
                else
                {
                    data += "evaPart is present\n";
                    data += "evaPart name: " + expr.evaPart.partName + "\n";
                }
            }
            data += "===\n";
            data += Util.GetGameObjectTree(gameObject);
            data += Util.GetGameObjectBehaviors(gameObject);

            Util.Log(data);
        }

        void UpdateExpressionSystems()
        {
            Util.Log("Finding Expression Systems...");
            _expressionSystems = Resources.FindObjectsOfTypeAll<kerbalExpressionSystem>();

            if (_expressionSystems == null || _expressionSystems.Length == 0)
            {
                Util.Log("Found no expression systems");
            }
            else
            {
                foreach (var ai in _expressionSystems)
                {
                    Util.Log("Exp System {0} attached to {1}", ai.name, ai.gameObject.name);
                    string tree = Util.GetGameObjectTree(ai.gameObject);
                    string behaviors = Util.GetGameObjectBehaviors(ai.gameObject);

                    Util.Log(tree + behaviors);
                }
            }
        }

        private void CatalogObjects()
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

        private string GetRootChildren(string tree, Transform transform)
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
