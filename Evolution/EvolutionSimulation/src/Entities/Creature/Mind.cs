using System;
using System.Collections.Generic;
using System.Linq;

namespace EvolutionSimulation.Entities
{
    class Mind
    {
        Memory mem;
        Creature creature;
        World world;

        EntityResource parentToFollow;
        EntityResource worthyPrey;
        EntityResource worthyCorpse;
        EntityResource worthyPlant;
        Vector2Int worthyWaterPosition;

        public Mind(Creature creature, int father, int mother)
        {
            this.creature = creature;
            world = creature.world;
            mem = new Memory(creature, father, mother);
            // Follow randomly the father or the mother
            if (RandomGenerator.Next(0, 2) == 0)
                parentToFollow = mem.Father;
            else
                parentToFollow = mem.Mother;
        }

        #region Memory
        public void CreateDanger() { mem.DangerousPosition(); }
        public void SafeWaterSource() { mem.SafeWaterSource(); }
        public void SafeEdiblePlant() { mem.SafeEdiblePlant(); }
        public void UpdatePerception() { mem.CalculatePerceptionRadius(); }
        public void TargetEnemy(int cID) { mem.TargetEnemy(world.GetCreature(cID)); }
        public float PositionDanger(int x, int y) { return mem.GetPositionDanger(x, y); }
        public List<int> NearbyAllies() { return mem.NearbyAllies(); }
        public int FatherID() { return mem.Father.ID; }
        public int MotherID() { return mem.Mother.ID; }
        #endregion

        #region Updates
        /// <summary>
        /// Given the creature's condition and memory, calculates the resources that it should
        /// prioritize based on their distance and danger.
        /// </summary>
        public void UpdatePriorities()
        {
            UpdateParent();
            UpdatePrey();
            UpdateCorpse();
            UpdateWaterSource();
            UpdatePlant();
        }

        /// <summary>
        /// This method change the parent to follow. The creature will always follow the same parent
        /// nevertheless it dead, then if the creature remember where is the other parent, it will 
        /// follow it.
        /// Also if is not following anyone but remember a parent (the creature perceive a parent)
        /// it will start follow it too.
        /// </summary>
        private void UpdateParent()
        {
            // The parent that the creature is following has died
            if (parentToFollow != null && world.GetCreature(parentToFollow.ID) == null)
            {
                // The mother is who has died
                if (mem.Mother != null && mem.Mother.ID == parentToFollow.ID)
                    parentToFollow = mem.Father;
                // The father is who has died
                else if (mem.Father != null && mem.Father.ID == parentToFollow.ID)
                    parentToFollow = mem.Mother;
                else // Just in case the creature doesn't have parents
                    parentToFollow = null;
            }// If the creature doesn't has a parent to follow but suddenly remember a parent
             // (the creature has seen a parent), then it start to follow the parent
            else if((parentToFollow != null && parentToFollow.ticks <= 0) || parentToFollow == null)
            {
                // Remember where is the mother
                if(mem.Mother != null && mem.Mother.ticks > 0)
                    parentToFollow = mem.Mother;
                // Remember where is the father
                else if (mem.Father != null && mem.Father.ticks > 0)
                    parentToFollow = mem.Father;
            }
        }
        private void UpdatePrey()
        {
            if (mem.Preys.Count == 0) //If there are no preys, null.
            {
                worthyPrey = null;
                return;
            }
            //For every posible prey (that being a creature this one wants to challenge) the best one based
            //on their ratio size/distance from this creature is chosen.
            float bestValue = 0;
            foreach (EntityResource prey in mem.Preys)
            {
                float preyValue = world.GetCreature(prey.ID).stats.Size / creature.DistanceToObjective(prey.position);
                if (preyValue > bestValue)
                {
                    worthyPrey = prey;
                    bestValue = preyValue;
                }
            }
        }
        private void UpdateCorpse()
        {
            if (mem.FreshCorpses.Count == 0 && mem.RottenCorpses.Count == 0) //If there are no corpses, null.
            {
                worthyCorpse = null;
                return;
            }
            else if (mem.FreshCorpses.Count == 0) //If there are no fresh corpses, it only eats a rotten one (if there is one) if desperate.
            {
                if (creature.IsVeryHungry()) worthyCorpse = mem.RottenCorpses[0];
                else worthyCorpse = null;
                return;
            }
            else //Else the creature goes to the closest fresh corpse it knows.
            {
                worthyCorpse = mem.FreshCorpses[0];
            }
        }
        private void UpdateWaterSource()
        {
            if (mem.WaterPositions.Count == 0 && mem.SafeWaterPositions.Count == 0)
            {
                worthyWaterPosition = null;
                return;
            }
            if (creature.IsVeryThirsty())
            {
                if (mem.WaterPositions.Count != 0)
                    worthyWaterPosition = mem.WaterPositions[0].position;
                else
                    worthyWaterPosition = mem.SafeWaterPositions[0].position;
                return;
            }

            worthyWaterPosition = BestBetweenCloseAndSafe(mem.WaterPositions, mem.SafeWaterPositions, Criteria).position; 
        }
        private void UpdatePlant()
        {
            if (mem.EdiblePlants.Count == 0 && mem.SafeEdiblePlants.Count == 0)
            {
                worthyPlant = null;
                return;
            }
            if (creature.IsVeryHungry())
            {
                if (mem.EdiblePlants.Count != 0)
                    worthyPlant = mem.EdiblePlants[0];
                else
                    worthyPlant = mem.SafeEdiblePlants[0];
                return;
            }

            worthyPlant = BestBetweenCloseAndSafe(mem.EdiblePlants, mem.SafeEdiblePlants, Criteria);
        }
        private T BestBetweenCloseAndSafe<T>(List<T> close, List<T> safe, Func<T, T, T> criteria) where T : Resource
        {
            if (close.Count == 0 && safe.Count == 0)
                return null;

            T bestClose = BestResourceInList(close, criteria);
            T bestSafe = BestResourceInList(safe, criteria);
            return BestResource(bestClose, bestSafe, criteria);
        }
        private T BestResourceInList<T>(List<T> l, Func<T, T, T> criteria) where T : Resource
        {
            if (l.Count == 0) return null;
            T best = l[0];
            for (int i = 1; i < l.Count; i++)
            {
                best = BestResource(best, l[i], criteria);
            }
            return best;
        }
        private T BestResource<T>(T a, T b, Func<T, T, T> criteria) where T : Resource
        {
            if (a == null) return b;
            if (b == null) return a;
            return criteria(a, b);
        }
        private T Criteria<T>(T w1, T w2) where T : Resource
        {
            float aDanger = mem.GetPositionDanger(w1.position);
            float bDanger = mem.GetPositionDanger(w2.position);
            float total = Math.Max(Math.Max(aDanger, bDanger), 0.1f);
            float aRelDanger = aDanger / total;
            float bRelDanger = bDanger / total;


            float aDist = creature.DistanceToObjective(w1.position);
            float bDist = creature.DistanceToObjective(w2.position);
            total = Math.Max(Math.Max(aDist, bDist), 0.1f);
            float aRelDist = aDist / total;
            float bRelDist = bDist / total;

            if (aRelDanger + aRelDist < bRelDanger + bRelDist)
                return w1;
            else
                return w2;
        }
        #endregion

        #region Getters
        public bool Enemy(out int id, out Vector2Int position) { return AssignEntityInfo(mem.Enemy, out id, out position); }
        public bool Menace(out int id, out Vector2Int position) { return AssignEntityInfo(mem.Menace, out id, out position); }
        public bool Parent(out int id, out Vector2Int position) { return AssignEntityInfo(parentToFollow, out id, out position); }
        public bool Prey(out int id, out Vector2Int position) { return AssignEntityInfo(worthyPrey, out id, out position); }
        public bool Ally(out int id, out Vector2Int position) { return AssignEntityInfo(mem.Allies[0], out id, out position); }
        public bool Mate(out int id, out Vector2Int position) { return AssignEntityInfo(mem.Mate, out id, out position); }
        public bool Corpse(out int id, out Vector2Int position) { return AssignEntityInfo(worthyCorpse, out id, out position); }
        public bool Plant(out int id, out Vector2Int position) { return AssignEntityInfo(worthyPlant, out id, out position); }
        public Vector2Int WaterPosition() { return worthyWaterPosition; }
        public Vector2Int SafePosition() { return mem.SafePosition; }
        public Vector2Int NewPosition() { return mem.NewPosition; }

        private bool AssignEntityInfo(EntityResource ent, out int id, out Vector2Int position)
        {
            if (ent == null)
            {
                id = -1; position = new Vector2Int(-1, -1);
                return false;
            }
            id = ent.ID; position = ent.position;
            return true;
        }
        #endregion
    }

    public class Resource : IEquatable<Resource>
    {
        public Vector2Int position;
        public int ticks;

        public Resource(Vector2Int p, int t) { position = p; ticks = t; }

        public static bool operator ==(Resource eR1, Resource eR2)
        {
            if (eR1 is null)
                return eR2 is null;
            return eR1.position == eR2.position;
        }
        public static bool operator !=(Resource eR1, Resource eR2) { return !(eR1 == eR2); }
        public override bool Equals(object obj) { return Equals(obj as Resource); }
        public virtual bool Equals(Resource obj) { return this == obj; }
        public override int GetHashCode() { return base.GetHashCode(); }
    }

    public class EntityResource : Resource
    {
        public int ID;

        public EntityResource(Vector2Int p, int id, int t) : base(p, t) { ID = id; }
        public EntityResource(int x, int y, int id, int t) : base(new Vector2Int(x, y), t) { ID = id; }

        public static bool operator ==(EntityResource eR1, EntityResource eR2)
        {
            if (eR1 is null)
                return eR2 is null;
            return eR1.ID == eR2.ID;
        }
        public static bool operator !=(EntityResource eR1, EntityResource eR2) { return !(eR1 == eR2); }
        public override bool Equals(object obj) { return Equals(obj as EntityResource); }
        public override bool Equals(Resource obj) { return this == obj as EntityResource; }
        public override int GetHashCode() { return base.GetHashCode(); }
    }
}
