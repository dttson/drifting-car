using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum GameState { Ready, Racing, Finished }

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Race Settings")]
    public float countdownTime = 3f;
    public GameUIManager uiManager;
    public DriftCarController myCar;
    public AICarController[] aiCars;

    private GameState state;
    private float startTime;
    private List<CarResult> results = new List<CarResult>();

    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else 
            Destroy(gameObject);
        
        state = GameState.Ready;
        
        InitUI();
        InitCars();
        RegisterEvents();
    }

    private void OnDestroy()
    {
        UnregisterEvents();
    }

    private void RegisterEvents()
    {
        uiManager.OnClickButtonStart += OnClickButtonStart;
    }

    private void InitUI()
    {
        uiManager.SetButtonStartEnable(true);
    }

    private void InitCars()
    {
        // Disable AI cars until race start
        foreach (var car in aiCars)
            car.enabled = false;
    }
    
    private void UnregisterEvents()
    {
        uiManager.OnClickButtonStart -= OnClickButtonStart;
    }

    private void OnClickButtonStart()
    {
        uiManager.SetButtonStartEnable(false);
        StartCoroutine(CountdownRoutine());
    }

    IEnumerator CountdownRoutine()
    {
        // Show and animate countdown
        uiManager.ShowCountdown(countdownTime);
        
        for (int i = (int)countdownTime; i >= 0; i--)
        {
            SoundManager.Instance.PlaySFX(i == 0 ? SoundEffect.Go : SoundEffect.Countdown);
            uiManager.UpdateCountdown(i);
            yield return new WaitForSeconds(1f);
        }

        // Start race
        uiManager.HideCountdown();
        state = GameState.Racing;
        startTime = Time.time;

        // Enable AI cars
        foreach (var car in aiCars)
            car.enabled = true;
        
        myCar.Activate();
    }

    // Call this from a finish-line trigger or car script
    public void CarFinished(Transform car)
    {
        if (state != GameState.Racing) return;

        float duration = Time.time - startTime;
        results.Add(new CarResult { carName = car.name, duration = duration });

        // Race ends when first car finishes
        if (results.Count == 1)
        {
            state = GameState.Finished;
            uiManager.ShowResults(results);
        }
    }

    // Data structure for results
    public class CarResult
    {
        public string carName;
        public float duration;
    }
}
