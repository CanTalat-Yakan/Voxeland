using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FlexibleGridLayout : LayoutGroup
{
    [SerializeField]
    private enum FitType
    {
        Uniform,
        Width,
        Height,
        FixedRows,
        FixedColumns
    }
    [SerializeField] private FitType m_fitType;

    [SerializeField] private int m_rows;
    [SerializeField] private int m_columns;

    [SerializeField] private Vector2 m_cellSize;
    [SerializeField] private Vector2 m_spacing;

    [SerializeField] private bool m_fitX;
    [SerializeField] private bool m_fitY;
    

    public override void CalculateLayoutInputHorizontal()
    {
        base.CalculateLayoutInputHorizontal();

        if (m_fitType == FitType.Width || m_fitType == FitType.Height || m_fitType == FitType.Uniform)
        {
            m_fitX = true;
            m_fitY = true;
            float sqrRt = Mathf.Sqrt(transform.childCount);
            m_rows = Mathf.CeilToInt(sqrRt);
            m_columns = Mathf.CeilToInt(sqrRt);
        }

        if (m_fitType == FitType.Width || m_fitType == FitType.FixedColumns)
        {
            m_rows = Mathf.CeilToInt(transform.childCount / (float)m_columns);
        }
        if (m_fitType == FitType.Height || m_fitType == FitType.FixedRows)
        {
            m_columns = Mathf.CeilToInt(transform.childCount / (float)m_rows);
        }

        float parentWidth = rectTransform.rect.width;
        float parentHeight = rectTransform.rect.height;

        float cellWidth = (parentWidth / (float)m_columns) - ((m_spacing.x / (float)m_columns) * 2) - (padding.left / (float)m_columns) - (padding.right / (float)m_columns);
        float cellHeight = (parentHeight / (float)m_rows) - ((m_spacing.y / (float)m_rows) * 2) - (padding.top / (float)m_rows) - (padding.bottom / (float)m_rows);

        m_cellSize.x = m_fitX ? cellWidth : m_cellSize.x;
        m_cellSize.y = m_fitY ? cellHeight : m_cellSize.y;

        int columnCount = 0;
        int rowCount = 0;

        for (int i = 0; i < rectChildren.Count; i++)
        {
            rowCount = i / m_columns;
            columnCount = i % m_columns;

            var item = rectChildren[i];

            var xPos = (m_cellSize.x * columnCount) + (m_spacing.x * columnCount) + padding.left;
            var yPos = (m_cellSize.y * rowCount) + (m_spacing.y * rowCount) + padding.top;

            SetChildAlongAxis(item, 0, xPos, m_cellSize.x);
            SetChildAlongAxis(item, 1, yPos, m_cellSize.y);
        }
    }

    #region -not used-
    public override void CalculateLayoutInputVertical()
    {

    }

    public override void SetLayoutHorizontal()
    {

    }

    public override void SetLayoutVertical()
    {

    }
    #endregion
}