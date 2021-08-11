using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GetColor : MonoBehaviour
{
    Image image;

    void Start()
    {
        image = GetComponent<Image>();
        StartCoroutine(SetImageColor());
    }

    IEnumerator SetImageColor()
    {
        yield return new WaitWhile(() => GameManager.Instance is null);
        yield return new WaitWhile(() => GameManager.Instance.m_Player is null);

        string colString = GameManager.Instance.m_Player.GetComponent<Player>().playerColor;
        ColorUtility.TryParseHtmlString(colString, out Color col);
        image.color = col;

        yield return null;
    }
}
