﻿using System;

namespace EvolutionSimulation.Entities
{
    /// <summary>
    /// Plant generated by the map's flora.
    /// Provides sustenance and physicial interaction
    /// </summary>
    public abstract class Plant : StableEntity
    {
        public int x { get; private set; }
        public int y { get; private set; }

        virtual public void Tick()
        {
            throw new NotImplementedException();
        }
    }
}