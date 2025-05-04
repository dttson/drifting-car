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

    // private void Update()
    // {
    //     if (Input.GetKeyUp(KeyCode.L))
    //     {
    //         foreach (BaseCarController carController in cars)
    //         {
    //             CarFinished(carController);
    //         }
    //     }
    //     
    //     if (Input.GetKeyUp(KeyCode.M))
    //         uiManager.ShowResults(results);
    // }

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
        if (results.Count >= cars.Length) 
            return;
        
        if (results.Exists(r => r.carName == car.CarName))
            return;

        var duration = (int)(Time.time - startTime);
        int rank = results.Count + 1;
        results.Add(new CarResult { rank = rank, carName = car.CarName, duration = duration, isMyCar = car.IsMyCar});
        
        if (car.IsMyCar)
        {
            state = GameState.Finished;
            
            car.DeActivate();

            // If user finish, then auto fill all data of other AI cars
            // TODO: Should have function to calculate result of other AIs
            for (int i = rank; i < cars.Length; i++)
            {
                var otherCar = cars[i];
                results.Add(new CarResult { rank = i + 1, carName = otherCar.CarName, duration = 0, isMyCar = false});
            }
            
            uiManager.ShowResults(results);
        }
    }

    // Data structure for results
    public class CarResult
    {
        public int rank;
        public string carName;
        public int duration;
        public bool isMyCar;
    }
}
