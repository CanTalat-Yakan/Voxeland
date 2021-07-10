using UnityEngine;

[System.Serializable]
public class VoxelTexture { public int front, left, back, right, top, bottom; }

[CreateAssetMenu(menuName = "Scriptables/Voxel Info", fileName = "Voxel Info", order = 3)]
public class VoxelInfo : ScriptableObject
{
    [Space(10)]
    public short ID = 0;
    public string VoxelName = "Default";
    public bool Transparent = false;
    public float Durability = 1.0f;
    [Space(10)]
    public VoxelTexture VoxelTexture = null;

    void OnValidate()
    {
        ID = (short)Mathf.Clamp(ID, 0, short.MaxValue);
        Durability = Mathf.Clamp(Durability, 0, Mathf.Infinity);
    }
}