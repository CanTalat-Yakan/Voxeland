using Mirror;
using UnityEngine;

public class LocalPlayerOnly : Mirror.NetworkBehaviour
{
    [SerializeField] private Camera m_camera;

    void Start()
    {
        Player player = GetComponent<Player>();

        // Network Move on LocalPlayer only
        if (isLocalPlayer)
        {
            if (GameManager.Instance)
            {
                GameManager.Instance.m_MainCamera = m_camera;
                GameManager.Instance.m_Player = gameObject;
            }
            player.SetShadowCastOnly();

            return;
        }
        player.SetupCanvas();
        player.SetupLayer(gameObject, LayerMask.NameToLayer("Client"));
        player.SetupMaterial();
        GetComponent<Rigidbody>().Sleep();
        GetComponent<CapsuleCollider>().enabled = false;

        MonoBehaviour[] comps = GetComponents<MonoBehaviour>();
        foreach (MonoBehaviour c in comps)
            c.enabled = false;
        m_camera.gameObject.SetActive(false);

        GetComponent<NetworkTransform>().enabled = true;
    }
}
