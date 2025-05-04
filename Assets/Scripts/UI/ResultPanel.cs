using System;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

namespace UI
{
    public class ResultPanel : MonoBehaviour
    {
        [Header("Title Animation")] 
        public RectTransform titleRect;
        public float titleAnimDuration = 0.5f;
        public Ease titleEase = Ease.OutBack;

        [Header("Records")] 
        public RankUI[] recordUIs; // Assign RankUI components in order

        [Header("Movement Settings")] 
        public float fastMoveDuration = 0.3f; // Quick move to near center
        public float slowMoveDuration = 1f; // Slow move to exact center
        public float betweenRecordDelay = 1f; // Delay between items
        public float floatAmplitude = 20f; // Amplitude for floating oscillation
        public float floatDuration = 2f; // Duration for one float cycle

        private Sequence sequence;

        void OnEnable()
        {
            AnimateTitle();
            AnimateRecords();
        }

        void OnDisable()
        {
            if (sequence != null)
                DOTween.Kill(sequence);
        }

        void AnimateTitle()
        {
            if (titleRect == null) 
                return;
            
            titleRect.localScale = new Vector3(0f, 1f, 1f);
            titleRect.DOScaleX(1f, titleAnimDuration).SetEase(titleEase);
        }

        void AnimateRecords()
        {
            for (int i = 0; i < recordUIs.Length; i++)
            {
                var rankUI = recordUIs[i];
                var rect = rankUI.GetComponent<RectTransform>();
                if (rect == null) continue;

                Vector2 originalPos = rect.anchoredPosition;
                float centerX = originalPos.x;
                float startX = Screen.width + 100;
                float nearX = Mathf.Lerp(startX, centerX, 0.8f);

                rect.anchoredPosition = new Vector2(startX, originalPos.y);

                float delay = betweenRecordDelay * i;

                if (sequence != null)
                    DOTween.Kill(sequence);
                
                sequence = DOTween.Sequence();
                sequence.AppendInterval(delay)
                    // .Append(rect.DOAnchorPosX(nearX, fastMoveDuration).SetEase(Ease.OutQuad))
                    .Append(rect.DOAnchorPosX(centerX, fastMoveDuration).SetEase(Ease.OutQuad))
                    .Append(rect.DOAnchorPosX(centerX + floatAmplitude, floatDuration)
                        .SetEase(Ease.InOutSine)
                        .SetLoops(-1, LoopType.Yoyo));
            }
        }

        /// <summary>
        /// Call this to populate the UI with race results in order.
        /// </summary>
        public void PopulateResults(List<GameManager.CarResult> results)
        {
            int count = Mathf.Min(results.Count, recordUIs.Length);
            for (int i = 0; i < count; i++)
            {
                recordUIs[i].UpdateData(results[i]);
                recordUIs[i].gameObject.SetActive(true);
            }

            // Hide extras
            for (int i = count; i < recordUIs.Length; i++)
                recordUIs[i].gameObject.SetActive(false);
        }
    }
}