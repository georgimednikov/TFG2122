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
        static private bool init = false;

        /// <summary>
        /// Chromosome in bits and its amount
        /// </summary>
        BitArray chromosome;
        private int chromosomeSize = CreatureFeatures.Features * 4;

        /// <summary>
        /// The max value of each gene
        /// </summary>
        static private int[] geneMaxValues;

        static private Random rnd;




        /// <summary>
        /// Sets the max value (inclusively) of the different genes in the chromosome
        /// </summary>
        public static void SetGeneRange(params int[] genes)
        {
            geneMaxValues = genes;
            rnd = new Random();
            init = true;
        }





        /// <summary>
        /// Creates a new, random chromosome.
        /// Must be initilized with SetGeneRange
        /// </summary>
        public CreatureChromosome() //Number of genes, not bits
        {
            chromosome = new BitArray(chromosomeSize);
            for (int i = 0; i < chromosomeSize; ++i)
                chromosome[i] = rnd.Next(0, 2) == 0;
        }

        /// <summary>
        /// Creates a chromosome given the genes
        /// Must be initilized with SetGeneRange
        /// </summary>
        public CreatureChromosome(BitArray chromosome)
        {
            this.chromosome = chromosome;
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
        /// Get the numeric value of the desired attribute
        /// Returns -1 if not initilized or the value asked does not exist
        /// </summary>
        public int GetAttribute(CreatureFeatures.CreatureAttribute attr)
        {
            if (!init) throw new Exception("The chromosome was not initialized");
            switch (attr)
            {
                case CreatureFeatures.CreatureAttribute.Strength:
                    return GetStrength();
                case CreatureFeatures.CreatureAttribute.Constitution:
                    return GetConstitution();
                case CreatureFeatures.CreatureAttribute.Fortitude:
                    return GetFortitude();
                case CreatureFeatures.CreatureAttribute.Mobility:
                    return GetMobility();
                case CreatureFeatures.CreatureAttribute.Resistence:
                    return GetResistence();
                case CreatureFeatures.CreatureAttribute.Perception:
                    return GetPerception();
                case CreatureFeatures.CreatureAttribute.Knowledge:
                    return GetKnowledge();
                case CreatureFeatures.CreatureAttribute.Camouflage:
                    return GetCamouflage();
                case CreatureFeatures.CreatureAttribute.Size:
                    return GetSize();
                case CreatureFeatures.CreatureAttribute.Piercing:
                    return GetPiercing();
                case CreatureFeatures.CreatureAttribute.Aggressiveness:
                    return GetAggressiveness();
                case CreatureFeatures.CreatureAttribute.Metabolism:
                    return GetMetabolism();
                case CreatureFeatures.CreatureAttribute.BodyTemperature:
                    return GetBodyTemperature();
                case CreatureFeatures.CreatureAttribute.Longevity:
                    return GetLongevity();
                case CreatureFeatures.CreatureAttribute.Diet:
                    return GetDiet();
                case CreatureFeatures.CreatureAttribute.Members:
                    return GetMembers();
                default:
                    return -1;
            }
        }

        /// <summary>
        /// Get the numeric value of the desired ability.
        /// Returns -1 if not initilized or the value asked does not exist
        /// </summary>
        public int GetAbilities(CreatureFeatures.CreatureAbility ab)
        {
            if (!init) throw new Exception("The chromosome was not initialized");
            switch (ab)
            {
                case CreatureFeatures.CreatureAbility.Arboreal:
                    return GetArboreal();
                case CreatureFeatures.CreatureAbility.Wings:
                    return GetWings();
                case CreatureFeatures.CreatureAbility.Venomous:
                    return GetVenomous();
                case CreatureFeatures.CreatureAbility.NightVision:
                    return GetNightVision();
                case CreatureFeatures.CreatureAbility.Horns:
                    return GetHorns();
                case CreatureFeatures.CreatureAbility.Mimic:
                    return GetMimic();
                case CreatureFeatures.CreatureAbility.Upright:
                    return GetUpright();
                case CreatureFeatures.CreatureAbility.Thorns:
                    return GetThorns();
                case CreatureFeatures.CreatureAbility.Scavenger:
                    return GetScavenger();
                case CreatureFeatures.CreatureAbility.Hair:
                    return GetHair();
                case CreatureFeatures.CreatureAbility.Paternity:
                    return GetPaternity();
                default:
                    return -1;
            }
        }

        private int GetStrength()
        {
            int value = CombineBits(0, 1, 2, 3);
            return GetFeatureWithinConfines(value, (int)CreatureFeatures.CreatureAbility.Paternity);
        }
        private int GetConstitution()
        {
            int value = CombineBits(0, 1, 2, 3);
            return GetFeatureWithinConfines(value, (int)CreatureFeatures.CreatureAbility.Paternity);
        }
        private int GetFortitude()
        {
            int value = CombineBits(0, 1, 2, 3);
            return GetFeatureWithinConfines(value, (int)CreatureFeatures.CreatureAbility.Paternity);
        }
        private int GetMobility()
        {
            int value = CombineBits(0, 1, 2, 3);
            return GetFeatureWithinConfines(value, (int)CreatureFeatures.CreatureAbility.Paternity);
        }
        private int GetResistence()
        {
            int value = CombineBits(0, 1, 2, 3);
            return GetFeatureWithinConfines(value, (int)CreatureFeatures.CreatureAbility.Paternity);
        }
        private int GetPerception()
        {
            int value = CombineBits(0, 1, 2, 3);
            return GetFeatureWithinConfines(value, (int)CreatureFeatures.CreatureAbility.Paternity);
        }
        private int GetKnowledge()
        {
            int value = CombineBits(0, 1, 2, 3);
            return GetFeatureWithinConfines(value, (int)CreatureFeatures.CreatureAbility.Paternity);
        }
        private int GetCamouflage()
        {
            int value = CombineBits(0, 1, 2, 3);
            return GetFeatureWithinConfines(value, (int)CreatureFeatures.CreatureAbility.Paternity);
        }
        private int GetSize()
        {
            int value = CombineBits(0, 1, 2, 3);
            return GetFeatureWithinConfines(value, (int)CreatureFeatures.CreatureAbility.Paternity);
        }
        private int GetPiercing()
        {
            int value = CombineBits(0, 1, 2, 3);
            return GetFeatureWithinConfines(value, (int)CreatureFeatures.CreatureAbility.Paternity);
        }
        private int GetAggressiveness()
        {
            int value = CombineBits(0, 1, 2, 3);
            return GetFeatureWithinConfines(value, (int)CreatureFeatures.CreatureAbility.Paternity);
        }
        private int GetMetabolism()
        {
            int value = CombineBits(0, 1, 2, 3);
            return GetFeatureWithinConfines(value, (int)CreatureFeatures.CreatureAbility.Paternity);
        }
        private int GetBodyTemperature()
        {
            int value = CombineBits(0, 1, 2, 3);
            return GetFeatureWithinConfines(value, (int)CreatureFeatures.CreatureAbility.Paternity);
        }
        private int GetLongevity()
        {
            int value = CombineBits(0, 1, 2, 3);
            return GetFeatureWithinConfines(value, (int)CreatureFeatures.CreatureAbility.Paternity);
        }
        private int GetDiet()
        {
            int value = CombineBits(0, 1, 2, 3);
            return GetFeatureWithinConfines(value, (int)CreatureFeatures.CreatureAbility.Paternity);
        }
        private int GetMembers()
        {
            int value = CombineBits(0, 1, 2, 3);
            return GetFeatureWithinConfines(value, (int)CreatureFeatures.CreatureAbility.Paternity);
        }
        private int GetArboreal()
        {
            int value = CombineBits(0, 1, 2, 3);
            return GetFeatureWithinConfines(value, (int)CreatureFeatures.CreatureAbility.Paternity);
        }
        private int GetWings()
        {
            int value = CombineBits(0, 1, 2, 3);
            return GetFeatureWithinConfines(value, (int)CreatureFeatures.CreatureAbility.Paternity);
        }
        private int GetVenomous()
        {
            int value = CombineBits(0, 1, 2, 3);
            return GetFeatureWithinConfines(value, (int)CreatureFeatures.CreatureAbility.Paternity);
        }
        private int GetNightVision()
        {
            int value = CombineBits(0, 1, 2, 3);
            return GetFeatureWithinConfines(value, (int)CreatureFeatures.CreatureAbility.Paternity);
        }
        private int GetHorns()
        {
            int value = CombineBits(0, 1, 2, 3);
            return GetFeatureWithinConfines(value, (int)CreatureFeatures.CreatureAbility.Paternity);
        }
        private int GetMimic()
        {
            int value = CombineBits(0, 1, 2, 3);
            return GetFeatureWithinConfines(value, (int)CreatureFeatures.CreatureAbility.Paternity);
        }
        private int GetUpright()
        {
            int value = CombineBits(0, 1, 2, 3);
            return GetFeatureWithinConfines(value, (int)CreatureFeatures.CreatureAbility.Paternity);
        }
        private int GetThorns()
        {
            int value = CombineBits(0, 1, 2, 3);
            return GetFeatureWithinConfines(value, (int)CreatureFeatures.CreatureAbility.Paternity);
        }
        private int GetScavenger()
        {
            int value = CombineBits(0, 1, 2, 3);
            return GetFeatureWithinConfines(value, (int)CreatureFeatures.CreatureAbility.Paternity);
        }
        private int GetHair()
        {
            int value = CombineBits(0, 1, 2, 3);
            return GetFeatureWithinConfines(value, (int)CreatureFeatures.CreatureAbility.Paternity);
        }
        private int GetPaternity()
        {
            int value = CombineBits(0, 1, 2, 3);
            return GetFeatureWithinConfines(value, (int)CreatureFeatures.CreatureAbility.Paternity);
        }

        private int GetFeatureWithinConfines(int value, int feature)
        {
            int bits = (int)Math.Log(geneMaxValues[feature], 2) + 1;// the min number of bites to store the range
            return (int)(value / Math.Pow(2, bits) * geneMaxValues[feature]);
        }

        /// <summary>
        /// Dados un número de posiciones en el chromosoma
        /// se cogen los bits en el orden dado y se convierten a entero
        /// </summary>
        private int CombineBits(params int[] bits)
        {
            BitArray result = new BitArray(bits.Length);
            for (int i = 0; i < bits.Length; ++i)
            {
                result[i] = chromosome[i];
            }
            return BinaryToInt(result);
        }

        /// <summary>
        /// Parses from binary to int
        /// </summary>
        private int BinaryToInt(BitArray bits)
        {
            //An int array with a single value is created, and then the binary
            //content of value is copies in it, so when the first and only
            //element is accessed, the information is parsed into int
            int[] array = new int[1];
            bits.CopyTo(array, 0);
            return array[0];
        }
    }
}
