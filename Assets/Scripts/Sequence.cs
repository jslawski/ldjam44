using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Sequence", menuName = "Sequence")]
public class Sequence : ScriptableObject {

	//NOTE: ALL SYNAPSES MUST BE ORDERED IN THE SAME ORDER AS THE SYNAPSELOCATION ENUM!
	public List<SynapseMode> synapseModes;
	public float sequenceDurationInSeconds;
}
