﻿using UnityEngine;
using UnityEngine.UI;

/**
 * This class represents a timer that counts down from
 * the initial value to 0. Given a text field it will
 * update display the timer formmated as mm:ss
 */
public class Timer : MonoBehaviour
{
  public delegate void TimerEndedHandler();
  public event TimerEndedHandler OnTimerEnded;

  public const float DEFAULT_TIMER_INIT_VALUE_IN_SECONDS = 120.0f;

  [SerializeField]
  private Text TimerText;
  public float TimerValueInSeconds { get; private set; }
  public bool IsPaused = true;

	void Update ()
  {
    if (IsPaused == true)
    {
      UpdateTimerText();
      return;
    }

    if (TimerValueInSeconds <= 0.0f)
    {
      if (OnTimerEnded != null)
      {
        OnTimerEnded();
      }

      Pause();
    }
    else
    {
      TimerValueInSeconds -= Time.deltaTime;
    }

    UpdateTimerText();
  }

  private void UpdateTimerText()
  {
    if (TimerValueInSeconds <= 0.0F)
    {
      TimerText.text = "00:00";
      return;
    }

    string minutes = Mathf.Floor(TimerValueInSeconds / 60).ToString("00");
    string seconds = Mathf.Floor(TimerValueInSeconds % 60).ToString("00");
    TimerText.text = string.Format("{0}:{1}", minutes, seconds);
  }

  public void Pause()
  {
    IsPaused = true;
  }

	public void SetTime(float intervalTimerValueInSeconds = DEFAULT_TIMER_INIT_VALUE_IN_SECONDS)
	{
		TimerValueInSeconds = intervalTimerValueInSeconds;
	}

  public void Reset(float initialTimerValueInSeconds = DEFAULT_TIMER_INIT_VALUE_IN_SECONDS)
  {
    TimerValueInSeconds = initialTimerValueInSeconds;
    IsPaused = false;
  }

  public void ReduceTimerBy(float seconds)
  {
    TimerValueInSeconds -= seconds;
  }
}
