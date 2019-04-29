using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TransitionFromSplashScene : MonoBehaviour
{
  [SerializeField]
  private float secondsToWaitBeforeTransition = 3.0f;
  [SerializeField]
  private string sceneToTransitionTo = string.Empty;

  private void Awake()
  {
    StartCoroutine(TransitionAfterWait());
  }

  private IEnumerator TransitionAfterWait()
  {
    yield return new WaitForSeconds(secondsToWaitBeforeTransition);

    if (sceneToTransitionTo != string.Empty)
    {
      SceneManager.LoadScene(sceneToTransitionTo);
    }

  }
}
