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
            player.DisablePointLight();
            m_camera.GetComponent<AudioListener>().enabled = true;

            return;
        }
        player.SetupLayer(gameObject, LayerMask.NameToLayer("Client"));
        player.SetupCanvas();
        player.SetupMaterial();
        player.SetupPointLight();
        GetComponent<Rigidbody>().Sleep();
        GetComponent<CapsuleCollider>().enabled = false;

        MonoBehaviour[] comps = GetComponents<MonoBehaviour>();
        foreach (MonoBehaviour c in comps)
            c.enabled = false;
        m_camera.gameObject.SetActive(false);

        GetComponent<NetworkTransform>().enabled = true;
        GetComponent<AnimationController>().enabled = true;
        GetComponent<NetworkAnimator>().enabled = true;
    }
}
