using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using Mirror;

public class SceneHandler : MonoBehaviour
{
    public static void ChangeScene(int _i)
    {
        Time.timeScale = 1;
        SceneManager.LoadScene(_i);
    }

    public static void ChangeScene(string _s)
    {
        Time.timeScale = 1;
        SceneManager.LoadScene(_s);
    }

    public static void AddScene(int _i)
    {
        SceneManager.LoadScene(_i, LoadSceneMode.Additive);
    }

    public static void AddScene(string _s)
    {
        SceneManager.LoadScene(_s, LoadSceneMode.Additive);
    }

    internal static bool IsSceneLoaded(int _i)
    {
        return SceneManager.GetSceneByBuildIndex(_i).isLoaded;
    }

    internal static bool IsSceneLoaded(string _s)
    {
        return SceneManager.GetSceneByName(_s).isLoaded;
    }

    public static void UnloadScene(int _i)
    {
        SceneManager.UnloadSceneAsync(_i);
    }

    public static void UnloadScene(string _s)
    {
        SceneManager.UnloadSceneAsync(_s);
    }

    public static void Locked(bool _b)
    {
        if (GameManager.Instance)
            GameManager.Instance.LOCKED = _b;
    }

    public static void Continue(string _s)
    {
        if (GameManager.Instance)
        {
            GameManager.Instance.LOCKED = false;

            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        SceneManager.UnloadSceneAsync(_s);
    }

    public static void DestroyGameManager()
    {
        Destroy(GameManager.Instance.gameObject);
    }

    public static void Quit()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    public static void Leave()
    {
        NetworkManager.singleton.StopClient();
    }
}
