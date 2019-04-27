using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum SynapseLocation { LeftLeft, LeftRight, LeftUp, LeftDown, RightLeft, RightRight, RightUp, RightDown };
public enum GameDifficulty {  Easy, Medium, Hard };
public class GameManager : MonoBehaviour {

	private const int SEQUENCE_CLEAR_LEVEL_UP_THRESHOLD = 10;
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

	[SerializeField]
	private Timer gameTimer;
  [SerializeField]
  private float timerInitValueInSeconds = Timer.DEFAULT_TIMER_INIT_VALUE_IN_SECONDS;

	[SerializeField]
	private Text scoreText;
	private int scoreValue = 0;
	[SerializeField]
	private Text comboText;
	private int comboValue = 1;

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

    StartGame();
	}

	public void StartGame()
	{
		this.gameTimer.Reset(timerInitValueInSeconds);
		this.LoadSequence(SequenceRetriever.GetNextSequence(this.currentDifficulty, this.currentSequence));
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
		this.scoreText.text = this.scoreValue.ToString();
		this.comboValue++;
		this.comboText.text = "Combo: x" + this.comboValue.ToString();
	}

	private void ScoreNegativeHit()
	{
		int decrementAmount = Mathf.RoundToInt((GameManager.SCORE_DECREMENT_PERCENTAGE) * this.scoreValue);
		this.scoreValue -= decrementAmount;
		this.scoreText.text = this.scoreValue.ToString();
		this.comboValue = 0;
		this.comboText.text = string.Empty;
	}

	private void ScoreNeutralHit()
	{
		this.comboValue = 0;
		this.comboText.text = string.Empty;
	}
	#endregion

	#region Synapse Hit Handling
	private void SynapseHit(SynapseLocation hitSynapse)
	{
		Debug.Log(hitSynapse + " Hit!");
		this.allSynapses[hitSynapse].HitSynapse();

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

		this.allSynapses[hitSynapse].HitSynapse();
	}

	private void OneTimePositiveHit(SynapseLocation synapseLocation)
	{
		this.ScorePositiveHit();

		this.allSynapses[synapseLocation].SetSynapseMode(SynapseMode.Neutral);

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

		StopCoroutine(this.runningSequenceCoroutine);
		this.LoadSequence(SequenceRetriever.GetNextSequence(this.currentDifficulty, this.currentSequence));
	}

	private void NeutralHit(SynapseLocation synapseLocation)
	{
		this.ScoreNeutralHit();
	}

	private void RepetitivePositiveHit(SynapseLocation synapseLocation)
	{
		this.ScorePositiveHit();
	}
  #endregion

  #region Sequence Load Handling
  public void LoadSequence(Sequence sequenceToLoad)
	{
		for (int i = 0; i < this.allSynapses.Count; i++)
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
    this.StopAllCoroutines();
    Debug.Log("GAME OVER!");

  }
  #endregion
}
