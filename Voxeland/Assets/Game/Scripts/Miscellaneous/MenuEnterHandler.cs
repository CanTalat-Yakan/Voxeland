using System.Collections;
using System.Collections.Generic;
using Mirror;
using TMPro;
using UnityEngine;

public class MenuEnterHandler : MonoBehaviour
{
    [SerializeField] private TMP_InputField m_input;
    [SerializeField] private TMP_InputField m_ip;
    [SerializeField] private GameObject m_host;
    [SerializeField] private NetworkManager m_manager;


    void Start()
    {
        m_input.interactable = true;
        m_input.Select();
        m_input.ActivateInputField();
        m_host.SetActive(Application.platform != RuntimePlatform.WebGLPlayer);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Return))
            JoinServer();
    }

    public void JoinServer()
    {
        m_manager.networkAddress = "18.159.34.101";
        m_manager.StartClient();
    }

    public void JoinAsClient()
    {
        m_manager.networkAddress = string.IsNullOrEmpty(m_ip.text) ? "localhost" : m_ip.text;
        m_manager.StartClient();
    }

    public void JoinAsHost()
    {
        m_manager.StartHost();
    }
}
