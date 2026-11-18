using UnityEngine;
namespace OfficeRampage
{
    public sealed partial class OfficeRampageGame
    {
        // The attack sweeps across the body on Y, then returns to the forward rest pose.
        private static Quaternion HorizontalSwingRotation(float remaining)
        {
            if(remaining<=0)return Quaternion.identity;
            float t=1-Mathf.Clamp01(remaining);
            float angle=t<.65f?Mathf.Lerp(75,-75,Mathf.SmoothStep(0,1,t/.65f)):Mathf.Lerp(-75,0,Mathf.SmoothStep(0,1,(t-.65f)/.35f));
            return Quaternion.Euler(0,angle,0);
        }
        private void UpdateWeaponSwingPose()
        {
            weapon.localPosition=thunderLeft>0?new Vector3(.48f,1.65f,.1f):new Vector3(.48f,weaponIndex==0?1.15f:.95f,.25f);
            weapon.localRotation=thunderLeft>0?Quaternion.Euler(-90,0,0):weaponIndex==0?HorizontalSwingRotation(swing):Quaternion.Euler(weaponIndex==4?swing*100-20:-swing*60,-swing*25,0);
        }
    }
}
