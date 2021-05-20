using UnityEngine;

public class OnlyWindowsPlatform : MonoBehaviour
{
    // Start is called before the first frame update
    void Awake()
    {

#if !UNITY_EDITOR
        if (Application.platform != RuntimePlatform.WindowsPlayer)
            gameObject.SetActive(false);
#endif
    }
}
