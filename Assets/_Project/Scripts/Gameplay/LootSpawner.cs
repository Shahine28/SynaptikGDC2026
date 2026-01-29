using System;
using System.Collections.Generic;
using NaughtyAttributes;
using UnityEngine;
using Random = UnityEngine.Random;

public class LootSpawner : MonoBehaviour
{
    [SerializeField, Range(0, 1)] private float _spawnChance = 0.5f;
    [SerializeField] private List<SpawnObject> _spawnObjects = new();
    [SerializeField, Tooltip("L'endroit où doit spawn l'objet"), Required] private Transform _spawnTransform;

    public void Spawn()
    {
        if (Random.value > _spawnChance)
        {
            return; 
        }
        
        if (_spawnObjects.Count == 0)
        {
            Debug.LogWarning($"{name}: La liste de loot est vide !");
            return;
        }

        GameObject prefabToSpawn = _spawnObjects.Count == 1 ? _spawnObjects[0].ObjectToSpawn : GetWeightedRandomObject();
        
        if (prefabToSpawn)
        {
            Instantiate(prefabToSpawn, _spawnTransform.position, _spawnTransform.rotation);
        }
    }

    private GameObject GetWeightedRandomObject()
    {
        float totalWeight = 0f;
        foreach (var spawnObj in _spawnObjects)
        {
            totalWeight += spawnObj.Weight;
        }
        
        if (totalWeight <= 0) return _spawnObjects[0].ObjectToSpawn;
        
        float randomPoint = Random.Range(0f, totalWeight);


        foreach (var spawnObj in _spawnObjects)
        {
            randomPoint -= spawnObj.Weight;
            
            if (randomPoint <= 0)
            {
                return spawnObj.ObjectToSpawn;
            }
        }
        
        return _spawnObjects[-1].ObjectToSpawn;
    }
    

}

[Serializable]
struct SpawnObject
{
    [SerializeField] private GameObject _objectToSpawn;
    public GameObject ObjectToSpawn => _objectToSpawn;
    
    [SerializeField, Range(0, 1)] private float _weight;
    public float Weight => _weight;
}
