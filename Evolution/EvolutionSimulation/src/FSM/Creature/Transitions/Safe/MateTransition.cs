using System;

namespace EvolutionSimulation.FSM.Creature.Transitions
{
    /// <summary>
    /// This transition is for males creatures and is to go to a female
    /// who is in heat.
    /// </summary>
    class MateTransition : CreatureTransition
    {
        public MateTransition(Entities.Creature creature)
        {
            this.creature = creature;
        }

        public override bool Evaluate()
        {
            //TODO que el objetivo sea el de mate
            Entities.Creature obj = creature.objective as Entities.Creature;
            bool result = obj.stats.InHeat                          //the objective is in heat (just in case)
                //&& creature.nearestMate != null                   // 
                                                                    // TODO: Comprobar si es de la misma especie
                && creature.stats.Gender == Genetics.Gender.Male    // the male is the one that goes to the female
                && !creature.stats.IsNewBorn();                     // and it has to be adult

            return result;
        }

        public override string ToString()
        {
            return "MateTransition";
        }

    }
}
