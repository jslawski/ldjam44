using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using EZCameraShake;
using UnityEngine.SceneManagement;

public class TutorialManager : MonoBehaviour {

  public static TutorialManager instance;

  public Dictionary<SynapseLocation, Synapse> allSynapses;
  private const float DEFAULT_WAIT_DURATION = 3f;

  [SerializeField]
  private Material shockWaveMaterial;

  [SerializeField]
  private NeedleController[] needles;

  public bool flashingSynapseHit = false;
  public bool tutorialComplete = false;

  private AudioSource[] tutorialAudio;

  void Awake()
  {
    this.tutorialAudio = GetComponentsInParent<AudioSource>();

    instance = this;

    this.allSynapses = new Dictionary<SynapseLocation, Synapse>();

    GameObject synapsesParent = GameObject.Find("TutorialSynapses");
    Synapse[] synapses = synapsesParent.GetComponentsInChildren<Synapse>();
    this.needles = GameObject.Find("Needles").GetComponentsInChildren<NeedleController>();

    foreach (NeedleController needle in this.needles)
    {
      needle.isTutorialAnimating = true;
    }

    //NOTE: THIS ASSUMES THAT THE SYNAPSES IN THE SCENE HIERARCHY MATCH THE ORDER OF THE SYNAPSELOCATION ENUM!
    for (int i = 0; i < synapses.Length; i++)
    {
      this.allSynapses.Add((SynapseLocation)i, synapses[i]);
    }

    NeedleController.onSynapseHit -= this.SynapseHit;
    NeedleController.onSynapseHit += this.SynapseHit;

    this.tutorialAudio[0].Play();
    StartCoroutine(TutorialWait(DEFAULT_WAIT_DURATION, "GreenTutorial"));
  }

  private IEnumerator EmptyWait()
  {
    this.tutorialAudio[1].Play();
    yield return new WaitForSeconds(DEFAULT_WAIT_DURATION);
    StartCoroutine(TutorialWait(DEFAULT_WAIT_DURATION, "FlashingTutorial"));
  }

  private IEnumerator TutorialWait(float duration, string nextCoroutine)
  {
    foreach (NeedleController needle in this.needles)
    {
      needle.isTutorialAnimating = true;
    }

    switch (nextCoroutine)
    {
      case "GreenTutorial":
        this.allSynapses[SynapseLocation.LeftUp].SetSynapseMode(SynapseMode.OneTimePositive);
        break;
      case "RedTutorial":
        this.allSynapses[SynapseLocation.RightRight].SetSynapseMode(SynapseMode.OneTimeNegative);
        break;
      case "FlashingTutorial":
        this.allSynapses[SynapseLocation.LeftRight].SetSynapseMode(SynapseMode.RepetitivePositiveTutorial);
        this.allSynapses[SynapseLocation.RightLeft].SetSynapseMode(SynapseMode.RepetitivePositiveTutorial);
        this.tutorialAudio[2].Play();
        break;
    }

    yield return new WaitForSeconds(duration);
    StartCoroutine(nextCoroutine);

    foreach (NeedleController needle in this.needles)
    {
      needle.isTutorialAnimating = false;
    }
  }

  private IEnumerator GreenTutorial()
  {
    while (this.allSynapses[SynapseLocation.LeftUp].Mode == SynapseMode.OneTimePositive)
    {
      yield return null;
    }

    StartCoroutine(TutorialWait(0.0f, "RedTutorial"));
  }

  private IEnumerator RedTutorial()
  {
    while (this.allSynapses[SynapseLocation.RightRight].Mode == SynapseMode.OneTimeNegative)
    {
      yield return null;
    }

    StartCoroutine(EmptyWait());
  }

  private IEnumerator FlashingTutorial()
  {
    while (this.tutorialComplete == false)
    {
      yield return null;
    }

    this.allSynapses[SynapseLocation.LeftRight].SetSynapseMode(SynapseMode.Neutral);
    this.allSynapses[SynapseLocation.RightLeft].SetSynapseMode(SynapseMode.Neutral);

    StartCoroutine(TutorialWait(DEFAULT_WAIT_DURATION, "FinishTutorial"));
  }

  private IEnumerator FinishTutorial()
  {
    foreach (AudioSource speech in this.tutorialAudio)
    {
      speech.Stop();
    }

    NeedleController.onSynapseHit -= this.SynapseHit;
    Destroy(GameObject.Find("TutorialSynapses"));
    SceneManager.LoadScene("main");
    yield return null;
  }

  private void SynapseHit(SynapseLocation hitSynapse)
  {
    switch (this.allSynapses[hitSynapse].Mode)
    {
      case SynapseMode.OneTimePositive:
        this.tutorialAudio[3].Play();
        this.OneTimePositiveHit(hitSynapse);
        break;
      case SynapseMode.OneTimeNegative:
        this.tutorialAudio[4].Play();
        this.OneTimeNegativeHit(hitSynapse);
        break;
      case SynapseMode.Neutral:
        this.NeutralHit(hitSynapse);
        break;
      case SynapseMode.RepetitivePositiveTutorial:
        this.tutorialAudio[3].Play();
        this.RepetitivePositiveHit(hitSynapse);
        break;
      default:
        Debug.LogError("GameManager.SynapseHit: Unknown synapse mode");
        break;
    }

    this.allSynapses[hitSynapse].HitSynapse();
  }

  private void OneTimePositiveHit(SynapseLocation synapseLocation)
  {
    this.allSynapses[synapseLocation].SetSynapseMode(SynapseMode.Neutral);
    this.allSynapses[synapseLocation].Illuminator.OnSynapseTouched(SynapseMode.OneTimePositive);
    CameraShaker.Instance.ShakeOnce(1f, 2f, 0.1f, 0.1f);
  }

  private void OneTimeNegativeHit(SynapseLocation synapseLocation)
  {
    this.allSynapses[synapseLocation].SetSynapseMode(SynapseMode.Neutral);
    this.allSynapses[synapseLocation].Illuminator.OnSynapseTouched(SynapseMode.OneTimeNegative);
    CameraShaker.Instance.ShakeOnce(7f, 2f, 0.1f, 1f);
    Synapse synapseObject = this.allSynapses[synapseLocation];
    float screenSpaceX = Camera.main.WorldToViewportPoint(synapseObject.gameObject.transform.position).x;
    float screenSpaceY = Camera.main.WorldToViewportPoint(synapseObject.gameObject.transform.position).y;
    StartCoroutine(this.ShockWaveEffect(screenSpaceX, screenSpaceY));
  }

  private void NeutralHit(SynapseLocation synapseLocation)
  {
    CameraShaker.Instance.ShakeOnce(4f, 1f, 0.3f, 0.3f);
  }

  private void RepetitivePositiveHit(SynapseLocation synapseLocation)
  {
    this.flashingSynapseHit = true;
    this.allSynapses[synapseLocation].Illuminator.OnSynapseTouched(SynapseMode.RepetitivePositive);
    CameraShaker.Instance.ShakeOnce(1f, 2f, 0.1f, 0.1f);
  }

  private IEnumerator ShockWaveEffect(float screenSpaceX, float screenSpaceY)
  {
    shockWaveMaterial.SetFloat("_CenterX", screenSpaceX);
    shockWaveMaterial.SetFloat("_CenterY", screenSpaceY);

    float tParam = 0;
    float waveRadius;
    while (tParam < 1.0f)
    {
      tParam += Time.deltaTime * 2.0f;
      waveRadius = Mathf.Lerp(-0.2f, 2.0f, tParam);
      shockWaveMaterial.SetFloat("_Radius", waveRadius);
      yield return null;
    }
  }

  public void SkipTutorial()
  {
    StartCoroutine(this.FinishTutorial());
  }
}

