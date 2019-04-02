using System;
using System.Runtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ACSL.Utility;

namespace ACSL.ControllerInput
{
    /// <summary>
    /// Controls and manages how keybindings get called.
    /// The new and improved InputManager. Easier to organize/track
    /// KB data and debug. 
    /// Adam Brown - Last Updated: 1/3/2019
    /// </summary>
    public class InputManager : MonoBehaviour
    {
        #region PUBLIC VARIABLES

        public string[] m_KeybindingTypes;
        public static InputManager m_Instance;

        #endregion
        #region PROTECTED VARIABLES

        protected Dictionary<InputAction, InputEvent> m_InputList = new Dictionary<InputAction, InputEvent>();

        #endregion
        #region PUBLIC METHODS

        public void ToggleKeybinding(Type kbType, bool toggle)
        {
            //find the kb by the type passed in
            InputAction kb = GetKeybindingByType(kbType);

            if (kb == null)
                return;

            // - subscribe/unsubscribe to the button events
            // - call the internal enabled/dsisabled functions
            // - call the event callback for enabled/disable functions
            if (toggle)
            {
                EventSubscribe(kb);
                kb.InputEnabled();
                kb.EventCallback_InputEnabled();
            }
            else
            {
                EventUnsubscribe(kb);
                kb.InputDisabled();
                kb.EventCallback_InputDisabled();
            }
        }
        public void ToggleKeybindingOnHand(Type kbType, int hand, bool toggle)
        {
            //find the proper keybinding
            InputAction kb = GetKeybindingByType(kbType);

            //set the appropriate hand to the toggle
            if ((hand & InputAction.C_LEFT) != 0)
                kb.LeftHand = toggle;
            else
                kb.RightHand = toggle;
        }
        public InputAction GetKeybindingByType(Type type)
        {
            //iterate through kb's, and return a match.
            //there should only be one type of keybinding.
            foreach (InputAction kb in m_InputList.Keys)
                if (kb.GetType() == type)
                    return kb;

            return null;
        }

        #endregion
        #region PRIVATE METHODS

        private void Update()
        {
            foreach (InputAction kb in m_InputList.Keys)
                kb.Update();
        }
        private void Awake()
        {
            m_Instance = this;

            Game.GameManager.Instance.OnSceneCleanUp += CleanUp;

            if (GameUtils.LocalController != null)
                CreateInputList(null);
            else
                BaseController.OnInputStart += CreateInputList;
        }

        private void CleanUp()
        {
            Game.GameManager.Instance.OnSceneCleanUp -= CleanUp;

            foreach (InputAction kb in m_InputList.Keys)
            {
                EventUnsubscribe(kb);

                ScriptableObject.DestroyImmediate(kb);
            }

            m_InputList.Clear();
        }

        private void CreateInputList(BaseController bc)
        {
            if (m_KeybindingTypes == null)
            {
                Debug.Log("Keybinding Types null when trying to CreateInputList");
                return;
            }

            foreach (string s in m_KeybindingTypes)
            {
                //instantiate the KB
                InputAction kb = ScriptableObject.Instantiate(Resources.Load<InputAction>("ScriptableObjects/" + s));

                //throw if we failed
                if (kb == null)
                    throw new NullReferenceException("A keybinding was entered in the InputManager incorrectly. Try clearing the list and reloading.");

                if(!GameUtils.VRActive && kb.InputType == InputTypes.ViveController)
                {
                    Debug.LogWarning("Tried to initiate a VR keybinding when VR was not active. Keybinding was not initialized.");
                    kb.InputType = InputTypes.Keyboard;
                    continue;
                }

                //add it to the list
                m_InputList.Add(kb, new InputEvent());

                //subscribe to button events
                EventSubscribe(kb);

                //call internal init
                kb.Init();
            }

            GameUtils.LocalController.GetInputList();
        }
        private void EventSubscribe(InputAction kb)
        {
            if(m_InputList[kb].GetInvocationList(KeyPressTypes.Down).Length < 1)
                m_InputList[kb].DownEvent += kb.KeyPressDown;
            if (m_InputList[kb].GetInvocationList(KeyPressTypes.Up).Length < 1)
                m_InputList[kb].UpEvent += kb.KeyPressUp;
            if (m_InputList[kb].GetInvocationList(KeyPressTypes.Press).Length < 1)
                m_InputList[kb].PressEvent += kb.KeyPress;
        }
        private void EventUnsubscribe(InputAction kb)
        {
            m_InputList[kb].DownEvent -= kb.KeyPressDown;
            m_InputList[kb].UpEvent -= kb.KeyPressUp;
            m_InputList[kb].PressEvent -= kb.KeyPress;
        }

        #endregion
        #region ACCESSORS

        public static InputManager Instance
        {
            get { return m_Instance; }
        }
        public Dictionary<InputAction, InputEvent> InputList
        {
            get { return m_InputList; }
        }

        #endregion
    }
}
