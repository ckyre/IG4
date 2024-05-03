using System;
using UnityEngine;
using UnityEngine.UI;

public class MainMenuManager : MonoBehaviour
{
    [SerializeField] private InterfaceSection startSection, menuSection, gameSettingsSection, endSection;
    [SerializeField] private KeyCode startButton = KeyCode.Space;

    private InterfaceSection currentSection;

    private void Start()
    {
        GameManager.GameManagerState GMState = GameManager.instance.CurrentState();

        if (GMState == GameManager.GameManagerState.Win ||
            GMState == GameManager.GameManagerState.Loose)
        {
            OpenSection(endSection);
        }
        else
        {
            OpenSection(startSection);
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(startButton) && currentSection == startSection)
        {
            OpenSection(menuSection);
        }
    }

    private void OpenSection(InterfaceSection nextSection)
    {
        if (currentSection != null)
        {
            currentSection.Active(false);
        }

        currentSection = nextSection;
        currentSection.Active(true);
    }
    
    public void OnPlayButton()
    {
        OpenSection(gameSettingsSection);
    }

    public void OnQuitButton()
    {
        GameManager.instance.QuitApplication();
    }

    public void OnDayButton()
    {
        GameManager.instance.StartPlay(GameManager.MapSettings.Day);
    }
    
    public void OnDuskButton()
    {
        GameManager.instance.StartPlay(GameManager.MapSettings.Dusk);
    }
    
    public void OnNightButton()
    {
        GameManager.instance.StartPlay(GameManager.MapSettings.Night);
    }
}
