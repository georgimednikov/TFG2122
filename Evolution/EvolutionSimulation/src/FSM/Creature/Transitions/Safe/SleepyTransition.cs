namespace EvolutionSimulation.FSM.Creature.Transitions
{
    class SleepyTransition : CreatureTransition
    {
        public SleepyTransition(Entities.Creature creature)
        {
            this.creature = creature;
        }

        public override bool Evaluate()
        {
            return creature.IsExhausted();
        }

        public override string ToString()
        {
            return "SleepyTransition";
        }
    }
}
