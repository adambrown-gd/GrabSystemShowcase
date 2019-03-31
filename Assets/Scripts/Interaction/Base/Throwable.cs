using System; 
using System.Linq; 
using System.Collections;
using System.Collections.Generic;

using UnityEngine;

using Photon.Pun;
using Photon.Realtime;

using ACSL.Utility;

namespace ACSL.Interaction
{
    /// <summary>
    /// Added to any DynamicObject that is dropped by a player.
    /// Helps track what should happen with thrown objects, like 
    /// snapping or dog interaction.
    /// 
    /// Adam Brown - Last Updated: 03/26/2019
    /// </summary>
	public class ThrownObject : MonoBehaviour
	{
        public GrabPoint m_ThrowPoint;
        public Vector3 m_ThrowOrigin;
        public Player m_Thrower;

        public void Awake()
        {
            gameObject.layer = LayerMask.NameToLayer("ThrownObject");
            GetComponent<DynamicObject>().OnGrabBegin += Remove;
        }
        public void Remove(GrabBase targetObject)
        {
            Destroy(this);
        }
        public void OnDestroy()
        {
            gameObject.layer = LayerMask.NameToLayer("Interactable");
            Physics.IgnoreCollision(m_ThrowPoint.GetComponents<CapsuleCollider>().First(c => !c.isTrigger), GetComponent<DynamicObject>().RigidbodyCollider, false);
            GetComponent<DynamicObject>().OnGrabBegin -= Remove;
        }
        public void OnCollisionEnter(Collision collision)
        {
            Remove(null);
        }
    }
}