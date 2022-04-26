using System;

namespace EvolutionSimulation.FSM.Creature.Transitions
{
    class CombatTransition : CreatureTransition
    {
        public CombatTransition(Entities.Creature creature)
        {
            this.creature = creature;
        }

        public override bool Evaluate()
        {
            int advID; Vector3Int advPos;

            // If it has no enemy and has no creature to hunt it does not engage in combat.
            if (!creature.Enemy() && !creature.Prey(out _, out _))
                return false;

            // If it has an enemy and enough strength between it and its allies to fight it, it engages in combat.
            if (creature.Enemy(out _, out advPos) && creature.ShouldPackFight(creature.CombatPack(), creature.PositionDanger(advPos.x, advPos.y)))
            {
                if (advPos.z == 2)
                    return true;
                return true;
            }

            // If a dangerous creature is nearby and it cannot run away, it engages in combat,
            if (creature.Menace(out advID, out advPos) && creature.cornered)
            {
                //TODO quitar esta comprobacion, no debería hacerse, está solo para debugear algun fallo.
                //if (creature.world.GetCreature(advID) == null)
                //{
                //    creature.mind.mem.Update();
                //    return false;
                //}
                creature.TargetEnemy(advID, advPos);
                if (advPos.z == 2)
                    return true;
                return true;
            }

            // Else it may want to hunt. If there is an available prey and is hungry.
            if (creature.Prey(out advID, out advPos) && creature.IsHungry())
            {
                // It goes for the prey if there is no plant or it is closer.
                if (!creature.Plant(out _, out Vector3Int plantPos) || creature.DistanceToObjective(plantPos) > creature.DistanceToObjective(advPos))
                {
                    creature.TargetEnemy(advID, advPos);   // The Prey becomes the Enemy, beginning combat
                    return true;
                }
            }

            return false;
        }

        public override string ToString()
        {
            return "CombatTransition";
        }

    }
}
