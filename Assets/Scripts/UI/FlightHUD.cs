using System;
using System.Collections;
using MFlight;
using TMPro;
using UnityEngine;

public class FlightHUD : MonoBehaviour
{
    public static FlightHUD instance;
    
    [Header("References")]
    [SerializeField] private TMP_Text collectablesText;
    [SerializeField] private TMP_Text timerText;
    [SerializeField] private Animator fadeAnimator;
    [SerializeField] private MouseFlightController mouseController;
    
    private void Awake()
    {
        instance = this;
    }

    private void Start()
    {
        if (mouseController != null)
        {
            mouseController.freezeControls = true;
            StartCoroutine(StartFlightAnimation());
        }
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

    private IEnumerator StartFlightAnimation()
    {
        yield return new WaitForSeconds(8.5f);

        if (mouseController != null)
        {
            mouseController.freezeControls = false;
        }
    }
}
