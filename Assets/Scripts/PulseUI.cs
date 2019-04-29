using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PulseUI : MonoBehaviour {

  [SerializeField]
  private Image uiImage;

  private float fadeSpeed = 2f;

	// Use this for initialization
	void Start () {
    StartCoroutine(Pulse());
	}
	
	// Update is called once per frame
	private IEnumerator Pulse()
  {
    float alphaValue = 1.0f;

    float finalValue = Mathf.Sin(alphaValue);

    while (true)
    {
      finalValue = Mathf.Sin(alphaValue);

      if (finalValue < 0)
      {
        finalValue = -finalValue;
      }
      this.uiImage.color = new Color(this.uiImage.color.r, this.uiImage.color.b, this.uiImage.color.g, finalValue);
      alphaValue += Time.deltaTime * fadeSpeed;
      yield return null;
    }
  }
}
