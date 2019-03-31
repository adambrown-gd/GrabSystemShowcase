using System;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;

using ACSL.Game;

namespace ACSL.Interaction
{
    /// <summary>
    /// Add to a GrabObject to allow it to have a use function.
    /// Note: Implement use function through subscription of 
    /// OnUseBegin and OnUseEnd.
    /// 
    /// Adam Brown - Last Updated: 03/26/2019
    /// </summary>
    [RequireComponent(typeof(GrabObject))]
    public class Useable : MonoBehaviour
    {
        public event EventHandler_GrabBase OnUseBegin;
        public event EventHandler_GrabBase OnUseEnd;

        private GrabBase m_UseableObject;

        public void EventCallback_OnUseBegin()
        {
            OnUseBegin?.Invoke(m_UseableObject);
        }
        public void EventCallback_OnUseEnd()
        {
            OnUseEnd?.Invoke(m_UseableObject);
        }
        private void Awake()
        {
            m_UseableObject = GetComponent<GrabBase>();
            GameManager.Instance.OnSceneCleanUp += CleanUp;
        }
        private void CleanUp()
        {
            Delegate.RemoveAll(OnUseBegin, OnUseBegin);
            Delegate.RemoveAll(OnUseEnd, OnUseEnd);
        }
    }
}
