using System;
using System.Numerics;
using System.Collections;

namespace EvolutionSimulation.src
{
    public class CreatureChromosome
    {
        /// <summary>
        /// If the chromosome has been given initial values or not
        /// </summary>
        static private bool init;

        /// <summary>
        /// Chromosome in bits and its amount
        /// </summary>
        BitArray chromosome; 
        static private int length;

        /// <summary>
        /// Length and start index of the genes in the Choromosome
        /// X = num of bites of the gene
        /// Y = start on the chromosome's array
        /// </summary>
        static private Vector2[] geneInfo;
        /// <summary>
        /// The max value of each gene
        /// </summary>
        static private int[] geneMaxValues;

        static private Random rnd;

        /// <summary>
        /// Number of genes in the chromosome and their values
        /// </summary>
        private int numGenes;
        private int[] geneValues;




        //TODO mover estos metodos
        /// <summary>
        /// Parses from binary to int
        /// </summary>
        static private int BinaryToInt(BitArray bits)
        {
            //An int array with a single value is created, and then the binary
            //content of value is copies in it, so when the first and only
            //element is accessed, the information is parsed into int
            int[] array = new int[1];
            bits.CopyTo(array, 0);
            return array[0];
        }

        /// <summary>
        /// Parses from int to binary
        /// </summary>
        static private BitArray IntToBinary(int num)
        {
            return new BitArray(new int[] { num });
        }

        /// <summary>
        /// Sets the max value (inclusively) of the different genes in the chromosome
        /// </summary>
        public static void SetGeneRange(params int[] genes)
        {
            geneMaxValues = genes;
            geneInfo = new Vector2[genes.Length];
           
            length = 0;
            int geneStart = 0;
            for (int i = 0; i < genes.Length; ++i)
            {
                geneInfo[i].X = (int)Math.Log(geneMaxValues[i],2) + 1;// the min number of bites to store the range
                geneInfo[i].Y = geneStart;
                geneStart += (int)geneInfo[i].X;
                length += (int)geneInfo[i].X;
            }
            rnd = new Random();
            init = true;
        }

        public static bool IsGeneValid(int geneIndex, BitArray gene)
        {
            return BinaryToInt(gene) > geneMaxValues[geneIndex];
        }





        /// <summary>
        /// Creates a new, random chromosome.
        /// Must be initilized with SetGeneRange
        /// </summary>
        public CreatureChromosome() //Number of genes, not bits
        {
            numGenes = geneInfo.Length;
            chromosome = new BitArray(length);
            geneValues = new int[numGenes];
            GenerateGenes();
        }

        /// <summary>
        /// Creates a chromosome given the genes
        /// Must be initilized with SetGeneRange
        /// </summary>
        public CreatureChromosome(BitArray chromosome)
        {
            numGenes = geneInfo.Length;
            this.chromosome = chromosome;
            geneValues = new int[numGenes];
            AssignValues();
        }

        /// <summary>
        /// Returns the desired gene in bits.
        /// </summary>
        public BitArray GetGene(int geneIndex)
        {
            if (!init || geneIndex >= geneInfo.Length || geneIndex < 0) return new BitArray(0);

            //Goes to the gene's position in the chromosome and copies the values to a new
            //BitArray with the pertinent size
            BitArray gene = new BitArray((int)geneInfo[geneIndex].X);
            int endIndex = gene.Length + (int)geneInfo[geneIndex].Y;
            int cont = 0;
            for (int i = (int)geneInfo[geneIndex].Y; i < endIndex; ++i, ++cont)
            {
                gene[cont] = chromosome[i];
            }
            return gene;
        }

        /// <summary>
        /// Returns the bit's value at position "bitIndex" in gene "geneIndex".
        /// Returns -1 if chromosome is not initialized or gene does not exist
        /// </summary>
        public int GetGeneBit(int geneIndex, int bitIndex)
        {
            if (!init) return -1;
            if (geneIndex < 0 || geneIndex >= numGenes) return -1;
            if (bitIndex < 0 || bitIndex >= length) return -1;
            return chromosome.Get((int)geneInfo[geneIndex].X + bitIndex) ? 1 : 0;
        }

        /// <summary>
        /// Returns the chromosome. Returns an empty array if not initialized.
        /// </summary>
        public BitArray GetChromosome()
        {
            if (!init) return new BitArray(0);
            return chromosome;
        }

        /// <summary>
        /// Get the numeric value of the desired attribute
        /// Returns -1 if not initilized or the value asked does not exist
        /// </summary>
        public int GetAttribute(CreatureAttribute attr)
        {
            return GetFeature((int)attr);
        }

        /// <summary>
        /// Get the numeric value of the desired ability.
        /// Returns -1 if not initilized or the value asked does not exist
        /// </summary>
        public int GetAbilities(CreatureAbility ab)
        {
            return GetFeature((int)ab);
        }

        /// <summary>
        /// Prints the chromosome. Debugging purposes
        /// </summary>
        public void PrintGenes()
        {
            if (!init)
            {
                Console.WriteLine("Invalid chromosome, is not initialized");
                return;
            }
            for (int i = 0; i < numGenes; ++i)
            {
                Console.WriteLine("Gene " + i + ": " + GetFeature(i));
            }
            for (int i = 0; i < numGenes; ++i)
            {
                for (int j = (int)geneInfo[i].X - 1; j >= 0; --j)
                    Console.Write(chromosome[(int)geneInfo[i].Y + j] ? 1 : 0);
                Console.Write(" ");
            }
            Console.WriteLine();
        }




        /// <summary>
        /// Given a chromosome, the values of the genes are assigned
        /// </summary>
        private void AssignValues()
        {
            for (int i = 0; i < numGenes; ++i)
            {
                BitArray value = new BitArray((int)geneInfo[i].X);
                for(int j = 0; j < value.Length; ++j)
                {
                    value[j] = chromosome[(int)geneInfo[i].Y + j];
                }
                geneValues[i] = BinaryToInt(value);
            }
        }


        /// <summary>
        /// Generates random values from 0 to the max value of each gene
        /// </summary>
        private void GenerateGenes()
        {
            for(int i = 0; i < numGenes; ++i)
            {
                GenerateGene(i);
            }
        }

        /// <summary>
        /// Generates a random value for the gene in position "geneIndex"
        /// </summary>
        private void GenerateGene(int geneIndex)
        {
            var value = rnd.Next(0, geneMaxValues[geneIndex] + 1);//+ 1 to be inclusive
            geneValues[geneIndex] = value;

            Vector2 gene = geneInfo[geneIndex];
            BitArray bits = IntToBinary(value);
            //Copy the new bits to the Chromosome
            for (int j = 0; j < gene.X; ++j)
            {
                chromosome[(int)gene.Y + j] = bits[j];
            }
        }

        private int GetFeature(int feature)
        {
            if (!init || feature >= numGenes || feature < 0)
                return -1;

            return geneValues[feature];
        }
    }
}
