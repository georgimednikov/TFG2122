﻿using System;
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

            public int ticksToBeForgotten;  //Number of ticks since the tile has been seen for the last time.
            public bool discovered;     //Whether this tile has been discovered by the creature at some point.

            public float totalDanger;    //The tiles danger in total, taking into account the others around it.
            //Dangers originating from the tile, separated from the total to be able to undo them when the tile is forgotten.
            public float experienceDanger;  //How dangerous the creature has experienced the tile to be.
            public float tangibleDanger;    //How dangerous the creature rekons the tile is.

            public bool water;

            // Actual pointers to resources, to use only when already next to them, which is calculated
            // using the positions as remembered by the creature, see below.
            public Creature enemyCreature;
            public Creature enemyCreatureReachable;
            public Creature ally;
            public Creature possibleMate;
            public Corpse edibleCorpse;
            public Corpse rottenCorpse;
            public EdiblePlant fruit;
        }

        // The resources' positions, as remembered by the creature
        // Use these when going to a place it remembers, and if it gets there
        // and the resource is no more, it will be automatically updated.
        Tuple<int, int> closestCreature;
        Tuple<int, int> closestCreatureReachable;
        Tuple<int, int> closestAlly;
        Tuple<int, int> closestPossibleMate;
        Tuple<int, int> closestCorpse;
        Tuple<int, int> closestRottenCorpse;
        Tuple<int, int> closestFruit;
        Tuple<int, int> closestWater;
        Tuple<int, int> closestSafePlace;
        Tuple<int, int> undiscoveredPlace;

        List<MemoryTileInfo> safePlants;
        List<MemoryTileInfo> safeWaterSource;

        Creature thisCreature;
        World world;
        MemoryTileInfo[,] map;
        List<MemoryTileInfo> rememberedTiles;
        MemoryTileComparer comparer;
        int maxTicksOfMemory;   //Number of ticks that a tile will remain remembered.
        int perceptionRadius;   //Radius around the creature in which it perceives the world.
        int dangerRadius;       //Radius around a tile in which the tile's danger spreads.


        public Tuple<int, int> ClosestCreaturePosition() { return closestCreature; }
        public Tuple<int, int> ClosestCreatureReachablePosition() { return closestCreatureReachable; }
        public Tuple<int, int> ClosestAllyPosition() { return closestAlly; }
        public Tuple<int, int> ClosestPossibleMatePosition() { return closestPossibleMate; }
        public Tuple<int, int> ClosestCorpsePosition() { return closestCorpse; }
        public Tuple<int, int> ClosestRottenCorpsePosition() { return closestRottenCorpse; }
        public Tuple<int, int> ClosestFruitPosition() { return closestFruit; }
        public Tuple<int, int> ClosestWaterPosition() { return closestWater; }
        public Tuple<int, int> ClosestSafePlacePosition() { return closestSafePlace; }
        public Tuple<int, int> UndiscoveredPlacePosition()
        {
            if (undiscoveredPlace == null || map[undiscoveredPlace.Item1, undiscoveredPlace.Item2].ticksToBeForgotten > (maxTicksOfMemory / 2))
                GetNewUndiscoveredPlace();
            return undiscoveredPlace;
        }

        public Creature ClosestCreature()
        {
            MemoryTileInfo tile = map[closestCreature.Item1, closestCreature.Item2];
            //if (tile.ticksUnchecked == 0)
            return tile.enemyCreature;
            //return null;
        }
        public Creature ClosestCreatureReachable()
        {
            MemoryTileInfo tile = map[closestCreatureReachable.Item1, closestCreatureReachable.Item2];
            return tile.enemyCreatureReachable;
        }
        public Creature ClosestAlly()
        {
            MemoryTileInfo tile = map[closestAlly.Item1, closestAlly.Item2];
            return tile.ally;
        }
        public Creature ClosestPossibleMate()
        {
            MemoryTileInfo tile = map[closestPossibleMate.Item1, closestPossibleMate.Item2];
            return tile.possibleMate;
        }
        public Corpse ClosestCorpse()
        {
            MemoryTileInfo tile = map[closestCorpse.Item1, closestCorpse.Item2];
            return tile.edibleCorpse;
        }
        public Corpse ClosestRottenCorpse()
        {
            MemoryTileInfo tile = map[closestRottenCorpse.Item1, closestRottenCorpse.Item2];
            return tile.edibleCorpse;
        }
        public EdiblePlant ClosestFruit()
        {
            MemoryTileInfo tile = map[closestFruit.Item1, closestFruit.Item2];
            return tile.fruit;
        }

        // Positions of the closest static resource the the creature feels is safe.
        // Once these positions are reached, IN THEORY, ClosestWaterPosition and ClosestFruitPosition will point
        // to these resources, or other at the same distance, which should make no difference.
        public Tuple<int, int> SafeWaterPosition() {
            if (safeWaterSource.Count == 0) return null;
            return new Tuple<int, int>(safeWaterSource[0].x, safeWaterSource[0].y);
        }
        public Tuple<int, int> SafeFruitPosition() {
            if (safePlants.Count == 0) return null;
            return new Tuple<int, int>(safePlants[0].x, safePlants[0].y);
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
            safePlants = new List<MemoryTileInfo>();
            safeWaterSource = new List<MemoryTileInfo>();
            comparer = new MemoryTileComparer(thisCreature);

            maxTicksOfMemory = thisCreature.stats.Knowledge * UniverseParametersManager.parameters.knowledgeTickMultiplier;
            perceptionRadius = (int)(thisCreature.stats.Perception * UniverseParametersManager.parameters.perceptionToRadiusMultiplier);
            dangerRadius = (int)((thisCreature.chromosome.GetFeatureMax(Genetics.CreatureFeature.Aggressiveness) - thisCreature.stats.Aggressiveness) * UniverseParametersManager.parameters.aggressivenessToRadiusMultiplier);
        }

        public void Update()
        {
            int x = thisCreature.x, y = thisCreature.y;

            for (int i = 0; i < rememberedTiles.Count; i++)
            {
                MemoryTileInfo tile = rememberedTiles[i];
                tile.ticksToBeForgotten--;
                if (tile.ticksToBeForgotten <= 0) //If it's time to forget a tile.
                {
                    tile.discovered = false;
                    //If the tile is forgotten the the creature no longer feels danger/fondness of it and its vecinity.
                    //To undo that information, the danger is adjusted with the opposite value.
                    AdjustDanger(tile.x, tile.y, -tile.tangibleDanger);
                    AdjustDanger(tile.x, tile.y, -tile.experienceDanger);
                    tile.tangibleDanger = tile.experienceDanger = 0;
                    rememberedTiles.Remove(tile);

                    if (tile.water && safeWaterSource.Contains(tile))
                        safeWaterSource.Remove(tile);
                    if (safePlants.Contains(tile))
                        safePlants.Remove(tile);
                }
                // If a once considered safe resource has a danger level it is no longer safe.
                if (tile.water && safeWaterSource.Contains(tile) && GetPositionDanger(tile.x, tile.y) > 0)
                    safeWaterSource.Remove(tile);
                if (safePlants.Contains(tile) && GetPositionDanger(tile.x, tile.y) > 0)
                    safePlants.Remove(tile);
            }
            // After removal of tiles no longer fit to be in these lists they are ordered based on distance from the creature.
            safeWaterSource.Sort(comparer);
            safePlants.Sort(comparer);

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
                                map[x + i, y + j].ally = creature;
                            }
                            //Else they have no relation
                            else
                            {
                                map[x + i, y + j].enemyCreature = creature;
                                if ((creature.creatureLayer == Creature.HeightLayer.Air && thisCreature.stats.AirReach) ||
                                    creature.creatureLayer == Creature.HeightLayer.Tree && thisCreature.stats.TreeReach ||
                                    creature.creatureLayer == Creature.HeightLayer.Ground)
                                    map[x + i, y + j].enemyCreatureReachable = creature;

                                tileDanger += creature.stats.Intimidation;
                            }
                        }
                    }
                    //The information for every tile in sight is updated.
                    UpdateMemoryTile(x + i, y + j, tileDanger);

                    //Saves the corpses in a tile and throws away the rest.
                    foreach (StaticEntity entity in perceivedEntities)
                    {
                        if (!(entity is Corpse)) continue;
                        else if (entity.x == x + i && entity.y == y + j)
                        {
                            Corpse newCorpse = entity as Corpse;
                            MemoryTileInfo tile = map[x + i, y + j];
                            if (thisCreature.stats.Scavenger > 0 || newCorpse.Edible)
                            {
                                if (tile.edibleCorpse == null)
                                    tile.edibleCorpse = newCorpse;
                            }
                            else
                            {
                                if (tile.rottenCorpse == null)
                                    tile.rottenCorpse = newCorpse;
                            }
                        }
                    }
                }
            }
            SearchResources();
        }

        /// <summary>
        /// Sets the creatures "fondness" (positive danger) of a tile and its surroundings, based on an experience, using dangerRadius.
        /// </summary>
        /// <param name="value">Value of "fondness" in the tile. It can be negative if it is actually dangerous</param>
        public void CreateExperience(int x, int y, float value)
        {
            AdjustDanger(x, y, -value);
            map[x, y].experienceDanger = -value;
        }
        /// <summary>
        /// Saves in memory a drinking spot that has proven to be safe for the creature.
        /// </summary>
        public void SafeWaterSpotFound(float exp)
        {
            int x = closestWater.Item1, y = closestWater.Item2;
            safeWaterSource.Add(map[x, y]);
            CreateExperience(x, y, exp);
        }
        /// <summary>
        /// Saves in memory an edible plant that has proven to be safe for the creature.
        /// </summary>
        public void SafePlantFound(float exp)
        {
            int x = closestFruit.Item1, y = closestFruit.Item2;
            safePlants.Add(map[closestFruit.Item1, closestFruit.Item2]);
            CreateExperience(x, y, exp);
        }

        /// <summary>
        /// Returns the total danger the creature feels of a tile.
        /// </summary>
        public float GetPositionDanger(int x, int y)
        {
            return map[x, y].totalDanger;
        }

        private void ResetMemoryTilePointers(int x, int y)
        {
            MemoryTileInfo tile = map[x, y];
            tile.enemyCreature = null;
            tile.enemyCreatureReachable = null;
            tile.ally = null;
            tile.possibleMate = null;
            tile.edibleCorpse = null;
            tile.rottenCorpse = null;
            tile.fruit = null;
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
            map[x, y].ticksToBeForgotten = maxTicksOfMemory;
            map[x, y].water = world.map[x, y].isWater;
            if (map[x, y].water) return; //If the tile is water there is nothing more to process.

            //There is fruit if the plant is edible and it hasn't been eaten.
            //TODO: Hierba si es herbivoro deberia contar
            if (world.map[x, y].plant is EdiblePlant)// && !(world.map[x, y].plant as EdiblePlant).eaten)
                map[x, y].fruit = world.map[x, y].plant as EdiblePlant;

            AdjustDanger(x, y, danger);
            map[x, y].tangibleDanger = danger;
        }

        /// <summary>
        /// Modies the danger level of a position, and as a consequence, the tiles around it in dangerRadius.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="danger">Level of danger</param>
        /// <param name="experience">If the danger comes from experience or perception</param>
        private void AdjustDanger(int x, int y, float danger)
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
                    map[x + i, y + j].totalDanger += tileDanger; //If not, dangers can stack in a single tile.
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
            closestRottenCorpse = null;
            closestFruit = null;
            closestWater = null;
            closestSafePlace = null;

            rememberedTiles.Sort(comparer); //TODO: Mirar esto
            foreach (MemoryTileInfo tile in rememberedTiles)
            {
                UpdateResources(tile);
                if (AllResourcesFound()) break; // Stops searching when everything has been found.
            }
        }

        /// <summary>
        /// Given a position this method tries to assign the resources the creature could need if
        /// it hasn't found them yet and they are found in the given world tile.
        /// </summary>
        private void UpdateResources(MemoryTileInfo tile)
        {
            Tuple<int, int> thisTile = new Tuple<int, int>(tile.x, tile.y);

            if (closestCreature == null && tile.enemyCreature != null)
                closestCreature = thisTile;
            if (closestCreatureReachable == null && tile.enemyCreatureReachable != null)
                closestCreatureReachable = thisTile;
            if (closestAlly == null && tile.ally != null)
                closestAlly = thisTile;
            if (closestPossibleMate == null && tile.possibleMate != null)
                closestPossibleMate = thisTile;
            if (closestCorpse == null && tile.edibleCorpse != null)
                closestCorpse = thisTile;
            if (closestRottenCorpse == null && tile.rottenCorpse != null)
                closestRottenCorpse = thisTile;
            if (closestFruit == null && tile.fruit != null && !tile.fruit.eaten)
                closestFruit = thisTile;
            if (closestWater == null && tile.water && GetPositionDanger(tile.x, tile.y) <= 0)
                closestWater = thisTile;
            if (closestSafePlace == null && GetPositionDanger(tile.x, tile.y) <= 0)
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
                    closestRottenCorpse != null &&
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
            while (map[x, y].water || map[x, y].ticksToBeForgotten > (maxTicksOfMemory / 2) || GetPositionDanger(x, y) > 0);
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
