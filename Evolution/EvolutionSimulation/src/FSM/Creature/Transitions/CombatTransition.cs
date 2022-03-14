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
            // If it has not been attacked and has no creature to hunt it does not engage in combat.
            if (creature.GetPreyPosition() == null && !creature.HasBeenAttacked()) return false;

            // Else if attacked it considers its nearby allies in combat.
            if (creature.HasBeenAttacked() &&
                (creature.AbleToFight() ||   // So it does not immediately return to combat while fleeing
                creature.cornered))    // So it fights as a last resort when fleeing
                return true;

            // Else it is trying to hunt
            if (creature.stats.Diet != Genetics.Diet.Herbivore && creature.GetPreyPosition() != null && creature.IsHungry() &&
                creature.stats.Aggressiveness >= creature.GetDanger(creature.GetPreyPosition().x, creature.GetPreyPosition().y) &&  // TODO: ajustar valor
                creature.AbleToFight())
            {
                if (creature.stats.Diet != Genetics.Diet.Carnivore)
                    return true;

                else if (creature.GetFruitPosition() == null || creature.DistanceToObjective(creature.GetFruitPosition()) > creature.DistanceToObjective(creature.GetPreyPosition()))
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
