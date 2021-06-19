using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VoxelEngine;

public class PlayerRayCast : MonoBehaviour
{
    [SerializeField] GameObject SelectionBox;
    Vector3Int? FocusedBlockPos, PlacementBlockPos;
    Chunk FocusedChunk;
    Chunk WalkingChunk;
    Voxel? FocusedVoxel;
    void Start()
    {
        if (!SelectionBox)
        {
            SelectionBox = GameObject.CreatePrimitive(PrimitiveType.Cube);
            SelectionBox.GetComponent<BoxCollider>().enabled = false;
            SelectionBox.transform.localScale *= 1.02f;
        }
        else
            SelectionBox = GameObject.Instantiate(SelectionBox, Vector3.zero, Quaternion.identity);
    }

    void Update()
    {
        if (!GameManager.Instance.m_MainCamera)
            return;

        GetBlockPos();

        OnHit();
    }

    void OnGUI()
    {
        if (!GameManager.Instance.m_ShowDebugInfo) return;


        int labelSpacing = 33;
        Rect rect = new Rect(Screen.width - 400, 0, 400, 30);

        var TextStyle = new GUIStyle();
        TextStyle.fontSize = 24;
        TextStyle.normal.textColor = Color.white;
        TextStyle.normal.background = Texture2D.grayTexture;
        TextStyle.font = GameManager.Instance.m_Settings.BlockFont;


        if (FocusedBlockPos != null)
            GUI.Box(rect, " Focused Block Pos: " + FocusedBlockPos, TextStyle);
        else
            GUI.Box(rect, " Focused Block Pos: None", TextStyle);
        rect.y += labelSpacing;

        if (FocusedVoxel != null)
            GUI.Box(rect, " Focused Voxel: " + FocusedVoxel, TextStyle);
        else
            GUI.Box(rect, " Focused Voxel: None", TextStyle);
        rect.y += labelSpacing;

        if (FocusedChunk != null)
            GUI.Box(rect, " Focused Chunk: " + (Vector3Int)FocusedChunk.chunkPos, TextStyle);
        else
            GUI.Box(rect, " Focused Chunk: None", TextStyle);
        rect.y += labelSpacing;

        if (WalkingChunk != null)
            GUI.Box(rect, " Walking Chunk: " + (Vector3Int)WalkingChunk.chunkPos, TextStyle);
        else
            GUI.Box(rect, " Walking Chunk: None", TextStyle);
        rect.y += labelSpacing;
    }

    Vector3Int GetLocalChunkPos(Vector3Int _worldPos)
    {
        _worldPos += Vector3Int.one; //offset
        Vector3Int localPos = new Vector3Int(
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
    Vector3Int GetLocalChunkPos(Vector3 _worldPos)
    {
        return GetLocalChunkPos(new Vector3Int(
            (int)_worldPos.x,
            (int)_worldPos.y,
            (int)_worldPos.z));
    }

    void GetBlockPos()
    {
        RaycastHit hit = GameManager.Instance.HitRayCast(
            GameManager.Instance.m_MainCamera.transform.position,
            GameManager.Instance.m_MainCamera.transform.forward,
            8);

        FocusedBlockPos = new Vector3Int(
            Mathf.RoundToInt(hit.point.x - hit.normal.x * 0.4f),
            Mathf.RoundToInt(hit.point.y - hit.normal.y * 0.4f),
            Mathf.RoundToInt(hit.point.z - hit.normal.z * 0.4f));

        PlacementBlockPos = new Vector3Int(
            Mathf.RoundToInt(hit.point.x + hit.normal.x * 0.4f),
            Mathf.RoundToInt(hit.point.y + hit.normal.y * 0.4f),
            Mathf.RoundToInt(hit.point.z + hit.normal.z * 0.4f));

        FocusedChunk = VoxelEngineManager.Instance.GetChunk(
            (Vector3i)GetLocalChunkPos(FocusedBlockPos.Value));

        WalkingChunk = VoxelEngineManager.Instance.GetChunk(
            (Vector3i)GetLocalChunkPos(gameObject.transform.position));

        SelectionBox.SetActive(FocusedBlockPos != null);
        if (FocusedBlockPos != null)
            SelectionBox.transform.position = FocusedBlockPos.Value;
    }

    bool OnHit()
    {
        if (GameManager.Instance.BoolRayCast(
            GameManager.Instance.m_MainCamera.transform.position,
            GameManager.Instance.m_MainCamera.transform.forward,
            8))
            return true;
        else
        {
            FocusedBlockPos = null;
            PlacementBlockPos = null;
            FocusedVoxel = null;
            FocusedChunk = null;
            WalkingChunk = null;
        }
        return false;
    }
}