using UnityEngine;
using UnityEngine.Audio;

public class FlightAudioManager : MonoBehaviour
{
    public static FlightAudioManager instance;

    [SerializeField]
    private AudioMixer mixer;
    
    [SerializeField]
    private AudioSource gameplayEventsSource;
    [SerializeField]
    private AudioClip explosionClip;
    [SerializeField]
    private AudioClip collectClip;
    [SerializeField]
    private AudioClip startCountDown;
    [SerializeField] private AudioClip endTimerAudioClip;

    [SerializeField]
    private AudioSource windSource;
    [Range(0.0f, 1.0f), SerializeField]
    private float windMaxVolume = 0.5f;
    
    [SerializeField]
    private AudioSource engineSource;

    private bool endTimerTrigger = false;

    private void Awake()
    {
        instance = this;
    }

    public void SetWindVolume(float planeHeight)
    {
        Vector2 planeRange = new Vector2(0.0f, 800.0f);
        float d = Mathf.Clamp01(planeHeight / (planeRange.y - planeRange.x));
        windSource.volume = windMaxVolume * d;
    }

    public void PlaneExplosion()
    {
        gameplayEventsSource.PlayOneShot(explosionClip);
    }

    public void PlaneCollect()
    {
        gameplayEventsSource.PlayOneShot(collectClip);
    }

    public void StartCountDown()
    {
        gameplayEventsSource.PlayOneShot(startCountDown);
    }

    public void EndCountDown()
    {
        gameplayEventsSource.PlayOneShot(endTimerAudioClip);
        Debug.Log("END TIMER NOW");
    }

    public void ActiveEngineSound(bool stop)
    {
        engineSource.mute = stop;
    }
}
