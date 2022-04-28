﻿using System;
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
             
            if(creature.stats.CurrEnergy <= 0)
            {
                Tracker.Instance.Track(new Die(creature.world.tick, creature.ID, DeathType.Starved, creature.speciesName));
                template += " starved to death";
                creature.world.deaths[3]++;
            }
            else if (creature.stats.CurrHydration <= 0)
            {
                Tracker.Instance.Track(new Die(creature.world.tick, creature.ID, DeathType.Dehydration, creature.speciesName));
                template += " died of thirst";
                creature.world.deaths[4]++;
            }
            else if (creature.stats.CurrRest <= 0)
            {
                Tracker.Instance.Track(new Die(creature.world.tick, creature.ID, DeathType.Exhaustion, creature.speciesName));
                template += " died of exhaustion";
                creature.world.deaths[5]++;
            }
            else if (creature.stats.CurrHealth <= 0)
            {
                template += creature.causeOfDeath;
                if (template.Contains("attack from "))
                {
                    Tracker.Instance.Track(new Die(creature.world.tick, creature.ID, DeathType.Attack, creature.speciesName));
                    creature.world.deaths[1]++;
                }
                else if (template.Contains("retalliation from "))
                {
                    Tracker.Instance.Track(new Die(creature.world.tick, creature.ID, DeathType.Retalliation, creature.speciesName));
                    creature.world.deaths[2]++;
                }
                else if (template.Contains("temperature difference: "))
                {
                    Tracker.Instance.Track(new Die(creature.world.tick, creature.ID, DeathType.Temperature, creature.speciesName));
                    creature.world.deaths[0]++;
                }
                else if (template.Contains("poison, "))
                {
                    Tracker.Instance.Track(new Die(creature.world.tick, creature.ID, DeathType.Temperature, creature.speciesName));
                    creature.world.deaths[6]++;
                }
            }
            if(corpse == null)
                return template + "\nCORPSE CANNOT BE CREATED IN: POSITION: (" + creature.x + ", " + creature.y + ") CREATURE ID: " + creature.ID;
            
            return template + "\nCORPSE CREATED WITH ID: " + corpse.ID + " IN POSITION: (" + creature.x + ", " + creature.y + ")";

        }

    }
}
