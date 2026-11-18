using UnityEngine;
namespace OfficeRampage
{
    public sealed partial class OfficeRampageGame
    {
        private void SpawnEndlessPickup(int kind,int id,Vector3 at)
        {
            if(kind==0&&id==4)return; // Preserve story IDs, exclude the decoy from all endless drops.
            endlessPickups.RemoveAll(p=>p==null);
            if(!MakeEndlessPickupSpace(kind))return;
            var pickup=new GameObject("Ground equipment").AddComponent<OfficeEndlessPickup>();pickup.transform.SetParent(endlessRoot,false);pickup.transform.position=at;
            pickup.kind=kind;pickup.weaponId=id;pickup.DisplayName=kind==1?"갑옷 · 내구도 +60":kind==2?"회복 · 체력 +30":kind==3?"피해 2배 · 20초":EndlessWeapons[id];
            var color=kind==3?Color.magenta:kind==1?Cyan:kind==2?Color.green:id>=7?new Color(1,.55f,.1f):Color.yellow;
            Shape(pickup.transform,"Equipment pedestal",new Vector3(0,.1f,0),new Vector3(1,.15f,1),color,false,PrimitiveType.Cylinder);
            var visual=new GameObject("Equipment preview").transform;visual.SetParent(pickup.transform,false);visual.localPosition=Vector3.up*.8f;
            if(kind==0)
            {
                var oldWeapon=weapon;int oldIndex=weaponIndex,oldId=endlessWeapon;bool oldSaber=endlessSaber;
                try{endlessSaber=false;weapon=visual;weaponIndex=EndlessBases[id];endlessWeapon=id;BuildExpandedWeapon();}
                finally{weapon=oldWeapon;weaponIndex=oldIndex;endlessWeapon=oldId;endlessSaber=oldSaber;}
            }
            else if(kind==1){Shape(visual,"Armor vest",Vector3.zero,new Vector3(.7f,.8f,.25f),Cyan,false);Shape(visual,"Armor plate",new Vector3(0,0,-.16f),new Vector3(.5f,.5f,.08f),Color.white,false);}
            else if(kind==3){Shape(visual,"Double damage crystal",Vector3.zero,Vector3.one*.7f,Color.magenta,false).transform.localRotation=Quaternion.Euler(25,45,25);}
            else {Shape(visual,"Health bottle",Vector3.zero,new Vector3(.4f,.6f,.4f),Color.green,false,PrimitiveType.Capsule);}
            var label=new GameObject("Equipment name").AddComponent<TextMesh>();label.transform.SetParent(pickup.transform,false);label.transform.localPosition=Vector3.up*1.7f;label.anchor=TextAnchor.MiddleCenter;label.fontSize=36;label.characterSize=.08f;label.text=pickup.DisplayName;label.color=color;
            ApplyKoreanWorldFont(pickup.transform);StyleEndlessEnvironmentPickup(pickup.transform);endlessPickups.Add(pickup);
        }
        private void CollectEndless(OfficeEndlessPickup pickup)
        {
            if(!endlessMode||!RunActive||CombatLocked||pickup==null||!endlessPickups.Contains(pickup)||Vector3.Distance(player.position,pickup.transform.position)>2.5f)return;
            if(pickup.kind==0&&pickup.weaponId==4){endlessPickups.Remove(pickup);pickup.gameObject.SetActive(false);Destroy(pickup.gameObject);return;}
            if(EndlessMechActive&&pickup.kind==0){Notify("탑승 종료 후 새 무기를 주울 수 있습니다",2);return;}
            if(pickup.kind==3){endlessDoubleDamageLeft=20;}
            else if(pickup.kind==1){if(endlessArmor>=100)return;endlessArmor=Mathf.Min(100,endlessArmor+60);}
            else if(pickup.kind==2){if(hp>=100)return;hp=Mathf.Min(100,hp+30);}
            else
            {
                endlessSaber=false;ResetElements();whirlwindLeft=whirlwindReady=0;ClearDecoy();endlessWeapon=pickup.weaponId;
                int index=EndlessBases[endlessWeapon];if(index==2)ammunition[2]=200;if(index==5)ammunition[5]=100;if(index==6)ammunition[6]=20;
                if(endlessWeapon==4)decoysUsed[1]=0;
                endlessSpecialAmmo=endlessWeapon==7||endlessWeapon>=13?1:endlessWeapon>=8?40:0;
                bfgReady=index==7;bfgUsed=false;endlessEquipAllowed=true;
                try{SetWeapon(index);}finally{endlessEquipAllowed=false;}
                cooldown=.25f;
            }
            Notify(pickup.DisplayName+" 획득",2);audioBus.Play("combo",.5f);endlessPickups.Remove(pickup);pickup.gameObject.SetActive(false);Destroy(pickup.gameObject);nearbySupply=null;
        }
        private bool BuildEndlessSpecialModel()
        {
            if(!endlessMode||endlessWeapon<8)return false;
            if(endlessWeapon>=14){BuildEndlessSummonWeapon();return true;}
            if(endlessWeapon==13){BuildRedButton(weapon);return true;}
            Color color=endlessWeapon==8?Cyan:endlessWeapon==9?Color.magenta:endlessWeapon==10?Orange:endlessWeapon==11?Color.blue:Color.yellow;
            Shape(weapon,"Special handle",new Vector3(0,0,.25f),new Vector3(.15f,.2f,.6f),Navy,false);
            if(endlessWeapon==8){Shape(weapon,"Stapler jaws",new Vector3(0,.15f,.6f),new Vector3(.3f,.2f,.8f),color,false);Shape(weapon,"Electric coil",new Vector3(0,.38f,.5f),new Vector3(.2f,.3f,.2f),color,false,PrimitiveType.Cylinder);}
            if(endlessWeapon==9){Shape(weapon,"Printer body",new Vector3(0,.2f,.5f),new Vector3(.7f,.4f,.7f),Color.white,false);Shape(weapon,"Printer feed roller",new Vector3(0,.23f,.9f),new Vector3(.12f,.3f,.12f),Navy,false,PrimitiveType.Cylinder).transform.localRotation=Quaternion.Euler(0,0,90);Shape(weapon,"Laser slot",new Vector3(0,.2f,.87f),new Vector3(.55f,.1f,.05f),color,false);}
            if(endlessWeapon==10){Shape(weapon,"Coffee pot",new Vector3(0,.2f,.5f),new Vector3(.5f,.5f,.5f),color,false,PrimitiveType.Cylinder);Shape(weapon,"Spout",new Vector3(0,.2f,.85f),new Vector3(.15f,.15f,.5f),Navy,false);}
            if(endlessWeapon==11){Shape(weapon,"Vacuum tank",new Vector3(0,.2f,.4f),new Vector3(.55f,.6f,.5f),color,false,PrimitiveType.Capsule);Shape(weapon,"Vacuum nozzle",new Vector3(0,.1f,.9f),new Vector3(.8f,.18f,.4f),Color.gray,false);}
            if(endlessWeapon==12)Shape(weapon,"Clock-out bell",new Vector3(0,.15f,.55f),new Vector3(.7f,.6f,.7f),color,false,PrimitiveType.Sphere);
            return true;
        }
    }
}
