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

                if (Input.GetKeyDown(KeyCode.Alpha0))
                {
                    UpdateExpressionSystems();
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

        KFSMEvent _stumble;
        KFSMEvent _swimForward;
        KFSMEvent _ladderClimb;
        KFSMEvent _startRun;
        KFSMState _floating;

        private void UpdateKerbal(KerbalActor actor)
        {
            if (actor == null)
            {
                throw new ArgumentNullException("actor");
            }

            var eva = actor.EVA;
            if (Input.GetKeyDown(KeyCode.Alpha1))
            {
                //var animations = actor.EVA.Animations.GetAllAnimations();
                //Util.Log("Reading crew {0} animations", actor.CrewMember.name);
                //foreach (var anim in animations)
                //{
                //    Util.Log("\tAnim {0}", anim.animationName);
                //}

                eva.fsm.RunEvent(_stumble);
            }
            else if (Input.GetKeyDown(KeyCode.Alpha2))
            {
                //Util.Log("Reading crew {0} meshes", actor.CrewMember.name);

                //var panicMeshes = actor.ExpressionAI.panicMeshes;
                //foreach (var mesh in panicMeshes)
                //{
                //    Util.Log("\tPanic mesh {0}, {1} vertices", mesh.name, mesh.vertexCount);
                //}

                //var wheeeMeshes = actor.ExpressionAI.wheeeMeshes;
                //foreach (var mesh in wheeeMeshes)
                //{
                //    Util.Log("\tWheee mesh {0}, {1} vertices", mesh.name, mesh.vertexCount);
                //}

                //Resources.FindObjectsOfTypeAll(typeof(SkinnedMeshRenderer))

                eva.fsm.RunEvent(_startRun);
            }
            else if (Input.GetKeyDown(KeyCode.Alpha3))
            {
                eva.fsm.RunEvent(_ladderClimb);
            }
            else if (Input.GetKeyDown(KeyCode.Alpha4))
            {
                eva.fsm.RunEvent(_swimForward);
            }
            else if (Input.GetKeyDown(KeyCode.Alpha5))
            {
                eva.fsm.StartFSM(_floating);
            }
            else if (Input.GetKeyDown(KeyCode.Alpha6))
            {
                var evaType = eva.GetType();
                var fields = evaType.GetFields(BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public);

                Type kfsmEventType = typeof(KFSMEvent);
                Type kfsmStateType = typeof(KFSMState);

                string fieldNames = "Field Names for " + evaType.ToString() + ":\n";
                foreach (var field in fields)
                {
                    try
                    {
                        if (field.FieldType == kfsmEventType)
                        {
                            KFSMEvent ke = (KFSMEvent)field.GetValue(eva);
                            fieldNames += "KFSMEvent: " + field.Name + " (" + ke.name + ")\n";
                            if (ke.name == "Stumble")
                            {
                                _stumble = ke;
                            }
                            else if (ke.name == "Swim Forward")
                            {
                                _swimForward = ke;
                            }
                            else if (ke.name == "Ladder Climb")
                            {
                                _ladderClimb = ke;
                            }
                            else if (ke.name == "Start Run")
                            {
                                _startRun = ke;
                            }
                        }
                        else if (field.FieldType == kfsmStateType)
                        {
                            KFSMState ks = (KFSMState)field.GetValue(eva);
                            fieldNames += "KFSMState: " + field.Name + " (" + ks.name + ")\n";
                            if (ks.name == "Idle (Floating)")
                            {
                                _floating = ks;
                            }
                        }
                    }
                    catch (Exception)
                    {
                        fieldNames += "???\n";
                    }
                }
                Util.Log(fieldNames);
            }
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
                    Util.PrintGameObjectTree(ai.gameObject);
                    Util.PrintGameObjectBehaviors(ai.gameObject);
                }
            }
        }
    }
}
