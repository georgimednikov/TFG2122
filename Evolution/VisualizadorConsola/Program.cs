using EvolutionSimulation;

namespace VisualizadorConsola
{
    public static class Program
    {
        static void Main(string[] args)
        {
#if DEBUG
            UserInfo.SetDebugInfo();
#else
            if (!UserInfo.AskInfoUsingConsole())
                return;
#endif
            EvolutionSimulation.Genetics.CreatureChromosome.SetChromosome();
            ISimulation s = new ConsoleSimulation();
            s.Init();
            s.Run();
        }
    }
}