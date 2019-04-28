using UnityEngine;

[ExecuteInEditMode]
public class ShockwaveEffect : MonoBehaviour
{

	public Material material;

	void OnRenderImage(RenderTexture src, RenderTexture dest)
	{
		Graphics.Blit(src, dest, material);
	}
}