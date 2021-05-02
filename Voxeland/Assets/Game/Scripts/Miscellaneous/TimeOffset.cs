using UnityEngine;

public class TimeOffset : MonoBehaviour
{
    [SerializeField] Vector2 m_scrollSpeed = new Vector2(1, 1);
    [SerializeField] Renderer m_rend;


    void Update()
    {
        m_rend.material.mainTextureOffset = (m_scrollSpeed * Time.time) * 0.001f;
    }
}
