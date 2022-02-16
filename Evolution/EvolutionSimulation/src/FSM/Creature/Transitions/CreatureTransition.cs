using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EvolutionSimulation.FSM.Creature.Transitions
{
    public abstract class CreatureTransition : ITransition
    {
        protected Entities.Creature creature;
        public abstract bool Evaluate();

        public override string ToString()
        {
            return "CreatureTransition";
        }
    }
}
