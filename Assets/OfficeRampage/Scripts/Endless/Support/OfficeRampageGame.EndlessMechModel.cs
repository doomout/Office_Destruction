using UnityEngine;
namespace OfficeRampage
{
    public sealed partial class OfficeRampageGame
    {
        private void BuildEndlessMechFrame()
        {
            var root=endlessMechRoot;var red=new Color(.55f,.035f,.045f);var steel=new Color(.15f,.21f,.27f);
            RobotPart(root,"Lower torso",new Vector3(0,3.05f,0),new Vector3(2.8f,.7f,1.45f),red);
            RobotPart(root,"Upper torso",new Vector3(0,4.25f,0),new Vector3(2.8f,1.6f,1.45f),red);
            RobotPart(root,"Reactor",new Vector3(0,4.1f,.78f),new Vector3(.7f,.7f,.12f),Cyan);
            RobotPart(root,"Head",new Vector3(0,5.85f,0),new Vector3(1.3f,1.2f,1.15f),steel);
            RobotPart(root,"Visor",new Vector3(0,5.9f,.59f),new Vector3(1.05f,.25f,.1f),Cyan);
            for(int side=-1;side<=1;side+=2)
            {
                var leg=new GameObject(side<0?"Player robot left leg":"Player robot right leg").transform;leg.SetParent(root,false);leg.localPosition=new Vector3(side*.85f,2.5f,0);
                if(side<0)endlessMechLeftLeg=leg;else endlessMechRightLeg=leg;
                RobotPart(leg,"Piston leg",new Vector3(0,-1,0),new Vector3(1.1f,2.2f,1.1f),steel);
                RobotPart(leg,"Foot",new Vector3(0,-2.1f,.3f),new Vector3(1.3f,.65f,1.8f),red);
                var arm=new GameObject(side<0?"Player robot left arm":"Player robot right arm").transform;arm.SetParent(root,false);arm.localPosition=new Vector3(side*1.9f,4.6f,0);
                RobotPart(arm,"Shoulder",Vector3.zero,new Vector3(1.25f,1.3f,1.5f),red);
                RobotPart(arm,"Forearm",new Vector3(0,-1.1f,.15f),new Vector3(.95f,1.6f,1.1f),steel);
                RobotPart(arm,"Sword fist",new Vector3(0,-1.8f,.6f),new Vector3(1.1f,.9f,1.4f),red);
                var sword=new GameObject(side<0?"Green giant saber":"Red giant saber").transform;
                sword.SetParent(arm,false);sword.localPosition=new Vector3(0,-1.8f,.8f);
                if(side<0)endlessMechOtherSword=sword;else endlessMechSword=sword;
                var blade=Shape(sword,"Colored energy blade",new Vector3(0,0,3),new Vector3(.4f,.5f,5.8f),Color.white,false);
                blade.GetComponent<Renderer>().sharedMaterial=EndlessGlow(side<0?Color.green:Color.red);
                Shape(sword,"White energy core",new Vector3(0,0,3),new Vector3(.12f,.52f,5.65f),Color.white,false);

            }
            var pilot=new GameObject("Player cockpit pilot").transform;pilot.SetParent(root,false);pilot.localPosition=new Vector3(0,3.2f,1);pilot.localScale=Vector3.one*.65f;
            BuildPerson(pilot,false);var visual=pilot.GetComponent<OfficeCharacterVisual>();if(visual!=null)visual.enabled=false;
            RobotPart(root,"Cockpit bottom frame",new Vector3(0,3.1f,1.05f),new Vector3(1.5f,.25f,.35f),steel);
        }
    }
}
