using System.IO;
using System.Windows.Forms;

namespace EvolutionSimulation
{
    static public class WorkingDirectories
    {
        static public string DataDirectory;
        static public string ExportDirectory;

        /// <summary>
        /// Asks the user where to look for the files containing the different values for the calculation of the chromosme, genes and stats,
        /// as well as the folder in which to save the resulting species.
        /// </summary>
        /// <returns></returns>
        static public bool AskDirectories()
        {
            FolderBrowserDialog browser = new FolderBrowserDialog();
            browser.Description = "Choose the folder where the program will search for the data files. " +
                "After this window a new one will open; if it does not show, minimize any other application open.";
            browser.SelectedPath = Path.GetFullPath("../../ProgramData");
            DialogResult result = browser.ShowDialog();

            if (result == DialogResult.OK)
                DataDirectory = browser.SelectedPath + "\\";
            else
                return false;

            browser.Description = "Choose the folder where the program will save the resulting creatures' information.";
            browser.SelectedPath = Path.GetFullPath("../../ResultingSpecies");
            result = browser.ShowDialog();

            if (result == DialogResult.OK)
                ExportDirectory = browser.SelectedPath + "\\";
            else
                return false;

            return true;
        }

        //TODO: Borrar este método CUANDO DEJE DE USARSE
        static public void SetDirectories()
        {
            DataDirectory = "../../ProgramData/";
            ExportDirectory = "../../ResultingData/";
        }
    }
}
