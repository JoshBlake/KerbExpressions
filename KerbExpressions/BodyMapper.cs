using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Windows.Kinect;
using Joint = Windows.Kinect.Joint;

namespace KerbExpressions
{
    class BodyMapper
    {
        Bone _rootBone;
        public Bone RootBone { get { return _rootBone; } }

        public BodyMapper()
        {
            CreateParentChildBones();
        }

        private void CreateParentChildBones()
        {
            _rootBone = new Bone(JointType.SpineShoulder);
            _rootBone.AddChildBone(JointType.Neck)
                    .AddChildBone(JointType.Head);

            var handLeft = _rootBone.AddChildBone(JointType.ShoulderLeft)
                                   .AddChildBone(JointType.ElbowLeft)
                                   .AddChildBone(JointType.WristLeft)
                                   .AddChildBone(JointType.HandLeft);

            handLeft.AddChildBone(JointType.HandTipLeft);
            handLeft.AddChildBone(JointType.ThumbLeft);

            var handRight = _rootBone.AddChildBone(JointType.ShoulderRight)
                                    .AddChildBone(JointType.ElbowRight)
                                    .AddChildBone(JointType.WristRight)
                                    .AddChildBone(JointType.HandRight);

            handRight.AddChildBone(JointType.HandTipRight);
            handRight.AddChildBone(JointType.ThumbRight);


        }

        public Bone GetBoneOrientations(Body body)
        {
            Dictionary<JointType, Quaternion> orientations = new Dictionary<JointType,Quaternion>();

            Queue<Bone> boneQueue = new Queue<Bone>();
            boneQueue.Enqueue(_rootBone);

            while (boneQueue.Count > 0)
            {
                var parentBone = boneQueue.Dequeue();
                

                foreach (var childBone in parentBone.ChildBones)
                {
                    UpdateBone(childBone, body);
                    boneQueue.Enqueue(childBone);
                }
            }

            return _rootBone;
        }

        private void UpdateBone(Bone bone, Body body)
        {
            var joints = body.Joints;
            Joint parentJoint = joints[bone.ParentJoint];
            Joint childJoint = joints[bone.ChildJoint];

            bone.TrackingState = GetBoneTrackingState(parentJoint, childJoint);

            var orientation = body.JointOrientations[bone.ChildJoint].Orientation;

            Quaternion q = new Quaternion(orientation.X, orientation.Y, orientation.Z, orientation.W);

            //Vector3 parentVector = GetBoneVector(nearJoint, parentJoint);
            //Vector3 childVector = GetBoneVector(parentJoint, childJoint);

            //Quaternion quaternion = GetQuaternionForVectors(parentVector, childVector);

            bone.Rotation = q;
        }

        private BoneTrackingState GetBoneTrackingState(Joint nearJoint, Joint farJoint)
        {
            var nearState = nearJoint.TrackingState;
            var farState = farJoint.TrackingState;

            //T + T => T
            //T + I => I
            //I + T => I
            //I + I => I

            if (nearState == TrackingState.Tracked && farState == TrackingState.Tracked)
            {
                return BoneTrackingState.Tracked;
            }

            if (nearState == TrackingState.NotTracked || farState == TrackingState.NotTracked)
            {
                return BoneTrackingState.NotTracked;
            }

            //Can't be both tracked, and neither are not tracked
            //Must be inferred and either tracking or inferred
            return BoneTrackingState.Inferred;
        }

        private Vector3 GetBoneVector(Joint parentJoint, Joint childJoint)
        {
            var parentVector = parentJoint.Position;
            var childVector = childJoint.Position;

            var vector = new Vector3(childVector.X - parentVector.X,
                                     childVector.Y - parentVector.Y,
                                     childVector.Z - parentVector.Z);

            return vector;
        }

        static Quaternion GetQuaternionForVectors(Vector3 fromVector, Vector3 toVector)
        {
            fromVector.Normalize();
            toVector.Normalize();

            float d = Vector3.Dot(fromVector, toVector);

            if (d >= 1.0f)
            {
                //Parllel -> No rotation
                return Quaternion.identity;
            }

            if (d < (Vector3.kEpsilon - 1.0f))
            {
                //Anti-parallel

                // Generate an axis
                Vector3 axis = Vector3.Cross(Vector3.right, fromVector);
                if (axis.magnitude == 0)
                {
                    // pick another if colinear
                    axis = Vector3.Cross(Vector3.up, fromVector);
                }
                axis.Normalize();
                var q = Quaternion.AngleAxis(180.0f, axis);
                return q;
            }
            else
            {
                float s = (float)Math.Sqrt((1 + d) * 2);
                float invs = 1 / s;

                Vector3 cross = Vector3.Cross(fromVector, toVector);

                float x = cross.x * invs;
                float y = cross.y * invs;
                float z = cross.z * invs;
                float w = s * 0.5f;
                Quaternion q = new Quaternion(x, y, z, w);

                Normalize(q);

                return q;
            }
        }

        static void Normalize(Quaternion q)
        {
            float len2 = q.w * q.w + 
                         q.x * q.x + 
                         q.y * q.y + 
                         q.z * q.z;

            float factor = (float)(1.0 / Math.Sqrt(len2));

            q.x *= factor;
            q.y *= factor;
            q.z *= factor;
            q.w *= factor;
        }
    }
}
