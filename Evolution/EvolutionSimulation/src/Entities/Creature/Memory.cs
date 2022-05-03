using System;
using System.Collections.Generic;
using System.Numerics;

namespace EvolutionSimulation.Entities
{
    public class Memory
    {
        private class Position
        {
            public Vector2Int position;
            public float intimidation;
            public float danger;
            public float safety;
            public int ticks;

            public Position(Vector2Int p, float d, float s, int t)
            {
                position = p;
                danger = d;
                safety = s;
                ticks = t;
            }
            public Position(Vector3Int p, float d, float s, int t)
            {
                position = new Vector2Int(p.x, p.y);
                danger = d;
                safety = s;
                ticks = t;
            }

            public float Danger() { return intimidation + danger + safety; }
        }

        Creature thisCreature;
        World world;

        public int perceptionRadius { get; private set; }
        int dangerRadius;
        float danger;                   //Value representing the danger remembered in a position when something bad happens to the creature.

        int maxExperienceTicks;         //Max number of ticks until the creature forgets.
        int maxResourcesRemembered;     //Max number of resources of each type that the creature can remember.
        int maxPositionsRemembered;     //Max number of positions the creature can remember to have their danger in mind.

        int ticksToSavePosition;        //Amount of ticks after which the current position of the creature (if different to the previous) is saved.
        int ticksElapsed;               //How many ticks have gone by, used along with ticksToSavePosition.

        ResourcePositionComparer resourceComparer;
        ResourcePositionEdibleComparer resourceEdibleComparer;
        ResourceValueComparer valueComparer;
        PositionComparer positionComparer;

        Queue<Vector2Int> explorePositionsRemembered;       //All the dangers the creature remembers, with their dangers and ticks left.
        Dictionary<int, int> exploredRegions;               // Map regions that the creature has explored
        List<Position> dangersRemembered;                   //All the dangers the creature remembers, with their dangers and ticks left.

        public EntityResource Enemy { get; private set; }   //Creature that has attacked this creature or an ally of its.
        public EntityResource Menace { get => menace; }     //Closest creature that is not part of the creature's "family" regarding its species.
        private EntityResource menace;
        public EntityResource Father
        { //If the father is forgotten or dead, his position is lost but not his ID so
          //it can be recognized, but in practice it is the same as returning null until found again.
            get
            {
                if (father != null && father.ticks >= 0)
                    return father;
                return null;
            }
        }
        private EntityResource father;
        public EntityResource Mother //If the mother is forgotten or dead, her position is lost but not her ID so
                                     //it can be recognized, but in practice it is the same as returning null until found again.
        {
            get
            {
                if (mother != null && mother.ticks >= 0)
                    return mother;
                return null;
            }
        }
        private EntityResource mother;
        public EntityResource Mate { get; private set; }
        public List<ValueResource> Preys { get; private set; }              //Closest reachable creature.
        public List<EntityResource> Allies { get; private set; }
        public List<EntityResource> FreshCorpses { get; private set; }
        public List<EntityResource> RottenCorpses { get; private set; }
        public List<Resource> WaterPositions { get; private set; }
        public List<Resource> SafeWaterPositions { get; private set; }      //List containing all safe water spots that the creature remembers.
        public List<EntityResource> EdiblePlants { get; private set; }
        public List<EntityResource> SafeEdiblePlants { get; private set; }  //List containing all safe edible plants that the creature remembers.
        public List<Vector2Int> SafePositions { get; private set; }         //The closests safes location for the creature.
        public List<Vector2Int> SafeTemperaturePositions { get; private set; }


        public Memory(Creature c, int fID, int mID)
        {
            thisCreature = c;
            world = c.world;
            Creature parent = world.GetCreature(fID);
            if (parent != null)
                father = new EntityResource(parent.x, parent.y, (int)parent.creatureLayer, parent.ID, maxExperienceTicks);
            else
                father = new EntityResource(-1, -1, -1, -1, maxExperienceTicks);  // Impossible value to represent no parent
            parent = world.GetCreature(mID);
            if (parent != null)
                mother = new EntityResource(parent.x, parent.y, (int)parent.creatureLayer, parent.ID, maxExperienceTicks);
            else
                mother = new EntityResource(-1, -1, -1, -1, maxExperienceTicks);  // Impossible value to represent no mother

            Preys = new List<ValueResource>();
            Allies = new List<EntityResource>();
            FreshCorpses = new List<EntityResource>();
            RottenCorpses = new List<EntityResource>();
            dangersRemembered = new List<Position>();
            exploredRegions = new Dictionary<int, int>();
            explorePositionsRemembered = new Queue<Vector2Int>();
            WaterPositions = new List<Resource>();
            SafeWaterPositions = new List<Resource>();
            EdiblePlants = new List<EntityResource>();
            SafeEdiblePlants = new List<EntityResource>();
            SafePositions = new List<Vector2Int>();
            SafeTemperaturePositions = new List<Vector2Int>();

            resourceComparer = new ResourcePositionComparer(c);
            resourceEdibleComparer = new ResourcePositionEdibleComparer(c);
            valueComparer = new ResourceValueComparer();
            positionComparer = new PositionComparer(c);

            maxResourcesRemembered = thisCreature.stats.Knowledge + UniverseParametersManager.parameters.maxResourcesRemembered; 
            maxPositionsRemembered = thisCreature.stats.Knowledge + UniverseParametersManager.parameters.maxPositionsRemembered; 
            ticksToSavePosition = 10; //TODO: Esto bien en base a action points?
            ticksElapsed = 0;
            maxExperienceTicks = thisCreature.stats.Knowledge * UniverseParametersManager.parameters.knowledgeTickMultiplier;
            dangerRadius = (int)(thisCreature.stats.Aggressiveness * UniverseParametersManager.parameters.aggressivenessToRadiusMultiplier);
            danger = thisCreature.chromosome.GetFeature(Genetics.CreatureFeature.Aggressiveness) * UniverseParametersManager.parameters.experienceMaxAggresivenessMultiplier;
            CalculatePerceptionRadius();
        }

        public void Update()
        {
            int x = thisCreature.x, y = thisCreature.y;

            if (!thisCreature.CheckTemperature(x, y) && SafeTemperaturePositions.Count == 0 && ticksElapsed % perceptionRadius == 0)
                AddTemperaturePosition();

            if (++ticksElapsed == ticksToSavePosition)
            {
                ticksElapsed = 0;
                AddExplorePosition();
                AddTemperaturePosition();
            }

            //If there is a creature targeted as the enemy and this creature loses sight or the creature dies the pointer is reset.
            if (Enemy != null)
            {
                Creature enemyEntity = world.GetCreature(Enemy.ID);
                if (enemyEntity == null || thisCreature.DistanceToObjective(enemyEntity) > perceptionRadius)
                    Enemy = null;
                else
                    Enemy.position = new Vector3Int(enemyEntity.x, enemyEntity.y,(int)enemyEntity.creatureLayer);
            }

            // If the tile has to be forgotten, it is removed from the creature's memory
            Forget();

            List<Creature> perceivedCreatures = world.PerceiveCreatures(thisCreature.ID, perceptionRadius);
            List<StaticEntity> perceivedEntities = world.PerceiveEntities(thisCreature.ID, perceptionRadius);

            //The list of creatures is also needed to calculate danger so it is done here too.
            //The danger in each tile seen by the creature caused by other creatures.
            float[,] intimidationFelt = new float[1 + perceptionRadius * 2, 1 + perceptionRadius * 2]; //* 2 to make it the diameter and +1 to account for the center.
           
            foreach (Creature creature in perceivedCreatures)
            {
                EntityResource resource = new EntityResource(creature.x, creature.y, (int)creature.creatureLayer, creature.ID, maxExperienceTicks);

                //If a creature is the same species as this creature or
                //it belongs to a child species of this creature's or
                //it belongs to a parent species of this creature's they're allies.
                if (creature.speciesName == thisCreature.speciesName ||
                    creature.progenitorSpeciesName == thisCreature.speciesName ||
                    creature.speciesName == thisCreature.progenitorSpeciesName)
                {
                    UpdateList(Allies, resource, maxExperienceTicks);
                    if (creature.ID == father.ID)
                        RefreshMemory(father, maxExperienceTicks);
                    else if (creature.ID == mother.ID)
                        RefreshMemory(mother, maxExperienceTicks);
                }
                //Else they have no relation
                else
                {
                    
                    int dist = thisCreature.DistanceToObjective(creature);
                    //The intimidation of the perceived creature increases based on how much health this one is missing, up to a value defined in UniverseParameters.
                    float intimidation = creature.stats.Intimidation *
                        (UniverseParametersManager.parameters.maxMenaceIntimidationMultiplierBasedOnMissingHealth - thisCreature.stats.CurrHealth / thisCreature.stats.MaxHealth);
                    if (intimidation > thisCreature.stats.Aggressiveness &&
                        (menace == null || menace == resource || thisCreature.DistanceToObjective(menace.position) >= dist)) //This is equal to update the rival information if it is the same
                    {
                        menace = resource;
                    }

                    //If the creature is reachable and not considered too dangerous it is considered possible prey.
                    float creatureDanger = GetPositionDanger(creature.x, creature.y);
                    if (!creature.IsHerbivorous() &&
                        thisCreature.CanReach(creature.creatureLayer) &&
                        creatureDanger < thisCreature.stats.Aggressiveness)
                    {
                        float preyValue = world.GetCreature(resource.ID).stats.Size / Math.Max(1.0f, creature.DistanceToObjective(resource.position));
                        ValueResource prey = new ValueResource(resource.position, resource.ID, preyValue, resource.ticks);
                        UpdateList(Preys, prey, maxExperienceTicks);
                    }

                    //Since it does not make sense to create an array with the size of the world only to save information about a part of it,
                    //and since using lists complicates the process making it necessary to go through the list with the intimidation levels comparing
                    //positions every time a new creature is processed to add their values if they share a position, plus when the perception area is
                    //check the list would have to be iterated every tile, so the solution is an array representing the perception locally,
                    //and to make the positions positive to save them in the array, the perception radius has to be added now and when reading the value.
                    int intX = creature.x - x + perceptionRadius;
                    int intY = creature.y - y + perceptionRadius;
                    intimidationFelt[intX, intY] += creature.stats.Intimidation;
                }
            }
            //Updates the memory's information about corpses and plants
            foreach (StaticEntity entity in perceivedEntities)
            {
                EntityResource resource = new EntityResource(entity.x, entity.y, 0, entity.ID, maxExperienceTicks);
                if (entity is Corpse && !thisCreature.IsHerbivorous())
                {
                    Corpse newCorpse = entity as Corpse;
                    if (thisCreature.chromosome.HasAbility(Genetics.CreatureFeature.Scavenger, Genetics.CreatureChromosome.AbilityUnlock[Genetics.CreatureFeature.Scavenger])
                        || newCorpse.Edible)
                        UpdateList(FreshCorpses, resource, maxExperienceTicks);
                    else
                        UpdateList(RottenCorpses, resource, maxExperienceTicks);
                }
                else if (entity is EdiblePlant && !thisCreature.IsCarnivorous())
                {
                    if (!(entity as EdiblePlant).eaten)
                    {
                        UpdateList(EdiblePlants, resource, maxExperienceTicks);
                    }
                    else // This removes a perceived and already seen plant
                    {
                        EdiblePlants.Remove(resource);
                    }
                }
            }
            //This for structure is recurrent all throughtout this class and is explained with the following example:
            //With radius = 3, it goes from -3 inclusive to 3 inclusive in both axis, going through -3, -2, -1, 0, 1, 2, 3
            //This is used to calculate the offsets of the creature's position to go through an area around it.
            for (int i = -perceptionRadius; i <= perceptionRadius; i++)
            {
                for (int j = -perceptionRadius; j <= perceptionRadius; j++)
                {
                    Vector2Int p = new Vector2Int(x + i, y + j);
                    if (!world.CheckBounds(p.x, p.y)) continue;

                    if (world.map[p.x, p.y].isWater)
                    {
                        //All shores adjacent to the creature are saved.
                        bool shore = false;
                        for (int k = -1; !shore && k <= 1; k++)
                            for (int h = -1; !shore && h <= 1; h++)
                                if (world.CheckBounds(p.x + k, p.y + h) && !world.map[p.x + k, p.y + h].isWater)
                                    shore = true;
                        if (shore) UpdateList(WaterPositions, new Resource(new Vector3Int(p.x, p.y, 0), maxExperienceTicks), maxExperienceTicks);

                        Position position = GetFromPositionDangers(p);
                        bool positionWasKnown = position == null;
                        if (positionWasKnown)
                            position = new Position(p, 0, 0, 0);

                        position.ticks = maxExperienceTicks; //The number of ticks left to be forgotten is reset too.
                        position.intimidation = intimidationFelt[i + perceptionRadius, j + perceptionRadius];

                        //If this position was not remembered and there is valuable information in it (a danger level) it is remembered.
                        if (!positionWasKnown && position.intimidation != 0) dangersRemembered.Add(position);
                    }
                }
            }
            // The following code also forgets, but since it does not deal with entities rather positions and danger
            // it also serves to assign safes places of the creature.
            float positionDanger;
            //The list is iterated through from the end to the start to deal with removing elements from it while iterating.
            for (int i = dangersRemembered.Count - 1; i >= 0; i--)
            {
                Position p = dangersRemembered[i];
                positionDanger = GetPositionDanger(p.position);

                //If the position is dangerous it is removed from the safe lists.
                if (positionDanger > 0)
                {
                    RemoveFromSafeWater(p.position);
                    RemoveFromSafePlant(p.position);
                }
                //If the tile remains in memory, it is safe and no safe place has been assigned or it is closer than the one already found, it is saved, unless that position can't be reached
                else if (positionDanger < 0 && world.CanMove(p.position.x, p.position.y) && !SafePositions.Contains(p.position))
                {
                    SafePositions.Add(p.position);
                }
            }

            // Check if the creature changed region
            int currentRegion = thisCreature.world.map[thisCreature.x, thisCreature.y].regionId;
            if (!exploredRegions.ContainsKey(currentRegion))
                exploredRegions.Add(currentRegion, maxExperienceTicks);
            else
                exploredRegions[currentRegion] = maxExperienceTicks;

            SortAndAdjustLists();
            if (menace != null && 
                menace.ticks != maxExperienceTicks && thisCreature.DistanceToObjective(menace.position) <= perceptionRadius)
                menace = null;
            if (thisCreature.stats.Gender == Genetics.Gender.Male)
            {
                Mate = null; //By default there is no mate available.
                for (int i = 0; i < Allies.Count; i++) //For every ally the creature remembers the following comprobations are done:
                {
                    Creature ally = world.GetCreature(Allies[i].ID);
                    if (ally == null || ally.stats.Gender == thisCreature.stats.Gender ||
                        !thisCreature.CanReach(ally.creatureLayer))                     //This is done to ignore creatures of the same gender as this one. The gender is
                        continue;                                                       //checked although the creature might not be in sight, but it is not modified
                                                                                        //and this way the gender is not saved (which would be inconvinient).
                                                                                        //The creature has to be able to reach de ally to considere it as a mate

                    if (ally.wantMate) //If it wants to mate, it is a match.
                    {
                        Mate = Allies[i];
                        break;
                    }

                }
            }
        }

        /// <summary>
        /// Returns a new unexplored region for the creature.
        /// In extreme cases (it has explored an it remebers all regions
        /// or it is in the middle of a huge mass of water) it returns a random region
        /// </summary>
        internal int NewExplorePosition()
        {
            int unexploredRegion = -1;
            Queue<int> regionsQueue = new Queue<int>();
            bool[] visited = new bool[world.regionMap.Count];
            int currentRegion = world.map[thisCreature.x, thisCreature.y].regionId;

            // If the creature is over the water, it searches for the nearest land position
            if (currentRegion == -1)
            {
                Vector2Int landPos;

                // If no land position is found, it returns a random region
                if (!SearchForLand(out landPos))
                    return RandomGenerator.Next(0, world.regionMap.Count);
                currentRegion = world.map[landPos.x, landPos.y].regionId;

                // If the land found is unexplored, no further search is needed
                if (!exploredRegions.ContainsKey(currentRegion))
                    unexploredRegion = currentRegion;
            }
            regionsQueue.Enqueue(currentRegion);
            visited[currentRegion] = true;
            // It searches for a unexplored region using BFS
            while (regionsQueue.Count > 0 && unexploredRegion == -1)
            {
                int nextRegion = regionsQueue.Dequeue();
                List<int> adyRegions = new List<int>(world.regionMap[nextRegion].links.Keys);
                Shuffle(adyRegions);    // Shuffle to add randomness
                for (int i = 0; unexploredRegion == -1 && i < adyRegions.Count; i++)
                {
                    if (!visited[adyRegions[i]])
                    {
                        if (!exploredRegions.ContainsKey(adyRegions[i]))
                            unexploredRegion = adyRegions[i];
                        else
                        {
                            visited[adyRegions[i]] = true;
                            regionsQueue.Enqueue(adyRegions[i]);
                        }
                    }
                }
            }
            if (unexploredRegion == -1)
                unexploredRegion = RandomGenerator.Next(0, world.regionMap.Count);
            return unexploredRegion;
        }
        /// <summary>
        /// Searches for a land position in the eight directions based on the creature position.
        /// Returns false if no position is found, true otherwise.
        /// </summary>
        /// <param name="landPos"> Returns a land position, if it is not founded, the position that returns is not valid</param>
        private bool SearchForLand(out Vector2Int landPos)
        {
            Vector2Int[] Dirs =
            {
                new Vector2Int(-1, -1), new Vector2Int(0, -1), new Vector2Int(1, -1),
                new Vector2Int(-1, 0),                       new Vector2Int(1, 0),
                new Vector2Int(-1, 1), new Vector2Int(0, 1), new Vector2Int(1, 1)
            };
            // The map is sqare, pithagoras to get diagonal, the maximum distance to search
            double mapDiag = Math.Sqrt(2 * Math.Pow(world.map.GetLength(0), 2));
            bool landFound = false;
            int indx = 0;
            int distInc = Math.Max(1, world.chunkSize / 2);
            int rad = distInc;  // TOOD: haciendo saltos de la mitad de tamanio de chunk es menos preciso, se puede poner de 1 a 1.
            Vector2Int creaturePos = new Vector2Int(thisCreature.x, thisCreature.y);
            landPos = new Vector2Int();
            while (!landFound && rad < mapDiag)
            {
                while (indx < Dirs.Length && !landFound)
                {
                    landPos = creaturePos + Dirs[indx] * rad;
                    landFound = thisCreature.world.CanMove(landPos.x, landPos.y);
                    indx++;
                }
                indx = 0;
                rad += distInc;
            }

            return landFound;
        }
        /// <summary>
        /// Saves a close position to the creature if it's a confortable temperature to the creature
        /// </summary>
        public void AddTemperaturePosition()
        {
            int x = thisCreature.x, y = thisCreature.y;

            bool found = false;
            for (int i = -perceptionRadius; i <= perceptionRadius && !found; i++)
            {
                for (int j = -perceptionRadius; j <= perceptionRadius && !found; j++)
                {
                    Vector2Int p = new Vector2Int(x + i, y + j);
                    if (!world.CanMove(p.x, p.y) || !thisCreature.CheckTemperature(p.x, p.y) || SafeTemperaturePositions.Contains(p))
                        continue;

                    SafeTemperaturePositions.Add(p);
                    found = true;

                }
            }
        }

        #region Find explore position
        /// <summary>
        /// Saves the creature's current position if it is different from the last one in the queue.
        /// If a new position is added and the maximum number of positions saved is surpassed, the
        /// least recent one is discarded.
        /// </summary>
        public void AddExplorePosition()
        {
            Vector2Int pos = new Vector2Int(thisCreature.x, thisCreature.y);
            if (explorePositionsRemembered.Count > 0 && explorePositionsRemembered.Peek() == pos) return;

            explorePositionsRemembered.Enqueue(pos);
            if (explorePositionsRemembered.Count > maxPositionsRemembered)
                explorePositionsRemembered.Dequeue();
        }

        /// <summary>
        /// Find the tile that the creature perceive that will deal less damage to him
        /// </summary>
        /// <returns> </returns>
        public Vector2Int BestTemperaturePosition()
        {
            int x = thisCreature.x, y = thisCreature.y;
            double range, damage, lessDamage = Double.PositiveInfinity;
            Vector2Int goal = new Vector2Int();
            bool found = false;
            Vector2Int checkPos = new Vector2Int();
            for (int i = -perceptionRadius; i <= perceptionRadius && !found; i++)
            {
                for (int j = -perceptionRadius; j <= perceptionRadius && !found; j++)
                {
                    checkPos.x = x + i; checkPos.y = y + j;
                    if (!world.CanMove(checkPos.x, checkPos.y) || (i == 0 && j == 0)) continue;

                    double tileTemperature = world.map[checkPos.x, checkPos.y].temperature;
                    double difference = 1;

                    //The difference between the extreme acceptable temperature an the tile temperature is calculated.
                    if (tileTemperature < thisCreature.stats.MinTemperature)
                        difference = thisCreature.stats.MinTemperature - tileTemperature;
                    else if (tileTemperature > thisCreature.stats.MaxTemperature)
                        difference = tileTemperature - thisCreature.stats.MaxTemperature;

                    //A range from 0 to 1 is calculated based on the difference of temperature and a max value for it.
                    range = Math.Min(difference / UniverseParametersManager.parameters.maxTemperatureDifference, 1);
                    //The base damage of being in an area with a temperature that cannot be stand is a porcentage of the max health each tick.
                    //To that, another instance of damage is added depending on how much this temperature supasses that acceptable for the creature.
                    damage = thisCreature.stats.MaxHealth *
                        ((range * (UniverseParametersManager.parameters.maxHealthTemperatureDamage - UniverseParametersManager.parameters.minHealthTemperatureDamage)) +
                        UniverseParametersManager.parameters.minHealthTemperatureDamage);

                    if (damage < lessDamage)
                    {
                        lessDamage = damage;
                        goal.x = checkPos.x;
                        goal.y = checkPos.y;
                    }

                }
            }

            return goal;
        }

        /// <summary>
        /// To find a new unexplored position, the creature goes to
        /// a position following the opposite direction of the average
        /// of the most recent positions that the creature has visited.
        /// It takes the furthest position that it can reach in that direction.
        /// If the position is not reachable, a new position is calculated in a
        /// circumference, with an angle and/or radius adjustment until 
        /// a reachable position its achieved.
        /// </summary>
        public Vector2Int FindNewPosition()
        {
            int x = thisCreature.x;
            int y = thisCreature.y;

            if (!thisCreature.CheckTemperature(x, y) && SafeTemperaturePositions.Count == 0)
                return BestTemperaturePosition();

            Vector3 vector = Vector3.Zero;

            // The average position of this creature's path so far is calculated and the used as a reference
            // to get a direction vector that is used to guide the creature away from places already visited.
            int averageX = x;
            int averageY = y;

            // The dangers encountered, whether good or bad, represent positions that the creature does not want
            // to visit when exploring, either because it is dangerous or because it is a safe place it frequents.
            foreach (Position p in dangersRemembered)
            {
                averageX += p.position.x;
                averageY += p.position.y;
            }
            // These positions are used to further avoid positions previously visited, updated every so often
            // and only when moving as to not stagnate when the creature is static.
            foreach (Vector2Int p in explorePositionsRemembered)
            {
                averageX += p.x;
                averageY += p.y;
            }
            averageX /= dangersRemembered.Count + explorePositionsRemembered.Count + 1;
            averageY /= dangersRemembered.Count + explorePositionsRemembered.Count + 1;
            vector = new Vector3(x - averageX, y - averageY, 0);
            if (vector.X != 0 || vector.Y != 0)
                vector = Vector3.Normalize(vector);

            // If no positions are remebered or the resulting vector is zero, the creature chooses a random direction
            if (vector.Length() == 0)
                vector = RandomDir();

            double radius = perceptionRadius;
            double angleIncrement = Math.PI / 4.0;  // 45 degrees
            double maxAngle = 3.0 * Math.PI / 2.0;
            double angleDesist = 0.08727;   // 5 degrees to stop searching forward
            Vector2Int finalPosition;

            int cont = 0;
            //Find a position to explore that is not water and is far of the vector calculated before
            do
            {
                if (!GetPositionsAtRadius(out finalPosition, vector, radius, angleIncrement, maxAngle))
                {
                    if (++cont % 2 == 0)    // TODO: these numbers are magic
                        angleIncrement /= 2.0;
                    else
                        radius *= 0.75f;
                }
                // If no viable position is found forward after several iterations, it goes back
                if (angleIncrement < angleDesist || radius < 0.5)
                {
                    vector *= -1;
                    radius = perceptionRadius;
                    angleIncrement = Math.PI / 4.0;
                    maxAngle = Math.PI / 2.0;   // 90 degrees, the area that the creature should have come from
                }
            }
            while (!thisCreature.world.CanMove(finalPosition.x, finalPosition.y, thisCreature.creatureLayer) // Repeat if it cannot move to the calculated destiny
                || (finalPosition.x == thisCreature.x && finalPosition.y == thisCreature.y));                // or the destiny is the same position as the creature position

            return finalPosition;
        }


        private Vector3 RandomDir()
        {
            int dirX, dirY;
            do
            {
                dirX = RandomGenerator.Next(2);
                dirY = RandomGenerator.Next(2);
            }
            while (dirX == 0 && dirY == 0);
            return new Vector3(dirX, dirY, 0);
        }
        /// <summary>
        /// Given an initial direction checks the tiles in a radius around the creature and returns
        /// a position where the creature can go. Checks positions increasing and decreasing an angle
        /// describing a circunference. If it does not find any position, it returns false and a smaller radius
        /// or smaller angle increment must be provieded.
        /// </summary>
        /// <param name="finalPosition"> Position where the creature try to go</param>
        /// <param name="dir"> Initial direction to start searching </param>
        /// <param name="radius"> Radius of the circunference </param>
        /// <param name="angleInc"> Angle increment to check positions in a circunference </param>
        private bool GetPositionsAtRadius(out Vector2Int finalPosition, Vector3 dir, double radius, double angleInc, double maxAngle)
        {
            finalPosition = new Vector2Int();
            double dot = Vector3.Dot(dir, Vector3.UnitX);
            double acos = Math.Acos(dot);
            double angleAcum = 0,
                actualAngle = acos;
            int inc = 1;
            do
            {
                finalPosition.x = thisCreature.x + (int)Math.Round(Math.Cos(actualAngle) * radius, MidpointRounding.AwayFromZero);
                finalPosition.y = thisCreature.y + (int)Math.Round(Math.Sin(actualAngle) * radius, MidpointRounding.AwayFromZero);

                inc *= -1;
                angleAcum += angleInc;
                actualAngle += angleAcum * inc;
            }
            while (!thisCreature.world.CanMove(finalPosition.x, finalPosition.y, thisCreature.creatureLayer) && angleAcum <= maxAngle);

            return angleAcum <= maxAngle
                && (finalPosition.x != thisCreature.x || finalPosition.y != thisCreature.y);
        }
        #endregion

        /// <summary>
        /// This method has to be called when day changed to night nad vice versa, to update the radius the creature sees based on the
        /// new perception value, which changes at night.
        /// </summary>
        public void CalculatePerceptionRadius()
        {
            perceptionRadius = thisCreature.stats.Perception;
        }

        /// <summary>
        /// Sets a creature to be the enemy of this one, that is to say, its combat target. This creature is forgotten when it leaves
        /// the perception radius or is dead.
        /// </summary>
        /// <param name="creatureID"> target's ID</param>
        /// <param name="pos"> target's creature</param>
        public void TargetEnemy(int creatureID, Vector3Int pos)
        {
            Enemy = new EntityResource(pos.x, pos.y, pos.z, creatureID, maxExperienceTicks);
        }

        public List<int> NearbyAllies()
        {
            List<int> allies = new List<int>();
            foreach (EntityResource ally in Allies)
            {
                if (thisCreature.DistanceToObjective(ally.position) <= perceptionRadius)
                    allies.Add(ally.ID);
                else
                    break;
            }
            return allies;
        }

        private void RefreshMemory(Resource r, int ticks)
        {
            r.ticks = ticks;
        }

        #region Danger
        /// <summary>
        /// Returns the given position's danger as remembered by the creature.
        /// </summary>
        public float GetPositionDanger(int x, int y)
        {
            float danger = 0;
            foreach (Position p in dangersRemembered)
            {
                int x1, y1;
                x1 = Math.Abs(x - p.position.x);
                y1 = Math.Abs(y - p.position.y);
                int dist = Math.Max(x1, y1);

                if (dist <= dangerRadius)
                    danger += p.Danger() / (float)Math.Pow(2, dist);
            }
            return danger;
        }
        public float GetPositionDanger(Vector3Int p) { return GetPositionDanger(p.x, p.y); }
        public float GetPositionDanger(Vector2Int p) { return GetPositionDanger(p.x, p.y); }

        /// <summary>
        /// Saves the closest mass of water as safe in memory, making the creature prefer it over the closest one, if not too far away.
        /// </summary>
        public void SafeWaterSource()
        {
            Position posDanger = GetFromPositionDangers(WaterPositions[0].position);
            if (posDanger != null) //If the position is already in the list it is updated.
            {
                posDanger.safety -= danger; //It is a safe position, so the danger is negative.
                posDanger.ticks = maxExperienceTicks; //The number of ticks until erasure is reset.
            }
            else //Else it is created and added.
            {
                posDanger = new Position(WaterPositions[0].position, 0, -danger, maxExperienceTicks);
                dangersRemembered.Add(posDanger);
            }

            SafeWaterPositions.Add(WaterPositions[0]);
        }
        /// <summary>
        /// Saves the closest edible plant as safe in memory, making the creature prefer it over the closest one, if not too far away.
        /// </summary>
        public void SafeEdiblePlant()
        {
            if (EdiblePlants.Count == 0) return;

            Position posDanger = GetFromPositionDangers(EdiblePlants[0].position);
            if (posDanger != null) //If the position is already in the list it is updated.
            {
                posDanger.safety -= danger; //It is a safe position, so the danger is negative.
                posDanger.ticks = maxExperienceTicks; //The number of ticks until erasure is reset.
            }
            else //Else it is created and added.
            {
                posDanger = new Position(EdiblePlants[0].position, 0, -danger, maxExperienceTicks);
                dangersRemembered.Add(posDanger);
            }
            if (!SafeEdiblePlants.Contains(EdiblePlants[0]))
                SafeEdiblePlants.Add(EdiblePlants[0]);
            else SafeEdiblePlants.Find(x => x == EdiblePlants[0]).ticks = maxExperienceTicks;
        }
        public void DangerousPosition(bool isDangerous)
        {
            float value = danger;
            if (!isDangerous) value = -danger;
            Vector2Int thisPos = new Vector2Int(thisCreature.x, thisCreature.y);
            Position posDanger = GetFromPositionDangers(thisPos);
            if (posDanger != null) //If the position is already in the list it is updated.
            {
                posDanger.danger += value;
                posDanger.ticks = maxExperienceTicks; //The number of ticks until erasure is reset.
            }
            else //Else it is created and added.
            {
                posDanger = new Position(thisPos, value, 0, maxExperienceTicks);
                dangersRemembered.Add(posDanger);
            }
        }
        #endregion

        #region Lists

        private void AdjustValueResourceList<T>(int max, List<T> list) where T : ValueResource
        {
            list.Sort(valueComparer);
            AdjustList(max, list);
        }
        private void AdjustResourceList<T>(int max, List<T> list) where T : Resource
        {
            list.Sort(resourceComparer);
            if (list.Count > max)
                list.RemoveRange(max, list.Count - max);
        }
        private void AdjustResourceList<T>(int max, List<T> list, IComparer<T> comparer) where T : Resource
        {
            list.Sort(comparer);
            if (list.Count > max)
                list.RemoveRange(max, list.Count - max);
        }
        private void AdjustList<T>(int max, List<T> list) where T : Resource
        {
            if (list.Count > max)
                list.RemoveRange(max, list.Count - max);
        }
        private void AdjustPosList<T>(int max, List<T> list) where T : Vector2Int
        {
            if (list.Count > max)
                list.RemoveRange(max, list.Count - max);
        }
        // TODO: si tarda mucho hacer priority queue
        private void SortAndAdjustLists()
        {
            RemoveFakeInformation(Preys);
            AdjustValueResourceList(maxResourcesRemembered, Preys);

            RemoveFakeInformation(Allies);
            AdjustResourceList(maxResourcesRemembered, Allies);

            RemoveFakeInformation(FreshCorpses);
            AdjustResourceList(maxResourcesRemembered, FreshCorpses);

            RemoveFakeInformation(RottenCorpses);
            AdjustResourceList(maxResourcesRemembered, RottenCorpses);

            AdjustResourceList(maxResourcesRemembered, WaterPositions);
            AdjustResourceList(maxResourcesRemembered, SafeWaterPositions);

            RemoveFruitlessPlants();
            AdjustResourceList(maxResourcesRemembered, EdiblePlants);
            AdjustResourceList(maxResourcesRemembered, SafeEdiblePlants, resourceEdibleComparer);

            SafePositions.Sort(positionComparer);
            AdjustPosList(maxPositionsRemembered, SafePositions);

            SafeTemperaturePositions.Sort(positionComparer);
            AdjustPosList(maxPositionsRemembered, SafeTemperaturePositions);
        }

        private void RemoveFakeInformation<T>(List<T> list) where T : EntityResource
        {
            for (int i = list.Count - 1; i >= 0; i--)
            {
                //If the resource is within sight and yet is has not been updated, that means that it no longer is there, and therefore, lost.
                if (list[i].ticks < maxExperienceTicks && thisCreature.DistanceToObjective(list[i].position) <= perceptionRadius)
                    list.RemoveAt(i);
            }
        }
        private void RemoveFruitlessPlants()
        {
            for (int i = EdiblePlants.Count - 1; i >= 0; i--)
            {
                EdiblePlant plant = world.GetStaticEntity(EdiblePlants[i].ID) as EdiblePlant;

                //If the resource is not a plant or is within sight and without fruit, it is worthless for sure, and therefore not worth remembering.
                //The if hungry condition is there so that the creature only forgets fruitless plants when it is looking for fruit. That way, it does not
                //forget the plant it has eaten from inmediatly after eating its fruit.
                if (plant == null || (plant.eaten && thisCreature.IsHungry() && thisCreature.DistanceToObjective(EdiblePlants[i].position) <= perceptionRadius))
                {
                    RemoveFromSafePlant(EdiblePlants[i].position);
                    EdiblePlants.RemoveAt(i);
                }
            }
        }
        #region Forget
        /// <summary>
        /// Check all the resources if the creature has to forget it 
        /// </summary>
        private void Forget()
        {
            i_forgor(Preys);
            i_forgor(Allies);
            i_forgor(FreshCorpses);
            i_forgor(RottenCorpses);
            i_forgor(WaterPositions);
            i_forgor(SafeWaterPositions);
            i_forgor(EdiblePlants);
            i_forgor(SafeEdiblePlants);

            i_forgor_position(ref father);
            i_forgor_position(ref mother);
            i_forgor(ref menace);

            // Forget regions
            List<int> regionsToForget = new List<int>();
            List<int> storedRegions = new List<int>(exploredRegions.Keys);
            foreach (int region in storedRegions)
            {
                if (--exploredRegions[region] <= 0) //If it is time to forget.
                    regionsToForget.Add(region);
            }
            foreach (int rf in regionsToForget)
            {
                exploredRegions.Remove(rf);
            }

            //The list is iterated through from the end to the start to deal with removing elements from it while iterating.
            for (int i = dangersRemembered.Count - 1; i >= 0; i--)
            {
                Position p = dangersRemembered[i];
                if (--p.ticks <= 0) //If it is time to forget the position.
                {
                    RemoveFromSafeWater(p.position);
                    RemoveFromSafePlant(p.position);
                    dangersRemembered.RemoveAt(i);
                    if (SafePositions.Contains(p.position))
                        SafePositions.Remove(p.position);
                }
            }
        }

        private void i_forgor<T>(List<T> list) where T : Resource
        {
            for (int i = list.Count - 1; i >= 0; i--)
            {
                T p = list[i];
                if (--p.ticks <= 0) //If it is time to forget.
                    list.RemoveAt(i);
            }
        }

        private void i_forgor<T>(ref T res) where T : Resource
        {
            if (res != null && --res.ticks == 0) //If it is time to forget.
                res = null;
        }

        private void i_forgor_position<T>(ref T res) where T : Resource
        {
            if (res != null && --res.ticks == 0) //If it is time to forget.
                res.position = null;
        }
        #endregion

        /// <summary>
        /// Given a position, it is searched for and found in positionDangers, and before being returned, it is removed from the list.
        /// </summary>
        private Position GetFromPositionDangers(Vector2Int pos)
        {
            int index = 0;
            for (; index < dangersRemembered.Count; index++)
            {
                if (dangersRemembered[index].position == pos)
                    break;
            }
            if (index < dangersRemembered.Count)
                return dangersRemembered[index];

            return null;
        }

        private Position GetFromPositionDangers(Vector3Int position)
        {
            return GetFromPositionDangers(new Vector2Int(position.x, position.y));
        }

        /// <summary>
        /// Remove a safe water that is in the given position 
        /// </summary>
        private void RemoveFromSafeWater(Vector2Int pos)
        {
            int index = 0;
            for (; index < SafeWaterPositions.Count; index++)
            {
                if (SafeWaterPositions[index].position.x == pos.x && SafeWaterPositions[index].position.y == pos.y)
                    break;
            }
            if (index < SafeWaterPositions.Count)
                SafeWaterPositions.RemoveAt(index);
        }
        /// <summary>
        /// Remove a safePlant that is in the given position 
        /// </summary>
        private void RemoveFromSafePlant(Vector2Int pos)
        {
            int index = 0;
            for (; index < SafeEdiblePlants.Count; index++)
            {
                if (SafeEdiblePlants[index].position.x == pos.x && SafeEdiblePlants[index].position.y == pos.y)
                    break;
            }
            if (index < SafeEdiblePlants.Count)
                SafeEdiblePlants.RemoveAt(index);
        }
        private void RemoveFromSafePlant(Vector3Int pos)
        {
            int index = 0;
            for (; index < SafeEdiblePlants.Count; index++)
            {
                if (SafeEdiblePlants[index].position == pos)
                    break;
            }
            if (index < SafeEdiblePlants.Count)
                SafeEdiblePlants.RemoveAt(index);
        }


        /// <summary>
        /// Add a resource to a given list and reset his ticks if the list already contains the resource
        /// </summary>
        private void UpdateList<T>(List<T> l, T r, int maxTicks) where T : Resource
        {
            int index = 0;
            for (; index < l.Count; index++)
            {
                if (l[index].Equals(r))
                    break;
            }
            if (index < l.Count)
            {
                l[index].position = r.position; 
                RefreshMemory(l[index], maxTicks);
            }
            else
                l.Add(r);
        }
        #endregion

        public void Shuffle<T>(IList<T> list)
        {
            int n = list.Count;
            while (n > 1)
            {
                n--;
                int k = RandomGenerator.Next(n);
                T value = list[k];
                list[k] = list[n];
                list[n] = value;
            }
        }

        #region Comparators
        /// <summary>
        /// Given a list of edible plants, these are ordered based on distance from it. The shortest goes first.
        /// </summary>
        private class ResourcePositionComparer : Comparer<Resource>
        {
            private Creature creature;
            public ResourcePositionComparer(Creature creature) { this.creature = creature; }

            public override int Compare(Resource a, Resource b)
            {
                int aDist = Math.Abs(creature.x - a.position.x) + Math.Abs(creature.y - a.position.y);
                int bDist = Math.Abs(creature.x - b.position.x) + Math.Abs(creature.y - b.position.y);
                return aDist.CompareTo(bDist);
            }
        }
        /// <summary>
        /// Given a list of edible plants, these are ordered based on if they are perceived
        /// and eaten or the distance from it. The shortest that is not eaten goes first.
        /// </summary>
        private class ResourcePositionEdibleComparer : Comparer<EntityResource>
        {
            private Creature creature;
            public ResourcePositionEdibleComparer(Creature creature) { this.creature = creature; }

            public override int Compare(EntityResource a, EntityResource b)
            {
                StaticEntity aSE = creature.world.GetStaticEntity(a.ID);
                StaticEntity bSE = creature.world.GetStaticEntity(b.ID);
                //check if the creature is perceiveing the plants and their are eaten
                List<StaticEntity> perceivedEntities = creature.world.PerceiveEntities(creature.ID, creature.mind.mem.perceptionRadius);
                if (perceivedEntities.Contains(aSE) && (aSE as EdiblePlant).eaten &&
                    perceivedEntities.Contains(bSE) && (bSE as EdiblePlant).eaten)
                    return 0;
                else if (perceivedEntities.Contains(aSE) && (aSE as EdiblePlant).eaten)
                    return -1;
                else if (perceivedEntities.Contains(bSE) && (bSE as EdiblePlant).eaten)
                    return 1;
                //Otherwise just check the distance
                int aDist = Math.Abs(creature.x - a.position.x) + Math.Abs(creature.y - a.position.y);
                int bDist = Math.Abs(creature.x - b.position.x) + Math.Abs(creature.y - b.position.y);
                return aDist.CompareTo(bDist);
            }
        }
        private class ResourceValueComparer : Comparer<ValueResource>
        {
            public ResourceValueComparer() { }

            public override int Compare(ValueResource a, ValueResource b)
            {
                return a.value.CompareTo(b.value);
            }
        }

        private class PositionComparer : Comparer<Vector2Int>
        {
            private Creature creature;
            public PositionComparer(Creature creature) { this.creature = creature; }

            public override int Compare(Vector2Int a, Vector2Int b)
            {
                int aDist = Math.Abs(creature.x - a.x) + Math.Abs(creature.y - a.y);
                int bDist = Math.Abs(creature.x - b.x) + Math.Abs(creature.y - b.y);
                return aDist.CompareTo(bDist);
            }

        }
        #endregion
    }
}
