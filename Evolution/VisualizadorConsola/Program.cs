using EvolutionSimulation;
using System;
using EvolutionSimulation.Genetics;
using System.Collections.Generic;

namespace VisualizadorConsola
{
    public static class Program
    {
        static void Main(string[] args)
        {
            ISimulation s = new ConsoleSimulation();
            s.Init();
            s.Run();
        }

        //NO BORRAR HASTA QUE SE COPIE A OTRO LADO
        static private void SetChromosome()
        {
            List<Gene> genes = new List<Gene>();

            //Base Attributes

            Gene strength = new Gene(CreatureFeature.Strength, 50);
            genes.Add(strength);
            Gene constitution = new Gene(CreatureFeature.Constitution, 100);
            genes.Add(constitution);
            Gene fortitude = new Gene(CreatureFeature.Fortitude, 25);
            genes.Add(fortitude);
            Gene perception = new Gene(CreatureFeature.Perception, 50);
            genes.Add(perception);
            Gene aggressiveness = new Gene(CreatureFeature.Aggressiveness, 50);
            genes.Add(aggressiveness);
            Gene members = new Gene(CreatureFeature.Members, 10);
            genes.Add(members);

            //The rest of the genes IN ORDER OF DEPENDENCY

            //Other Attributes

            Gene piercing = new Gene(CreatureFeature.Piercing, 25);
            piercing.AddRelation(0.4f, CreatureFeature.Strength);
            genes.Add(piercing);

            Gene resistence = new Gene(CreatureFeature.Resistence, 50);
            resistence.AddRelation(0.25f, CreatureFeature.Constitution);
            genes.Add(resistence);

            Gene size = new Gene(CreatureFeature.Size, 50);
            size.AddRelation(0.4f, CreatureFeature.Constitution);
            size.AddRelation(0.25f, CreatureFeature.Strength);
            genes.Add(size);

            Gene knowledge = new Gene(CreatureFeature.Knowledge, 50);
            knowledge.AddRelation(0.1f, CreatureFeature.Size);
            knowledge.AddRelation(0.25f, CreatureFeature.Perception);
            genes.Add(knowledge);

            Gene camouflage = new Gene(CreatureFeature.Camouflage, 50);
            camouflage.AddRelation(-0.5f, CreatureFeature.Size);
            genes.Add(camouflage);

            Gene metabolism = new Gene(CreatureFeature.Metabolism, 50);
            metabolism.AddRelation(-0.2f, CreatureFeature.Size);
            genes.Add(metabolism);

            Gene idealTemp = new Gene(CreatureFeature.IdealTemperature, 50);
            idealTemp.AddRelation(0.15f, CreatureFeature.Metabolism);
            idealTemp.AddRelation(-0.25f, CreatureFeature.Size);
            genes.Add(idealTemp);

            Gene tempRange = new Gene(CreatureFeature.TemperatureRange, 50);
            tempRange.AddRelation(0.4f, CreatureFeature.Resistence);
            genes.Add(tempRange);

            Gene longevity = new Gene(CreatureFeature.Longevity, 50);
            longevity.AddRelation(-0.5f, CreatureFeature.Metabolism);
            genes.Add(longevity);

            Gene diet = new Gene(CreatureFeature.Diet, 15);
            diet.AddRelation(0.35f, CreatureFeature.Aggressiveness);
            genes.Add(diet);

            Gene mobility = new Gene(CreatureFeature.Mobility, 50);
            mobility.AddRelation(0.4f, CreatureFeature.Members);
            mobility.AddRelation(-0.2f, CreatureFeature.Size);
            mobility.AddRelation(-0.2f, CreatureFeature.Fortitude);
            genes.Add(mobility);

            //Abilities

            Gene arboreal = new Gene(CreatureFeature.Arboreal, 10);
            arboreal.AddRelation(-0.15f, CreatureFeature.Size);
            genes.Add(arboreal);
            Gene wings = new Gene(CreatureFeature.Wings, 10);
            wings.AddRelation(-0.3f, CreatureFeature.Size);
            genes.Add(wings);
            Gene venomous = new Gene(CreatureFeature.Venomous, 10);
            genes.Add(venomous);
            Gene nightvision = new Gene(CreatureFeature.NightVision, 10);
            wings.AddRelation(0.15f, CreatureFeature.Perception);
            genes.Add(nightvision);
            Gene horns = new Gene(CreatureFeature.Horns, 10);
            genes.Add(horns);
            Gene mimic = new Gene(CreatureFeature.Mimic, 10);
            genes.Add(mimic);
            Gene upright = new Gene(CreatureFeature.Upright, 10);
            genes.Add(upright);
            Gene thorns = new Gene(CreatureFeature.Thorns, 50);
            thorns.AddRelation(0.2f, CreatureFeature.Fortitude);
            genes.Add(thorns);
            Gene scavenger = new Gene(CreatureFeature.Scavenger, 10);
            genes.Add(scavenger);
            Gene hair = new Gene(CreatureFeature.Hair, 10);
            genes.Add(hair);
            Gene paternity = new Gene(CreatureFeature.Paternity, 10);
            genes.Add(paternity);

            CreatureChromosome.SetStructure(genes);
        }
    }
}