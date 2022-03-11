using System;

namespace EvolutionSimulation.FSM.Creature.Transitions
{
    /// <summary>
    /// Checks if the creature is close to the eating objective
    /// Go to eat -> Eating
    /// </summary>
    class EatingTransition : CreatureTransition
    {
        public EatingTransition(Entities.Creature creature)
        {
            this.creature = creature;
        }

        /// <summary>
        /// Check if the creature is close (UniverseParametersManager.parameters.adjacentLength tile at most) to an eating objective
        /// The eating objective depends on the creature's diet
        /// </summary>
        /// <returns> True if close the an eating objective</returns>
        public override bool Evaluate()
        {
            //Herbivore
            if(creature.stats.Diet == Genetics.Diet.Herbivore)
                return creature.GetClosestFruitPosition() != null && creature.DistanceToObjective(creature.GetClosestFruitPosition()) <= UniverseParametersManager.parameters.adjacentLength;

            //Carnivore
            if (creature.stats.Diet == Genetics.Diet.Carnivore)
            {
                if (creature.GetClosestCorpsePosition() != null && creature.DistanceToObjective(creature.GetClosestCorpsePosition()) <= UniverseParametersManager.parameters.adjacentLength)
                    return true;
                else if (creature.GetClosestRottenCorpsePosition() != null && creature.DistanceToObjective(creature.GetClosestRottenCorpsePosition()) <= UniverseParametersManager.parameters.adjacentLength)
                    return true;
                return false;
            }

            if (creature.GetClosestCorpsePosition() != null && creature.GetClosestFruitPosition() != null)
            {
                int distPlant = creature.DistanceToObjective(creature.GetClosestFruitPosition()),
                    distCorpse = creature.DistanceToObjective(creature.GetClosestCorpsePosition());
                //Omnivore, close to an eating objective
                return (creature.GetClosestCorpsePosition() != null && distCorpse <= UniverseParametersManager.parameters.adjacentLength) ||
                    (creature.GetClosestFruitPosition() != null && distPlant <= UniverseParametersManager.parameters.adjacentLength);
            }

            return creature.DistanceToObjective(creature.GetClosestRottenCorpsePosition()) <= UniverseParametersManager.parameters.adjacentLength;
        }

        public override string ToString()
        {
            return "EatingTransition";
        }

    }
}
