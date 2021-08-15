using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ZoomMap : MonoBehaviour
{
    [SerializeField] Camera m_cam;

//Handles Camera Rendersize of MiniMap displayed in HUD
    void Update()
    {
        if (Input.GetKey(KeyCode.PageDown))
            m_cam.orthographicSize += 40 * Time.deltaTime;
        if (Input.GetKey(KeyCode.PageUp))
            m_cam.orthographicSize -= 40 * Time.deltaTime;

        m_cam.orthographicSize = Mathf.Clamp(m_cam.orthographicSize, 15, 330);
    }
}
