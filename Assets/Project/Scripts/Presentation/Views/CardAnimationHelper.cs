using System;
using DG.Tweening;
using UnityEngine;

namespace CardFramework.Presentation.Views {
    public static class CardAnimationHelper {
        public static void MoveCardTo(GameObject cardInstance, Vector3 targetPosition, Quaternion targetRotation, float duration = 0.25f, Ease ease = Ease.OutCubic, Action onComplete = null) {
            if (cardInstance == null) {
                onComplete?.Invoke();
                return;
            }

            cardInstance.transform.DOKill();
            Sequence moveSequence = DOTween.Sequence();
            moveSequence.Join(cardInstance.transform.DOMove(targetPosition, duration).SetEase(ease));
            moveSequence.Join(cardInstance.transform.DORotateQuaternion(targetRotation, duration).SetEase(ease));
            moveSequence.OnComplete(() => onComplete?.Invoke());
        }

        public static void FlipCardTo(GameObject cardInstance, Quaternion targetRotation, float duration = 0.25f, Ease ease = Ease.InOutSine, Action onComplete = null) {
            if (cardInstance == null) {
                onComplete?.Invoke();
                return;
            }

            cardInstance.transform.DOKill();
            Sequence flipSequence = DOTween.Sequence();
            flipSequence.Join(cardInstance.transform.DORotateQuaternion(targetRotation, duration).SetEase(ease));
            flipSequence.OnComplete(() => onComplete?.Invoke());
        }

        public static void MoveCardTo(GameObject cardInstance, Transform targetTransform, float duration = 0.25f, Ease ease = Ease.OutCubic, Action onComplete = null) {
            if (targetTransform == null) {
                onComplete?.Invoke();
                return;
            }

            MoveCardTo(cardInstance, targetTransform.position, targetTransform.rotation, duration, ease, onComplete);
        }
    }
}
