using System.Collections.Generic;
using System;
using UnityEngine;
using TMPro;
using Mirror;

public class Player : NetworkBehaviour
{
    public static event Action<Player, string> OnMessage;

    [SyncVar]
    public string playerName;
    [SyncVar]
    public string playerColor;
    [SyncVar]
    public int playerTexture;

    [SerializeField] Renderer[] m_renderer;
    [SerializeField] Canvas m_canvasNameTag;
    [SerializeField] Canvas m_canvasHUD;
    [SerializeField] TextMeshProUGUI m_nameTMPro;
    [SerializeField] Light m_pointLight;


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
    public void SetupCanvasHUD()
    {
        m_canvasHUD.gameObject.SetActive(true);
    }
    public void SetupCanvas()
    {
        m_canvasNameTag.gameObject.SetActive(true);
        m_canvasNameTag.worldCamera = GameManager.Instance.m_MainCamera;
        m_nameTMPro.text = playerName;
        m_canvasNameTag.gameObject.layer = LayerMask.NameToLayer("Draw over");
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
