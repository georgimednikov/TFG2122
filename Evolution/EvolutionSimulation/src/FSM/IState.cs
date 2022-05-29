namespace EvolutionSimulation.FSM
{
    /// <summary>
    /// State machine state interface
    /// </summary>
    public interface IState 
    {
        /// <summary>
        /// Returns if the state can perform 
        /// its action with the FSm's current AP.
        /// </summary>
        int GetCost();

        /// <summary>
        /// Action which is executed in the state
        /// </summary>
        void Action();

        /// <summary>
        /// Action executed when it is transitioned into
        /// </summary>
        void OnEntry();

        /// <summary>
        /// Action executed when the state transitions to other
        /// </summary>
        void OnExit();

        /// <summary>
        /// Additional information about this state
        /// </summary>
        string GetInfo();
    }
}
