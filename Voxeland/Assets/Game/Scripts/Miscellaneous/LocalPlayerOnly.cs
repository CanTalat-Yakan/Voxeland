using UnityEngine;

public class LocalPlayerOnly : Mirror.NetworkBehaviour
{
    void Start()
    {
        // Network Move on LocalPlayer only
        if (isLocalPlayer)
            return;

        MonoBehaviour[] comps = GetComponents<MonoBehaviour>();
        foreach (MonoBehaviour c in comps)
            c.enabled = false;

        // GetComponent<...>().enabled = true;
    }
}
