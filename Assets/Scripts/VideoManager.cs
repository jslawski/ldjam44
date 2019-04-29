using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Video;

public class VideoManager : MonoBehaviour {

  [SerializeField]
  private VideoPlayer player;
  [SerializeField]
  private string videoName; //Include Extension
  [SerializeField]
  private string sceneToLoadOnVideoLoopPointReached = string.Empty;

	void Awake() {
    player.url = System.IO.Path.Combine(Application.streamingAssetsPath, this.videoName);
    player.Play();

    player.loopPointReached += this.LoadGameScene;
	}

  public void SkipCutscene()
  {
    this.player.Stop();
    this.LoadGameScene(this.player);
  }

  private void LoadGameScene(VideoPlayer source)
  {
    if (sceneToLoadOnVideoLoopPointReached == string.Empty)
    {
      return;
    }

    SceneManager.LoadScene(sceneToLoadOnVideoLoopPointReached);
  }
}
