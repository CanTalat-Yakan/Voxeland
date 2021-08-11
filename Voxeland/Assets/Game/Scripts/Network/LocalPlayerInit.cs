using Mirror;
using UnityEngine;

public class LocalPlayerInit : NetworkBehaviour
{
    [SerializeField] private GameObject m_cameraPivot;
    Player m_player;

    void Start()
    {
        m_player = GetComponent<Player>();

        if (isLocalPlayer) // Network Move on LocalPlayer only
        {
            GameManager.Instance.m_MainCamera.transform.parent = m_cameraPivot.transform;
            GameManager.Instance.m_MainCamera.transform.localPosition = Vector3.zero;
            GameManager.Instance.SpawnPlayer(gameObject);

            m_player.SetShadowCastOnly();
            m_player.DisablePointLight();
            m_player.SetupCanvasHUD();
        }
        else
        {
            GameManager.Instance.AddClient(m_player);

            m_player.SetupLayer(gameObject, LayerMask.NameToLayer("Client"));
            m_player.SetupCanvas();
            m_player.SetupMaterial();
            m_player.SetupPointLight();
            
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

    void OnDestroy()
    {
        if (GameManager.Instance)
            GameManager.Instance.RemoveClient(m_player);
    }
}
