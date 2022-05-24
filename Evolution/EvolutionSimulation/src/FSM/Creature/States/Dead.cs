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

        public override void OnEntry()
        {
            base.OnEntry();
            creature.WitnessDeath();  // the death action will be executed just once
        }
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
            creature.world.Destroy(creature.ID); // aqui ya null en el diccionario y no le pueden volver a hacer daño

#if TRACKER_ENABLED
            Tracker.Instance.Track(new CreatureDeath(creature.world.tick, creature.ID, creature.speciesName, (DeathType)creature.causeOfDeath, creature.killerID, creature.killingBlow, creature.x, creature.y));
#endif         
            // Set cause of death
            template = creature.speciesName + " with ID " + creature.ID + " DIES; CAUSE OF DEATH: ";
            template += creature.causeOfDeath.ToString();
            creature.world.deaths[(int)creature.causeOfDeath]++;

            //To avoid create corpses in a water tile
            if (!creature.world.map[creature.x, creature.y].isWater)
            {
                corpse = creature.world.CreateStaticEntity<Entities.Corpse>(creature.x, creature.y, 50);    // TODO: no poner el hp a pelo
                corpse.SetTraits(creature);
                corpse.AddToUpdateList();
                template += "\nCORPSE CREATED WITH ID: " + corpse.ID + " IN POSITION: (" + creature.x + ", " + creature.y + ")";
            }
            else
                template += "\nCORPSE CANNOT BE CREATED IN: POSITION: (" + creature.x + ", " + creature.y + ") CREATURE ID: " + creature.ID;  
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
