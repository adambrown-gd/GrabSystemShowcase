using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using ACSL;
using ACSL.Interaction;
using ACSL.Utility;

namespace ACSL.ControllerInput
{
    /// <summary>
    /// The keybinding that allows objects like the clamp can be used. 
    /// Adam Brown - Last Updated: 1/3/2019
    /// </summary>
    public class Keybinding_UseObject : InputAction
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

            //if the grabpoint is not grabbing, return
            if (!targetPoint.Grabbing)
                return;

            //if the grab target is not useable, return
            if (!targetPoint.GrabbedObject.IsUseable)
                return;

            //call UseBegin on the object
            targetPoint.GrabbedObject.GetComponent<Useable>().EventCallback_OnUseBegin();
        }
        public override void KeyPressUp(int hand)
        {
            GrabPoint targetPoint = GrabPointByHand(hand);

            //if the grabpoint is not grabbing, return
            if (!targetPoint.Grabbing)
                return;

            //if the grab target is not useable, return
            if (!targetPoint.GrabbedObject.IsUseable)
                return;

            //call UseEnd on the object
            targetPoint.GrabbedObject.GetComponent<Useable>().EventCallback_OnUseEnd();
        }
        public override void Update()
        {
            //not implemented.
        }

        #endregion
    }
}
