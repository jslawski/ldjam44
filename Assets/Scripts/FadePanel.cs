using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FadePanel : MonoBehaviour {

  [SerializeField]
  private Image panelToFade;

	// Use this for initialization
	void Start () {
    StartCoroutine(DoFade());
	}
	
	private IEnumerator DoFade()
  {
    for (float i = 1; i > 0; i -= Time.deltaTime * 3.0f)
    {
      panelToFade.color = new Color(panelToFade.color.r, panelToFade.color.b, panelToFade.color.g, i);
      yield return null;
    }

    yield return new WaitForSeconds(3.0f);

    for (float i = 0; i < 1; i += Time.deltaTime * 3.0f)
    {
      panelToFade.color = new Color(panelToFade.color.r, panelToFade.color.b, panelToFade.color.g, i);
      yield return null;
    }

    panelToFade.color = Color.black;
  }
}
