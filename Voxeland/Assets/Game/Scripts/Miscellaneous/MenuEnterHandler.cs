using System.Collections;
using System.Collections.Generic;
using Mirror;
using TMPro;
using UnityEngine;

public class MenuEnterHandler : MonoBehaviour
{
    [SerializeField] private TMP_InputField input;

    // Start is called before the first frame update
    void Start()
    {
        input.interactable = true;
        input.Select();
        input.ActivateInputField();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Return))
            NetworkManager.singleton.StartClient();
    }
}
