using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using AlgoritmoEvolutivo;

namespace VisualizadorConsola
{
    /// <summary>
    /// Implementacion de la simulacion con output de consola
    /// </summary>
    public class ConsoleSimulation : ISimulation
    {
        public void Init()
        {
            world = new World();
            world.Init(8);
            Creature c = world.AddEntity<Creature>();
            c.Init(world, 4, 4);
        }

        public void Run()
        {
            while (true)
            {
                world.Tick();
                Render();
                Thread.Sleep(100);
            }
        }

        /// <summary>
        /// Renderiza el mapa y las criaturas
        /// </summary>
        void Render()
        {
            Console.Clear();
            for (int i = 0; i < world.mapSize; i++)
            {
                for (int j = 0; j < world.mapSize; j++)
                {
                    Console.Write(". ");
                }
                Console.WriteLine();
            }
            foreach (var e in world.entities)
            {
                Console.SetCursorPosition((e as Creature).x * 2, (e as Creature).y);
                Console.Write("e ");
            }
            Console.SetCursorPosition(world.mapSize, world.mapSize);
            Console.WriteLine("\n" + cr_state);
        }

        World world;
        string cr_state = "";
    }
}
