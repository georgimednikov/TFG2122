using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AlgoritmoEvolutivo
{
    /// <summary>
    /// Simulation of evolution
    /// </summary>
    public class Simulation : ISimulation
    {
        public void Init()
        {
            world = new World();
            world.Init(32);
            //Creature c = world.AddEntity<Creature>();
            //c.Init(world, 4, 4);
        }

        public void Run()
        {
            while (true)
            {
                world.Tick();
            }
        }

        public void Render()
        {
        }

        World world;
    }
}
