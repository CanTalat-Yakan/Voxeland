using UnityEngine;

public class MeshInfo : MonoBehaviour
{
    public MeshRenderer Renderer;
    public MeshFilter Filter;
    public MeshCollider Collider;
    public ChunkManager Manager;
    public Mesh Mesh;

    public void ResetAll()
    {
        gameObject.SetActive(false);
        gameObject.name = "Pooled";
        gameObject.transform.SetSiblingIndex(0);
        gameObject.transform.position = Vector3.zero;
        gameObject.transform.localScale = Vector3.one;

        Mesh = null;
        Filter.sharedMesh = null;
        Collider.sharedMesh = null;
    }
}
