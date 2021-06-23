using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VoxelEngine;

public class PlayerRayCast : MonoBehaviour
{
    [SerializeField] GameObject m_selectionBox;
    bool m_createdBox = false;
    internal Vector3Int? m_TargetBlockPos, m_TargetBlockNormal;
    internal Chunk m_TargetChunk, m_CurrentChunk;
    internal Voxel? m_TargetVoxel;
    Vector3Int m_localVoxelPos = new Vector3Int();


    void Update()
    {
        if (GameManager.Instance.LOCKED) return;
        if (!GameManager.Instance.m_MainCamera) return;

        if (!m_createdBox)
            m_selectionBox = GameObject.Instantiate(m_selectionBox, Vector3.zero, Quaternion.identity);
        m_createdBox = true;

        if (OnHit()) SetupRayInformation();

        if (m_TargetVoxel is null) return;
        if (Input.GetMouseButtonDown(0))
        {
            m_TargetChunk.voxelData[Chunk.VoxelDataIndex(m_localVoxelPos)] = new Voxel(0);
            m_TargetChunk.dirtyMesh = true;
            m_TargetChunk.BuildMesh();
        }
        if (Input.GetMouseButtonDown(1) && !IsPlayerInBlock())
        {
            m_TargetChunk.voxelData[Chunk.VoxelDataIndex((m_localVoxelPos + m_TargetBlockNormal).Value)] = new Voxel(1);
            m_TargetChunk.dirtyMesh = true;
            m_TargetChunk.BuildMesh();
        }
    }

    void OnGUI()
    {
        if (!GameManager.Instance.m_ShowDebugInfo) return;


        int labelSpacing = 33;
        Rect rect = new Rect(Screen.width - 404, 0, 400, 30);
        Rect rect2 = new Rect(Screen.width - 404, Screen.height, 400, 30);

        var TextStyle = new GUIStyle();
        TextStyle.fontSize = 24;
        TextStyle.normal.textColor = Color.white;
        TextStyle.normal.background = Texture2D.grayTexture;
        TextStyle.font = GameManager.Instance.m_Settings.BlockFont;

        rect.y += 8;
        if (GameManager.Instance)
            GUI.Box(rect, " Player Pos: " + FloatToInt(GameManager.Instance.m_Player.gameObject.transform.position + Vector3.up * 0.02f), TextStyle);
        rect.y += labelSpacing;
        if (m_CurrentChunk != null)
            GUI.Box(rect, " Walking Chunk: " + (Vector3Int)m_CurrentChunk.chunkPos, TextStyle);
        else
            GUI.Box(rect, " Walking Chunk: None", TextStyle);
        rect.y += labelSpacing;

        if (m_TargetChunk != null)
            GUI.Box(rect, " Focused Chunk: " + (Vector3Int)m_TargetChunk.chunkPos, TextStyle);
        else
            GUI.Box(rect, " Focused Chunk: None", TextStyle);
        rect.y += labelSpacing;

        if (m_TargetBlockPos != null)
        {
            GUI.Box(rect, " Target Voxel: " + m_TargetVoxel.Value.IsSolid(), TextStyle);
            rect.y += labelSpacing;
            rect.x += labelSpacing;
            rect.width -= labelSpacing;
            GUI.Box(rect, " Voxel Pos: " + m_TargetBlockPos, TextStyle);
            rect.y += labelSpacing;
            rect.x += labelSpacing;
            rect.width -= labelSpacing;
            GUI.Box(rect, " Local: " + (Vector3Int)m_localVoxelPos, TextStyle);
            rect.x -= labelSpacing * 2;
            rect.width += labelSpacing * 2;
        }
        else
            GUI.Box(rect, " Target Voxel: None", TextStyle);
        rect.y += labelSpacing;



        string s = "";

        if (Vector3.Dot(GameManager.Instance.m_MainCamera.transform.forward, Vector3.right) > 0.5f) s = "North";
        else if (Vector3.Dot(GameManager.Instance.m_MainCamera.transform.forward, Vector3.right) < -0.5f) s = "South";
        else if (Vector3.Dot(GameManager.Instance.m_MainCamera.transform.right, Vector3.right) > 0.5f) s = "East";
        else if (Vector3.Dot(GameManager.Instance.m_MainCamera.transform.right, Vector3.right) < -0.5f) s = "West";

        if (Vector3.Dot(GameManager.Instance.m_MainCamera.transform.forward, Vector3.right) > 0.34f)
        {
            if (Vector3.Dot(GameManager.Instance.m_MainCamera.transform.right, Vector3.right) > 0.34f) s = "North East";
            else if (Vector3.Dot(GameManager.Instance.m_MainCamera.transform.right, Vector3.right) < -0.34f) s = "North West";
        }
        if (Vector3.Dot(GameManager.Instance.m_MainCamera.transform.forward, Vector3.right) < -0.34f)
        {
            if (Vector3.Dot(GameManager.Instance.m_MainCamera.transform.right, Vector3.right) > 0.34f) s = "South East";
            else if (Vector3.Dot(GameManager.Instance.m_MainCamera.transform.right, Vector3.right) < -0.34f) s = "South West";
        }

        rect2.y -= 8;
        rect2.y -= labelSpacing;
        if (GameManager.Instance)
            GUI.Box(rect2, " Compass: " + s, TextStyle);
        rect2.y -= labelSpacing;
    }

    bool IsPlayerInBlock()
    {
        return m_TargetBlockPos + m_TargetBlockNormal == Vector3Int.FloorToInt(GameManager.Instance.m_Player.gameObject.transform.position + Vector3.up * 0.02f)
                || m_TargetBlockPos + m_TargetBlockNormal == Vector3Int.FloorToInt(GameManager.Instance.m_Player.gameObject.transform.position + Vector3.up * 1.02f);
    }

    Vector3Int GetChunkPos(Vector3Int _worldPos)
    {
        _worldPos += Vector3Int.one;

        var localPos = new Vector3Int(
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

    Vector3Int GetLocalVoxelPos(Vector3Int _chunkPos, ref Vector3Int _v)
    {
        _v = -Vector3Int.FloorToInt(m_selectionBox.transform.InverseTransformPoint(_chunkPos * 16));
        
        _v.x = Mathf.Abs(_v.x);
        _v.y = Mathf.Abs(_v.y);
        _v.z = Mathf.Abs(_v.z);

        return _v;
    }

    void SetupRayInformation()
    {
        RaycastHit hit = GameManager.Instance.HitRayCast(
            GameManager.Instance.m_MainCamera.transform.position,
            GameManager.Instance.m_MainCamera.transform.forward,
            8);

        m_TargetBlockPos = FloatToInt(hit.point - hit.normal * 0.4f);

        m_TargetBlockNormal = FloatToInt(hit.normal);

        m_TargetChunk = VoxelEngineManager.Instance.GetChunk(
            GetChunkPos(m_TargetBlockPos.Value));

        if (m_TargetChunk != null)
            m_TargetVoxel = m_TargetChunk.GetVoxel(
                GetLocalVoxelPos(m_TargetChunk.chunkPos, ref m_localVoxelPos));

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
            m_TargetBlockNormal = null;
            m_TargetChunk = null;
            m_CurrentChunk = null;
            m_TargetVoxel = null;
        }
        m_selectionBox.SetActive(b);

        m_CurrentChunk = VoxelEngineManager.Instance.GetChunk(
            GetChunkPos(Vector3Int.FloorToInt(gameObject.transform.position)));

        return b;
    }

    Vector3Int FloatToInt(Vector3 _v)
    {
        return new Vector3Int(
            Mathf.RoundToInt(_v.x),
            Mathf.RoundToInt(_v.y),
            Mathf.RoundToInt(_v.z));
    }
}