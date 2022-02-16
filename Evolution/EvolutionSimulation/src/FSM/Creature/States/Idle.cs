using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EvolutionSimulation.FSM.Creature.States
{
    // TODO: Estado para testear, hacer el estado correctamente
    class Idle : CreatureState
    {
        public Idle(Entities.Creature c) : base(c) { creature = c; }

        public override bool canPerformAction(int actionPoints)
        {
            return actionPoints >= 10 * creature.stats.Metabolism;
        }

        public override int Action()
        {
            Console.WriteLine("Idle");
            return 10 * creature.stats.Metabolism;
        }
    }
}
