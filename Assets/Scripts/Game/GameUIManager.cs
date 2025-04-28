using System;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using TMPro;

public class GameUIManager : MonoBehaviour
{
    public event Action OnClickButtonStart;

    [Header("Joystick")] 
    public GameObject joystickUI;

    [Header("Countdown UI")]
    public GameObject countdownBackground;
    public CanvasGroup countdownGroup;
    public TMP_Text countdownText;

    [Header("Results UI")]
    public GameObject resultsPanel;
    public TMP_Text[] rankingTexts;

    public Button buttonStart;

    public float countdownMoveDuration = 0.2f;
    public float countdownMoveSlowDuration = 0.5f;
    public Ease countdownEase = Ease.InOutQuad;

    private RectTransform countdownRect;
    private float offscreenX;

    void Awake()
    {
        joystickUI.SetActive(false);
        resultsPanel.SetActive(false);
        countdownGroup.alpha = 0f;

        countdownRect = countdownGroup.GetComponent<RectTransform>();
        var parentRect = countdownRect.parent.GetComponent<RectTransform>();
        offscreenX = parentRect.rect.width / 2 + countdownRect.rect.width;
        
        buttonStart.onClick.AddListener(() =>
        {
            OnClickButtonStart?.Invoke();
        });
    }

    public void SetJoystickEnable(bool isEnabled)
    {
        joystickUI.SetActive(isEnabled);
    }

    public void SetButtonStartEnable(bool isEnabled)
    {
        buttonStart.gameObject.SetActive(isEnabled);
    }
    
    public void ShowCountdown(float time)
    {
        countdownGroup.gameObject.SetActive(true);
        countdownText.text = time.ToString(CultureInfo.CurrentCulture);
        countdownGroup.alpha = 1f;
        countdownRect.anchoredPosition = new Vector2(0, countdownRect.anchoredPosition.y);
        // TODO: Play countdown sound here
    }
    
    public void UpdateCountdown(int time)
    {
        countdownText.text = time == 0 ? "GO!" : time.ToString();
        if (time == 0) countdownBackground.SetActive(false);
        
        // Start offscreen right
        countdownRect.anchoredPosition = new Vector2(offscreenX, countdownRect.anchoredPosition.y);
        
        var sequence = DOTween.Sequence();
        sequence.Append(countdownRect.DOAnchorPosX(100f, countdownMoveDuration).SetEase(Ease.OutQuad));
        sequence.Append(countdownRect.DOAnchorPosX(0f, countdownMoveSlowDuration));
        sequence.Append(countdownRect.DOAnchorPosX(-offscreenX, countdownMoveDuration).SetEase(Ease.OutQuad));
        sequence.Play();
    }
    
    public void HideCountdown()
    {
        countdownGroup.DOFade(0f, 0.5f);
        countdownBackground.SetActive(false);
    }
    
    public void ShowResults(List<GameManager.CarResult> results)
    {
        resultsPanel.SetActive(true);
        for (int i = 0; i < results.Count && i < rankingTexts.Length; i++)
        {
            var r = results[i];
            rankingTexts[i].text = $"{i + 1}. {r.carName} - {(r.duration > 0f ? r.duration.ToString("F2") : "--:--:--")}s";
        }
    }
}
