using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using EZCameraShake;

public enum SynapseLocation { LeftLeft, LeftRight, LeftUp, LeftDown, RightLeft, RightRight, RightUp, RightDown };
public enum GameDifficulty {  Easy, Medium, Hard };
public class GameManager : MonoBehaviour {

	private const int SEQUENCE_CLEAR_LEVEL_UP_THRESHOLD = 15;
	private const float TIMER_MEDIUM_LEVEL_UP_THRESHOLD = Timer.DEFAULT_TIMER_INIT_VALUE_IN_SECONDS * (2.0f / 3.0f);
	private const float TIMER_HARD_LEVEL_UP_THRESHOLD = Timer.DEFAULT_TIMER_INIT_VALUE_IN_SECONDS * (1.0f / 3.0f);

	private const int SCORE_INCREMENT_VALUE = 50;
	private const float SCORE_DECREMENT_PERCENTAGE = 0.3f;

	static GameManager instance;

	public Dictionary<SynapseLocation, Synapse> allSynapses;

	public GameDifficulty currentDifficulty = GameDifficulty.Easy;

  private Sequence currentSequence = null;
	private Coroutine runningSequenceCoroutine;
	private int consecutiveClearedSequences = 0;
  private bool IsGameActive = false;

	[SerializeField]
	private Timer gameTimer;
  [SerializeField]
  private float timerInitValueInSeconds = Timer.DEFAULT_TIMER_INIT_VALUE_IN_SECONDS;
  [SerializeField]
  private GameObject gameOverUI;
  /// <summary>
  /// This game object contains the countdown text and the get ready text,
  /// so we can disable it to disable both.
  /// </summary>
  [SerializeField]
  private GameObject getReadyUI;
  /// <summary>
  /// Will tick down every second to give the user some time to prepare
  /// before the game starts.
  /// </summary>
  [SerializeField]
  private Text CountdownText;

	[SerializeField]
	private Text scoreText;
	private int scoreValue = 0;

	[SerializeField]
	private Text comboText;
	private int comboValue = 1;

	[SerializeField]
	private Material shockWaveMaterial;

	void Awake()
	{
		instance = this;

		this.allSynapses = new Dictionary<SynapseLocation, Synapse>();

		GameObject synapsesParent = GameObject.Find("Synapses");
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

    ReadyGame();
	}

	public void ReadyGame()
	{
		this.gameTimer.SetTime();
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
    CountdownText.text = counter.ToString();
    getReadyUI.SetActive(true);
    while (counter > 0)
    {
      CountdownText.text = counter.ToString();
      yield return new WaitForSeconds(1.0f);
      counter--;
    }
    getReadyUI.SetActive(false);
    StartGame();
  }

  private void StartGame()
  {
    this.IsGameActive = true;
    this.LoadSequence(SequenceRetriever.GetNextSequence(this.currentDifficulty, this.currentSequence));
    this.gameTimer.Reset(timerInitValueInSeconds);
  }

	private void Update()
	{
		if (this.gameTimer.TimerValueInSeconds <= GameManager.TIMER_MEDIUM_LEVEL_UP_THRESHOLD && this.currentDifficulty < GameDifficulty.Medium)
		{
			this.currentDifficulty = GameDifficulty.Medium;
		}
		else if (this.gameTimer.TimerValueInSeconds <= GameManager.TIMER_HARD_LEVEL_UP_THRESHOLD && this.currentDifficulty < GameDifficulty.Hard)
		{
			this.currentDifficulty = GameDifficulty.Hard;
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

	#region Difficulty Level Handling
	private bool IsSequenceCleared()
	{
		for (int i = 0; i < this.allSynapses.Count; i++)
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
		for (int i = 0; i < this.allSynapses.Count; i++)
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

		if (this.consecutiveClearedSequences >= GameManager.SEQUENCE_CLEAR_LEVEL_UP_THRESHOLD && this.currentDifficulty != GameDifficulty.Hard)
		{
			this.currentDifficulty++;
			this.consecutiveClearedSequences = 0;
		}
	}
	#endregion

	#region Score Handling
	private void ScorePositiveHit()
	{
		this.scoreValue += (GameManager.SCORE_INCREMENT_VALUE * this.comboValue);
		this.comboValue++;
	}

	private void ScoreNegativeHit()
	{
		int decrementAmount = Mathf.RoundToInt((GameManager.SCORE_DECREMENT_PERCENTAGE) * this.scoreValue);
		this.scoreValue -= decrementAmount;
		this.comboValue = 0;
	}

	private void ScoreNeutralHit()
	{
		this.comboValue = 0;
	}
	#endregion

	#region Synapse Hit Handling
	private void SynapseHit(SynapseLocation hitSynapse)
	{
    if (IsGameActive == false)
    {
      // We want the player to be able to move the needles
      // around when the game is over but don't want to
      // process those hits.
      return;
    }

		Debug.Log(hitSynapse + " Hit!");

		switch (this.allSynapses[hitSynapse].Mode)
		{
			case SynapseMode.OneTimePositive:
				this.OneTimePositiveHit(hitSynapse);
				break;
			case SynapseMode.OneTimeNegative:
				this.OneTimeNegativeHit(hitSynapse);
				break;
			case SynapseMode.Neutral:
				this.NeutralHit(hitSynapse);
				break;
			case SynapseMode.RepetitivePositive:
				this.RepetitivePositiveHit(hitSynapse);
				break;
			default:
				Debug.LogError("GameManager.SynapseHit: Unknown synapse mode");
				break;
		}

		Synapse synapseObject = this.allSynapses[hitSynapse];

		synapseObject.HitSynapse();
	}

	IEnumerator ShockWaveEffect(float screenSpaceX, float screenSpaceY)
	{
		shockWaveMaterial.SetFloat("_CenterX", screenSpaceX);
		shockWaveMaterial.SetFloat("_CenterY", screenSpaceY);

		float tParam = 0;
		float waveRadius;
		while (tParam < 1)
		{
			tParam += Time.deltaTime * 2;
			waveRadius = Mathf.Lerp(-0.2f, 2, tParam);
			shockWaveMaterial.SetFloat("_Radius", waveRadius);
			yield return null;
		}
	}

	private void OneTimePositiveHit(SynapseLocation synapseLocation)
	{
		this.ScorePositiveHit();

		this.allSynapses[synapseLocation].SetSynapseMode(SynapseMode.Neutral);

		CameraShaker.Instance.ShakeOnce(1f, 2f, 0.1f, 0.1f);

		if (this.IsSequenceCleared())
		{
			this.AddConsecutiveSequenceClear();
			if (this.DoesSequenceHaveRepetitivePositive() == false)
			{
				StopCoroutine(this.runningSequenceCoroutine);
				this.LoadSequence(SequenceRetriever.GetNextSequence(this.currentDifficulty, this.currentSequence));
			}
		}
	}

	private void OneTimeNegativeHit(SynapseLocation synapseLocation)
	{
		this.ScoreNegativeHit();

		this.consecutiveClearedSequences = 0;

		CameraShaker.Instance.ShakeOnce(7f, 2f, 0.1f, 1f);

		Synapse synapseObject = this.allSynapses[synapseLocation];
		float screenSpaceX = Camera.main.WorldToViewportPoint(synapseObject.gameObject.transform.position).x;
		float screenSpaceY = Camera.main.WorldToViewportPoint(synapseObject.gameObject.transform.position).y;
		StartCoroutine(this.ShockWaveEffect(screenSpaceX, screenSpaceY));

		StopCoroutine(this.runningSequenceCoroutine);
		this.LoadSequence(SequenceRetriever.GetNextSequence(this.currentDifficulty, this.currentSequence));
	}

	private void NeutralHit(SynapseLocation synapseLocation)
	{
		CameraShaker.Instance.ShakeOnce(4f, 1f, 0.3f, 0.3f);
		this.ScoreNeutralHit();
	}

	private void RepetitivePositiveHit(SynapseLocation synapseLocation)
	{
		CameraShaker.Instance.ShakeOnce(3f, 2f, 0.1f, 0.1f);
		this.ScorePositiveHit();
	}
  #endregion

  #region Sequence Load Handling
  public void LoadSequence(Sequence sequenceToLoad)
	{
		for (int i = 0, count = allSynapses.Count; i < count; i++)
		{
			this.allSynapses[(SynapseLocation) i].SetSynapseMode(sequenceToLoad.synapseModes[i]);
		}
		this.currentSequence = sequenceToLoad;
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

		this.LoadSequence(SequenceRetriever.GetNextSequence(this.currentDifficulty, this.currentSequence));
	}
  #endregion

  #region End Game Handling
  private void OnTimerEnded()
  {
    this.IsGameActive = false;
    this.StopAllCoroutines();
    gameOverUI.SetActive(true);
  }
  #endregion
}
