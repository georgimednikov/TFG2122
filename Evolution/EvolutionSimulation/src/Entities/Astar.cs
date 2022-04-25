using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace EvolutionSimulation.Entities
{
    public struct DebugPathInfo
    {
        public Vector3[] path;
        public int[] regionPath;
        public Vector3 start, end, borderEnd;
    }

    class GraphNode : IComparable<GraphNode>, IEquatable<GraphNode>
    {
        public GraphNode(Vector3 p, GraphNode pr, double csf = 0, double ec = 0, double ed = 0)
        {
            pos = p;
            prev = pr;
            costSoFar = csf;
            estimatedCost = ec;
            euclidDist = ed;
        }

        public int CompareTo(GraphNode other)
        {
            if (estimatedCost < other.estimatedCost)
                return -1;
            else if (estimatedCost == other.estimatedCost)
                if (other.euclidDist < euclidDist)
                    return -1;
                else if (other.euclidDist == euclidDist)
                    return 0;
            return 1;
        }

        public bool Equals(GraphNode other)
        {
            return pos == other.pos;
        }

        public Vector3 pos;
        public double costSoFar;
        public double estimatedCost;
        public double euclidDist;
        public GraphNode prev;
    }

    public static class Astar
    {
        /// <summary>
        /// Returns a path from path to end using A*.
        /// </summary>
        /// <param name="c">Creature that moves.</param>
        /// <param name="w">World where it happens</param>
        /// <param name="treeDensity">Percentage of trees on the path</param>
        public static Vector3[] GetPath(Creature c, World w, Vector3 start, Vector3 end, out double treeDensity, DebugPathInfo info = default(DebugPathInfo))
        {
#if DEBUG
            Console.WriteLine("Empieza Astar: " + start + " " + end);
#endif
            info.start = start;
            info.end = end;

            if ((end-start).LengthSquared() <= 2)
            {
                Vector3[] path = new Vector3[1];
                path[0] = end;
                treeDensity = 0;
                return path;
            }

            if (end == start) throw new InvalidOperationException("It cannot be the same position");
            Vector3 tempEnd = HighAstar(w, start, end, out int endRegion, info);

            return LowAstar(c, w, start, tempEnd, endRegion, out treeDensity, info);
        }

        private static Vector3 HighAstar(World w, Vector3 start, Vector3 end, out int endRegion, DebugPathInfo info)
        {
            Vector3 nStart = new Vector3(w.map[(int)start.X, (int)start.Y].regionId, 0, 0);
            Vector3 nEnd = new Vector3(w.map[(int)end.X, (int)end.Y].regionId, 0, 0);

            if (nEnd.X != nStart.X)
            {


                List<GraphNode> list = new List<GraphNode>();
                Utils.PriorityQueue<GraphNode> open = new Utils.PriorityQueue<GraphNode>();
                List<GraphNode> closed = new List<GraphNode>();
                GraphNode init = new GraphNode(nStart, null);
                open.Insert(init);

                while (open.Count > 0)
                {
                    GraphNode current = open.RemoveTop();
                    if (closed.Contains(current))
                        continue;
                    closed.Add(current);
                    if (current.pos == nEnd)
                        break;
                    foreach (GraphNode n in HighGetNeighbours(w, current))
                    {
                        n.estimatedCost = current.costSoFar + manhattanHeuristic(w, n.pos, nEnd);
                        if (!open.Contains(n))
                            open.Insert(n);
                    }
                }

                GraphNode aux = closed[closed.Count - 1];
                while (aux != null)
                {
                    list.Add(aux);
                    aux = aux.prev;
                }
                list.Reverse();

                Vector3 finalPos = new Vector3();
                double bestDist = double.MaxValue;

                info.regionPath = new int[list.Count];
                for (int i = 0; i < info.regionPath.Length; i++)
                {
                    info.regionPath[i] = (int)list[i].pos.X;
                }

                List<Vector2> pos = w.highMap[(int)list[1].pos.X].links[(int)nStart.X];

                for (int i = 0; i < pos.Count; i++)
                {
                    Vector3 nPos = new Vector3(pos[i].X, pos[i].Y, 0);
                    if ((nPos - start).Length() < bestDist)
                    {
                        bestDist = (nPos - start).Length();
                        finalPos = nPos;
                    }
                }
                endRegion = (int)list[1].pos.X;
                return finalPos;
            }
            endRegion = -1;
            return end;
        }

        private static Vector3[] LowAstar(Creature c, World w, Vector3 start, Vector3 end, int endRegion, out double treeDensity, DebugPathInfo info)
        {
            List<GraphNode> path = new List<GraphNode>();
            Utils.PriorityQueue<GraphNode> open = new Utils.PriorityQueue<GraphNode>();
            List<GraphNode> closed = new List<GraphNode>();
            GraphNode init = new GraphNode(start, null);
            open.Insert(init);
            double heur = CustomHeuristic(false, c, start, end, w, out treeDensity);
            int thres = c.GetTreeThreshold(treeDensity);
            bool treeBetter = (thres >= 0 && heur >= thres);
            while (open.Count > 0)
            {
                GraphNode current = open.RemoveTop();
                if (closed.Contains(current))
                    continue;
                closed.Add(current);
                if (current.pos == end)
                    break;
                foreach (GraphNode n in LowGetNeighbours(treeBetter, c, w, current, endRegion))
                {
                    n.estimatedCost = current.costSoFar + CustomHeuristic(treeBetter, c, n.pos, end, w, out double partialTreeDensity);
                    Vector3 posToEnd = end - n.pos;
                    n.euclidDist = posToEnd.X + posToEnd.Y;
                    if (!open.Contains(n))
                        open.Insert(n);
                }
            }

            GraphNode aux = closed[closed.Count - 1];
            int passedTrees = 0;
            while (aux != null)
            {
                if (w.IsTree((int)Math.Round(aux.pos.X), (int)Math.Round(aux.pos.Y))) passedTrees = passedTrees + 1;
                path.Add(aux);
                aux = aux.prev;
            }

            treeDensity = ((float)passedTrees) / ((float)path.Count);
            path.Reverse();
            Vector3[] retPath = new Vector3[path.Count - 1];
            for (int i = 1; i < path.Count; ++i)
            {
                retPath[i - 1] = path[i].pos;
            }
            info.path = retPath;
            //Console.WriteLine("Acaba Astar");
            return retPath;
        }

        /// <summary>
        /// Heuritic for A* using linear distance, but accounting on tree movement.
        /// </summary>
        /// <param name="treeBetter">If theoretically tree movement is more efficient than ground</param>
        /// <param name="c">Creature from which stats are taken from</param>
        /// <param name="w">World where it happens</param>
        /// <param name="treeDensity">Percentage of trees theoretically on the path</param>
        /// <returns></returns>
        static double CustomHeuristic(bool treeBetter, Creature c, Vector3 start, Vector3 end, World w, out double treeDensity)
        {
            Vector3 dir = end - start;
            Vector3 dirN = Vector3.Normalize(dir);
            Vector3 dirAux = start;
            int ntiles = 0;
            treeDensity = 0;
            while ((dirAux - end).Length() > 0.6f)
            {
                if (w.IsTree((int)Math.Round(dirAux.X), (int)Math.Round(dirAux.Y)))
                    treeDensity++;
                ntiles++;
                dirAux += dirN;
            }
            if (ntiles != 0) treeDensity /= ntiles;

            double ret = Math.Max(Math.Abs(dir.X), Math.Abs(dir.Y));
            if ((start.Z == (int)Creature.HeightLayer.Tree || end.Z == (int)Creature.HeightLayer.Tree) && treeBetter) ret *= c.stats.GroundSpeed / c.stats.ArborealSpeed;
            else ret *= c.stats.ArborealSpeed / c.stats.GroundSpeed;
            return ret;
        }

        /// <summary>
        /// Gets adjacent tiles and the tile above/under the current position
        /// </summary>
        /// <param name="treeBetter">If theoretically tree movement is more efficient than ground</param>
        /// <param name="c">Creature from which stats are taken from</param>
        /// <param name="w">World where it happens</param>
        /// <param name="n">Current node</param>
        /// <returns></returns>
        static List<GraphNode> LowGetNeighbours(bool treeBetter, Creature c, World w, GraphNode n, int otherId)
        {
            List<GraphNode> neigh = new List<GraphNode>();
            int id = w.map[(int)n.pos.X, (int)n.pos.Y].regionId;
            for (int i = -1; i <= 1; ++i)
                for (int j = -1; j <= 1; ++j)
                {
                    Vector3 newPos;
                    if (i == 0 && j == 0 && w.canMove(newPos = (new Vector3(n.pos.X + i, n.pos.Y + j, n.pos.Z == (int)Creature.HeightLayer.Tree ? (int)Creature.HeightLayer.Ground : (int)Creature.HeightLayer.Tree))))
                        neigh.Add(new GraphNode(newPos, n, n.costSoFar + (treeBetter ? 0 : (int)Creature.HeightLayer.Tree) - c.PositionDanger((int)n.pos.X, (int)n.pos.Y)));
                    else if (w.canMove(newPos = (n.pos + new Vector3(i, j, 0))) && (id == w.map[(int)n.pos.X + i, (int)n.pos.Y + j].regionId || otherId == w.map[(int)n.pos.X + i, (int)n.pos.Y + j].regionId))
                    {
                        double costSoFar = n.costSoFar;
                        if (!w.canMove(newPos)) continue;
                        Plant p = w.map[(int)newPos.X, (int)newPos.Y].plant;
                        if (p is Tree) costSoFar += (2 - Tree.movementPenalty);
                        else if (p is EdibleTree) costSoFar += (2 - EdibleTree.movementPenalty);
                        else costSoFar += 1;
                        neigh.Add(new GraphNode(newPos, n, costSoFar - c.PositionDanger((int)n.pos.X + i, (int)n.pos.Y + j))); //TODO: Danger aqui es problematico 
                    }
                }
            return neigh;
        }

        static List<GraphNode> HighGetNeighbours(World w, GraphNode n)
        {
            List<GraphNode> neigh = new List<GraphNode>();

            foreach (var item in w.highMap[(int)n.pos.X].links.Keys)
                neigh.Add(new GraphNode(new Vector3(item, 0, 0), n, n.costSoFar + 1));

            return neigh;
        }

        static float manhattanHeuristic(World w, Vector3 start, Vector3 end)
        {
            Vector2 start2 = w.highMap[(int)start.X].spawnPoint,
            end2 = w.highMap[(int)end.X].spawnPoint;
            return (Math.Abs(end2.X - start2.X) + Math.Abs(end2.Y - start2.Y));
        }

        /// <summary>
        /// Returns a straight line using Bresenham algorithm.
        /// </summary>
        public static Vector3[] GetAirPath(Vector3 start, Vector3 end)
        {
            int x0 = (int)start.X, x1 = (int)end.X, y0 = (int)start.Y, y1 = (int)end.Y;
            return GetPointsOnLine(x0, y0, x1, y1, (int)end.Z);
        }

        /// <summary>
        /// Bresenham algorithm
        /// </summary>
        static Vector3[] GetPointsOnLine(int x0, int y0, int x1, int y1, int z1)
        {
            List<Vector3> path = new List<Vector3>();
            bool steep = Math.Abs(y1 - y0) > Math.Abs(x1 - x0);
            if (steep)
            {
                int t;
                t = x0; // swap x0 and y0
                x0 = y0;
                y0 = t;
                t = x1; // swap x1 and y1
                x1 = y1;
                y1 = t;
            }
            int dy = Math.Abs(y1 - y0);
            int ystep = (y0 < y1) ? 1 : -1;
            int y = y0;
            int dx = Math.Abs(x1 - x0);
            int error = dx / 2;
            if (x0 > x1) // Going left
            {
                for (int x = x0; x >= x1; x--)
                {
                    AddToPath(path, steep, dx, dy, ref error, ystep, ref y, x);
                }
            }
            else // Going right
            {
                for (int x = x0; x <= x1; x++)
                {
                    AddToPath(path, steep, dx, dy, ref error, ystep, ref y, x);
                }
            }
            path.Add(new Vector3((steep ? y1 : x1), (steep ? x1 : y1), z1));
            return path.ToArray();
        }

        private static void AddToPath(List<Vector3> path, bool steep, int dx, int dy, ref int error, int ystep, ref int y, int x)
        {
            path.Add(new Vector3((steep ? y : x), (steep ? x : y), 2));
            error = error - dy;
            if (error < 0)
            {
                y += ystep;
                error += dx;
            }
        }
    }
}
