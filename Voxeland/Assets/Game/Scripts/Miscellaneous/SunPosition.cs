using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SunPosition : MonoBehaviour
{
    [SerializeField] Light m_directionalLight;
    [Range(0, 1440)]
    [SerializeField] double m_dayTime = 846;
    bool m_day = false;

    void Update()
    {
        transform.position = m_directionalLight.transform.forward * -500;

        m_dayTime = System.DateTime.Now.TimeOfDay.TotalMinutes;
        m_directionalLight.transform.rotation = Quaternion.Euler((float)(m_dayTime - 360) * 0.25f, -30, 0);

        SetIntensityOfSun();

    }
    void SetIntensityOfSun()
    {
        float a = Vector3.Dot(Vector3.up, gameObject.transform.position - GameManager.Instance.m_MainCamera.transform.position);

        RenderSettings.ambientLight = Color.white * GameManager.Instance.Map(a, -50, 20, 0.65f, 1f);
        m_directionalLight.intensity = GameManager.Instance.Map(a, -20, 20, 0, 1.3f);

        m_day = a < 0;
    }
}
