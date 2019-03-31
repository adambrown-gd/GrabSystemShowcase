using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using ACSL.Interaction;
using ACSL.Utility;

namespace ACSL.ControllerInput
{
    /// <summary>
    /// A keybinding that allows players to access their 
    /// menu options by pressing buttons
    /// Michael Watt & Adam Brown - 1/3/2019
    /// </summary>
    public class Keybinding_ButtonPress : InputAction
    {
        #region PUBLIC VARIABLES

        // Optionally show the debug ray.
        public bool m_ShowDebugRay;

        #endregion
        #region PRIVATE VARIABLES

        private ViveController m_VController;
        private VRMenuButton m_CurrentMenuButton;
        private VRMenuButton m_LastMenuButton;
        private VRMenuButton m_LastHoldButton;

        private GameObject m_RightHand;

        private float m_DebugRayLength = 5f;           // Debug ray length.
        private float m_DebugRayDuration = 1f;         // How long the Debug ray will remain visible.
        private float m_RayLength = 1f;


        private Coroutine m_WaitForFill;

        private MissionButton missionSelected;

        #endregion
        #region PUBLIC METHODS
        public override void Update()
        {
            if (m_RightHand != null && GameUtils.LocalController != null)
                RightHandRaycast();
        }
        public override void Init()
        {
            base.Init();

            if(!(m_Controller is KBController))
            {
                m_VController = m_Controller as ViveController;
                m_RightHand = m_VController.GetComponent<SteamVR_ControllerManager>().right;
            }
            else
            {
                m_VController = null;
                m_RightHand = null;
            }
        }
        public override void InputEnabled()
        {
            //not implemented.
        }
        public override void InputDisabled()
        {
            //not implemented.
        }
        public override void KeyPress(int hand)
        {
            //not implemented.
        }
        public override void KeyPressUp(int hand)
        {
            if (m_CurrentMenuButton != null)
            {
                m_CurrentMenuButton.StopSelecting();
            }
            if (m_LastHoldButton != null)
            {
                m_LastHoldButton.StopSelecting();
                m_LastHoldButton = null;
            }
            StopWaiting();
        }
        public override void KeyPressDown(int hand)
        {
            //Will need to remove but for now leaving in so that the code continues to function
            //m_MenuManager.PressButton();
            if (m_CurrentMenuButton != null)
            {
                if (m_CurrentMenuButton.m_Hold)
                    m_LastHoldButton = CurrentMenuButton;

                m_VController.Device_RightController.TriggerHapticPulse(1000);
                m_CurrentMenuButton.StartSelecting();
                m_WaitForFill = InputManager.Instance.StartCoroutine(m_CurrentMenuButton.WaitForSelectionRadialToFill());
            }

            //if (missionSelected != null)
            //{
            //    missionSelected.BroadcastMessage("Selected");
            //}
        }

        #endregion
        #region PRIVATE METHODS

        private void StopWaiting()
        {
            if (m_WaitForFill != null && InputManager.Instance != null)
                InputManager.Instance.StopCoroutine(m_WaitForFill);
        }
        private void RightHandRaycast()
        {
            Vector3 pos = new Vector3();

            Transform laserPointer = GrabPoint.RightHand.transform.Find("Laser Pointer");

            pos = laserPointer.position;

            // Show the debug ray if required
            if (m_ShowDebugRay)
                Debug.DrawRay(pos, laserPointer.forward * m_DebugRayLength, Color.blue, m_DebugRayDuration);

            // Create a ray that points forwards from the camera.
            Ray ray = new Ray(laserPointer.GetComponent<SteamVR_LaserPointer>().transform.position, laserPointer.GetComponent<SteamVR_LaserPointer>().transform.forward);
            RaycastHit hit;

            // Do the raycast forwards to see if we hit an interactive item
            if (Physics.Raycast(ray, out hit, m_RayLength, 1 << LayerMask.NameToLayer("Menu")))
            {
                VRMenuButton interactable = hit.collider.GetComponent<VRMenuButton>(); //attempt to get the VRMenuButton on the hit object
                m_CurrentMenuButton = interactable;

                // If we hit an interactive item and it's not the same as the last interactive item, then call Over
                if (interactable && interactable != m_LastMenuButton)
                {
                    interactable.Over();
                }

                // Deactivate the last interactive item 
                if (interactable != m_LastMenuButton)
                    DeactiveLastInteractable();

                m_LastMenuButton = interactable;

                if (m_CurrentMenuButton != null)    
                {
                    interactable.SetSelectorLocation(hit);
                }

                // Something was hit, set at the hit position.
                /*if (m_Reticle)
                    m_Reticle.SetPosition(hit);
                
                if (OnRaycasthit != null)
                    OnRaycasthit(hit);
                */
            }
            else
            {
                // Nothing was hit, deactivate the last interactive item.
                DeactiveLastInteractable();
                m_CurrentMenuButton = null;

                // Position the reticle at default distance.
                /*if (m_Reticle)
                    m_Reticle.SetPosition();*/
            }
        }
        private void DeactiveLastInteractable()
        {
            if (m_LastMenuButton == null)
                return;

            StopWaiting();
            m_LastMenuButton.Out();

            m_LastMenuButton = null;
        }
        private void OnDestroy()
        {
            StopWaiting();
        }

        #endregion
        #region ACCESSORS

        public VRMenuButton CurrentMenuButton
        {
            get { return m_CurrentMenuButton; }
        }

        #endregion
    }
}

