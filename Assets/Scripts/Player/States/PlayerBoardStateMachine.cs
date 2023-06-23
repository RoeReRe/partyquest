public abstract class PlayerBoardStateMachine : StateMachine {
    protected new PlayerBoardState currentState;
    protected PlayerBoardState battleState;

    public void ChangeState(PlayerBoardState newState) {
        currentState.OnExit();
        currentState = newState;
        currentState.OnEnter();
    }

    public void ChangeBattleState(PlayerBoardState newState) {
        battleState.OnExit();
        battleState = newState;
        battleState.OnEnter();
    }
}