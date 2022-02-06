using Stateless;

namespace EvolutionSimulation
{
    public static class MainClass
    {
        static void Main(string[] args)
        {
            while (true) ;
        }

        public static int Test()
        {
            //GeneticSharp.Domain.Chromosomes.IntegerChromosome chromosome = new GeneticSharp.Domain.Chromosomes.IntegerChromosome(0, 1);
            src.CreatureChromosome.SetGeneSize(10, 10, 10);
            src.CreatureChromosome chromosome = new src.CreatureChromosome();
            chromosome.GetAttribute(src.CreatureAttribute.Strength);
            chromosome.GetFeature((int)src.CreatureAttribute.Knowledge);
            return chromosome.Length;
        }
    }
}