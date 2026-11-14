using System;
using System.Collections.Generic;

namespace OfficeRampage
{
    // Conservative floor grid shared by NPC movement and executable tests. No baked NavMesh required.
    public sealed class OfficeNavigation
    {
        private const float Step = .4f;
        private readonly int nx, nz;
        private readonly float left;
        private readonly float length;
        private readonly bool[] reachable;
        private readonly List<int> cells = new List<int>();
        private readonly int[] dx = { -1, 1, 0, 0 }, dz = { 0, 0, -1, 1 };
        public int Count { get { return cells.Count; } }

        public OfficeNavigation(OfficeLayout layout)
        {
            left = -layout.width / 2 + .6f;
            length = layout.length;
            nx = (int)((layout.width - 1.2f) / Step) + 1;
            nz = (int)((layout.length - 1.2f) / Step) + 1;
            reachable = new bool[nx * nz];
            var obstacles = new List<OfficeLayout.Area>(layout.walls);
            foreach (var prop in layout.furniture) obstacles.Add(prop.Footprint);
            bool[] blocked = new bool[nx * nz];
            for (int i = 0; i < blocked.Length; i++)
            {
                var point = Point(i);
                var body = new OfficeLayout.Area(point.x, point.z, .92f, .92f);
                foreach (var obstacle in obstacles)
                    if (obstacle.Overlaps(body)) { blocked[i] = true; break; }
            }
            int origin = -1;
            float nearest = float.MaxValue;
            for (int i = 0; i < blocked.Length; i++)
            {
                if (blocked[i]) continue;
                var p = Point(i);
                float distance = p.x * p.x + (p.z - 3) * (p.z - 3);
                if (distance < nearest) { nearest = distance; origin = i; }
            }
            if (origin < 0) throw new InvalidOperationException("Office has no navigable floor");
            var queue = new Queue<int>(); queue.Enqueue(origin); reachable[origin] = true;
            while (queue.Count > 0)
            {
                int cell = queue.Dequeue(); cells.Add(cell);
                for (int d = 0; d < 4; d++)
                {
                    int next = Neighbor(cell, d);
                    if (next >= 0 && !blocked[next] && !reachable[next])
                    { reachable[next] = true; queue.Enqueue(next); }
                }
            }
        }

        private int Neighbor(int cell, int d)
        {
            int x = cell % nx + dx[d], z = cell / nx + dz[d];
            return x < 0 || x >= nx || z < 0 || z >= nz ? -1 : z * nx + x;
        }
        private OfficeLayout.Point Point(int cell)
        { return new OfficeLayout.Point(left + cell % nx * Step, .6f + cell / nx * Step); }
        public OfficeLayout.Point RandomPoint(Random random) { return Point(cells[random.Next(cells.Count)]); }
        public List<OfficeLayout.Point> SpawnPoints(int count, Random random, bool reserveBoss = false, float separation = 2f)
        {
            var result = new List<OfficeLayout.Point>();
            for (int i = 0; i < count; i++)
            {
                bool found = false;
                for (int attempt = 0; attempt < 3000; attempt++)
                {
                    var p = RandomPoint(random);
                    if (p.z < 5 || p.z > length - 4) continue;
                    if (reserveBoss && p.x * p.x + (p.z - (length - 5)) * (p.z - (length - 5)) < 9) continue;
                    bool overlaps = false;
                    foreach (var previous in result)
                        if ((previous.x - p.x) * (previous.x - p.x) + (previous.z - p.z) * (previous.z - p.z) < separation * separation)
                        { overlaps = true; break; }
                    if (overlaps) continue;
                    result.Add(p); found = true; break;
                }
                if (!found) throw new InvalidOperationException("No safe defender spawn");
            }
            return result;
        }
        private int Nearest(float x, float z)
        {
            int best = cells[0]; float distance = float.MaxValue;
            foreach (int cell in cells)
            {
                var p = Point(cell);
                float d = (x - p.x) * (x - p.x) + (z - p.z) * (z - p.z);
                if (d < distance) { distance = d; best = cell; }
            }
            return best;
        }
        public List<OfficeLayout.Point> Path(float x, float z, float targetX, float targetZ)
        {
            int start = Nearest(x, z), goal = Nearest(targetX, targetZ);
            var parent = new int[reachable.Length];
            for (int i = 0; i < parent.Length; i++) parent[i] = -1;
            var queue = new Queue<int>(); queue.Enqueue(start); parent[start] = start;
            while (queue.Count > 0 && parent[goal] < 0)
            {
                int cell = queue.Dequeue();
                for (int d = 0; d < 4; d++)
                {
                    int next = Neighbor(cell, d);
                    if (next >= 0 && reachable[next] && parent[next] < 0)
                    { parent[next] = cell; queue.Enqueue(next); }
                }
            }
            var path = new List<OfficeLayout.Point>();
            for (int cell = goal; ; cell = parent[cell])
            {
                path.Add(Point(cell));
                if (cell == start) break;
            }
            path.Reverse();
            return path;
        }
    }
}
