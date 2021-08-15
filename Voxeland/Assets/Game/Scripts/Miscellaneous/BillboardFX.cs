using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BillboardFX : MonoBehaviour
{
    Quaternion m_originalRotation;

    void Start()
    {
        m_originalRotation = transform.localRotation;
    }

    void Update()
    {
        if (GameManager.Instance)
            if (GameManager.Instance.m_MainCamera)
                transform.rotation = Quaternion.LookRotation(transform.position - GameManager.Instance.m_MainCamera.transform.position) * m_originalRotation;
    }
}