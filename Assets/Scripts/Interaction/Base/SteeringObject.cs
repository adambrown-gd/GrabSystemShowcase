using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using ACSL;
using ACSL.Utility;

namespace ACSL.Interaction
{
    /// <summary>
    /// Derived from StaticObject. Used for driving
    /// interfaces for the TEx in the form of the 
    /// Throttle and Joystick.
    /// 
    /// Adam Brown - Last Updated: 03/26/2019
    /// </summary>
    public class SteeringObject : StaticObject
    {
        #region PUBLIC VARIABLES

        public Vector3 m_OriginRotation;

        #endregion
        #region PRIVATE VARIABLES

        private Timer m_LerpTimer;
        private Vector2 m_AngleRatios;

        #endregion
        #region ACCESSORS

        public Vector2 AngleRatios
        {
            get { return m_AngleRatios; }
            set { m_AngleRatios = value; }
        }
        public Timer LerpTimer
        {
            get { return m_LerpTimer; }
            set { m_LerpTimer = value; }
        }

        #endregion
        #region PUBLIC METHODS

        protected override void GrabBegin(GrabPoint targetPoint)
        {
            if (m_LerpTimer.Started)
                m_LerpTimer.Stop();
        }
        protected override void GrabEnd(GrabPoint targetPoint)
        {
            m_LerpTimer.NewTimer(1.5f);
            m_LerpTimer.StartTimer();
            m_AngleRatios = Vector3.zero;
        }

        #endregion
        #region PRIVATE METHODS

        protected override void Start()
        {
            m_LerpTimer = gameObject.AddComponent<Timer>();
            m_LerpTimer.NewTimer(1.5f);
            m_LerpTimer.OnTimerUpdate += p => { transform.localRotation = Quaternion.Euler(MathUtils.Vector3LerpAngle(transform.localRotation.eulerAngles, m_OriginRotation, p)); };
        }
        protected override void Init()
        {
            base.Init();
            m_OriginRotation = transform.localEulerAngles;
        }
        /// <summary>
        /// Overridden to allow calculation of the angle at 
        /// which the SteeringObjects are at currently.
        /// Passed into a vehicle or something else to handle 
        /// driving.
        /// 
        /// Adam Brown - Last Updated: 03/26/2019
        /// </summary>
        protected override void HandleStaticGrab()
        {
            base.HandleStaticGrab();

            Vector3 rot = transform.localEulerAngles;

            float totalAngleX = Mathf.Abs(m_XConstraints.x - m_XConstraints.y);
            float totalAngleZ = Mathf.Abs(m_ZConstraints.x - m_ZConstraints.y);

            float angleAdjustedX = m_RawRotation.x + (totalAngleX / 2.0f);
            float angleAdjustedZ = m_RawRotation.z + (totalAngleZ / 2.0f);

            float anglePercentageX = angleAdjustedX / totalAngleX;
            float anglePercentageZ = angleAdjustedZ / totalAngleZ;

            m_AngleRatios.x = -((anglePercentageZ * 2) - 1);
            m_AngleRatios.y = ((anglePercentageX * 2) - 1);
            m_AngleRatios = new Vector2(float.IsNaN(m_AngleRatios.x) ? 0 : m_AngleRatios.x, float.IsNaN(m_AngleRatios.y) ? 0 : m_AngleRatios.y);
        }

        #endregion
    }
}
