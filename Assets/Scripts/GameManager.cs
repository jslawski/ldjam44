using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using EZCameraShake;

public enum SynapseLocation { LeftLeft, LeftRight, LeftUp, LeftDown, RightLeft, RightRight, RightUp, RightDown };
public enum GameDifficulty {  Easy, Medium, Hard };
public class GameManager : MonoBehaviour
{
	private const int SEQUENCE_CLEAR_LEVEL_UP_THRESHOLD = 10;

  // Multiplies with the combo value to increment the current score which is a long.
	private const int SCORE_INCREMENT_VALUE = 50;
  // Determines how much to decrement the score, which is a long, by.
  // 3 means decrement the score by a third.
	private const long SCORE_DECREMENT_DIVISOR = 3;

  public static GameManager instance = null;

	public Dictionary<SynapseLocation, Synapse> allSynapses;
	public GameDifficulty currentDifficulty = GameDifficulty.Easy;
  public Sequence currentSequence = null;
  public float InitialGameTimerInSeconds = 120.0F;

  private Coroutine runningSequenceCoroutine;
  private Coroutine currentLoadNextSequenceCoroutine;
	private int consecutiveClearedSequences = 0;
  private bool IsGameActive = false;

	[SerializeField]
	private Timer gameTimer;
  [SerializeField]
  private float NegativeHitTimerPenaltyInSeconds = 3.0f;
  [SerializeField]
  private GameObject gameOverUI;
  /// <summary>
  /// This game object contains the countdown text and the get ready text,
  /// so we can disable it to disable both.
  /// </summary>
  //[SerializeField]
  //private GameObject getReadyUI;
  /// <summary>
  /// Will tick down every second to give the user some time to prepare
  /// before the game starts.
  /// </summary>
  [SerializeField]
  private Image CountdownImage;
	[SerializeField]
	private Text scoreText;
	private long scoreValue = 0;
	[SerializeField]
	private Text comboText;
	private int comboValue = 1;
	[SerializeField]
	private Material shockWaveMaterial;

  private AudioSource goodGrannySound;
  private AudioSource badGrannySound;
  private AudioSource goodHitSound;
  private AudioSource badHitSound;

  #region Unity Game Loop
  void Awake()
	{
		instance = this;
    this.allSynapses = new Dictionary<SynapseLocation, Synapse>();
		GameObject synapsesParent = GameObject.Find("NewSynapses");
		Synapse[] synapses = synapsesParent.GetComponentsInChildren<Synapse>();

		//NOTE: THIS ASSUMES THAT THE SYNAPSES IN THE SCENE HIERARCHY MATCH THE ORDER OF THE SYNAPSELOCATION ENUM!
		for (int i = 0; i < synapses.Length; i++)
		{
			this.allSynapses.Add((SynapseLocation)i, synapses[i]);
		}

		NeedleController.onSynapseHit -= this.SynapseHit;
		NeedleController.onSynapseHit += this.SynapseHit;
    gameTimer.OnTimerEnded -= this.OnTimerEnded;
    gameTimer.OnTimerEnded += this.OnTimerEnded;

    AudioSource[] audioSources = GetComponentsInParent<AudioSource>();
    this.goodGrannySound = audioSources[0];
    this.badGrannySound = audioSources[1];
    this.goodHitSound = audioSources[2];
    this.badHitSound = audioSources[3];

    ReadyGame();
	}

  private void Update()
  {
    if (this.consecutiveClearedSequences >= GameManager.SEQUENCE_CLEAR_LEVEL_UP_THRESHOLD && this.currentDifficulty != GameDifficulty.Hard)
    {
      this.currentDifficulty++;
      this.consecutiveClearedSequences = 0;
    }

    this.scoreText.text = this.scoreValue.ToString();
    if (this.comboValue == 0)
    {
      this.comboText.text = string.Empty;
    }
    else
    {
      this.comboText.text = "Combo: x" + this.comboValue.ToString();
    }
  }

  private void OnDestroy()
  {
    NeedleController.onSynapseHit -= this.SynapseHit;
    gameTimer.OnTimerEnded -= this.OnTimerEnded;
  }
  #endregion

  #region Start Game Sequence
  public void ReadyGame()
	{
		this.gameTimer.SetTime(InitialGameTimerInSeconds);
		currentSequence = null;
    currentDifficulty = GameDifficulty.Easy;
    scoreValue = 0;
    comboValue = 1;

    for (int i = 0, count = allSynapses.Count; i < count; i++)
    {
      allSynapses[(SynapseLocation) i].SetSynapseMode(SynapseMode.Neutral);
    }

    gameOverUI.SetActive(false);
    StartCoroutine(StartCountdownToGameStart());
  }

  private IEnumerator StartCountdownToGameStart()
  {
    int counter = 3; // 3 second coundown
    CountdownImage.sprite = Resources.Load<Sprite>("Icon" + counter);
    CountdownImage.gameObject.SetActive(true);
    while (counter > 0)
    {
      CountdownImage.sprite = Resources.Load<Sprite>("Icon" + counter);
      yield return new WaitForSeconds(1.0f);
      counter--;
    }
    CountdownImage.gameObject.SetActive(false);
    StartGame();
  }

  private void StartGame()
  {
    this.IsGameActive = true;
    this.LoadSequence();
    this.gameTimer.StartTimer(InitialGameTimerInSeconds);
  }

  #endregion

  #region Difficulty Level Handling
  private bool IsSequenceCleared()
	{
		for (int i = 0, count = this.allSynapses.Count; i < count; i++)
		{
			if (this.allSynapses[(SynapseLocation)i].Mode == SynapseMode.OneTimePositive)
			{
				return false;
			}
		}

		return true;
	}

	private bool DoesSequenceHaveRepetitivePositive()
	{
		for (int i = 0, count = this.allSynapses.Count; i < count; i++)
		{
			if (this.allSynapses[(SynapseLocation)i].Mode == SynapseMode.RepetitivePositive)
			{
				return true;
			}
		}

		return false;
	}

	private void AddConsecutiveSequenceClear()
	{
		this.consecutiveClearedSequences++;
	}
	#endregion

	#region Score Handling
	private void ScorePositiveHit()
	{
		this.scoreValue += (long) (GameManager.SCORE_INCREMENT_VALUE * this.comboValue);
		this.comboValue++;

    if (this.comboValue % 50 == 0)
    {
      this.goodGrannySound.Play();
    }
	}

	private void ScoreNegativeHit()
	{
    gameTimer.ReduceTimerBy(NegativeHitTimerPenaltyInSeconds);
    this.scoreValue -= (this.scoreValue / GameManager.SCORE_DECREMENT_DIVISOR);

    if (this.comboValue > 50)
    {
      this.badGrannySound.Play();
    }

		this.comboValue = 0;
	}

	private void ScoreNeutralHit()
	{
    if (this.comboValue > 50)
    {
      this.badGrannySound.Play();
    }

    this.comboValue = 0;
	}
	#endregion

	#region Synapse Hit Handling
	private void SynapseHit(SynapseLocation synapseHitLocation)
	{
    if (IsGameActive)
    {
      ProcessSynapseHit(synapseHitLocation);
    }
    // Display the hit effects all the time, even on game over.
    DisplaySynapseHitEffect(synapseHitLocation);
	}

  private void ProcessSynapseHit(SynapseLocation synapseHitLocation)
  {
    Synapse synapseObject = this.allSynapses[synapseHitLocation];
    synapseObject.HitSynapse();

    switch (synapseObject.Mode)
    {
      case SynapseMode.OneTimePositive:
        this.ProcessOneTimePositiveHit(synapseHitLocation);
        break;
      case SynapseMode.OneTimeNegative:
        this.ProcessOneTimeNegativeHit(synapseHitLocation);
        break;
      case SynapseMode.Neutral:
        this.ProcessNeutralHit(synapseHitLocation);
        break;
      case SynapseMode.RepetitivePositive:
        this.ProcessRepetitivePositiveHit(synapseHitLocation);
        break;
      default:
        Debug.LogError("GameManager.ProcessSynapseHit: Unknown synapse mode");
        break;
    }
  }

  private void DisplaySynapseHitEffect(SynapseLocation synapseHitLocation)
  {
    Synapse synapseObject = this.allSynapses[synapseHitLocation];
    switch (synapseObject.Mode)
    {
      case SynapseMode.OneTimePositive:
        this.DisplayOneTimePositiveHitEffect(synapseHitLocation);
        break;
      case SynapseMode.OneTimeNegative:
        this.DisplayOneTimeNegativeHitEffect(synapseHitLocation);
        break;
      case SynapseMode.Neutral:
        this.DisplayNeutralHitEffect(synapseHitLocation);
        break;
      case SynapseMode.RepetitivePositive:
        this.DisplayRepetitivePositiveHitEffect(synapseHitLocation);
        break;
      default:
        Debug.LogError("GameManager.DisplaySynapseHitEffect: Unknown synapse mode");
        break;
    }
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

	private void ProcessOneTimePositiveHit(SynapseLocation synapseLocation)
	{
    this.goodHitSound.Play();

    this.ScorePositiveHit();
    this.allSynapses[synapseLocation].SetSynapseMode(SynapseMode.Neutral);

    if (this.IsSequenceCleared())
    {
      this.AddConsecutiveSequenceClear();
      if (this.DoesSequenceHaveRepetitivePositive() == false)
      {
        if (this.runningSequenceCoroutine != null) { StopCoroutine(this.runningSequenceCoroutine); }
        if (this.currentLoadNextSequenceCoroutine != null) { StopCoroutine(this.currentLoadNextSequenceCoroutine); }
        this.currentLoadNextSequenceCoroutine = StartCoroutine(LoadSequenceOnNextFrame());
      }
    }
	}

  private void DisplayOneTimePositiveHitEffect(SynapseLocation synapseLocation)
  {
    CameraShaker.Instance.ShakeOnce(1f, 2f, 0.1f, 0.1f);
  }

  private void ProcessOneTimeNegativeHit(SynapseLocation synapseLocation)
	{
    this.badHitSound.Play();

    this.consecutiveClearedSequences = 0;
    this.ScoreNegativeHit();
    if (this.runningSequenceCoroutine != null) { StopCoroutine(this.runningSequenceCoroutine); }
    if (this.currentLoadNextSequenceCoroutine != null) { StopCoroutine(this.currentLoadNextSequenceCoroutine); }
    this.currentLoadNextSequenceCoroutine = StartCoroutine(LoadSequenceOnNextFrame());
	}

  private void DisplayOneTimeNegativeHitEffect(SynapseLocation synapseLocation)
  {
    CameraShaker.Instance.ShakeOnce(7f, 2f, 0.1f, 1f);
    Synapse synapseObject = this.allSynapses[synapseLocation];
    float screenSpaceX = Camera.main.WorldToViewportPoint(synapseObject.gameObject.transform.position).x;
    float screenSpaceY = Camera.main.WorldToViewportPoint(synapseObject.gameObject.transform.position).y;
    StartCoroutine(this.ShockWaveEffect(screenSpaceX, screenSpaceY));
  }

	private void ProcessNeutralHit(SynapseLocation synapseLocation)
	{
    this.ScoreNeutralHit();
	}

  private void DisplayNeutralHitEffect(SynapseLocation synapseLocation)
  {
    CameraShaker.Instance.ShakeOnce(4f, 1f, 0.3f, 0.3f);
  }

  private void ProcessRepetitivePositiveHit(SynapseLocation synapseLocation)
	{
    this.goodHitSound.Play();
    this.ScorePositiveHit();
	}

  private void DisplayRepetitivePositiveHitEffect(SynapseLocation synapseLocation)
  {
    CameraShaker.Instance.ShakeOnce(1f, 2f, 0.1f, 0.1f);
  }
  #endregion

  #region Sequence Load Handling
  private IEnumerator LoadSequenceOnNextFrame()
  {
    // Wait a frame before loading the next sequence to let all the animations complete.
    yield return null;
    LoadSequence();
  }

  private void LoadSequence()
	{
    Sequence sequenceToLoad = SequenceRetriever.GetNextSequence(this.currentDifficulty, this.currentSequence);
    this.currentSequence = sequenceToLoad;
    for (int i = 0, count = allSynapses.Count; i < count; i++)
		{
			this.allSynapses[(SynapseLocation) i].SetSynapseMode(sequenceToLoad.synapseModes[i]);
		}
		this.runningSequenceCoroutine = this.StartCoroutine(this.RunSequence());
	}

	private IEnumerator RunSequence()
	{
		for (float elapsedTime = 0; elapsedTime < this.currentSequence.sequenceDurationInSeconds; elapsedTime += Time.deltaTime)
		{
			yield return null;
		}

		if (this.IsSequenceCleared())
		{
			this.AddConsecutiveSequenceClear();
		}
		else
		{
			this.consecutiveClearedSequences = 0;
		}

    if (this.currentLoadNextSequenceCoroutine != null) { StopCoroutine(this.currentLoadNextSequenceCoroutine); }
    this.currentLoadNextSequenceCoroutine = StartCoroutine(LoadSequenceOnNextFrame());
	}
  #endregion

  #region End Game Handling
  private void OnTimerEnded()
  {
    this.IsGameActive = false;
    this.StopAllCoroutines();
    for (int i = 0, count = allSynapses.Count; i < count; i++)
    {
      Synapse synapse = allSynapses[(SynapseLocation) i];
      synapse.StopAllCoroutines();
      synapse.SetSynapseMode(SynapseMode.OneTimeNegative);
    }

    gameOverUI.SetActive(true);
  }
  #endregion

  public static float GetRandomPitch()
  {
    return Random.Range(0.5f, 1.5f);
  }
}
