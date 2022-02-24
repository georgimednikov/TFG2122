using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EvolutionSimulation.Entities
{
    public class EdibleTree : EdiblePlant
    {
        public float movementPenalty { get; private set; }
        public EdibleTree()
        {
            movementPenalty = RandomGenerator.Next(50, 100);
            regrowhtTime = RandomGenerator.Next(50, 101);
            nutritionalValue = RandomGenerator.Next(5, 11);
        }

        public EdibleTree(float movementPenalty, int regrowhtTime, float nutritionalValue)
        {
            this.movementPenalty = movementPenalty;
            this.regrowhtTime = regrowhtTime;
            this.nutritionalValue = nutritionalValue;
        }
    }
}
