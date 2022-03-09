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
            int distPlant = creature.DistanceToObjective(creature.GetClosestFruitPosition()),
                distCorpse = creature.DistanceToObjective(creature.GetClosestCorpsePosition());

            return creature.stats.CurrEnergy >= UniverseParametersManager.parameters.stopEatingTransitionEnergyMultiplier * creature.stats.MaxEnergy         // no hunger
                || (distCorpse > UniverseParametersManager.parameters.adjacentLength && distPlant > UniverseParametersManager.parameters.adjacentLength)                                    // Both eating objective are far
                || (distPlant > UniverseParametersManager.parameters.adjacentLength && creature.stats.Diet == Genetics.Diet.Herbivore)    // hervibore and plant objective is far
                || (distCorpse > UniverseParametersManager.parameters.adjacentLength && creature.stats.Diet == Genetics.Diet.Carnivore);  // carnivore and corpse objective is far
        }

        public override string ToString()
        {
            return "StopEatingTransition";
        }

    }
}
