using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PulseUI : MonoBehaviour {

  [SerializeField]
  private Image uiImage;

  private float fadeSpeed = 3f;

	// Use this for initialization
	void Start () {
    StartCoroutine(Pulse());
	}
	
	// Update is called once per frame
	private IEnumerator Pulse()
  {
    float alphaValue = 1.0f;

    while (true)
    {
      if (alphaValue < 0)
      {
        alphaValue = -alphaValue;
      }
      this.uiImage.color = new Color(this.uiImage.color.r, this.uiImage.color.b, this.uiImage.color.g, Mathf.Sin(alphaValue));
      alphaValue += Time.deltaTime * fadeSpeed;
      yield return null;
    }
  }
}
