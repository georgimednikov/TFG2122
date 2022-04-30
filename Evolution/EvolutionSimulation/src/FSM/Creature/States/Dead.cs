using System;
using Telemetry;
using Telemetry.Events;

namespace EvolutionSimulation.FSM.Creature.States
{
   
    class Dead : CreatureState
    {
        public Dead(Entities.Creature c) : base(c) { creature = c; }
        Entities.Corpse corpse;
        public override int GetCost()
        {
            return Math.Max(creature.ActionPoints, 1); // This allows for one death and prevents subsequent ones
        }

        /// <summary>
        /// If the creature dies, it consumes all its action points to avoid doing actions while dead. 
        /// Creates a corpse in the death position.
        /// </summary>
        public override void Action()
        {
            creature.world.Destroy(creature.ID);
            //To avoid create corpses in a water tile
            if (!creature.world.map[creature.x, creature.y].isWater)
            {
                corpse = creature.world.CreateStaticEntity<Entities.Corpse>(creature.x, creature.y, 50);    // TODO: no poner el hp a pelo
                corpse.SetTraits(creature);
                corpse.AddToUpdateList();
            }
        }

        public override string ToString()
        {
            return "DeadState";
        }
        public override string GetInfo()
        {
            string template = creature.speciesName + " with ID " + creature.ID + " DIES; CAUSE OF DEATH: ";
            World.Death death = new World.Death();

            Tracker.Instance.Track(new Die(creature.world.tick, creature.ID, (DeathType)creature.causeOfDeath, creature.speciesName));
            template += creature.causeOfDeath.ToString();
            creature.world.deaths[(int)creature.causeOfDeath]++;

            if(corpse == null)
                return template + "\nCORPSE CANNOT BE CREATED IN: POSITION: (" + creature.x + ", " + creature.y + ") CREATURE ID: " + creature.ID;  // TODO: Coordenada altura?
            
            return template + "\nCORPSE CREATED WITH ID: " + corpse.ID + " IN POSITION: (" + creature.x + ", " + creature.y + ")";

        }

    }
}
