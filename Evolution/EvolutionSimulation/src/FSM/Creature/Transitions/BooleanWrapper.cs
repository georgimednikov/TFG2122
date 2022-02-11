using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EvolutionSimulation.FSM.Creature.Transitions
{
    class BooleanWrapper
    {
        public bool value { get; set; }

        public BooleanWrapper(bool b) { value = b; }
    }
}
