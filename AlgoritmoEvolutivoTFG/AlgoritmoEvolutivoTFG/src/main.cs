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
            SetChromosome();

            src.CreatureChromosome chromosome1 = new src.CreatureChromosome();
            Console.WriteLine("First Chromosome:");
            chromosome1.PrintChromosome();
            Console.WriteLine();

            src.CreatureChromosome chromosome2 = new src.CreatureChromosome();
            Console.WriteLine("Second Chromosome:");
            chromosome2.PrintChromosome();
            Console.WriteLine();

            src.CreatureChromosome chromosome3 = src.GeneticFunctions.UniformCrossover(chromosome1, chromosome2, 0.5f);
            Console.WriteLine("Crossover Result:");
            chromosome3.PrintChromosome();
            Console.WriteLine();

            src.GeneticFunctions.Mutation(chromosome3, 0.1f);
            Console.WriteLine("Child's Chromosome After Mutation:");
            chromosome3.PrintChromosome();
            Console.WriteLine();

            return 0;
        }

        private static void SetChromosome()
        {
            List<src.Gene> genes = new List<src.Gene>();

            //Base Attributes

            src.Gene strength = new src.Gene(src.CreatureFeature.Strength, 50);
            genes.Add(strength);
            src.Gene constitution = new src.Gene(src.CreatureFeature.Constitution, 50);
            genes.Add(constitution);
            src.Gene fortitude = new src.Gene(src.CreatureFeature.Fortitude, 50);
            genes.Add(fortitude);
            src.Gene perception = new src.Gene(src.CreatureFeature.Perception, 50);
            genes.Add(perception);
            src.Gene aggressiveness = new src.Gene(src.CreatureFeature.Aggressiveness, 50);
            genes.Add(aggressiveness);
            src.Gene members = new src.Gene(src.CreatureFeature.Members, 50);
            genes.Add(members);

            //The rest of the genes IN ORDER OF DEPENDENCY

            //Other Attributes

            src.Gene piercing = new src.Gene(src.CreatureFeature.Piercing, 50);
            piercing.AddRelation(0.1f, src.CreatureFeature.Strength);
            genes.Add(piercing);

            src.Gene resistence = new src.Gene(src.CreatureFeature.Resistence, 50);
            resistence.AddRelation(0.1f, src.CreatureFeature.Constitution);
            genes.Add(resistence);

            src.Gene size = new src.Gene(src.CreatureFeature.Size, 50);
            size.AddRelation(0.1f, src.CreatureFeature.Constitution);
            size.AddRelation(0.1f, src.CreatureFeature.Strength);
            genes.Add(size);

            src.Gene knowledge = new src.Gene(src.CreatureFeature.Knowledge, 50);
            knowledge.AddRelation(0.1f, src.CreatureFeature.Size);
            knowledge.AddRelation(0.1f, src.CreatureFeature.Perception);
            genes.Add(knowledge);

            src.Gene camouflage = new src.Gene(src.CreatureFeature.Camouflage, 50);
            camouflage.AddRelation(-0.1f, src.CreatureFeature.Size);
            genes.Add(camouflage);

            src.Gene metabolism = new src.Gene(src.CreatureFeature.Metabolism, 50);
            metabolism.AddRelation(0.1f, src.CreatureFeature.Size);
            genes.Add(metabolism);

            src.Gene idealTemp = new src.Gene(src.CreatureFeature.IdealTemperature, 50);
            idealTemp.AddRelation(0.1f, src.CreatureFeature.Metabolism);
            idealTemp.AddRelation(-0.1f, src.CreatureFeature.Size);
            genes.Add(idealTemp);

            src.Gene tempRange = new src.Gene(src.CreatureFeature.TemperatureRange, 50);
            tempRange.AddRelation(0.1f, src.CreatureFeature.Resistence);
            genes.Add(tempRange);

            src.Gene longevity = new src.Gene(src.CreatureFeature.Longevity, 50);
            longevity.AddRelation(-0.1f, src.CreatureFeature.Metabolism);
            genes.Add(longevity);

            src.Gene diet = new src.Gene(src.CreatureFeature.Diet, 50);
            diet.AddRelation(0.1f, src.CreatureFeature.Aggressiveness);
            genes.Add(diet);

            src.Gene mobility = new src.Gene(src.CreatureFeature.Mobility, 50);
            mobility.AddRelation(0.1f, src.CreatureFeature.Members);
            mobility.AddRelation(-0.1f, src.CreatureFeature.Size);
            mobility.AddRelation(-0.1f, src.CreatureFeature.Fortitude);
            genes.Add(mobility);

            //Abilities

            src.Gene arboreal = new src.Gene(src.CreatureFeature.Arboreal, 10);
            genes.Add(arboreal);
            src.Gene wings = new src.Gene(src.CreatureFeature.Wings, 10);
            genes.Add(wings);
            src.Gene venomous = new src.Gene(src.CreatureFeature.Venomous, 10);
            genes.Add(venomous);
            src.Gene nightvision = new src.Gene(src.CreatureFeature.NightVision, 10);
            genes.Add(nightvision);
            src.Gene horns = new src.Gene(src.CreatureFeature.Horns, 10);
            genes.Add(horns);
            src.Gene mimic = new src.Gene(src.CreatureFeature.Mimic, 10);
            genes.Add(mimic);
            src.Gene upright = new src.Gene(src.CreatureFeature.Upright, 10);
            genes.Add(upright);
            src.Gene thorns = new src.Gene(src.CreatureFeature.Thorns, 10);
            genes.Add(thorns);
            src.Gene scavenger = new src.Gene(src.CreatureFeature.Scavenger, 10);
            genes.Add(scavenger);
            src.Gene hair = new src.Gene(src.CreatureFeature.Hair, 10);
            genes.Add(hair);
            src.Gene paternity = new src.Gene(src.CreatureFeature.Paternity, 10);
            genes.Add(paternity);

            src.CreatureChromosome.SetStructure(genes);
        }
    }
}