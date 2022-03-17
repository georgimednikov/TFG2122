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

        private class Resource
        {
            public Vector2Int position;
            public int ticks;

            public Resource(Vector2Int p, int t)
            {
                position = p;
                ticks = t;
            }
        }

        private class EntityResource : Resource
        {
            public int ID;

            public EntityResource(Vector2Int p, int id, int t) : base(p, t) { ID = id; }
            public EntityResource(int x, int y, int id, int t) : base(new Vector2Int(x, y), t) { ID = id; }
        }

        Creature thisCreature;
        World world;
        int maxExperienceTicks;
        int maxResourcesRemembered;
        int perceptionRadius;
        int dangerRadius;

        float danger;

        EntityResource enemy;             //Creature that has attacked this creature or an ally of its.
        EntityResource rival;             //Closest creature that is not part of the creature's "family" regarding its species.
        EntityResource father;
        EntityResource mother;
        List<EntityResource> preys;              //Closest reachable creature.
        List<EntityResource> mates;              //Closest ally in heat.
        List<EntityResource> nearbyAllies;

        List<EntityResource> freshCorpses;
        List<EntityResource> rottenCorpses;

        List<Position> positionsRemembered;     //All the positions the creature remembers, with their dangers and ticks left.
        Vector2Int safePlace;             //The closest safe location for the creature.

        List<Resource> water;
        List<Resource> safeWater;             //List containing all safe water spots that the creature remembers.

        List<EntityResource> plants;
        List<EntityResource> safePlants;           //List containing all safe edible plants that the creature remembers.

        ResourcePositionComparer resourceComparer;


        #region Getters
        public Creature Rival() { return world.GetCreature(rival.ID); }
        public Creature Enemy()
        {
            if (enemy != null) return world.GetCreature(enemy.ID);
            return world.GetCreature(preys.ID);
        }
        public Creature Father() { return world.GetCreature(father.ID); }
        public Vector2Int FatherPosition() { return father.position; }

        public Creature Mother() { return world.GetCreature(mother.ID); }
        public Creature Mate() { return world.GetCreature(mates.ID); }
        public Creature Ally() { return world.GetCreature(allies[0].ID); }
        public Corpse FreshCorpse() { return world.GetStaticEntity(freshCorpses[0].ID) as Corpse; }
        public Corpse RottenCorpse() { return world.GetCreature(rottenCorpses.ID); }
        //TODO: Coger agua
        public Vector2Int Water()
        {
            if (safeWater.Count == 0) return water;
            int distClose = thisCreature.DistanceToObjective(water);
            int distSafe = thisCreature.DistanceToObjective(safeWater[0]);

            if (distSafe > distClose * UniverseParametersManager.parameters.safePrefferedOverClosestResourceRatio)
                return water;
            else
                return safeWater[0];
        }
        public EdiblePlant Plant()
        {
            if (safePlants.Count == 0) return plants;
            int distClose = thisCreature.DistanceToObjective(plants);
            int distSafe = thisCreature.DistanceToObjective(safePlants[0]);

            if (distSafe > distClose * UniverseParametersManager.parameters.safePrefferedOverClosestResourceRatio)
                return plants;
            else
                return safePlants[0];
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
            positionsRemembered = new List<Position>();
            water = new List<Resource>();
            safeWater = new List<Resource>();
            plants = new List<EntityResource>();
            safePlants = new List<EntityResource>();
           
            maxExperienceTicks = thisCreature.stats.Knowledge * UniverseParametersManager.parameters.knowledgeTickMultiplier;
            dangerRadius = (int)((thisCreature.chromosome.GetFeatureMax(Genetics.CreatureFeature.Aggressiveness) - thisCreature.stats.Aggressiveness) * UniverseParametersManager.parameters.aggressivenessToRadiusMultiplier);
            danger = thisCreature.chromosome.GetFeatureMax(Genetics.CreatureFeature.Aggressiveness) * UniverseParametersManager.parameters.experienceMaxAggresivenessMultiplier;
            CalculatePerceptionRadius();
        }

        public void Update()
        {
            int x = thisCreature.x, y = thisCreature.y;

            //If there is a creature targeted as the enemy and this creature loses sight or the creature dies the pointer is reset.
            if (enemy != null) {
                Creature enemyEntity = world.GetCreature(enemy.ID);
                if (enemyEntity == null || thisCreature.DistanceToObjective(enemyEntity) > perceptionRadius)
                    enemy = null;
                else
                    enemy.position = new Vector2Int(enemyEntity.x, enemyEntity.y);
            }

            Forget();


            int safePlaceDist = 0;
            //The list is iterated through from the end to the start to deal with removing elements from it while iterating.
            for (int i = positionsRemembered.Count - 1; i >= 0; i--)
            {
                Position p = positionsRemembered[i];
                if (--p.ticks <= 0) //If it is time to forget the position.
                {
                    RemoveFromSafeWater(p.position);
                    RemoveFromSafePlant(p.position);
                    positionsRemembered.RemoveAt(i);
                }
                //If the tile remains in memory, it is safe and no safe place has been assigned or it is closer than the one already found, it is saved.
                else if (p.Danger() <= 0 && (safePlace == null || safePlaceDist > thisCreature.DistanceToObjective(p.position)))
                {
                    safePlace = p.position;
                    safePlaceDist = thisCreature.DistanceToObjective(p.position);
                }
            }

            List<Creature> perceivedCreatures = world.PerceiveCreatures(thisCreature.ID, perceptionRadius);
            List<StaticEntity> perceivedEntities = world.PerceiveEntities(thisCreature.ID, perceptionRadius);

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
                        AddToList(water, new Resource(p, maxExperienceTicks));

                    bool positionWasUnknown = false;
                    Position position = GetFromPositionDangers(p);
                    if (position == null)
                    {
                        positionWasUnknown = true;
                        position = new Position(p, 0, 0, 0);
                    }
                    position.intimidation = 0; //The danger caused by creatures in this position is reset because it is going to be account for again later.
                    position.ticks = maxExperienceTicks; //The number of ticks left to be forgotten is reset too.

                    //The list of creatures is also needed to calculate danger so it is done here too.
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
                            int dist = thisCreature.DistanceToObjective(creature);
                            if (!AddToList(nearbyAllies, resource))
                                RefreshMemory(resource, maxExperienceTicks);
                            if (creature.ID == father.ID)
                                RefreshMemory(father, maxExperienceTicks);
                            else if (creature.ID == mother.ID)
                                RefreshMemory(mother, maxExperienceTicks);
                            else if (creature.wantMate)
                                AddToList(mates, resource);
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
                                AddToList(preys, resource);

                            position.intimidation += creature.stats.Intimidation;
                        }
                    }

                    //Saves the corpses in a tile and throws away the rest.
                    foreach (StaticEntity entity in perceivedEntities)
                    {
                        if (!(entity is Corpse)) continue;
                        else
                        {
                            EntityResource resource = new EntityResource(entity.x, entity.y, entity.ID, maxExperienceTicks);
                            Corpse newCorpse = entity as Corpse;//TODO cambiar 0.4
                            if (thisCreature.HasAbility(Genetics.CreatureFeature.Scavenger, 0.4f) || newCorpse.Edible)
                                AddToList(freshCorpses, resource);

                            else
                                AddToList(rottenCorpses, resource);
                        }
                    }

                    //If this position was not remembered and there is valuable information in it (a danger level) it is remembered.
                    if (positionWasUnknown && position.intimidation != 0) positionsRemembered.Add(position);
                }
            }
            AdjustLists();
        }

        // TODO: si tarda mucho hacer priority queue
        private void AdjustLists()
        {
            preys.Sort(resourceComparer); 
            preys.RemoveRange(maxResourcesRemembered, preys.Count);
            mates.Sort(resourceComparer);
            mates.RemoveRange(maxResourcesRemembered, mates.Count);
            nearbyAllies.Sort(resourceComparer);
            nearbyAllies.RemoveRange(maxResourcesRemembered, nearbyAllies.Count);
            freshCorpses.Sort(resourceComparer);
            freshCorpses.RemoveRange(maxResourcesRemembered, freshCorpses.Count);
            rottenCorpses.Sort(resourceComparer);
            rottenCorpses.RemoveRange(maxResourcesRemembered, rottenCorpses.Count);
            water.Sort(resourceComparer);
            water.RemoveRange(maxResourcesRemembered, water.Count);
            safeWater.Sort(resourceComparer);
            safeWater.RemoveRange(maxResourcesRemembered, safeWater.Count);
            plants.Sort(resourceComparer);
            plants.RemoveRange(maxResourcesRemembered, plants.Count);
            safePlants.Sort(resourceComparer);
            safePlants.RemoveRange(maxResourcesRemembered, safePlants.Count);
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
            //TODO olvidar padre, madre y demas que no sea una lista
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
        private Vector2Int FindNewPlace()
        {
            int x = thisCreature.x;
            int y = thisCreature.y;
            int averageX = 0;
            int averageY = 0;
            foreach (Position p in positionsRemembered)
            {
                averageX += p.position.x;
                averageY += p.position.y;
            }
            averageX /= positionsRemembered.Count;
            averageY /= positionsRemembered.Count;

            Vector3 vector = new Vector3(x - averageX, y - averageY, 0);
            vector /= vector.Length();

            //int cont = 0;
            //while (creature.world.map[finalPosition.x, finalPosition.y].isWater)
            //{
            //    //The increment has to have the same sign as cont to add their values without possible substractions,
            //    //but cont's sign has to be mantained to alternate between going "left" or "right" realtive to the current sector.
            //    int inc = 1; if (cont < 0) inc *= -1;
            //    cont = (cont + inc) * -1;
            //    sector = (sector + cont) % 8;
            //    finalPosition = SectorToPosition(posToDrink, sector);
            //}
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

        private void RefreshMemory(Resource r, int mt)
        {
            r.ticks = mt;
        }

        #region Danger
        /// <summary>
        /// Returns the given position's danger as remembered by the creature.
        /// </summary>
        public float GetPositionDanger(int x, int y)
        {
            float danger = 0;
            foreach (Position p in positionsRemembered)
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
                positionsRemembered.Add(posDanger);
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
                positionsRemembered.Add(posDanger);
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
                positionsRemembered.Add(posDanger);
            }
        }
        #endregion

        #region Lists

        /// <summary>
        /// Given a position, it is searched for and found in positionDangers, and before being returned, it is removed from the list.
        /// </summary>
        private Position GetFromPositionDangers(Vector2Int pos)
        {
            int index = 0;
            for (; index < positionsRemembered.Count; index++)
            {
                if (positionsRemembered[index].position == pos)
                    break;
            }
            if (index < positionsRemembered.Count)
                return positionsRemembered[index];

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
        /// Add a resource to a given list
        /// </summary>
        /// <returns> Return false if the list contains the resource </returns>
        private bool AddToList<T>(List<T> l, T r) where T : Resource
        {
            if (l.Contains(r)) return false;
            l.Add(r);
            return true;
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
    }
}
