using UnityEngine;
using AttackFlyStates;

public class AttackFly : Monster<AttackFly>
{
    private enum States { Idle, Move, Dead }
    private States? curState;

    public Vector2 playerSearchBox;
    public float collisionCircle;

    [Tooltip("Speed for following the player")]
    public float followSpeed;

    private void Start()
    {
        curState = States.Idle;
        fsm = new FSM<AttackFly>(new IdleState(this));
    }

    protected override void OnEnable()
    {
        base.OnEnable();

        if (curState != null) ChangeState(States.Idle);
    }

    private void Update()
    {
        if (curState == States.Dead) return;

        switch (curState)
        {
            case States.Idle:
                if (OnDead())
                {
                    ChangeState(States.Dead);
                }
                else if (isSpawned)
                {
                    ChangeState(States.Move);
                }
                break;
            case States.Move:
                if (OnDead())
                {
                    ChangeState(States.Dead);
                }
                break;
            case States.Dead:
                // Dead state logic, if necessary
                break;
        }

        fsm.UpdateState();
    }

    private void ChangeState(States nextState)
    {
        curState = nextState;

        switch (curState)
        {
            case States.Idle:
                fsm.ChangeState(new IdleState(this));
                break;
            case States.Move:
                fsm.ChangeState(new MoveState(this));
                break;
            case States.Dead:
                fsm.ChangeState(new DeadState(this));
                break;
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(transform.position, playerSearchBox);

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, collisionCircle);
    }
}
