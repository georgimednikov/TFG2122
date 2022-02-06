using Stateless;
using System.Collections.Generic;

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
            src.CreatureChromosome.SetGeneRange(4, 40, 4);
            src.CreatureChromosome chromosome1 = new src.CreatureChromosome();
            src.CreatureChromosome chromosome2 = new src.CreatureChromosome();
            //chromosome.GetAttribute(src.CreatureAttribute.Strength);
            //chromosome.GetAttribute(src.CreatureAttribute.Knowledge);
            //chromosome.PrintGenes();

            src.CreatureChromosome chromosome3 = src.CrossoverFunction.UniformCrossover(chromosome1, chromosome2);
            chromosome3.PrintGenes();
            return 0;
        }
    }
}