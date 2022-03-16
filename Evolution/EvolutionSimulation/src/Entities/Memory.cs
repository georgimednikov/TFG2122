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
        int maxExperienceTicks;
        int perceptionRadius;
        int dangerRadius;

        float danger;

        Creature rival;             //Closest creature that is not part of the creature's "family" regarding its species.
        Creature prey;              //Closest reachable creature.
        Creature enemy;             //Creature that has attacked this creature or an ally of its.
        Creature father;
        Creature mother;
        Creature mate;              //Closest ally in heat.
        Creature ally;              //Closest member of the creature's "family" regarding its species.
        List<Creature> nearbyAllies;

        Corpse freshCorpse;
        Corpse rottenCorpse;
        EdiblePlant plant;

        Vector2Int water;
        Vector2Int safePlace;       //The closest safe location for the creature.

        //All the positions the creature remembers, with their dangers and ticks left.
        List<Position> positionsRemembered;

        //List containing all safe water spots that the creature remembers.
        List<Vector2Int> safeWater;
        PositionComparer positionComparer;

        //List containing all safe edible plants that the creature remembers.
        List<EdiblePlant> safePlants;
        EntityPositionComparer plantComparer;

        #region Getters
        public Creature Rival() { return rival; }
        public Creature Enemy()
        {
            if (enemy != null) return enemy;
            return prey;
        }
        public Creature Father() { return father; }
        public Creature Mother() { return mother; }
        public Creature Mate() { return mate; }
        public Creature Ally() { return ally; }
        public Corpse FreshCorpse() { return freshCorpse; }
        public Corpse RottenCorpse() { return rottenCorpse; }
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
            if (safePlants.Count == 0) return plant;
            int distClose = thisCreature.DistanceToObjective(plant);
            int distSafe = thisCreature.DistanceToObjective(safePlants[0]);

            if (distSafe > distClose * UniverseParametersManager.parameters.safePrefferedOverClosestResourceRatio)
                return plant;
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
            father = f;
            mother = m;

            nearbyAllies = new List<Creature>();
            positionsRemembered = new List<Position>();
            safeWater = new List<Vector2Int>();
            safePlants = new List<EdiblePlant>();

            maxExperienceTicks = thisCreature.stats.Knowledge * UniverseParametersManager.parameters.knowledgeTickMultiplier;
            dangerRadius = (int)((thisCreature.chromosome.GetFeatureMax(Genetics.CreatureFeature.Aggressiveness) - thisCreature.stats.Aggressiveness) * UniverseParametersManager.parameters.aggressivenessToRadiusMultiplier);
            danger = thisCreature.chromosome.GetFeatureMax(Genetics.CreatureFeature.Aggressiveness) * UniverseParametersManager.parameters.experienceMaxAggresivenessMultiplier;
            CalculatePerceptionRadius();
        }

        public void Update()
        {
            int x = thisCreature.x, y = thisCreature.y;

            //TODO: MIRAR SI ESTA MUERTA!!!!!!!!!!!!!!!!!
            //If there is a creature targeted as the enemy and this creature loses sight or the creature dies the pointer is reset.
            if (enemy != null && thisCreature.DistanceToObjective(enemy) > perceptionRadius)
                enemy = null;

            safePlace = null;
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
            safeWater.Sort(positionComparer);
            safePlants.Sort(plantComparer);

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

                    bool positionWasUnknown = false;
                    Position position = GetFromPositionDangers(p);
                    if (position == null)
                    {
                        positionWasUnknown = true;
                        position = new Position(p, 0, 0, 0);
                    }
                    position.intimidation = 0; //The danger caused by creatures in this position is reset because it is going to be account for again later.
                    position.ticks = maxExperienceTicks; //The number of ticks left to be forgotten is reset too.

                    int closestAllyDist = int.MaxValue;
                    int closestMateDist = int.MaxValue;
                    int closestRivalDist = int.MaxValue;
                    //The list of creatures is also needed to calculate danger so it is done here too.
                    foreach (Creature creature in perceivedCreatures)
                    {
                        //If a creature is the same species as this creature or
                        //it belongs to a child species of this creature's or
                        //it belongs to a parent species of this creature's they're allies.
                        if (creature.speciesName == thisCreature.speciesName ||
                            creature.progenitorSpeciesName == thisCreature.speciesName ||
                            creature.speciesName == thisCreature.progenitorSpeciesName)
                        {
                            int dist = thisCreature.DistanceToObjective(creature);
                            nearbyAllies.Add(creature);
                            if (closestAllyDist < dist)
                            {
                                ally = creature;
                                closestAllyDist = dist;
                            }
                            if (closestMateDist < dist)
                            {
                                mate = creature;
                                closestMateDist = dist;
                            }
                        }
                        //Else they have no relation
                        else
                        {
                            if (creature.stats.Intimidation > thisCreature.stats.Aggressiveness &&
                                (closestRivalDist > thisCreature.DistanceToObjective(creature)))
                            {
                                rival = creature;
                                closestRivalDist = thisCreature.DistanceToObjective(creature);
                            }

                            //If the creature is reachable and not considered too dangerous it is considered possible prey.
                            float creatureDanger = GetPositionDanger(creature.x, creature.y);
                            if ((creature.creatureLayer == Creature.HeightLayer.Air && thisCreature.stats.AirReach) ||
                                creature.creatureLayer == Creature.HeightLayer.Tree && thisCreature.stats.TreeReach ||
                                creature.creatureLayer == Creature.HeightLayer.Ground &&
                                creatureDanger <= thisCreature.stats.Aggressiveness)
                                prey = creature;

                            position.intimidation += creature.stats.Intimidation;
                        }
                    }

                    float freshCorpseValue = int.MaxValue;
                    float rottenCorpseValue = int.MaxValue;

                    //Saves the corpses in a tile and throws away the rest.
                    foreach (StaticEntity entity in perceivedEntities)
                    {
                        if (!(entity is Corpse)) continue;
                        else if (entity.x == x + i && entity.y == y + j)
                        {
                            Corpse newCorpse = entity as Corpse;
                            float value; float dist = thisCreature.DistanceToObjective(newCorpse);
                            if (dist == 0) dist = 0.01f; //In order to not divide by 0, since distances are int it should not matter.
                            value = newCorpse.NutritionalValue(thisCreature) / dist; 

                            if (thisCreature.stats.Scavenger > 0 || newCorpse.Edible)
                            {
                                if (freshCorpseValue < value)
                                {
                                    freshCorpse = newCorpse;
                                    freshCorpseValue = value;
                                }
                            }
                            else
                            {
                                if (rottenCorpseValue < value)
                                {
                                    rottenCorpse = newCorpse;
                                    rottenCorpseValue = value;
                                }
                            }
                        }
                    }

                    //If this position was not remembered and there is valuable information in it (a danger level) it is remembered.
                    if (positionWasUnknown && position.intimidation != 0) positionsRemembered.Add(position);
                }
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
            enemy = creature;
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
            Position posDanger = GetFromPositionDangers(water);
            if (posDanger != null) //If the position is already in the list it is updated.
            {
                posDanger.safety -= danger; //It is a safe position, so the danger is negative.
                posDanger.ticks = maxExperienceTicks; //The number of ticks until erasure is reset.
            }
            else //Else it is created and added.
            {
                posDanger = new Position(water, 0, -danger, maxExperienceTicks);
                positionsRemembered.Add(posDanger);
            }

            safeWater.Add(water);
        }
        /// <summary>
        /// Saves the closest edible plant as safe in memory, making the creature prefer it over the closest one, if not too far away.
        /// </summary>
        public void SafePlant()
        {
            Vector2Int plantPos = new Vector2Int(plant.x, plant.y);
            Position posDanger = GetFromPositionDangers(plantPos);
            if (posDanger != null) //If the position is already in the list it is updated.
            {
                posDanger.safety -= danger; //It is a safe position, so the danger is negative.
                posDanger.ticks = maxExperienceTicks; //The number of ticks until erasure is reset.
            }
            else //Else it is created and added.
            {
                posDanger = new Position(plantPos, 0, -danger, maxExperienceTicks);
                positionsRemembered.Add(posDanger);
            }

            safePlants.Add(plant);
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
            safeWater.Remove(pos);
        }
        private void RemoveFromSafePlant(Vector2Int pos)
        {
            int index = 0;
            for (; index < safePlants.Count; index++)
            {
                EdiblePlant p = safePlants[index];
                if (p.x == pos.x && p.y == pos.y)
                    break;
            }
            safeWater.RemoveAt(index);
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

        #region Comparators
        /// <summary>
        /// Given a list of positions, these are ordered based on distance from it. The shortest goes first.
        /// </summary>
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
        /// <summary>
        /// Given a list of edible plants, these are ordered based on distance from it. The shortest goes first.
        /// </summary>
        private class EntityPositionComparer : Comparer<IEntity>
        {
            private Creature creature;
            public EntityPositionComparer(Creature creature) { this.creature = creature; }

            public override int Compare(IEntity a, IEntity b)
            {
                int aDist = Math.Abs(creature.x - a.x) + Math.Abs(creature.y - a.y);
                int bDist = Math.Abs(creature.x - b.x) + Math.Abs(creature.y - b.y);
                return aDist.CompareTo(bDist);
            }
        }
        #endregion
    }
}
