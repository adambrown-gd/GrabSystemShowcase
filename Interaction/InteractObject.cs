using System;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;


namespace ACSL.Interaction
{
    /// <summary>
    /// Derived from GrabBase. Used for objects that
    /// are interacted with, but not manipulated by the player.
    /// 
    /// Adam Brown - Last Updated: 03/26/2019
    /// </summary>
    public class InteractObject : GrabBase
    {
        public event EventHandler_GrabBase OnInteract;

        protected override void Action_GrabObject(GrabPoint targetPoint, bool grab = true, bool forceGrab = false)
        {
            //unhighlight
            Action_Highlight(targetPoint, false);

            //call object-specific functionality
            GrabOnce(targetPoint);

            //event callback
            EventCallback_Interact();

            if (m_Debug)
                Debug.Log(gameObject.name + " - GrabOnce: true" + " - GrabPoint: " + (targetPoint.BitHand == 1 ? "Left" : targetPoint.BitHand == 2 ? "Right" : "null"));
        }
        protected override void CleanUp()
        {
            base.CleanUp();
            Delegate.RemoveAll(OnInteract, OnInteract);
        }
        public void EventCallback_Interact()
        {
            OnInteract?.Invoke(this);
        }
    }
}
