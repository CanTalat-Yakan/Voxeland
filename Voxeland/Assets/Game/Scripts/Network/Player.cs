using System;
using UnityEngine;
using TMPro;

namespace Mirror
{
    public class Player : NetworkBehaviour
    {
        [SyncVar]
        public string playerName;
        [SyncVar]
        public string playerColor;
        [SyncVar]
        public int playerTexture;

        [SerializeField] private Renderer[] m_renderer;
        [SerializeField] private Canvas m_canvas;
        [SerializeField] private TextMeshProUGUI m_nameTMPro;
        [SerializeField] private Light m_pointLight;

        public static event Action<Player, string> OnMessage;

        public void SetupMaterial()
        {
            ColorUtility.TryParseHtmlString(playerColor, out Color newCol);
            if (!string.IsNullOrEmpty(playerColor))
            {
                m_renderer[0].material.SetTexture("_BaseMap", GameManager.Instance.m_Settings.SkinHeadTextures[playerTexture]);
                m_renderer[0].material.SetColor("_BaseColor", newCol);

                m_renderer[1].material.SetTexture("_BaseMap", GameManager.Instance.m_Settings.SkinTopTextures[playerTexture]);
                m_renderer[1].material.SetColor("_BaseColor", newCol);

                m_renderer[2].material.SetTexture("_BaseMap", GameManager.Instance.m_Settings.SkinBottomTextures[playerTexture]);
                m_renderer[2].material.SetColor("_BaseColor", newCol);
            }
        }
        public void SetupLayer(GameObject go, int layerNumber)
        {
            foreach (Transform trans in go.GetComponentsInChildren<Transform>(true))
                trans.gameObject.layer = layerNumber;
        }
        public void SetupCanvas()
        {
            m_canvas.gameObject.SetActive(true);
            m_canvas.worldCamera = GameManager.Instance.m_MainCamera;
            m_nameTMPro.text = playerName;
            m_canvas.gameObject.layer = LayerMask.NameToLayer("Draw over");
        }
        public void SetShadowCastOnly()
        {
            foreach (var item in m_renderer)
                item.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.ShadowsOnly;
        }
        public void SetupPointLight()
        {
            ColorUtility.TryParseHtmlString(playerColor, out Color newCol);
            m_pointLight.color = newCol;
        }
        public void DisablePointLight()
        {
            m_pointLight.enabled = false;
        }

        [Command]
        public void CmdSend(string message)
        {
            Debug.Log(message.Trim());
            if (message.Trim() != "")
                RpcReceive(message.Trim());
        }

        [ClientRpc]
        public void RpcReceive(string message)
        {
            OnMessage?.Invoke(this, message);
        }
    }
}
