using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace OfficeRampage
{
    public sealed partial class OfficeRampageGame
    {
        private int robotDeliveryStage;
        public int RobotDeliveryStage { get { return robotDeliveryStage; } }
        private bool bossEjected;
        private IEnumerator DeliverRobotFromOrbit(Transform robot)
        {
            robotDeliveryStage=1;float oldFar=view.farClipPlane;bool fog=RenderSettings.fog;Color sky=view.backgroundColor;
            Vector3 impact=bossUnit.transform.position;impact.y=0;
            var arena=world.Find("Desert arena");arena.gameObject.SetActive(false);
            RenderSettings.fog=false;view.backgroundColor=new Color(.002f,.004f,.015f);view.farClipPlane=2500;
            Transform[] satellites;var space=BuildOrbitScene(impact,out satellites);
            foreach(var satellite in satellites) satellite.gameObject.SetActive(false);
            var carrier=new GameObject("Orbital robot carrier").transform;carrier.SetParent(space,false);
            Vector3 launch=impact+new Vector3(-100,145,-110);carrier.position=launch+new Vector3(0,55,-15);
            Shape(carrier,"Carrier spine",Vector3.zero,new Vector3(65,12,35),new Color(.25f,.29f,.34f),false);
            for(int side=-1;side<=1;side+=2)
            {
                Shape(carrier,"Docking arm",new Vector3(side*22,-15,8),new Vector3(7,30,10),Color.gray,false);
                Shape(carrier,"Carrier solar panel",new Vector3(side*65,0,0),new Vector3(60,2,38),new Color(.04f,.2f,.46f),false);
            }
            robot.SetParent(space,true);robot.position=launch;robot.localScale=Vector3.one*5;
            view.fieldOfView=58;view.transform.position=launch+new Vector3(-145,95,-155);view.transform.LookAt(launch+new Vector3(45,-65,50));
            yield return SceneWait(1.4f);
            robotDeliveryStage=2;audioBus.Play("rocket",.7f);
            Vector3 cameraStart=view.transform.position;Quaternion cameraRotation=view.transform.rotation;
            float t=0;while(t<2.4f)
            {
                if(RunActive)
                {
                    t+=StoryCinematicDelta;float k=Mathf.Clamp01(t/2.4f);
                    robot.position=Vector3.Lerp(launch,impact+Vector3.up*35,k*k);robot.localScale=Vector3.one*Mathf.Lerp(5,1,k);
                    view.transform.position=Vector3.Lerp(cameraStart,impact+new Vector3(-20,35,-45),Mathf.SmoothStep(0,1,k));
                    view.transform.rotation=Quaternion.Slerp(cameraRotation,Quaternion.LookRotation(robot.position-view.transform.position),Mathf.SmoothStep(0,1,k));
                }
                yield return null;
            }
            robot.SetParent(bossUnit.transform,false);robot.localScale=Vector3.one;robot.localPosition=Vector3.up*35;
            robot.localRotation=Quaternion.identity;
            space.gameObject.SetActive(false);Destroy(space.gameObject);arena.gameObject.SetActive(true);
            RenderSettings.fog=fog;view.backgroundColor=sky;view.farClipPlane=oldFar;robotDeliveryStage=3;
        }
        private IEnumerator ShatterRobotAndEject(OfficeDefender unit)
        {
            var robot=unit.visual;var pilot=robot.Find("CEO pilot");
            if(pilot==null) yield break;
            var animation=robot.GetComponent<OfficeBossRobot>();if(animation!=null) animation.enabled=false;
            if(unit.flames!=null) unit.flames.gameObject.SetActive(false);
            if(unit.label!=null) unit.label.gameObject.SetActive(false);
            pilot.SetParent(world,true);robot.SetParent(world,true);
            var upper=new GameObject("Robot upper wreck").transform;upper.SetParent(robot,false);upper.localPosition=Vector3.up*3.44f;
            var lower=new GameObject("Robot lower wreck").transform;lower.SetParent(robot,false);
            var parts=new List<Transform>();foreach(Transform child in robot) if(child!=upper && child!=lower) parts.Add(child);
            foreach(var part in parts) part.SetParent(part.localPosition.y>3.44f?upper:lower,true);
            Vector3 upperStart=upper.localPosition,pilotStart=pilot.position;Quaternion pilotRotation=pilot.rotation;
            Vector3 landing=new Vector3(Mathf.Clamp(unit.transform.position.x+(unit.transform.position.x>0?-16:16),-current.width/2+2,current.width/2-2),.32f,Mathf.Clamp(unit.transform.position.z-4,current.start+2,current.end-2));
            view.transform.position=robot.position+new Vector3(-14,30,-20);view.transform.LookAt(robot.position+Vector3.up*2);
            audioBus.Play("metal2",.9f);float t=0;
            while(t<1.1f)
            {
                if(RunActive)
                {
                    t+=StoryCinematicDelta;float k=Mathf.Clamp01(t/1.1f);
                    upper.localPosition=upperStart+new Vector3(-4*k,Mathf.Sin(k*Mathf.PI)*3,-k*2);
                    upper.localRotation=Quaternion.Euler(0,0,65*k);lower.localRotation=Quaternion.Euler(0,0,-20*k);
                }
                yield return null;
            }
            t=0;while(t<1.8f)
            {
                if(RunActive)
                {
                    t+=StoryCinematicDelta;float k=Mathf.Clamp01(t/1.8f);
                    pilot.position=Vector3.Lerp(pilotStart,landing,k)+Vector3.up*(4*k*(1-k)*18);
                    pilot.rotation=Quaternion.Slerp(pilotRotation,Quaternion.Euler(90,0,0),k);pilot.localScale=Vector3.one*Mathf.Lerp(.7f,1,k);
                    view.transform.position=pilot.position+new Vector3(-7,6,-10);view.transform.LookAt(pilot.position);
                }
                yield return null;
            }
            if(unit.Alive) unit.Teleport(new Vector3(landing.x,.1f,landing.z));else unit.transform.position=new Vector3(landing.x,.1f,landing.z);
            pilot.SetParent(unit.transform,true);pilot.position=landing;pilot.rotation=Quaternion.Euler(90,0,0);pilot.localScale=Vector3.one;
            unit.visual=pilot;bossEjected=true;audioBus.Play("thud",.8f);
        }
    }
}
