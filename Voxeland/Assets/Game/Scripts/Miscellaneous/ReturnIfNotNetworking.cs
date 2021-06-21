using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ReturnIfNotNetworking : MonoBehaviour
{
    void Awake()
    {
        if (Mirror.NetworkManager.singleton is null)
            SceneHandler.ChangeScene(0);
    }
}
