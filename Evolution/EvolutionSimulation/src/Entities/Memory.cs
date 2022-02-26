using System;
using System.Collections.Generic;

namespace EvolutionSimulation.Entities
{
    struct MemoryTileInfo
    {
        public bool discovered; //Whether this tile has been discovered by the creature at some point.
        public bool water;
        public bool fruit; //Whether there is fruit in this tile.
        public float danger; //How dangerous it is to go though this tile.

        //TODO: Puede que estas dos listas se puedan simplificar para que no contengan tanta informacion
        public List<Creature> creatures; //The list of creatures of a different species
        public List<Creature> allies; //The list of creatures of the same species
        public List<Corpse> corpses; //The nutritional value of each edible corpse seen.
    }

    public class Memory
    {
        Creature thisCreature;
        World world;
        MemoryTileInfo[,] map;
        int perceptionRadius; //Radius around the creature in which it percieves the world.
        int dangerRadius; //Radius around a tile in which the tile's danger spreads.

        public Creature closestCreature { get; private set; }
        public Creature closestCreatureReachable { get; private set; }
        public Creature closestAlly { get; private set; }
        public Creature closestPossibleMate { get; private set; }
        public Corpse closestCorpse { get; private set; }
        public Tuple<int, int> closestFruit { get; private set; }
        public Tuple<int, int> closestWater { get; private set; }

        public Memory(Creature c, World w, int perceptionRadius, int dangerRadius)
        {
            thisCreature = c;
            world = w;
            this.perceptionRadius = perceptionRadius;
            this.dangerRadius = dangerRadius;
            map = new MemoryTileInfo[world.map.GetLength(0), world.map.GetLength(1)];
        }

        public void Update()
        {
            int x = thisCreature.x, y = thisCreature.y;
            List<Creature> perceivedCreatures = world.PerceiveCreatures(thisCreature, perceptionRadius);
            List<StableEntity> perceivedCorpses = world.PerceiveEntities(thisCreature, perceptionRadius);

            //This for structure is recurrent all throughtout this class and is explained with the following example:
            //With radius = 3, it goes from -3 inclusive to 3 inclusive in both axis, going through -3, -2, -1, 0, 1, 2, 3
            //This is used to calculate the offsets of the creature's position to go through an area around it.
            for (int i = -perceptionRadius; i <= perceptionRadius; i++)
            {
                for (int j = -perceptionRadius; j <= perceptionRadius; j++)
                {
                    if (IsOutOfBounds(x + i, y + j)) continue;

                    //Each tile's danger has to be reset before any danger is calculated because a tile's
                    //danger influences the rest ALL AROUND IT, so no matter how you process the area
                    //there would be overwrittes.
                    map[x + i, y + j].danger = 0;

                    //The list of creatures is also needed to calculate danger so it is done here too.
                    foreach (Creature creature in perceivedCreatures)
                    {
                        //TODO: Quitarla si no tiene reach si solo se quiere como fuente de comida
                        if (creature.x == x + i && creature.y == y + j)
                        {
                            //If a creature is the same species as this creature or
                            //it belongs to a child species of this creature's or
                            //it belongs to a parent species of this creature's they're allies.
                            if (creature.speciesName == thisCreature.speciesName ||
                                creature.progenitorSpeciesName == thisCreature.speciesName ||
                                creature.speciesName == thisCreature.progenitorSpeciesName)
                                map[x + i, y + j].allies.Add(creature);
                            //Else they have no relation
                            else
                                map[x + i, y + j].creatures.Add(creature);

                            perceivedCreatures.Remove(creature);
                        }
                    }

                    foreach (StableEntity entity in perceivedCorpses)
                    {
                        if (!(entity is Corpse)) perceivedCorpses.Remove(entity);
                        else if (entity.x == x + i && entity.y == y + j)
                        {
                            map[x + i, y + j].corpses.Add(entity as Corpse);
                            perceivedCorpses.Remove(entity);
                        }
                    }
                }
            }

            //With the danger values reset and creatures accounted for, the information
            //for every tile in sight can be updated.
            for (int i = -perceptionRadius; i <= perceptionRadius; i++)
            {
                for (int j = -perceptionRadius; j <= perceptionRadius; j++)
                {
                    UpdateMemoryTile(x + i, y + j);
                }
            }

            SearchResources();
        }

        private void UpdateMemoryTile(int x, int y)
        {
            if (IsOutOfBounds(x, y)) return; //If the position is out of bounds it is ignored.

            map[x, y].water = world.map[x, y].isWater;
            if (map[x, y].water) return; //If the tile is water there is nothing more to process.

            //There is fruit if the plant is edible and it hasn't been eaten.
            map[x, y].fruit = world.map[x, y].plant is EdiblePlant && !(world.map[x, y].plant as EdiblePlant).eaten;

            //The danger in a tile is calculated by adding up all the intimidation stats of the creatures in it.
            //Then, the danger is increased in the vecinity in a radius, dividing the original danger
            //by a power of 2 depending on distance:
            //  0 distance -> 16
            //  1 distance -> 8
            //  2 distance -> 4...
            float posDanger = 0;
            foreach (Creature creature in map[x, y].creatures)
            {
                posDanger += creature.stats.Intimidation;
            }
            for (int i = -dangerRadius; i <= dangerRadius; i++)
            {
                for (int j = -dangerRadius; j <= dangerRadius; j++)
                {
                    if (IsOutOfBounds(x + i, y + j)) continue;
                    map[x + i, y + j].danger += posDanger / (float)Math.Pow(2, Math.Max(i, j));
                }
            }
        }

        /// <summary>
        /// Searches the creatures memory for resources it has seen, stating from its current position
        /// and expanding outwards until all resources have been located or all known world tiles have been processed.
        /// </summary>
        private void SearchResources()
        {
            int x = thisCreature.x, y = thisCreature.y;

            //Values reset to know if a resource hasn't been found
            closestCreature = null;
            closestCreatureReachable = null;
            closestAlly = null;
            closestPossibleMate = null;
            closestCorpse = null;
            closestFruit = null;
            closestWater = null;

            bool tilesDiscoveredLeft = true; //This boolean is true when all tiles processed in the last iteration of the algorith where NOT discovered.
            int searchRadius = 0; //Areas are represented by squares, and this variable represents the length of the area's side.

            //TODO: Todas se han asignado
            //While there are tiles discovered in memory they have to be processed
            //This while ends when there has been an iteration where all tiles where undiscovered or all values have been asigned.
            while (tilesDiscoveredLeft ||
                (closestCreature != null &&
                closestCreatureReachable != null &&
                closestAlly != null &&
                closestPossibleMate != null &&
                closestCorpse != null &&
                closestFruit != null &&
                closestWater != null))
            {
                tilesDiscoveredLeft = false; //It is supposed that there are no more discovered tiles to process anymore
                for (int i = -searchRadius; i <= searchRadius; i++)
                {
                    for (int j = -searchRadius; j <= searchRadius;)
                    {
                        if (!IsOutOfBounds(x, y))
                        {
                            UpdateResources(x + i, y + j);
                            if (!tilesDiscoveredLeft) //If the supposition still holds, it is checked if the tile being processed is discovered.
                                                      //If it is not, the supposition holds; if it is, the while must continue so the value won't be changed anymore.
                                tilesDiscoveredLeft = map[x + i, y + j].discovered;
                        }

                        //This double for does NOT go normally through an area. What it does is increase the area every iteration, doing the following:
                        //X -> First iteration
                        //O -> Second iteration
                        //S -> Third iteration
                        //S S S S S
                        //S O O O S
                        //S O X O S
                        //S O O O S
                        //S S S S S

                        //So when the first or the last row is being processed, it is processed completely, otherwise, only the first and last elements are processed.
                        //To jump from the first to the last, it is done j += searchRadius * 2 because if radius = 3 then on the first j iteration j = -3,
                        //and after being processed j = -3 + (3 * 2) = 3, which is the last column. Then, to leave the j for, it is added again.
                        if (i == -searchRadius || i == searchRadius) j++;
                        else j += searchRadius * 2;
                    }
                }
                //Search radius is increased.
                searchRadius++;
            }
        }

        /// <summary>
        /// Given a position this method tries to assign the resources the creature could need if
        /// it hasn't found them yet and they are found in the given world tile.
        /// </summary>
        private void UpdateResources(int x, int y)
        {
            if (map[x, y].creatures.Count > 0)
            {
                if (closestCreature == null) closestCreature = map[x, y].creatures[0];
                if (closestCreatureReachable == null)
                    foreach (Creature creature in map[x, y].creatures)
                        if ((creature.creatureLayer == Creature.HeightLayer.Air && thisCreature.stats.AirReach) ||
                            creature.creatureLayer == Creature.HeightLayer.Tree && thisCreature.stats.TreeReach ||
                            creature.creatureLayer == Creature.HeightLayer.Ground)
                            closestCreatureReachable = creature;
            }
                
            if (map[x, y].allies.Count > 0)
            {
                if (closestAlly == null) closestAlly = map[x, y].allies[0];
                if (closestPossibleMate == null)
                    foreach (Creature creature in map[x, y].allies)
                        if (creature.stats.InHeat) closestPossibleMate = creature;
            }
            if (closestCorpse == null && map[x, y].creatures.Count > 0)
                closestCorpse = map[x, y].corpses[0];
            if (closestFruit == null && map[x, y].fruit)
                closestFruit = new Tuple<int, int>(x, y);
            if (closestWater == null && map[x, y].water)
                closestWater = new Tuple<int, int>(x, y);
        }

        /// <summary>
        /// Checks if a position is outside the world, in other words, if the position given is valid.
        /// </summary>
        private bool IsOutOfBounds(int x, int y)
        {
            return x < 0 || y < 0 || x >= map.GetLength(0) || y >= map.GetLength(1);
        }
    }
}
