using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CheckNetwork : MonoBehaviour
{
    void Awake()
    {
        if (Mirror.NetworkManager.singleton is null)
            SceneHandler.Leave();
    }
}
