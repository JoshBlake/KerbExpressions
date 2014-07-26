using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Windows.Kinect;
using Joint = Windows.Kinect.Joint;

namespace KerbExpressions
{
    enum BoneTrackingState
    {
        NotTracked,
        Inferred,
        Tracked
    }

    class Bone
    {
        public JointType ParentJoint { get; private set; }
        public JointType ChildJoint { get; private set; }
        public Bone ParentBone { get; private set; }

        private readonly List<Bone> _childBones = new List<Bone>();
        public IList<Bone> ChildBones { get { return _childBones; } }

        public bool IsRoot
        {
            get
            {
                return ParentJoint == ChildJoint;
            }
        }

        public BoneTrackingState TrackingState { get; set; }

        public Quaternion Rotation { get; set; }

        public string Name
        {
            get
            {
                if (IsRoot)
                {
                    return ChildJoint.ToString() + " (Root)";
                }
                else
                {
                    return ParentJoint.ToString() + "-" + ChildJoint.ToString();
                }
            }
        }

        public Bone(JointType parentJoint, JointType childJoint, Bone parentBone)
        {
            ParentJoint = parentJoint;
            ChildJoint = childJoint;
            ParentBone = parentBone;
            Rotation = Quaternion.identity;
            TrackingState = BoneTrackingState.NotTracked;
        }

        public Bone(JointType rootJoint)
        {
            ParentJoint = rootJoint;
            ChildJoint = ChildJoint;
            ParentBone = null;
            Rotation = Quaternion.identity;
            TrackingState = BoneTrackingState.NotTracked;
        }

        public Bone AddChildBone(JointType childJoint)
        {
            var childBone = new Bone(ChildJoint, childJoint, this);
            ChildBones.Add(childBone);

            return childBone;
        }

        public Bone FindBoneWithChild(JointType targetJoint)
        {
            if (ChildJoint == targetJoint)
            {
                return this;
            }

            foreach (var child in _childBones)
            {
                var foundBone = child.FindBoneWithChild(targetJoint);

                if (foundBone != null)
                {
                    return foundBone;
                }
            }

            return null;
        }
    }

}
