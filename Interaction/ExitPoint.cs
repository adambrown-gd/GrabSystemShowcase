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
    public class ExitPoint : InteractObject
    {
        protected EntryPoint m_PairedEntryPoint;
        protected Timer m_Timer;

        public EntryPoint PairedEntryPoint
        {
            get { return m_PairedEntryPoint; }
            set { m_PairedEntryPoint = value; }
        }
        public Timer Timer
        {
            get { return m_Timer; }
            set { m_Timer = value; }
        }
        protected override void GrabOnce(GrabPoint targetPoint)
        {
            if(m_PairedEntryPoint.m_LerpToPoints)
            {
                m_PairedEntryPoint.m_LerpStartPoint = GameUtils.LocalController.transform.position;
                m_PairedEntryPoint.m_LerpEndPoint = m_PairedEntryPoint.m_TransformExitPoint.transform.position;

                m_Timer.NewTimer(2.0f);
                m_Timer.StartTimer();
            }
            else if(m_PairedEntryPoint.m_TeleportToPoints)
                GameUtils.LocalController.transform.position = m_PairedEntryPoint.m_TransformExitPoint.transform.position;

            m_PairedEntryPoint.Entered = false;

            if (m_PairedEntryPoint.m_DisableMovementOnEntry)
                InputManager.Instance.ToggleKeybinding(typeof(Keybinding_Locomotion), true);

            if (m_PairedEntryPoint.m_MakeKinematicOnEntry)
            {
                Rigidbody rBody = GameUtils.LocalController.GetComponent<Rigidbody>();
                rBody.isKinematic = false;
                rBody.useGravity = true;
            }

            m_PairedEntryPoint.EnableInteract = true;
            EnableInteract = false;

            m_PairedEntryPoint.Exit();
        }
        protected override void Start()
        {
            base.Start();

            m_Timer = gameObject.AddComponent<Timer>();
            m_Timer.NewTimer(2.0f);
            m_Timer.OnTimerUpdate += p => GameUtils.LocalController.transform.position = MathUtils.Vector3Lerp(m_PairedEntryPoint.m_LerpStartPoint, m_PairedEntryPoint.m_LerpEndPoint, p);
        }
        protected override void Init()
        {
            base.Init();
        }

    }
}
