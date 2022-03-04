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
            public bool fruit;  //Whether there is fruit in this tile.

            //TODO: Puede que estas dos listas se puedan simplificar para que no contengan tanta informacion
            public List<Creature> creatures = new List<Creature>(); //The list of creatures of a different species
            public List<Creature> allies = new List<Creature>();    //The list of creatures of the same species
            public List<Corpse> corpses = new List<Corpse>();       //The nutritional value of each edible corpse seen.
        }

        Creature thisCreature;
        World world;
        MemoryTileInfo[,] map;
        List<MemoryTileInfo> rememberedTiles;
        MemoryTileComparer comparer;
        int maxTicksUnchecked;
        int perceptionRadius;   //Radius around the creature in which it perceives the world.
        int dangerRadius;       //Radius around a tile in which the tile's danger spreads.

        Creature closestCreature;
        Creature closestCreatureReachable;
        Creature closestAlly;
        Creature closestPossibleMate;
        Corpse closestCorpse;
        EdiblePlant closestFruit;
        Tuple<int, int> closestWater;
        Tuple<int, int> closestSafePlace;
        Tuple<int, int> undiscoveredPlace;

        public Creature ClosestCreature() { return closestCreature; }
        public Creature ClosestCreatureReachable() { return closestCreatureReachable; }
        public Creature ClosestAlly() { return closestAlly; }
        public Creature ClosestPossibleMate() { return closestPossibleMate; }
        public Corpse ClosestCorpse() { return closestCorpse; }
        public EdiblePlant ClosestFruit() { return closestFruit; }
        public Tuple<int, int> ClosestWater() { return closestWater; }
        public Tuple<int, int> ClosestSafePlace() { return closestSafePlace; }
        public Tuple<int, int> UndiscoveredPlace() {
            if (undiscoveredPlace == null || map[undiscoveredPlace.Item1, undiscoveredPlace.Item2].discovered)
                GetNewUndiscoveredPlace();    
            return undiscoveredPlace;
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

            GetNewUndiscoveredPlace();
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

                    map[x + i, y + j].allies.Clear();
                    map[x + i, y + j].creatures.Clear();
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
                                map[x + i, y + j].allies.Add(creature);
                            //Else they have no relation
                            else
                                map[x + i, y + j].creatures.Add(creature);

                            //perceivedCreatures.Remove(creature); //Once processed the object is removed to reduce cost.
                        }
                    }

                    //Saves the corpses in a tile and throws away the rest.
                    foreach (StaticEntity entity in perceivedEntities)
                    {
                        if (!(entity is Corpse)) perceivedEntities.Remove(entity);
                        else if (entity.x == x + i && entity.y == y + j)
                        {
                            map[x + i, y + j].corpses.Add(entity as Corpse);
                            //perceivedEntities.Remove(entity); //Once processed the object is removed to reduce cost.
                        }
                    }
                    //With the creatures accounted for, the information for every tile in sight can be updated.
                    UpdateMemoryTile(x + i, y + j);
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

        private void UpdateMemoryTile(int x, int y)
        {
            if (IsOutOfBounds(x, y)) return; //If the position is out of bounds it is ignored.

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
            //TODO: Hierba
            map[x, y].fruit = world.map[x, y].plant is EdiblePlant && !(world.map[x, y].plant as EdiblePlant).eaten;

            float danger = 0;
            foreach (Creature creature in map[x, y].creatures)
            {
                danger += creature.stats.Intimidation;
            }
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
            //If there are creatures unrelated to the creature:
            if (tile.creatures.Count > 0)
            {
                //The first one is assigned as the closest one if there isn't one already.
                if (closestCreature == null) closestCreature = tile.creatures[0];
                //The fist creature reachable is assigned if there isn't one already.
                if (closestCreatureReachable == null)
                    foreach (Creature creature in tile.creatures)
                        if ((creature.creatureLayer == Creature.HeightLayer.Air && thisCreature.stats.AirReach) ||
                            creature.creatureLayer == Creature.HeightLayer.Tree && thisCreature.stats.TreeReach ||
                            creature.creatureLayer == Creature.HeightLayer.Ground)
                            closestCreatureReachable = creature;
            }

            //Same everything else.
            if (tile.allies.Count > 0)
            {
                if (closestAlly == null) closestAlly = tile.allies[0];
                if (closestPossibleMate == null)
                    foreach (Creature creature in tile.allies)
                        if (creature.wantMate) closestPossibleMate = creature;
            }
            if (closestCorpse == null && tile.corpses.Count > 0)
                closestCorpse = tile.corpses[0];
            if (closestFruit == null && tile.fruit)
                closestFruit = world.map[tile.x, tile.y].plant as EdiblePlant;
            if (closestWater == null && tile.water)
                closestWater = new Tuple<int, int>(tile.x, tile.y);
            if (closestSafePlace == null && GetPositionDanger(tile.x, tile.y) == 0)
                closestSafePlace = new Tuple<int, int>(tile.x, tile.y);
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
