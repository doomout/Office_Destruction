$ErrorActionPreference = 'Stop'
$source = Get-Content (Join-Path $PSScriptRoot '../Assets/OfficeRampage/OfficeLayout.cs') -Raw
$source += [Environment]::NewLine + ((Get-Content (Join-Path $PSScriptRoot '../Assets/OfficeRampage/OfficeRules.cs') -Raw) -replace '^using System;\s*', '')
$test = @'
namespace OfficeRampage
{
    public static class LayoutChecks
    {
        public static string Run()
        {
            int tested = 0, minProps = int.MaxValue, maxProps = 0;
            if (OfficeRules.HealthAfterBreak(100, OfficeRules.Computer) != 95 ||
                OfficeRules.HealthAfterBreak(50, OfficeRules.Copier) != 45 ||
                OfficeRules.HealthAfterBreak(3, OfficeRules.Computer) != 0 ||
                OfficeRules.HealthAfterBreak(90, OfficeRules.WaterCooler) != 95 ||
                OfficeRules.HealthAfterBreak(98, OfficeRules.WaterCooler) != 100 ||
                OfficeRules.HealthAfterBreak(100, OfficeRules.WaterCooler) != 100)
                throw new System.Exception("Damage/healing bounds failed");
            foreach (int kind in new[] { OfficeRules.Desk, OfficeRules.Cabinet, OfficeRules.Plant })
                if (OfficeRules.HealthAfterBreak(50, kind) != 50) throw new System.Exception("Neutral furniture changes health");
            float health = 100;
            for (int i = 0; i < 3; i++) health = OfficeRules.HealthAfterBreak(health, OfficeRules.Computer);
            if (health != 85) throw new System.Exception("Per-object explosion damage failed");
            if (!OfficeRules.OnOil(0, 0, .08f, 0, 0, .65f, .75f) ||
                OfficeRules.OnOil(.7f, 0, .08f, 0, 0, .65f, .75f) ||
                OfficeRules.OnOil(0, 0, 1, 0, 0, .65f, .75f) ||
                OfficeRules.MovementSpeed(8, true) != 4 || OfficeRules.MovementSpeed(5.6f, true) != 2.8f ||
                OfficeRules.MovementSpeed(8, false) != 8)
                throw new System.Exception("Oil entry/exit/jump or 50 percent movement failed");
            for (int style = 0; style < 5; style++)
            for (int seed = 0; seed < 200; seed++)
            {
                OfficeLayout p = OfficeLayout.Create(style, seed);
                if (p.furniture.Count == 0) throw new System.Exception("Empty office");
                var kinds = new System.Collections.Generic.HashSet<int>();
                foreach (var item in p.furniture) kinds.Add(item.kind);
                if (kinds.Count != 6) throw new System.Exception("Missing furniture type");
                if (p.oil.Count == 0) throw new System.Exception("Missing oil hazard");
                foreach (var oil in p.oil)
                {
                    var patch = new OfficeLayout.Area(oil.x, oil.z, 1.3f, 1.5f);
                    foreach (var wall in p.walls) if (patch.Overlaps(wall)) throw new System.Exception("Oil intersects wall");
                    foreach (var prop in p.furniture) if (patch.Overlaps(prop.Footprint)) throw new System.Exception("Oil under furniture");
                }
                for (int i = 0; i < p.furniture.Count; i++)
                    for (int j = i + 1; j < p.furniture.Count; j++)
                        if (p.furniture[i].Footprint.Overlaps(p.furniture[j].Footprint)) throw new System.Exception("Overlapping furniture");
                minProps = System.Math.Min(minProps, p.furniture.Count);
                maxProps = System.Math.Max(maxProps, p.furniture.Count);
                var obstacles = new System.Collections.Generic.List<OfficeLayout.Area>(p.walls);
                foreach (var prop in p.furniture)
                {
                    var area = prop.Footprint;
                    foreach (var wall in p.walls)
                        if (area.Overlaps(wall, .3f)) throw new System.Exception("Furniture touches partition");
                    foreach (var route in p.route)
                        if (area.Overlaps(route)) throw new System.Exception("Reserved route blocked");
                    if (System.Math.Abs(prop.x) + area.width / 2 > p.width / 2 || prop.z - area.depth / 2 < 0 || prop.z + area.depth / 2 > p.length)
                        throw new System.Exception("Furniture outside room");
                    obstacles.Add(area);
                }
                // Flood fill includes solid furniture and a 0.46m player clearance radius.
                const float step = .4f;
                float left = -p.width / 2 + .6f;
                int nx = (int)((p.width - 1.2f) / step) + 1;
                int nz = (int)((p.length - 1.2f) / step) + 1;
                bool[] blocked = new bool[nx * nz], visited = new bool[nx * nz];
                for (int iz = 0; iz < nz; iz++)
                for (int ix = 0; ix < nx; ix++)
                {
                    var player = new OfficeLayout.Area(left + ix * step, .6f + iz * step, .92f, .92f);
                    foreach (var obstacle in obstacles)
                        if (player.Overlaps(obstacle)) { blocked[iz * nx + ix] = true; break; }
                }
                int sx = (int)System.Math.Round(-left / step);
                int sz = (int)System.Math.Round((3 - .6f) / step);
                int start = sz * nx + sx;
                if (blocked[start]) throw new System.Exception("Spawn blocked");
                var queue = new System.Collections.Generic.Queue<int>();
                queue.Enqueue(start); visited[start] = true;
                int[] dx = { -1, 1, 0, 0 }, dz = { 0, 0, -1, 1 };
                while (queue.Count > 0)
                {
                    int cell = queue.Dequeue(), x = cell % nx, z = cell / nx;
                    for (int d = 0; d < 4; d++)
                    {
                        int xx = x + dx[d], zz = z + dz[d];
                        if (xx < 0 || xx >= nx || zz < 0 || zz >= nz) continue;
                        int next = zz * nx + xx;
                        if (!blocked[next] && !visited[next]) { visited[next] = true; queue.Enqueue(next); }
                    }
                }
                int exit = (nz - 1) * nx + sx;
                if (!visited[exit]) throw new System.Exception("Exit unreachable: style " + style + " seed " + seed);
                foreach (var prop in p.furniture)
                {
                    bool accessible = false;
                    for (int z = 0; z < nz && !accessible; z++)
                    for (int x = 0; x < nx; x++)
                        if (visited[z * nx + x] && System.Math.Abs(left + x * step - prop.x) < 2.4f &&
                            System.Math.Abs(.6f + z * step - prop.z) < 2f) { accessible = true; break; }
                    if (!accessible) throw new System.Exception("Unreachable furniture: style " + style + " seed " + seed + " x " + prop.x + " z " + prop.z);
                }
                tested++;
            }
            return "PASS: health/oil rules; " + tested + " layouts; six types and oil in every office; all exits and furniture reachable; furniture count " + minProps + "-" + maxProps;
        }
    }
}
'@
Add-Type -TypeDefinition ($source + [Environment]::NewLine + $test)
[OfficeRampage.LayoutChecks]::Run()
