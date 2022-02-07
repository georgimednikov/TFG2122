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

            src.Gene strength = new src.Gene(src.CreatureFeatures.CreatureFeature.Strength, 50);
            strength.AddRelation(0.20f, src.CreatureFeatures.CreatureFeature.Constitution);
            strength.AddRelation(0.10f, src.CreatureFeatures.CreatureFeature.Aggressiveness);
            genes.Add(strength);


            //The rest of the genes IN ORDER OF DEPENDENCY


            src.CreatureChromosome.SetStructure(genes);
            return 0;
        }
    }
}