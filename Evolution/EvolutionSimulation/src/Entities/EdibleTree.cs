using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EvolutionSimulation
{
    public class EdibleTree : EdiblePlant
    {
        public EdibleTree()
        {
            regrowhtTime = RandomGenerator.Next(50, 101);
            nutritionalValue = RandomGenerator.Next(5, 11);
        }
    }
}
