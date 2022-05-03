using System;

using Telemetry;
using Telemetry.Events;

namespace EvolutionSimulation.FSM.Creature.States
{
   
    class Dead : CreatureState
    {
        public Dead(Entities.Creature c) : base(c) { creature = c; }
        Entities.Corpse corpse;
        string template;
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
            Tracker.Instance.Track(new CreatureDeath(creature.world.tick, creature.ID, creature.speciesName, (DeathType)creature.causeOfDeath, creature.killerID, creature.killingBlow));
            
            // Set cause of death
            template = creature.speciesName + " with ID " + creature.ID + " DIES; CAUSE OF DEATH: ";
            template += creature.causeOfDeath.ToString();
            creature.world.deaths[(int)creature.causeOfDeath]++;

            //To avoid create corpses in a water tile
            if (!creature.world.map[creature.x, creature.y].isWater)
            {
                template += "\nCORPSE CANNOT BE CREATED IN: POSITION: (" + creature.x + ", " + creature.y + ") CREATURE ID: " + creature.ID;  // TODO: Coordenada altura?
                corpse = creature.world.CreateStaticEntity<Entities.Corpse>(creature.x, creature.y, 50);    // TODO: no poner el hp a pelo
                corpse.SetTraits(creature);
                corpse.AddToUpdateList();
            }
            else
                template += "\nCORPSE CREATED WITH ID: " + corpse.ID + " IN POSITION: (" + creature.x + ", " + creature.y + ")";
        }

        public override string ToString()
        {
            return "DeadState";
        }
        public override string GetInfo()
        {
            return template;
        }

    }
}
