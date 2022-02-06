using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AlgoritmoEvolutivo
{
    /// <summary>
    /// Clase que simula la evolucion
    /// </summary>
    public class Simulation
    {

        public void Init()
        {
            world = new World();
            world.Init(8);
            Creature c = world.AddEntity<Creature>();
            c.Init(world, 4, 4);
        }

        public void Run()
        {
            while (true)
            {
                world.Tick();
            }
        }

        World world;
    }
}
