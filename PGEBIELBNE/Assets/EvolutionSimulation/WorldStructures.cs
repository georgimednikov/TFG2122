using System;

namespace EvolutionSimulation.Unity
{
    public interface IEntity
    {
        void Tick();

        int x { get; }

        int y { get; }
    }

    public abstract class Plant : IEntity
    {
        public int x { get; private set; }
        public int y { get; private set; }

        virtual public void Tick()
        {
            throw new NotImplementedException();
        }
    }

    public struct MapData
    {
        public double height, humidity, temperature, flora;
        public Plant plant;
    }
}