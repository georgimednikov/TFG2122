using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EvolutionSimulation.FSM.Creature.States
{
    // TODO: Estado para testear, hacer el estado correctamente
    class Dead : CreatureState
    {
        public Dead(Entities.Creature c) : base(c) { creature = c; }

        public override bool canPerformAction(int actionPoints)
        {
            return actionPoints > 0;
        }

        /// If the creature dies, it consumes all its action points to avoid
        /// doing actions while dead
        public override int Action()
        {
            Console.WriteLine("Dead");
            creature.world.Destroy(creature);
            return creature.actionPoints;
        }

        public override string ToString()
        {
            return "DeadState";
        }
    }
}
