using System;
namespace EvolutionSimulation.FSM.Creature.Transitions
{
    /// <summary>
    /// Checks if the creature is close to the safe objective
    /// </summary>
    class GoToSafePlaceTransition : CreatureTransition
    {
        public GoToSafePlaceTransition(Entities.Creature creature)
        {
            this.creature = creature;
        }

        public override bool Evaluate()
        {
            //TODO que el objetivo sea el de un sitio seguro
            return creature.objective != null
               && Math.Abs(creature.objective.x - creature.x) < 1
               && Math.Abs(creature.objective.y - creature.y) < 1;
        }

        public override string ToString()
        {
            return "GoToSafePlaceTransition";
        }

    }
}
