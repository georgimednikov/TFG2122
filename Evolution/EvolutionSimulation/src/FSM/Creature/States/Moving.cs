using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EvolutionSimulation.FSM.Creature.States
{
    // TODO: Estado para testear, hacer el estado correctamente
    class Moving : IState
    {
        public void Action()
        {
            Console.WriteLine("Moving");
        }
    }
}
