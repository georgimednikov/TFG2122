using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EvolutionSimulation.Unity
{
    class SimulationInfo
    {
        public SimulationInfo(World world)
        {
            this.World = world;
        }
        public World World { get; private set; }
    }
}
