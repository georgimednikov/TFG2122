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
        /// If the creature has the paternity ability and is a child, 
        /// check if it is far from his parent
        /// </summary>
        /// <returns> True if is far from his parent </returns>
        public override bool Evaluate()
        {
            if (!creature.stats.IsNewBorn() || creature.parentToFollow == null || creature.GetParentToFollowPosition() == null)
                return false;

            //TODO que la distancia a empezar a seguir al padre dependa de algo (nivel de paternity, peligro...)
            if (creature.DistanceToObjective(creature.parentToFollow) > 10)
                return true;

            return false;
        }

        public override string ToString()
        {
            return "FollowParentTransition";
        }

    }
}
