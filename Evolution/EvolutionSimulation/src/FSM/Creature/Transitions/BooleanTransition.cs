using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EvolutionSimulation.FSM.Creature.Transitions
{
    class BooleanTransition : ITransition
    {
        /// <summary>
        /// Constructor for the boolean transition, which evaluates true if the given bool is true
        /// </summary>
        public BooleanTransition(ref bool b)
        {
            flag = b;
        }

        /// <summary>
        /// Evaluates true if the given bool is true
        /// </summary>
        public bool Evaluate()
        {
            return flag;
        }

        bool flag;
    }
}
