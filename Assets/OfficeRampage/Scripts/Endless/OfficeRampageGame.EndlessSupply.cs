using System.Collections.Generic;
using UnityEngine;
namespace OfficeRampage
{
    public sealed partial class OfficeRampageGame
    {
        private const float EndlessWeaponInterval=3;
        private float endlessSupportClock;
        private readonly List<int> endlessWeaponBag=new List<int>();
        private int lastEndlessDrop=-1;
        private void ResetEndlessSupply(){endlessSupportClock=8;endlessWeaponBag.Clear();lastEndlessDrop=-1;}
        private int NextEndlessWeapon()
        {
            if(endlessWeaponBag.Count==0)
            {
                for(int id=0;id<EndlessWeapons.Length;id++)if(id!=4)endlessWeaponBag.Add(id);
                for(int i=endlessWeaponBag.Count-1;i>0;i--){int j=Random.Range(0,i+1);int temp=endlessWeaponBag[i];endlessWeaponBag[i]=endlessWeaponBag[j];endlessWeaponBag[j]=temp;}
            }
            // Keep late-game blasts out of the opening 30 seconds without consuming their bag entries.
            int index=endlessWeaponBag.FindIndex(id=>(endlessTime>=30||(id!=7&&id!=13))&&id!=lastEndlessDrop&&id!=endlessWeapon);
            if(index<0)index=endlessWeaponBag.FindIndex(id=>endlessTime>=30||(id!=7&&id!=13));
            if(index<0)return 8; // Only two locked rewards can remain before 30 seconds.
            int result=endlessWeaponBag[index];endlessWeaponBag.RemoveAt(index);lastEndlessDrop=result;return result;
        }
        private void TickEndlessSupply(float dt)
        {
            endlessSupplyClock-=dt;
            if(endlessSupplyClock<=0){SpawnEndlessPickup(0,NextEndlessWeapon(),NearbyEndlessDrop());endlessSupplyClock=EndlessWeaponInterval;}
            endlessSupportClock-=dt;
            if(endlessSupportClock<=0){int cycle=(int)(endlessTime/8);SpawnEndlessPickup(Random.value<.14f?3:cycle%2==0?1:2,0,NearbyEndlessDrop());endlessSupportClock=8;}
        }
        private Vector3 NearbyEndlessDrop()
        {
            for(int i=0;i<36;i++)
            {
                float angle=Random.value*Mathf.PI*2;Vector3 at=player.position+new Vector3(Mathf.Cos(angle),0,Mathf.Sin(angle))*Random.Range(6f,12f);
                at.x=Mathf.Clamp(at.x,-28,28);at.z=Mathf.Clamp(at.z,4,60);at.y=.08f;
                if(Vector3.Distance(at,player.position)<4||Physics.CheckSphere(at+Vector3.up,.85f))continue;
                bool occupied=false;foreach(var pickup in endlessPickups)if(pickup!=null&&Vector3.Distance(pickup.transform.position,at)<1.5f){occupied=true;break;}
                if(!occupied)return at;
            }
            return SafeEndlessDrop();
        }
        private bool MakeEndlessPickupSpace(int kind)
        {
            if(endlessPickups.Count<EndlessPickupCap)return true;
            int weapons=0;foreach(var p in endlessPickups)if(p.kind==0)weapons++;
            int index=-1;
            for(int pass=0;pass<2&&index<0;pass++)for(int i=0;i<endlessPickups.Count;i++)
            {
                var p=endlessPickups[i];if(Vector3.Distance(player.position,p.transform.position)<2.5f)continue;
                if(kind!=0&&p.kind==0&&weapons<=8)continue;
                if(pass==0&&((kind==0&&weapons<8)?p.kind==0:(p.kind==0)!=(kind==0)))continue;
                index=i;break;
            }
            if(index<0)return false;
            var old=endlessPickups[index];endlessPickups.RemoveAt(index);old.gameObject.SetActive(false);Destroy(old.gameObject);return true;
        }
    }
}
