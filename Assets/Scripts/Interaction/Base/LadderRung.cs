using UnityEngine;
using UnityEditor;
using System;
using System.Collections;
using System.Collections.Generic;
using ACSL.ControllerInput;
using ACSL.Utility;
using ACSL.Network;
using ACSL.Game;

namespace ACSL.Interaction
{
    public class LadderRung : StaticObject
    {
        public float m_PullDistance = 2.0f;

        public Vector3 m_LeftHandPos;
        public Vector3 m_LeftHandRot;

        public Vector3 m_RightHandPos;
        public Vector3 m_RightHandRot;

        protected override void Init()
        {
            base.Init();
            m_HoldToGrab = false;
            m_HoldToGrabTime = 0;
        }
        protected override void Update()
        {
            if(Grabbed)
            {
                ViveController vController = GameUtils.LocalController as ViveController;

                float yPos = (ViveController.GetSteamVRDeviceFromTrackedObject(GrabParent.GetComponent<SteamVR_TrackedObject>()).velocity.y * -1) * Time.deltaTime;
                vController.transform.position += new Vector3(0, yPos, 0);

                GrabParent.m_HandModel.localPosition = GrabParent.BitHand == InputAction.C_LEFT ? m_LeftHandPos : m_RightHandPos;
                GrabParent.m_HandModel.localEulerAngles = GrabParent.BitHand == InputAction.C_LEFT ? m_LeftHandRot : m_RightHandRot;
            }

            base.Update();
        }
    }
}
