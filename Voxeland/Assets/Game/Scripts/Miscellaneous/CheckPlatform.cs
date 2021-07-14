using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CheckPlatform : MonoBehaviour
{
    [SerializeField] RuntimePlatform m_checkPlatform;

    void Awake()
    {
        if (Application.platform != m_checkPlatform)
            gameObject.SetActive(false);
    }
}
