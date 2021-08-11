using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActiveOnKeyPress : MonoBehaviour
{
    [SerializeField] GameObject m_object;
    [SerializeField] bool m_hold = false;
    [SerializeField] KeyCode m_key;
    bool m_b = false;
    bool m_tmp = false;

    void Update()
    {
        if (m_hold)
            m_b = Input.GetKey(m_key);
        else if (Input.GetKeyDown(m_key))
            m_b = !m_b;

        if (m_tmp != m_b)
            m_object.SetActive(m_b);
        m_tmp = m_b;
    }
}
