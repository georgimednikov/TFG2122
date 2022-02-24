using System;

namespace EvolutionSimulation.FSM.Creature.Transitions
{
    /// <summary>
    /// This transition is for males creatures and is to go to a female
    /// who is in heat.
    /// Wander -> Go to mate
    /// </summary>
    class MateTransition : CreatureTransition
    {
        public MateTransition(Entities.Creature creature)
        {
            this.creature = creature;
        }

        public override bool Evaluate()
        {
            bool result = creature.nearestMate != null
                && creature.nearestMate.stats.InHeat                //the objective is in heat (just in case)
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
