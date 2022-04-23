namespace EvolutionSimulation.FSM.Creature.Transitions
{
    /// <summary>
    /// Checks if the creature is close to his parent
    /// FollowParent -> Wander
    /// </summary>
    class StopFollowParentTransition : CreatureTransition
    {
        public StopFollowParentTransition(Entities.Creature creature)
        {
            this.creature = creature;
        }

        /// <summary>
        /// Check if the creature is not a child or is close to his parent
        /// </summary>
        /// <returns> True if is close to his parent </returns>
        public override bool Evaluate()
        {
            Vector3Int parentPos;
            //TODO que la distancia a dejar de seguir al padre dependa de algo (nivel de paternity, peligro...)
            if (!creature.stats.IsNewBorn() || !creature.Parent(out _, out parentPos) || parentPos == null
                || creature.DistanceToObjective(parentPos) < 3)
                return true;

            return false;
        }

        public override string ToString()
        {
            return "StopFollowParentTransition";
        }

    }
}
