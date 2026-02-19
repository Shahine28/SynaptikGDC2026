using UnityEngine;

public class WorldEntity : MonoBehaviour
{
    [SerializeField] private WorldEntityID _entityID;
    public WorldEntityID EntityID => _entityID;
}
