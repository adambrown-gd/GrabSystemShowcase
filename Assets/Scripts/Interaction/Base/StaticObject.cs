using UnityEngine;
using UnityEditor;
using System;
using System.Collections;
using System.Collections.Generic;
using ACSL.Utility;
using ACSL.Network;
using Photon.Pun;

namespace ACSL.Interaction
{
    /// <summary>
    /// Derived from GrabObject. StaticObjects generally are constrained 
    /// by movement or rotation, and can't be snapped.
    /// 
    /// Adam Brown - Last Updated: 03/26/2019
    /// </summary>
    public class StaticObject : GrabObject
    {
        public Transform m_ZeroPoint;

        [Tooltip("Constrain Left/Right rotation, meaning Z rotation")]
        public bool m_ConstrainX;
        [Tooltip("Min and max rotation values")]
        public Vector2 m_XConstraints;
        [Tooltip("Angle offset from 0")]
        public float m_XOffset;

        [Tooltip("Constrain Y rotation")]
        public bool m_ConstrainY;
        [Tooltip("Min and max rotation values")]
        public Vector2 m_YConstraints;
        [Tooltip("Angle offset from 0")]
        public float m_YOffset;

        [Tooltip("Constrain fwd/back rotation, meaning X rotation")]
        public bool m_ConstrainZ;
        [Tooltip("Min and max rotation values")]
        public Vector2 m_ZConstraints;
        [Tooltip("Angle offset from 0")]
        public float m_ZOffset;

        [Tooltip("The transform to base rotation constraint off of.")]
        public Transform m_LocalCoordSystem;

        public bool m_StaticHand = true;
        public Vector3 m_HandPosition;
        public Vector3 m_HandRotation;

        protected Vector3 m_RawRotation;
        protected Quaternion m_Original;

        #region PROTECTED METHODS

        protected override void Action_DetailedGrab(GrabPoint targetPoint, bool grab = true, bool forceGrab = false)
        {
            if (m_StaticHand)
            {
                //parent the hand to the object, and set position and rotation of it.
                targetPoint.m_HandModel.GetComponent<NetSync>().ParentThisObject(Grabbed ? transform : targetPoint.transform);
                targetPoint.m_HandModel.transform.localPosition = Grabbed ? (this as StaticObject).m_HandPosition : Vector3.zero;
                targetPoint.m_HandModel.transform.localEulerAngles = Grabbed ? (this as StaticObject).m_HandRotation : Vector3.zero;
                targetPoint.m_HandModel.transform.localScale = Vector3.one;
            }
        }
        protected override void Init()
        {
            base.Init();
        }
        protected override void Update()
        {
            if (m_LocalCoordSystem)
                m_Original = m_LocalCoordSystem.rotation;

            if (Grabbed && GrabParent.GetComponent<PhotonView>().IsMine)
                HandleStaticGrab();

            base.Update();
        }
        /// <summary>
        /// The function which governs which axis-rotations to apply.
        /// A little outdated, SteeringObject still uses this functionality.
        /// 
        /// TO DO: Move this into SteeringObject, and handle regular StaticObjects
        /// with joints
        /// 
        /// Adam Brown & Mike Watt - Last Updated: 03/26/2019
        /// </summary>
        protected virtual void HandleStaticGrab()
        {
            if (this is SteeringObject)
                Constraint_XZRotation();

            if (m_ConstrainY)
                Constraint_XYRotation();
            /*
            if (m_ConstrainX)
                Constraint_XZRotation();
            else if (m_ConstrainY)
                Constraint_XYRotation();
            else if (m_ConstrainZ)
                Constraint_YZRotation();
                */
        }
        /// <summary>
        /// Constrain the static object on the X and Z axis.
        /// 
        /// Adam Brown & Mike Watt - Last Updated: 03/26/2019
        /// </summary>
        protected void Constraint_XZRotation()
        {
            Vector3 dirXY, upXY, dirYZ, upYZ;
            Vector3 dirToTarget = (GrabParent.transform.position - transform.position);
            Vector3 originalUp = m_Original * Vector3.up;

            Vector3 zAxis = m_Original * Vector3.forward; // coordinate system z axis
            dirXY = Vector3.ProjectOnPlane(dirToTarget, zAxis);
            upXY = Vector3.ProjectOnPlane(originalUp, zAxis);
            float zAngle = Vector3.Angle(dirXY, upXY) * Mathf.Sign(Vector3.Dot(zAxis, Vector3.Cross(upXY, dirXY)));
            float zClamped = Mathf.Clamp(zAngle, m_ZConstraints.x, m_ZConstraints.y);
            Quaternion zRotation = Quaternion.AngleAxis(zClamped, zAxis);

            //if (m_Debug)
            //    Debug.Log(string.Format("Desired Z rotation: {0}, clamped Z rotation: {1}", zAngle, zClamped), this);

            originalUp = zRotation * m_Original * Vector3.up;
            Vector3 xAxis = zRotation * m_Original * Vector3.right; // our local x axis
            dirYZ = Vector3.ProjectOnPlane(dirToTarget, xAxis);
            upYZ = Vector3.ProjectOnPlane(originalUp, xAxis);
            float xAngle = Vector3.Angle(dirYZ, upYZ) * Mathf.Sign(Vector3.Dot(xAxis, Vector3.Cross(upYZ, dirYZ)));
            float xClamped = Mathf.Clamp(xAngle, m_XConstraints.x, m_XConstraints.y);
            Quaternion xRotation = Quaternion.AngleAxis(xClamped, Vector3.right);

            //if (m_Debug)
            //    Debug.Log(string.Format("Desired X rotation: {0}, clamped X rotation: {1}", xAngle, xClamped), this);

            Quaternion newRotation = zRotation * m_Original * xRotation;
            transform.rotation = newRotation;

            m_RawRotation.x = xClamped;
            m_RawRotation.z = zClamped;
        }
        /// <summary>
        /// Constrain the static object on the X and Y axis.
        /// 
        /// Adam Brown & Mike Watt - Last Updated: 03/26/2019
        /// </summary>
        protected void Constraint_XYRotation()
        {
            Vector3 dirXZ, forwardXZ, dirYZ, forwardYZ;
            Vector3 dirToTarget = (GrabParent.transform.position - transform.position);
            Vector3 originalForward = m_Original * Vector3.forward;

            Vector3 yAxis = m_Original * Vector3.up; // coordinate system y axis
            dirXZ = Vector3.ProjectOnPlane(dirToTarget, yAxis);
            forwardXZ = Vector3.ProjectOnPlane(originalForward, yAxis);
            float yAngle = Vector3.Angle(dirXZ, forwardXZ) * Mathf.Sign(Vector3.Dot(yAxis, Vector3.Cross(forwardXZ, dirXZ))) + m_YOffset;
            float yClamped = Mathf.Clamp(yAngle, m_YConstraints.x + m_YOffset, m_YConstraints.y + m_YOffset);
            Quaternion yRotation = Quaternion.AngleAxis(yClamped, yAxis);

            //if (m_Debug)
            //    Debug.Log(string.Format("Desired Y rotation: {0}, clamped Y rotation: {1}", yAngle, yClamped), this);

            originalForward = yRotation * m_Original * Vector3.forward;
            Vector3 xAxis = yRotation * m_Original * Vector3.right; // our local x axis
            dirYZ = Vector3.ProjectOnPlane(dirToTarget, xAxis);
            forwardYZ = Vector3.ProjectOnPlane(originalForward, xAxis);
            float xAngle = Vector3.Angle(dirYZ, forwardYZ) * Mathf.Sign(Vector3.Dot(xAxis, Vector3.Cross(forwardYZ, dirYZ)));
            float xClamped = Mathf.Clamp(xAngle, m_XConstraints.x, m_XConstraints.y);
            Quaternion xRotation = Quaternion.AngleAxis(xClamped, Vector3.right);

            //if (m_Debug)
            //    Debug.Log(string.Format("Desired X rotation: {0}, clamped X rotation: {1}", xAngle, xClamped), this);

            Quaternion newRotation = yRotation * m_Original * xRotation;
            transform.rotation = newRotation;

            m_RawRotation.x = xClamped;
            m_RawRotation.y = yClamped;
        }
        /// <summary>
        /// Constrain the static object on the Y and Z axis.
        /// 
        /// Adam Brown & Mike Watt - Last Updated: 03/26/2019
        /// </summary>
        protected void Constraint_YZRotation()
        {
            Vector3 dirYZ, rightYZ, dirXY, rightXY;
            Vector3 dirToTarget = (GrabParent.transform.position - transform.position);
            Vector3 originalRight = m_Original * Vector3.right;

            Vector3 zAxis = m_Original * Vector3.forward; // coordinate system z axis
            dirXY = Vector3.ProjectOnPlane(dirToTarget, zAxis);
            rightXY = Vector3.ProjectOnPlane(originalRight, zAxis);
            float zAngle = Vector3.Angle(dirXY, rightXY) * Mathf.Sign(Vector3.Dot(zAxis, Vector3.Cross(rightXY, dirXY)));
            float zClamped = Mathf.Clamp(zAngle, m_ZConstraints.x, m_ZConstraints.y);
            Quaternion zRotation = Quaternion.AngleAxis(zClamped, zAxis);

            //if (m_Debug)
            //    Debug.Log(string.Format("Desired Z rotation: {0}, clamped Z rotation: {1}", zAngle, zClamped), this);

            originalRight = zRotation * m_Original * Vector3.right;
            Vector3 yAxis = zRotation * m_Original * Vector3.up; // our local y axis
            dirYZ = Vector3.ProjectOnPlane(dirToTarget, yAxis);
            rightYZ = Vector3.ProjectOnPlane(originalRight, yAxis);
            float yAngle = Vector3.Angle(dirYZ, rightYZ) * Mathf.Sign(Vector3.Dot(yAxis, Vector3.Cross(rightYZ, dirYZ)));
            float yClamped = Mathf.Clamp(yAngle, m_XConstraints.x, m_XConstraints.y);
            Quaternion yRotation = Quaternion.AngleAxis(yClamped, Vector3.up);

            //if (m_Debug)
            //    Debug.Log(string.Format("Desired Y rotation: {0}, clamped Y rotation: {1}", yAngle, yClamped), this);

            Quaternion newRotation = zRotation * m_Original * yRotation;
            transform.rotation = newRotation;

            m_RawRotation.y = yClamped;
            m_RawRotation.z = zClamped;
        }
        #endregion
    }
}
