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
            if (!creature.stats.IsNewBorn() || !creature.Parent(out _, out parentPos) || parentPos == null
                || creature.DistanceToObjective(parentPos) < UniverseParametersManager.parameters.maxDistanceToStopFollowParent
                || !creature.CanReach((Entities.Creature.HeightLayer)parentPos.z))
                return true;

            return false;
        }

        public override string ToString()
        {
            return "StopFollowParentTransition";
        }

    }
}
