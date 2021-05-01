using UnityEngine;
using System.Collections.Generic;

[AddComponentMenu("FastNoise/FastNoise Mesh Warp", 3)]
public class FastNoiseMeshWarp : MonoBehaviour
{
	public FastNoiseUnity fastNoiseUnity;
	public bool fractal;

	private Dictionary<GameObject, Mesh> originalMeshes = new Dictionary<GameObject, Mesh>();

	// Use this for initialization
	void Start ()
	{
		WarpAllMeshes();
	}

	public void WarpAllMeshes()
	{
		foreach (MeshFilter meshFilter in gameObject.GetComponentsInChildren<MeshFilter>())
		{
			WarpMesh(meshFilter);
		}
	}

	public void WarpMesh(MeshFilter meshFilter)
	{
		if (meshFilter.sharedMesh == null)
			return;

		Vector3 offset = meshFilter.gameObject.transform.position - gameObject.transform.position;
		Vector3[] verts;

		if (originalMeshes.ContainsKey(meshFilter.gameObject))
		{
			verts = originalMeshes[meshFilter.gameObject].vertices;
		}
		else
		{
			originalMeshes[meshFilter.gameObject] = meshFilter.sharedMesh;
			verts = meshFilter.sharedMesh.vertices;
		}

		var x = FastNoise.GetDecimalType();
		var y = x;
		var z = x;

		for (int i = 0; i < verts.Length; i++)
		{
			verts[i] += offset;

			x = verts[i].x;
			y = verts[i].y;
			z = verts[i].z;

			if (fractal)
				fastNoiseUnity.fastNoise.GradientPerturbFractal(ref x, ref y, ref z);
			else
				fastNoiseUnity.fastNoise.GradientPerturb(ref x, ref y, ref z);

			verts[i].Set((float)x, (float)y, (float)z);

			verts[i] -= offset;
		}

		meshFilter.mesh.vertices = verts;
		meshFilter.mesh.RecalculateNormals();
		meshFilter.mesh.RecalculateBounds();
	}
}
