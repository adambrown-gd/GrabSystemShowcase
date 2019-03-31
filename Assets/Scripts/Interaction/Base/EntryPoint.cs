using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ACSL;
using ACSL.TEx;
using ACSL.Utility;
using ACSL.ControllerInput;
using ACSL.Network;

namespace ACSL.Interaction
{
    public class EntryPoint : InteractObject
    {
        #region PUBLIC  VARIABLES

        public GameObject m_TransformEntryPoint;
        public GameObject m_TransformExitPoint;

        public bool m_HasExitPoint;
        public GameObject m_ExitPoint;

        public Vector3 m_LerpStartPoint;
        public Vector3 m_LerpEndPoint;

        public bool m_LerpToPoints;
        public bool m_TeleportToPoints;

        public bool m_DisableMovementOnEntry;
        public bool m_MakeKinematicOnEntry;

        #endregion
        #region PROTECTED METHODS

        protected bool m_Entered;
        protected ExitPoint m_PairedExitPoint;

        protected Timer m_Timer;

        #endregion
        #region PUBLIC METHODS

        /// <summary>
        /// override this method for specific 
        /// entry implementation (disabling certain objects, etc)
        /// Adam Brown : Last Updated 02/01/2019
        /// </summary>
        public virtual void Enter()
        {
            //not implemented.
        }
        /// <summary>
        /// override this method for specific 
        /// exit implementation (enabling certain objects, etc)
        /// Adam Brown : Last Updated 02/01/2019
        /// </summary>
        public virtual void Exit()
        {
            //not implemented.
        }

        #endregion
        #region PROTECTED METHODS

        protected override void GrabOnce(GrabPoint targetPoint)
        {
            if(m_LerpToPoints)
            {
                m_LerpStartPoint = GameUtils.LocalController.transform.position;
                m_LerpEndPoint = m_TransformEntryPoint.transform.position;

                m_Timer.NewTimer(0.5f);
                m_Timer.StartTimer();
            }
            else if(m_TeleportToPoints)
                GameUtils.LocalController.transform.position = m_TransformEntryPoint.transform.position;

            m_Entered = true;

            if (m_DisableMovementOnEntry)
                InputManager.Instance.ToggleKeybinding(typeof(Keybinding_Locomotion), false);

            if (m_MakeKinematicOnEntry)
            {
                Rigidbody rBody = GameUtils.LocalController.GetComponent<Rigidbody>();
                rBody.isKinematic = true;
                rBody.useGravity = true;
            }

            if (m_HasExitPoint)
            {
                m_ExitPoint.GetComponent<ExitPoint>().EnableInteract = true;
                EnableInteract = false;
            }

            Enter();
        }
        protected override void Start()
        {
            base.Start();

            m_Timer = gameObject.AddComponent<Timer>();
            m_Timer.NewTimer(0.5f);
            m_Timer.OnTimerUpdate += p => GameUtils.LocalController.transform.position = MathUtils.Vector3Lerp(m_LerpStartPoint, m_LerpEndPoint, p);
        }
        protected override void Init()
        {
            base.Init();

            if (m_HasExitPoint && m_ExitPoint != null)
            {
                m_PairedExitPoint = m_ExitPoint.AddComponent<ExitPoint>();
                m_PairedExitPoint.PairedEntryPoint = this;
                m_PairedExitPoint.EnableInteract = false;
            }
        }

        #endregion
        #region ACCESSORS

        public bool Entered
        {
            get { return m_Entered; }
            set { m_Entered = value; }
        }
        public ExitPoint PairedExitPoint
        {
            get { return m_PairedExitPoint; }
            set { m_PairedExitPoint = value; }
        }

        #endregion
    }
}
