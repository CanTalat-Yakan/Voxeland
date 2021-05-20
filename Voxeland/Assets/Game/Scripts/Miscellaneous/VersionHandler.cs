using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class VersionHandler : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI m_versionTMPro;

    void OnValidate()
    {
        m_versionTMPro.text = Application.version;
    }
}