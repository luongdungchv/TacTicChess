using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StateMachine : MonoBehaviour
{
    private Dictionary<State, State[]> transitions;
    private State currentState;
    // Start is called before the first frame update
    void Start()
    {
        transitions.Add(State.Normal, new State[] { State.MoveSelect, State.BarrierSelect });
        transitions.Add(State.MoveSelect, new State[] { State.Normal, State.OtherTurn, State.AnimationPlaying });
        transitions.Add(State.BarrierSelect, new State[] { State.Normal, State.OtherTurn, State.AnimationPlaying });
        transitions.Add(State.OtherTurn, new State[] { State.Normal });
        transitions.Add(State.AnimationPlaying, new State[] { State.Normal });

    }

    // Update is called once per frame
    void Update()
    {

    }
}
public enum State
{
    BarrierSelect, MoveSelect, AnimationPlaying, OtherTurn, Normal
}
