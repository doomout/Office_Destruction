using UnityEngine;
namespace OfficeRampage
{
    public sealed partial class OfficeRampageGame
    {
        private static readonly string[] EndlessThemeNames={"도시","사막","섬","공원","우주 정거장","화산 지대","지하 동굴","설원","항구","공장 지대"};
        private static readonly Color[] ThemeFloors={new Color(.12f,.19f,.23f),new Color(.65f,.48f,.29f),new Color(.66f,.63f,.49f),new Color(.27f,.43f,.23f),new Color(.20f,.24f,.34f),new Color(.24f,.18f,.17f),new Color(.26f,.30f,.34f),new Color(.78f,.85f,.89f),new Color(.32f,.36f,.39f),new Color(.32f,.29f,.24f)};
        private static readonly Color[] ThemeSkies={new Color(.18f,.27f,.38f),new Color(.83f,.66f,.43f),new Color(.34f,.74f,.91f),new Color(.47f,.73f,.84f),new Color(.005f,.008f,.035f),new Color(.30f,.10f,.07f),new Color(.045f,.065f,.10f),new Color(.64f,.76f,.87f),new Color(.34f,.55f,.65f),new Color(.46f,.29f,.20f)};
        private void BuildExtraEndlessTheme()
        {
            switch(endlessEnvironment)
            {
                case 0:BuildEndlessCity();break;
                case 3:BuildEndlessPark();break;
                case 4:BuildEndlessSpace();break;
                case 5:BuildEndlessVolcano();break;
                case 6:BuildEndlessCave();break;
                case 7:BuildEndlessSnow();break;
                case 8:BuildEndlessHarbor();break;
                case 9:BuildEndlessFactory();break;
            }
        }
        private void ThemeRail(Color color)
        {
            for(int side=-1;side<=1;side+=2)
            {
                Scenery("Boundary rail",new Vector3(side*31.65f,.45f,32),new Vector3(.25f,.9f,64),color);
                Scenery("Boundary rail",new Vector3(0,.45f,side<0?.35f:63.65f),new Vector3(64,.9f,.25f),color);
            }
        }
        private void GroundStripes(Color color)
        {for(int i=0;i<8;i++)Scenery("Ground marking",new Vector3(-26+i*7.5f,.012f,32),new Vector3(.12f,.015f,60),color);}
        private void ThemeTree(Vector3 at,bool snow=false)
        {
            Scenery("Tree trunk",at+Vector3.up*2,new Vector3(.8f,4,.8f),new Color(.32f,.22f,.16f),PrimitiveType.Cylinder);
            Scenery(snow?"Snowy tree crown":"Tree crown",at+Vector3.up*5,new Vector3(5,6,5),snow?new Color(.82f,.90f,.92f):new Color(.14f,.38f,.18f),PrimitiveType.Sphere);
        }
        private void BuildEndlessCity()
        {
            Scenery("City avenue",new Vector3(0,-.5f,32),new Vector3(160,.4f,160),new Color(.08f,.11f,.14f));
            for(int side=-1;side<=1;side+=2)for(int i=0;i<5;i++)
            {
                float x=side*48,z=-8+i*20,h=16+(i%3)*9;
                Scenery("Office tower",new Vector3(x,h/2,z),new Vector3(12,h,13),new Color(.19f,.25f,.34f));
                for(int row=0;row<4;row++)Scenery("Lit tower windows",new Vector3(x-side*6.1f,3+row*4,z),new Vector3(.05f,1.6f,9),new Color(.65f,.85f,.93f));
                Scenery("Streetlamp",new Vector3(side*35,3,z),new Vector3(.2f,6,.2f),Navy);
                Scenery("Streetlamp head",new Vector3(side*35,6,z),new Vector3(1.4f,.2f,.6f),Color.yellow);
            }
            ThemeRail(new Color(.25f,.32f,.38f));GroundStripes(new Color(.32f,.48f,.55f));
        }
        private void BuildEndlessPark()
        {
            Scenery("Park lawn",new Vector3(0,-.4f,32),new Vector3(210,.5f,210),new Color(.20f,.40f,.18f));
            Scenery("Park path",new Vector3(0,.012f,32),new Vector3(5,.02f,64),new Color(.59f,.54f,.42f));
            for(int side=-1;side<=1;side+=2)for(int i=0;i<7;i++)
            {
                Vector3 at=new Vector3(side*(37+i%2*10),0,-10+i*14);ThemeTree(at);
                Scenery("Flower bed",at+new Vector3(-side*2,.25f,3),new Vector3(3,.5f,2),i%2==0?new Color(.85f,.31f,.48f):new Color(.94f,.73f,.22f),PrimitiveType.Sphere);
            }
            Scenery("Fountain basin",new Vector3(0,.4f,80),new Vector3(15,.5f,15),new Color(.57f,.61f,.60f),PrimitiveType.Cylinder);
            Scenery("Fountain water",new Vector3(0,1,80),new Vector3(13,.1f,13),Cyan,PrimitiveType.Cylinder);
            Scenery("Fountain jet",new Vector3(0,4,80),new Vector3(.7f,6,.7f),new Color(.64f,.91f,.96f),PrimitiveType.Capsule);
            BoundaryStones(new Color(.45f,.49f,.36f));
        }
        private void BuildEndlessSpace()
        {
            Scenery("Orbital platform",new Vector3(0,-1,32),new Vector3(70,1.5f,70),new Color(.14f,.17f,.23f));
            Scenery("Blue planet",new Vector3(50,12,140),Vector3.one*60,new Color(.10f,.36f,.72f),PrimitiveType.Sphere);
            Scenery("Distant moon",new Vector3(-85,10,105),Vector3.one*18,new Color(.7f,.72f,.77f),PrimitiveType.Sphere);
            for(int i=0;i<44;i++)
            {float angle=i*2.399963f;Scenery("Star",new Vector3(Mathf.Cos(angle)*145,-32+i%7*14,32+Mathf.Sin(angle)*145),Vector3.one*(i%3==0?.8f:.4f),Color.white,PrimitiveType.Sphere);}
            for(int side=-1;side<=1;side+=2)
            {
                Scenery("Station arm",new Vector3(side*50,-1,32),new Vector3(35,1,2),new Color(.4f,.45f,.52f));
                for(int i=0;i<4;i++)Scenery("Solar array",new Vector3(side*50,0,10+i*15),new Vector3(14,.3f,11),new Color(.08f,.28f,.58f));
            }
            ThemeRail(Cyan);GroundStripes(new Color(.35f,.51f,.64f));
        }
        private void BuildEndlessVolcano()
        {
            Color lava=new Color(1,.20f,.025f),rock=new Color(.17f,.13f,.14f);
            Scenery("Lava sea",new Vector3(0,-.8f,32),new Vector3(220,.4f,220),lava);
            Scenery("Basalt island",new Vector3(0,-.3f,32),new Vector3(78,.5f,78),rock);
            Scenery("Volcanic mountain",new Vector3(0,8,108),new Vector3(70,46,62),rock,PrimitiveType.Sphere);
            Scenery("Volcano crater",new Vector3(0,29,108),new Vector3(18,.8f,18),Color.black,PrimitiveType.Cylinder);
            Scenery("Crater magma",new Vector3(0,30,108),new Vector3(13,.3f,13),lava,PrimitiveType.Cylinder);
            for(int side=-1;side<=1;side+=2)
            {
                Vector3[] points={new Vector3(side*3,29,94),new Vector3(side*6,17,79),new Vector3(side*9,3,76)};
                for(int i=0;i<2;i++){Vector3 delta=points[i+1]-points[i];var stream=Scenery("Lava stream",(points[i]+points[i+1])*.5f,new Vector3(1.2f,.2f,delta.magnitude),lava);stream.transform.rotation=Quaternion.LookRotation(delta);}
            }
            for(int i=0;i<5;i++)Scenery("Volcanic plume",new Vector3(i*2,35+i*7,108),Vector3.one*(9+i*3),new Color(.23f,.19f,.20f),PrimitiveType.Sphere);
            for(int side=-1;side<=1;side+=2)for(int i=0;i<5;i++)
            {Scenery("Basalt spire",new Vector3(side*(39+i%2*4),3+i%3,-5+i*18),new Vector3(5,9+i%3*4,6),rock,PrimitiveType.Cylinder);}
            BoundaryStones(new Color(.42f,.23f,.18f));
            for(int i=0;i<7;i++)Scenery("Glowing ground fissure",new Vector3(-25+i*8,.015f,7+i%3*23),new Vector3(4,.02f,.16f),lava).transform.localRotation=Quaternion.Euler(0,i*19,0);
        }
        private void BuildEndlessCave()
        {
            Color stone=new Color(.16f,.21f,.27f),crystal=new Color(.25f,.68f,.85f);
            Scenery("Cavern ground",new Vector3(0,-.4f,32),new Vector3(140,.4f,170),stone);
            Scenery("Cavern ceiling",new Vector3(0,35,32),new Vector3(150,5,170),stone);
            for(int side=-1;side<=1;side+=2)for(int i=0;i<7;i++)
            {
                var at=new Vector3(side*(44+i%2*5),0,-14+i*15);
                Scenery("Cavern wall",at+Vector3.up*12,new Vector3(15,32,19),stone,PrimitiveType.Sphere);
                Scenery("Crystal pillar",new Vector3(side*34,3,at.z+2),new Vector3(1.4f,6+i%3,1.4f),crystal).transform.localRotation=Quaternion.Euler(0,25,side*12);
                Scenery("Stalactite",new Vector3(side*22,29,-10+i*16),new Vector3(3,9,3),stone,PrimitiveType.Capsule);
            }
            Scenery("Cave back wall",new Vector3(0,12,96),new Vector3(95,40,24),stone,PrimitiveType.Sphere);
            Scenery("Cave entrance wall",new Vector3(0,12,-32),new Vector3(95,40,24),stone,PrimitiveType.Sphere);
            BoundaryStones(crystal);
        }
        private void BuildEndlessSnow()
        {
            var snow=new Color(.80f,.87f,.91f);
            Scenery("Snow field",new Vector3(0,-.5f,32),new Vector3(300,.5f,300),snow);
            for(int side=-1;side<=1;side+=2)for(int i=0;i<6;i++)
            {ThemeTree(new Vector3(side*(38+i%2*8),0,-8+i*16),true);Scenery("Glacial mountain",new Vector3(side*85,8,-30+i*29),new Vector3(33,32+i%3*12,38),new Color(.57f,.69f,.78f),PrimitiveType.Sphere);}
            Scenery("Frozen lake",new Vector3(0,-.1f,98),new Vector3(60,.1f,35),new Color(.38f,.66f,.80f),PrimitiveType.Cylinder);
            BoundaryStones(new Color(.44f,.60f,.70f));
        }
        private void BuildEndlessHarbor()
        {
            Scenery("Harbor water",new Vector3(0,-1,32),new Vector3(300,.4f,300),new Color(.06f,.31f,.43f));
            Scenery("Concrete wharf",new Vector3(0,-.6f,32),new Vector3(76,1,78),new Color(.35f,.39f,.40f));
            for(int side=-1;side<=1;side+=2)
            {
                Scenery("Cargo ship hull",new Vector3(side*63,1,32),new Vector3(18,6,65),new Color(.20f,.26f,.35f),PrimitiveType.Capsule);
                for(int i=0;i<6;i++)Scenery("Shipping container",new Vector3(side*63,6,5+i*10),new Vector3(12,5,8),i%2==0?new Color(.65f,.22f,.12f):new Color(.13f,.40f,.52f));
                Scenery("Dock crane tower",new Vector3(side*43,14,65),new Vector3(2,28,2),Orange);
                Scenery("Dock crane boom",new Vector3(side*51,27,65),new Vector3(25,2,2),Orange);
                Scenery("Dock crane cable",new Vector3(side*62,18,65),new Vector3(.13f,18,.13f),Navy);
            }
            ThemeRail(new Color(.83f,.63f,.18f));GroundStripes(new Color(.72f,.72f,.56f));
        }
        private void BuildEndlessFactory()
        {
            var steel=new Color(.23f,.26f,.28f);
            Scenery("Industrial yard",new Vector3(0,-.5f,32),new Vector3(210,.5f,210),new Color(.25f,.23f,.21f));
            for(int side=-1;side<=1;side+=2)for(int i=0;i<4;i++)
            {
                Scenery("Factory hall",new Vector3(side*52,7,-8+i*26),new Vector3(24,14,22),steel);
                Scenery("Smokestack",new Vector3(side*52,23,-8+i*26),new Vector3(3,24,3),new Color(.39f,.25f,.20f),PrimitiveType.Cylinder);
                Scenery("Factory chimney smoke",new Vector3(side*52+3,40,-8+i*26),new Vector3(9,12,9),new Color(.36f,.33f,.31f),PrimitiveType.Sphere);
                Scenery("Factory door",new Vector3(side*39.9f,4,-8+i*26),new Vector3(.1f,8,10),Orange);
            }
            ThemeRail(Orange);GroundStripes(new Color(.67f,.51f,.18f));
        }
    }
}
