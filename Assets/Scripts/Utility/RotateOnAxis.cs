using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ACSL.Utility;

public class RotateOnAxis : MonoBehaviour
{
    public bool m_Oscillate;
    public Vector2 m_OscillateLimits;

    public bool m_RotateX;
    public bool m_RotateY;
    public bool m_RotateZ;

    public float m_Speed;
    private bool m_Inverted;
    public void Update()
    {
        Vector3 rotation = transform.eulerAngles;

        MathUtils.ClampAngle(rotation.x, m_OscillateLimits.x, m_OscillateLimits.y);
        MathUtils.ClampAngle(rotation.y, m_OscillateLimits.x, m_OscillateLimits.y);
        MathUtils.ClampAngle(rotation.z, m_OscillateLimits.x, m_OscillateLimits.y);

        if (m_RotateX)
        {
            if (m_Oscillate)
                if (rotation.x >= m_OscillateLimits.y || rotation.x <= m_OscillateLimits.x)
                    m_Inverted = !m_Inverted;

            rotation.x += (m_Inverted ? -m_Speed : m_Speed) * Time.smoothDeltaTime;
        }
        if (m_RotateY)
        {
            if (m_Oscillate)
                if (rotation.y >= m_OscillateLimits.y || rotation.y <= m_OscillateLimits.x)
                    m_Inverted = !m_Inverted;

            rotation.y += (m_Inverted ? -m_Speed : m_Speed) * Time.smoothDeltaTime;
        }
        if (m_RotateZ)
        {
            if (m_Oscillate)
                if (rotation.z >= m_OscillateLimits.y || rotation.z <= m_OscillateLimits.x)
                    m_Inverted = !m_Inverted;

            rotation.z += (m_Inverted ? -m_Speed : m_Speed) * Time.smoothDeltaTime;
        }

        transform.rotation = Quaternion.Euler(rotation);
    }
}
