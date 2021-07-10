using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [SerializeField] internal VoxelMaster m_VoxelMaster;
    [SerializeField] internal bool LOCKED = false;
    [SerializeField] internal bool m_ShowDebugInfo = false;
    [SerializeField] internal SettingsContainer m_Settings;
    [SerializeField] internal LayerMask m_IgnoreLayer;
    [SerializeField] internal Camera m_MainCamera;
    internal GameObject m_Player;

    void Awake()
    {
        if (Instance is null)
            Instance = this;
        else
        {
            Destroy(gameObject);
            return;
        }

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        LOCKED = false;

        if (m_MainCamera is null)
            m_MainCamera = Camera.main;
    }
    void OnDestroy()
    {
        SceneHandler.UnloadScene("Chat");
        Instance = null;
    }

    void Start()
    {
        SceneHandler.AddScene("Chat");
    }

    void Update()
    {
        if (m_MainCamera)
            m_MainCamera.fieldOfView = m_Settings.FOV;

        if (Input.GetButtonDown("Cancel"))
            OptionsOverlay();

        if (m_Player)
            if (m_Player.transform.position.y <= -130)
                m_Player.transform.position = m_Player.transform.position + Vector3.up * 256;

        // Time.timeScale = LOCKED ? 0 : 1;
        if (Input.GetKeyDown(KeyCode.F2))
            m_ShowDebugInfo = !m_ShowDebugInfo;

        if (!LOCKED)
        {
            if (Input.GetKey(KeyCode.F2))
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }
            else
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }
        }
    }
    void OptionsOverlay()
    {
        LOCKED = !LOCKED;

        //Pause
        if (LOCKED)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;

            if (!SceneHandler.IsSceneLoaded("Options"))
                SceneHandler.AddScene("Options");
        }
        //Continue
        else
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;

            if (SceneHandler.IsSceneLoaded("Options"))
                SceneHandler.UnloadScene("Options");
        }
    }

    internal RaycastHit HitRayCast(float _maxDistance, Ray? _ray = null)
    {
        RaycastHit hit;

        Physics.Raycast(
            _ray is null
                ? m_MainCamera.ViewportPointToRay(new Vector2(0.5f, 0.5f))
                : _ray.Value,
            out hit,
            _maxDistance,
            ~m_IgnoreLayer);

        return hit;
    }
    internal bool BoolRayCast(float _maxDistance, Ray? _ray = null)
    {
        return Physics.Raycast(
            _ray is null
                ? m_MainCamera.ViewportPointToRay(new Vector2(0.5f, 0.5f))
                : _ray.Value,
            _maxDistance,
            ~m_IgnoreLayer);
    }
    internal static float Map(float _oldValue, float _oldMin, float _oldMax, float _newMin, float _newMax)
    {
        float oldRange = _oldMax - _oldMin;
        float newRange = _newMax - _newMin;
        float newValue = ((_oldValue - _oldMin) * newRange / oldRange) + _newMin;

        return Mathf.Clamp(newValue, _newMin, _newMax);
    }
}
public static class ExtensionMethods
{
    public static float Remap(this float value, float from1, float to1, float from2, float to2)
    {
        return (value - from1) / (to1 - from1) * (to2 - from2) + from2;
    }
}