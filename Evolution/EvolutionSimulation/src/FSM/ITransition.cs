namespace EvolutionSimulation.FSM
{
    /// <summary>
    /// State machine transition interface
    /// </summary>
    public interface ITransition
    {
        /// <summary>
        /// Evaluates if the transition is fullfilled
        /// </summary>
        bool Evaluate();
    }
}
