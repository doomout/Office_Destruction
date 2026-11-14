using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
namespace OfficeRampage.Editor
{
    // Explicit editor command; no automatic scene modification or random runtime generation.
    public static class OfficeArtBuilder
    {
        const string Root="Assets/OfficeRampage/Art/KenneyMiniCharacters";
        [MenuItem("Tools/Office Rampage/Rebuild Character Prefabs")]
        public static void BuildCharacters()
        {
            Directory.CreateDirectory(Root+"/Prefabs"); Directory.CreateDirectory("Assets/OfficeRampage/Resources/OfficeCharacters"); AssetDatabase.Refresh();
            var mat=AssetDatabase.LoadAssetAtPath<Material>(Root+"/CharacterURP.mat");
            if(mat==null) { mat=new Material(Shader.Find("Universal Render Pipeline/Lit")); AssetDatabase.CreateAsset(mat,Root+"/CharacterURP.mat"); }
            mat.mainTexture=AssetDatabase.LoadAssetAtPath<Texture2D>(Root+"/Textures/colormap.png"); mat.color=Color.white;
            var catalog=AssetDatabase.LoadAssetAtPath<OfficeCharacterCatalog>("Assets/OfficeRampage/Resources/OfficeCharacters/Catalog.asset");
            if(catalog==null) {catalog=ScriptableObject.CreateInstance<OfficeCharacterCatalog>();AssetDatabase.CreateAsset(catalog,"Assets/OfficeRampage/Resources/OfficeCharacters/Catalog.asset");}
            var male=new System.Collections.Generic.List<GameObject>(); var female=new System.Collections.Generic.List<GameObject>();
            foreach(var file in Directory.GetFiles(Root,"*.fbx").OrderBy(x=>x))
            {
                string path=file.Replace('\\','/'); var importer=(ModelImporter)AssetImporter.GetAtPath(path);
                importer.animationType=ModelImporterAnimationType.Legacy; importer.importAnimation=true; importer.materialImportMode=ModelImporterMaterialImportMode.None; importer.SaveAndReimport();
                var source=AssetDatabase.LoadAssetAtPath<GameObject>(path); var holder=new GameObject(Path.GetFileNameWithoutExtension(path));
                var instance=(GameObject)PrefabUtility.InstantiatePrefab(source); instance.transform.SetParent(holder.transform,false);
                Bounds bounds=new Bounds(); bool found=false;
                foreach(var renderer in instance.GetComponentsInChildren<Renderer>())
                {renderer.sharedMaterial=mat; if(!found){bounds=renderer.bounds;found=true;}else bounds.Encapsulate(renderer.bounds);}
                float scale=1.9f/Mathf.Max(.01f,bounds.size.y); instance.transform.localScale*=scale;
                instance.transform.localPosition=Vector3.up*(-bounds.min.y*scale);
                var anim=instance.GetComponent<Animation>(); if(anim==null)anim=instance.AddComponent<Animation>();
                var clips=AssetDatabase.LoadAllAssetsAtPath(path).OfType<AnimationClip>().Where(c=>!c.name.StartsWith("__preview__")).ToArray();
                foreach(var clip in clips) {var key=clip.name.Split('|').Last(); anim.AddClip(clip,key);}
                anim.playAutomatically=false; anim.cullingType=AnimationCullingType.AlwaysAnimate;
                var saved=PrefabUtility.SaveAsPrefabAsset(holder,Root+"/Prefabs/"+holder.name+".prefab");
                if(holder.name.Contains("female"))female.Add(saved);else if(!holder.name.EndsWith("male-g")) male.Add(saved);
                File.AppendAllText("art-import-report.txt",holder.name+" height="+bounds.size.y+" clips="+string.Join(",",clips.Select(c=>c.name))+"\n");
                UnityEngine.Object.DestroyImmediate(holder);
            }
            catalog.male=male.ToArray();catalog.female=female.ToArray();EditorUtility.SetDirty(catalog);EditorUtility.SetDirty(mat);AssetDatabase.SaveAssets();
        }
        public static void PrepareValidation()
        {
            BuildCharacters(); BakeInitialFloors(); AssetDatabase.SaveAssets(); AssetDatabase.Refresh(); EditorApplication.Exit(0);
        }
        // Run once to author the initial JSON assets. Existing authored maps are never overwritten.
        private static void BakeInitialFloors()
        {
            const string dest="Assets/OfficeRampage/Resources/OfficeLayouts";Directory.CreateDirectory(dest);
            string[] names={"영업팀 사무실","기획팀 사무실","관리팀 사무실","임원 사무실","대표 사무실"};
            for(int floor=1;floor<=5;floor++)
            {
                string path=dest+"/floor"+floor+".json";if(File.Exists(path))continue;
                var layout=floor==5?OfficeLayout.CreateExecutive():OfficeLayout.Create(floor-1,4100+floor);
                var nav=new OfficeNavigation(layout);int count=new[]{18,11,7,6,5}[floor-1];var rng=new System.Random(8300+floor);
                var data=new OfficeFixedFloor {width=layout.width,length=layout.length,name=names[floor-1]};
                data.furniture=layout.furniture.Select(p=>new OfficeFixedFloor.Spot{x=p.x,z=p.z,kind=p.kind,yaw=rng.Next(2)*180}).ToArray();
                data.oil=layout.oil.Select(p=>new OfficeFixedFloor.Spot{x=p.x,z=p.z}).ToArray();
                data.routes=layout.route.Select(a=>new OfficeFixedFloor.Aisle{x=a.x,z=a.z,width=a.width,depth=a.depth}).ToArray();
                data.enemies=nav.SpawnPoints(count,rng,floor==5).Select(p=>new OfficeFixedFloor.Spot{x=p.x,z=p.z}).ToArray();
                data.potions=nav.SpawnPoints(2,new System.Random(9700+floor),floor==5).Select(p=>new OfficeFixedFloor.Spot{x=p.x,z=p.z}).ToArray();
                File.WriteAllText(path,JsonUtility.ToJson(data,true));
            }
        }
    }
}
