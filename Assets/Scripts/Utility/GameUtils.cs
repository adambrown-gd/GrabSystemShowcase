using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using ACSL;
using ACSL.ControllerInput;
using ACSL.Missions;
using ACSL.Interaction;
using ACSL.Tutorials;

namespace ACSL.Utility
{
    /// <summary>
    /// A static class to hold local references.
    /// Adam Brown - Last Updated: 1/13/2019
    /// </summary>
    public static class GameUtils
    {
        #region VARIABLES

        private static BaseController m_LocalController = null;
        private static Mission m_LocalMission = null;
        private static Tutorial m_LocalTutorial = null;

        #endregion
        #region METHODS

        /// <summary>
        /// Returns true if an HMD is detected.
        /// Adam Brown - Last Updated: 1/13/2019
        /// </summary>
        public static bool VRActive
        {
            get { return UnityEngine.XR.XRDevice.isPresent; }
        }

        /// <summary>
        /// Returns the Controller for the local player. 
        /// Cast to ViveController to get access to VR controller
        /// functionality.
        /// Adam Brown - Last Update 1/13/2019
        /// </summary>
        public static BaseController LocalController
        {
            get { return m_LocalController; }
            set { m_LocalController = value; }
        }

        /// <summary>
        /// Returns the current mission for the local player.
        /// Cast to child class of Mission to access Mission
        /// specific details.
        /// Adam Brown - Last Updated: 1/13/2019
        /// </summary>
        public static Mission LocalMission
        {
            get { return m_LocalMission; }
            set { m_LocalMission = value; }
        }

        public static Tutorial LocalTutorial
        {
            get { return m_LocalTutorial; }
            set { m_LocalTutorial = value; }
        }

        /// <summary>
        /// A shorthand function to spawn a prefab at a path in Resources.
        /// Adam Brown - Last Updated: 1/13/2019
        /// </summary>
        /// <param name="prefabName"></param>
        /// <returns></returns>
        public static GameObject SpawnPrefab(string prefabName)
        {
            return GameObject.Instantiate(Resources.Load(prefabName, typeof(GameObject)) as GameObject);
        }

        #endregion
    }
}
