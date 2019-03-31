using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;

using ACSL.Utility;
using ACSL.ControllerInput;
using ACSL.Network;
using ACSL.Interaction;

namespace ACSL.Interaction
{
    public class LadderEntryPoint : EntryPoint
    {
        public override void Enter()
        {
            m_Timer.OnTimerEnd += InLadder;
            GetComponentsInChildren<LadderRung>().ToList().ForEach(lr => lr.EnableInteract = true);
        }
        public override void Exit()
        {
            GrabPoint.LeftHand.ClearHand();
            GrabPoint.RightHand.ClearHand();

            m_PairedExitPoint.Timer.OnTimerEnd += OutLadder;
            GetComponentsInChildren<LadderRung>().ToList().ForEach(lr => lr.EnableInteract = false);
        }
        public void InLadder(float p)
        {
            InputManager.Instance.ToggleKeybinding(typeof(Keybinding_Locomotion), false);

            Rigidbody rBody = GameUtils.LocalController.GetComponent<Rigidbody>();
            rBody.isKinematic = true;
            rBody.useGravity = true;

            m_Timer.OnTimerEnd -= InLadder;
        }
        public void OutLadder(float p)
        {
            InputManager.Instance.ToggleKeybinding(typeof(Keybinding_Locomotion), true);

            Rigidbody rBody = GameUtils.LocalController.GetComponent<Rigidbody>();
            rBody.isKinematic = false;
            rBody.useGravity = true;

            m_PairedExitPoint.Timer.OnTimerEnd -= OutLadder;
        }
        protected override void Init()
        {
            base.Init();

            if (m_PairedExitPoint)
                Destroy(m_PairedExitPoint);

            //find the ladder rung the farthest distance away from the entry point, and replace it with an exit point
            m_ExitPoint = GetComponentsInChildren<LadderRung>(true).Aggregate((lr, next) => Vector3.Distance(m_TransformEntryPoint.transform.position, next.transform.position) > Vector3.Distance(m_TransformEntryPoint.transform.position, lr.transform.position) ? next : lr).gameObject;
            Destroy(m_ExitPoint.GetComponent<LadderRung>());
            m_ExitPoint.name = "ExitPoint";
            m_PairedExitPoint = m_ExitPoint.AddComponent<ExitPoint>();
            m_PairedExitPoint.PairedEntryPoint = this;
            m_PairedExitPoint.EnableInteract = false;
        }
    }
}
