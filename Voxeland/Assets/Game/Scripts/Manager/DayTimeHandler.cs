using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DayTimeHandler : MonoBehaviour
{
    [SerializeField] bool m_custom = false;
    [SerializeField] double m_timeOffset;
    [SerializeField] ParticleSystem m_sky;
    [SerializeField] GameObject m_sun;
    [SerializeField] GameObject m_moon;
    [SerializeField] Light m_directionalLight;
    [Range(0, 1440)]
    [SerializeField] double m_dayTime = 846;
    bool m_day = false;

    void Update()
    {
        if (!GameManager.Instance) return;
        if (!GameManager.Instance.m_MainCamera) return;
        if (GameManager.Instance.LOCKED) return;

        m_sun.transform.position = m_directionalLight.transform.forward * -400 + GameManager.Instance.m_MainCamera.transform.position;
        m_moon.transform.position = m_directionalLight.transform.forward * 400 + GameManager.Instance.m_MainCamera.transform.position;

        if (!m_custom)
            m_dayTime = System.DateTime.Now.TimeOfDay.TotalMinutes + m_timeOffset;
        m_directionalLight.transform.rotation = Quaternion.Euler((float)(m_dayTime - 360 ) * 0.25f, -30, 0);

        SetIntensityOfSun();

        if (m_day)
            m_sky.Stop();
        else
            m_sky.Play();
    }
    void SetIntensityOfSun()
    {
        float a = Vector3.Dot(Vector3.up, m_sun.transform.position - GameManager.Instance.m_MainCamera.transform.position);

        RenderSettings.ambientLight = Color.white * GameManager.Instance.Map(a, -50, 20, 0.65f, 1f);
        m_directionalLight.intensity = GameManager.Instance.Map(a, -20, 20, 0.001f, 1.3f);

        m_day = a > 0;
    }
}
