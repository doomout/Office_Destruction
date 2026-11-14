using UnityEngine;
namespace OfficeRampage
{
    public sealed partial class OfficeRampageGame
    {
        private void BuildOfficeShell(Room room,Transform parent)
        {
            var shell=new GameObject("Office architecture").transform; shell.SetParent(parent,false);
            float middle=(room.start+room.end)/2,length=room.end-room.start,half=room.width/2;
            Color wall=new Color(.64f,.69f,.72f),frame=new Color(.18f,.24f,.28f);
            Shape(shell,"Ceiling",new Vector3(0,11.6f,middle),new Vector3(room.width,.22f,length),new Color(.73f,.76f,.77f));
            for(float z=room.start+4;z<room.end-2;z+=6)
            {
                Shape(shell,"Ceiling beam",new Vector3(0,11.42f,z),new Vector3(room.width,.15f,.12f),frame,false);
                for(int side=-1;side<=1;side+=2)
                    Shape(shell,"Ceiling light panel",new Vector3(side*room.width*.23f,11.41f,z),new Vector3(3,.08f,1),new Color(.9f,.97f,1),false);
            }
            for(int side=-1;side<=1;side+=2)
            {
                float x=side*half;
                Shape(shell,"Wall below windows",new Vector3(x,.65f,middle),new Vector3(.22f,1.3f,length),wall,false);
                Shape(shell,"Wall above windows",new Vector3(x,9.95f,middle),new Vector3(.22f,3.3f,length),wall,false);
                for(float z=room.start;z<=room.end;z+=5)
                {
                    Shape(shell,"Window mullion",new Vector3(x,4.8f,z),new Vector3(.28f,6.8f,.16f),frame,false);
                    if(z+2.5f<room.end) Shape(shell,"Window crossbar",new Vector3(x,2.6f,z+2.5f),new Vector3(.25f,.08f,5),frame,false);
                }
                foreach(float y in new[]{1.35f,8.25f}) Shape(shell,"Window sill",new Vector3(x,y,middle),new Vector3(.36f,.12f,length),frame,false);
                // Skyline is outside the existing solid boundary: the view opens up, not the playable area.
                for(int i=0;i<8;i++)
                {
                    float z=room.start-8+i*7, height=8+(i*7+room.number*3)%15;
                    var building=Shape(parent,"City building",new Vector3(side*(half+14+(i%3)*5),height/2-6,z),new Vector3(5,height,5),new Color(.2f+i*.014f,.29f+i*.012f,.36f+i*.01f),false);
                    for(float y=-3;y<height-7;y+=2.4f)
                        Shape(building.transform,"City window",new Vector3(-side*.505f,(y-(height/2-6))/height,0),new Vector3(.01f,.07f,.65f),new Color(.67f,.84f,.9f),false);
                }
                float segment=(room.width-3.4f)/2;
                Shape(shell,"Exit surround",new Vector3(side*(1.7f+segment/2),5.8f,room.end),new Vector3(segment,11.6f,.18f),wall,false);
            }
            Shape(shell,"Entry surround",new Vector3(0,5.8f,room.start),new Vector3(room.width,11.6f,.18f),wall,false);
            var cutaway=shell.gameObject.AddComponent<OfficeRoofCutaway>();cutaway.height=11.3f;
            cutaway.renderers=shell.GetComponentsInChildren<Renderer>();
        }
    }
}
