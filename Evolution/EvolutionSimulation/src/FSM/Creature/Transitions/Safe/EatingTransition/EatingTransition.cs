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
        /// Check if the creature is close (1 tile at most) to an eating objective
        /// The eating objective depends on the creature's diet
        /// </summary>
        /// <returns> True if close the an eating objective</returns>
        public override bool Evaluate()
        {
            //Close to a fruit
            if(creature.stats.Diet == Genetics.Diet.Herbivore )
                return creature.GetClosestFruitPosition() != null && creature.DistanceToObjective(creature.GetClosestFruitPosition()) <= 1;

            //Close to a corpse
            if (creature.stats.Diet == Genetics.Diet.Carnivore )
                return creature.GetClosestCorpsePosition() != null && creature.DistanceToObjective(creature.GetClosestCorpsePosition()) <= 1;

            int distPlant = creature.DistanceToObjective(creature.GetClosestFruitPosition()),
                distCorpse = creature.DistanceToObjective(creature.GetClosestCorpsePosition());
            //Omnivore, close to an eating objective
            return (creature.GetClosestCorpsePosition() != null && distCorpse <= 1) ||
                (creature.GetClosestFruitPosition() != null && distPlant <= 1); 
        }

        public override string ToString()
        {
            return "EatingTransition";
        }

    }
}
