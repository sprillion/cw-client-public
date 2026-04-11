using System;
using DG.Tweening;
using Sirenix.OdinInspector;
using UnityEngine;

namespace ui
{
    public class LoadingPanel : MonoBehaviour
    {
        [SerializeField] private RectTransform _block1;
        [SerializeField] private RectTransform _block2;
        [SerializeField] private RectTransform _block3;

        private Sequence _sequence;

        private Vector2 _block1Pos;
        private Vector2 _block2Pos;
        private Vector2 _block3Pos;
        private void Awake()
        {
            _block1Pos = _block1.anchoredPosition;
            _block2Pos = _block2.anchoredPosition;
            _block3Pos = _block3.anchoredPosition;
        }

        [Button]
        public void Show()
        {
            LaunchLoading();
            gameObject.SetActive(true);
        }

        public void Hide()
        {
            _sequence?.Kill();
            gameObject.SetActive(false);
            
            _block1.anchoredPosition = _block1Pos;
            _block2.anchoredPosition = _block2Pos;
            _block3.anchoredPosition = _block3Pos;
        }

        private void OnDestroy()
        {
            _sequence?.Kill();
        }

        private void LaunchLoading()
        {
            _sequence?.Kill();
            _sequence = null;
            _sequence = DOTween.Sequence();

            _sequence.Append(_block1.DOAnchorPosX(30, 0.7f).SetEase(Ease.OutQuint));
            _sequence.Append(_block2.DOAnchorPosY(30, 0.7f).SetEase(Ease.OutQuint));
            _sequence.Append(_block3.DOAnchorPosX(-30, 0.7f).SetEase(Ease.OutQuint));
            
            _sequence.Append(_block1.DOAnchorPosY(-30, 0.7f).SetEase(Ease.OutQuint));
            _sequence.Append(_block2.DOAnchorPosX(30, 0.7f).SetEase(Ease.OutQuint));
            _sequence.Append(_block3.DOAnchorPosY(30, 0.7f).SetEase(Ease.OutQuint));
            
            _sequence.Append(_block1.DOAnchorPosX(-30, 0.7f).SetEase(Ease.OutQuint));
            _sequence.Append(_block2.DOAnchorPosY(-30, 0.7f).SetEase(Ease.OutQuint));
            _sequence.Append(_block3.DOAnchorPosX(30, 0.7f).SetEase(Ease.OutQuint));
            
            _sequence.Append(_block1.DOAnchorPosY(30, 0.7f).SetEase(Ease.OutQuint));
            _sequence.Append(_block2.DOAnchorPosX(-30, 0.7f).SetEase(Ease.OutQuint));
            _sequence.Append(_block3.DOAnchorPosY(-30, 0.7f).SetEase(Ease.OutQuint));

            _sequence.SetLoops(-1);
        }
    }
}