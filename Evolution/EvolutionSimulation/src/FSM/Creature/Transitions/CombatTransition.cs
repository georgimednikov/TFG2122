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
            Vector2Int preyPos;
            // If it has not been attacked and has no creature to hunt it does not engage in combat.
            if (!creature.Prey(out _, out preyPos) && !creature.HasBeenAttacked()) return false;

            // Else if attacked it considers its nearby allies in combat.
            if (creature.HasBeenAttacked() &&
                (creature.AbleToFight() ||   // So it does not immediately return to combat while fleeing
                creature.cornered))    // So it fights as a last resort when fleeing
                return true;

            // Else it is trying to hunt
            if (!creature.IsHerbivorous() && creature.Prey(out _, out preyPos) && creature.IsHungry() &&
                creature.stats.Aggressiveness >= creature.PositionDanger(preyPos.x, preyPos.y) &&  // TODO: ajustar valor
                creature.AbleToFight())
            {
                if (creature.IsCarnivorous())
                    return true;
                else if (!creature.Plant(out _, out Vector2Int plantPos) || creature.DistanceToObjective(plantPos) > creature.DistanceToObjective(preyPos))
                    return true;
            }

            return false;
        }

        public override string ToString()
        {
            return "CombatTransition";
        }

    }
}
