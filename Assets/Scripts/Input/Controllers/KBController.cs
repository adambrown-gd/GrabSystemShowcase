using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ACSL.ControllerInput
{
    /// <summary>
    /// Simple keyboard controller. Used mostly for debugging/dev mode.
    /// </summary>
    //  Adam Brown - 09/11/2018
    public class KBController : BaseController
    {
        public override void GetInputList()
        {
            m_ListOfInput = InputManager.Instance.InputList;
        }
        #region PROTECTED METHODS

        protected override void QueryInputButton()
        {
            if (m_ListOfInput != null)
            {
                foreach (InputAction kb in m_ListOfInput.Keys)
                {
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
