using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VoxelEngine;

public class PlayerRayCast : MonoBehaviour
{
    [SerializeField] GameObject m_selectionBox;
    bool m_createdBox = false;
    internal Vector3Int? m_TargetBlockPos, m_ToPlaceBlockPos;
    internal Chunk m_TargetChunk, m_CurrentChunk;
    internal Voxel? m_TargetVoxel, m_ToPlaceVoxel;
    Vector3Int m_localVoxelPos = new Vector3Int();


    void Update()
    {
        if (!GameManager.Instance.m_MainCamera) return;

        if (!m_createdBox)
            m_selectionBox = GameObject.Instantiate(m_selectionBox, Vector3.zero, Quaternion.identity);
        m_createdBox = true;

        if (OnHit()) SetupRayInformation();

        if (Input.GetMouseButtonDown(0))
        {
            m_TargetChunk.voxelData[Chunk.VoxelDataIndex((Vector3i)m_localVoxelPos)] = new Voxel(0);
            m_TargetChunk.dirtyMesh = true;
            m_TargetChunk.BuildMesh();
            foreach (var item in m_TargetChunk.adjChunks)
                item.BuildMesh();
        }
    }

    void OnGUI()
    {
        if (!GameManager.Instance.m_ShowDebugInfo) return;


        int labelSpacing = 33;
        Rect rect = new Rect(Screen.width - 404, 0, 400, 30);

        var TextStyle = new GUIStyle();
        TextStyle.fontSize = 24;
        TextStyle.normal.textColor = Color.white;
        TextStyle.normal.background = Texture2D.grayTexture;
        TextStyle.font = GameManager.Instance.m_Settings.BlockFont;


        rect.y += 4;
        if (m_TargetBlockPos != null)
        {
            GUI.Box(rect, " Target Voxel: " + m_TargetVoxel.Value.IsSolid(), TextStyle);
            rect.y += labelSpacing;
            GUI.Box(rect, "     Voxel Pos: " + m_TargetBlockPos, TextStyle);
            rect.y += labelSpacing;
            GUI.Box(rect, "     Voxel Pos: " + (Vector3Int)(Vector3i)m_localVoxelPos + " local", TextStyle);
        }
        else
            GUI.Box(rect, " Target Voxel: None", TextStyle);
        rect.y += labelSpacing;

        if (m_TargetChunk != null)
            GUI.Box(rect, " Focused Chunk: " + (Vector3Int)m_TargetChunk.chunkPos, TextStyle);
        else
            GUI.Box(rect, " Focused Chunk: None", TextStyle);
        rect.y += labelSpacing;

        if (m_CurrentChunk != null)
            GUI.Box(rect, " Walking Chunk: " + (Vector3Int)m_CurrentChunk.chunkPos, TextStyle);
        else
            GUI.Box(rect, " Walking Chunk: None", TextStyle);
        rect.y += labelSpacing;
    }

    Vector3i GetLocalChunkPos(Vector3Int _worldPos)
    {
        _worldPos += Vector3Int.one; //offset
        var localPos = new Vector3i(
            _worldPos.x / 16,
            _worldPos.y / 16,
            _worldPos.z / 16);

        if (_worldPos.x < 0)
            localPos.x -= 1;
        if (_worldPos.y < 0)
            localPos.y -= 1;
        if (_worldPos.z < 0)
            localPos.z -= 1;

        return localPos;
    }
    Vector3i GetLocalChunkPos(Vector3 _worldPos)
    {
        return GetLocalChunkPos(new Vector3Int(
            (int)_worldPos.x,
            (int)_worldPos.y,
            (int)_worldPos.z));
    }

    Vector3i GetLocalVoxelPos(Vector3i _chunkPos)
    {
        m_localVoxelPos = (Vector3Int)(Vector3i)m_selectionBox.transform.InverseTransformPoint((Vector3)(_chunkPos * 16));

        m_localVoxelPos.x = Mathf.Abs(m_localVoxelPos.x);
        m_localVoxelPos.y = Mathf.Abs(m_localVoxelPos.y);
        m_localVoxelPos.z = Mathf.Abs(m_localVoxelPos.z);

        return (Vector3i)m_localVoxelPos;
    }

    void SetupRayInformation()
    {
        RaycastHit hit = GameManager.Instance.HitRayCast(
            GameManager.Instance.m_MainCamera.transform.position,
            GameManager.Instance.m_MainCamera.transform.forward,
            8);

        m_TargetBlockPos = new Vector3Int(
            Mathf.RoundToInt(hit.point.x - hit.normal.x * 0.4f),
            Mathf.RoundToInt(hit.point.y - hit.normal.y * 0.4f),
            Mathf.RoundToInt(hit.point.z - hit.normal.z * 0.4f));

        m_ToPlaceBlockPos = new Vector3Int(
            Mathf.RoundToInt(hit.point.x + hit.normal.x * 0.4f),
            Mathf.RoundToInt(hit.point.y + hit.normal.y * 0.4f),
            Mathf.RoundToInt(hit.point.z + hit.normal.z * 0.4f));

        m_TargetChunk = VoxelEngineManager.Instance.GetChunk(
            GetLocalChunkPos(m_TargetBlockPos.Value));

        if (m_TargetChunk != null)
            m_TargetVoxel = m_TargetChunk.GetVoxel(
                GetLocalVoxelPos(m_TargetChunk.chunkPos));

        if (m_TargetBlockPos != null)
            m_selectionBox.transform.position = m_TargetBlockPos.Value;
    }

    bool OnHit()
    {
        bool b = GameManager.Instance.BoolRayCast(
            GameManager.Instance.m_MainCamera.transform.position,
            GameManager.Instance.m_MainCamera.transform.forward,
            8);

        if (!b)
        {
            m_TargetBlockPos = null;
            m_ToPlaceBlockPos = null;
            m_TargetChunk = null;
            m_CurrentChunk = null;
            m_TargetVoxel = null;
            m_ToPlaceVoxel = null;
        }
        m_selectionBox.SetActive(b);

        m_CurrentChunk = VoxelEngineManager.Instance.GetChunk(
            GetLocalChunkPos(gameObject.transform.position));

        return b;
    }
}