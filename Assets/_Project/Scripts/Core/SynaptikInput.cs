using System;

[Serializable]
public enum EmotionType 
{   
    None,
    Friendly,
    Aggressive,
    Fearful,
    Curious 
}
[Serializable]
public enum ActionType 
{ 
    None,
    Action,
    Word 
}

[Serializable]
public struct SynaptikInput
{
    public EmotionType emotionType;
    public ActionType actionType;
}
