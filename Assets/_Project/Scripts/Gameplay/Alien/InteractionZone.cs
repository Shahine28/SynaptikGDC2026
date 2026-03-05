using System;
using NaughtyAttributes;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(SphereCollider))]
public class InteractionZone : MonoBehaviour
{
    [Header("Configuration")]
    [SerializeField] private SphereCollider _bounds;
    public  SphereCollider Bounds => _bounds;
    [SerializeField, Required] private WorldEntity _targetToDetect;
    public WorldEntity TargetToDetect => _targetToDetect;
    [SerializeField, ReadOnly] private bool _isTargetInRange;
    public bool IsTargetInRange => _isTargetInRange || IsTargetInZone();    
    
    [Header("Settings")]
    public UnityEvent OnPlayerEnter;
    public UnityEvent OnPlayerExit;
    
    
    [Header("Debug")]
    [SerializeField] private Color _gizmoColor = new(0, 1, 0, 1f);
    
    private void Reset()
    {
        _bounds = GetComponent<SphereCollider>();
        _bounds.isTrigger = true;
    }
    
    private void Awake()
    {
        if (_bounds == null)
        {
            _bounds = GetComponent<SphereCollider>();
        }
        if (_bounds && !_bounds.isTrigger) _bounds.isTrigger = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.TryGetComponent(out WorldEntity entity) || entity != _targetToDetect) return;
        OnPlayerEnter?.Invoke();
        _isTargetInRange = true;
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.TryGetComponent(out WorldEntity entity) || entity != _targetToDetect) return;
        OnPlayerExit?.Invoke();
        _isTargetInRange = false;
    }
    
    public void SetRadius(float radius)
    {
        _bounds.radius = radius;
    }
    
    public bool IsTargetInZone()
    {
        if (_targetToDetect == null) return false;
        
        Vector3 targetPos = _targetToDetect.transform.position;
        
        float distance = Vector3.Distance(_bounds.bounds.center, targetPos);
        
        float maxScale = Mathf.Max(Mathf.Abs(transform.lossyScale.x), Mathf.Abs(transform.lossyScale.y), Mathf.Abs(transform.lossyScale.z));

        float worldRadius = _bounds.radius * maxScale;
        
        return distance <= worldRadius;
    }
    
    private void OnDrawGizmos()
    {
        if (_bounds == null) return;
    
        Gizmos.color = _gizmoColor;


        Vector3 globalScale = transform.lossyScale;
        
        float maxScale = Mathf.Max(Mathf.Abs(globalScale.x), Mathf.Abs(globalScale.y), Mathf.Abs(globalScale.z));

        float worldRadius = _bounds.radius * maxScale;


        Gizmos.DrawWireSphere(_bounds.bounds.center, worldRadius);
    }
}
