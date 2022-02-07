using Stateless;
using System;
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
            src.CreatureChromosome.SetGeneRange(
                10, //Strength
                10, //Constitution
                10, //Fortitude
                10, //Mobility
                10, //Resistence
                10, //Perception
                10, //Knowledge
                10, //Camouflage
                10, //Size
                10, //Piercing
                10, //Aggressiveness
                10, //Metabolism
                10, //BodyTemperature
                10, //Longevity
                10, //Diet
                10, //Members
                10, //Arboreal
                10, //Wings
                10, //Venomous
                10, //NightVision
                10, //Horns
                10, //Mimic
                10, //Upright
                10, //Thorns
                10, //Scavenger
                10, //Hair
                10); //Paternity
            src.CreatureChromosome chromosome1 = new src.CreatureChromosome();
            src.CreatureChromosome chromosome2 = new src.CreatureChromosome();
            src.CreatureChromosome chromosome3 = src.GeneticFunctions.UniformCrossover(chromosome1, chromosome2);
            Console.WriteLine(chromosome3.GetAttribute(src.CreatureFeatures.CreatureAttribute.Strength));
            src.GeneticFunctions.Mutation(chromosome3);
            Console.WriteLine(chromosome3.GetAttribute(src.CreatureFeatures.CreatureAttribute.Strength));
            return 0;
        }
    }
}