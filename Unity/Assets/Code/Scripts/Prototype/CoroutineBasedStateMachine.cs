using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum State
{
    State1,
    State2,
    State3
}

public class CoroutineBasedStateMachine : MonoBehaviour
{
    Coroutine _current;
    public State currentState;

    void Start()
    {
        SetState(State.State1);
    }

    public void SetState(State state)
    {
        if (_current != null) StopCoroutine(_current);
        currentState = state;
        switch (currentState)
        {
            case State.State1:
                _current = StartCoroutine(State1Routine());
                break;
            case State.State2:
                _current = StartCoroutine(State2Routine());
                break;
            case State.State3:
                _current = StartCoroutine(State3Routine());
                break;
        }
    }

    IEnumerator State1Routine()
    {
        // Behaviour for state 1
        yield return new WaitForSeconds(1);
        SetState(State.State2);
    }

    IEnumerator State2Routine()
    {
        // Behaviour for state 2
        yield return new WaitForSeconds(2);
        SetState(State.State3);
    }

    IEnumerator State3Routine()
    {
        // Behaviour for state 2
        yield return new WaitForSeconds(3);
        SetState(State.State1);
    }
}