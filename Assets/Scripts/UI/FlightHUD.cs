using System;
using System.Collections;
using MFlight;
using TMPro;
using UnityEngine;
using UnityEngine.Audio;

public class FlightHUD : MonoBehaviour
{
    public static FlightHUD instance;
    
    [Header("References")]
    [SerializeField] private CanvasGroup mainSection, pauseSection;
    [SerializeField] private TMP_Text collectablesText;
    [SerializeField] private TMP_Text timerText, altitudeText, speedText;
    [SerializeField] private Animator hudAnimator;
    [SerializeField] private Animator fadeAnimator;
    [SerializeField] private MouseFlightController mouseController;
    [SerializeField] private AudioMixer mainMixer;

    private float volumeForce = 1.0f;

    private bool paused = false;
    
    private void Awake()
    {
        instance = this;
    }

    private void Start()
    {
        mainMixer.GetFloat("volume", out volumeForce);
        FlightAudioManager.instance.StartCountDown();
        if (mouseController != null)
        {
            mouseController.freezeControls = true;
            StartCoroutine(StartFlightAnimation());
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            paused = !paused;

            if (paused == true)
            {
                Time.timeScale = 0.0f;
                mainSection.alpha = 0.0f;
                pauseSection.alpha = 1.0f;
                pauseSection.interactable = true;
                mainMixer.SetFloat("volume", -80.0f);
            }
            else
            {
                Time.timeScale = 1.0f;
                mainSection.alpha = 1.0f;
                pauseSection.alpha = 0.0f;
                pauseSection.interactable = false;
                mainMixer.SetFloat("volume", 0.0f);
            }
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

    public void UpdateAltitude(float altitude)
    {
        altitudeText.text = "<mspace=0.5em>" + altitude.ToString("0") + "</mspace>";
    }
    
    public void UpdateSpeed(float speed)
    {
        speedText.text = speed.ToString("0") + " km/h";
    }

    public void FadeOut()
    {
        fadeAnimator.SetBool("Show", false);
    }

    public void StartFinalCountdown()
    {
        hudAnimator.SetTrigger("Countdown");
    }

    public void StartVictoryMessage()
    {
        hudAnimator.SetTrigger("Victory");
    }

    private IEnumerator StartFlightAnimation()
    {
        yield return new WaitForSeconds(4.5f);

        if (mouseController != null)
        {
            mouseController.freezeControls = false;
        }
    }

    public void OnResumeButton()
    {
        mainMixer.SetFloat("volume", 0.0f);
        Time.timeScale = 1.0f;
        mainSection.alpha = 1.0f;
        pauseSection.alpha = 0.0f;
        pauseSection.interactable = false;
        paused = false;
    }

    public void OnQuitButton()
    {
        Time.timeScale = 1.0f;
        GameManager.instance.QuitFlightScene();
    }
}
