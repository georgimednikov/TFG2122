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

        public override int GetCost()
        {
            return 1000000; // TODO: Controlar este coste para que no se muera varias veces
        }

        /// <summary>
        /// If the creature dies, it consumes all its action points to avoid doing actions while dead. 
        /// Creates a corpse in the death position.
        /// </summary>
        public override void Action()
        {
            Console.WriteLine("Dead");
            creature.world.Destroy(creature);
            Entities.Corpse corpse = creature.world.CreateStableEntity<Entities.Corpse>();
            corpse.Init(creature.world, 5, creature.x, creature.y, 0.4f, 0.7f, 5, 5, 80f); // TODO: stats to be derived from creacher
        }

        public override string ToString()
        {
            return "DeadState";
        }
    }
}
