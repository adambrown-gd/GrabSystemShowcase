using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;

using ACSL.Utility;
using ACSL.Interaction;

namespace ACSL.ControllerInput
{
    public class Keybinding_LongGrab : InputAction
    {
        private GrabBase m_LeftHand_HighlightTarget;
        private GrabBase m_RightHand_HighlightTarget;

        public override void InputDisabled()
        {
            //not implemented.
        }
        public override void InputEnabled()
        {
            //not implemented.
        }
        public override void KeyPress(int hand)
        {
            //not implemented.
        }
        public override void KeyPressDown(int hand)
        {
            GrabPoint targetPoint = GrabPointByHand(hand);

            SteamVR_LaserPointer pointer = targetPoint.transform.Find("LongGrabPointer").GetComponent<SteamVR_LaserPointer>();
            pointer.gameObject.SetActive(true);
        }
        public override void KeyPressUp(int hand)
        {
            GrabPoint targetPoint = GrabPointByHand(hand);

            SteamVR_LaserPointer pointer = targetPoint.transform.Find("LongGrabPointer").GetComponent<SteamVR_LaserPointer>();
            pointer.gameObject.SetActive(false);

            if (targetPoint.BitHand == C_LEFT)
            {
                if (m_LeftHand_HighlightTarget)
                    m_LeftHand_HighlightTarget.Action_Highlight(targetPoint, false);
            }
            if (targetPoint.BitHand == C_RIGHT)
            {
                if (m_RightHand_HighlightTarget)
                    m_RightHand_HighlightTarget.Action_Highlight(targetPoint, false);
            }
        }
        public override void Update()
        {
            if (GrabPoint.LeftHand.transform.Find("LongGrabPointer").GetComponent<SteamVR_LaserPointer>().gameObject.activeInHierarchy)
                Raycast(GrabPoint.LeftHand);
            if (GrabPoint.RightHand.transform.Find("LongGrabPointer").GetComponent<SteamVR_LaserPointer>().gameObject.activeInHierarchy)
                Raycast(GrabPoint.RightHand);
        }
        public void Raycast(GrabPoint targetPoint)
        {
            SteamVR_LaserPointer pointer = targetPoint.transform.Find("LongGrabPointer").GetComponent<SteamVR_LaserPointer>();

            Ray grabRay = new Ray(pointer.transform.position, pointer.transform.forward);
            RaycastHit result;
            Debug.DrawRay(pointer.transform.position, pointer.transform.forward);

            int layerMask = LayerMask.GetMask("Interactable", "GrabPoint");

            if (Physics.Raycast(grabRay, out result, 2.5f, layerMask, QueryTriggerInteraction.Collide))
            {
                DynamicObject targetObject = result.transform.GetComponent<DynamicObject>();

                if (!targetObject)
                    return;
                if (targetObject.Highlighted || targetObject.Grabbed || targetObject.Snapped)
                    return;

                if (targetPoint.BitHand == C_LEFT)
                {
                    if (m_LeftHand_HighlightTarget)
                    {
                        if (m_LeftHand_HighlightTarget != targetObject)
                            m_LeftHand_HighlightTarget.Action_Highlight(targetPoint, false);

                        m_LeftHand_HighlightTarget = targetObject;
                        m_LeftHand_HighlightTarget.Action_Highlight(targetPoint, true);
                    }
                    else
                    {
                        m_LeftHand_HighlightTarget = targetObject;
                        m_LeftHand_HighlightTarget.Action_Highlight(targetPoint, true);
                    }
                }
                else if (targetPoint.BitHand == C_RIGHT)
                {
                    if (m_RightHand_HighlightTarget)
                    {
                        if (m_RightHand_HighlightTarget != targetObject)
                            m_RightHand_HighlightTarget.Action_Highlight(targetPoint, false);

                        m_RightHand_HighlightTarget = targetObject;
                        m_RightHand_HighlightTarget.Action_Highlight(targetPoint, true);
                    }
                    else
                    {
                        m_RightHand_HighlightTarget = targetObject;
                        m_RightHand_HighlightTarget.Action_Highlight(targetPoint, true);
                    }
                }
            }
            else
            {
                if (targetPoint.BitHand == C_LEFT)
                {
                    if(m_LeftHand_HighlightTarget)
                        m_LeftHand_HighlightTarget.Action_Highlight(targetPoint, false);
                }
                if (targetPoint.BitHand == C_RIGHT)
                {
                    if (m_RightHand_HighlightTarget)
                        m_RightHand_HighlightTarget.Action_Highlight(targetPoint, false);
                }
            }
        }
    }
}
