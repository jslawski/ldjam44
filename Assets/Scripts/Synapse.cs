using UnityEngine;
using UnityEngine.Assertions;

/**
 * Represents a single Synapse that can be lit up
 * and touched.
 */
public class Synapse : MonoBehaviour
{
  public SynapseMode Mode = SynapseMode.Neutral;
  public SynapseIlluminator Illuminator;


  public void Awake()
  {
    Assert.IsTrue(Illuminator != null, "Illuminator has not been set for Synapse. This Synapse's color will not change.");
    Illuminator.OnSynapseModeChanged(Mode);
  }

  public void HitSynapse()
  {
    Illuminator.OnSynapseTouched(Mode);


  }

  public void SetSynapseMode(SynapseMode modeToSet)
  {
    Mode = modeToSet;
    Illuminator.OnSynapseModeChanged(modeToSet);
  }
}
