using System;
using EvolutionSimulation;

namespace VisualizadorConsola
{
    public class ConsoleLoadingBar
    {
        DateTime prevT;
        DateTime newT;
        double prevN;

        public void Begin(EventSimulation e)
        {
            prevT = DateTime.Now;
            newT = DateTime.Now;
            prevN = 0;

            String s = "[";
            for (int j = 0; j < prevN; j++)
            {
                s += ".";
            }
            for (int j = (int)prevN; j < 100; j++)
            {
                s += " ";
            }
            Console.Clear();
            Console.WriteLine(s + "] " + prevN + "%");
            Console.WriteLine("Ticks: {0}/{2} | Num Creatures: {1} | Births: {3} | Entities to Update: {4} | {5}",
                0, e.World.Creatures.Count, e.TotalTicks, e.Births, e.World.StaticEntitiesToUpdate.Count, newT - prevT);
        }

        public void Update(EventSimulation e)
        {
            double newN = Math.Round((double)e.CurrentTick / (double)e.TotalTicks * 100.0);
            if (newN > prevN)
            {
                newT = DateTime.Now;
                prevN = newN;
                String tempS = "[";
                for (int j = 0; j < prevN; j++)
                {
                    tempS += ".";
                }
                for (int j = (int)prevN; j < 100; j++)
                {
                    tempS += " ";
                }

                Console.Clear();
                Console.WriteLine(tempS + "] " + prevN + "%");
                Console.WriteLine("Ticks: {0}/{2} | Num Creatures: {1} | Births: {3} | Entities to Update: {4} | {5}", e.CurrentTick, e.World.Creatures.Count, e.TotalTicks, e.Births, e.World.StaticEntitiesToUpdate.Count, newT - prevT);
                prevT = newT;
            }
        }
    }
}
