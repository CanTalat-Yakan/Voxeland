using Mirror;
using UnityEngine;

public class LocalPlayerOnly : Mirror.NetworkBehaviour
{
    [SerializeField] private GameObject m_cameraPivot;

    void Start()
    {
        Player player = GetComponent<Player>();


        if (isLocalPlayer) // Network Move on LocalPlayer only
        {
            GameManager.Instance.m_MainCamera.transform.parent = m_cameraPivot.transform;
            GameManager.Instance.m_MainCamera.transform.localPosition = Vector3.zero;
            GameManager.Instance.m_Player = gameObject;

            player.SetShadowCastOnly();
            player.DisablePointLight();
            player.SetupCanvasHUD();
            GameManager.Instance.m_MainCamera.GetComponent<AudioListener>().enabled = true;
        }
        else
        {
            player.SetupLayer(gameObject, LayerMask.NameToLayer("Client"));
            player.SetupCanvas();
            player.SetupMaterial();
            player.SetupPointLight();
            GetComponent<Rigidbody>().Sleep();
            GetComponent<CapsuleCollider>().enabled = false;

            MonoBehaviour[] comps = GetComponents<MonoBehaviour>();
            foreach (MonoBehaviour c in comps)
                c.enabled = false;

            GetComponent<NetworkTransform>().enabled = true;
            GetComponent<PlayerInputHandler>().enabled = true;
            GetComponent<NetworkAnimator>().enabled = true;
        }
    }
}
