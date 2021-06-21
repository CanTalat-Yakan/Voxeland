using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowMainCamera : MonoBehaviour
{
    Vector3 m_initialPos;

    void Start() { m_initialPos = transform.position; }
    void Update()
    {
        if (GameManager.Instance)
            if (GameManager.Instance.m_MainCamera)
                transform.position = GameManager.Instance.m_MainCamera.transform.position + m_initialPos;
    }
}
