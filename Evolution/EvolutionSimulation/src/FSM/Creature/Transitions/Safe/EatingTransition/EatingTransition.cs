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
            Vector2Int corpsePos;
            Vector2Int plantPos;
            creature.Corpse(out _, out corpsePos);
            creature.Plant(out _, out plantPos);
            int distPlant = creature.DistanceToObjective(plantPos), //Returns int max value if it does not exist
                distCorpse = creature.DistanceToObjective(corpsePos);
            //Omnivore, close to an eating objective
            return (distCorpse <= UniverseParametersManager.parameters.adjacentLength) ||
                (distPlant <= UniverseParametersManager.parameters.adjacentLength);
        }

        public override string ToString()
        {
            return "EatingTransition";
        }

    }
}
