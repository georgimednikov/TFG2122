namespace EvolutionSimulation.FSM.Creature.Transitions
{
    /// <summary>
    /// Checks if the creature has stopped to mate
    /// Mating -> wander
    /// </summary>
    class StopMatingTransition : CreatureTransition
    {
        public StopMatingTransition(Entities.Creature creature)
        {
            this.creature = creature;
        }

        public override bool Evaluate()
        {
            return !creature.mating;
        }

        public override string ToString()
        {
            return "StopMatingTransition";
        }

    }
}
