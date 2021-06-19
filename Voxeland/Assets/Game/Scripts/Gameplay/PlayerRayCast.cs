using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VoxelEngine;

public class PlayerRayCast : MonoBehaviour
{
    [SerializeField] GameObject SelectionBox;
    Vector3Int? FocusedBlockPos, PlacementBlockPos;
    Chunk FocusedChunk;
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

        if (!OnHit())
            if (FocusedChunk != null)
                Debug.Log(FocusedChunk.chunkGameObject.gameObject.name);
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
        }
        return false;
    }
}