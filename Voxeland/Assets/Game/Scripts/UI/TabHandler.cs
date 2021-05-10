using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TabHandler : MonoBehaviour
{
    [SerializeField] bool firstPanelOpen = true;
    [SerializeField] List<GameObject> m_panels = new List<GameObject>();

    int? m_panelIndex = 0;
    int m_previousPanelIndex;


    void Start()
    {
        m_previousPanelIndex = m_panelIndex.Value;

        if (m_panels.Count == 0)
            return;

        if (firstPanelOpen)
        {
            m_panels[0].gameObject.SetActive(true);

            m_panelIndex = 0;
            for (int i = 1; i < m_panels.Count; i++)
                m_panels[i].gameObject.SetActive(false);
        }
        else
            CloseAllPanelsAndIndexNull(true);
    }

    void ShowCurrentPanel()
    {
        for (int i = 0; i < m_panels.Count; i++)
            if (m_panelIndex != null)
                if (i == m_panelIndex.Value)
                {
                    if (m_panels[i])
                        m_panels[i].SetActive(true);
                }
                else if (m_panels[i] != null)
                {
                    // CheckForTabManagersInHierachy(i);
                    m_panels[i].gameObject.SetActive(false);
                }
    }
    public void SetPageIndex(int _index)
    {
        m_previousPanelIndex = m_panelIndex.Value;

        //when closing current panel, set index from  null to tmpIndex and show Panels again
        if (m_panelIndex == null)
        {
            m_panelIndex = m_previousPanelIndex;
            ShowCurrentPanel();
        }

        //update panelIndex
        m_panelIndex = _index;

        //show  current panel onyl once
        if (m_previousPanelIndex != m_panelIndex)
            ShowCurrentPanel();
    }
    public void CloseAllPanelsAndIndexNull(bool _tmp)
    {
        if (!_tmp)
            return;

        for (int i = 0; i < m_panels.Count; i++)
            m_panels[i].gameObject.SetActive(false);

        m_panelIndex = null;
        _tmp = false;
    }
    public void SetPreviousIndex()
    {
        m_panelIndex = m_previousPanelIndex;
        ShowCurrentPanel();
    }
    void CheckForTabManagersInHierachy(int _i)
    {
        TabHandler tmpTM;
        if (tmpTM = m_panels[_i].GetComponentInChildren<TabHandler>())
            tmpTM.ResetIndex();
    }
    public void ResetIndex()
    {
        m_panelIndex = m_previousPanelIndex = 0;
        ShowCurrentPanel();
    }
}

