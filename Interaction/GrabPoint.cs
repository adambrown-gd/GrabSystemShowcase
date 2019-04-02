using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ACSL.Network;
using ACSL.Utility;
using ACSL.ControllerInput;
using ACSL.Game;

using Photon.Pun;

namespace ACSL.Interaction
{
    [RequireComponent(typeof(PhotonView)), RequireComponent(typeof(NetSync)), RequireComponent(typeof(CapsuleCollider))]
    public class GrabPoint : MonoBehaviour
    {
        #region PUBLIC VARIABLES

        public bool m_Disabled = false;
        public Transform m_ObjectPosition;
        public Transform m_HandModel;

        #endregion
        #region PRIVATE VARIABLES

#pragma warning disable 0414
        private PhotonView m_PV;
        private NetSync m_NetSync;
#pragma warning restore 0414

        [PhotonData(typeof(int), PhotonData.PhotonDataType.Consistent)]
        private int m_HighlightTarget_ViewID = -1;
        [PhotonData(typeof(int), PhotonData.PhotonDataType.Consistent)]
        private int m_GrabbedObject_ViewID = -1;
        [PhotonData(typeof(int), PhotonData.PhotonDataType.Consistent)]
        private int m_GrabState = 0x0;

        private Timer m_HoldTimer;
        private GrabObject m_HoldTarget;

        private bool m_ResetHoldGrab;
        private ConfigurableJoint m_Joint;

        #endregion
        #region PUBLIC METHODS

        /// <summary>
        /// This function tests whether an object can be grabbed or not.
        /// Called by Keybinding_GrabObject.
        /// 
        /// Adam Brown - Last Updated: 03/26/2019
        /// </summary>
        /// <param name="kpt"></param>
        public void Action_GrabQuery(KeyPressTypes kpt)
        {
            if (kpt == KeyPressTypes.Down)
            {
                if (!GrabbedObject && !HighlightTarget)
                    return;

                //not grabbing, but highlighting = we want to try grabbing an object
                if (!Grabbing && HighlightTarget)
                {
                    //the object is grab once, call the function and do nothing else.
                    //note - Action_GrabOnce unhighlights, no need to do it here

                    if (HighlightTarget is InteractObject)
                    {
                        HighlightTarget.Action_Grab(this);
                        return;
                    }
                    //the object is highlight only, do nothing
                    else if (HighlightTarget.HighlightOnly)
                        return;
                    //else, start a hold grab

                    else if (HighlightTarget is GrabObject)
                    {
                        GrabObject targetObject = HighlightTarget as GrabObject;
                        m_HoldTarget = targetObject;

                        if (!targetObject.m_HoldToGrab)
                        {
                            CallGrab(1);
                            return;
                        }
                        else
                        {
                            m_HoldTimer.NewTimer(targetObject.HoldToGrabTime);
                            m_HoldTimer.StartTimer();
                        }
                    }
                }
            }
            else if (kpt == KeyPressTypes.Up)
            {
                if (!GrabbedObject && !HighlightTarget)
                    return;

                //we are grabbing, which means we want to drop an object
                if (Grabbing)
                {
                    GrabObject target = GrabbedObject;

                    //ungrab like normal
                    GrabbedObject.Action_Grab(this, false);

                    if (target is DynamicObject)
                    {
                        DynamicObject dynamicTarget = target as DynamicObject;

                        //does the object we were holding want to enter a snapzone? snap it
                        if (dynamicTarget.SnapTarget != null)
                            dynamicTarget.Action_Snap(this, dynamicTarget.SnapTarget, true);
                    }
                }
                else
                {
                    if (HighlightTarget)
                    {
                        if (HighlightTarget.HighlightOnly || HighlightTarget is InteractObject)
                            return;

                        m_HoldTimer.Stop();

                        if (HighlightTarget is GrabObject)
                            if (HoldTarget == HighlightTarget)
                                HighlightTarget.Action_Grab(this, true);
                    }
                }
            }
        }
        public void Action_Reset()
        {
            m_GrabState = 0;
            m_Disabled = false;

            m_GrabbedObject_ViewID = -1;
            m_HighlightTarget_ViewID = -1;
        }
        /// <summary>
        /// Call to vibrate a ViveController. Reference 
        /// See SteamVR documentation for vibration strength examples
        /// 
        /// Adam Brown - Last Updated: 03/26/2019
        /// </summary>
        /// <param name="strength"></param>
        public void VibrateController(ushort strength = 100)
        {
            if (!GameUtils.VRActive)
                return;

            SteamVR_TrackedObject obj = GetComponent<SteamVR_TrackedObject>();
            ViveController vController = GameUtils.LocalController as ViveController;
            vController.VibrateController(obj == vController.m_LeftControllerTrackedObject ? InputAction.C_LEFT : obj == vController.m_RightControllerTrackedObject ? InputAction.C_RIGHT : 0, strength);
        }
        /// <summary>
        /// Called by a timer to handle hold grab mechanics.
        /// 
        /// Adam Brown - Last Updated: 03/26/2019
        /// </summary>
        /// <param name="p"></param>
        public void CallGrab(float p)
        {
            if (m_HoldTarget == HighlightTarget)
            {
                if (m_HoldTarget is DynamicObject)
                {
                    DynamicObject dynamicTarget = m_HoldTarget as DynamicObject;

                    if (dynamicTarget.Snapped)
                        dynamicTarget.Action_Snap(this, dynamicTarget.SnapParent, false);
                }

                HoldTarget.Action_Grab(this, true);
            }
        }
        /// <summary>
        /// Call to disengage the hand from grabbing anything.
        /// 
        /// Adam Brown - Last Updated: 03/26/2019
        /// </summary>
        public void ClearHand()
        {
            if (GrabbedObject != null)
            {
                if (GrabbedObject.Grabbed)
                    GrabbedObject.Action_Grab(this, false);

                m_HandModel.transform.SetParent(transform);
                m_HandModel.transform.localPosition = Vector3.zero;
                m_HandModel.transform.localEulerAngles = Vector3.zero;
            }
            if (HighlightTarget != null)
            {
                if (HighlightTarget.Highlighted)
                    HighlightTarget.Action_Highlight(this, false);
            }
        }

        #endregion
        #region PRIVATE METHODS

        private void PhotonJoinRoom()
        {
            PhotonRoomManager.Instance.InitFields(this);
        }
        protected virtual void CleanUp()
        {
            GameManager.Instance.OnSceneCleanUp -= CleanUp;

            GameManager.Instance.NetworkManager.PhotonLink.JoinedRoom -= PhotonJoinRoom;
        }
        private void Awake()
        {
            if (GameUtils.LocalController == null)
            {
                BaseController.OnInputStart += bc =>
                {
                    m_HoldTimer = gameObject.AddComponent<Timer>();
                    m_HoldTimer.NewTimer(0);
                    m_HoldTimer.OnTimerEnd += CallGrab;
                };
            }
            else
            {
                m_HoldTimer = gameObject.AddComponent<Timer>();
                m_HoldTimer.NewTimer(0);
                m_HoldTimer.OnTimerEnd += CallGrab;
            }

            m_Joint = GetComponent<ConfigurableJoint>();

            HandModel hm = m_HandModel.gameObject.GetComponent<HandModel>();
            hm.OriginalPosition = m_HandModel.transform.localPosition;
            hm.OriginalRotation = m_HandModel.transform.localEulerAngles;

            m_PV = this.GetComponent<PhotonView>();
            m_NetSync = this.GetComponent<NetSync>();

            //Subs
            GameManager.Instance.NetworkManager.PhotonLink.JoinedRoom += PhotonJoinRoom;
            GameManager.Instance.OnSceneCleanUp += CleanUp;
        }
        private void OnJointBreak(float breakForce)
        {
            GrabbedObject.Action_Grab(this, false);
        }
        private void OnTriggerEnter(Collider other)
        {
            GrabBase obj = null;

            //return if the GrabPoint is disabled
            if (m_Disabled)
                return;

            if (other.GetComponent<GrabBase>() != null)
                obj = other.GetComponent<GrabBase>();
            else return;

            if (obj is GrabObject)
            {
                GrabObject grabObj = obj as GrabObject;

                if (!grabObj.Grabbed)
                    grabObj.Action_Highlight(this, true);
            }
            else
            {
                if (HighlightTarget)
                    HighlightTarget.Action_Highlight(this, false);

                obj.Action_Highlight(this, true);
            }
        }
        private void OnTriggerExit(Collider other)
        {

            GrabBase obj = null;

            //return if the GrabPoint is disabled
            if (m_Disabled)
                return;

            if (other.GetComponent<GrabBase>() != null)
                obj = other.GetComponent<GrabBase>();
            else return;

            if (m_HoldTarget && m_HoldTarget == obj)
                return;

            obj.Action_Highlight(this, false);
        }

        public void Debug_MouseEnter(GrabBase targetObject)
        {
            if (GameUtils.VRActive)
                return;

            //return if the GrabPoint is disabled
            if (m_Disabled)
                return;

            if (targetObject is GrabObject)
                if (!(targetObject as GrabObject).Grabbed)
                    targetObject.Action_Highlight(this, true);
        }

        public void Debug_MouseExit(GrabBase targetObject)
        {
            if (GameUtils.VRActive)
                return;

            //return if the GrabPoint is disabled
            if (m_Disabled)
                return;

            if (m_HoldTarget && m_HoldTarget == targetObject)
                return;

            targetObject.Action_Highlight(this, false);
        }
        #endregion
        #region ACCESSORS

        public Collider TriggerCollider
        {
            get { return GetComponent<Collider>(); }
        }

        #region PHOTON ACCESSORS

        public int ViewID
        {
            get { return GetComponent<PhotonView>().ViewID; }
        }
        public int HighlightTarget_ViewID
        {
            get { return m_HighlightTarget_ViewID; }
            set { m_HighlightTarget_ViewID = value; }
        }
        public GrabBase HighlightTarget
        {

            get { return m_HighlightTarget_ViewID == -1 ? null : PhotonView.Find(m_HighlightTarget_ViewID).GetComponent<GrabBase>(); }
            set { m_HighlightTarget_ViewID = value != null ? value.ViewID : -1; }
        }
        public int GrabbedObject_ViewID
        {
            get { return m_GrabbedObject_ViewID; }
            set { m_GrabbedObject_ViewID = value; }
        }
        public GrabObject GrabbedObject
        {

            get { return m_GrabbedObject_ViewID == -1 ? null : PhotonView.Find(m_GrabbedObject_ViewID).GetComponent<GrabObject>(); }
            set { m_GrabbedObject_ViewID = value != null ? value.ViewID : -1; }
        }

        #endregion
        #region BITWISE BITS

        public bool Grabbing
        {
            get { return (m_GrabState & 1) != 0; }
            set { m_GrabState = value ? m_GrabState | 1 : (m_GrabState & 1) != 0 ? m_GrabState ^ 1 : m_GrabState; }
        }
        public bool HoldGrab
        {
            get { return (m_GrabState & 1) != 0; }
            set { m_GrabState = value ? m_GrabState | 2 : (m_GrabState & 2) != 0 ? m_GrabState ^ 2 : m_GrabState; }
        }
        public int BitHand
        {
            get { return GameUtils.VRActive ? GetComponent<SteamVR_TrackedObject>() == (GameUtils.LocalController as ViveController).m_LeftControllerTrackedObject ? InputAction.C_LEFT : InputAction.C_RIGHT : -1; }
        }
        public static GrabPoint LeftHand
        {
            get { return GameUtils.VRActive ? (GameUtils.LocalController as ViveController).m_LeftControllerTrackedObject.GetComponent<GrabPoint>() : null; }
        }
        public static GrabPoint RightHand
        {
            get { return GameUtils.VRActive ? (GameUtils.LocalController as ViveController).m_RightControllerTrackedObject.GetComponent<GrabPoint>() : null; }
        }
        public static GrabPoint DebugHand
        {
            get { return GameUtils.VRActive ? LeftHand : GameUtils.LocalController.GetComponentInChildren<GrabPoint>(true); }
        }

        public GrabObject HoldTarget
        {
            get { return m_HoldTarget; }
            set { m_HoldTarget = value; }
        }
        public Timer HoldTimer
        {
            get { return m_HoldTimer; }
        }
        public ConfigurableJoint GrabJoint
        {
            get { return m_Joint; }
        }

        #endregion

        #endregion
    }
}
