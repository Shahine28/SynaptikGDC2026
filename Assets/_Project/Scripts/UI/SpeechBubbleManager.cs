using NaughtyAttributes;
using UnityEngine;

public class SpeechBubbleManager : MonoBehaviour
{
    public static SpeechBubbleManager Instance { get; private set; }

    [Header("Settings")]
    [SerializeField, Required] private SpeechBubble _speechBubblePrefab;
    
    [SerializeField] private Vector3 _defaultOffset = new(0, 1f, 0); 
    
    
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }
    
    public void SpawnBubble(Transform target, SynaptikInput input, string text, Vector3? customOffset = null)
    {
        if (_speechBubblePrefab == null)
        {
            Debug.LogError("[SpeechBubbleManager] Prefab not assigned in SpeechBubbleManager !");
            return;
        }
        
        SpeechBubble existingBubble = target.GetComponentInChildren<SpeechBubble>();
        if (existingBubble != null)
        {
            Destroy(existingBubble.gameObject);
        }
        
        Vector3 offset = customOffset ?? _defaultOffset; // Si custom est null, prend default
        

        SpeechBubble newBubble = Instantiate(_speechBubblePrefab, target.position + offset, Quaternion.identity, target);
        
        newBubble.transform.localPosition = offset; 
        newBubble.transform.localScale = Vector3.one;
        
        newBubble.SetSpeechBubble(input, text);
    }
}