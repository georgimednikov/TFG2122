﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using EvolutionSimulation;
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
            /*Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            EvolutionSimulation.TestFSM testFSM = new EvolutionSimulation.TestFSM();
            
            var result = MessageBox.Show("State: " + testFSM.GetState().ToString() + "\nKill?", "TestFSM",
                     MessageBoxButtons.YesNo,
                     MessageBoxIcon.Question);

            if (result == System.Windows.Forms.DialogResult.Yes)
            {
                testFSM.Fire(EvolutionSimulation.Trigger.Die);
            }

            result = MessageBox.Show("State: " + testFSM.GetState().ToString(), "TestFSM",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Information);

            Application.Run(new Form1());*/
            MainClass.Test();
        }
    }
}
