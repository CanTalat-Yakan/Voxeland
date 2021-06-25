using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Mirror;

public class GetServerAddress : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI m_text;
    void Start()
    {
        m_text = GetComponent<TextMeshProUGUI>();
        if (NetworkManager.singleton.isNetworkActive)
        {
            m_text.text = NetworkManager.singleton.networkAddress;
            GUIUtility.systemCopyBuffer = NetworkManager.singleton.networkAddress;
        }
    }
}
