using EvolutionSimulation;

namespace VisualizadorConsola
{
    public static class Program
    {
        static void Main(string[] args)
        {
            WorkingDirectories.SetDirectories();
            EvolutionSimulation.Genetics.CreatureChromosome.SetChromosome();
            ISimulation s = new ConsoleSimulation();
            s.Init();
            s.Run();
        }
    }
}