using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EvolutionSimulation.FSM
{
    /// <summary>
    /// Superstate that calms the creature down once it enters it.
    /// </summary>
    class CalmState : CompoundState
    {
        //  Creature which will be calmed
        Entities.Creature c;

        public CalmState(string name, Fsm fsm, Entities.Creature c) : base(name, fsm) 
        {
            this.c = c;
        }

        public override void OnEntry()    // So this resets the creature's panic state
        {
            base.OnEntry();
            c.cornered = false;
        }
    }
}
