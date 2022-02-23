namespace EvolutionSimulation.FSM.Creature.Transitions
{
    /// <summary>
    /// The creature is in a safe place (probably with other creatures of his species)
    /// and want to sleep 
    /// </summary>
    class SleepySafeTransition : CreatureTransition
    {
        public SleepySafeTransition(Entities.Creature creature)
        {
            this.creature = creature;
        }

        public override bool Evaluate()
        {
            //TODO comprobar que es es un sitio seguro
            return (creature.stats.CurrRest <= 0.2 * creature.stats.MaxRest /*&& safeplace*/)
                || creature.stats.CurrRest <= 0.1 * creature.stats.MaxRest;
        }

        public override string ToString()
        {
            return "SleepySafeTransition";
        }
    }
}
