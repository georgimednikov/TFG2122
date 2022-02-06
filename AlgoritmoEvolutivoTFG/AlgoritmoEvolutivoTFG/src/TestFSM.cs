using Stateless;
namespace EvolutionSimulation
{
    public enum State
    {
        Alive,
        Dead
    }

    public enum Trigger
    {
        Die,
        Revive
    }
    public class TestFSM
    {
        StateMachine<State, Trigger> fsm;

        public TestFSM()
        {
            fsm = new StateMachine<State, Trigger>(State.Alive);

            fsm.Configure(State.Alive)
                .Permit(Trigger.Die, State.Dead);

            fsm.Configure(State.Dead)
                .Permit(Trigger.Revive, State.Alive);

        }
        public State GetState()
        {
            return fsm.State;
        }
        public void Fire(Trigger trigger)
        {
            fsm.Fire(trigger);
        }
    }
}
