using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EvolutionSimulation.FSM.Creature.States
{
    class Idle : IState
    {
        public void Action()
        {
            Console.WriteLine("Idle");
        }
    }
}
