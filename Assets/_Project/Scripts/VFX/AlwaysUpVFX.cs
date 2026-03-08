using UnityEngine;

public class AlwaysUpVFX : MonoBehaviour
{
    [SerializeField] private Vector3 rotation = new Vector3(0, 90, 0);
    
    // Update is called once per frame
    void Update()
    {
        transform.rotation = Quaternion.Euler(rotation);
    }
}
