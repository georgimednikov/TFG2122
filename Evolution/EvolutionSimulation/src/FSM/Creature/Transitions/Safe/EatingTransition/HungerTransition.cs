namespace EvolutionSimulation.FSM.Creature.Transitions
{
    /// <summary>
    /// Checks if a creature need to eat and know where to go
    /// Wander -> Go to eat
    /// </summary>
    class HungerTransition : CreatureTransition
    {

        public HungerTransition(Entities.Creature creature)
        {
            this.creature = creature;
        }

        public override bool Evaluate()
        {
            if(creature.stats.CurrEnergy > creature.stats.hungerThreshold * creature.stats.MaxEnergy) return false;

            return creature.HasEatingObjective(); 
        }

        public override string ToString()
        {
            return "HungerTransition";
        }

    }
}
