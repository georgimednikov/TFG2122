using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace EvolutionSimulation.src
{
    /// <summary>
    /// Contains the information of a gene: its maximum value and the other genes
    /// it is related to as well as the percentage of the relation
    /// </summary>
    public class Gene
    {
        public int maxValue;
        public CreatureFeatures.CreatureFeature feature;
        public List<Tuple<float, CreatureFeatures.CreatureFeature>> relations { get; }

        public Gene(CreatureFeatures.CreatureFeature feat, int max)
        {
            maxValue = max;
            feature = feat;
            relations = new List<Tuple<float, CreatureFeatures.CreatureFeature>>();
        }

        public void AddRelation(float percentage, CreatureFeatures.CreatureFeature relation)
        {
            relations.Add(new Tuple<float, CreatureFeatures.CreatureFeature>(percentage, relation));
        }
    }







    public class CreatureChromosome
    {
        /// <summary>
        /// If the chromosome has been given initial values or not
        /// </summary>
        static private bool init = false;
        static private int chromosomeSize;

        /// <summary>
        /// The max value of each gene
        /// </summary>
        static private int[] geneMaxValues;
        /// <summary>
        /// First: First bit of each gene
        /// Second: The length of the gene
        /// </summary>
        static private Tuple<int, int>[] geneInfo;
        /// <summary>
        /// The different relations and their percentages that each gene has with others
        /// </summary>
        static Tuple<float, CreatureFeatures.CreatureFeature>[][] geneRelations;

        static private Random rnd;


        /// <summary>
        /// Chromosome in bits and its amount
        /// </summary>
        BitArray chromosome;
        /// <summary>
        /// The current value of each gene
        /// </summary>
        private int[] geneValues;


        /// <summary>
        /// Sets the internal extructure of the chromosome given the genes and their respectives max values
        /// </summary>
        public static void SetStructure(List<Gene> genes)
        {
            if (genes.Count != CreatureFeatures.Features)
                throw new Exception("The number of genes and max values must be the same as the total features");
            rnd = new Random();

            for (int i = 0; i < genes.Count; ++i)
            {
                geneMaxValues[i] = genes[i].maxValue;
            }
            geneInfo = new Tuple<int, int>[genes.Count];
            chromosomeSize = 0;
            int relationsMaxValue = 0;
            foreach (Gene gene in genes)
            {
                //The numeric value of the feature
                int featureIndex = (int)gene.feature;

                //The relations are saved for later use
                geneRelations[featureIndex] = gene.relations.ToArray();
                
                foreach (Tuple<float, CreatureFeatures.CreatureFeature> rel in gene.relations)
                {
                    //The highest possible value of the features related is calculated (percentaje * maxValue)
                    relationsMaxValue += (int)(rel.Item1 * geneMaxValues[(int)rel.Item2]); //The percetaje gets truncated!!!
                }

                int leftover = geneMaxValues[featureIndex] - relationsMaxValue;
                if (leftover <= 0)
                    throw new Exception("The genes must not depend completely on other genes and must have percetage values in their relations between 0 and 1");

                //The lowest power of 2 needed to store the remaining values after substracting the relations
                int bitsNeeded = (int)Math.Log(leftover, 2) + 1;// the min number of bites to store the range;
                geneInfo[featureIndex] = new Tuple<int, int>(chromosomeSize, bitsNeeded); //Start and length in bits of the feature
                chromosomeSize += bitsNeeded;
            }

            init = true;
        }





        /// <summary>
        /// Creates a new, random chromosome.
        /// Must be initilized with SetGeneRange
        /// </summary>
        public CreatureChromosome()
        {
            chromosome = new BitArray(chromosomeSize);
            for (int i = 0; i < chromosomeSize; ++i)
                chromosome[i] = rnd.Next(0, 2) == 0;
            SetFeatures();
        }

        /// <summary>
        /// Creates a chromosome given the genes
        /// Must be initilized with SetGeneRange
        /// </summary>
        public CreatureChromosome(BitArray chromosome)
        {
            if (chromosome.Length != chromosomeSize)
                throw new Exception("The given chromosome must have the right size");
            this.chromosome = chromosome;
            SetFeatures();
        }

        /// <summary>
        /// Returns the chromosome. Returns an empty array if not initialized.
        /// </summary>
        public BitArray GetChromosome()
        {
            if (!init) throw new Exception("The chromosome was not initialized");
            return chromosome;
        }

        /// <summary>
        /// Given the chromosome values, calculates the values of the features
        /// </summary>
        private void SetFeatures()
        {
            geneValues = Enumerable.Repeat(-1, geneInfo.Length).ToArray();
            //For each feature (gene)
            for (int feat = 0; feat < CreatureFeatures.Features; ++feat)
            {
                int startPoint = geneInfo[feat].Item1;
                int total = 0;
                //The amount of its exclusive bits gets counted
                for (int j = startPoint; j < startPoint + geneInfo[feat].Item2; ++j)
                {
                    if (chromosome[j]) ++total; //If the bit is 1 then the total count is aumented
                }
                foreach (Tuple<float, CreatureFeatures.CreatureFeature> relation in geneRelations[feat])
                {
                    if (geneValues[(int)relation.Item2] < 0)
                        throw new Exception("The genes must have been passed in order of dependency of relations");
                    //total += percentage of usage of the related gene * the value of the gene
                    total += (int)(relation.Item1 * geneValues[(int)relation.Item2]);
                }
                geneValues[feat] = total;
            }
        }

        /// <summary>
        /// Get the numeric value of the desired feature (attribute or ability)
        /// Returns -1 if not initilized or the value asked does not exist
        /// </summary>
        public int GetFeature(CreatureFeatures.CreatureFeature feat)
        {
            if (!init) throw new Exception("The chromosome was not initialized");
            return geneValues[(int)feat];
        }



        //private int GetFeatureWithinConfines(int value, int feature)
        //{
        //    int bits = (int)Math.Log(geneMaxValues[feature], 2) + 1;// the min number of bites to store the range
        //    return (int)(value / Math.Pow(2, bits) * geneMaxValues[feature]);
        //}

        ///// <summary>
        ///// Dados un número de posiciones en el chromosoma
        ///// se cogen los bits en el orden dado y se convierten a entero
        ///// </summary>
        //private int CombineBits(params int[] bits)
        //{
        //    BitArray result = new BitArray(bits.Length);
        //    for (int i = 0; i < bits.Length; ++i)
        //    {
        //        result[i] = chromosome[i];
        //    }
        //    return BinaryToInt(result);
        //}

        ///// <summary>
        ///// Parses from binary to int
        ///// </summary>
        //private int BinaryToInt(BitArray bits)
        //{
        //    //An int array with a single value is created, and then the binary
        //    //content of value is copies in it, so when the first and only
        //    //element is accessed, the information is parsed into int
        //    int[] array = new int[1];
        //    bits.CopyTo(array, 0);
        //    return array[0];
        //}
    }
}
