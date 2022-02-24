namespace EvolutionSimulation.FSM.Creature.Transitions
{
    /// <summary>
    /// Checks if the creature has stopped to mate
    /// </summary>
    class StopMatingTransition : CreatureTransition
    {
        public StopMatingTransition(Entities.Creature creature)
        {
            this.creature = creature;
        }

        public override bool Evaluate()
        {
            //TODO comprobar que ya se ha reproducido o ha tenido que 
            return !creature.mating || creature.nearestMate == null;
        }

        public override string ToString()
        {
            return "StopMatingTransition";
        }

    }
}
