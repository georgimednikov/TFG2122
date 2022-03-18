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

        private class Resource : IEquatable<Resource>
        {
            public Vector2Int position;
            public int ticks;

            public Resource(Vector2Int p, int t) { position = p; ticks = t; }

            public static bool operator ==(Resource eR1, Resource eR2) { return eR1.position == eR2.position; }
            public static bool operator !=(Resource eR1, Resource eR2) { return eR1.position != eR2.position; }
            public override bool Equals(object obj) { return Equals(obj as Resource); }
            public virtual bool Equals(Resource obj) { return this == obj; }
            public override int GetHashCode() { return base.GetHashCode(); }
        }

        private class EntityResource : Resource
        {
            public int ID;

            public EntityResource(Vector2Int p, int id, int t) : base(p, t) { ID = id; }
            public EntityResource(int x, int y, int id, int t) : base(new Vector2Int(x, y), t) { ID = id; }

            public static bool operator ==(EntityResource eR1, EntityResource eR2) { return eR1.ID == eR2.ID; }
            public static bool operator !=(EntityResource eR1, EntityResource eR2) { return eR1.ID != eR2.ID; }
            public override bool Equals(object obj) { return Equals(obj as EntityResource); }
            public override bool Equals(Resource obj) { return this == obj as EntityResource; }
            public override int GetHashCode() { return base.GetHashCode(); }
        }

        Creature thisCreature;
        World world;
        int perceptionRadius;
        int dangerRadius;
        int maxExperienceTicks;
        int maxResourcesRemembered;
        int maxPositionsRemembered;
        int ticksToSavePosition;
        int ticksElapsed;

        float danger;

        EntityResource enemy;                        //Creature that has attacked this creature or an ally of its.
        EntityResource rival;                        //Closest creature that is not part of the creature's "family" regarding its species.
        EntityResource father;
        EntityResource mother;
        List<EntityResource> preys;                  //Closest reachable creature.
        List<EntityResource> mates;                  //Closest ally in heat.
        List<EntityResource> nearbyAllies;

        List<EntityResource> freshCorpses;
        List<EntityResource> rottenCorpses;

        List<Position> dangersRemembered;            //All the dangers the creature remembers, with their dangers and ticks left.
        Queue<Vector2Int> explorePositionsRemembered;   //All the dangers the creature remembers, with their dangers and ticks left.
        Vector2Int safePlace;                        //The closest safe location for the creature.

        List<Resource> water;
        List<Resource> safeWater;                    //List containing all safe water spots that the creature remembers.

        List<EntityResource> plants;
        List<EntityResource> safePlants;             //List containing all safe edible plants that the creature remembers.

        ResourcePositionComparer resourceComparer;

        #region Getters
        /// <summary>
        /// Returns rivals ID, and its last seen position
        /// </summary>
        public void Rival(out int id, out Vector2Int position)
        {
            if (rival == null)
            {
                id = -1;
                position = null;
            }
            else
            {
                id = rival.ID;
                position = rival.position;
            }
        }

        public void Enemy(out int id, out Vector2Int position)
        {
            if (enemy == null)
            {
                id = -1;
                position = null;
            }
            else
            {
                id = enemy.ID;
                position = enemy.position;
            }
        }

        public void Father(out int id, out Vector2Int position)
        {
            if (father == null)
            {
                id = -1;
                position = null;
            }
            else
            {
                id = father.ID;
                position = father.position;
            }
        }

        public void Mother(out int id, out Vector2Int position)
        {
            if (mother == null)
            {
                id = -1;
                position = null;
            }
            else
            {
                id = mother.ID;
                position = mother.position;
            }
        }
        public void Mate(out int id, out Vector2Int position)
        {
            if (mates.Count <= 0)
            {
                id = -1;
                position = null;
            }
            else
            {
                id = mates[0].ID;
                position = mates[0].position;
            }
        }
        public void Ally(out int id, out Vector2Int position)
        {
            if (nearbyAllies.Count <= 0)
            {
                id = -1;
                position = null;
            }
            else
            {
                id = rival.ID;
                position = rival.position;
            }
        }

        public void FreshCorpse(out int id, out Vector2Int position)
        {
            if (freshCorpses.Count <= 0)
            {
                id = -1;
                position = null;
            }
            else
            {
                id = freshCorpses[0].ID;
                position = freshCorpses[0].position;
            }
        }

        public void RottenCorpse(out int id, out Vector2Int position)
        {
            if (rottenCorpses.Count <= 0)
            {
                id = -1;
                position = null;
            }
            else
            {
                id = rottenCorpses[0].ID;
                position = rottenCorpses[0].position;
            }
        }

        //TODO: Coger agua
        public Vector2Int Water()
        {
            if (safeWater.Count <= 0)
            {
                if (water.Count > 0)
                    return water[0].position;
                return null;
            }
            else if (water.Count <= 0)
                return safeWater[0].position;

            //water and safewater
            int distClose = thisCreature.DistanceToObjective(water[0].position);
            int distSafe = thisCreature.DistanceToObjective(safeWater[0].position);

            if (distSafe > distClose * UniverseParametersManager.parameters.safePrefferedOverClosestResourceRatio)
                return water[0].position;
            else
                return safeWater[0].position;
        }
        public void Plant(out int id, out Vector2Int position)
        {
            if (safePlants.Count <= 0)
            {
                if (plants.Count > 0)
                {
                    id = plants[0].ID;
                    position = plants[0].position;
                }
                else
                {
                    id = -1;
                    position = null;
                }
                return;
            }
            else if (water.Count <= 0)
            {
                id = safePlants[0].ID;
                position = safePlants[0].position;
                return;
            }

            int distClose = thisCreature.DistanceToObjective(plants[0].position);
            int distSafe = thisCreature.DistanceToObjective(safePlants[0].position);

            if (distSafe > distClose * UniverseParametersManager.parameters.safePrefferedOverClosestResourceRatio)
            {
                id = plants[0].ID;
                position = plants[0].position;
            }
            else
            {
                id = safePlants[0].ID;
                position = safePlants[0].position;
            }
        }
        public Vector2Int SafePlace() { return safePlace; }
        public Vector2Int NewPlace() { return FindNewPlace(); }
        #endregion

        public Memory(Creature c, World w, Creature f, Creature m)
        {
            thisCreature = c;
            world = w;
            father = new EntityResource(f.x, f.y, f.ID, maxExperienceTicks);
            mother = new EntityResource(m.x, m.y, m.ID, maxExperienceTicks);

            preys = new List<EntityResource>();
            mates = new List<EntityResource>();
            nearbyAllies = new List<EntityResource>();
            freshCorpses = new List<EntityResource>();
            rottenCorpses = new List<EntityResource>();
            dangersRemembered = new List<Position>();
            explorePositionsRemembered = new Queue<Vector2Int>();
            water = new List<Resource>();
            safeWater = new List<Resource>();
            plants = new List<EntityResource>();
            safePlants = new List<EntityResource>();

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
            if (enemy != null)
            {
                Creature enemyEntity = world.GetCreature(enemy.ID);
                if (enemyEntity == null || thisCreature.DistanceToObjective(enemyEntity) > perceptionRadius)
                    enemy = null;
                else
                    enemy.position = new Vector2Int(enemyEntity.x, enemyEntity.y);
            }

            // If the tile has to be forgotten, it is removed from the creature's memory
            Forget();

            // The following code also forgets, but since it does not deal with entities rather positions and danger
            // it also serves to assign the safe place of the creature.
            int safePlaceDist = 0;
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
                //If the tile remains in memory, it is safe and no safe place has been assigned or it is closer than the one already found, it is saved.
                else if (GetPositionDanger(p.position) <= 0 && (safePlace == null || safePlaceDist > thisCreature.DistanceToObjective(p.position)))
                {
                    safePlace = p.position;
                    safePlaceDist = thisCreature.DistanceToObjective(p.position);
                }
            }

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
                    UpdateList(nearbyAllies, resource, maxExperienceTicks);
                    if (creature.ID == father.ID)
                        RefreshMemory(father, maxExperienceTicks);
                    else if (creature.ID == mother.ID)
                        RefreshMemory(mother, maxExperienceTicks);
                    else if (creature.wantMate)
                        UpdateList(mates, resource, maxExperienceTicks);
                }
                //Else they have no relation
                else
                {
                    int dist = thisCreature.DistanceToObjective(creature);
                    if (creature.stats.Intimidation > thisCreature.stats.Aggressiveness &&
                        (rival == null || thisCreature.DistanceToObjective(rival.position) >= dist)) //This is equal to update the rival information if it is the same
                    {
                        rival = resource;
                    }

                    //If the creature is reachable and not considered too dangerous it is considered possible prey.
                    float creatureDanger = GetPositionDanger(creature.x, creature.y);
                    if ((creature.creatureLayer == Creature.HeightLayer.Air && thisCreature.stats.AirReach) ||
                        creature.creatureLayer == Creature.HeightLayer.Tree && thisCreature.stats.TreeReach ||
                        creature.creatureLayer == Creature.HeightLayer.Ground &&
                        creatureDanger < thisCreature.stats.Aggressiveness)
                        UpdateList(preys, resource, maxExperienceTicks);

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
                        UpdateList(freshCorpses, resource, maxExperienceTicks);
                    else
                        UpdateList(rottenCorpses, resource, maxExperienceTicks);
                }
                else if (entity is EdiblePlant && !(entity as EdiblePlant).eaten)
                {
                    UpdateList(plants, resource, maxExperienceTicks);
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
                        UpdateList(water, new Resource(p, maxExperienceTicks), maxExperienceTicks);

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
        private Vector2Int FindNewPlace()
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
            enemy = new EntityResource(creature.x, creature.y, creature.ID, maxExperienceTicks);
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
            Position posDanger = GetFromPositionDangers(water[0].position);
            if (posDanger != null) //If the position is already in the list it is updated.
            {
                posDanger.safety -= danger; //It is a safe position, so the danger is negative.
                posDanger.ticks = maxExperienceTicks; //The number of ticks until erasure is reset.
            }
            else //Else it is created and added.
            {
                posDanger = new Position(water[0].position, 0, -danger, maxExperienceTicks);
                dangersRemembered.Add(posDanger);
            }

            safeWater.Add(water[0]);
        }
        /// <summary>
        /// Saves the closest edible plant as safe in memory, making the creature prefer it over the closest one, if not too far away.
        /// </summary>
        public void SafePlant()
        {
            Position posDanger = GetFromPositionDangers(plants[0].position);
            if (posDanger != null) //If the position is already in the list it is updated.
            {
                posDanger.safety -= danger; //It is a safe position, so the danger is negative.
                posDanger.ticks = maxExperienceTicks; //The number of ticks until erasure is reset.
            }
            else //Else it is created and added.
            {
                posDanger = new Position(plants[0].position, 0, -danger, maxExperienceTicks);
                dangersRemembered.Add(posDanger);
            }

            safePlants.Add(plants[0]);
        }
        public void DangerousPosition()
        {
            Vector2Int thisPos = new Vector2Int(thisCreature.x, thisCreature.y);
            Position posDanger = GetFromPositionDangers(thisPos);
            if (posDanger != null) //If the position is already in the list it is updated.
            {
                posDanger.danger += danger; //It is a safe position, so the danger is negative.
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
            RemoveFakeInformation(preys);
            preys.Sort(resourceComparer);
            preys.RemoveRange(maxResourcesRemembered, preys.Count);

            RemoveFakeInformation(mates);
            mates.Sort(resourceComparer);
            mates.RemoveRange(maxResourcesRemembered, mates.Count);

            RemoveFakeInformation(nearbyAllies);
            nearbyAllies.Sort(resourceComparer);
            nearbyAllies.RemoveRange(maxResourcesRemembered, nearbyAllies.Count);

            RemoveFakeInformation(freshCorpses);
            freshCorpses.Sort(resourceComparer);
            freshCorpses.RemoveRange(maxResourcesRemembered, freshCorpses.Count);

            RemoveFakeInformation(rottenCorpses);
            rottenCorpses.Sort(resourceComparer);
            rottenCorpses.RemoveRange(maxResourcesRemembered, rottenCorpses.Count);

            water.Sort(resourceComparer);
            water.RemoveRange(maxResourcesRemembered, water.Count);
            safeWater.Sort(resourceComparer);
            safeWater.RemoveRange(maxResourcesRemembered, safeWater.Count);

            RemoveFruitlessPlants();
            plants.Sort(resourceComparer);
            plants.RemoveRange(maxResourcesRemembered, plants.Count);
            safePlants.Sort(resourceComparer);
            safePlants.RemoveRange(maxResourcesRemembered, safePlants.Count);
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
            for (int i = plants.Count - 1; i <= 0; i--)
            {
                EdiblePlant plant = world.GetStaticEntity(plants[i].ID) as EdiblePlant;

                //If the resource is not a plant or is within sight and without fruit, it is worthless for sure, and therefore not worth remembering.
                if (plant == null || (plant.eaten && thisCreature.DistanceToObjective(plants[i].position) <= perceptionRadius))
                    plants.RemoveAt(i);

                //TODO: Safe plants que?
            }
        }

        private void Forget()
        {
            i_forgor(preys);
            i_forgor(mates);
            i_forgor(nearbyAllies);
            i_forgor(freshCorpses);
            i_forgor(rottenCorpses);
            i_forgor(water);
            i_forgor(safeWater);
            i_forgor(plants);
            i_forgor(safePlants);

            i_forgor_position(ref father);
            i_forgor_position(ref mother);
            i_forgor(ref rival);
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
            for (; index < safeWater.Count; index++)
            {
                if (safeWater[index].position == pos)
                    break;
            }
            safeWater.RemoveAt(index);
        }
        private void RemoveFromSafePlant(Vector2Int pos)
        {
            int index = 0;
            for (; index < safePlants.Count; index++)
            {
                if (safePlants[index].position == pos)
                    break;
            }
            safePlants.RemoveAt(index);
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
