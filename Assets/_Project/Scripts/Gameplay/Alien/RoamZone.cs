using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(SphereCollider))]
public class RoamZone : MonoBehaviour
{
    [Header("Configuration")]
    [SerializeField] private SphereCollider _bounds;

    [Header("Visuals")]
    [SerializeField] private Color _zoneColor = new(0, 0, 1, 1f);

    private float _finalRadius;
    private void Reset()
    {
        _bounds = GetComponent<SphereCollider>();
        _bounds.isTrigger = true;
    }
    
    public Vector3 GetRandomPointInZone()
    {
        int iterator = 0;
        while (iterator < 15) // On essaye 15 fois max de trouver un point à visiter
        {
            Vector2 randomCircle = Random.insideUnitCircle * _bounds.radius;
            Vector3 randomPoint = _bounds.bounds.center + new Vector3(randomCircle.x, 0, randomCircle.y);
            if (NavMesh.SamplePosition(randomPoint, out NavMeshHit hit, 2.0f, NavMesh.AllAreas))
            {
                return hit.position;
            }
            iterator++;
        }
        return _bounds.bounds.center;
    }
    
    public bool IsPointInZone(Vector3 point)
    {
        float distance = Vector2.Distance(new Vector2(point.x, point.z), 
            new Vector2(_bounds.bounds.center.x, _bounds.bounds.center.z)
        );

        return distance <= _bounds.radius;
    }

    
    public Vector3 ClampPositionToZone(Vector3 targetPosition)
    {
        Vector3 direction = targetPosition - _bounds.bounds.center;
        direction.y = 0;


        if (direction.magnitude <= _finalRadius) return targetPosition;
        
        Vector3 clampedPosition = _bounds.bounds.center + direction.normalized * _finalRadius;

        clampedPosition.y = targetPosition.y; 
    
        
        return clampedPosition;
    }
    
    public void SetRadius(float radius)
    {
        _bounds.radius = radius;
    }

    private void OnDrawGizmos()
    {
        if (_bounds == null) return;
    
        Gizmos.color = _zoneColor;
        
        Vector3 globalScale = transform.lossyScale;
        
        float maxScale = Mathf.Max(Mathf.Abs(globalScale.x), Mathf.Abs(globalScale.y), Mathf.Abs(globalScale.z));

        _finalRadius = _bounds.radius * maxScale;


        Gizmos.DrawWireSphere(_bounds.bounds.center, _finalRadius);
    }
}
