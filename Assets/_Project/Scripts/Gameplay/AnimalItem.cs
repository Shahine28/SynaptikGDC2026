using UnityEngine;
// using FMODUnity;

public sealed class AnimalItem : MonoBehaviour, IInteraction
{
    [SerializeField]
    private GameObject itemToSpawnOnDeath;

    // [Space(7)]
    // [SerializeField] private EventReference _soundReaction;
    // [SerializeField] private EventReference _soundDeath;

    public void Interact(SynaptikInput action, HoldableItem item = null, PlayerInteraction playerInteraction = null)
    {
        if (action.actionType == ActionType.Action && action.emotionType == EmotionType.Aggressive)
        {
            // FMODUnity.RuntimeManager.PlayOneShot(_soundDeath, transform.position);
            
            Die();
        }
        else
        {
            // FMODUnity.RuntimeManager.PlayOneShot(_soundReaction, transform.position);
        }
    }

    private void Die()
    {
        if (itemToSpawnOnDeath)
        {
            Instantiate(itemToSpawnOnDeath, transform.position, Quaternion.identity);
        }

        Destroy(gameObject);
    }
}
