﻿using System.Collections;
using UnityEngine;

public class SynapseIlluminator : MonoBehaviour
{
  private float flashOnDurationInSeconds = 0.1f;
  [SerializeField]
  private ParticleSystem PositiveParticleEffect;
  [SerializeField]
  private ParticleSystem NegativeParticleEffect;
  [SerializeField]
  private ParticleSystem ElectricParticleEffect;

  public void OnSynapseModeChanged(SynapseMode newSynapseMode)
  {
    StopAllCoroutines();

    this.ElectricParticleEffect.Stop();

    MeshRenderer renderer = GetComponent<MeshRenderer>();
    switch (newSynapseMode)
    {
      case SynapseMode.OneTimePositive:
        renderer.material = Resources.Load<Material>("BlueSynapse");
        break;
      case SynapseMode.OneTimeNegative:
        renderer.material = Resources.Load<Material>("RedSynapse");
        break;
      case SynapseMode.RepetitivePositive:
        StartCoroutine(this.BlinkSynapse());
        break;
      case SynapseMode.Neutral:
        renderer.material = Resources.Load<Material>("SynapseBase");
        break;
      case SynapseMode.RepetitivePositiveTutorial:
        StartCoroutine(this.BlinkSynapseTutorial());
        break;
    }

    this.TurnOnElectricParticles(newSynapseMode);
  }

  public void OnSynapseTouched(SynapseMode touchedSynapseMode)
  {
    switch (touchedSynapseMode)
    {
      case SynapseMode.OneTimePositive:
        PositiveParticleEffect.Play();
        break;
      case SynapseMode.OneTimeNegative:
        NegativeParticleEffect.Play();
        break;
      case SynapseMode.RepetitivePositive:
        PositiveParticleEffect.Play();
        break;
      case SynapseMode.Neutral:
        // TODO: Add any touch effects we want.
        ElectricParticleEffect.Stop();
        break;
    }
  }

  private void TurnOnElectricParticles(SynapseMode newSynapseMode)
  {
    Gradient newGradient = new Gradient();

    switch (newSynapseMode)
    {
      case SynapseMode.OneTimePositive:
        newGradient.SetKeys(new GradientColorKey[] { new GradientColorKey(new Color32(0xFF, 0xFF, 0xFF, 0xFF), 0.0f),
          new GradientColorKey(new Color32(0x00, 0xB8, 0xE2, 0xFF), 1.0f) },
          new GradientAlphaKey[] { new GradientAlphaKey(1.0f, 0.0f), new GradientAlphaKey(1.0f, 1.0f) });
        break;
      case SynapseMode.OneTimeNegative:
        newGradient.SetKeys(new GradientColorKey[] { new GradientColorKey(new Color32(0xFF, 0xFF, 0xFF, 0xFF), 0.0f),
          new GradientColorKey(new Color32(0xFF, 0x36, 0x36, 0xFF), 1.0f) }, 
          new GradientAlphaKey[] { new GradientAlphaKey(1.0f, 0.0f), new GradientAlphaKey(1.0f, 1.0f) });
        break;
      case SynapseMode.RepetitivePositive:
        newGradient.SetKeys(new GradientColorKey[] { new GradientColorKey(new Color32(0xFF, 0xFF, 0xFF, 0xFF), 0.0f),
          new GradientColorKey(new Color32(0x00, 0xB8, 0xE2, 0xFF), 1.0f) },
          new GradientAlphaKey[] { new GradientAlphaKey(1.0f, 0.0f), new GradientAlphaKey(1.0f, 1.0f) });
        break;
      case SynapseMode.Neutral:
        return;
        break;
      case SynapseMode.RepetitivePositiveTutorial:
        newGradient.SetKeys(new GradientColorKey[] { new GradientColorKey(new Color32(0xFF, 0xFF, 0xFF, 0xFF), 0.0f),
          new GradientColorKey(new Color32(0x00, 0xB8, 0xE2, 0xFF), 1.0f) },
          new GradientAlphaKey[] { new GradientAlphaKey(1.0f, 0.0f), new GradientAlphaKey(1.0f, 1.0f) });
        break;
    }

    ParticleSystem.ColorOverLifetimeModule particleColor = this.ElectricParticleEffect.colorOverLifetime;
    particleColor.color = newGradient;
    this.ElectricParticleEffect.Play();
  }

  private IEnumerator BlinkSynapse()
  {
    MeshRenderer renderer = GetComponent<MeshRenderer>();
    float offDuration = 0.0f;
    for (float i = 0; i < GameManager.instance.currentSequence.sequenceDurationInSeconds; i += Time.deltaTime + flashOnDurationInSeconds + offDuration)
    {
      offDuration = ((GameManager.instance.currentSequence.sequenceDurationInSeconds - i) / 10.0f);

      renderer.material = Resources.Load<Material>("BlinkingSynapse");
      yield return new WaitForSeconds(this.flashOnDurationInSeconds);
      renderer.material = Resources.Load<Material>("SynapseBase");
      yield return new WaitForSeconds(offDuration);
    }

  }

  private IEnumerator BlinkSynapseTutorial()
  {
    MeshRenderer renderer = GetComponent<MeshRenderer>();
    float totalDuration = 5f;
    float offDuration = 0.0f;

    while (TutorialManager.instance.flashingSynapseHit == false)
    {
      offDuration = totalDuration / 10f;

      renderer.material = Resources.Load<Material>("BlinkingSynapse");
      yield return new WaitForSeconds(this.flashOnDurationInSeconds);

      if (TutorialManager.instance.flashingSynapseHit == true)
      {
        break;
      }

      renderer.material = Resources.Load<Material>("SynapseBase");
      yield return new WaitForSeconds(offDuration);

      if (TutorialManager.instance.flashingSynapseHit == true)
      {
        break;
      }
    }

    for (float i = 0; i < totalDuration; i += Time.deltaTime + flashOnDurationInSeconds + offDuration)
    {
      offDuration = ((totalDuration - i) / 10.0f);

      renderer.material = Resources.Load<Material>("BlinkingSynapse");
      yield return new WaitForSeconds(this.flashOnDurationInSeconds);
      renderer.material = Resources.Load<Material>("SynapseBase");
      yield return new WaitForSeconds(offDuration);
    }

    TutorialManager.instance.tutorialComplete = true;
  }
}
