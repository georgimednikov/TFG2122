//using GeneticSharp.Domain;

namespace AlgoritmoEvolutivo
{
    public static class MainClass
    {
        static void Main(string[] args)
        {
            while (true) ;
        }

        public static int Test()
        {
            GeneticSharp.Domain.Chromosomes.IntegerChromosome chromosome = new GeneticSharp.Domain.Chromosomes.IntegerChromosome(0, 1);
            return chromosome.Length;
        }
    }
}