using System; 
using System.Linq; 
using System.Collections;
using System.Collections.Generic;

using UnityEngine;

using Photon.Pun;


namespace ACSL.Utility
{
    public delegate void TimerMoment(float p);

    public class Timer : MonoBehaviour
    {
        private float m_CurrentTime;
        private float m_MaxTime;

        private bool m_HasStarted;
        private bool m_HasFinished;

        public event TimerMoment OnTimerUpdate;
        public event TimerMoment OnTimerEnd;

        public float MaxTime { get { return m_MaxTime; } }
        public float TimeLeft { get { return m_CurrentTime; } }
        public float Percentage { get { return m_CurrentTime / m_MaxTime; } }
        public bool Started { get { return m_HasStarted; } }
        public bool Finished { get { return m_HasFinished; } }

        public void NewTimer(float max, bool clearEvents = false)
        {
            if (clearEvents)
            {
                Delegate.RemoveAll(OnTimerUpdate, OnTimerUpdate);
                Delegate.RemoveAll(OnTimerEnd, OnTimerEnd);
            }

            m_CurrentTime = 0;
            m_MaxTime = max;
            m_HasStarted = false;
            m_HasFinished = false;
        }
        public void StartTimer()
        {
            m_HasStarted = true;
            m_CurrentTime = 0;
        }
        public void Stop()
        {
            m_HasStarted = false;
        }
        public void Update()
        {
            if (m_HasStarted && !Finished)
            {
                m_CurrentTime += Time.deltaTime;
                m_HasFinished = m_CurrentTime >= MaxTime;
                m_CurrentTime = Finished ? MaxTime : m_CurrentTime;

                OnTimerUpdate?.Invoke(Percentage);
            }
            if (m_HasStarted && Finished)
            {
                m_HasStarted = false;
                Stop();

                OnTimerEnd?.Invoke(Percentage);
            }
        }
        public void OnDestroy()
        {
            NewTimer(0, true);
        }
    }
}