﻿using System;

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
            corpse = creature.world.CreateStaticEntity<Entities.Corpse>(creature.x, creature.y, 50);    // TODO: no poner el hp a pelo
            corpse.SetTraits(creature);
        }

        public override string ToString()
        {
            return "DeadState";
        }

        public override string GetInfo()
        {
            return creature.speciesName + "with ID " + creature.ID + " DIES\nCORPSE CREATED WITH ID: " + corpse.ID + "IN POSITION: (" + creature.x + ", " + creature.y + ")";
        }
    }
}
