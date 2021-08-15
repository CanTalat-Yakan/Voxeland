using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FadeSprite : MonoBehaviour
{
    Image m_img;
    Sprite m_lastTexture;

    void Start()
    {
        m_img = GetComponent<Image>();
        StartCoroutine(StarFade(2, 0.33f));
    }

    // Update is called once per frame
    void Update()
    {
        if (m_img.sprite != m_lastTexture)
        {
            StopAllCoroutines();
            StartCoroutine(StarFade(2, 0.33f));
        }
        m_lastTexture = m_img.sprite;
    }

    //Fades Sprite in GUI away when not changed;
    IEnumerator StarFade(float _duration, float _fadeOutDuration)
    {
        m_img.color = Color.white;

        float startTime = Time.time;
        float endTime = startTime + _duration;
        yield return new WaitUntil(() => Time.time > endTime);

        startTime = Time.time;
        endTime = startTime + _fadeOutDuration;
        while (Time.time < endTime)
        {
            float value = (Time.time - startTime) / _fadeOutDuration;
            m_img.color = new Color(1, 1, 1, Mathf.Lerp(1, 0, value));
            yield return new WaitForEndOfFrame();
        }
        m_img.color = new Color(1, 1, 1, 0);

        yield return null;
    }
}
