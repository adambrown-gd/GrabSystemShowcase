using System;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR;
using ACSL.Utility;

namespace ACSL.ControllerInput
{
    /// <summary>
    /// The Vive controller. Contains functionality for different
    /// controller hands and rumbling intensity/length.
    /// </summary>
    //  Adam Brown - 09/11/2018
    [DisallowMultipleComponent]
    public class ViveController : BaseController
    {
        #region PUBLIC VARIABLES

        [Space(20)]
        public SteamVR_TrackedObject m_RightControllerTrackedObject;
        public SteamVR_TrackedObject m_LeftControllerTrackedObject;

        #endregion
        #region PRIVATE VARIABLES

        private SteamVR_Controller.Device m_RightControllerDevice;
        private SteamVR_Controller.Device m_LeftControllerDevice;

        #endregion
        #region ACCESSORS

        public Vector2 TouchAxis_LeftController
        {
            get
            {
                if (Device_LeftController != null)
                {                   
                    return Device_LeftController.GetAxis(EVRButtonId.k_EButton_SteamVR_Touchpad);
                }
                return Vector2.zero;
            }
        }
        public Vector2 TouchAxis_RightController
        {
            get
            {
                if (Device_RightController != null)
                {
                    return Device_RightController.GetAxis(EVRButtonId.k_EButton_SteamVR_Touchpad);
                }
                return Vector2.zero;
            }
        }
        public SteamVR_Controller.Device Device_RightController
        {
            get
            {
                if (m_RightControllerTrackedObject != null)
                {
                    int controllerIndex = (int)m_RightControllerTrackedObject.index;
                    if (IsValidControllerIndex(controllerIndex))
                    {
                        m_RightControllerDevice = SteamVR_Controller.Input(controllerIndex);
                    }
                }
                return m_RightControllerDevice;
            }
        }
        public SteamVR_Controller.Device Device_LeftController
        {
            get
            {
                if (m_LeftControllerTrackedObject != null)
                {
                    int controllerIndex = (int)m_LeftControllerTrackedObject.index;
                    if (IsValidControllerIndex(controllerIndex))
                    {
                        m_LeftControllerDevice = SteamVR_Controller.Input(controllerIndex);
                    }
                }
                return m_LeftControllerDevice;
            }
        }
        public bool QueryViveInput(Type kbType, KeyPressTypes kpt, int hand)
        {

            InputAction kb = InputManager.Instance.GetKeybindingByType(kbType);
            ViveController vController = GameUtils.LocalController as ViveController;
            SteamVR_Controller.Device d = (hand & InputAction.C_LEFT) != 0 ? vController.Device_LeftController : (hand & InputAction.C_RIGHT) != 0 ? vController.Device_RightController : null;

            if (d == null)
                return false;

            switch (kpt)
            {
                case KeyPressTypes.Down:
                    return d.GetPressDown(kb.ViveBinding);
                case KeyPressTypes.Press:
                    return d.GetPress(kb.ViveBinding);
                case KeyPressTypes.Up:
                    return d.GetPressUp(kb.ViveBinding);
            }
            return false;
        }

        #endregion
        #region PULIC METHODS

        public override void GetInputList()
        {
            m_ListOfInput = InputManager.Instance.InputList;
        }
        public void VibrateController(int hand, ushort strength = 100)
        {
            SteamVR_Controller.Device d = (hand & InputAction.C_LEFT) != 0 ? m_LeftControllerDevice : (hand & InputAction.C_RIGHT) != 0 ? m_RightControllerDevice : null;
            if (d != null && strength != 0)
                d.TriggerHapticPulse(strength);
        }

        #endregion
        #region PRIVATE METHODS

        public static SteamVR_Controller.Device GetSteamVRDeviceFromTrackedObject(SteamVR_TrackedObject aTrackedObject)
        {
            if (aTrackedObject != null)
            {
                int controllerIndex = (int)aTrackedObject.index;
                if (IsValidControllerIndex(controllerIndex) == true)
                {
                    return SteamVR_Controller.Input(controllerIndex);
                }
            }

            return null;
        }
        /*
        Description: Function to check that the vive vr controller index is valid
        Parameters: int aIndex-The index for the current HTC vive controller
        Creator: Alvaro Chavez Mixco
        */
        private static bool IsValidControllerIndex(int index)
        {
            return index > 0 && index < OpenVR.k_unMaxTrackedDeviceCount;
        }
        /// <summary>
        /// A separate query for the vive controllers. 
        /// Returns as soon as it finds input.
        /// Adam Brown - 12/18/2018
        /// </summary>
        /// <param name="kb">Which keybinding are we checking?</param>
        /// <param name="pressTypes">Which press type are we checking?</param>
        /// <returns></returns>
        private bool ViveQuery(int hand, InputAction kb, KeyPressTypes pressTypes)
        {
            //put controllers in an array of 2. 0 = left, 1 = right
            SteamVR_Controller.Device[] controllers = new SteamVR_Controller.Device[2] { Device_LeftController, Device_RightController };
            bool[] activeHands = { kb.LeftHand, kb.RightHand };

            if (!activeHands[hand - 1])
                return false;

            //check press down
            if (pressTypes == KeyPressTypes.Down)
            {
                //at the EVRButtonId
                if (controllers[hand - 1].GetPressDown(kb.ViveBinding))
                {
                    //set the input hand and return true.
                    if ((kb.m_VibrationBits & (int)KeyPressTypes.Down) != 0 && kb.m_Vibrate)
                        VibrateController(hand, (ushort)kb.m_VibrationStrength);

                    return true;
                }
            }
            //check press up
            else if (pressTypes == KeyPressTypes.Up)
            {
                //at the EVRButtonId
                if (controllers[hand - 1].GetPressUp(kb.ViveBinding))
                {
                    //set the input hand and return true.
                    if ((kb.m_VibrationBits & (int)KeyPressTypes.Up) != 0 && kb.m_Vibrate)
                        VibrateController(hand, (ushort)kb.m_VibrationStrength);

                    return true;
                }
            }
            //check press
            else if (pressTypes == KeyPressTypes.Press)
            {
                //at the EVRButtonId
                if (controllers[hand - 1].GetPress(kb.ViveBinding))
                {
                    //set the input hand and return true.
                    if ((kb.m_VibrationBits & (int)KeyPressTypes.Press) != 0 && kb.m_Vibrate)
                        VibrateController(hand, (ushort)kb.m_VibrationStrength);

                    return true;
                }
            }

            //we didn't press anything, return false.
            return false;
        }
        #endregion
        #region PROTECTED METHODS
        protected override void QueryInputButton()
        {
            if (m_ListOfInput != null)
            {
                foreach (InputAction kb in m_ListOfInput.Keys)
                {
                    if (kb.InputType == InputTypes.ViveController || kb.InputType == InputTypes.Both)
                    {
                        for (int i = 0; i < 2; i++)
                        {
                            if (ViveQuery(i + 1, kb, KeyPressTypes.Down))
                                EventCallback_DownEvent(kb, i + 1);
                            if (ViveQuery(i + 1, kb, KeyPressTypes.Up))
                                EventCallback_UpEvent(kb, i + 1);
                            if (ViveQuery(i + 1, kb, KeyPressTypes.Press))
                                EventCallback_PressEvent(kb, i + 1);
                        }
                    }
                    if (kb.InputType == InputTypes.Keyboard || kb.InputType == InputTypes.Both)
                    {
                        if (Input.GetKeyDown(kb.KeyboardBinding))
                            EventCallback_DownEvent(kb, -1);
                        if (Input.GetKeyUp(kb.KeyboardBinding))
                            EventCallback_UpEvent(kb, -1);
                        if (Input.GetKey(kb.m_KeyboardBinding))
                            EventCallback_PressEvent(kb, -1);
                    }
                }
            }
        }
        #endregion
    }
}

