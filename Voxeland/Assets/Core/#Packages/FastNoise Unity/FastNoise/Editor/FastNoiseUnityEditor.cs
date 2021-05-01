using System;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(FastNoiseUnity))]
public class FastNoiseUnityEditor : Editor
{
	public override void OnInspectorGUI()
	{
		FastNoiseUnity fastNoiseUnity = ((FastNoiseUnity)target);
		FastNoise fastNoise = fastNoiseUnity.fastNoise;

		fastNoiseUnity.noiseName = EditorGUILayout.TextField("Name", fastNoiseUnity.noiseName);

		fastNoiseUnity.generalSettingsFold = EditorGUILayout.Foldout(fastNoiseUnity.generalSettingsFold, "General Settings");

		if (fastNoiseUnity.generalSettingsFold)
		{
			fastNoise.SetNoiseType(
				fastNoiseUnity.noiseType = (FastNoise.NoiseType)EditorGUILayout.EnumPopup("Noise Type", fastNoiseUnity.noiseType));
			fastNoise.SetSeed(fastNoiseUnity.seed = EditorGUILayout.IntField("Seed", fastNoiseUnity.seed));
			fastNoise.SetFrequency(fastNoiseUnity.frequency = EditorGUILayout.FloatField("Frequency", fastNoiseUnity.frequency));
			fastNoise.SetInterp(
				fastNoiseUnity.interp = (FastNoise.Interp)EditorGUILayout.EnumPopup("Interpolation", fastNoiseUnity.interp));
		}

		fastNoiseUnity.fractalSettingsFold = EditorGUILayout.Foldout(fastNoiseUnity.fractalSettingsFold, "Fractal Settings");

		if (fastNoiseUnity.fractalSettingsFold)
		{
			fastNoise.SetFractalType(
				fastNoiseUnity.fractalType =
					(FastNoise.FractalType)EditorGUILayout.EnumPopup("Fractal Type", fastNoiseUnity.fractalType));
			fastNoise.SetFractalOctaves(
				fastNoiseUnity.octaves = EditorGUILayout.IntSlider("Octaves", fastNoiseUnity.octaves, 2, 9));
			fastNoise.SetFractalLacunarity(
				fastNoiseUnity.lacunarity = EditorGUILayout.FloatField("Lacunarity", fastNoiseUnity.lacunarity));
			fastNoise.SetFractalGain(fastNoiseUnity.gain = EditorGUILayout.FloatField("Gain", fastNoiseUnity.gain));
		}

		fastNoiseUnity.cellularSettingsFold = EditorGUILayout.Foldout(fastNoiseUnity.cellularSettingsFold,
			"Cellular Settings");

		if (fastNoiseUnity.cellularSettingsFold)
		{
			fastNoise.SetCellularReturnType(
				fastNoiseUnity.cellularReturnType =
					(FastNoise.CellularReturnType)EditorGUILayout.EnumPopup("Return Type", fastNoiseUnity.cellularReturnType));
			if (fastNoiseUnity.cellularReturnType == FastNoise.CellularReturnType.NoiseLookup)
			{
				fastNoiseUnity.cellularNoiseLookup =
					(FastNoiseUnity)
						EditorGUILayout.ObjectField("Noise Lookup", fastNoiseUnity.cellularNoiseLookup, typeof(FastNoiseUnity), true);

				if (fastNoiseUnity.cellularNoiseLookup)
					fastNoise.SetCellularNoiseLookup(fastNoiseUnity.cellularNoiseLookup.fastNoise);
			}
			fastNoise.SetCellularDistanceFunction(
				fastNoiseUnity.cellularDistanceFunction =
					(FastNoise.CellularDistanceFunction)
						EditorGUILayout.EnumPopup("Distance Function", fastNoiseUnity.cellularDistanceFunction));
			fastNoiseUnity.cellularDistanceIndex0 = EditorGUILayout.IntSlider("Distance2 Index 0", Mathf.Min(fastNoiseUnity.cellularDistanceIndex0, fastNoiseUnity.cellularDistanceIndex1 - 1), 0, 2);
			fastNoiseUnity.cellularDistanceIndex1 = EditorGUILayout.IntSlider("Distance2 Index 1", fastNoiseUnity.cellularDistanceIndex1, 1, 3);
			fastNoise.SetCellularDistance2Indicies(fastNoiseUnity.cellularDistanceIndex0, fastNoiseUnity.cellularDistanceIndex1);

			fastNoise.SetCellularJitter(
				fastNoiseUnity.cellularJitter =
					EditorGUILayout.Slider("Cell Jitter", fastNoiseUnity.cellularJitter, 0f, 1f));
		}

		fastNoiseUnity.positionWarpSettingsFold = EditorGUILayout.Foldout(fastNoiseUnity.positionWarpSettingsFold,
			"Perturb Settings");

		if (fastNoiseUnity.positionWarpSettingsFold)
			fastNoise.SetGradientPerturbAmp(
				fastNoiseUnity.gradientPerturbAmp = EditorGUILayout.FloatField("Amplitude", fastNoiseUnity.gradientPerturbAmp));

		if (GUILayout.Button("Reset"))
		{
			fastNoise.SetSeed(fastNoiseUnity.seed = 1337);
			fastNoise.SetFrequency(fastNoiseUnity.frequency = 0.01f);
			fastNoise.SetInterp(fastNoiseUnity.interp = FastNoise.Interp.Quintic);
			fastNoise.SetNoiseType(fastNoiseUnity.noiseType = FastNoise.NoiseType.Simplex);

			fastNoise.SetFractalOctaves(fastNoiseUnity.octaves = 3);
			fastNoise.SetFractalLacunarity(fastNoiseUnity.lacunarity = 2.0f);
			fastNoise.SetFractalGain(fastNoiseUnity.gain = 0.5f);
			fastNoise.SetFractalType(fastNoiseUnity.fractalType = FastNoise.FractalType.FBM);

			fastNoise.SetCellularDistanceFunction(
				fastNoiseUnity.cellularDistanceFunction = FastNoise.CellularDistanceFunction.Euclidean);
			fastNoise.SetCellularReturnType(fastNoiseUnity.cellularReturnType = FastNoise.CellularReturnType.CellValue);

			fastNoise.SetCellularDistance2Indicies(fastNoiseUnity.cellularDistanceIndex0 = 0, fastNoiseUnity.cellularDistanceIndex1 = 1);
			fastNoise.SetCellularJitter(fastNoiseUnity.cellularJitter = 0.45f);

			fastNoise.SetGradientPerturbAmp(fastNoiseUnity.gradientPerturbAmp = 1.0f);
		}
	}

	public override bool HasPreviewGUI()
	{
		return true;
	}

	public override GUIContent GetPreviewTitle()
	{
		return new GUIContent("FastNoise Unity - " + ((FastNoiseUnity)target).noiseName);
	}

	public override void DrawPreview(Rect previewArea)
	{
		FastNoiseUnity fastNoiseUnity = ((FastNoiseUnity)target);
		FastNoise fastNoise = fastNoiseUnity.fastNoise;

		if (fastNoiseUnity.noiseType == FastNoise.NoiseType.Cellular &&
			fastNoiseUnity.cellularReturnType == FastNoise.CellularReturnType.NoiseLookup &&
			fastNoiseUnity.cellularNoiseLookup == null)
		{
			GUI.Label(previewArea, "Set cellular noise lookup", new GUIStyle
			{
				fontStyle = FontStyle.Bold,
				alignment = TextAnchor.MiddleCenter,
				fontSize = 18
			});
			return;
		}

		Texture2D tex = new Texture2D((int)previewArea.width, (int)previewArea.height);
		Color32[] pixels = new Color32[tex.width * tex.height];

		float[] noiseSet = new float[pixels.Length];

		float min = Single.MaxValue;
		float max = Single.MinValue;
		int index = 0;

		for (int y = 0; y < tex.height; y++)
		{
			for (int x = 0; x < tex.width; x++)
			{
				var noise = noiseSet[index++] = (float) fastNoise.GetNoise(x, y);
				min = Mathf.Min(min, noise);
				max = Mathf.Max(max, noise);
			}
		}

		float scale = 255f / (max - min);

		for (int i = 0; i < noiseSet.Length; i++)
		{
			byte noise = (byte)Mathf.Clamp((noiseSet[i] - min) * scale, 0f, 255f);
				pixels[i] = new Color32(noise, noise, noise, 255);
		}

		tex.SetPixels32(pixels);
		tex.Apply();
		GUI.DrawTexture(previewArea, tex, ScaleMode.StretchToFill, false);
	}
}