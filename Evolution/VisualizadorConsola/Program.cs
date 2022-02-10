using EvolutionSimulation;
using System;
using EvolutionSimulation.Genetics;
using System.Collections.Generic;

namespace VisualizadorConsola
{
    public static class Program
    {
        static void Main(string[] args)
        {
            CreatureChromosome.SetChromosome();
            ISimulation s = new ConsoleSimulation();
            s.Init();
            s.Run();
        }
    }
}