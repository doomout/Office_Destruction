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
            public float x, z, yaw;
            public int kind;
            public Point(float x, float z, int kind = 0) { this.x = x; this.z = z; this.kind = kind; this.yaw=0; }
            public Area Footprint { get { return kind <= 1 ? new Area(x, z, 2.9f, 1.9f) : new Area(x, z, 1.7f, 1.5f); } }
        }

        public float width, length;
        public string name;
        public readonly List<Area> walls = new List<Area>();
        public readonly List<Area> route = new List<Area>();
        public readonly List<Point> furniture = new List<Point>();
        public readonly List<Point> oil = new List<Point>();

        public static OfficeLayout CreateExecutive()
        {
            var layout = new OfficeLayout { width = 26, length = 30, name = "대표 사무실" };
            for (int i = 0; i < 6; i++)
            {
                layout.furniture.Add(new Point(-9, 5 + i * 3.7f, i));
                layout.furniture.Add(new Point(9, 5 + i * 3.7f, 5 - i));
            }
            return layout;
        }

        public static OfficeLayout Create(int style, int seed)
        {
            var rng = new Random(seed);
            string[] names = { "넓은 사무실", "긴 사무실", "분산형 사무실", "대형 사무실", "개방형 사무실" };
            var plan = new OfficeLayout { width = 32 + rng.Next(9), length = 34 + rng.Next(11), name = names[style % names.Length] };
            plan.AddRoute(0, 0, 0, plan.length);
            float cross = plan.length * (.35f + (float)rng.NextDouble() * .3f);
            plan.AddRoute(-plan.width/2,cross,plan.width/2,cross);
            int target = rng.Next(36,51);
            // Random positions, not a fixed grid. Reserve connected aisles and reject overlaps.
            for(int attempt=0; attempt<6000 && plan.furniture.Count<target; attempt++)
            {
                int kind=plan.furniture.Count<6 ? plan.furniture.Count : rng.Next(6);
                float x=(float)(rng.NextDouble()*(plan.width-4)-plan.width/2+2);
                float z=3+(float)rng.NextDouble()*(plan.length-6);
                var point=new Point(x,z,kind); bool blocked=false;
                foreach(var aisle in plan.route) if(aisle.Overlaps(point.Footprint,.25f)) { blocked=true; break; }
                if(blocked) continue;
                foreach(var other in plan.furniture) if(other.Footprint.Overlaps(point.Footprint,1.4f)) { blocked=true; break; }
                if(!blocked) plan.furniture.Add(point);
            }
            for(float z=7+(float)rng.NextDouble()*3; z<plan.length-4; z+=7+(float)rng.NextDouble()*4)
                plan.oil.Add(new Point(0,z));
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
