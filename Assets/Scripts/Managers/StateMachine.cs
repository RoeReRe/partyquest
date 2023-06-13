using UnityEngine;
using Photon.Pun;
public abstract class StateMachine : MonoBehaviourPunCallbacks {
    protected State currentState;

    public virtual void ChangeState(State newState) {
        currentState.OnExit();
        currentState = newState;
        currentState.OnEnter();
    }
}