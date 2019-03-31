using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR;

using ACSL.Utility;


namespace ACSL.ControllerInput
{
    /// <summary>
    /// The base for any controller in the ACSL framework. Currently,
    /// the only controller to be used is the ViveController, which contains both
    /// keyboard and VR input.This could be changed later on to incorporate the
    /// occulus, hololens, gamepads, etc.
    /// </summary>
    //  Adam Brown - 09/11/2018
    public abstract class BaseController : MonoBehaviour
    {
        #region EVENTS

        private static List<InputStart> m_Delegates = new List<InputStart>();
        private static event InputStart OnInputStartReal;
        public static event InputStart OnInputStart
        {
            add
            {
                OnInputStartReal += value;
                m_Delegates.Add(value);
            }
            remove
            {
                OnInputStartReal -= value;
                m_Delegates.Remove(value);
            }
        }

        #endregion
        #region PROTECTED VARIABLES

        protected Dictionary<InputAction, InputEvent> m_ListOfInput;

        #endregion
        #region ACCESSORS

        public Dictionary<InputAction, InputEvent> ListOfInput
        {
            get { return m_ListOfInput; }
            set { m_ListOfInput = value; }
        }

        #endregion
        #region PUBLIC METHODS

        public abstract void GetInputList();

        #endregion
        #region PROTECTED METHODS

        protected abstract void QueryInputButton();

        protected void Update()
        {
            QueryInputButton();
        }
        protected void OnDestroy()
        {
            foreach (InputStart d in m_Delegates)
                OnInputStartReal -= d;

            m_Delegates.Clear();
        }
        protected virtual void EventCallback_DownEvent(InputAction kb, int hand)
        {
            InputManager.Instance.InputList[kb].CallDownEvent(hand);
        }
        protected virtual void EventCallback_UpEvent(InputAction kb, int hand)
        {
            InputManager.Instance.InputList[kb].CallUpEvent(hand);
        }
        protected virtual void EventCallback_PressEvent(InputAction kb, int hand)
        {
            InputManager.Instance.InputList[kb].CallPressEvent(hand);
        }
        #endregion
    }
}
