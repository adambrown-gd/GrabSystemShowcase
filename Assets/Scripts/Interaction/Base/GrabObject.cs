using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;

using Photon.Pun;
using Photon.Realtime;

using ACSL.Game;
using ACSL.Utility;
using ACSL.ControllerInput;
using ACSL.Network;

namespace ACSL.Interaction
{
    /// <summary>
    /// The base class for objects that have a persistent effect 
    /// when interacted with, or grabbed. Contains base functionality
    /// for being grabbed.
    /// 
    /// Adam Brown - Last Updated: 03/26/2019
    /// </summary>
    public abstract class GrabObject : GrabBase
    {
        #region EVENTS

        public event EventHandler_GrabObject OnGrabBegin;
        public event EventHandler_GrabObject OnGrabEnd;

        #endregion
        #region PUBLIC VARIABLES 

        [Tooltip("Set to true to enable grab hold time.")]
        public bool m_HoldToGrab;
        [Tooltip("The amount of time it takes to grab this object.")]
        public float m_HoldToGrabTime = 0.3f;

        #endregion
        #region PROTECTED VARIABLES

        //ViewID trackers
        [PhotonData(typeof(int), PhotonData.PhotonDataType.Consistent)]
        protected int m_GrabParent_ViewID = -1;

        #endregion
        #region PUBLIC METHODS

        /// <summary>
        /// A continued extension of Action_Grab in GrabBase.
        /// Calls generic implementation for GrabObjects, then 
        /// calls non-generic derived functionality.
        /// 
        /// Adam Brown - Last Updated: 03/26/2019
        /// </summary>
        /// <param name="targetPoint"></param>
        /// <param name="grab"></param>
        /// <param name="forceGrab"></param>
        protected abstract void Action_DetailedGrab(GrabPoint targetPoint, bool grab = true, bool forceGrab = false);
        /// <summary>
        /// Extension of GrabBase.Action_Grab(). Calls generic
        /// functionality in GrabObject, then calls derived
        /// functionality through Action_DetailedGrab().
        /// 
        /// Adam Brown - Last Updated: 03/26/2019
        /// </summary>
        /// <param name="targetPoint"></param>
        /// <param name="grab"></param>
        /// <param name="forceGrab"></param>
        protected override void Action_GrabObject(GrabPoint targetPoint, bool grab = true, bool forceGrab = false)
        {
            //toggle the bit flag
            Grabbed = grab;

            //set the parent to the GrabPoint
            GrabParent = grab ? targetPoint : null;

            //set GrabPoint flag
            targetPoint.Grabbing = grab;
            targetPoint.GrabbedObject = grab ? this : null;
            targetPoint.HighlightTarget = null;

            //transfer net ownership
            m_PV.RPC("RPC_UpdateOwner", RpcTarget.All, m_Owner == null ? PhotonNetwork.LocalPlayer.ActorNumber : -1);

            Action_DetailedGrab(targetPoint, grab, forceGrab);

            //internal function calls and event callbacks
            {
                if (grab)
                {
                    targetPoint.HoldTarget = null;

                    Action_Highlight(targetPoint, false);
                    GrabBegin(targetPoint);
                    EventCallback_GrabBegin();
                }
                else
                {
                    GrabEnd(targetPoint);
                    EventCallback_GrabEnd();
                    GrabBoundsTest(targetPoint, targetPoint.TriggerCollider.bounds);
                }
            }

            if (m_Debug)
                Debug.Log(gameObject.name + " - Grabbed: " + grab + " - GrabPoint: " + (targetPoint.BitHand == 1 ? "Left" : targetPoint.BitHand == 2 ? "Right" : "Debug"));
        }
        public override void Action_Reset(bool clearHands = false)
        {
            base.Action_Reset();

            Grabbed = false;
            m_GrabParent_ViewID = -1;
        }

        #endregion
        #region PROTECTED METHODS 

        protected override void Init()
        {
            base.Init();
            IsUseable = GetComponent<Useable>() != null;
        }
        protected override void CleanUp()
        {
            base.CleanUp();

            Delegate.RemoveAll(OnGrabBegin, OnGrabBegin);
            Delegate.RemoveAll(OnGrabEnd, OnGrabEnd);
        }

        #endregion
        #region EVENT CALLBACKS

        public void EventCallback_GrabBegin()
        {
            OnGrabBegin?.Invoke(this);
        }
        public void EventCallback_GrabEnd()
        {
            OnGrabEnd?.Invoke(this);
        }

        #endregion
        #region INTERNAL FUNCTION OVERRIDES

        /// <summary>
        /// Called by Action_Grab().
        /// Called when ending a grab.
        /// </summary>
        /// <param name="targetPoint">The triggering GrabPoint</param>
        protected virtual void GrabBegin(GrabPoint targetPoint) { }
        /// <summary>
        /// Called by Action_Grab().
        /// Called when first grabbing an object.
        /// </summary>
        /// <param name="targetPoint">The triggering GrabPoint</param>
        protected virtual void GrabEnd(GrabPoint targetPoint) { }

        #endregion
        #region ACCESSORS

        public float HoldToGrabTime
        {
            get { return m_HoldToGrabTime; }
        }

        #endregion
        #region PHOTON ACCESSORS

        public GrabPoint GrabParent
        {
            get { return m_GrabParent_ViewID == -1 ? null : PhotonView.Find(m_GrabParent_ViewID).GetComponent<GrabPoint>(); }
            set { m_GrabParent_ViewID = value != null ? value.ViewID : -1; }
        }

        #endregion
        #region BITWISE BOOLS

        public bool Grabbed
        {
            get { return (m_GrabInfo & C_GRABBED) != 0; }
            set { m_GrabInfo = value ? m_GrabInfo | C_GRABBED : (m_GrabInfo & C_GRABBED) != 0 ? m_GrabInfo ^ C_GRABBED : m_GrabInfo; }
        }
        public bool IsUseable
        {
            get { return (m_GrabInfo & C_USEABLE) != 0; }
            set { m_GrabInfo = value ? m_GrabInfo | C_USEABLE : (m_GrabInfo & C_USEABLE) != 0 ? m_GrabInfo ^ C_USEABLE : m_GrabInfo; }
        }

        #endregion


    }
}