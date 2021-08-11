using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Mirror;
using UnityEngine;

public class GameManager : NetworkBehaviour
{
    public static GameManager Instance { get; private set; }
    public List<Player> ClientList { get; private set; }
    public bool IsClient { get; private set; }
    public bool IsServer { get; private set; }
    public bool IsHost { get => IsClient && IsServer; }

    [SerializeField] internal NodeGeneration m_generation;
    [SerializeField] internal VoxelMaster m_VoxelMaster;
    [SerializeField] internal GameObject m_LoadingScreen;
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

        ClientList = new List<Player>();
        IsClient = isClient;
        IsServer = isServer;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        LOCKED = false;

        if (m_MainCamera is null)
            m_MainCamera = Camera.main;
    }

    internal void SpawnPlayer(GameObject _player)
    {
        StartCoroutine(SpawnPlayerCoroutine(_player));
    }
    IEnumerator SpawnPlayerCoroutine(GameObject _player)
    {
        m_LoadingScreen.SetActive(true);
        m_Player = _player;
        m_Player.SetActive(false);

        Vector3 spawnPositin = Vector3.zero + Vector3.one * 0.5f + Vector3.up * (m_generation.GetSurfaceHeigth(0, 0) - 15 + 2);
        m_Player.transform.position = spawnPositin;

        yield return new WaitWhile(() => !BoolRayCast(3, new Ray(spawnPositin, Vector3.down)));

        m_VoxelMaster.FastRefresh();
        m_Player.SetActive(true);
        m_LoadingScreen.SetActive(false);

        yield return null;
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

    internal void AddClient(Player _p)
    {
        if (IsClient) ClientList.Add(_p);

        foreach (var item in ClientList)
            Debug.Log(item);
    }
    internal void RemoveClient(Player _p)
    {
        if (IsClient) if (ClientList.Contains(_p)) ClientList.Remove(_p);

        foreach (var item in ClientList)
            Debug.Log(item);
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
    public static float Remap(this float _value, float _oldMin, float _oldMax, float _newMin, float _newMax)
    {
        return (_value - _oldMin) / (_oldMax - _oldMin) * (_newMax - _newMin) + _newMin;
    }
}