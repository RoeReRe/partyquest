public abstract class PlayerBoardStateMachine : StateMachine {
    protected new PlayerBoardState currentState;

    public void ChangeState(PlayerBoardState newState) {
        currentState.OnExit();
        currentState = newState;
        currentState.OnEnter();
    }
}