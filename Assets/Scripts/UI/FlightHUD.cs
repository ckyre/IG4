using System;
using TMPro;
using UnityEngine;

public class FlightHUD : MonoBehaviour
{
    public static FlightHUD instance;
    
    [Header("References")]
    [SerializeField] private TMP_Text collectablesText;
    [SerializeField] private TMP_Text timerText;
    [SerializeField] private Animator fadeAnimator;

    private void Awake()
    {
        instance = this;
    }

    public void UpdateCollectables(int current, int max)
    {
        collectablesText.text = current + "/" + max;
    }

    public void UpdateTimer(double seconds)
    {
        TimeSpan timer = TimeSpan.FromSeconds(seconds);
        timerText.text = timer.ToString(@"mm\:ss");
    }

    public void FadeOut()
    {
        fadeAnimator.SetBool("Show", false);
    }
}
