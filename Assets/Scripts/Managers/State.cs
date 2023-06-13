using UnityEngine;

public abstract class State
{
    protected StateMachine context;
    public State(StateMachine context) {
        this.context = context;
    }
    public abstract void OnEnter();
    public abstract void OnExit();    
}
