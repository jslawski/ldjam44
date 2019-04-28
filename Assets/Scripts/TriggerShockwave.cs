using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TriggerShockwave : MonoBehaviour
{

	public Material shockWaveMaterial;

	void Start()
	{
		shockWaveMaterial.SetFloat("_Radius", -0.2f);
	}

	IEnumerator ShockWaveEffect(float screenSpaceX, float screenSpaceY)
	{
		Debug.LogError("Executing shockwave");
		float tParam = 0;
		float waveRadius;
		while (tParam < 1)
		{
			Debug.LogError("In loop");
			tParam += Time.deltaTime * 2;
			waveRadius = Mathf.Lerp(-0.2f, 2, tParam);
			shockWaveMaterial.SetFloat("_Radius", waveRadius);
			yield return null;
		}
	}
}