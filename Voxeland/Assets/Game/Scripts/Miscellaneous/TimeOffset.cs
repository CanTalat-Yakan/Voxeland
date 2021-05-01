using UnityEngine;

public class TimeOffset : MonoBehaviour
{
    [SerializeField] float scrollSpeed = 0.01f;
    Renderer m_rend;

    void Start()
    {
        m_rend = GetComponent<Renderer>();
    }

    void Update()
    {
        float offset = Time.time * scrollSpeed;
        m_rend.material.mainTextureOffset = (new Vector2(offset, 0));
    }
}
