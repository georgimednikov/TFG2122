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
            Console.WriteLine(creature.x + " " + creature.y + " | " + creature.GetClosestWaterPosition() + " | " + (Math.Abs(creature.GetClosestWaterPosition().Item1 - creature.x) <= 1 && Math.Abs(creature.GetClosestWaterPosition().Item2 - creature.y) <= 1));
            return (Math.Abs(creature.GetClosestWaterPosition().Item1 - creature.x) <= 1 && Math.Abs(creature.GetClosestWaterPosition().Item2 - creature.y) <= 1);
        }

        public override string ToString()
        {
            return "DrinkingTransition";
        }

    }
}
