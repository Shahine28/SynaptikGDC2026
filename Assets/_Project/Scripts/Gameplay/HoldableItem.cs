using System;
using System.Collections;
using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(Rigidbody))]
public sealed class HoldableItem : MonoBehaviour
{
    [SerializeField] public ItemID itemID;
    
    [Header("Respawn")]
    [SerializeField] private float respawnDelay = 5f;

    [SerializeField] private float despawnTime = 0.5f;

    [SerializeField] private AnimationCurve despawnAnim = AnimationCurve.Linear(0, 0, 1, 1);

    [SerializeField] private bool respawnAtDrop = false;

    [SerializeField] private GameObject despawnVfxPrefab;

    private Rigidbody rigidbodyComponent;
    private Collider[] colliders = Array.Empty<Collider>();
    private Transform originalParent;
    private Vector3 spawnLocation;
    private Quaternion spawnRotation;
    private Vector3 spawnScale;
    private Coroutine respawnCoroutine;
    private float currentDelay;
    [SerializeField] private bool canTake = true;

    public bool IsHeld { get; private set; }

    public bool CanBePicked => canTake && !IsHeld;

    private void Awake()
    {
        rigidbodyComponent = GetComponent<Rigidbody>();
        colliders = GetComponentsInChildren<Collider>(true);

        spawnLocation = transform.position;
        spawnRotation = transform.rotation;
        spawnScale = transform.localScale;
        originalParent = transform.parent;
    }
    

    public bool TryPick(Transform handSocket)
    {
        if (IsHeld || !canTake)
        {
            Debug.LogWarning($"{gameObject.name} Ramassage invalide pour '{name}' (IsHeld={IsHeld}, CanTake={canTake}).");
            return false;
        }

        if (respawnCoroutine != null)
        {
            StopCoroutine(respawnCoroutine);
        }

        IsHeld = true;
        
        rigidbodyComponent.linearVelocity = Vector3.zero;
        rigidbodyComponent.angularVelocity = Vector3.zero;
        rigidbodyComponent.isKinematic = true;
        rigidbodyComponent.useGravity = false;

        foreach (var collider in colliders) collider.enabled = false;
        
        transform.SetParent(handSocket);
        
        transform.localPosition = Vector3.zero;
        transform.localRotation = Quaternion.identity;
        
        // Debug.Log($"{LogPrefix} '{name}' ramassé par '{handSocket.name}'.");

        return true;
    }
    

    public bool TryDrop()
    {
        if (!IsHeld)
        {
            Debug.LogWarning($"{gameObject.name} Tentative de drop alors que '{name}' n'est pas tenu.");
            return false;
        }
        
        transform.position += transform.forward * 0.8f; // pour pas se faire pousser par l'objet qu'on drop 
        
        transform.SetParent(originalParent);
        
        rigidbodyComponent.linearVelocity = Vector3.zero;
        rigidbodyComponent.angularVelocity = Vector3.zero;
        rigidbodyComponent.isKinematic = false;
        rigidbodyComponent.useGravity = true;
        
        foreach (var collider in colliders) collider.enabled = true;

        IsHeld = false;
        // Debug.Log($"{LogPrefix} '{name}' lâché.");
        if (respawnAtDrop) respawnCoroutine = StartCoroutine(Respawn());
        return true;
    }

    private IEnumerator Respawn(float durationOverride = -1f)
    {
        currentDelay = durationOverride < 0f ? respawnDelay : durationOverride;
        Debug.Log($"{gameObject.name} Respawn de '{name}' démarré ({currentDelay:F1}s).");
        yield return new WaitForSeconds(currentDelay);

        canTake = false;

        currentDelay = despawnTime;
        var startScale = transform.localScale;

        while (currentDelay > 0f)
        {
            currentDelay -= Time.fixedDeltaTime;
            var lerpFactor = despawnAnim.Evaluate(currentDelay / despawnTime);
            transform.localScale = Vector3.Lerp(Vector3.zero, startScale, lerpFactor);
            yield return new WaitForFixedUpdate();
        }

        if (despawnVfxPrefab != null)
        {
            Instantiate(despawnVfxPrefab, transform.position, Quaternion.identity);
        }

        SetAtSpawn();
    }

    public void SetAtSpawn()
    {
        rigidbodyComponent.linearVelocity = Vector3.zero;
        rigidbodyComponent.angularVelocity = Vector3.zero;

        transform.SetPositionAndRotation(spawnLocation, spawnRotation);
        transform.localScale = spawnScale;
        
        transform.SetParent(originalParent, true);
        foreach (var collider in colliders)
        {
            collider.enabled = true;
        }
        rigidbodyComponent.isKinematic = false;
        rigidbodyComponent.useGravity = true;
        

        IsHeld = false;
        canTake = true;
        Debug.Log($"{gameObject.name} '{name}' réinitialisé et disponible.");
    }
}
