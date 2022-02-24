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
            return creature.stats.CurrRest <= creature.stats.exhaustThreshold * creature.stats.MaxRest;
        }

        public override string ToString()
        {
            return "SleepyTransition";
        }
    }
}
