using System;
using System.Collections.Generic;
using UnityEngine;
namespace OfficeRampage
{
    [Serializable] public sealed class OfficeFixedFloor
    {
        [Serializable] public struct Spot { public float x,z,yaw,footprintX,footprintZ,footprintWidth,footprintDepth; public int kind; }
        [Serializable] public struct Aisle { public float x,z,width,depth; }
        public float width,length;
        public string name;
        public Spot[] furniture,oil,enemies,potions;
        public Aisle[] routes, walls;
        private static readonly Dictionary<int,OfficeFixedFloor> cache=new Dictionary<int,OfficeFixedFloor>();
        public static OfficeFixedFloor Load(int floor)
        {
            var prefab=Resources.Load<OfficeFloorPrefab>("OfficeFloors/Floor"+floor);
            if(prefab!=null) return prefab.ReadLayout();
            if(cache.TryGetValue(floor,out var data)) return data;
            var asset=Resources.Load<TextAsset>("OfficeLayouts/floor"+floor);
            if(asset==null) throw new InvalidOperationException("고정 사무실 데이터 누락: "+floor);
            data=JsonUtility.FromJson<OfficeFixedFloor>(asset.text); cache[floor]=data; return data;
        }
        public OfficeLayout CreateLayout()
        {
            var layout=new OfficeLayout {width=width,length=length,name=name};
            foreach(var p in furniture) layout.furniture.Add(new OfficeLayout.Point(p.x,p.z,p.kind){yaw=p.yaw,footprintX=p.footprintX,footprintZ=p.footprintZ,footprintWidth=p.footprintWidth,footprintDepth=p.footprintDepth});
            foreach(var p in oil) layout.oil.Add(new OfficeLayout.Point(p.x,p.z));
            foreach(var p in routes) layout.route.Add(new OfficeLayout.Area(p.x,p.z,p.width,p.depth));
            if(walls!=null) foreach(var p in walls) layout.walls.Add(new OfficeLayout.Area(p.x,p.z,p.width,p.depth));
            return layout;
        }
        public List<OfficeLayout.Point> EnemyPoints() { return Points(enemies); }
        public List<OfficeLayout.Point> PotionPoints() { return Points(potions); }
        private static List<OfficeLayout.Point> Points(Spot[] source)
        { var result=new List<OfficeLayout.Point>(); foreach(var p in source) result.Add(new OfficeLayout.Point(p.x,p.z)); return result; }
    }
}
