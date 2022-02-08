using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

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
            EvolutionSimulation.Creature c = new EvolutionSimulation.Creature();
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
    }
}
