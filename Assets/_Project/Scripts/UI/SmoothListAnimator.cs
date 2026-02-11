using System.Collections;
using System.Collections.Generic;
using NaughtyAttributes;
using UnityEngine;
using UnityEngine.UI;

public class SmoothListAnimator : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private float _animationDuration = 0.5f;
    [SerializeField] private AnimationCurve _curve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    [SerializeField, Required] private VerticalLayoutGroup _layoutGroup;
    [SerializeField, Required] private RectTransform _contentRect;
    
    public void MoveQuestToBottom(RectTransform questItem)
    {
        StartCoroutine(AnimateReorder(questItem));
    }

    private IEnumerator AnimateReorder(RectTransform targetItem)
    {
        Dictionary<RectTransform, Vector2> startPositions = new Dictionary<RectTransform, Vector2>();
        foreach (RectTransform child in _contentRect.transform)
        {
            startPositions[child] = child.anchoredPosition;
        }

        targetItem.SetAsLastSibling();


        Canvas.ForceUpdateCanvases();
        _layoutGroup.enabled = false; 

        Dictionary<RectTransform, Vector2> endPositions = new Dictionary<RectTransform, Vector2>();
        
        LayoutRebuilder.ForceRebuildLayoutImmediate(_contentRect);

        foreach (RectTransform child in  _contentRect.transform)
        {
            endPositions[child] = child.anchoredPosition;
            
            if (startPositions.ContainsKey(child))
            {
                child.anchoredPosition = startPositions[child];
            }
        }
        
        float timer = 0f;
        while (timer < _animationDuration)
        {
            timer += Time.deltaTime;
            float percent = _curve.Evaluate(timer / _animationDuration);

            foreach (RectTransform child in  _contentRect.transform)
            {
                if (endPositions.ContainsKey(child))
                {
                    child.anchoredPosition = Vector2.Lerp(startPositions[child], endPositions[child], percent);
                }
            }
            yield return null;
        }
        
        foreach (RectTransform child in  _contentRect.transform)
        {
            if (endPositions.ContainsKey(child))
                child.anchoredPosition = endPositions[child];
        }

        _layoutGroup.enabled = true;
    }
}