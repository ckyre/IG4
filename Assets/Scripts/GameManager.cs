using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    [Header("References")]
    [SerializeField] private FlightHUD hud;
    [Header("Game parameters")]
    [SerializeField] private double startTimer = 30.0f;
    [SerializeField] private int maxCollectables = 1;
    [Header("Player current stats")]
    [SerializeField] private int collectables = 0;
    [SerializeField] private bool alive = true;

    private double timer;
    
    private void Awake()
    {
        // Singleton.
        
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            // Cannot create multiple instance of GameManager.
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        maxCollectables = FindObjectsOfType<Collectable>().Length;
        hud.UpdateCollectables(collectables, maxCollectables);

        timer = startTimer;
    }

    private void Update()
    {
        timer -= Time.deltaTime;
        hud.UpdateTimer(Math.Max(0.0, timer));
    }

    public void PlayerCollect()
    {
        collectables++;
        hud.UpdateCollectables(collectables, maxCollectables);
    }

    public void PlayerCrash()
    {
        if (alive == false)
            return;
        
        alive = false;
        StartCoroutine("LooseGame");
    }

    private IEnumerator LooseGame()
    {
        yield return new WaitForSeconds(3.0f);
        hud.FadeOut();
        yield return new WaitForSeconds(2.0f);
        // Load main menu scene.
    }
}
