using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;

namespace KerbExpressions
{
    class KerbalActor
    {
        Mesh _helmetMesh;
        Mesh _visorMesh;

        KFSMEvent _stumble;
        KFSMEvent _swimForward;
        KFSMEvent _ladderClimb;
        KFSMEvent _startRun;
        KFSMState _floating;

        public Part Part { get; private set; }
        public ProtoCrewMember CrewMember { get; private set; }
        public KerbalEVA EVA { get; private set; }
        public kerbalExpressionSystem ExpressionSystem { get; private set; }
        public Animator Animator { get; private set; }

        private bool _showHelmet = true;
        public bool ShowHelmet
        {
            get
            {
                return _showHelmet;
            }
            set
            {
                if (_showHelmet == value)
                {
                    return;
                }
                _showHelmet = value;
                UpdateHeadMeshes();
            }
        }

        public bool IsExpressionEnabled
        {
            get
            {
                return ExpressionSystem.enabled;
            }
            set
            {
                ExpressionSystem.enabled = value;
            }
        }

        public KerbalActor(Part part, ProtoCrewMember crewMember)
        {
            if (part == null)
            {
                throw new ArgumentNullException("part");
            }
            if (crewMember == null)
            {
                throw new ArgumentNullException("crewMember");
            }

            Util.Log("Creating actor: {0} with name {1}, gameobject name {2}", crewMember.name, part.name, part.gameObject.name);

            Part = part;

            Util.GetGameObjectBehaviors(part.gameObject);
            CrewMember = crewMember;

            EVA = part.GetComponent<KerbalEVA>();
            if (EVA == null)
            {
                throw new InvalidOperationException("Part does not have KerbalEVA");
            }

            GetExpessionAnimator(part);

            GetFSM();

            GetHeadMeshes();
        }

        private void UpdateHeadMeshes()
        {
            foreach (Renderer renderer in Part.GetComponentsInChildren<Renderer>())
            {
                SkinnedMeshRenderer smr = renderer as SkinnedMeshRenderer;

                if (smr != null)
                {
                    switch (smr.name)
                    {
                        case "helmet":
                            if (!ShowHelmet)
                            {
                                smr.enabled = false;
                            }
                            else
                            {
                                smr.enabled = true;
                                //if (!isEva)
                                //    smr.sharedMesh = helmetMesh;
                            }
                            break;
                        case "visor":
                            if (!ShowHelmet)
                            {
                                smr.enabled = false;
                            }
                            else
                            {
                                smr.enabled = true;
                                //if (!isEva)
                                //    smr.sharedMesh = isAtmSuit ? null : visorMesh;
                            }
                            break;
                    }
                }
            }
        }

        private void GetHeadMeshes()
        {
            // Save pointer to helmet & visor meshes so helmet removal can restore them.
            foreach (SkinnedMeshRenderer smr
                     in Resources.FindObjectsOfTypeAll(typeof(SkinnedMeshRenderer)))
            {
                if (smr.name == "helmet")
                    _helmetMesh = smr.sharedMesh;
                else if (smr.name == "visor")
                    _visorMesh = smr.sharedMesh;
            }
        }

        public void Stumble()
        {
            EVA.fsm.RunEvent(_stumble);
        }

        private void GetFSM()
        {
            var evaType = EVA.GetType();
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
                        KFSMEvent ke = (KFSMEvent)field.GetValue(EVA);
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
                        KFSMState ks = (KFSMState)field.GetValue(EVA);
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

        private void GetExpessionAnimator(Part part)
        {
            ExpressionSystem = part.GetComponent<kerbalExpressionSystem>();
            if (ExpressionSystem == null)
            {
                var prefabEva = FlightEVA.fetch.evaPrefab_generic;
                var prefabAnim = prefabEva.GetComponent<Animator>();
                var prefabExpr = prefabEva.GetComponent<kerbalExpressionSystem>();

                Animator = part.GetComponent<Animator>();
                if (Animator == null)
                {
                    Util.Log("Creating Animator...");
                    Animator = part.gameObject.AddComponent<Animator>();

                    Animator.avatar = prefabAnim.avatar;
                    Animator.runtimeAnimatorController = prefabAnim.runtimeAnimatorController;

                    Animator.cullingMode = AnimatorCullingMode.BasedOnRenderers;
                    Animator.rootRotation = Quaternion.identity;
                    Animator.applyRootMotion = false;
                    //Animator.rootPosition = new Vector3(0.4f, 1.5f, 0.4f);
                    //Animator.rootRotation = new Quaternion(-0.7f, 0.5f, -0.1f, -0.5f);
                }

                Util.Log("Creating kerbalExpressionSystem...");
                ExpressionSystem = part.gameObject.AddComponent<kerbalExpressionSystem>();
                ExpressionSystem.evaPart = Part;
                ExpressionSystem.animator = Animator;
            }
            else
            {
                Animator = part.GetComponent<Animator>();
                Util.Log("Found Expression System");
            }
        }
    }
}
