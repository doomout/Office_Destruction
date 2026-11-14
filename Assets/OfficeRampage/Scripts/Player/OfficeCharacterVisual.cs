using UnityEngine;
namespace OfficeRampage
{
    public sealed class OfficeCharacterVisual : MonoBehaviour
    {
        private OfficeRampageGame game;
        private GameObject model;
        private Animation animationPlayer;
        private Vector3 previous;
        private float actionLeft;
        private bool cinematicAction;
        private string playing;
        private static OfficeCharacterCatalog catalog;
        public static void Attach(Transform root,OfficeRampageGame owner,bool boss)
        {
            if(catalog==null) catalog=Resources.Load<OfficeCharacterCatalog>("OfficeCharacters/Catalog");
            if(catalog==null) return;
            var visual=root.gameObject.AddComponent<OfficeCharacterVisual>(); visual.game=owner;
            visual.SetIdentity(false,boss?1:0,boss?10:0); visual.previous=root.position;
        }
        public void SetIdentity(bool female,int index,int rank)
        {
            var choices=female?catalog.female:catalog.male;
            if(choices==null || choices.Length==0) return;
            if(model!=null) { model.SetActive(false); Destroy(model); }
            model=Instantiate(choices[(index+rank)%choices.Length],transform,false); model.name="Character model";
            animationPlayer=model.GetComponentInChildren<Animation>(); playing=null; actionLeft=0;
            Play("idle"); HidePrimitives();
        }
        public void RefreshLegacy() { HidePrimitives(); }
        private void HidePrimitives()
        {
            foreach(Transform child in transform)
            {
                if(child==model?.transform || child.name=="Baton" || child.name=="A4 hand") continue;
                var renderer=child.GetComponent<Renderer>(); if(renderer!=null) renderer.enabled=false;
            }
        }
        public void PlayAction(string clip,bool duringCinematic=false)
        {
            if(actionLeft>.15f || animationPlayer==null || animationPlayer[clip]==null) return;
            actionLeft=.55f; cinematicAction=duringCinematic; Play(clip,true);
        }
        private void Play(string clip,bool once=false)
        {
            if(animationPlayer==null || animationPlayer[clip]==null || playing==clip) return;
            var state=animationPlayer[clip]; state.wrapMode=once?WrapMode.Once:WrapMode.Loop;
            animationPlayer.CrossFade(clip,.12f); playing=clip;
        }
        private void LateUpdate()
        {
            HidePrimitives();
            if(game==null || animationPlayer==null) return;
            var unit=GetComponentInParent<OfficeDefender>();
            bool frozen=unit!=null && unit.FrozenLeft>0;
            bool stopped=!game.RunActive || frozen || (game.CombatLocked && !cinematicAction);
            foreach(AnimationState state in animationPlayer) state.speed=stopped?0:1;
            Vector3 delta=transform.position-previous; delta.y=0; previous=transform.position;
            if(stopped) return;
            if(actionLeft>0) { actionLeft=Mathf.Max(0,actionLeft-Time.deltaTime); if(actionLeft>0) return; cinematicAction=false; }
            float speed=delta.magnitude/Mathf.Max(.001f,Time.deltaTime);
            Play(speed>6?"sprint":speed>.15f?"walk":"idle");
        }
    }
}
