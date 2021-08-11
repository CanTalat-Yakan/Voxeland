using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PrefabPool : MonoBehaviour
{
    [SerializeField] GameObject prefab;
    [SerializeField] int poolSize;
    internal Stack<MeshInfo> pool;

    void Awake()
    {
        pool = new Stack<MeshInfo>();
        CreateInstance();
    }

    void CreateInstance()
    {
        for (int i = 0; i < poolSize; i++)
            pool.Push(GameObject.Instantiate(prefab, Vector3.zero, Quaternion.identity, transform).GetComponent<MeshInfo>());

        Debug.Log($"Instanced  {poolSize} GameObjects");
    }

    public void Add(MeshInfo obj)
    {
        pool.Push(obj);
    }

    public MeshInfo Get()
    {
        if (pool.Count == 0)
            CreateInstance();

        return pool.Pop();
    }
}
