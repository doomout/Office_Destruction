$ErrorActionPreference = 'Stop'
$source = "using System;`nusing System.Collections.Generic;`n"
foreach ($name in @('OfficeLayout.cs','OfficeNavigation.cs','OfficeDefenseRules.cs','OfficeCombatRules.cs')) {
    $source += (Get-Content "$PSScriptRoot/../Assets/OfficeRampage/$name" -Raw -Encoding UTF8) -replace '(?m)^using [^;]+;\s*', ''
}
$source += @'
namespace OfficeRampage {
public static class ExpansionChecks {
static void Check(bool ok, string msg) { if (!ok) throw new Exception(msg); }
public static string Run() {
var counts = new Dictionary<string,int>();
for(int room=1; room<=4; room++) for(int i=0;i<OfficeDefenseRules.Count(room);i++) {
string name=OfficeDefenseRules.Name(room,i); if(!counts.ContainsKey(name)) counts[name]=0; counts[name]++;
}
string[] names={"경비","사원","주임","대리","차장","부장","이사","상무","전무"};
int[] expected={10,10,7,5,3,2,1,1,1};
for(int i=0;i<names.Length;i++) Check(counts[names[i]]==expected[i], "Roster: "+names[i]);
Check(counts.Count==9,"Unexpected rank");
int layouts=0,spawns=0;
for(int style=0;style<5;style++) for(int seed=0;seed<30;seed++) {
var plan=OfficeLayout.Create(style,seed); Check(plan.walls.Count==0,"Interior walls remain");
var nav=new OfficeNavigation(plan); var points=nav.SpawnPoints(22,new Random(seed));
Check(points.Count==22,"Missing NPCs");
for(int i=0;i<points.Count;i++) {
var p=points[i]; var footprint=new OfficeLayout.Area(p.x,p.z,.92f,.92f);
foreach(var prop in plan.furniture) Check(!footprint.Overlaps(prop.Footprint),"Spawn in prop");
for(int j=0;j<i;j++) Check(Math.Pow(p.x-points[j].x,2)+Math.Pow(p.z-points[j].z,2)>=4,"Overlapping NPCs");
} layouts++;spawns+=points.Count;
}
Check(!OfficeCombatRules.Rage(251,500)&&OfficeCombatRules.Rage(250,500),"Rage threshold");
Check(!OfficeCombatRules.Gift(101,500)&&OfficeCombatRules.Gift(100,500),"Gift threshold");
Check(OfficeCombatRules.DamageResult(500,9999,500,false)==100,"Phase one can be skipped");
Check(OfficeCombatRules.DamageResult(100,9999,500,true)==0,"Phase two cannot die");
Check(OfficeCombatRules.PhaseTwoHealth(100,500)==700,"Wrong recovery");
Check(OfficeCombatRules.Weapons.Length==8,"Weapon slots");
Check(OfficeCombatRules.Cooldowns[3]<OfficeCombatRules.Cooldowns[2],"Machinegun speed");
Check(OfficeCombatRules.Ranges[6]>OfficeCombatRules.Ranges[5],"Rocket range");
return "PASS: exact rank roster; "+layouts+" open layouts / "+spawns+" safe NPC spawns; boss thresholds, lethal-hit clamp, recovery and weapon rules.";
}}}
'@
Add-Type -TypeDefinition $source -Language CSharp
[OfficeRampage.ExpansionChecks]::Run()
