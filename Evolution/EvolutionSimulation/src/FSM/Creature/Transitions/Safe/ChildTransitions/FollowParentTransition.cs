using System;

namespace EvolutionSimulation.FSM.Creature.Transitions
{
    /// <summary>
    /// Checks if the creature is a child and is far from his parent
    /// Wander -> FollowParent
    /// </summary>
    class FollowParentTransition : CreatureTransition
    {
        public FollowParentTransition(Entities.Creature creature)
        {
            this.creature = creature;
        }

        /// <summary>
        /// If the creature has the paternity ability, is a child and remember the parent position, 
        /// check if it is far from his parent
        /// </summary>
        /// <returns> True if is far from his parent </returns>
        public override bool Evaluate()
        {
            Vector3Int parentPos;
            if (!creature.stats.IsNewBorn() || !creature.Parent(out _, out parentPos) || parentPos == null)
                return false;

            if (creature.DistanceToObjective(parentPos) > UniverseParametersManager.parameters.maxDistanceToStartFollowParent
                && creature.CanReach((Entities.Creature.HeightLayer)parentPos.z))
                return true;

            return false;
        }

        public override string ToString()
        {
            return "FollowParentTransition";
        }

    }
}
