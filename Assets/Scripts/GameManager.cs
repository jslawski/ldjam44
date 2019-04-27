using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum SynapseLocation { LeftLeft, LeftRight, LeftUp, LeftDown, RightLeft, RightRight, RightUp, RightDown };
public enum GameDifficulty {  Easy, Medium, Hard };
public class GameManager : MonoBehaviour {

	private const int SEQUENCE_CLEAR_LEVEL_UP_THRESHOLD = 10;
	private const float TIMER_MEDIUM_LEVEL_UP_THRESHOLD = Timer.DefaultTimerInitValueInSeconds * (2.0f / 3.0f);
	private const float TIMER_HARD_LEVEL_UP_THRESHOLD = Timer.DefaultTimerInitValueInSeconds * (1.0f / 3.0f);

	static GameManager instance;

	public Dictionary<SynapseLocation, Synapse> allSynapses;

	public GameDifficulty currentDifficulty = GameDifficulty.Easy;

	private Sequence currentSequence;
	private Coroutine runningSequenceCoroutine;
	private int consecutiveClearedSequences = 0;

	[SerializeField]
	private Timer gameTimer;

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

		this.gameTimer.Reset();
	}

	public void StartGame()
	{
		this.gameTimer.Reset();
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

	#region Synapse Hit Handling
	private void SynapseHit(SynapseLocation hitSynapse)
	{
		Debug.Log(hitSynapse + " Hit!");
		this.allSynapses[hitSynapse].HitSynapse();

		switch (this.allSynapses[hitSynapse].Mode)
		{
			case SynapseMode.OneTimePositive:
				this.OneTimePositiveHit();
				break;
			case SynapseMode.OneTimeNegative:
				this.OneTimeNegativeHit();
				break;
			case SynapseMode.Neutral:
				this.NeutralHit();
				break;
			case SynapseMode.RepetitivePositive:
				this.RepetitivePositiveHit();
				break;
			default:
				Debug.LogError("GameManager.SynapseHit: Unknown synapse mode");
				break;
		}
	}

	private void OneTimePositiveHit()
	{
		//TODO: Increment score and combo here...

		//Check sequence clear
		if (this.IsSequenceCleared())
		{
			this.AddConsecutiveSequenceClear();
			StopCoroutine(this.runningSequenceCoroutine);
			this.LoadSequence(SequenceRetriever.GetNextSequence(this.currentDifficulty, this.currentSequence));
		}
	}

	private void OneTimeNegativeHit()
	{
		//TODO: Decrement score and clear combo

		this.consecutiveClearedSequences = 0;

		StopCoroutine(this.runningSequenceCoroutine);
		this.LoadSequence(SequenceRetriever.GetNextSequence(this.currentDifficulty, this.currentSequence));
	}

	private void NeutralHit()
	{
		//TODO: Clear combo here
	}

	private void RepetitivePositiveHit()
	{
		//TODO: Increment score and combo here
	}
	#endregion

	#region Sequence Load Handling
	public void LoadSequence(Sequence sequenceToLoad)
	{
		for (int i = 0; i < this.allSynapses.Count; i++)
		{
			this.allSynapses[(SynapseLocation)i].Mode = sequenceToLoad.synapseModes[i];
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
}
