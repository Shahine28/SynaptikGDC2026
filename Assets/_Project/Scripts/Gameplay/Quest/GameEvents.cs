using System;

public static class GameEvents
{
    public static event Action<WorldEntityID, EmotionType> OnEmotionChanged;
    public static void TriggerEmotionChange(WorldEntityID entity, EmotionType emotion) => OnEmotionChanged?.Invoke(entity, emotion);
    
    public static event Action<WorldEntityID, SynaptikInput> OnSynaptikInputChanged;
    public static void TriggerSynaptikInputChange(WorldEntityID entity, SynaptikInput synaptikInput) => OnSynaptikInputChanged?.Invoke(entity, synaptikInput);
    
    public static event Action<WorldEntityID, ItemID, bool> OnInventoryChanged;
    public static void TriggerInventoryChange(WorldEntityID owner, ItemID item, bool added) => OnInventoryChanged?.Invoke(owner, item, added);
    
    public static event Action<WorldEntityID, DestroyableItemID> OnDestroyableItemDestroyed;
    public static void TriggerItemDestroyed(WorldEntityID owner, DestroyableItemID item) => OnDestroyableItemDestroyed?.Invoke(owner, item);
    
    public static event Action<WorldEntityID, ZoneID> OnZoneEntered; 
    public static void TriggerZoneEntered(WorldEntityID entity, ZoneID zoneId) => OnZoneEntered?.Invoke(entity, zoneId);
    
    public static event Action<WorldEntityID, ZoneID> OnZoneExited; 
    public static void TriggerZoneExited(WorldEntityID entity, ZoneID zoneId) => OnZoneExited?.Invoke(entity, zoneId);
    
    public static event Action<WorldEntityID, string> OnAnimationAction;
    public static void TriggerAnimationAction(WorldEntityID entity, string actionID) => OnAnimationAction?.Invoke(entity, actionID);
}