using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GetYRotation : MonoBehaviour
{
    [SerializeField] RectTransform m_transform;
    [SerializeField] float m_offSet;

    void Update()
    {
        if (GameManager.Instance)
            if (GameManager.Instance.m_Player)
                m_transform.rotation = Quaternion.Euler(0, 0, m_offSet + -GameManager.Instance.m_Player.transform.localRotation.eulerAngles.y);
    }
}
