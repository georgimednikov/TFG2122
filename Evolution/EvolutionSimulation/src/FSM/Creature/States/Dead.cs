using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EvolutionSimulation.FSM.Creature.States
{
    class Dead : IState
    {
        public void Action()
        {
            Console.WriteLine("Dead");
        }
    }
}
