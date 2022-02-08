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
            List<src.Gene> genes = new List<src.Gene>();
            //features that not depends on others 
            src.Gene strength = new src.Gene(src.CreatureFeatures.CreatureFeature.Strength, 100);
            src.Gene constitution = new src.Gene(src.CreatureFeatures.CreatureFeature.Constitution, 100);
            src.Gene fortitude = new src.Gene(src.CreatureFeatures.CreatureFeature.Fortitude, 100);
            src.Gene perception = new src.Gene(src.CreatureFeatures.CreatureFeature.Perception, 10);
            src.Gene aggressiveness = new src.Gene(src.CreatureFeatures.CreatureFeature.Aggressiveness, 100);
            src.Gene members = new src.Gene(src.CreatureFeatures.CreatureFeature.Members, 3);

            //features that depends on others
            src.Gene size = new src.Gene(src.CreatureFeatures.CreatureFeature.Size, 200);
            src.Gene knowledge = new src.Gene(src.CreatureFeatures.CreatureFeature.Knowledge, 100);
            src.Gene piercing  = new src.Gene(src.CreatureFeatures.CreatureFeature.Piercing, 100);
            src.Gene metabolism = new src.Gene(src.CreatureFeatures.CreatureFeature.Metabolism, 200);
            src.Gene diet = new src.Gene(src.CreatureFeatures.CreatureFeature.Diet, 3);
            src.Gene resistence = new src.Gene(src.CreatureFeatures.CreatureFeature.Resistence, 100);
            src.Gene camouflage = new src.Gene(src.CreatureFeatures.CreatureFeature.Camouflage, 100);
            src.Gene mobility = new src.Gene(src.CreatureFeatures.CreatureFeature.Mobility, 200);
            src.Gene idealTemperature = new src.Gene(src.CreatureFeatures.CreatureFeature.IdealTemperature, 100);
            src.Gene temperatureRange = new src.Gene(src.CreatureFeatures.CreatureFeature.TemperatureRange, 100);
            src.Gene longevity = new src.Gene(src.CreatureFeatures.CreatureFeature.Longevity, 100);


            strength.AddRelation(0.20f, src.CreatureFeatures.CreatureFeature.Constitution);
            strength.AddRelation(0.10f, src.CreatureFeatures.CreatureFeature.Aggressiveness);
            genes.Add(strength);
            genes.Add(constitution);
            genes.Add(fortitude);
            genes.Add(perception);
            genes.Add(aggressiveness);
            genes.Add(members);
            genes.Add(knowledge);
            genes.Add(camouflage);
            genes.Add(size);
            genes.Add(piercing);
            genes.Add(resistence);
            genes.Add(metabolism);
            genes.Add(idealTemperature);
            genes.Add(temperatureRange);
            genes.Add(longevity);
            genes.Add(diet);
            genes.Add(mobility);


            //abilities
            src.Gene Arboreal = new src.Gene(src.CreatureFeatures.CreatureFeature.Arboreal, 10);
            src.Gene Wings = new src.Gene(src.CreatureFeatures.CreatureFeature.Wings, 10);
            src.Gene Venomous = new src.Gene(src.CreatureFeatures.CreatureFeature.Venomous, 10);
            src.Gene NightVision = new src.Gene(src.CreatureFeatures.CreatureFeature.NightVision, 10);
            src.Gene Horns = new src.Gene(src.CreatureFeatures.CreatureFeature.Horns, 10);
            src.Gene Mimic = new src.Gene(src.CreatureFeatures.CreatureFeature.Mimic, 10);
            src.Gene Upright = new src.Gene(src.CreatureFeatures.CreatureFeature.Upright, 10);
            src.Gene Thorns = new src.Gene(src.CreatureFeatures.CreatureFeature.Thorns, 10);
            src.Gene Scavenger = new src.Gene(src.CreatureFeatures.CreatureFeature.Scavenger, 10);
            src.Gene Hair = new src.Gene(src.CreatureFeatures.CreatureFeature.Hair, 10);
            src.Gene Paternity = new src.Gene(src.CreatureFeatures.CreatureFeature.Paternity, 10);
            
            genes.Add(Arboreal);
            genes.Add(Wings);
            genes.Add(Venomous);
            genes.Add(NightVision);
            genes.Add(Horns);
            genes.Add(Mimic);
            genes.Add(Upright);
            genes.Add(Thorns);
            genes.Add(Scavenger);
            genes.Add(Hair);
            genes.Add(Paternity);

            //The rest of the genes IN ORDER OF DEPENDENCY


            src.CreatureChromosome.SetStructure(genes);
            return 0;
        }
    }
}