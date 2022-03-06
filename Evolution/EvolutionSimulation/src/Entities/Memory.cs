using System;
using System.Collections.Generic;

namespace EvolutionSimulation.Entities
{
    public class Memory
    {
        private class MemoryTileInfo
        {
            //The tile's position relative to the world.
            public int x;
            public int y;

            public int ticksUnchecked;  //Number of ticks since the tile has been seen for the last time.
            public bool discovered;     //Whether this tile has been discovered by the creature at some point.

            public float experienceDanger;  //How dangerous the creature has experienced the tile to be.
            public float tangibleDanger;    //How dangerous the creature rekons the tile is.

            public bool water;

            // Actual pointers to resources, to use only when already next to them, which is calculated
            // using the positions as remembered by the creature, see below.
            public Creature closestCreature;
            public Creature closestCreatureReachable;
            public Creature closestAlly;
            public Creature closestPossibleMate;
            public Corpse closestCorpse;
            public EdiblePlant closestFruit;
        }

        // The resources' positions, as remembered by the creature
        // Use these when going to a place it remembers, and if it gets there
        // and the resource is no more, it will be automatically updated.
        Tuple<int, int> closestCreature;
        Tuple<int, int> closestCreatureReachable;
        Tuple<int, int> closestAlly;
        Tuple<int, int> closestPossibleMate;
        Tuple<int, int> closestCorpse;
        Tuple<int, int> closestFruit;
        Tuple<int, int> closestWater;
        Tuple<int, int> closestSafePlace;
        Tuple<int, int> undiscoveredPlace;


        Creature thisCreature;
        World world;
        MemoryTileInfo[,] map;
        List<MemoryTileInfo> rememberedTiles;
        MemoryTileComparer comparer;
        int maxTicksUnchecked;
        int perceptionRadius;   //Radius around the creature in which it perceives the world.
        int dangerRadius;       //Radius around a tile in which the tile's danger spreads.


        public Tuple<int, int> ClosestCreaturePosition() { return closestCreature; }
        public Tuple<int, int> ClosestCreatureReachablePosition() { return closestCreatureReachable; }
        public Tuple<int, int> ClosestAllyPosition() { return closestAlly; }
        public Tuple<int, int> ClosestPossibleMatePosition() { return closestPossibleMate; }
        public Tuple<int, int> ClosestCorpsePosition() { return closestCorpse; }
        public Tuple<int, int> ClosestFruitPosition() { return closestFruit; }
        public Tuple<int, int> ClosestWaterPosition() { return closestWater; }
        public Tuple<int, int> ClosestSafePlacePosition() { return closestSafePlace; }
        public Tuple<int, int> UndiscoveredPlacePosition()
        {
            if (undiscoveredPlace == null || map[undiscoveredPlace.Item1, undiscoveredPlace.Item2].ticksUnchecked < (maxTicksUnchecked / 2))
                GetNewUndiscoveredPlace();
            return undiscoveredPlace;
        }

        public Creature ClosestCreature()
        {
            MemoryTileInfo tile = map[closestCreature.Item1, closestCreature.Item2];
            //if (tile.ticksUnchecked == 0)
            return tile.closestCreature;
            //return null;
        }
        public Creature ClosestCreatureReachable()
        {
            MemoryTileInfo tile = map[closestCreatureReachable.Item1, closestCreatureReachable.Item2];
            return tile.closestCreatureReachable;
        }
        public Creature ClosestAlly()
        {
            MemoryTileInfo tile = map[closestAlly.Item1, closestAlly.Item2];
            return tile.closestAlly;
        }
        public Creature ClosestPossibleMate()
        {
            MemoryTileInfo tile = map[closestPossibleMate.Item1, closestPossibleMate.Item2];
            return tile.closestPossibleMate;
        }
        public Corpse ClosestCorpse()
        {
            MemoryTileInfo tile = map[closestCorpse.Item1, closestCorpse.Item2];
            return tile.closestCorpse;
        }
        public EdiblePlant ClosestFruit()
        {
            MemoryTileInfo tile = map[closestFruit.Item1, closestFruit.Item2];
            return tile.closestFruit;
        }





        public Memory(Creature c, World w)
        {
            thisCreature = c;
            world = w;
            map = new MemoryTileInfo[world.map.GetLength(0), world.map.GetLength(1)];
            for (int i = 0; i < map.GetLength(0); i++)
                for (int j = 0; j < map.GetLength(1); j++)
                    map[i, j] = new MemoryTileInfo();

            rememberedTiles = new List<MemoryTileInfo>();
            comparer = new MemoryTileComparer(thisCreature);

            maxTicksUnchecked = thisCreature.stats.Knowledge * UniverseParametersManager.parameters.knowledgeTickMultiplier;
            perceptionRadius = /*thisCreature.stats.Perception*/ 5; // TODO: Perception es literalmente 0, eso no vale así que hay que arreglarlo
            dangerRadius = thisCreature.chromosome.GetFeatureMax(Genetics.CreatureFeature.Aggressiveness) - thisCreature.stats.Aggressiveness;

            //GetNewUndiscoveredPlace();
        }

        public void Update()
        {
            int x = thisCreature.x, y = thisCreature.y;

            for (int i = 0; i < rememberedTiles.Count; i++)
            {
                MemoryTileInfo tile = rememberedTiles[i];
                tile.ticksUnchecked++;
                if (tile.ticksUnchecked >= maxTicksUnchecked) //If it's time to forget a tile.
                {
                    tile.discovered = false;
                    //If the tile is forgotten the the creature no longer feels danger/fondness of it and its vecinity.
                    //To undo that information, the danger is adjusted with the opposite value.
                    AdjustDanger(tile.x, tile.y, -tile.tangibleDanger, false);
                    AdjustDanger(tile.x, tile.y, -tile.experienceDanger, true);
                    rememberedTiles.Remove(tile);
                }
            }

            List<Creature> perceivedCreatures = world.PerceiveCreatures(thisCreature, perceptionRadius);
            List<StaticEntity> perceivedEntities = world.PerceiveEntities(thisCreature, perceptionRadius);

            //This for structure is recurrent all throughtout this class and is explained with the following example:
            //With radius = 3, it goes from -3 inclusive to 3 inclusive in both axis, going through -3, -2, -1, 0, 1, 2, 3
            //This is used to calculate the offsets of the creature's position to go through an area around it.
            for (int i = -perceptionRadius; i <= perceptionRadius; i++)
            {
                for (int j = -perceptionRadius; j <= perceptionRadius; j++)
                {
                    if (IsOutOfBounds(x + i, y + j)) continue;
                    ResetMemoryTilePointers(x + i, y + j);

                    map[x + i, y + j].allies.Clear();
                    map[x + i, y + j].creatures.Clear();
                    float tileDanger = 0;
                    //The list of creatures is also needed to calculate danger so it is done here too.
                    foreach (Creature creature in perceivedCreatures)
                    {
                        if (creature.x == x + i && creature.y == y + j)
                        {
                            //If a creature is the same species as this creature or
                            //it belongs to a child species of this creature's or
                            //it belongs to a parent species of this creature's they're allies.
                            if (creature.speciesName == thisCreature.speciesName ||
                                creature.progenitorSpeciesName == thisCreature.speciesName ||
                                creature.speciesName == thisCreature.progenitorSpeciesName)
                            {
                                map[x + i, y + j].closestAlly = creature;
                            }
                            //Else they have no relation
                            else
                            {
                                map[x + i, y + j].closestCreature = creature;
                                if ((creature.creatureLayer == Creature.HeightLayer.Air && thisCreature.stats.AirReach) ||
                                    creature.creatureLayer == Creature.HeightLayer.Tree && thisCreature.stats.TreeReach ||
                                    creature.creatureLayer == Creature.HeightLayer.Ground)
                                    map[x + i, y + j].closestCreatureReachable = creature;

                                tileDanger += creature.stats.Intimidation;
                            }
                        }
                    }
                    //The information for every tile in sight is updated.
                    UpdateMemoryTile(x + i, y + j, tileDanger);

                    //Saves the corpses in a tile and throws away the rest.
                    foreach (StaticEntity entity in perceivedEntities)
                    {
                        if (!(entity is Corpse)) perceivedEntities.Remove(entity);
                        else if (entity.x == x + i && entity.y == y + j)
                        {
                            Corpse corpse = entity as Corpse;
                            if (thisCreature.stats.Scavenger > 0 || corpse.Edible)
                                map[x + i, y + j].closestCorpse = corpse;
                            break;
                        }
                    }
                }
            }
            SearchResources();
        }

        /// <summary>
        /// Sets the creatures "fondness" (positive danger) of a tile and its surroundings, based on an experience, using dangerRadius.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="value">Value of "fondness" in the tile. It can be negative if it is actually dangerous</param>
        public void CreateExperience(int x, int y, float value)
        {
            AdjustDanger(x, y, -value, true);
        }

        /// <summary>
        /// Returns the total danger the creature feels of a tile.
        /// </summary>
        public float GetPositionDanger(int x, int y)
        {
            return map[x, y].experienceDanger + map[x, y].tangibleDanger;
        }

        private void ResetMemoryTilePointers(int x, int y)
        {
            MemoryTileInfo tile = map[x, y];
            tile.closestCreature = null;
            tile.closestCreatureReachable = null;
            tile.closestAlly = null;
            tile.closestPossibleMate = null;
            tile.closestCorpse = null;
            tile.closestFruit = null;

        }
        private void UpdateMemoryTile(int x, int y, float danger)
        {
            // If the tile is not remembered by the creature, it now is, and is added to the list.
            if (!map[x, y].discovered)
            {
                map[x, y].x = x; map[x, y].y = y;
                map[x, y].discovered = true;
                rememberedTiles.Add(map[x, y]);
            }

            // The tile's tick count gets reseted and its information reassigned.
            map[x, y].ticksUnchecked = 0;
            map[x, y].water = world.map[x, y].isWater;
            if (map[x, y].water) return; //If the tile is water there is nothing more to process.

            //There is fruit if the plant is edible and it hasn't been eaten.
            //TODO: Hierba si es herbivoro deberia contar
            if (world.map[x, y].plant is EdiblePlant)// && !(world.map[x, y].plant as EdiblePlant).eaten)
                map[x, y].closestFruit = world.map[x, y].plant as EdiblePlant;

            AdjustDanger(x, y, danger, false);
        }

        /// <summary>
        /// Modies the danger level of a position, and as a consequence, the tiles around it in dangerRadius.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="danger">Level of danger</param>
        /// <param name="experience">If the danger comes from experience or perception</param>
        private void AdjustDanger(int x, int y, float danger, bool experience)
        {
            //The danger in a tile is calculated by adding up all the intimidation stats of the creatures in it.
            //Then, the danger is increased in the vecinity in a radius, dividing the original danger
            //by a power of 2 depending on distance:
            //  0 distance -> 16
            //  1 distance -> 8
            //  2 distance -> 4...
            for (int i = -dangerRadius; i <= dangerRadius; i++)
            {
                for (int j = -dangerRadius; j <= dangerRadius; j++)
                {
                    if (IsOutOfBounds(x + i, y + j)) continue;
                    float tileDanger = danger / (float)Math.Pow(2, Math.Max(i, j));
                    if (experience) map[x + i, y + j].experienceDanger = tileDanger; // If it's experience danger it is reseted.
                    else map[x + i, y + j].tangibleDanger += tileDanger; //If not, dangers can stack in a single tile.
                }
            }
        }

        /// <summary>
        /// Searches the creatures memory for resources it has seen, stating from its current position
        /// and expanding outwards until all resources have been located or all known world tiles have been processed.
        /// </summary>
        private void SearchResources()
        {
            //Values reset to know if a resource hasn't been found
            closestCreature = null;
            closestCreatureReachable = null;
            closestAlly = null;
            closestPossibleMate = null;
            closestCorpse = null;
            closestFruit = null;
            closestWater = null;
            closestSafePlace = null;

            rememberedTiles.Sort(comparer);
            foreach (MemoryTileInfo tile in rememberedTiles)
            {
                UpdateResources(tile);
                if (AllResourcesFound()) break; // Stops searching when everything has been found.
            }
            GetNewUndiscoveredPlace();
        }

        /// <summary>
        /// Given a position this method tries to assign the resources the creature could need if
        /// it hasn't found them yet and they are found in the given world tile.
        /// </summary>
        private void UpdateResources(MemoryTileInfo tile)
        {
            Tuple<int, int> thisTile = new Tuple<int, int>(tile.x, tile.y);
            //If there are creatures unrelated to the creature:
            //if (tile.creatures.Count > 0)
            //{
            //    //The first one is assigned as the closest one if there isn't one already.
            //    if (closestCreature == null) closestCreature = thisTile;
            //    //The fist creature reachable is assigned if there isn't one already.
            //    if (closestCreatureReachable == null)
            //        foreach (Creature creature in tile.creatures)
            //            if ((creature.creatureLayer == Creature.HeightLayer.Air && thisCreature.stats.AirReach) ||
            //                creature.creatureLayer == Creature.HeightLayer.Tree && thisCreature.stats.TreeReach ||
            //                creature.creatureLayer == Creature.HeightLayer.Ground)
            //                closestCreatureReachable = thisTile;
            //}

            //Same everything else.
            //if (tile.allies.Count > 0)
            //{
            //    if (closestAlly == null) closestAlly = thisTile;
            //    if (closestPossibleMate == null)
            //        foreach (Creature creature in tile.allies)
            //            if (creature.wantMate) closestPossibleMate = thisTile;
            //}

            if (closestCreature == null && tile.closestCreature != null)
                closestCreature = thisTile;
            if (closestCreatureReachable == null && tile.closestCreatureReachable != null)
                closestCreatureReachable = thisTile;
            if (closestAlly == null && tile.closestAlly != null)
                closestAlly = thisTile;
            if (closestPossibleMate == null && tile.closestPossibleMate != null)
                closestPossibleMate = thisTile;
            if (closestCorpse == null && tile.closestCorpse != null)
                closestCorpse = thisTile;
            if (closestFruit == null && tile.closestFruit != null && !tile.closestFruit.eaten)
                closestFruit = thisTile;
            if (closestWater == null && tile.water)
                closestWater = thisTile;
            if (closestSafePlace == null && GetPositionDanger(tile.x, tile.y) == 0)
                closestSafePlace = thisTile;
        }

        /// <summary>
        /// Checks if the creature has recollection of all resources it might need in the world.
        /// </summary>
        private bool AllResourcesFound()
        {
            return closestCreature != null &&
                    closestCreatureReachable != null &&
                    closestAlly != null &&
                    closestPossibleMate != null &&
                    closestCorpse != null &&
                    closestFruit != null &&
                    closestWater != null &&
                    closestSafePlace != null;
        }

        /// <summary>
        /// Gets a random unknown position in the map for the creature a certain distance away so
        /// it can utilize its mobility, since if the target position is close it will always walk there.
        /// </summary>
        private void GetNewUndiscoveredPlace()
        {
            // A min dist so that the place is somewhat far away and the creature has to move for a while,
            // moving through its preferred medium that way.
            // The radius = perceptionRadius, diameter = radius * 2 + the center, since it is not counted.
            int minDistRadius = perceptionRadius;
            int maxDistRadius = (int)(perceptionRadius * 2);
            int minDistDiameter = minDistRadius * 2 + 1;
            int maxDistDiameter = maxDistRadius * 2 + 1;
            int x, y;
            do
            {
                // The way this formula works is by placing the "cursor" in the X position of the creature first
                // and then adding the radius + 1 to be located in the first tile past the min dist from the creature.
                // Then the random number, which can have values up to the number of tiles left outside the area,
                // is added, and then % size to loop to the left of the creature.
                int num = RandomGenerator.Next(0, maxDistDiameter - minDistDiameter);
                x = (thisCreature.x + minDistRadius + 1 + num) % maxDistDiameter;

                num = RandomGenerator.Next(0, maxDistDiameter - minDistDiameter);
                y = (thisCreature.y + minDistRadius + 1 + num) % maxDistDiameter;
            }
            while (map[x, y].water && map[x, y].discovered && map[x, y].ticksUnchecked < (maxTicksUnchecked / 2));
            undiscoveredPlace = new Tuple<int, int>(x, y);
        }

        /// <summary>
        /// Checks if a position is outside the world, in other words, if the position given is valid.
        /// </summary>
        private bool IsOutOfBounds(int x, int y)
        {
            return x < 0 || y < 0 || x >= map.GetLength(0) || y >= map.GetLength(1);
        }

        /// <summary>
        /// Given a creature sorts Tiles of its memory based on distance from it. The shortest goes first.
        /// </summary>
        private class MemoryTileComparer : Comparer<MemoryTileInfo>
        {
            private Creature creature;

            public MemoryTileComparer(Creature creature)
            {
                this.creature = creature;
            }

            public override int Compare(MemoryTileInfo a, MemoryTileInfo b)
            {
                int aDist = Math.Abs(creature.x - a.x) + Math.Abs(creature.y - a.y);
                int bDist = Math.Abs(creature.x - b.x) + Math.Abs(creature.y - b.y);
                return aDist.CompareTo(bDist);
            }
        }
    }
}
