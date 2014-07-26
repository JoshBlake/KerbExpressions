using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using KerbExpressions.Rendering;
using UnityEngine;

namespace KerbExpressions
{
    enum Joints
    {

    }

    class KerbalActor : IDisposable
    {
        Mesh _helmetMesh;
        Mesh _visorMesh;

        KFSMEvent _stumble;
        KFSMEvent _swimForward;
        KFSMEvent _ladderClimb;
        KFSMEvent _startRun;
        KFSMState _floating;

        private readonly Dictionary<Transform, LineData> _transformLineData = new Dictionary<Transform, LineData>();

        public Part Part { get; private set; }
        public ProtoCrewMember CrewMember { get; private set; }
        public KerbalEVA EVA { get; private set; }
        public kerbalExpressionSystem ExpressionSystem { get; private set; }
        public Animator Animator { get; private set; }

        public Transform ElbowRightTransform { get; private set; }
        public Transform ShoulderRightTransform { get; private set; }


        public Transform ElbowLeftTransform { get; private set; }
        public Transform ShoulderLeftTransform { get; private set; }


        public LineOverlay LineOverlay { get; private set; }

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

            if (!Part.GetComponent<LineOverlay>())
            {
                //var parentTransform = Part.parentTransform.FindChild("globalMove01");

                LineOverlay = Part.gameObject.AddComponent<LineOverlay>();
            }

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

            GetBoneRig();
        }

        ~KerbalActor()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            if (LineOverlay != null)
            {
                GameObject.Destroy(LineOverlay);
                LineOverlay = null;
            }
        }

        public void UpdateRigLines()
        {
            //var d1 = new LineData(Vector3.zero, new Vector3(1, 0, 0), Color.red);
            //var d2 = new LineData(Vector3.zero, new Vector3(0, 1, 0), Color.white);
            //var d3 = new LineData(Vector3.zero, new Vector3(0, 0, 1), Color.blue);

            //LineOverlay.AddLine(d1);
            //LineOverlay.AddLine(d2);
            //LineOverlay.AddLine(d3);

            var name = "globalMove01";

            var rootTransform = Part.transform.FindChild(name);

            var offset = new Vector3(1, 0, 0);
            var rotatedOffset = rootTransform.TransformDirection(offset);
            UpdateRigHelper(rootTransform, rotatedOffset);
        }

        private void UpdateRigHelper(Transform parentTransform, Vector3 offset)
        {
            if (parentTransform == null)
            {
                return;
            }
            var parentPos = parentTransform.position + offset;

            var count = parentTransform.childCount;
            for (int i = 0; i < count; i++)
            {
                var childTransform = parentTransform.GetChild(i);

                var childPos = childTransform.position + offset;

                LineData data = null;
                if (!_transformLineData.TryGetValue(childTransform, out data))
                {
                    data = new LineData(parentPos, childPos, Color.green)
                    {
                        UseWorldSpace = true,
                    };
                    _transformLineData[childTransform] = data;
                    LineOverlay.AddLine(data);
                }

                data.Start = parentPos;
                data.End = childPos;

                UpdateRigHelper(childTransform, offset);
            }
        }

        public void ResetRig()
        {
            var name = "globalMove01";

            var rootTransform = Part.transform.FindChild(name);

            ResetRigHelper(rootTransform);
        }

        private void ResetRigHelper(Transform parentTransform)
        {
            var count = parentTransform.childCount;

            for (int i = 0; i < count; i++)
            {
                var childTransform = parentTransform.GetChild(i);
                ResetRigHelper(childTransform);
            }

            parentTransform.localRotation = Quaternion.identity;
        }

        public void OutputRig()
        {
            var name = "globalMove01";

            var rootTransform = Part.transform.FindChild(name);

            string data = OutputRigHelper(rootTransform, "");
            File.WriteAllText("KerbalRig.txt", data);
        }

        private string OutputRigHelper(Transform parentTransform, string indent)
        {
            if (parentTransform == null)
            {
                return "(null parentTransform)\n";
            }
            string ret = indent + parentTransform.name;
            ret += "\tP: " + parentTransform.localPosition.ToString();
            ret += "\tRl: " + parentTransform.localRotation.eulerAngles.ToString();
            ret += "\tRg: " + parentTransform.rotation.eulerAngles.ToString() + "\n";

            var count = parentTransform.childCount;
            indent += "\t";
            for (int i = 0; i < count; i++)
            {
                var childTransform = parentTransform.GetChild(i);
                ret += OutputRigHelper(childTransform, indent);
            }

            return ret;
        }

        private void GetBoneRig()
        {
            var moveChild = Part.transform.FindChild("globalMove01");
            var transforms = moveChild.GetComponentsInChildren<Transform>(true);

            ElbowRightTransform = FindRigTransformByName("bn_r_elbow_a01", transforms);
            ShoulderRightTransform = FindRigTransformByName("bn_r_arm01 1", transforms);

            ElbowLeftTransform = FindRigTransformByName("bn_l_elbow_a01", transforms);
            ShoulderLeftTransform = FindRigTransformByName("bn_l_arm01 1", transforms);
        }

        private Transform FindRigTransformByName(string rigName, Transform[] transforms)
        {
            var targetTransform = transforms.Where((t) => t.name == rigName).FirstOrDefault();

            if (targetTransform != null)
            {
                var pos = targetTransform.localPosition;
                var rot = targetTransform.localRotation;
                Util.Log("Bone rig " + rigName + " found! Pos: " + pos.x + ", " + pos.y + ", " + pos.z + "  Rot: " + rot.x + ", " + rot.y + ", " + rot.z + ", " + rot.w);
            }
            else
            {
                Util.Log("Bone rig " + rigName + " not found...");
            }
            return targetTransform;
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
            //Util.Log(fieldNames);
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
