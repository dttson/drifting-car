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

    [Header("Countdown UI")]
    public GameObject countdownBackground;
    public CanvasGroup countdownGroup;
    public TMP_Text countdownText;

    [Header("Results UI")]
    public GameObject resultsPanel;
    public TMP_Text[] rankingTexts;

    public Button buttonStart;

    // Duration for the logic to animate across the screen
    public float countdownMoveDuration = 0.2f;
    public float countdownMoveSlowDuration = 0.5f;
    public Ease countdownEase = Ease.InOutQuad;

    private RectTransform countdownRect;
    private float offscreenX;

    void Awake()
    {
        // Hide results panel and countdown initially
        resultsPanel.SetActive(false);
        countdownGroup.alpha = 0f;

        // Cache RectTransform and compute offscreen position based on parent width
        countdownRect = countdownGroup.GetComponent<RectTransform>();
        var parentRect = countdownRect.parent.GetComponent<RectTransform>();
        offscreenX = parentRect.rect.width / 2 + countdownRect.rect.width;
        
        buttonStart.onClick.AddListener(() =>
        {
            OnClickButtonStart?.Invoke();
        });
    }

    public void SetButtonStartEnable(bool isEnabled)
    {
        buttonStart.gameObject.SetActive(isEnabled);
    }

    /// <summary>
    /// Show the initial countdown number without movement
    /// </summary>
    public void ShowCountdown(float time)
    {
        countdownGroup.gameObject.SetActive(true);
        countdownText.text = time.ToString(CultureInfo.CurrentCulture);
        countdownGroup.alpha = 1f;
        countdownRect.anchoredPosition = new Vector2(0, countdownRect.anchoredPosition.y);
        // TODO: Play countdown sound here
    }

    /// <summary>
    /// Update the countdown each second by moving the text across the screen
    /// </summary>
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
        // TODO: Add tick sound here
    }

    /// <summary>
    /// Hide the countdown group with fade-out.
    /// </summary>
    public void HideCountdown()
    {
        countdownGroup.DOFade(0f, 0.5f);
        countdownBackground.SetActive(false);
    }

    /// <summary>
    /// Display race results with ranking and durations.
    /// </summary>
    public void ShowResults(List<GameManager.CarResult> results)
    {
        resultsPanel.SetActive(true);
        for (int i = 0; i < results.Count && i < rankingTexts.Length; i++)
        {
            var r = results[i];
            rankingTexts[i].text = $"{i + 1}. {r.carName} - {r.duration:F2}s";
        }
    }
}
