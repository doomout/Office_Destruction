using System;
using System.Collections.Generic;

namespace OfficeRampage
{
    // Engine-independent layout data: identical geometry is used by the game and validation.
    public sealed class OfficeLayout
    {
        public struct Area
        {
            public float x, z, width, depth;
            public Area(float x, float z, float width, float depth)
            { this.x = x; this.z = z; this.width = width; this.depth = depth; }
            public bool Overlaps(Area other, float padding = 0)
            {
                return Math.Abs(x - other.x) < (width + other.width) / 2 + padding &&
                       Math.Abs(z - other.z) < (depth + other.depth) / 2 + padding;
            }
        }

        public struct Point
        {
            public float x, z;
            public Point(float x, float z) { this.x = x; this.z = z; }
        }

        public float width, length;
        public string name;
        public readonly List<Area> walls = new List<Area>();
        public readonly List<Area> route = new List<Area>();
        public readonly List<Point> furniture = new List<Point>();

        public static OfficeLayout Create(int style, int seed)
        {
            var rng = new Random(seed);
            int[] widths = { 16, 32, 18, 34, 24 };
            int[] lengths = { 21, 25, 40, 37, 30 };
            string[] names = { "COMPACT", "WIDE FLOOR", "LONG WING", "GRAND OFFICE", "PARTITIONS" };
            var plan = new OfficeLayout {
                width = widths[style] + rng.Next(-1, 2),
                length = lengths[style] + rng.Next(-1, 2), name = names[style]
            };
            int side = rng.Next(2) == 0 ? -1 : 1;
            float lastX = 0, lastZ = 3;
            const float gapWidth = 3.6f;
            // Alternate doorways force turns. No random wall can seal a compartment.
            for (float z = 7; z < plan.length - 5; z += 7)
            {
                float gapX = side * (plan.width / 2 - 3);
                float gapLeft = gapX - gapWidth / 2;
                float gapRight = gapX + gapWidth / 2;
                float leftWidth = gapLeft + plan.width / 2;
                float rightWidth = plan.width / 2 - gapRight;
                plan.walls.Add(new Area(-plan.width / 2 + leftWidth / 2, z, leftWidth, .45f));
                plan.walls.Add(new Area(gapRight + rightWidth / 2, z, rightWidth, .45f));
                plan.AddRoute(lastX, lastZ, gapX, lastZ);
                plan.AddRoute(gapX, lastZ, gapX, z + 2);
                lastX = gapX; lastZ = z + 2; side = -side;
            }
            plan.AddRoute(lastX, lastZ, 0, lastZ);
            plan.AddRoute(0, lastZ, 0, plan.length);
            plan.AddRoute(0, 0, 0, 3);
            for (float z = 4.8f; z < plan.length - 3; z += 3.6f)
                for (float x = -plan.width / 2 + 2; x < plan.width / 2 - 1.7f; x += 4.2f)
                {
                    var footprint = new Area(x, z, 2.9f, 1.9f);
                    bool blocked = false;
                    foreach (var wall in plan.walls) if (wall.Overlaps(footprint, .5f)) { blocked = true; break; }
                    if (blocked) continue;
                    foreach (var aisle in plan.route) if (aisle.Overlaps(footprint)) { blocked = true; break; }
                    if (!blocked) plan.furniture.Add(new Point(x, z));
                }
            return plan;
        }

        private void AddRoute(float x1, float z1, float x2, float z2)
        {
            // 2.2m reserved aisle, including corners. Furniture never occupies this footprint.
            route.Add(new Area((x1 + x2) / 2, (z1 + z2) / 2,
                Math.Abs(x2 - x1) + 2.2f, Math.Abs(z2 - z1) + 2.2f));
        }
    }
}
