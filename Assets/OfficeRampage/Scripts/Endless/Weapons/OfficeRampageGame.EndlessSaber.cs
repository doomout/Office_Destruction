using UnityEngine;
namespace OfficeRampage
{
    public sealed partial class OfficeRampageGame
    {
        private Material endlessEnergyMaterial;
        private void EnergyBlade(Transform root,Vector3 position,Vector3 size)
        {
            if(endlessEnergyMaterial==null)
            {endlessEnergyMaterial=new Material(Shader.Find("Universal Render Pipeline/Unlit"));endlessEnergyMaterial.color=new Color(.05f,.9f,1);}
            var blade=Shape(root,"Energy blade",position,size,Cyan,false);
            blade.GetComponent<Renderer>().sharedMaterial=endlessEnergyMaterial;
            Shape(root,"White energy core",position,new Vector3(size.x*.42f,size.y*1.03f,size.z*.98f),Color.white,false);
        }
        private void BuildEndlessSaber()
        {
            Shape(weapon,"Saber grip",new Vector3(0,0,.2f),new Vector3(.15f,.15f,.5f),Navy,false);
            Shape(weapon,"Saber emitter",new Vector3(0,0,.5f),new Vector3(.23f,.23f,.14f),Color.gray,false);
            EnergyBlade(weapon,new Vector3(0,0,1.4f),new Vector3(.17f,.17f,1.7f));
        }
        private void BuildEndlessMechRemote()
        {
            Shape(weapon,"Mech remote",new Vector3(0,.1f,.35f),new Vector3(.4f,.16f,.6f),new Color(.65f,.04f,.05f),false);
            Shape(weapon,"Mech activation core",new Vector3(0,.22f,.35f),Vector3.one*.22f,Cyan,false,PrimitiveType.Sphere);
            Shape(weapon,"Mech antenna",new Vector3(.15f,.15f,.7f),new Vector3(.05f,.05f,.4f),Color.gray,false);
        }
    }
}
