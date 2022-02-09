﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EvolutionSimulation.FSM
{
    /// <summary>
    /// State machine state interface
    /// </summary>
    public interface IState 
    {
        /// <summary>
        /// Action which is executed in the state
        /// </summary>
        bool Action();
    }

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
