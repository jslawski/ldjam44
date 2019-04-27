using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum SynapseLocation { LeftLeft, LeftRight, LeftUp, LeftDown, RightLeft, RightRight, RightUp, RightDown };

public class GameManager : MonoBehaviour {

	static GameManager instance;

	public Dictionary<SynapseLocation, Synapse> allSynapses;

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
	}

	private void SynapseHit(SynapseLocation hitSynapse)
	{
		Debug.Log(hitSynapse + " Hit!");
		this.allSynapses[hitSynapse].HitSynapse();
	}
}
