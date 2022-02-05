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
            var result = MessageBox.Show((AlgoritmoEvolutivo.MainClass.Test()).ToString(), "Smth",
                                 MessageBoxButtons.YesNo,
                                 MessageBoxIcon.Question);
            Application.Run(new Form1());
        }
    }
}
