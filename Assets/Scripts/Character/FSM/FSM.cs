public class FSM<T> where T : class
{
      private BaseState<T> curState;

      public FSM(BaseState<T> initState)
      {
            curState = initState;
            ChangeState(curState);
      }

      public void ChangeState(BaseState<T> nextState)
      {
            if (curState == nextState) return;

            curState?.OnStateExit();

            curState = nextState;
            curState.OnStateEnter();
      }

      public void UpdateState()
      {
            if (curState == null) return;

            curState.OnStateUpdate();
      }
}
