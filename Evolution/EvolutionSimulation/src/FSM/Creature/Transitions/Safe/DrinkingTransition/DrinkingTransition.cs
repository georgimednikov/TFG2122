using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EvolutionSimulation.FSM.Creature.Transitions
{
    /// <summary>
    /// Checks if the creature is close to the drink objective
    /// Go to drink -> drinking
    /// </summary>
    class DrinkingTransition : CreatureTransition
    {
        public DrinkingTransition(Entities.Creature creature)
        {
            this.creature = creature;
        }

        public override bool Evaluate()
        {
            return (Math.Abs(creature.GetClosestWaterPosition().x - creature.x) <= 1 && Math.Abs(creature.GetClosestWaterPosition().y - creature.y) <= 1);
        }

        public override string ToString()
        {
            return "DrinkingTransition";
        }

    }
}
