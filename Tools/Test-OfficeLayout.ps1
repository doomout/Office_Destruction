$ErrorActionPreference = 'Stop'
$source = Get-Content (Join-Path $PSScriptRoot '../Assets/OfficeRampage/OfficeLayout.cs') -Raw
$test = @'
namespace OfficeRampage
{
    public static class LayoutChecks
    {
        public static string Run()
        {
            int tested = 0, minProps = int.MaxValue, maxProps = 0;
            for (int style = 0; style < 5; style++)
            for (int seed = 0; seed < 200; seed++)
            {
                OfficeLayout p = OfficeLayout.Create(style, seed);
                if (p.furniture.Count == 0) throw new System.Exception("Empty office");
                minProps = System.Math.Min(minProps, p.furniture.Count);
                maxProps = System.Math.Max(maxProps, p.furniture.Count);
                var obstacles = new System.Collections.Generic.List<OfficeLayout.Area>(p.walls);
                foreach (var prop in p.furniture)
                {
                    var area = new OfficeLayout.Area(prop.x, prop.z, 2.9f, 1.9f);
                    foreach (var wall in p.walls)
                        if (area.Overlaps(wall, .45f)) throw new System.Exception("Furniture touches partition");
                    foreach (var route in p.route)
                        if (area.Overlaps(route)) throw new System.Exception("Reserved route blocked");
                    if (System.Math.Abs(prop.x) + 1.45f > p.width / 2 || prop.z - .95f < 0 || prop.z + .95f > p.length)
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
            return "PASS: " + tested + " layouts; all exits and furniture reachable with player clearance; furniture count " + minProps + "-" + maxProps;
        }
    }
}
'@
Add-Type -TypeDefinition ($source + [Environment]::NewLine + $test)
[OfficeRampage.LayoutChecks]::Run()
