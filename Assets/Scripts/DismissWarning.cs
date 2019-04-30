using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class DismissWarning : MonoBehaviour {

	// Use this for initialization
	public void DismissWebGLWarning() {
        SceneManager.LoadScene("TitleScene");
    }
}
