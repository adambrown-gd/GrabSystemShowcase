using System;
using System.Runtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR;
using ACSL;

namespace ACSL.ControllerInput
{
    #region ENUMS

    //Vive Controller differentiation
    public enum ViveHand
    {
        Left = 0,
        Right = 1,
        Both = 2,
        None = 3
    }
    //differentiating between button states
    public enum KeyPressTypes
    {
        Down = 1,
        Up = 2,
        Press = 4,
        Touch = 8,
        None = 16,
    }
    //Touchpad uses
    public enum TouchpadUses
    {
        None = 0,
        Touch = 1,
        Press = 2,
        Both = 3,
    }
    //Input type differentiation
    public enum InputTypes
    {
        ViveController = 0,
        Keyboard = 1,
        Both = 2,
    }

    #endregion
    #region STRUCTS

    /// <summary>
    /// Data structure for the events that get generated through the amt of key bindings in 
    /// the scene.Keybinding events are subscribed through the InputAction parent class.
    /// Adam Brown - Last Updated: 09/11/2018
    /// </summary>
    [Serializable]
    public class InputEvent
    {
        public event InputDel DownEvent;
        public event InputDel UpEvent;
        public event InputDel PressEvent;

        public Delegate[] GetInvocationList(KeyPressTypes type)
        {
            Delegate[] returnList = null;

            if (type == KeyPressTypes.Down && DownEvent != null)
                returnList = DownEvent.GetInvocationList();
            if (type == KeyPressTypes.Press && PressEvent != null)
                returnList = PressEvent.GetInvocationList();
            if (type == KeyPressTypes.Up && UpEvent != null)
                returnList = UpEvent.GetInvocationList();

            if (returnList != null)
                return returnList;
            else
                return new Delegate[] { };
        }
        public void CallDownEvent(int hand)
        {
            if (DownEvent != null)
                DownEvent(hand);
        }
        public void CallUpEvent(int hand)
        {
            if (UpEvent != null)
                UpEvent(hand);
        }
        public void CallPressEvent(int hand)
        {
            if (PressEvent != null)
                PressEvent(hand);
        }
    }

    #endregion
    #region DELEGATES

    //used for enabling and disabling keybindings
    public delegate void InputMoment(InputAction keybinding);

    //used to call up down and press events for keybindings
    public delegate void InputDel(int hand);

    //used to get the last hand used in an input call
    public delegate void InputHand(int handActivity);

    //used to signify the starting point for anything that needs to listen to 
    //the Input system. 
    public delegate void InputStart(BaseController controller);
    public delegate void sdfsdf(InputStart e);

    #endregion
}