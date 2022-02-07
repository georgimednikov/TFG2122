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
            BitArray childChromosome = new BitArray(0);
            BitArray childGene;
            BitArray maleFeature, femaleFeature;
            int features = (int)CreatureAbility.Count - 2; //TODO:CAMBIAR
            for (int i = 0; i < features; ++i)
            {
                maleFeature = male.GetGene(i);
                femaleFeature = female.GetGene(i);
                childGene = new BitArray(maleFeature.Length);
                do
                {
                    for (int j = 0; j < maleFeature.Length; ++j)
                    {
                        childGene[j] = rnd.Next(0, 2) == 0 ? maleFeature[j] : femaleFeature[j];
                    }
                }
                while (CreatureChromosome.IsGeneValid(i, childGene));

                //Concatenamos
                childChromosome = ConcatenateBits(childChromosome, childGene);
            }

            return new CreatureChromosome(childChromosome); //Child Chromosome
        }

        static private BitArray ConcatenateBits(BitArray original, BitArray expansion)
        {
            BitArray result = new BitArray(original.Length + expansion.Length);
            int index = 0;
            for (; index < original.Length; ++index)
            {
                result[index] = original[index];
            }
            for (int i = 0; index < result.Length; ++index, ++i)
            {
                result[index] = expansion[i];
            }
            return result;
        }
    }
}
