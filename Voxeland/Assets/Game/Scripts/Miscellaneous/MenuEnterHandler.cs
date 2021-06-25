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


    // Start is called before the first frame update
    void Start()
    {
        input.interactable = true;
        input.Select();
        input.ActivateInputField();
        host.SetActive(Application.platform != RuntimePlatform.WebGLPlayer);
    }

    // Update is called once per frame
    void Update()
    {
        if (!Input.GetKeyDown(KeyCode.Return)) return;

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
