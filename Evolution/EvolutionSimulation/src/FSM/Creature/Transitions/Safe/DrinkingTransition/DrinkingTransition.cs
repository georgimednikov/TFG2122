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
            //bool shore = false;
            //for (int k = -1; !shore && k <= 1; k++)
            //    for (int h = -1; !shore && h <= 1; h++)
            //        if (creature.world.checkBounds(creature.x + k, creature.y + h) && creature.world.map[creature.x + k, creature.y + h].isWater && creature.creatureLayer == 0)
            //            shore = true;
            //return shore;
            int range = UniverseParametersManager.parameters.adjacentLength;
#if DEBUG
            //Console.Write("TRANSICION BEBER: " + creature.speciesName + " with ID: " + creature.ID + " IN (" + creature.x + ", " + creature.y + ")" + " IS NEXT TO (" + creature.WaterPosition().x + ", " + creature.WaterPosition().y + ")? ");
#endif
            bool a = (Math.Abs(creature.WaterPosition().x - creature.x) <= range && Math.Abs(creature.WaterPosition().y - creature.y) <= range);
#if DEBUG
            //Console.WriteLine(a);
#endif
            return a;
        }

        public override string ToString()
        {
            return "DrinkingTransition";
        }

    }
}
