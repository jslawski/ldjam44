using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public class TransitionFromTitleScene : MonoBehaviour, IPointerClickHandler
{
  [SerializeField]
  private string sceneToTransitionTo = string.Empty;

  public void OnPointerClick(PointerEventData eventData)
  {
    if (sceneToTransitionTo == string.Empty)
    {
      return;
    }

    SceneManager.LoadScene(sceneToTransitionTo);
  }
}
