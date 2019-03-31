using System;
using System.Linq;
using System.Runtime;
using System.Collections;
using System.Reflection;
using System.Collections.Generic;

using UnityEngine;

using Photon.Pun;
using Photon.Realtime;

using ACSL.Network;
using ACSL.ControllerInput;
using ACSL.Utility;
using ACSL.Game;
using ACSL.HUD;

namespace ACSL.Interaction
{
    /// <summary>
    /// The base class for all grabbable objects.
    /// Contains base set-up, as well as highlighting functionality.
    /// 
    /// Adam Brown - Last Updated 03/26/2019
    /// </summary>
    [RequireComponent(typeof(PhotonView)), RequireComponent(typeof(NetSync)), RequireComponent(typeof(Collider))]
    public abstract class GrabBase : MonoBehaviour
    {
        #region EVENTS

        public event EventHandler_GrabBase OnHighlightBegin;
        public event EventHandler_GrabBase OnHighlightEnd;
        public event EventHandler_GrabBase OnInitialized;

        #endregion
        #region PUBLIC VARIABLES

        //initial states
        public bool m_BeginDisabled;

        //materials to swap
        public List<Renderer> m_SwappedMaterials = new List<Renderer>();

        //debug
        public bool m_ShowWireframe = true;
        public bool m_Debug = false;

        #endregion
        #region PROTECTED VARIABLES

        //bit mask states for Grabbable objects
        protected int m_GrabInfo = 0x0;

        [PhotonData(typeof(int), PhotonData.PhotonDataType.Consistent)]
        protected int m_HighlightTarget_ViewID = -1;

        //reference to the trigger collider to highlight the object.
        protected Collider m_TriggerCollider;

        //internal flags
        protected bool m_InitializedAndReady;

        //network variables
        protected PhotonView m_PV;
        protected NetSync m_NetSync;
        public Player m_Owner;

        #endregion
        #region OWNER RPC UPDATE

        private void PlayerJoinedRoom(Player newPlayer)
        {
            if (PhotonNetwork.IsMasterClient)
                m_PV.RPC("RPC_UpdateOwner", newPlayer, m_Owner == null ? -1 : m_Owner.ActorNumber);
        }
        [PunRPC]
        protected void RPC_UpdateOwner(int a_OwnerID)
        {
            m_Owner = a_OwnerID < 0 ? null : PhotonNetwork.CurrentRoom.GetPlayer(a_OwnerID);
        }

        #endregion
        #region GRAB ACTIONS

        /// <summary>
        /// Resets the base functionality for this GrabObject.
        /// Adam Brown - Last Updated: 3/16/2019
        /// </summary>
        /// <param name="clearHands"></param>
        public virtual void Action_Reset(bool clearHands = false)
        {
            EnableInteract = true;
            m_HighlightTarget_ViewID = -1;
            m_Owner = null;

            if (clearHands)
            {
                if(GameUtils.VRActive)
                {
                    GrabPoint.LeftHand.ClearHand();
                    GrabPoint.RightHand.ClearHand();
                }
                else
                {
                    GrabPoint.DebugHand.ClearHand();
                }
            }

            if (m_Debug)
                Debug.Log(gameObject.name + " - Reset");
        }
        /// <summary>
        /// Toggles the highlight on an object.
        /// 
        /// Adam Brown - Last Updated: 3/16/2019
        /// </summary>
        /// <param name="targetPoint">The GrabPoint that triggered the function</param>
        /// <param name="highlight">Are we highlighting or un-highlighting?</param>
        public void Action_Highlight(GrabPoint targetPoint, bool highlight)
        {
            if (!EnableInteract)
                return;

            if (m_Owner != null && m_Owner != PhotonNetwork.LocalPlayer || targetPoint.GetComponent<PhotonView>().Owner != PhotonNetwork.LocalPlayer)
                return;

            //set emissive values
            foreach (Renderer r in m_SwappedMaterials)
                r.material.SetFloat("_EmissiveValue", highlight ? 1 : 0);

            Highlighted = highlight;
            HighlightTarget = highlight ? HighlightTarget != null ? HighlightTarget : targetPoint : null;

            targetPoint.HighlightTarget = highlight ? this : null;

            //call the respective function, and the event callback.
            if (highlight)
            {
                HighlightBegin(targetPoint);
                EventCallback_HighlightBegin();
            }
            else
            {
                HighlightEnd(targetPoint);
                EventCallback_HighlightEnd();
            }

            if (m_Debug)
                Debug.Log(gameObject.name + " - Highlighted: " + highlight + " - GrabPoint: " + (targetPoint.BitHand == 1 ? "Left" : targetPoint.BitHand == 2 ? "Right" : "Debug"));
        }
        public void Action_Grab(GrabPoint targetPoint, bool grab = true, bool forceGrab = false)
        {
            //return if enable interact is false - if force is set to true, ignore
            if (!EnableInteract && !forceGrab)
                return;

            //Owner is someone other than Local Player             //Interactor != Me
            if (m_Owner != null && m_Owner != PhotonNetwork.LocalPlayer || targetPoint.GetComponent<PhotonView>().Owner != PhotonNetwork.LocalPlayer)
                return;

            if(m_Owner == null)
            {
                m_Owner = PhotonNetwork.LocalPlayer;
                m_PV.RPC("RPC_UpdateOwner", RpcTarget.All, PhotonNetwork.LocalPlayer.ActorNumber);

                m_PV.TransferOwnership(PhotonNetwork.LocalPlayer);
            }
            else if(m_Owner == PhotonNetwork.LocalPlayer)
            {
                m_Owner = null;
                m_PV.RPC("RPC_UpdateOwner", RpcTarget.All, -1);
            }
            

            Action_GrabObject(targetPoint, grab, forceGrab);
        }
        /// <summary>
        /// Abstract function is an extension of Action_Grab().
        /// Calls non-generic code from derived types of GrabBase.
        /// 
        /// Adam Brown - Last Updated: 03/26/2019
        /// </summary>
        /// <param name="targetPoint"></param>
        /// <param name="grab"></param>
        /// <param name="forceGrab"></param>
        protected abstract void Action_GrabObject(GrabPoint targetPoint, bool grab = true, bool forceGrab = false);

        #endregion
        #region PUBLIC METHODS

        public void OnDrawGizmos()
        {
            OnDrawGizmosSelected();
        }
        public virtual void OnDrawGizmosSelected()
        {
            if (m_ShowWireframe)
            {
                if(GetComponent<Collider>())
                {
                    if (GetComponent<Collider>().isTrigger)
                    {
                        Gizmos.color = Color.yellow;
                        if (GetComponent<Collider>() is BoxCollider)
                            Gizmos.DrawWireCube(GetComponent<Collider>().bounds.center, GetComponent<Collider>().bounds.size);
                        else if (GetComponent<Collider>() is SphereCollider)
                            Gizmos.DrawWireSphere(GetComponent<Collider>().bounds.center, GetComponent<SphereCollider>().radius);
                    }
                }
            }
        }
        #region EVENT CALLBACKS

        public void EventCallback_HighlightBegin()
        {
            OnHighlightBegin?.Invoke(this);
        }
        public void EventCallback_HighlightEnd()
        {
            OnHighlightEnd?.Invoke(this);
        }
        public void EventCallback_InitComplete()
        {
            OnInitialized?.Invoke(this);
        }

        #endregion

        #endregion
        #region PROTECTED METHODS

        /// <summary>
        /// Tests whether or not an object should be highlighted
        /// Called at the end of a grab action.
        /// Adam Brown Last Updated: 03/26/2019
        /// </summary>
        /// <param name="targetPoint"></param>
        /// <param name="testBounds"></param>
        protected void GrabBoundsTest(GrabPoint targetPoint, Bounds testBounds)
        {
            if(TriggerCollider.bounds.Intersects(testBounds))
                Action_Highlight(targetPoint, true);
        }
        private void HighlightBoundsTest(GrabPoint targetPoint, Bounds testBounds)
        {
            if (!TriggerCollider.bounds.Intersects(testBounds))
                FixHighlightFlags(targetPoint);
        }
        private void FixHighlightFlags(GrabPoint targetPoint)
        {
            //set emissive values
            foreach (Renderer r in m_SwappedMaterials)
                r.material.SetFloat("_EmissiveValue", 0);

            Highlighted = false;
            HighlightTarget = null;

            HighlightEnd(targetPoint);
            EventCallback_HighlightEnd();

            if (m_Debug)
                Debug.Log(gameObject.name + " - Highlighted: " + false + " - GrabPoint: " + (targetPoint.BitHand == 1 ? "Left" : targetPoint.BitHand == 2 ? "Right" : "Debug"));
        }
        protected virtual void CleanUp()
        {
            GameManager.Instance.OnSceneCleanUp -= CleanUp;

            Delegate.RemoveAll(OnHighlightBegin, OnHighlightBegin);
            Delegate.RemoveAll(OnHighlightEnd, OnHighlightEnd);

            Delegate.RemoveAll(OnInitialized, OnInitialized);
        }
        protected virtual void Update()
        {
            //not implemented.
        }
        protected void FixedUpdate()
        {
            //to help solve lost highlight triggers
            if (Highlighted && HighlightTarget)
            {
                if (!HighlightTarget.HighlightTarget)
                    FixHighlightFlags(HighlightTarget);
                else
                    HighlightBoundsTest(HighlightTarget, TriggerCollider.bounds);
            }
        }
        /// <summary>
        /// Function to override for intialization.
        /// Adam Brown - Last Updated: 03/26/2019
        /// </summary>
        protected virtual void Init() { }
        protected void ResetNetOwnership(Player player)
        {
            if (PhotonNetwork.LocalPlayer.IsMasterClient)
            {
                if (m_PV == null)
                {
                    Debug.Log("Grabbable Object view was null when trying to Reset Net Ownership.");
                    return;
                }

                if (m_PV.Owner == player || m_PV.Owner == null)
                {
                    //Player left while owning object, reset objects owner ids
                    m_PV.TransferOwnership(PhotonNetwork.MasterClient.ActorNumber);
                }
            }
        }
        /// <summary>
        /// Swaps out the current material on an object with a 
        /// Highlight shell material, and stuffs in all textures used.
        /// 
        /// Adam Brown - Last Updated: 03/26/2019
        /// </summary>
        protected virtual void MaterialSwap()
        {
            if (m_SwappedMaterials.Count < 1)
            {
                //if(m_Debug)
                //    Debug.LogWarning("GrabbableObject " + gameObject.name + " has no materials added. Converting all MR's to highlight material.", gameObject);

                m_SwappedMaterials = GetComponentsInChildren<Renderer>(true).ToList();
            }

            foreach (Renderer r in m_SwappedMaterials)
            {
                if (r == null || r is LineRenderer || r.GetComponent<ParticleSystem>() != null)
                    continue;

                Material mat = new Material(InteractionUtils.Material_Highlight);

                //the texture names in the standard shader
                string[] stdTextureNames =
                {
                    "_MetallicGlossMap",
                    "_BumpMap",
                    "_ParallaxMap",
                    "_OcclusionMap",
                    "_EmissionMap"
                };

                //the texture names in our highlight shader
                string[] intTextureNames =
                {
                    "_Metallic",
                    "_NormalMap",
                    "_HeightMap",
                    "_Occlusion",
                    "_Emission"
                };

                //if the main texture isn't null
                if (r.material.HasProperty("_MainTex"))
                {
                    //store the main texture into our material
                    mat.mainTexture = r.material.mainTexture;

                    //iterate through each of the extra textures
                    for (int i = 0; i < 5; i++)
                        //if the specific texture isn't null
                        if (r.material.HasProperty(stdTextureNames[i]))
                            //set the texture to the standard equivalent.
                            mat.SetTexture(intTextureNames[i], r.material.GetTexture(stdTextureNames[i]));
                }

                if (r.material.HasProperty("_Color"))
                    mat.SetColor("_Color", r.material.GetColor("_Color"));
                if(r.material.HasProperty("_Glossiness"))
                    mat.SetFloat("_Smoothness", r.material.GetFloat("_Glossiness"));

                //initialize our EmissiveValue
                mat.SetFloat("_EmissiveValue", 0);

                //save the new material into the renderer.
                r.material = mat;
            }
        }
        protected void Awake()
        {
            //whether or not we start enabled.
            EnableInteract = !m_BeginDisabled;

            //set tag and layer to interactable if it isn't already
            gameObject.tag = "Interactable";
            gameObject.layer = LayerMask.NameToLayer("Interactable");

            //trigger collider setup
            m_TriggerCollider = GetComponent<Collider>();

            if (!TriggerCollider.isTrigger)
                Debug.LogWarning("Expected a collider that is a trigger. Did you forget to set it?", gameObject);

            //material setup
            MaterialSwap();

            //cleanup
            GameManager.Instance.OnSceneCleanUp += CleanUp;
            GameManager.Instance.NetworkManager.PhotonLink.PlayerLeftRoom += ResetNetOwnership;

            m_PV = GetComponent<PhotonView>();
            m_NetSync = GetComponent<NetSync>();

            //Call init, and call the initialized event.
            Init();
            EventCallback_InitComplete();
            m_InitializedAndReady = true;
        }
        protected virtual void Start() { }
        #region OVERRIDES

        /// <summary>
        /// Called by Action_Highlight().
        /// Called when first highlighting and object.
        /// </summary>
        /// <param name="targetPoint">The triggering GrabPoint</param>
        protected virtual void HighlightBegin(GrabPoint targetPoint) { }
        /// <summary>
        /// Called by Action_Highlight().
        /// Called when ending a highlight.
        /// </summary>
        /// <param name="targetPoint">The triggering GrabPoint</param>
        protected virtual void HighlightEnd(GrabPoint targetPoint) { }
        /// <summary>
        /// Called by Action_GrabOnce().
        /// Called when a GrabPoint grabs an OnlyGrabOnce object.
        /// </summary>
        /// <param name="targetPoint">The triggering GrabPoint</param>
        protected virtual void GrabOnce(GrabPoint targetPoint) { }

        #endregion

        #endregion
        #region ACCESSORS

        public Collider TriggerCollider
        {
            get { return m_TriggerCollider; }
        }

        #region PHOTON ACCESSORS

        public int ViewID
        {
            get { return GetComponent<PhotonView>().ViewID; }
        }
        public bool InitComplete
        {
            get { return m_InitializedAndReady; }
        }
        public GrabPoint HighlightTarget
        {
            get { return m_HighlightTarget_ViewID == -1 ? null : PhotonView.Find(m_HighlightTarget_ViewID).GetComponent<GrabPoint>(); }
            set { m_HighlightTarget_ViewID = value != null ? value.ViewID : -1; }
        }

        #endregion
        #region BITWISE CONSTANTS

        public const int C_GRABBED = 1;
        public const int C_HIGHLIGHTED = 2;
        public const int C_SNAPPED = 4;
        public const int C_KINEMATIC = 8;
        public const int C_GRAVITY = 16;
        public const int C_USEABLE = 32;
        public const int C_ENABLE_INTERACT = 64;
        public const int C_GRAB_ONCE = 128;
        public const int C_HIGHLIGHT_ONLY = 256;

        #endregion
        #region BITWISE ACCESSORS


        public bool Highlighted
        {
            get { return (m_GrabInfo & C_HIGHLIGHTED) != 0; }
            set { m_GrabInfo = value ? m_GrabInfo | C_HIGHLIGHTED : (m_GrabInfo & C_HIGHLIGHTED) != 0 ? m_GrabInfo ^ C_HIGHLIGHTED : m_GrabInfo; }
        }
        public bool EnableInteract
        {
            get { return (m_GrabInfo & C_ENABLE_INTERACT) != 0; }
            set { m_GrabInfo = value ? m_GrabInfo | C_ENABLE_INTERACT : (m_GrabInfo & C_ENABLE_INTERACT) != 0 ? m_GrabInfo ^ C_ENABLE_INTERACT : m_GrabInfo; }
        }
        public bool HighlightOnly
        {
            get { return (m_GrabInfo & C_HIGHLIGHT_ONLY) != 0; }
            set { m_GrabInfo = value ? m_GrabInfo | C_HIGHLIGHT_ONLY : (m_GrabInfo & C_HIGHLIGHT_ONLY) != 0 ? m_GrabInfo ^ C_HIGHLIGHT_ONLY : m_GrabInfo; }
        }

        #endregion

        #endregion
        #region DEBUG

        private void OnMouseEnter()
        {
            GrabPoint.DebugHand.Debug_MouseEnter(this);
        }
        private void OnMouseExit()
        {
            GrabPoint.DebugHand.Debug_MouseExit(this);
        }

        #endregion
    }

}