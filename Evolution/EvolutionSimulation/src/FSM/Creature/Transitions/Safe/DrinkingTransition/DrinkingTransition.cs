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
            int range = UniverseParametersManager.parameters.adjacentLength;
            return (Math.Abs(creature.WaterPosition().x - creature.x) <= range && Math.Abs(creature.WaterPosition().y - creature.y) <= range);
        }

        public override string ToString()
        {
            return "DrinkingTransition";
        }

    }
}
