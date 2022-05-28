namespace UnitySimulation
{
    // Interfaces to implement the Listener Pattern

    /// <summary>
    /// Subject that provides information to the subscribers
    /// when an event happens
    /// </summary>
    public interface ISubject<T>
    {
        /// <summary>
        /// Adds a subscriber.
        /// If the subscription fails, it returns false
        /// </summary>
        bool Subscribe(IListener<T> listener);

        /// <summary>
        /// Removes a subscriber.
        /// If the unsubscription fails, it returns false
        /// </summary>
        bool Unsubscribe(IListener<T> listener);
    }

    /// <summary>
    /// Object that can be subscribed to a Subject to 
    /// be notified about the information of the Subject events
    /// </summary>
    public interface IListener<T>
    {
        void OnNotify(T info);
    }

}
