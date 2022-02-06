using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EvolutionSimulation.src
{
    /// <summary>
    /// Class that receive 2 chromosomes and use UniformCrossover to cross them
    /// UniformCrossover means that all the bits in both chromosomes have the same
    /// probability to be selected (50%)
    /// </summary>
    public class CrossoverFunction
    {
        static public CreatureChromosome UniformCrossover(CreatureChromosome male, CreatureChromosome female)
        {
            Random rnd = new Random();

            BitArray mc = male.GetChromosome();
            BitArray fc = female.GetChromosome();
            BitArray cc = new BitArray(mc.Length);

            for (int i = 0; i < mc.Length; ++i)
            {
                cc[i] = rnd.Next(0, 2) == 0 ? mc[i] : fc[i];
            }
            

            return new CreatureChromosome(cc); //Child Chromosome
        }
    }
}
