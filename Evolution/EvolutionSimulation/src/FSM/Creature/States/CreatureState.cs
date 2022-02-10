using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EvolutionSimulation.FSM.Creature.States
{
    /// <summary>
    /// State the creature will use in its FSM
    /// </summary>
    class CreatureState : IState
    {
        protected Entities.Creature creature;

        public CreatureState(Entities.Creature c) { creature = c; }

        public virtual bool canPerformAction(int actionPoints) { return true; }

        public virtual int Action() { return 1; }
    }
}
