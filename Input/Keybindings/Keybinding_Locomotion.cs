using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ACSL.ControllerInput
{
    /// <summary>
    /// A keybinding that allows players to move around.
    /// Adam Brown - Last Updated 1/3/2019
    /// </summary>
    public class Keybinding_Locomotion : InputAction
    {
        #region VARIABLES

        private ViveController m_VController;
        private Locomotion m_PlayerLocomotion;

        #endregion

        #region METHODS

        public override void InputDisabled()
        {
            m_PlayerLocomotion.ButtonPressed = false;

            m_PlayerLocomotion.m_ButtonsPressed[0] = false;
            m_PlayerLocomotion.m_ButtonsPressed[1] = false;
        }
        public override void InputEnabled()
        {
            //not implemented.
        }
        public override void Init()
        {
            base.Init();

            m_VController = m_Controller as ViveController;
            m_PlayerLocomotion = m_VController.GetComponent<Locomotion>();
        }
        public override void KeyPress(int hand)
        {
            //not implemented.
            m_PlayerLocomotion.m_ButtonsPressed[hand - 1] = true;
        }
        public override void KeyPressDown(int hand)
        {
            m_PlayerLocomotion.m_ButtonsPressed[hand - 1] = true;

            m_PlayerLocomotion.ButtonPressed = true;
            m_PlayerLocomotion.InputHand = hand;
        }
        public override void KeyPressUp(int hand)
        {
            m_PlayerLocomotion.m_ButtonsPressed[hand - 1] = false;

            m_PlayerLocomotion.ButtonPressed = false;
        }
        public override void Update()
        {
            //not implemented.
        }

        #endregion
    }
}
