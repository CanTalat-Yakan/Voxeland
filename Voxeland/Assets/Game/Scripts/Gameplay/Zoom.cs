using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Zoom : MonoBehaviour
{
    Camera m_camera;
    [SerializeField] int m_zoomFOV = 30;

    void Start()
    {
    }

    void LateUpdate()
    {
        if (m_camera is null)
        {
            if (GameManager.Instance)
                if (GameManager.Instance.m_MainCamera)
                    m_camera = GameManager.Instance.m_MainCamera;
        }
        else if (Input.GetKey(KeyCode.LeftAlt))
            m_camera.fieldOfView = m_zoomFOV;
    }
}
