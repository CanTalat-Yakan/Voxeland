using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GetColor : MonoBehaviour
{
    Image m_image;

    void Start()
    {
        m_image = GetComponent<Image>();
        StartCoroutine(SetImageColor());
    }

    IEnumerator SetImageColor()
    {
        yield return new WaitWhile(() => GameManager.Instance is null);
        yield return new WaitWhile(() => GameManager.Instance.m_Player is null);

        string colString = GameManager.Instance.m_Player.GetComponent<Player>().playerColor;
        ColorUtility.TryParseHtmlString(colString, out Color col);
        m_image.color = col;

        yield return null;
    }
}
