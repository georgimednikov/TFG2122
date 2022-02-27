namespace EvolutionSimulation.FSM.Creature.Transitions
{
    /// <summary>
    /// The creature is in a safe place (probably with other creatures of his species)
    /// and want to sleep. If the creature is exhausted, it goes to sleep although it is 
    /// not in a safe place
    /// </summary>
    class SleepySafeTransition : CreatureTransition
    {
        public SleepySafeTransition(Entities.Creature creature)
        {
            this.creature = creature;
        }

        public override bool Evaluate()
        {
            //TODO comprobar que es un sitio seguro
            return (creature.IsTired() /*&& safeplace*/)
                || creature.IsExhausted();
        }

        public override string ToString()
        {
            return "SleepySafeTransition";
        }
    }
}
