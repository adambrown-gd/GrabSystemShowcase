using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ACSL;
using ACSL.Utility;

namespace ACSL.Interaction
{
    public delegate void EventHandler_GrabObject(GrabObject obj);
    public delegate void EventHandler_GrabBase(GrabBase obj);
    public delegate void EventHandler_GrabbableGeneric();

    /// <summary>
    /// A struct written to more easily connect joints
    /// to eachother for the GrabSystem.
    /// 
    /// Adam Brown - Last Updated: 03/26/2019
    /// </summary>
    public struct JointData
    {
        public const int StandardBreakForce = 10000;
        public ConfigurableJoint Joint;
        public GrabPoint TargetPoint;
        public bool IsConnected()
        {
            return TargetPoint != null && Joint.connectedBody != null;
        }
        public void Connect(Rigidbody connectingBody, GrabPoint targetPoint)
        {
            Joint = targetPoint.gameObject.AddComponent<ConfigurableJoint>();

            Joint.breakForce = StandardBreakForce;
            Joint.enableCollision = false;
            Joint.connectedBody = connectingBody;

            Joint.angularXMotion = ConfigurableJointMotion.Locked;
            Joint.angularYMotion = ConfigurableJointMotion.Locked;
            Joint.angularZMotion = ConfigurableJointMotion.Locked;

            Joint.xMotion = ConfigurableJointMotion.Limited;
            Joint.yMotion = ConfigurableJointMotion.Limited;
            Joint.zMotion = ConfigurableJointMotion.Limited;

            Joint.connectedAnchor = connectingBody.GetComponent<DynamicObject>().m_ObjectPosition;
            Joint.anchor = Vector3.zero;

            Joint.autoConfigureConnectedAnchor = false;
            Joint.axis = Vector3.zero;

            TargetPoint = targetPoint;
        }
        public void Disconnect()
        {
            GameObject.Destroy(Joint);
            TargetPoint = null;
        }
    }
}