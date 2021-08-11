using System.Collections;
using System.Collections.Generic;
using Mirror;
using TMPro;
using UnityEngine;

public class MenuEnterHandler : MonoBehaviour
{
    [SerializeField] private TMP_InputField input;
    [SerializeField] private TMP_InputField ip;
    [SerializeField] private GameObject host;
    [SerializeField] private NetworkManager manager;


    void Start()
    {
        input.interactable = true;
        input.Select();
        input.ActivateInputField();
        host.SetActive(Application.platform != RuntimePlatform.WebGLPlayer);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Return))
            JoinServer();
    }

    public void JoinServer()
    {
        manager.networkAddress = "18.159.34.101";
        manager.StartClient();
    }

    public void JoinAsClient()
    {
        manager.networkAddress = string.IsNullOrEmpty(ip.text) ? "localhost" : ip.text;
        manager.StartClient();
    }

    public void JoinAsHost()
    {
        manager.StartHost();
    }
}
