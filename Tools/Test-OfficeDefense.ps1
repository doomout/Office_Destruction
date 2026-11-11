$ErrorActionPreference = 'Stop'
$source = "using System;`nusing System.Collections.Generic;`n"
foreach ($file in @('OfficeLayout.cs', 'OfficeDefenseRules.cs', 'OfficeNavigation.cs')) {
    $text = Get-Content (Join-Path $PSScriptRoot "../Assets/OfficeRampage/$file") -Raw -Encoding UTF8
    $source += [Environment]::NewLine + ($text -replace '(?m)^using [^;]+;\s*', '')
}
$test = @'
namespace OfficeRampage
{
    public static class DefenseChecks
    {
        private static void Require(bool condition, string message)
        { if (!condition) throw new Exception(message); }
        private static void CheckPoint(OfficeLayout p, OfficeLayout.Point point)
        {
            var body = new OfficeLayout.Area(point.x, point.z, .92f, .92f);
            Require(Math.Abs(point.x) + .46f < p.width / 2 && point.z > .46f && point.z + .46f < p.length, "Body outside floor");
            foreach (var wall in p.walls) Require(!body.Overlaps(wall), "NPC path crosses wall");
            foreach (var prop in p.furniture) Require(!body.Overlaps(prop.Footprint), "NPC path crosses prop");
        }
        public static string Run()
        {
            var rng = new Random(42);
            Require(OfficeDefenseRules.Count(1) == 10 && OfficeDefenseRules.Count(2) == 22 &&
                OfficeDefenseRules.Count(3) == 5 && OfficeDefenseRules.Count(4) == 3 && OfficeDefenseRules.Count(5) == 5, "Wrong roster sizes");
            Require(OfficeDefenseRules.Health(1, 0, rng) == 60 && OfficeDefenseRules.BossHealth == 500, "Guard/CEO health");
            for (int i = 0; i < 22; i++) Require(OfficeDefenseRules.Health(2, i, rng) == (i < 10 ? 20 : i < 17 ? 40 : 60), "Junior rank health");
            for (int i = 0; i < 5; i++) Require(OfficeDefenseRules.Health(3, i, rng) == (i < 3 ? 100 : 120), "Manager health");
            for (int i = 0; i < 3; i++) Require(OfficeDefenseRules.Health(4, i, rng) == 140 + i * 20, "Executive health");
            bool low = false, high = false;
            for (int i = 0; i < 10000; i++)
            {
                int hp = OfficeDefenseRules.Health(5, i % 5, rng);
                Require(hp >= 100 && hp <= 200, "Dog HP outside range");
                low |= hp == 100; high |= hp == 200;
            }
            Require(low && high, "Dog range must include 100 and 200");
            for (int dogs = 1; dogs <= 5; dogs++) Require(!OfficeDefenseRules.CanWakeBoss(5, dogs, false), "Boss wakes too soon");
            Require(OfficeDefenseRules.CanWakeBoss(5, 0, false), "Boss does not wake after last dog");
            Require(!OfficeDefenseRules.CanWakeBoss(5, 0, true) && !OfficeDefenseRules.CanWakeBoss(4, 0, false), "Boss wakes twice or outside room 5");
            int layouts = 0, paths = 0;
            for (int style = 0; style < 6; style++)
            for (int seed = 0; seed < 100; seed++)
            {
                var p = style == 5 ? OfficeLayout.CreateExecutive() : OfficeLayout.Create(style, seed);
                var nav = new OfficeNavigation(p);
                var spawns = nav.SpawnPoints(style == 5 ? 5 : 22, rng, style == 5);
                foreach (var start in spawns)
                {
                    CheckPoint(p, start);
                    if (style == 5) Require(start.x * start.x + (start.z - (p.length - 5)) * (start.z - (p.length - 5)) >= 9, "Dog spawns inside seated CEO");
                    Require(start.z >= 5 && start.z <= p.length - 4, "Spawn too close to a doorway");
                    var target = nav.RandomPoint(rng);
                    var path = nav.Path(start.x, start.z, target.x, target.z);
                    Require(path.Count > 0, "Empty path");
                    for (int i = 0; i < path.Count; i++)
                    {
                        CheckPoint(p, path[i]);
                        if (i > 0)
                        {
                            float dx = path[i].x - path[i - 1].x, dz = path[i].z - path[i - 1].z;
                            Require(dx * dx + dz * dz <= .161f, "Path skips a wall");
                        }
                    }
                    paths++;
                }
                for (int a = 0; a < spawns.Count; a++)
                    for (int b = a + 1; b < spawns.Count; b++)
                    {
                        float dx = spawns[a].x - spawns[b].x, dz = spawns[a].z - spawns[b].z;
                        Require(dx * dx + dz * dz >= 4, "NPC spawn overlap");
                    }
                layouts++;
            }
            return "PASS: roster HP/counts; 10,000 dog HP samples; boss wake gate; " + layouts + " floor/spawn cases; " + paths + " collision-free NPC paths";
        }
    }
}
'@
Add-Type -TypeDefinition ($source + [Environment]::NewLine + $test)
[OfficeRampage.DefenseChecks]::Run()
