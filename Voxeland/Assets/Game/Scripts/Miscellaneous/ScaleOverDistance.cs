using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScaleOverDistance : MonoBehaviour
{
    [SerializeField] float m_scale;
    [SerializeField] float m_distance = 50;
    [Range(0.1f, 1)]
    [SerializeField] float m_minScale = 0.5f;
    Vector3 m_initialScale;

    void Start()
    {
        m_initialScale = transform.localScale;
    }

    void Update()
    {
        if (GameManager.Instance)
            if (GameManager.Instance.m_MainCamera)
            {
                float dis = Vector3.Distance(GameManager.Instance.m_MainCamera.transform.position, transform.position);
                transform.localScale = m_initialScale * dis * m_scale * (1 - Mathf.Clamp(dis / m_distance, 0 , m_minScale));
            }
    }
}
