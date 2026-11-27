using UnityEngine;
namespace OfficeRampage
{
    public sealed partial class OfficeCharacterVisual
    {
        private Transform rightWrist,rightKnuckle;
        private void CacheHandBones()
        {
            rightWrist=rightKnuckle=null;
            foreach(var bone in model.GetComponentsInChildren<Transform>(true))
            {
                if(bone.name=="Wrist.R")rightWrist=bone;
                else if(bone.name=="Middle1.R")rightKnuckle=bone;
            }
        }
        public bool TryGetRightHandGrip(out Vector3 position)
        {
            position=transform.position;
            if(rightWrist==null)return false;
            position=rightKnuckle!=null?Vector3.Lerp(rightWrist.position,rightKnuckle.position,.55f):rightWrist.position;
            return true;
        }
        private static bool IsHeldItem(string name)
        {
            return name=="Weapon"||name=="Baton"||name=="A4 hand"||name=="Security weapon"||
                name=="Driver baton"||name=="Document stack"||name=="Approval device";
        }
        // Resolve active children because weapon switching destroys old meshes at the end of the frame.
        private static Vector3 GripPosition(Transform item)
        {
            foreach(Transform part in item)
                if(part.gameObject.activeSelf&&(part.name=="Grip"||part.name=="Saber grip"||part.name=="Special handle"))return part.position;
            foreach(Transform part in item)
                if(part.gameObject.activeSelf&&(part.name=="Button case"||part.name=="Support controller"||
                    part.name=="Seismic controller"||part.name=="Meteor controller"||part.name=="Infernal book"||
                    part.name=="Traffic remote"||part.name=="Mech remote"))return part.TransformPoint(new Vector3(0,-.5f,0));
            Vector3 local=Vector3.zero;
            if(item.name=="Baton"||item.name=="Driver baton"||item.name=="A4 hand"||item.name=="Document stack")local=new Vector3(0,0,-.35f);
            else if(item.name=="Approval device")local=new Vector3(0,-.35f,0);
            return item.TransformPoint(local);
        }
        private void FollowHeldItems()
        {
            if(!TryGetRightHandGrip(out var hand))return;
            foreach(Transform item in transform)
            {
                if(!item.gameObject.activeSelf||!IsHeldItem(item.name))continue;
                // Translate after skeletal animation and procedural recoil/swing. Preserve aim, scale,
                // hierarchy and attack rotation; pin the visible grip rather than the model origin.
                item.position+=hand-GripPosition(item);
            }
        }
    }
}
