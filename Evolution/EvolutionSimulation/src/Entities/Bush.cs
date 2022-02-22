using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EvolutionSimulation
{
    public class Bush : EdiblePlant
    {
        public Bush()
        {
            regrowhtTime = RandomGenerator.Next(1, 101);
            nutritionalValue = RandomGenerator.Next(5, 11);
        }

        public Bush(int regrowhtTime, float nutritionalValue)
        {
            this.regrowhtTime = regrowhtTime;
            this.nutritionalValue = nutritionalValue;
        }
    }
}
