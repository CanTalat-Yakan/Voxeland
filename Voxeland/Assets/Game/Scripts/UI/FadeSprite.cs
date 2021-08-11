using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FadeSprite : MonoBehaviour
{
    Image img;
    Sprite lastTexture;
    void Start()
    {
        img = GetComponent<Image>();
        StartCoroutine(StarFade(2, 0.33f));
    }

    // Update is called once per frame
    void Update()
    {
        if (img.sprite != lastTexture)
        {
            StopAllCoroutines();
            StartCoroutine(StarFade(2, 0.33f));
        }
        lastTexture = img.sprite;
    }

    IEnumerator StarFade(float _duration, float _fadeOutDuration)
    {
        img.color = Color.white;

        float startTime = Time.time;
        float endTime = startTime + _duration;
        yield return new WaitUntil(() => Time.time > endTime);

        startTime = Time.time;
        endTime = startTime + _fadeOutDuration;
        while (Time.time < endTime)
        {
            float value = (Time.time - startTime) / _fadeOutDuration;
            img.color = new Color(1, 1, 1, Mathf.Lerp(1, 0, value));
            yield return new WaitForEndOfFrame();
        }
        img.color = new Color(1, 1, 1, 0);

        yield return null;
    }
}
