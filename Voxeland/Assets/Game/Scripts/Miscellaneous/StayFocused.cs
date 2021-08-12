using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class StayFocused : MonoBehaviour
{
    TMP_InputField m_inputField;

    void Start()
    {
        m_inputField = GetComponent<TMP_InputField>();
    }

    void Update()
    {
        if (!string.IsNullOrEmpty(m_inputField.text))
        {
            m_inputField.interactable = true;
            m_inputField.ActivateInputField();
            m_inputField.Select();
        }
    }
}
