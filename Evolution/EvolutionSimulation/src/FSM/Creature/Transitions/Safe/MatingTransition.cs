using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EvolutionSimulation.FSM.Creature.Transitions
{
    /// <summary>
    /// Checks if the creature is close to the eating objective
    /// </summary>
    class MatingTransition : CreatureTransition
    {
        public MatingTransition(Entities.Creature creature)
        {
            this.creature = creature;
        }

        public override bool Evaluate()
        {
            //TODO que el objetivo sea el de hacer mate. Comprobar tambien que
            //sea receptiva(no tenga que ir a comer, dormir, o este relacionandose con otro macho)
            //que la criatura no tenga necesidad de ir a comer, dormir... que haciendo la accion
            //no se va a parar hasta pasado un tiempo
            return creature.objective != null
               && Math.Abs(creature.objective.x - creature.x) < 1
               && Math.Abs(creature.objective.y - creature.y) < 1;
        }

        public override string ToString()
        {
            return "MatingTransition";
        }

    }
}
