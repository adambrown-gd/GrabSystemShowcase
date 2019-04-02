using UnityEngine;
using UnityEditor;
using System;
using System.Linq;
using System.Runtime;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;
using ACSL;
using ACSL.Interaction;
using ACSL.Network;

namespace ACSL.Utility
{
    /// <summary>
    /// Holds utility methods for the InteractionSystem.
    /// Adam Brown - Last Updated: 1/13/2019
    /// </summary>
    public static class InteractionUtils
    {
        #region VARIABLES

        private static Material m_Highlight = null;
        private static Material m_SnapzoneHighlight = null;
        private static Material m_SnapzoneClone = null;
        private static Material m_BrokenHologram = null;
        private static Material m_Dissolve = null;

        #endregion
        #region METHODS

        /// <summary>
        /// Swaps all materials in the given GameObject
        /// with the material passed in.
        /// Adam Brown 11/18/2018
        /// </summary>
        /// <param name="go">The GameObject to swap.</param>
        /// <param name="m">The material to swap</param>
        /// <param name="InstanceMaterial">TRUE for Instanced Materials on each Renderer. FALSE for the original.</param>
        public static void SwapObjectToMaterial(GameObject go, Material m, bool InstanceMaterial = false)
        {
            foreach (Renderer r in go.GetComponentsInChildren<Renderer>())
                r.material = InstanceMaterial ? new Material(m) : m;
        }

        /// <summary>
        /// This method creates a visual clone of a game object by copying 
        /// all it's meshes and placing them into a new GameObject
        /// Adam Brown 11/18/2018
        /// </summary>
        /// <param name="go">The gameobject to clone</param>
        /// <param name="includeInteractableMesh">for interactable objects, include the ARHUD box or not</param>
        /// <returns></returns>
        public static GameObject CreateVisualCopy(GameObject go, bool includeInteractableMesh = false)
        {
            GameObject root = GameObject.Instantiate(go);

            foreach (MonoBehaviour mb in root.GetComponentsInChildren<MonoBehaviour>(true))
                mb.enabled = false;
            foreach (Collider c in root.GetComponentsInChildren<Collider>(true))
                c.enabled = false;

            root.GetComponentInChildren<Rigidbody>(true).Sleep();

            return root;
        }

        /// <summary>
        /// Highlights an object snapped in a snapzone.
        /// Adam Brown - Last Updated: 1/13/2019
        /// </summary>
        /// <param name="snapObjectTarget"></param>
        public static void HighlightSnapObject(GameObject snapObjectTarget)
        {
            foreach (Renderer r in snapObjectTarget.GetComponentsInChildren<Renderer>())
                r.material = m_SnapzoneHighlight;
        }

        #endregion
        #region ACCESSORS

        public static Material Material_Highlight
        {
            get { return m_Highlight != null ? m_Highlight : m_Highlight = Resources.Load("Shaders/Materials/Highlight", typeof(Material)) as Material; }
        }
        public static Material Material_SnapzoneHighlight
        {
            get { return m_SnapzoneHighlight != null ? m_SnapzoneHighlight : m_SnapzoneHighlight = Resources.Load("Shaders/Materials/Highlight", typeof(Material)) as Material; }
        }
        public static Material Material_SnapzoneClone
        {
            get { return m_SnapzoneClone != null ? m_SnapzoneClone : m_SnapzoneClone = Resources.Load("Shaders/Materials/SnapzoneClone", typeof(Material)) as Material; }
        }
        public static Material Material_BrokenHologram
        {
            get { return m_BrokenHologram != null ? m_BrokenHologram : m_BrokenHologram = Resources.Load("Shaders/Materials/DefHologram", typeof(Material)) as Material; }
        }
        public static Material Material_Dissolve
        {
            get { return m_Dissolve != null ? m_Dissolve : m_Dissolve = Resources.Load("Shaders/Materials/Dissolve", typeof(Material)) as Material; }
        }

        #endregion
    }
}
