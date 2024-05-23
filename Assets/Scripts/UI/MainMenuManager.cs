using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MainMenuManager : MonoBehaviour
{
    [SerializeField] private InterfaceSection startSection, menuSection, gameSettingsSection, looseSection, winSection;
    [SerializeField] private KeyCode startButton = KeyCode.Space;
    [SerializeField] private TMP_Text resultsText;
    
    private InterfaceSection currentSection;

    private void Start()
    {
        GameManager.GameManagerState GMState = GameManager.instance.CurrentState();

        if (GMState == GameManager.GameManagerState.Win)
        {
            int collectables = GameManager.instance.CollectablesCount();
            double time = GameManager.instance.CurrentTimer();
            resultsText.text = String.Format("Score: {0}\nTime: {1:N2}", collectables, time);
            OpenSection(winSection);
        }
        else if (GMState == GameManager.GameManagerState.Loose)
        {
            OpenSection(looseSection);
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

    public void OnBackMenuButton()
    {
        OpenSection(menuSection);
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
