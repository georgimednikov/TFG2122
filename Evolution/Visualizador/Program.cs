using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using EvolutionSimulation.Genetics;

namespace Visualizador
{
    static class Program
    {
        /// <summary>
        /// Punto de entrada principal para la aplicación.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            SetChromosome();

            EvolutionSimulation.src.Entities.Animal c = new EvolutionSimulation.src.Entities.Animal();
            EvolutionSimulation.World w = new EvolutionSimulation.World();
            c.Init(w, 0, 0);
            int a = 20;
            while (a > 0)
            {
                --a;
                c.Tick();
            }

            // TODO: ?????
            //AlgoritmoEvolutivo.TestFSM testFSM = new AlgoritmoEvolutivo.TestFSM();

            //var result = MessageBox.Show("State: " + testFSM.GetState().ToString() + "\nKill?", "TestFSM",
            //         MessageBoxButtons.YesNo,
            //         MessageBoxIcon.Question);

            //if (result == System.Windows.Forms.DialogResult.Yes)
            //{
            //    testFSM.Fire(AlgoritmoEvolutivo.Trigger.Die);
            //}

            //result = MessageBox.Show("State: " + testFSM.GetState().ToString(), "TestFSM",
            //                MessageBoxButtons.OK,
            //                MessageBoxIcon.Information);

            //Application.Run(new Form1());
        }

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
            Gene aggressiveness = new Gene(CreatureFeature.Aggressiveness, 20);
            genes.Add(aggressiveness);
            Gene members = new Gene(CreatureFeature.Members, 50);
            genes.Add(members);
            //The rest of the genes IN ORDER OF DEPENDENCY

            //Other Attributes
            Gene resistence = new Gene(CreatureFeature.Resistence, 50);
            resistence.AddRelation(0.25f, CreatureFeature.Constitution);
            genes.Add(resistence);

            Gene piercing = new Gene(CreatureFeature.Piercing, 25);
            piercing.AddRelation(0.4f, CreatureFeature.Strength);
            genes.Add(piercing);


            //int maxSize = 80;
            Gene size = new Gene(CreatureFeature.Size, 200);
            size.AddRelation(0.4f, CreatureFeature.Constitution);
            size.AddRelation(0.25f, CreatureFeature.Strength);
            genes.Add(size);

            Gene knowledge = new Gene(CreatureFeature.Knowledge, 50);
            knowledge.AddRelation(0.1f, CreatureFeature.Size);
            knowledge.AddRelation(0.25f, CreatureFeature.Perception);
            genes.Add(knowledge);

            //if its a low value, it will be always 0 because of the negative dependency with size
            //int maxCamouflage = (int)(maxSize * 0.5);
            Gene camouflage = new Gene(CreatureFeature.Camouflage, 50);
            camouflage.AddRelation(-0.3f, CreatureFeature.Size);
            genes.Add(camouflage);//540

            Gene metabolism = new Gene(CreatureFeature.Metabolism, 50);
            metabolism.AddRelation(-0.2f, CreatureFeature.Size);
            genes.Add(metabolism);

            Gene idealTemp = new Gene(CreatureFeature.IdealTemperature, 50);
            idealTemp.AddRelation(0.15f, CreatureFeature.Metabolism);
            idealTemp.AddRelation(-0.25f, CreatureFeature.Size);
            genes.Add(idealTemp);

            Gene tempRange = new Gene(CreatureFeature.TemperatureRange, 20);
            tempRange.AddRelation(0.3f, CreatureFeature.Resistence);
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
            nightvision.AddRelation(0.15f, CreatureFeature.Perception);
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
