using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Zoom : MonoBehaviour
{
    Camera m_camera;
    [SerializeField] int m_zoomFOV = 30;
    [SerializeField] KeyCode m_key = KeyCode.C;
    ECM.Components.MouseLook m_mouseLook;

    void Start()
    {
        m_mouseLook = GetComponent<ECM.Components.MouseLook>();
    }

    void LateUpdate()
    {
        m_mouseLook.smooth = false;

        if (m_camera is null)
        {
            if (GameManager.Instance)
                if (GameManager.Instance.m_MainCamera)
                    m_camera = GameManager.Instance.m_MainCamera;
        }
        else if (Input.GetKey(m_key) && !GameManager.Instance.LOCKED)
        {
            m_camera.fieldOfView = m_zoomFOV;
            m_mouseLook.smooth = true;
        }

    }
}
