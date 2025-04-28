using System;
using System.Collections;
using System.Collections.Generic;
using DefaultNamespace;
using UnityEngine;

public enum GameState { Ready, Racing, Finished }

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Race Settings")]
    public float countdownTime = 3f;
    public GameUIManager uiManager;
    public BaseCarController[] cars;

    [Header("Other")] 
    public GameObject finishTriggerObject;

    private GameState state;
    private float startTime;
    private List<CarResult> results = new List<CarResult>();

    private int carHitCount = 0;

    private void Awake()
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
        uiManager.SetJoystickEnable(false);
    }

    private void InitCars()
    {
        // Disable cars until race start
        foreach (var car in cars)
            car.DeActivate();
    }
    
    private void UnregisterEvents()
    {
        uiManager.OnClickButtonStart -= OnClickButtonStart;
    }

    private void OnClickButtonStart()
    {
        if (state != GameState.Ready)
            return;
        
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

        // Enable cars
        foreach (var car in cars)
            car.Activate(CarFinished);
        
        // Enable joystick
        uiManager.SetJoystickEnable(true);
        
        //TODO: Need using more reliable way to check
        finishTriggerObject.SetActive(false);
        yield return new WaitForSeconds(20f);
        finishTriggerObject.SetActive(true);
    }

    // Call this from a finish-line trigger or car script
    public void CarFinished(ICarController car)
    {
        if (state != GameState.Racing) 
            return;
        
        float duration = Time.time - startTime;
        results.Add(new CarResult { carName = car.IsMyCar ? "YOU" : "AI Car", duration = duration });

        state = GameState.Finished;
        
        if (car.IsMyCar)
        {
            car.DeActivate();
            
            results.Add(new CarResult { carName = "AI Car", duration = duration });
            
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
