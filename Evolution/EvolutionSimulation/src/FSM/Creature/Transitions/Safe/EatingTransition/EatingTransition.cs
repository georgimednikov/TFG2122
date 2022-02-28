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

        public override bool Evaluate()
        {
            if(creature.stats.Diet == Genetics.Diet.Herbivore )
            {
                return creature.GetClosestFruit() != null
                  && Math.Abs(creature.GetClosestFruit().x - creature.x) < 1
                  && Math.Abs(creature.GetClosestFruit().y - creature.y) < 1;
            }

            if (creature.stats.Diet == Genetics.Diet.Carnivore )
            {
                return creature.GetClosestCorpse() != null
                  && Math.Abs(creature.GetClosestCorpse().x - creature.x) < 1
                  && Math.Abs(creature.GetClosestCorpse().y - creature.y) < 1;
            }

            //Omnivore
            return (creature.GetClosestCorpse() != null
                && Math.Abs(creature.GetClosestCorpse().x - creature.x) < 1
                && Math.Abs(creature.GetClosestCorpse().y - creature.y) < 1) 
                || creature.GetClosestFruit() != null
                && Math.Abs(creature.GetClosestFruit().x - creature.x) < 1
                && Math.Abs(creature.GetClosestFruit().y - creature.y) < 1; ;
            
        }

        public override string ToString()
        {
            return "EatingTransition";
        }

    }
}
