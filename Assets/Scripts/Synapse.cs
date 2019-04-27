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

  /*
  // TODO: Throw this shit ass test code away when you're done with it.
  public void OnMouseDown()
  {
    OnTouch();
  }

  // TODO: Throw this shit ass test code away when you're done with it.
  public void ChangeModes()
  {
    switch (Mode)
    {
      case SynapseMode.OneTimePositive:
        SetSynapseMode(SynapseMode.OneTimeNegative);
        break;
      case SynapseMode.OneTimeNegative:
        SetSynapseMode(SynapseMode.RepetitivePositive);
        break;
      case SynapseMode.RepetitivePositive:
        SetSynapseMode(SynapseMode.Neutral);
        break;
      case SynapseMode.Neutral:
        SetSynapseMode(SynapseMode.OneTimePositive);
        break;
    }
  }
  */
  public void OnTouch()
  {
    // TODO: Hook this up to whatever is keeping score.
    switch (Mode)
    {
      case SynapseMode.OneTimePositive:
        Debug.Log("This synapse is one time positive");
        break;
      case SynapseMode.OneTimeNegative:
        Debug.Log("This synapse is one time negative");
        break;
      case SynapseMode.RepetitivePositive:
        Debug.Log("This synapse is repetitive positive");
        break;
      case SynapseMode.Neutral:
        Debug.Log("This synapse is neutral");
        break;
    }
    Illuminator.OnSynapseTouched(Mode);
  }

  public void SetSynapseMode(SynapseMode ModeToSet)
  {
    Mode = ModeToSet;
    Illuminator.OnSynapseModeChanged(ModeToSet);
  }
}
