using System;

namespace EvolutionSimulation.FSM.Creature.States
{
    // TODO: Estado para testear, hacer el estado correctamente
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
#if DEBUG
        public override string GetInfo()
        {
            string template = creature.speciesName + " with ID " + creature.ID + " DIES; CAUSE OF DEATH: ";
            World.Death death = new World.Death();

            death.pos = new System.Numerics.Vector2(creature.x, creature.y);
            if(creature.stats.CurrEnergy <= 0)
            {
                template += " starved to death";
                creature.world.deaths[3]++;
                death.cause = World.DeathCause.Starvation;
                creature.world.deathsPos.Add(death);
            }
            else if (creature.stats.CurrHydration <= 0)
            {
                template += " died of thirst";
                creature.world.deaths[4]++;
                death.cause = World.DeathCause.Thirst;
                creature.world.deathsPos.Add(death);
            }
            else if (creature.stats.CurrRest <= 0)
            {
                template += " died of exhaustion";
                creature.world.deaths[5]++;
                death.cause = World.DeathCause.Exhaustion;
                creature.world.deathsPos.Add(death);
            }
            else if (creature.stats.CurrHealth <= 0)
            {
                template += creature.causeOfDeath;
                if (template.Contains("attack from ")) { 
                    creature.world.deaths[1]++; 
                    death.cause = World.DeathCause.Others;
                    creature.world.deathsPos.Add(death);
                }
                else if (template.Contains("retalliation from ")) { 
                    creature.world.deaths[2]++;
                    death.cause = World.DeathCause.Retaliation;
                    creature.world.deathsPos.Add(death);
                }
                else if (template.Contains("temperature difference: ")) { 
                    creature.world.deaths[0]++; 
                    death.cause = World.DeathCause.Temperature;
                    creature.world.deathsPos.Add(death);
                }
            }
            if(corpse == null)
                return template + "\nCORPSE CANNOT BE CREATED IN: POSITION: (" + creature.x + ", " + creature.y + ") CREATURE ID: " + creature.ID;
            
            return template + "\nCORPSE CREATED WITH ID: " + corpse.ID + " IN POSITION: (" + creature.x + ", " + creature.y + ")";

        }
#endif
    }
}
