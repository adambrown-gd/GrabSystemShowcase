using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;

using ACSL.ControllerInput;
using ACSL.Utility;

namespace ACSL.Interaction
{
    /// <summary>
    /// The class used for generic props. Can be snapped 
    /// into Snapzones.
    /// 
    /// Adam Brown - Last Updated: 03/26/2019
    /// </summary>
    public class DynamicObject : GrabObject
    {
        #region EVENTS

        public event EventHandler_GrabBase OnSnapBegin;
        public event EventHandler_GrabBase OnSnapEnd;

        #endregion
        #region PUBLIC VARIABLES

        //DEBUG
        public bool m_DebugMouseGrab = false;

        #endregion
        #region PROTECTED VARIABLES

        //ViewID trackers
        [PhotonData(typeof(int), PhotonData.PhotonDataType.Consistent)]
        protected int m_SnapTarget_ViewID = -1;
        [PhotonData(typeof(int), PhotonData.PhotonDataType.Consistent)]
        protected int m_SnapParent_ViewID = -1;

        //Variables to track hand lerping
        protected Timer m_HandLerp_Timer;
        protected Vector3 m_HandLerp_Position;
        protected Vector3 m_HandLerp_Rotation;

        //Variables to track rigidbody info
        protected JointData m_JointData;
        protected Rigidbody m_Rigidbody;
        protected Collider m_RigidbodyCollider;

        //the way the object is held in a GrabPoint;
        public Vector3 m_ObjectPosition;

        public bool m_UseTargetRotation;
        public Vector3 m_ObjectRotation;

        #endregion

        public override void OnDrawGizmosSelected()
        {
            base.OnDrawGizmosSelected();
            if (m_ShowWireframe)
            {
                if (RigidbodyCollider)
                {
                    Gizmos.color = Color.red;

                    if (RigidbodyCollider is BoxCollider)
                        Gizmos.DrawWireCube(RigidbodyCollider.bounds.center, RigidbodyCollider.bounds.size);
                    else if (RigidbodyCollider is SphereCollider)
                        Gizmos.DrawWireSphere(RigidbodyCollider.bounds.center, (RigidbodyCollider as SphereCollider).radius);
                }
            }
        }
        protected override void Action_DetailedGrab(GrabPoint targetPoint, bool grab = true, bool forceGrab = false)
        {
            //toggle locomotion on the grab hand if the object can be used.
            if (IsUseable)
                InputManager.Instance.ToggleKeybindingOnHand(typeof(Keybinding_Locomotion), targetPoint.BitHand, !grab);

            //hand and object animation
            {
                if (GameUtils.VRActive)
                    targetPoint.m_HandModel.GetComponent<HandModel>().ToggleBool("Grabbing", grab);

                if (grab)
                {
                    Physics.IgnoreCollision(targetPoint.GetComponents<CapsuleCollider>().First(c => !c.isTrigger), RigidbodyCollider, true);

                    m_HandLerp_Timer.NewTimer(0.4f);
                    m_HandLerp_Timer.StartTimer();
                }
                else
                {
                    if (m_HandLerp_Timer.Started)
                        m_HandLerp_Timer.Stop();

                    if (m_JointData.IsConnected())
                        m_JointData.Disconnect();
                }
            }

            //throwing 
            if (!grab && GameUtils.VRActive)
                Action_Throw(targetPoint);
        }
        protected void Action_Throw(GrabPoint targetPoint)
        {
            Rigidbody rBody = GetComponent<Rigidbody>();
            ViveController vController = GameUtils.LocalController as ViveController;
            SteamVR_Controller.Device vDevice = ((targetPoint.BitHand & InputAction.C_LEFT) != 0 ? vController.Device_LeftController : vController.Device_RightController);

            ////get the point on the rigidbody collider the closest to the GrabPoint's forward.
            //Vector3 forwardClosest = targetPoint.GetComponents<CapsuleCollider>().First(c => !c.isTrigger).ClosestPoint(targetPoint.transform.position + (targetPoint.transform.forward * 10));
            ////get the point in world space where the object can be next to the GrabPoint's rigidbody collider without actually colliding
            //Vector3 ClosestNonTouching = forwardClosest + RigidbodyCollider.bounds.extents;

            //transform.position = ClosestNonTouching;

            rBody.velocity = targetPoint.GetComponent<SteamVR_TrackedObject>().origin.TransformVector(vDevice.velocity);
            rBody.GetRelativePointVelocity(targetPoint.GetComponent<SteamVR_TrackedObject>().origin.TransformVector(vDevice.angularVelocity));

            rBody.AddForce(vDevice.velocity);
            rBody.angularVelocity = vDevice.angularVelocity;

            if (RigidbodyCollider)
            {
                //if we have a thrown component on already, remove it.
                GetComponent<ThrownObject>()?.Remove(this);

                //create ThrownObject component
                ThrownObject thrown = gameObject.AddComponent<ThrownObject>();
                thrown.m_ThrowOrigin = transform.position;
                thrown.m_Thrower = GetComponent<PhotonView>().Owner;
                thrown.m_ThrowPoint = targetPoint;
            }
        }
        public override void Action_Reset(bool clearHands = false)
        {
            base.Action_Reset(clearHands);

            Snapped = false;

            m_SnapTarget_ViewID = -1;
            m_SnapParent_ViewID = -1;

            //disconnect joints?
        }
        /// <summary>
        /// Allows DynamicObjects to be snapped into snapzones.
        /// 
        /// Adam Brown - Last Updated: 03/26/2019
        /// </summary>
        /// <param name="targetPoint">The hand that triggered the snap</param>
        /// <param name="snapzone">The snapzone in question</param>
        /// <param name="snap"> Are we snapping or unsnapping?</param>
        public void Action_Snap(GrabPoint targetPoint, Snapzone snapzone, bool snap)
        {
            if (!EnableInteract || !targetPoint || !snapzone)
                return;

            Snapped = snap;
            snapzone.Snapping = snap;

            SnapParent = snap ? SnapTarget : null;
            SnapTarget = null;
            snapzone.SnappedObject = snap ? this : null;

            snapzone.Highlighting = false;
            snapzone.HighlightTarget = null;

            //GetComponent<NetSync>().ParentThisObject(snap ? snapzone.transform : null);
            GetComponent<NetSync>().ReplicateGrabState(GetComponent<Rigidbody>().isKinematic, !snap, true);

            if (snap)
            {
                snapzone.SnapzoneLerp();
                snapzone.EventCallback_OnSnapBegin(this);

                SnapBegin(snapzone);
                EventCallback_SnapBegin();
                GrabBoundsTest(targetPoint, targetPoint.TriggerCollider.bounds);
            }
            else
            {
                if (snapzone.LerpTimer.Started)
                    snapzone.LerpTimer.Stop();

                snapzone.DisconnectJoint();
                snapzone.EventCallback_OnSnapEnd(this);

                SnapEnd(snapzone);
                EventCallback_SnapEnd();
            }

            if (m_Debug)
                Debug.Log(gameObject.name + " - Snapped: " + snap + " - Snapzone: " + snapzone.name);
        }
        /// <summary>
        /// Connects a DynamicObject to the current GrabParent.
        /// 
        /// Adam Brown - Last Updated 03/26/2019
        /// </summary>
        /// <param name="p"></param>
        protected void ConnectJoint(float p)
        {
            m_JointData.Connect(GetComponent<Rigidbody>(), GrabParent);
        }
        protected override void Init()
        {
            base.Init();

            m_JointData = new JointData();

            if (transform.Find("RigidbodyCollider"))
            {
                m_RigidbodyCollider = transform.Find("RigidbodyCollider").GetComponent<Collider>();
                m_RigidbodyCollider.gameObject.layer = LayerMask.NameToLayer("Interactable");
                m_RigidbodyCollider.gameObject.tag = "Interactable";
            }

            if (!RigidbodyCollider)
            {
                enabled = false;
                throw new MissingReferenceException(gameObject.name + " missing RigidbodyCollider child GameObject! Did not initialize.");
            }

            m_HandLerp_Timer = gameObject.AddComponent<Timer>();
            m_HandLerp_Timer.NewTimer(0.4f);
            m_HandLerp_Timer.OnTimerUpdate += p =>
            {
                if (Grabbed)
                {
                    transform.position = MathUtils.Vector3Lerp(transform.position, GrabParent.transform.TransformPoint(m_ObjectPosition), p);
                    if (m_UseTargetRotation)
                        transform.rotation = Quaternion.Slerp(transform.rotation, GrabParent.transform.rotation * Quaternion.Euler(m_ObjectRotation), p);
                }
            };
            m_HandLerp_Timer.OnTimerEnd += ConnectJoint;

            m_Rigidbody = GetComponent<Rigidbody>();

            IsKinematic = m_Rigidbody != null ? m_Rigidbody.isKinematic : false;
            UsesGravity = m_Rigidbody != null ? m_Rigidbody.useGravity : false;
        }
        protected override void CleanUp()
        {
            base.CleanUp();

            Delegate.RemoveAll(OnSnapBegin, OnSnapEnd);
            Delegate.RemoveAll(OnSnapEnd, OnSnapEnd);
        }
        #region ACCESSORS

        public JointData ConnectedJoint
        {
            get { return m_JointData; }
        }

        #endregion
        #region EVENT CALLBACKS

        public void EventCallback_SnapBegin()
        {
            OnSnapBegin?.Invoke(this);
        }
        public void EventCallback_SnapEnd()
        {
            OnSnapEnd?.Invoke(this);
        }

        #endregion
        #region INTERNAL FUNCTION OVERRIDES

        /// <summary>
        /// Called by Action_Snap().
        /// Called when first snapping an object.
        /// </summary>
        /// <param name="targetPoint">The triggering GrabPoint</param>
        protected virtual void SnapBegin(Snapzone snapzone) { }
        /// <summary>
        /// Called by Action_Snap().
        /// Called when ending a snap.
        /// </summary>
        /// <param name="targetPoint">The triggering GrabPoint</param>
        protected virtual void SnapEnd(Snapzone snapzone) { }

        #endregion
        #region PHOTON ACCESSORS

        public Collider RigidbodyCollider
        {
            get { return m_RigidbodyCollider; }
        }
        public Snapzone SnapTarget
        {
            get { return m_SnapTarget_ViewID == -1 ? null : PhotonView.Find(m_SnapTarget_ViewID).GetComponent<Snapzone>(); }
            set { m_SnapTarget_ViewID = value != null ? value.ViewID : -1; }
        }
        public Snapzone SnapParent
        {
            get { return m_SnapParent_ViewID == -1 ? null : PhotonView.Find(m_SnapParent_ViewID).GetComponent<Snapzone>(); }
            set { m_SnapParent_ViewID = value != null ? value.ViewID : -1; }
        }

        #endregion
        #region BITWISE BOOLS

        public bool Snapped
        {
            get { return (m_GrabInfo & C_SNAPPED) != 0; }
            set { m_GrabInfo = value ? m_GrabInfo | C_SNAPPED : (m_GrabInfo & C_SNAPPED) != 0 ? m_GrabInfo ^ C_SNAPPED : m_GrabInfo; }
        }
        public bool IsKinematic
        {
            get { return (m_GrabInfo & C_KINEMATIC) != 0; }
            set { m_GrabInfo = value ? m_GrabInfo | C_KINEMATIC : (m_GrabInfo & C_KINEMATIC) != 0 ? m_GrabInfo ^ C_KINEMATIC : m_GrabInfo; }
        }
        public bool UsesGravity
        {
            get { return (m_GrabInfo & C_GRAVITY) != 0; }
            set { m_GrabInfo = value ? m_GrabInfo | C_GRAVITY : (m_GrabInfo & C_GRAVITY) != 0 ? m_GrabInfo ^ C_GRAVITY : m_GrabInfo; }
        }

        #endregion
        #region DEBUG

        private void OnMouseDown()
        {
            GrabPoint.DebugHand.Action_GrabQuery(KeyPressTypes.Down);
            m_DebugMouseGrab = true;
        }
        private void OnMouseUp()
        {
            GrabPoint.DebugHand.Action_GrabQuery(KeyPressTypes.Up);
            m_DebugMouseGrab = false;
        }

        #endregion
    }
}