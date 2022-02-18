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

        /// <summary>
        /// If the creature dies, it consumes all its action points to avoid doing actions while dead. 
        /// Creates a corpse in the death position.
        /// </summary>
        public override int Action()
        {
            Console.WriteLine("Dead");
            creature.world.Destroy(creature);
            Corpse corpse = creature.world.CreateStableEntity<Corpse>();
            corpse.Init(creature.world, 5, creature.x, creature.y, 40, 70, 80); // TODO: stats to be derived from creacher
            return creature.actionPoints;
        }

        public override string ToString()
        {
            return "DeadState";
        }
    }
}
