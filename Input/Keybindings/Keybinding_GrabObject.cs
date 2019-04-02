using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using ACSL.Interaction;
using ACSL.Utility;

namespace ACSL.ControllerInput
{
    /// <summary>
    /// The keybinding that allows objects to be grabbed. 
    /// Adam Brown - Last Updated: 1/3/2019
    /// </summary>
    public class Keybinding_GrabObject : InputAction
    {
        #region METHODS

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
            targetPoint.Action_GrabQuery(KeyPressTypes.Down);
        }
        public override void KeyPressUp(int hand)
        {
            GrabPoint targetPoint = GrabPointByHand(hand);
            targetPoint.Action_GrabQuery(KeyPressTypes.Up);
        }
        public override void Update()
        {
            //not implemented.
        }

        #endregion
    }
}
