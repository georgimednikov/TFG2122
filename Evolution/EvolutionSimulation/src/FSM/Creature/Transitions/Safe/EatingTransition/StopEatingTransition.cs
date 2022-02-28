namespace EvolutionSimulation.FSM.Creature.Transitions
{
    /// <summary>
    /// Checks if the creature has eaten enough or do not have objective to eat close
    /// Eating -> wander
    /// </summary>
    class StopEatingTransition : CreatureTransition
    {
        public StopEatingTransition(Entities.Creature creature)
        {
            this.creature = creature;
        }

        public override bool Evaluate()
        {
            int distPlant = creature.DistanceToObjective(creature.GetClosestFruit()),
                distCorpse = creature.DistanceToObjective(creature.GetClosestCorpse());

            return creature.stats.CurrEnergy >= 0.85 * creature.stats.MaxEnergy         // no hunger
                || (distCorpse > 1 && distPlant > 1)                                    // Both eating objective are far
                || (distPlant > 1 && creature.stats.Diet == Genetics.Diet.Herbivore)    // hervibore and plant objective is far
                || (distCorpse > 1 && creature.stats.Diet == Genetics.Diet.Carnivore);  // carnivore and corpse objective is far
        }

        public override string ToString()
        {
            return "StopEatingTransition";
        }

    }
}
