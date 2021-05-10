using UnityEngine;

public class LocalPlayerOnly : Mirror.NetworkBehaviour
{
    [SerializeField] private Camera m_camera;

    void Start()
    {
        // Network Move on LocalPlayer only
        if (isLocalPlayer)
        {
            if (GameManager.Instance)
            {
                GameManager.Instance.m_MainCamera = m_camera;
                GameManager.Instance.m_Player = gameObject;
            }
            return;
        }

        MonoBehaviour[] comps = GetComponents<MonoBehaviour>();
        foreach (MonoBehaviour c in comps)
            c.enabled = false;
        // GetComponent<...>().enabled = true;

        m_camera.enabled = false;
    }
}
