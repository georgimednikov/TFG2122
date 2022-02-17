using EvolutionSimulation.Genetics;
using System;

namespace EvolutionSimulation.Entities.Status
{
    /// <summary>
    /// Interface detailing methods a status effect must have
    /// </summary>
    public abstract class Status : IStatus
    {
        // Maximum duration of the effect IN TICKS
        // Negative values imply permanent duration
        int duration;
        int span;   // Time elapsed with the effect active

        // Creature who is under this status effect
        protected Creature owner;

        public Status(int d)
        {
            duration = d;
            span = 0;
        } 

        public void setOwner(Creature c)
        {
            owner = c;
        }

        public virtual bool OnTick() 
        {
            if(duration < 0) return false;
            else return ++span >= duration;
        }

        public virtual void OnApply()
        {
        }

        public virtual void OnRemove()
        {
        }

        public virtual void OnExpire()
        {
        }
    }
}
