using UnityEngine;

public class SynapseIlluminator : MonoBehaviour
{
  public void OnSynapseModeChanged(SynapseMode newSynapseMode)
  {
    MeshRenderer renderer = GetComponent<MeshRenderer>();
    switch (newSynapseMode)
    {
      case SynapseMode.OneTimePositive:
        renderer.material = Resources.Load<Material>("Green");
        break;
      case SynapseMode.OneTimeNegative:
        renderer.material = Resources.Load<Material>("Red");
        break;
      case SynapseMode.RepetitivePositive:
        renderer.material = Resources.Load<Material>("Blue");
        break;
      case SynapseMode.Neutral:
        renderer.material = Resources.Load<Material>("White");
        break;
    }
  }

  public void OnSynapseTouched(SynapseMode touchedSynapseMode)
  {
    switch (touchedSynapseMode)
    {
      case SynapseMode.OneTimePositive:
        // TODO: Add any touch effects we want.
        break;
      case SynapseMode.OneTimeNegative:
        // TODO: Add any touch effects we want.
        break;
      case SynapseMode.RepetitivePositive:
        // TODO: Add any touch effects we want.
        break;
      case SynapseMode.Neutral:
        // TODO: Add any touch effects we want.
        break;
    }
  }
}
