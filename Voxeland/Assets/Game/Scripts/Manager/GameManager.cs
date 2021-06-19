using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;
using VoxelEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public bool LOCKED;
    public Camera m_MainCamera;
    public GameObject m_Player;
    public LayerMask m_PlayerLayer;
    public LayerMask m_EntityLayer;
    public Settings_Container m_Settings;

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

    internal RaycastHit HitRayCast(Vector3 _origin, Vector3 _direction, float _maxDistance)
    {
        RaycastHit hit;

        Ray ray = new Ray(
            _origin,
            _direction);

        Physics.Raycast(
            ray,
            out hit,
            _maxDistance,
            ~m_PlayerLayer & ~m_EntityLayer);

        return hit;
    }
    internal bool BoolRayCast(Vector3 _origin, Vector3 _direction, float _maxDistance)
    {
        Ray ray = new Ray(
            _origin,
            _direction);

        return Physics.Raycast(
            ray,
            _maxDistance,
            ~m_PlayerLayer & ~m_EntityLayer);
    }
}
