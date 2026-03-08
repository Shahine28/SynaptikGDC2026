using System.Collections;
using System.Collections.Generic;
using NaughtyAttributes;
using UnityEngine;
using UnityEngine.UI;

public sealed class CableFeedback : MonoBehaviour
{
    [Header("Output References")]
    [SerializeField] private Image outputLeft;
    [SerializeField] private Image outputRight;

    [Header("VFX Parameters")]
    [SerializeField] private GameObject vfxPrefab;
    [Space(5)]
    [SerializeField] private int numberOfElementsByVFX = 6;
    [SerializeField] private Vector3 _offset = new Vector3(0f, 0f, 0f);
    [SerializeField] private bool _shouldOrientToDirection = true;
    [SerializeField] private float _spawnExplosionForce = 30f;
    [SerializeField] private float _gravityForce = 10f;
    [SerializeField] private float _lifespan = 1.5f;
    [SerializeField] private SimpleAudioEventBinder _audioSource;

    [Header("Action Colors")]
    [SerializeField] private Color defaultActionColor = Color.white;
    
    [Header("Emotion Colors")]
    [SerializeField] private Color defaultEmotionColor = Color.white;
    
    [Header("Binding")]
    [SerializeField, Required] private PlayerInputSystem _playerInputSystem;
    [SerializeField, Required] private AlienColorFromEmotionType alienColorFromEmotion;

    void OnEnable()
    {
        if (!_playerInputSystem) return;
        _playerInputSystem.OnSynaptikInput += HandleSynaptikInput;
    }
    
    void OnDisable()
    {
        if (!_playerInputSystem) return;
        _playerInputSystem.OnSynaptikInput -= HandleSynaptikInput;
    }

    void Start()
    {
        outputLeft.color = defaultActionColor;
        outputRight.color = defaultEmotionColor;
    }

    void FixedUpdate()
    {
        // if (outputLeft && outputLeft.color != defaultActionColor)
        // {
        //     StartCoroutine(SpawnVFX(outputRight.transform, outputLeft.color));
        // }
        // if (outputRight && outputRight.color != defaultEmotionColor)
        // {
        //     StartCoroutine(SpawnVFX(outputRight.transform, outputRight.color));
        // }
    }
    
    private void HandleSynaptikInput(SynaptikInput synaptikInput)
    {
        if (outputLeft)
        {
            if (synaptikInput.actionType != ActionType.None && outputLeft.color == defaultActionColor)
            {
                _audioSource.PlaySoundByIndex(Random.Range(0, 3));
                StartCoroutine(SpawnVFX(outputLeft.transform, alienColorFromEmotion.AlienColorFromAction[synaptikInput.actionType]));
            }
            
            outputLeft.color = synaptikInput.actionType == ActionType.None 
                ? defaultActionColor : alienColorFromEmotion.AlienColorFromAction[synaptikInput.actionType];
        }

        if (outputRight)
        {
            if (synaptikInput.emotionType != EmotionType.None && outputRight.color == defaultEmotionColor)
            {
                _audioSource.PlaySoundByIndex(Random.Range(0, 3));
                StartCoroutine(SpawnVFX(outputRight.transform, alienColorFromEmotion.AlienColorFromEmotion[synaptikInput.emotionType]));
            }
            
            outputRight.color = synaptikInput.emotionType == EmotionType.None
                ? defaultEmotionColor : alienColorFromEmotion.AlienColorFromEmotion[synaptikInput.emotionType];
        }
    }                                                                                                   

    private IEnumerator SpawnVFX(Transform origin, Color color, int nbElements = -1)
    {
        List<GameObject> listOfElements = new List<GameObject>();
        List<Vector3> listOfForce = new List<Vector3>();

        if (nbElements <= 0)
            nbElements = numberOfElementsByVFX;
        
        for (int i = 0; i < nbElements; i++)
        {
            GameObject element = Instantiate(vfxPrefab, origin);
            if (element.TryGetComponent<Image>(out Image image))
                image.color = color;
            else
                Debug.LogError("No image in Image component feedback");

            
            element.transform.localPosition = _offset;
            Vector3 force = new Vector3(Random.Range(-1f, 1f), Random.Range(-1f, 0f), 0f).normalized * Random.Range(0f, _spawnExplosionForce);
            
            listOfElements.Add(element);
            listOfForce.Add(force);
        }

        float timer = _lifespan;
        while (timer > 0)
        {
            float elementSize = timer / _lifespan;
            for(int i = 0; i < listOfElements.Count; i++)
            {
                listOfForce[i] += Vector3.down * _gravityForce * Time.fixedDeltaTime;
                
                listOfElements[i].transform.position += listOfForce[i] * Time.fixedDeltaTime;
                listOfElements[i].transform.localScale = Vector3.one * elementSize;
                
                if (_shouldOrientToDirection)
                    listOfElements[i].transform.rotation = Quaternion.LookRotation(Vector3.forward, listOfForce[i].normalized);
            }
            
            yield return new WaitForFixedUpdate();
            timer -= Time.fixedDeltaTime;
        }

        foreach (var element in listOfElements)
        {
            Destroy(element);
        }

        yield break;
    }
}
