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

        public static event Action<Player, string> OnMessage;

        public void SetupMaterial()
        {
            ColorUtility.TryParseHtmlString(playerColor, out Color newCol);
            if (!string.IsNullOrEmpty(playerColor))
                foreach (var item in m_renderer)
                {
                    item.material.SetTexture("_BaseMap", GameManager.Instance.m_Settings.SkinTextures[playerTexture]);
                    item.material.SetColor("_BaseColor", newCol);
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
        }
        public void SetShadowCastOnly()
        {
            foreach (var item in m_renderer)
                item.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.ShadowsOnly;
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
