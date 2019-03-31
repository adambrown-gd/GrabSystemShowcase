using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.Events;

using ACSL.Network;
using ACSL.Utility;
using ACSL.ControllerInput;
using ACSL.Game;

using Photon.Pun;

namespace ACSL.Interaction
{
    /// <summary>
    /// Volumes that DynamicObjects can be placed/thrown into. 
    /// 
    /// Adam Brown - Last Updated: 03/26/2019
    /// </summary>
    [RequireComponent(typeof(PhotonView)), RequireComponent(typeof(NetSync)), RequireComponent(typeof(Collider)), RequireComponent(typeof(Rigidbody))]
    public class Snapzone : MonoBehaviour
    {
        #region PUBLIC VARIABLES

        [Tooltip("Set to true if you want this snapzone to pre-snap an object on Awake.")]
        public bool m_SnapOnAwake;

        [Tooltip("The object to pre-snap.")]
        public DynamicObject m_SnapOnAwakeObject;

        [Tooltip("The Transform the snapped object will be set to.")]
        public Transform m_SnappedObjectTransform;

        //Occurs right after an object is snapped into the snapzone.
        public GrabbableEvent OnSnapBegin;

        //Occurs right after an object is unsnapped from the snapzone.
        public GrabbableEvent OnSnapEnd;

        #endregion
        #region PRIVATE VARIABLES

#pragma warning disable 0414
        private PhotonView m_PV;
        private NetSync m_NetSync;
#pragma warning restore 0414

        [PhotonData(typeof(int), PhotonData.PhotonDataType.Consistent)]
        protected int m_SnappedObject_ViewID;
        [PhotonData(typeof(int), PhotonData.PhotonDataType.Consistent)]
        protected int m_HighlightTarget_ViewID;
        [PhotonData(typeof(int), PhotonData.PhotonDataType.Consistent)]
        protected int m_GrabState = 0x0;

        //The timer to lerp objects into the snapzone.
        protected Timer m_LerpTimer;

        //the joint for connecting objects.
        protected ConfigurableJoint m_Joint;

        #endregion
        #region PUBLIC METHODS

        /// <summary>
        /// Calls OnSnapBegin event.
        /// Called at the end of GrabbableObject.Action_Snap().
        /// Adam Brown
        /// </summary>
        /// <param name="targetObject"></param>
        public void EventCallback_OnSnapBegin(GrabBase targetObject)
        {
            OnSnapBegin.Invoke(targetObject);
        }

        /// <summary>
        /// Calls OnSnapEnd event.
        /// Called at the end of GrabbableObject.Action_Snap().
        /// Adam Brown
        /// </summary>
        /// <param name="targetObject"></param>
        public void EventCallback_OnSnapEnd(GrabBase targetObject)
        {
            OnSnapEnd.Invoke(targetObject);
        }

        /// <summary>
        /// Adds a connecting joint to the snapzone, using the snapped object as the connecting body
        /// Called by m_LerpTimer.OnTimerEnd().
        /// </summary>
        /// <param name="p"></param>
        public virtual void ConnectJoint(float p)
        {
            m_Joint = gameObject.AddComponent<ConfigurableJoint>();

            m_Joint.breakForce = Mathf.Infinity;
            m_Joint.enableCollision = false;
            m_Joint.enablePreprocessing = false;

            m_Joint.anchor = Vector3.zero;
            m_Joint.connectedAnchor = SnappedObject.transform.InverseTransformPoint(m_SnappedObjectTransform.position);
            m_Joint.connectedBody = SnappedObject.GetComponent<Rigidbody>();

            m_Joint.autoConfigureConnectedAnchor = false;
            m_Joint.axis = Vector3.zero;

            m_Joint.angularXMotion = ConfigurableJointMotion.Free;
            m_Joint.angularYMotion = ConfigurableJointMotion.Free;
            m_Joint.angularZMotion = ConfigurableJointMotion.Free;

            m_Joint.xMotion = ConfigurableJointMotion.Limited;
            m_Joint.yMotion = ConfigurableJointMotion.Limited;
            m_Joint.zMotion = ConfigurableJointMotion.Limited;

            SnappedObject.OnGrabBegin += ReleaseSnap;
        }

        /// <summary>
        /// Removes the connecting joint for the snapped object.
        /// Called by GrabbableObject.Action_Snap().
        /// </summary>
        public void DisconnectJoint()
        {
            if (m_Joint)
                Destroy(m_Joint);
        }

        /// <summary>
        /// Begins the lerping the object's position to the snapzone.
        /// Called at the end of GrabbableObject.Action_Snap().
        /// Adam Brown
        /// </summary>
        public void SnapzoneLerp()
        {
            m_LerpTimer.NewTimer(2.0f);
            m_LerpTimer.StartTimer();
        }

        /// <summary>
        /// Checks if the object we're pre-snapping is initialized, and waits if it isn't.
        /// </summary>
        /// <param name="bc"></param>
        public void Action_PresnapQuery(BaseController bc)
        {
            if (!m_SnapOnAwakeObject.InitComplete)
                m_SnapOnAwakeObject.OnInitialized += Action_Presnap;
            else
                Action_Presnap(m_SnapOnAwakeObject);
        }

        /// <summary>
        /// Snaps an object in the initial setup for the scene.
        /// Called by Snapzone.Init()
        /// </summary>
        /// <param name="targetObject"></param>
        public void Action_Presnap(GrabBase targetObject)
        {
            bool disableOnStart = false;
            if (!m_SnapOnAwakeObject.EnableInteract)
            {
                m_SnapOnAwakeObject.EnableInteract = true;
                disableOnStart = true;
            }

            m_SnapOnAwakeObject.SnapTarget = this;
            m_SnapOnAwakeObject.Action_Snap(GameUtils.VRActive ? GrabPoint.RightHand : GrabPoint.DebugHand, this, true);

            BaseController.OnInputStart -= Action_PresnapQuery;

            if (disableOnStart)
                m_SnapOnAwakeObject.EnableInteract = false;

            Debug.Log("Presnapped " + m_SnapOnAwakeObject.name + " to " + gameObject.name);
        }

        /// <summary>
        /// Tests if an object can be snapped or unsnapped from this snapzone, and sets highlighting flags.
        /// Called by Snapzone.OnTriggerEnter() and Snapzone.OnTriggerExit()
        /// When overriding, called base.Action_SnapQuery() AFTER the overridden functionality.
        /// </summary>
        /// <param name="targetPoint"></param>
        /// <param name="enter"></param>
        public virtual void Action_SnapQuery(GrabPoint targetPoint, bool enter)
        {
            if (!targetPoint.Grabbing || Snapping)
                return;

            if (targetPoint.GrabbedObject is StaticObject)
                return;

            //Grabbed Object
            DynamicObject grabbedObject = targetPoint.GrabbedObject as DynamicObject;
            grabbedObject.Action_Highlight(targetPoint, enter);
            grabbedObject.SnapTarget = enter ? this : null;

            //GrabPoint
            targetPoint.HighlightTarget = null;

            //Snapzone
            Highlighting = enter;
            HighlightTarget = enter ? grabbedObject : null;
        }

        #endregion
        #region PROTECTED METHODS


        private void PhotonJoinRoom()
        {
            PhotonRoomManager.Instance.InitFields(this);
        }
        protected virtual void CleanUp()
        {
            GameManager.Instance.OnSceneCleanUp -= CleanUp;

            GameManager.Instance.NetworkManager.PhotonLink.JoinedRoom -= PhotonJoinRoom;

            OnSnapBegin.RemoveAllListeners();
            OnSnapEnd.RemoveAllListeners();
        }
        /// <summary>
        /// Called by the GrabEnd event of the DynamicObject
        /// in the snapzone.
        /// 
        /// Adam Brown - Last Updated: 03/26/2019
        /// </summary>
        /// <param name="targetObject"></param>
        protected virtual void ReleaseSnap(GrabObject targetObject)
        {
            DynamicObject dynamicTarget = targetObject as DynamicObject;

            dynamicTarget.Action_Snap(targetObject.GrabParent, this, false);
            dynamicTarget.OnGrabEnd -= ReleaseSnap;
        }
        protected void OnTriggerEnter(Collider other)
        {
            if (other.GetComponent<GrabPoint>())
                Action_SnapQuery(other.GetComponent<GrabPoint>(), true);

            if (other.GetComponent<GrabObject>())
            {
                if (other.GetComponent<GrabObject>().Grabbed)
                {
                    Action_SnapQuery(other.GetComponent<GrabObject>().GrabParent, true);

                    if (other.GetComponent<ThrownObject>())
                        other.GetComponent<ThrownObject>().Remove(null);
                }
                else if (other.GetComponent<ThrownObject>())
                {
                    if (!other.GetComponent<GrabObject>().Grabbed && !Snapping)
                    {
                        if (other.GetComponent<DynamicObject>())
                        {
                            other.GetComponent<DynamicObject>().Action_Snap(other.GetComponent<ThrownObject>().m_ThrowPoint, this, true);
                            other.GetComponent<ThrownObject>().Remove(null);
                        }
                    }
                }
            }
        }
        protected void OnTriggerExit(Collider other)
        {

            if (other.GetComponent<GrabPoint>())
                Action_SnapQuery(other.GetComponent<GrabPoint>(), false);

            if (other.GetComponent<GrabObject>())
                if (other.GetComponent<GrabObject>().Grabbed)
                    Action_SnapQuery(other.GetComponent<GrabObject>().GrabParent, false);
        }
        private void Awake()
        {
            m_PV = this.GetComponent<PhotonView>();
            m_NetSync = this.GetComponent<NetSync>();

            //Subs
            GameManager.Instance.NetworkManager.PhotonLink.JoinedRoom += PhotonJoinRoom;
            GameManager.Instance.OnSceneCleanUp += CleanUp;
        }
        private void Start()
        {
            m_SnappedObject_ViewID = -1;
            m_HighlightTarget_ViewID = -1;

            m_GrabState = 0;


            m_LerpTimer = gameObject.AddComponent<Timer>();
            m_LerpTimer.NewTimer(3.0f);
            m_LerpTimer.OnTimerUpdate += p =>
            {
                SnappedObject.transform.position = MathUtils.Vector3Lerp(SnappedObject.transform.position, m_SnappedObjectTransform.transform.position, p);
                SnappedObject.transform.rotation = Quaternion.Slerp(SnappedObject.transform.rotation, m_SnappedObjectTransform.transform.rotation, p);
            };
            m_LerpTimer.OnTimerEnd += ConnectJoint;


            //if no transform was given, use the snapzones.
            if (!m_SnappedObjectTransform)
            {
                m_SnappedObjectTransform = transform;
                Debug.LogWarning("Warning: " + gameObject.name + " Snapzone component was not given a Transform target for objects. \nUsing the Snapzone's transform, which may yield weird results.");
            }

            //if we're snapping on awake, check if the player is initialized first. Then call PresnapQuery.
            if (m_SnapOnAwake)
            {
                if (GameUtils.LocalController != null)

                    Action_PresnapQuery(null);
                else

                    BaseController.OnInputStart += Action_PresnapQuery;
            }

        }

        protected void Update()
        {
            //for extra spinny goodness.
            //in development

            //if (Snapping && m_LerpTimer.Finished)
            //{
            //    Vector3 dir = (SnappedObject.transform.position - m_SnappedObjectTransform.position);
            //    float distance = Vector3.Distance(SnappedObject.transform.position, m_SnappedObjectTransform.position);
            //    float perc = Mathf.Clamp(distance / 1, 0, 1);
            //    float forceMult = 3f;

            //    SnappedObject.GetComponent<Rigidbody>().AddForce(-dir.normalized * (forceMult * perc), ForceMode.Force);
            //}
        }
        #endregion
        #region ACCESSORS

        public Timer LerpTimer
        {
            get { return m_LerpTimer; }
            set { m_LerpTimer = value; }
        }
        public Collider TriggerCollider
        {
            get { return GetComponent<Collider>(); }
        }
        public int ViewID
        {
            get { return GetComponent<PhotonView>().ViewID; }
        }
        public int HighlightTarget_ViewID
        {
            get { return m_HighlightTarget_ViewID; }
            set { m_HighlightTarget_ViewID = value; }
        }

        public int SnappedObject_ViewID
        {

            get { return m_SnappedObject_ViewID; }
            set { m_SnappedObject_ViewID = value; }
        }
        public DynamicObject SnappedObject
        {
            get { return m_SnappedObject_ViewID == -1 ? null : PhotonView.Find(m_SnappedObject_ViewID).GetComponent<DynamicObject>(); }
            set { m_SnappedObject_ViewID = value != null ? value.ViewID : -1; }
        }
        public DynamicObject HighlightTarget
        {

            get { return m_HighlightTarget_ViewID == -1 ? null : PhotonView.Find(m_HighlightTarget_ViewID).GetComponent<DynamicObject>(); }
            set { m_HighlightTarget_ViewID = value != null ? value.ViewID : -1; }
        }

        #region BITWISE BOOLS

        /// <summary>
        /// Snapping tests separately from GrabbableObjects bits, so we can double up here.
        /// Adam Brown - 1/1/2019
        /// </summary>
        public bool Snapping
        {
            get { return (m_GrabState & 1) != 0; }
            set { m_GrabState = value ? m_GrabState | 1 : (m_GrabState & 1) != 0 ? m_GrabState ^ 1 : m_GrabState; }
        }
        public bool Highlighting
        {
            get { return (m_GrabState & 2) != 0; }
            set { m_GrabState = value ? m_GrabState | 2 : (m_GrabState & 2) != 0 ? m_GrabState ^ 2 : m_GrabState; }
        }

        #endregion
        #endregion
    }
}
