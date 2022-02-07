﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using EvolutionSimulation;

namespace VisualizadorConsola
{
    /// <summary>
    /// Implementation of a simulation with console output
    /// </summary>
    public class ConsoleSimulation : ISimulation
    {
        public void Init()
        {
            entities = new List<IEntity>();
            creatures = new List<Creature>();
            delete = new List<IEntity>();
            world = new World();
            world.Init(32);
            Creature c = CreateEntity<Creature>();
            c.metabolism = 200;
            c.Init(world, 16, 16);
            c = CreateEntity<Creature>();
            c.metabolism = 100;
            c.Init(world, 12, 16);
        }

        public void Run()
        {
            while (true)
            {
                world.Tick();
                entities.ForEach(delegate (IEntity e) { e.Tick(); });   // Orders the entity to perform a step
                creatures.Sort(new SortByMetabolism());
                creatures.ForEach(delegate (Creature e) { e.Tick(); });

                delete.ForEach(delegate (IEntity e) { entities.Remove(e); });

                delete.Clear();
                Render();
                Thread.Sleep(1000);
            }
        }

        /// <summary>
        /// Adds an entity to the list
        /// </summary>
        /// <typeparam name="T">Entity type</typeparam>
        /// <returns>The added entity</returns>
        public T CreateEntity<T>() where T : IEntity, new()
        {
            T ent = new T();
            if (ent is Creature) creatures.Add(ent as Creature);
            else entities.Add(ent);
            return ent;
        }


        /// <summary>
        /// Designates an entity to be eliminated before the next frame
        /// </summary>
        public void Delete(IEntity entity)
        {
            delete.Add(entity);
        }


        // Entities in the world
        public List<IEntity> entities { get; private set; }
        public List<Creature> creatures { get; private set; }
        // Entities to be deleted
        List<IEntity> delete;

        /// <summary>
        /// Renders the map and creatures
        /// TODO: Modify this MF
        /// </summary>
        public void Render()
        {
            Console.Clear();
            for (int i = 0; i < world.map.GetLength(0); i++)
            {
                //for (int j = 0; j < world.map.GetLength(1); j++)
                //{
                //    double val = world.map[j, i].height;
                //    if (val < 0.3) Console.BackgroundColor = ConsoleColor.Black;
                //    else if (val < 0.5) Console.BackgroundColor = ConsoleColor.DarkBlue;
                //    else if (val == 0.5) Console.BackgroundColor = ConsoleColor.DarkGreen;
                //    else if (val < 0.6) Console.BackgroundColor = ConsoleColor.DarkCyan;
                //    else if (val < 0.7) Console.BackgroundColor = ConsoleColor.Blue;
                //    else if (val < 0.8) Console.BackgroundColor = ConsoleColor.Cyan;
                //    else Console.BackgroundColor = ConsoleColor.White;

                //    Console.Write((Math.Truncate(world.map[j, i].height * 10) / 1));
                //    Console.BackgroundColor = ConsoleColor.Black;
                //}
                //Console.Write(" ");
                /*
                for (int j = 0; j < world.map.GetLength(1); j++)
                {
                    double val = world.map[j, i].humidity;

                    if (val < 0.3) Console.BackgroundColor = ConsoleColor.Red;
                    else if (val < 0.4) Console.BackgroundColor = ConsoleColor.DarkRed;
                    else if (val < 0.5) Console.BackgroundColor = ConsoleColor.Green;
                    else if (val < 0.6) Console.BackgroundColor = ConsoleColor.Cyan;
                    else if (val < 0.8) Console.BackgroundColor = ConsoleColor.DarkCyan;
                    else if (val < 1) Console.BackgroundColor = ConsoleColor.DarkBlue;
                    else Console.BackgroundColor = ConsoleColor.White;

                    if (world.map[j, i].height < 0.5f) Console.BackgroundColor = ConsoleColor.Magenta ;

                    val = (Math.Truncate(val * 10) / 1);
                    if (val >= 10) Console.Write("X");
                    else Console.Write(val);
                    Console.BackgroundColor = ConsoleColor.Black;
                }
                */
                //Console.Write(" ");
                /*
                for (int j = 0; j < world.map.GetLength(1); j++)
                {
                    double val = world.map[j, i].temperature;
                    if (val < 0.2) Console.BackgroundColor = ConsoleColor.DarkBlue;
                    else if (val < 0.3) Console.BackgroundColor = ConsoleColor.Blue;
                    else if (val < 0.5) Console.BackgroundColor = ConsoleColor.DarkYellow;
                    else if (val < 0.6) Console.BackgroundColor = ConsoleColor.Yellow;
                    else if (val < 0.8) Console.BackgroundColor = ConsoleColor.DarkRed;
                    else Console.BackgroundColor = ConsoleColor.Red;

                    Console.Write((Math.Truncate(val * 10) / 1));
                    Console.BackgroundColor = ConsoleColor.Black;
                }
                Console.Write(" ");
                */
                
                Random r = new Random();
                for (int j = 0; j < world.map.GetLength(1); j++)
                {
                    double val = world.map[j, i].flora;
                    if (val == 0) Console.BackgroundColor = ConsoleColor.DarkBlue;
                    else if (val < 0.3) Console.BackgroundColor = ConsoleColor.DarkRed;
                    else if (val < 0.4) Console.BackgroundColor = ConsoleColor.DarkYellow;
                    else if (val < 0.5) Console.BackgroundColor = ConsoleColor.Yellow;
                    else if (val < 0.7) Console.BackgroundColor = ConsoleColor.DarkGreen;
                    else Console.BackgroundColor = ConsoleColor.Green;
                    //if (val >= 0 && r.NextDouble() <= val)
                    //    if (val <= 0.5)
                    //        Console.BackgroundColor = ConsoleColor.Red;
                    //    else if (val <= 0.7)
                    //        Console.BackgroundColor = ConsoleColor.Yellow;
                    //    else
                    //        Console.BackgroundColor = ConsoleColor.Green;
                    val = (Math.Truncate(val * 10) / 1);
                    if (val == 10) Console.Write("X");
                    else 
                        Console.Write(" ");
                    Console.BackgroundColor = ConsoleColor.Black;
                }
                
                Console.WriteLine();
            }

            //TODO: Let he who is without sin cast the first stone
            //TODO: Matthew 7:1
            //Console.Write("Height");
            //Console.Write("                           Humidity");
            //Console.Write("                         Temperature");
            //Console.Write("                      Flora");

            foreach (var e in creatures)
            {
                Console.SetCursorPosition(e.x, e.y);
                Console.Write("e");
            }
            Console.SetCursorPosition(world.map.GetLength(0), world.map.GetLength(0));
        }

        World world;
        string cr_state = "";
    }
}