//using GeneticSharp.Domain;
using Stateless;

namespace AlgoritmoEvolutivo
{
    public static class MainClass
    {
        static void Main(string[] args)
        {
            Simulation s = new Simulation();
            s.Init();
            s.Run();
        }

        public static int Test()
        {
            GeneticSharp.Domain.Chromosomes.IntegerChromosome chromosome = new GeneticSharp.Domain.Chromosomes.IntegerChromosome(0, 1);
            return chromosome.Length;
        }
    }
}