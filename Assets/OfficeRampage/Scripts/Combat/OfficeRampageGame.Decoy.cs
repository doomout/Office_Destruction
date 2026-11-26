using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
namespace OfficeRampage
{
    public sealed partial class OfficeRampageGame
    {
        private const int DecoysPerOffice=3;
        private readonly int[] decoysUsed = new int[OfficeStoryFloors.Boss+1];
        private Transform decoy, decoyPreview;
        private Vector3 decoyTarget, lureCenter;
        private bool decoyAiming, decoyValid, decoyFlying;
        private float lureLeft;
        private int lureRoom;
        private Coroutine decoyFlight;
        private static readonly Color Pink = new Color(1, .16f, .53f);
        public bool TryGetLure(int room, out Vector3 center)
        {
            center = lureCenter;
            return lureLeft > 0 && decoy != null && room == lureRoom && current.number == room;
        }
        private void ClearDecoy()
        {
            if (decoyFlight != null) StopCoroutine(decoyFlight);
            decoyFlight = null; lureLeft = 0; decoyFlying = decoyAiming = decoyValid = false;
            if (decoy != null) Destroy(decoy.gameObject);
            if (decoyPreview != null) Destroy(decoyPreview.gameObject);
            decoy = decoyPreview = null;
        }
        private void ResetDecoy()
        { ClearDecoy(); System.Array.Clear(decoysUsed, 0, decoysUsed.Length); }
        private void BuildDecoyModel(Transform root, float scale)
        {
            var model = new GameObject("Beauty decoy mannequin").transform;
            model.SetParent(root, false); model.localScale = Vector3.one * scale;
            BuildPerson(model, false); model.GetComponent<OfficeCharacterVisual>()?.SetIdentity(true,0,0);
            Shape(model, "Pink dress", new Vector3(0, .78f, 0), new Vector3(.76f, .75f, .49f), Pink, false);
            Shape(model, "Long hair", new Vector3(0, 1.65f, -.18f), new Vector3(.52f, .65f, .18f), Navy, false);
            CreateHeartMarker(model).localPosition = Vector3.up * 2.15f;
        }
        public Transform CreateHeartMarker(Transform parent)
        {
            var heart = new GameObject("Attraction heart").transform; heart.SetParent(parent, false);
            Shape(heart, "Left heart lobe", new Vector3(-.115f, .08f, 0), new Vector3(.28f,.28f,.09f), Pink, false, PrimitiveType.Sphere);
            Shape(heart, "Right heart lobe", new Vector3(.115f,.08f,0), new Vector3(.28f,.28f,.09f), Pink, false, PrimitiveType.Sphere);
            Shape(heart, "Heart tip", new Vector3(0,-.07f,0), new Vector3(.27f,.27f,.08f), Pink, false).transform.localRotation=Quaternion.Euler(0,0,45);
            return heart;
        }
        private Vector3 ThrowPoint(Vector3 start, Vector3 end, float t)
        { return Vector3.Lerp(start, end, t) + Vector3.up * (4 * t * (1-t) * 3); }
        private bool ValidDecoyLanding(Vector3 point)
        {
            if (point.x < -current.width/2+1 || point.x > current.width/2-1 || point.z < current.start+1 || point.z > current.end-1) return false;
            foreach (var col in Physics.OverlapCapsule(point+Vector3.up*.4f, point+Vector3.up*1.6f, .35f, ~0, QueryTriggerInteraction.Ignore))
                if (!IsPlayerCollider(col) && col.GetComponentInParent<OfficeDefender>() == null) return false;
            return true;
        }
        private bool ClearThrowPath(Vector3 start, Vector3 end)
        {
            Vector3 previous = start;
            for (int i=1;i<=24;i++)
            {
                Vector3 next = ThrowPoint(start,end,i/24f), delta=next-previous;
                foreach(var hit in Physics.SphereCastAll(previous,.12f,delta.normalized,delta.magnitude,~0,QueryTriggerInteraction.Ignore))
                    if (!IsPlayerCollider(hit.collider) && hit.collider.GetComponentInParent<OfficeDefender>()==null) return false;
                previous=next;
            }
            return true;
        }
        private void UpdateDecoyAim()
        {
            Vector3 origin=player.position+Vector3.up*1.4f;
            Ray ray=new Ray(view.transform.position,view.transform.forward);
            var ground=new Plane(Vector3.up,new Vector3(0,.2f,0));
            Vector3 aim = ground.Raycast(ray,out float distance) ? ray.GetPoint(distance) : player.position+Quaternion.Euler(0,yaw,0)*Vector3.forward*12;
            Vector3 flat=aim-player.position; flat.y=0; flat=Vector3.ClampMagnitude(flat,16);
            decoyTarget=new Vector3(Mathf.Clamp(player.position.x+flat.x,-current.width/2+1,current.width/2-1),.2f,
                Mathf.Clamp(player.position.z+flat.z,current.start+1,current.end-1));
            decoyValid=Vector3.Distance(player.position,decoyTarget)>1.5f && ValidDecoyLanding(decoyTarget) && ClearThrowPath(origin,decoyTarget);
            if(decoyPreview==null)
            {
                decoyPreview=new GameObject("Dotted throw preview").transform; decoyPreview.SetParent(world);
                for(int i=0;i<25;i++) Shape(decoyPreview,"Trajectory dot",Vector3.zero,Vector3.one*.1f,Cyan,false,PrimitiveType.Sphere);
            }
            decoyPreview.gameObject.SetActive(true);
            for(int i=0;i<25;i++)
            {
                var dot=decoyPreview.GetChild(i); dot.position=ThrowPoint(origin,decoyTarget,(i+1)/25f);
                dot.GetComponent<Renderer>().sharedMaterial=Mat(decoyValid?Cyan:Orange);
                dot.localScale=Vector3.one*(i==24?.4f:.1f);
            }
        }
        private void TickDecoy(float dt)
        {
            if(!RunActive || CombatLocked) return;
            if(lureRoom!=current.number && (decoy!=null || decoyFlying)) ClearDecoy();
            if(lureLeft>0) { lureLeft=Mathf.Max(0,lureLeft-dt); if(lureLeft<=0) ClearDecoy(); }
            var mouse=Mouse.current;
            if(weaponIndex!=4 || decoyFlying || decoysUsed[current.number]>=DecoysPerOffice) decoyAiming=false;
            else if(mouse!=null && Cursor.lockState==CursorLockMode.Locked && mouse.rightButton.wasPressedThisFrame) decoyAiming=!decoyAiming;
            if(decoyAiming) UpdateDecoyAim();
            else if(decoyPreview!=null) decoyPreview.gameObject.SetActive(false);
        }
        private void ThrowDecoy()
        {
            if(!RunActive || CombatLocked || weaponIndex!=4 || !decoyAiming || !decoyValid || decoyFlying || decoysUsed[current.number]>=DecoysPerOffice) return;
            Vector3 start=player.position+Vector3.up*1.4f, target=decoyTarget;
            if(!ValidDecoyLanding(target) || !ClearThrowPath(start,target)) return;
            ClearDecoy(); decoysUsed[current.number]++; lureRoom=current.number; decoyFlying=true;
            decoy=new GameObject("Thrown beauty decoy").transform; decoy.SetParent(world); decoy.position=start;
            BuildDecoyModel(decoy,.8f); swing=1; cooldown=.9f;
            audioBus.Play("swing",.5f); decoyFlight=StartCoroutine(FlyDecoy(start,target));
        }
        private IEnumerator FlyDecoy(Vector3 start, Vector3 target)
        {
            float t=0;
            while(t<1)
            {
                if(RunActive && !CombatLocked)
                {
                    t=Mathf.Min(1,t+Time.deltaTime/.8f);
                    decoy.position=ThrowPoint(start,target,t); decoy.rotation=Quaternion.Euler(0,yaw,t*360);
                }
                yield return null;
            }
            decoy.rotation=Quaternion.identity; lureCenter=target; lureLeft=10; decoyFlying=false; decoyFlight=null;
            audioBus.Play("combo",.4f); Notify("미인계 성공! 남성 직원을 10초 동안 유인합니다. 대표는 면역입니다.",3);
        }
        private void DrawDecoyHUD()
        {
            // Lure count/timer is part of DrawCompactSkills.
        }
    }
}
