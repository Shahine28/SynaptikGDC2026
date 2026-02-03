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
    private SerializedDictionary<EmotionType, StateID> _alienStateFromPlayerEmotion = new();
    
    
    private State _currentState;
    private Alien _alien;
    
    public void StateMachineUpdate(float deltaTime) // On appelle la fonction StateUpdate de l'état actuel à chaque tick pour simuler une fonction update
    {
        if (_currentState == null) return;
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
    }

    public void UpdateState(EmotionType playerEmotionType)
    {
        if (_alienStateFromPlayerEmotion.ContainsKey(playerEmotionType) && _alienStateFromPlayerEmotion[playerEmotionType] != _currentStateID)
        {
            ChangeState(_alienStateFromPlayerEmotion[playerEmotionType]);   
        }
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
