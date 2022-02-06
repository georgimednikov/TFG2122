using AlgoritmoEvolutivo;
using System;
using System.IO;

namespace VisualizadorConsola
{
    public static class Program
    {
        static void Main(string[] args)
        {
            ISimulation s = new ConsoleSimulation();
            s.Init();
            s.Run();
            s.Render();
        }
    }
}