using System.Collections.Generic;
using UnityEngine;
namespace OfficeRampage
{
    public sealed class OfficeBuildingCollapse : MonoBehaviour
    {
        private struct Piece { public Transform part; public Vector3 origin,velocity,spin; public float delay; }
        private readonly List<Piece> pieces=new List<Piece>();
        private OfficeRampageGame game;
        private Material material;
        private Vector3 blast;
        private float age;
        public int PieceCount { get { return pieces.Count; } }
        public void Initialize(OfficeRampageGame owner,Transform shell,float width,float start,float end,Vector3 center)
        {
            game=owner;blast=center;
            material=new Material(Shader.Find("Universal Render Pipeline/Lit"));material.color=new Color(.64f,.68f,.7f);
            float length=end-start;
            // Visible fragments replace the cutaway roof, including in the high nuclear camera.
            for(int x=0;x<6;x++) for(int z=0;z<8;z++)
                Add(new Vector3(-width/2+(x+.5f)*width/6,11.6f,start+(z+.5f)*length/8),new Vector3(width/6-.15f,.22f,length/8-.15f));
            for(int side=-1;side<=1;side+=2)
            {
                for(int z=0;z<8;z++) for(int y=0;y<3;y++)
                    Add(new Vector3(side*width/2,1.9f+y*3.8f,start+(z+.5f)*length/8),new Vector3(.25f,3.65f,length/8-.15f));
                for(int x=0;x<6;x++) for(int y=0;y<3;y++)
                    Add(new Vector3(-width/2+(x+.5f)*width/6,1.9f+y*3.8f,side<0?start:end),new Vector3(width/6-.15f,3.65f,.25f));
            }
            shell.gameObject.SetActive(false);
        }
        private void Add(Vector3 at,Vector3 size)
        {
            var block=GameObject.CreatePrimitive(PrimitiveType.Cube);block.name="Broken building slab";block.transform.SetParent(transform,false);
            block.transform.position=at;block.transform.localScale=size;block.GetComponent<Collider>().enabled=false;Destroy(block.GetComponent<Collider>());
            block.GetComponent<Renderer>().sharedMaterial=material;
            Vector3 away=at-blast;away.y=0;
            pieces.Add(new Piece {part=block.transform,origin=at,delay=away.magnitude/35,
                velocity=away.normalized*Random.Range(8,15)+Vector3.up*Random.Range(4,10),spin=Random.onUnitSphere*Random.Range(80,180)});
        }
        private void Update()
        {
            if(game==null || !game.RunActive) return;
            age+=Time.deltaTime;
            foreach(var piece in pieces)
            {
                float t=Mathf.Max(0,age-piece.delay);
                piece.part.position=piece.origin+piece.velocity*t+Vector3.down*(4.9f*t*t);
                piece.part.rotation=Quaternion.Euler(piece.spin*t);
                // Keep foreground debris from filling the nuclear cinematic camera.
                var camera=Camera.main;
                var renderer=piece.part.GetComponent<Renderer>();
                if(camera!=null) renderer.enabled=renderer.bounds.SqrDistance(camera.transform.position)>64;
            }
            if(age>=6) Destroy(gameObject);
        }
        private void OnDestroy() { if(material!=null) Destroy(material); }
    }
}
