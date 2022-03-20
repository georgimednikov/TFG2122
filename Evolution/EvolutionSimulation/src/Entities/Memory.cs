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

            public float Danger() { return intimidation + danger + safety; }
        }

        Creature thisCreature;
        World world;

        int perceptionRadius;
        int dangerRadius;
        float danger;                   //Value representing the danger remembered in a position when something bad happens to the creature.

        int maxExperienceTicks;         //Max number of ticks until the creature forgets.
        int maxResourcesRemembered;     //Max number of resources of each type that the creature can remember.
        int maxPositionsRemembered;     //Max number of positions the creature can remember to have their danger in mind.

        int ticksToSavePosition;        //Amount of ticks after which the current position of the creature (if different to the previous) is saved.
        int ticksElapsed;               //How many ticks have gone by, used along with ticksToSavePosition.

        ResourcePositionComparer resourceComparer;
        Queue<Vector2Int> explorePositionsRemembered;   //All the dangers the creature remembers, with their dangers and ticks left.
        List<Position> dangersRemembered;               //All the dangers the creature remembers, with their dangers and ticks left.

        public EntityResource Enemy { get; private set; }               //Creature that has attacked this creature or an ally of its.
        public EntityResource Menace { get; private set; }              //Closest creature that is not part of the creature's "family" regarding its species.
        public EntityResource Father { get; private set; }
        public EntityResource Mother { get; private set; }
        public List<EntityResource> Preys { get; private set; }         //Closest reachable creature.
        public List<EntityResource> NearbyAllies { get; private set; }
        public List<EntityResource> Mates { get; private set; }         //Closest ally in heat.
        public List<EntityResource> FreshCorpses { get; private set; }
        public List<EntityResource> RottenCorpses { get; private set; }
        public List<Resource> WaterPositions { get; private set; }
        public List<Resource> SafeWaterPositions { get; private set; }        //List containing all safe water spots that the creature remembers.
        public List<EntityResource> EdiblePlants { get; private set; }
        public List<EntityResource> SafeEdiblePlants { get; private set; }  //List containing all safe edible plants that the creature remembers.
        public Vector2Int SafePosition { get; private set; }            //The closest safe location for the creature.
        public Vector2Int NewPosition { get => FindNewPosition(); }


        public Memory(Creature c, Creature f, Creature m)
        {
            thisCreature = c;
            world = c.world;
            Father = new EntityResource(f.x, f.y, f.ID, maxExperienceTicks);
            Mother = new EntityResource(m.x, m.y, m.ID, maxExperienceTicks);

            Preys = new List<EntityResource>();
            Mates = new List<EntityResource>();
            NearbyAllies = new List<EntityResource>();
            FreshCorpses = new List<EntityResource>();
            RottenCorpses = new List<EntityResource>();
            dangersRemembered = new List<Position>();
            explorePositionsRemembered = new Queue<Vector2Int>();
            WaterPositions = new List<Resource>();
            SafeWaterPositions = new List<Resource>();
            EdiblePlants = new List<EntityResource>();
            SafeEdiblePlants = new List<EntityResource>();

            resourceComparer = new ResourcePositionComparer(c);

            maxResourcesRemembered = 5; //TODO: Esto bien
            maxPositionsRemembered = 10; //TODO: Esto bien
            ticksToSavePosition = 10; //TODO: Esto bien en base a action points?
            ticksElapsed = 0;
            maxExperienceTicks = thisCreature.stats.Knowledge * UniverseParametersManager.parameters.knowledgeTickMultiplier;
            dangerRadius = (int)((thisCreature.chromosome.GetFeatureMax(Genetics.CreatureFeature.Aggressiveness) - thisCreature.stats.Aggressiveness) * UniverseParametersManager.parameters.aggressivenessToRadiusMultiplier);
            danger = thisCreature.chromosome.GetFeatureMax(Genetics.CreatureFeature.Aggressiveness) * UniverseParametersManager.parameters.experienceMaxAggresivenessMultiplier;
            CalculatePerceptionRadius();
        }

        public void Update()
        {
            int x = thisCreature.x, y = thisCreature.y;

            if (ticksElapsed++ == ticksToSavePosition)
            {
                ticksElapsed = 0;
                AddExplorePosition();
            }

            //If there is a creature targeted as the enemy and this creature loses sight or the creature dies the pointer is reset.
            if (Enemy != null)
            {
                Creature enemyEntity = world.GetCreature(Enemy.ID);
                if (enemyEntity == null || thisCreature.DistanceToObjective(enemyEntity) > perceptionRadius)
                    Enemy = null;
                else
                    Enemy.position = new Vector2Int(enemyEntity.x, enemyEntity.y);
            }

            // If the tile has to be forgotten, it is removed from the creature's memory
            Forget();

            List<Creature> perceivedCreatures = world.PerceiveCreatures(thisCreature.ID, perceptionRadius);
            List<StaticEntity> perceivedEntities = world.PerceiveEntities(thisCreature.ID, perceptionRadius);


            //The list of creatures is also needed to calculate danger so it is done here too.
            float[,] intimidationFelt = new float[perceptionRadius, perceptionRadius]; //The danger in each tile seen by the creature caused by other creatures.
            foreach (Creature creature in perceivedCreatures)
            {
                EntityResource resource = new EntityResource(creature.x, creature.y, creature.ID, maxExperienceTicks);

                //If a creature is the same species as this creature or
                //it belongs to a child species of this creature's or
                //it belongs to a parent species of this creature's they're allies.
                if (creature.speciesName == thisCreature.speciesName ||
                    creature.progenitorSpeciesName == thisCreature.speciesName ||
                    creature.speciesName == thisCreature.progenitorSpeciesName)
                {
                    UpdateList(NearbyAllies, resource, maxExperienceTicks);
                    if (creature.ID == Father.ID)
                        RefreshMemory(Father, maxExperienceTicks);
                    else if (creature.ID == Mother.ID)
                        RefreshMemory(Mother, maxExperienceTicks);
                    else if (creature.wantMate)
                        UpdateList(Mates, resource, maxExperienceTicks);
                }
                //Else they have no relation
                else
                {
                    int dist = thisCreature.DistanceToObjective(creature);
                    if (creature.stats.Intimidation > thisCreature.stats.Aggressiveness &&
                        (Menace == null || thisCreature.DistanceToObjective(Menace.position) >= dist)) //This is equal to update the rival information if it is the same
                    {
                        Menace = resource;
                    }

                    //If the creature is reachable and not considered too dangerous it is considered possible prey.
                    float creatureDanger = GetPositionDanger(creature.x, creature.y);
                    if ((creature.creatureLayer == Creature.HeightLayer.Air && thisCreature.stats.AirReach) ||
                        creature.creatureLayer == Creature.HeightLayer.Tree && thisCreature.stats.TreeReach ||
                        creature.creatureLayer == Creature.HeightLayer.Ground &&
                        creatureDanger < thisCreature.stats.Aggressiveness)
                        UpdateList(Preys, resource, maxExperienceTicks);

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
                EntityResource resource = new EntityResource(entity.x, entity.y, entity.ID, maxExperienceTicks);
                if (entity is Corpse)
                {
                    Corpse newCorpse = entity as Corpse;//TODO cambiar 0.4
                    if (thisCreature.HasAbility(Genetics.CreatureFeature.Scavenger, 0.4f) || newCorpse.Edible)
                        UpdateList(FreshCorpses, resource, maxExperienceTicks);
                    else
                        UpdateList(RottenCorpses, resource, maxExperienceTicks);
                }
                else if (entity is EdiblePlant && !(entity as EdiblePlant).eaten)
                {
                    UpdateList(EdiblePlants, resource, maxExperienceTicks);
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
                    if (IsOutOfBounds(p)) continue;

                    if (world.map[p.x, p.y].isWater)
                        UpdateList(WaterPositions, new Resource(p, maxExperienceTicks), maxExperienceTicks);

                    Position position = GetFromPositionDangers(p);
                    bool positionWasKnown = position == null;
                    if (!positionWasKnown)
                        position = new Position(p, 0, 0, 0);

                    position.ticks = maxExperienceTicks; //The number of ticks left to be forgotten is reset too.
                    position.intimidation = intimidationFelt[i + perceptionRadius, j + perceptionRadius];

                    //If this position was not remembered and there is valuable information in it (a danger level) it is remembered.
                    if (!positionWasKnown && position.intimidation != 0) dangersRemembered.Add(position);
                }
            }

            // The following code also forgets, but since it does not deal with entities rather positions and danger
            // it also serves to assign the safe place of the creature.
            int safePlaceDist = 0;
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
                //If the tile remains in memory, it is safe and no safe place has been assigned or it is closer than the one already found, it is saved.
                else if (positionDanger <= 0 && (SafePosition == null || safePlaceDist > thisCreature.DistanceToObjective(p.position)))
                {
                    SafePosition = p.position;
                    safePlaceDist = thisCreature.DistanceToObjective(p.position);
                }
            }

            SortAndAdjustLists();
        }
        /// <summary>
        /// Saves the creature's current position if it is different from the last one in the queue.
        /// If a new position is added and the maximum number of positions saved is surpassed, the
        /// least recent one is discarded.
        /// </summary>
        public void AddExplorePosition()
        {
            Vector2Int pos = new Vector2Int(thisCreature.x, thisCreature.y);
            if (explorePositionsRemembered.Peek() == pos) return;

            explorePositionsRemembered.Enqueue(pos);
            if (explorePositionsRemembered.Count > maxPositionsRemembered)
                explorePositionsRemembered.Dequeue();
        }
        /// <summary>
        /// 
        /// </summary>
        private Vector2Int FindNewPosition()
        {

            // The average position of this creature's path so far is calculated and the used as a reference
            // to get a direction vector that is used to guide the creature away from places already visited.
            int x = thisCreature.x;
            int y = thisCreature.y;
            int averageX = 0;
            int averageY = 0;

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
            averageX /= dangersRemembered.Count + explorePositionsRemembered.Count;
            averageY /= dangersRemembered.Count + explorePositionsRemembered.Count;

            Vector3 vector = new Vector3(x - averageX, y - averageY, 0);
            vector /= vector.Length();

            float radius = perceptionRadius;
            float degreesInc = (float)(Math.PI / 4.0);  // 45 degrees

            Vector2Int targetPosition = new Vector2Int(thisCreature.x + (int)(vector.X * radius), thisCreature.y + (int)(vector.Y * radius));
            Vector2Int finalPosition = new Vector2Int(targetPosition.x, targetPosition.y);
            int cont = 0;
            //Find a position to explore that is not water and is far of the vector calculated before
            do
            {
                if (!GetPositionsAtRadius(ref finalPosition, degreesInc))
                {
                    if (++cont % 2 == 0)
                        degreesInc /= 2;
                    else
                        targetPosition *= 0.75f;

                    finalPosition.x = targetPosition.x; finalPosition.y = targetPosition.y;
                }
            }
            while (thisCreature.world.map[finalPosition.x, finalPosition.y].isWater);

            return finalPosition;
        }
        /// <summary>
        /// Find a point to explore given a radius that is not water.
        /// </summary>
        /// <param name="finalPosition"> position where the creature try to go</param>
        /// <param name="angleInc"></param>
        /// <returns>Returns false if search in all directions and always find water</returns>
        private bool GetPositionsAtRadius(ref Vector2Int finalPosition, float angleInc)
        {
            float angle = angleInc;
            int inc = 1;
            while (thisCreature.world.map[finalPosition.x, finalPosition.y].isWater && angle <= 360)
            {
                float actualAngle = angle * inc;
                Vector2Int old = finalPosition;
                finalPosition.x = (int)(finalPosition.x * Math.Cos(actualAngle) - finalPosition.y * Math.Sin(actualAngle));
                finalPosition.y = (int)(old.x * Math.Sin(actualAngle) + old.y * Math.Cos(actualAngle));

                inc *= -1;
                angle += angleInc;
            }
            return angle <= 360;
        }
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
        /// <param name="creature"></param>
        public void TargetEnemy(Creature creature)
        {
            Enemy = new EntityResource(creature.x, creature.y, creature.ID, maxExperienceTicks);
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
                int dist = (int)Math.Sqrt(Math.Pow(x1, 2) + Math.Pow(y1, 2));

                if (dist <= dangerRadius)
                    danger += p.Danger() / (float)Math.Pow(2, dist);
            }
            return danger;
        }
        public float GetPositionDanger(Vector2Int p) { return GetPositionDanger(p.x, p.y); }

        /// <summary>
        /// Saves the closest mass of water as safe in memory, making the creature prefer it over the closest one, if not too far away.
        /// </summary>
        public void SafeWater()
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
        public void SafePlant()
        {
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

            SafeEdiblePlants.Add(EdiblePlants[0]);
        }
        public void DangerousPosition()
        {
            Vector2Int thisPos = new Vector2Int(thisCreature.x, thisCreature.y);
            Position posDanger = GetFromPositionDangers(thisPos);
            if (posDanger != null) //If the position is already in the list it is updated.
            {
                posDanger.danger += danger;
                posDanger.ticks = maxExperienceTicks; //The number of ticks until erasure is reset.
            }
            else //Else it is created and added.
            {
                posDanger = new Position(thisPos, danger, 0, maxExperienceTicks);
                dangersRemembered.Add(posDanger);
            }
        }
        #endregion

        #region Lists
        // TODO: si tarda mucho hacer priority queue
        private void SortAndAdjustLists()
        {
            RemoveFakeInformation(Preys);
            Preys.Sort(resourceComparer);
            Preys.RemoveRange(maxResourcesRemembered, Preys.Count);

            RemoveFakeInformation(Mates); //TODO: Quita bien a uno que deja de estar en celo?
            Mates.Sort(resourceComparer);
            Mates.RemoveRange(maxResourcesRemembered, Mates.Count);

            RemoveFakeInformation(NearbyAllies);
            NearbyAllies.Sort(resourceComparer);
            NearbyAllies.RemoveRange(maxResourcesRemembered, NearbyAllies.Count);

            RemoveFakeInformation(FreshCorpses);
            FreshCorpses.Sort(resourceComparer);
            FreshCorpses.RemoveRange(maxResourcesRemembered, FreshCorpses.Count);

            RemoveFakeInformation(RottenCorpses);
            RottenCorpses.Sort(resourceComparer);
            RottenCorpses.RemoveRange(maxResourcesRemembered, RottenCorpses.Count);

            WaterPositions.Sort(resourceComparer);
            WaterPositions.RemoveRange(maxResourcesRemembered, WaterPositions.Count);
            SafeWaterPositions.Sort(resourceComparer);
            SafeWaterPositions.RemoveRange(maxResourcesRemembered, SafeWaterPositions.Count);

            RemoveFruitlessPlants();
            EdiblePlants.Sort(resourceComparer);
            EdiblePlants.RemoveRange(maxResourcesRemembered, EdiblePlants.Count);
            SafeEdiblePlants.Sort(resourceComparer);
            SafeEdiblePlants.RemoveRange(maxResourcesRemembered, SafeEdiblePlants.Count);
        }

        private void RemoveFakeInformation(List<EntityResource> list)
        {
            for (int i = list.Count - 1; i <= 0; i--)
            {
                //If the resource is within sight and yet is has not been updated, that means that it no longer is there, and therefore, lost.
                if (list[i].ticks < maxExperienceTicks && thisCreature.DistanceToObjective(list[i].position) <= perceptionRadius)
                    list.RemoveAt(i);
            }
        }
        private void RemoveFruitlessPlants()
        {
            for (int i = EdiblePlants.Count - 1; i <= 0; i--)
            {
                EdiblePlant plant = world.GetStaticEntity(EdiblePlants[i].ID) as EdiblePlant;

                //If the resource is not a plant or is within sight and without fruit, it is worthless for sure, and therefore not worth remembering.
                //The if hungry condition is there so that the creature only forgets fruitless plants when it is looking for fruit. That way, it does not
                //forget the plant it has eaten from inmediatly after eating its fruit.
                if (plant == null || (plant.eaten && thisCreature.IsHungry() && thisCreature.DistanceToObjective(EdiblePlants[i].position) <= perceptionRadius))
                {
                    EdiblePlants.RemoveAt(i);
                    RemoveFromSafePlant(EdiblePlants[i].position);
                }
            }
        }

        private void Forget()
        {
            i_forgor(Preys);
            i_forgor(Mates);
            i_forgor(NearbyAllies);
            i_forgor(FreshCorpses);
            i_forgor(RottenCorpses);
            i_forgor(WaterPositions);
            i_forgor(SafeWaterPositions);
            i_forgor(EdiblePlants);
            i_forgor(SafeEdiblePlants);

            i_forgor_position(ref Father);
            i_forgor_position(ref Mother);
            i_forgor(ref Menace);

            //The list is iterated through from the end to the start to deal with removing elements from it while iterating.
            for (int i = dangersRemembered.Count - 1; i >= 0; i--)
            {
                Position p = dangersRemembered[i];
                if (--p.ticks <= 0) //If it is time to forget the position.
                {
                    RemoveFromSafeWater(p.position);
                    RemoveFromSafePlant(p.position);
                    dangersRemembered.RemoveAt(i);
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
            if (--res.ticks == 0) //If it is time to forget.
                res = null;
        }

        private void i_forgor_position<T>(ref T res) where T : Resource
        {
            if (--res.ticks == 0) //If it is time to forget.
                res.position = null;
        }

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
        private void RemoveFromSafeWater(Vector2Int pos)
        {
            int index = 0;
            for (; index < SafeWaterPositions.Count; index++)
            {
                if (SafeWaterPositions[index].position == pos)
                    break;
            }
            if (index < SafeWaterPositions.Count)
                SafeWaterPositions.RemoveAt(index);
        }
        private void RemoveFromSafePlant(Vector2Int pos)
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
                if (l[index].Equals(r))//TODO probar que se llame al operator equals de EntityResource tb
                    break;
            }
            if (index < l.Count)
            {
                l[index].position = r.position; //TODO Actualizar posicion?
                RefreshMemory(l[index], maxTicks);
            }
            else
                l.Add(r);
        }
        #endregion

        #region AuxiliaryMethods
        /// <summary>
        /// Checks if a position is outside the world, in other words, if the position given is valid.
        /// </summary>
        private bool IsOutOfBounds(Vector2Int p)
        {
            return p.x < 0 || p.y < 0 || p.x >= world.map.GetLength(0) || p.y >= world.map.GetLength(1);
        }
        #endregion

        #region Comparator
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
        #endregion
    }
}
