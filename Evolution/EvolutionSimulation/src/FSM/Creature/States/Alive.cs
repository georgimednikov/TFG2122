﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EvolutionSimulation.FSM.Creature.States
{
    // TODO: Estado para testear, hacer el estado correctamente

    class Alive : IState
    {
        public bool canPerformAction(int actionPoints)
        {
            return actionPoints < 1000;
        }

        public int Action()
        {
            Console.WriteLine("Alive");
            return 1000;
        }
    }
}
