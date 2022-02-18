using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EvolutionSimulation.FSM.Creature.Transitions
{
    class MateTransition : CreatureTransition
    {
        public MateTransition(Entities.Creature creature)
        {
            this.creature = creature;
        }

        public override bool Evaluate()
        {
            Entities.Creature obj = creature.objective as Entities.Creature;
            bool result = creature.stats.InHeat
                //&& creature.nearestMate != null               // 
                                                                // TODO: Comprobar si es de la misma especie
                && !obj.stats.IsNewBorn()                       // it has to be adult
                && creature.stats.Gender != obj.stats.Gender;   // and with different gender

            if (result)
            {
                //creature.objective = creature.nearestMate
                
            }

            return result;
        }

        public override string ToString()
        {
            return "MateTransition";
        }

    }
}
