using System;
using System.Collections.Generic;
using AYellowpaper.SerializedCollections;
using NaughtyAttributes;
using UnityEngine;

public class StateMachine : MonoBehaviour
{
    //Je mets certains champs et fonctions en protected pour les utiliser si je fais des variantes de ce state machine
    [SerializeField] private List<State> _allStates = new List<State>();
    [SerializeField, ReadOnly] private StateID _currentStateID;
     
    [SerializeField, SerializedDictionary("Player Emotion", "Alien Reaction"), Tooltip("Quel state adopte l'alien lorsque le joueur intéragit avec l'alien avec telle émotion")]
    private SerializedDictionary<SynaptikInput, StateID> _alienStateFromPlayerEmotion = new();
    
    private State _currentState;
    private Alien _alien;

    [Header("Puke Settings")] 
    [SerializeField] private int _maxEmotionChangesBeforePuke = 3;
    private int currentEmotionChangesBeforePuke;
    
    [SerializeField] private float _emotionChangeWindow = 10;
    private float currentEmotionChangeWindow;
    
    public void StateMachineUpdate(float deltaTime) // On appelle la fonction StateUpdate de l'état actuel à chaque tick pour simuler une fonction update
    {
        if (_currentState == null) return;
        if (currentEmotionChangesBeforePuke != 0)
        {
            currentEmotionChangeWindow += deltaTime;
            if (currentEmotionChangeWindow > _emotionChangeWindow)
            {
                currentEmotionChangesBeforePuke = 0;
                currentEmotionChangeWindow = 0;
            }
        }
       
        _currentState.StateUpdate(deltaTime);
    }

    public void Init(Alien alien)
    {
        _alien = alien;
        InitStates();
        ChangeState(_alien.DefaultStateID);
    }

    public Alien GetAlien() 
    {
        return _alien;
    }
    
    protected void InitStates() // On initialise les états en leur donnant une référence à la state Machine
    {
        if (_allStates.Count == 0)
        {
            Debug.LogWarning("No states initialized!");
            return;
        }
        foreach (State state in _allStates)
        {
            state.StateInit(this);
        }
    }

    public void SwitchToFriendly() => SwitchToState(StateID.Friendly);
    public void SwitchToAggressive() => SwitchToState(StateID.Aggressive);
    public void SwitchToFearful() => SwitchToState(StateID.Fearful);
    public void SwitchToCurious() => SwitchToState(StateID.Curious);


    private void SwitchToState(StateID NextStateID)
    {
        if (NextStateID != _currentStateID)
        {
            currentEmotionChangesBeforePuke++;
            if (currentEmotionChangesBeforePuke > _maxEmotionChangesBeforePuke)
            {
                currentEmotionChangesBeforePuke = 0;
                _alien.AlienAnimation.PlayPuke();
            }
            if (TryGetComponent(out WorldEntity worldEntity))
            {
                GameEvents.TriggerEmotionChange(worldEntity.EntityID, GetEmotionTypeFromStateId(NextStateID));
            }
            ChangeState(NextStateID);  
            _alien.UpdateAlien();
        }
    }
    
    public void UpdateState(SynaptikInput playerSynaptikInput)
    {
        if (_alienStateFromPlayerEmotion.ContainsKey(playerSynaptikInput) && _alienStateFromPlayerEmotion[playerSynaptikInput] != _currentStateID)
        {
            currentEmotionChangesBeforePuke++;
            if (currentEmotionChangesBeforePuke > _maxEmotionChangesBeforePuke)
            {
                currentEmotionChangesBeforePuke = 0;
                _alien.AlienAnimation.PlayPuke();
            }
            if (TryGetComponent(out WorldEntity worldEntity))
            {
                GameEvents.TriggerEmotionChange(worldEntity.EntityID, playerSynaptikInput.emotionType);
            }
            ChangeState(_alienStateFromPlayerEmotion[playerSynaptikInput]);   
        }
    }
    
    public void ChangeState(StateID NextStateID) // Change l'état actuel du StateMachine et appelle le StateExit de l'état précédent et le StateStart du nouvel état
    {
        if (_currentStateID == NextStateID)
        {
            Debug.LogWarning("Cannot change state while state is already active!");
            return;
        }
        
        State nextState = GetState(NextStateID);
        if (nextState == null)
        {
            Debug.LogWarning("No state with the ID " + NextStateID + " was found!");
            return;
        }
        

        if (_currentState != null)
        {
            _currentState.StateExit(NextStateID);
        }
        
        StateID previousStateID = _currentStateID;
        
        _currentStateID = NextStateID;
        _currentState = nextState;

        if (_currentState != null)
        {
            _currentState.StateEnter(previousStateID);
        }
        _alien.AlienAnimation.SetEmotion(GetEmotionTypeFromStateId(_currentStateID));
    }

    

    public State GetState(StateID StateID) // Renvoie le state en fonction d'un StateID
    {
        foreach (State state in _allStates)
        {
            if (StateID == state.GetStateID())
            {
                return state;
            }
        }

        return null;
    }

    public State GetCurrentState()
    {
        return _currentState;
    }

    public StateID GetCurrentStateID() // Renvoie le stateID actuel
    {
        return _currentStateID;
    }

    public EmotionType GetCurrentEmotionType()
    {
        return GetEmotionTypeFromStateId(_currentStateID);
    }

    public EmotionType GetEmotionTypeFromStateId(StateID stateID)
    {
        return stateID switch
        {
            StateID.Friendly => EmotionType.Friendly,
            StateID.Aggressive => EmotionType.Aggressive,
            StateID.Fearful => EmotionType.Fearful,
            StateID.Curious => EmotionType.Curious,
            _ => EmotionType.None
        };
    }
    
}
