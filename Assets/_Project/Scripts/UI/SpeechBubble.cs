using System.Collections;
using System.Linq;
using AYellowpaper.SerializedCollections;
using NaughtyAttributes;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SpeechBubble : MonoBehaviour
{
    [Header("UI Binding")]
    [SerializeField] private Image _speechBubbleImage;
    [SerializeField] private TextMeshProUGUI _speechBubbleText;
    
    [Header("Speech Bubble Sprites")]
    [SerializeField, SerializedDictionary("Synaptik Input", "BackgroundImage")] 
    private SerializedDictionary<SynaptikInput, Sprite> _spriteFromSynaptikInput;


    [Header("Speech Bubble Settings")] 
    [Tooltip("Temps d'affichage par caractère (hors espaces)")]
    [SerializeField] private float _secondPerCharacter = 1f; 
    
    [Tooltip("Temps minimum d'affichage (pour les textes courts)")]
    [SerializeField] private float _minDisplayTime = 2f;

    [Header("Dynamic Scaling")]
    [MinMaxSlider(0.1f, 5f)]
    [SerializeField] private Vector2 _scaleRange = new(0.5f, 1.5f);
    [MinMaxSlider(1f, 50f)] 
    [SerializeField] private Vector2 _distanceRange = new(5f, 20f);
    
    [Header("Dynamic Y position")]
    [MinMaxSlider(0.1f, 5f)]
    [SerializeField] private Vector2 _positionYRange = new(0.5f, 2);
    private Camera _mainCamera;
    
    private void Awake()
    {
        _mainCamera = Camera.main;
    }
    
    private void LateUpdate()
    {
        if (_mainCamera == null) _mainCamera = Camera.main;
        if (_mainCamera == null) return;
        transform.rotation = _mainCamera.transform.rotation;
        UpdateScaleAndYPositionBasedOnDistance();
    }
    
    private void UpdateScaleAndYPositionBasedOnDistance()
    {
        float distance = Vector3.Distance(transform.position, _mainCamera.transform.position);
        
        float t = Mathf.InverseLerp(_distanceRange.x, _distanceRange.y, distance);
        float targetScale = Mathf.Lerp(_scaleRange.x, _scaleRange.y, t);

        transform.localScale = Vector3.one * targetScale;

        float targetYPosition = Mathf.Lerp(_positionYRange.x, _positionYRange.y, t);
        transform.localPosition = new Vector3(transform.localPosition.x, targetYPosition, transform.localPosition.z);
    }

    public void SetSpeechBubble(SynaptikInput input, string text)
    {
        if (_spriteFromSynaptikInput.TryGetValue(input, out var sprite))
        {
            _speechBubbleImage.sprite = sprite;
            _speechBubbleText.text = text;
            
            int charCount = text.Count(c => !char.IsWhiteSpace(c));
            
            float duration = Mathf.Max(_minDisplayTime, charCount * _secondPerCharacter);
            
            StartCoroutine(WaitBeforeDestroySpeechBubble(duration));
            return;
        }
        
        Debug.LogWarning($"[SpeechBubble] No Sprite found for input : {input}");
        Destroy(gameObject);
    }

    private IEnumerator WaitBeforeDestroySpeechBubble(float secondsBeforeDestroy)
    {
        yield return new WaitForSeconds(secondsBeforeDestroy);
        Destroy(gameObject);
    }
}
