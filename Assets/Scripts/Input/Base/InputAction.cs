using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR;
using ACSL.Utility;
using ACSL.Interaction;

namespace ACSL.ControllerInput
{
    /// <summary>
    /// The parent class for all keybindings. Scriptable objects allow
    /// for custom keybinding scenarios without keybindings needing to
    /// be GameObjects.
    /// Adam Brown - Last Updated: 1/3/2019
    /// </summary>
    public abstract class InputAction : ScriptableObject
    {
        #region EVENTS

        public event InputMoment OnInputEnabled;
        public event InputMoment OnInputDisabled;

        #endregion
        #region PUBLIC VARIABLES

        public InputTypes m_InputType;

        public KeyCode m_KeyboardBinding;

        public int m_HandBits;
        public EVRButtonId m_ViveBinding;
        public TouchpadUses m_TouchpadUses;

        public bool m_IsActive;

        public bool m_Vibrate;
        public float m_VibrationStrength = 0;
        public int m_VibrationBits = 0x0;

        #endregion
        #region PROTECTED VARIABLES

        protected BaseController m_Controller;

        #endregion
        #region PUBLIC METHODS

        #region ABSTRACT METHODS

        public abstract void Update();
        public abstract void InputEnabled();
        public abstract void InputDisabled();
        public abstract void KeyPress(int hand);
        public abstract void KeyPressUp(int hand);
        public abstract void KeyPressDown(int hand);

        #endregion
        #region EVENT CALLBACKS

        public void EventCallback_InputEnabled()
        {
            if (OnInputEnabled != null)
                OnInputEnabled(this);
        }
        public void EventCallback_InputDisabled()
        {
            if (OnInputDisabled != null)
                OnInputDisabled(this);
        }

        #endregion
        /// <summary>
        /// Function called by the CreateKeybindingWindow to store info 
        /// into the generated InputAction
        /// Adam Brown  - Last Updated: 12/15/2018
        /// </summary>
        public void Create(InputTypes type, EVRButtonId vb, int handActivity, KeyCode kb, TouchpadUses touchUses)
        {
            m_IsActive = true;
            m_InputType = type;
            m_ViveBinding = vb;
            m_HandBits = handActivity;
            m_KeyboardBinding = kb;
            m_TouchpadUses = touchUses;
        }
        public virtual void Init()
        {
            m_Controller = GameUtils.LocalController;
        }
        public static GrabPoint GrabPointByHand(int hand)
        {
            if (!GameUtils.VRActive)
                return GrabPoint.DebugHand;

            return (hand & C_LEFT) != 0 ? (GameUtils.LocalController as ViveController).m_LeftControllerTrackedObject.GetComponent<GrabPoint>() :
                   (hand & C_RIGHT) != 0 ? (GameUtils.LocalController as ViveController).m_RightControllerTrackedObject.GetComponent<GrabPoint>() : null;
        }
        #endregion
        #region PRIVATE METHODS
        private void GetLocalController(BaseController bc)
        {
            m_Controller = bc;
        }

        #endregion
        #region ACCESSORS

        public static int C_LEFT
        {
            get { return 1; }
        }
        public static int C_RIGHT
        {
            get { return 2; }
        }
        /// <summary>
        /// Use this to access the last used Controller Hand. Useful for keybindings.
        /// Adam Brown - Last Updated: 1/1/2019
        /// </summary>
        public bool LeftHand
        {
            get { return (m_HandBits & C_LEFT) != 0; }
            set { m_HandBits = value ? m_HandBits | C_LEFT : (m_HandBits & C_LEFT) != 0 ? m_HandBits ^ C_LEFT : m_HandBits; }
        }
        public bool RightHand
        {
            get { return (m_HandBits & C_RIGHT) != 0; }
            set { m_HandBits = value ? m_HandBits | C_RIGHT : (m_HandBits & C_RIGHT) != 0 ? m_HandBits ^ C_RIGHT : m_HandBits; }
        }
        public bool IsActive
        {
            get { return m_IsActive; }
            set { m_IsActive = value; }
        }
        public InputTypes InputType
        {
            get { return m_InputType; }
            set { m_InputType = value; }
        }
        public EVRButtonId ViveBinding
        {
            get { return m_ViveBinding; }
            set { m_ViveBinding = value; }
        }
        public TouchpadUses TouchpadUses
        {
            get { return m_TouchpadUses; }
            set { m_TouchpadUses = value; }
        }
        public KeyCode KeyboardBinding
        {
            get { return m_KeyboardBinding; }
            set { m_KeyboardBinding = value; }
        }
        public int HandBits
        {
            get { return m_HandBits; }
            set { m_HandBits = value; }
        }

        #endregion
    }
}
