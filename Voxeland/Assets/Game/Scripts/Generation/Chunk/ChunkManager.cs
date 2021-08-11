using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChunkManager : MonoBehaviour
{
    internal Chunk Chunk { get; set; }
    internal int RenderDistance { get => 1 << Chunk.Master.RenderDistance + Chunk.LOD; }
    internal bool Dirty { get; set; }

    Camera mainCamera { get => GameManager.Instance.m_MainCamera; }
    float tick = 0.75f;
    float timer = 0f;

    void Update()
    {
        if (timer < tick)
            timer += Time.deltaTime;
        else
        {
            UpdateChunk();
            timer = 0;
        }
    }

    void UpdateChunk()
    {
        if (mainCamera is null || Chunk is null || Chunk.Master is null) return;

        Vector3 dir = mainCamera.transform.position - Chunk.CenteredPosition;
        float offset = offset = Vector3.Magnitude(Vector3.Normalize(dir) * Chunk.Diameter * ((1 << Chunk.Master.RenderDistance) >> 7));
        float dist = Vector3.Magnitude(dir);

        bool inRange = (dist - offset) <= RenderDistance;
        if (Chunk.LOD != 0) inRange &= (dist + offset) > (RenderDistance >> 1);

        bool canDispose = dist > RenderDistance * Chunk.Master.DisposeFactor;
        if (Chunk.LOD != 0) canDispose = true;

        Chunk.SetVisible = inRange;

        if (!Dirty)
            if (!inRange && canDispose)
                Chunk.Remove();
    }
}
