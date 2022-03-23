namespace EvolutionSimulation.FSM.Creature.Transitions
{
    /// <summary>
    /// Checks if a creature need to drink and know where to drink
    /// Wander -> Go to drink
    /// </summary>
    class ThirstyTransition : CreatureTransition
    {
        public ThirstyTransition(Entities.Creature creature)
        {
            this.creature = creature;
        }

        public override bool Evaluate()
        {
            return creature.IsThirsty() && creature.WaterPosition() != null;
        }

        public override string ToString()
        {
            return "ThirstyTransition";
        }

    }
}
