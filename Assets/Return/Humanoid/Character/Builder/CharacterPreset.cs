using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using Return.Agents;
using UnityEngine.Assertions;
using Return.Humanoid.Modular;
using UnityEngine.AddressableAssets;

namespace Return.Humanoid.Character
{
    public class CharacterPreset : HumanoidModularPreset
    {
        //public Dictionary<object,SkinnedMeshRendererPreset>

        public AssetReferenceGameObject CharacterSkeleton;

        [SerializeField]
        [ListDrawerSettings(Expanded = true)]
        public List<SkinnedMeshRendererPreset> Skins=new();

        [Button]
        public virtual GameObject CreateCharacter(int skinIndex=-1,int matIndex=-1)
        {
            var character=CharacterSkeleton.LoadInstantiateGameObject();

            Assert.IsNotNull(character);

            var skin = skinIndex<0? Skins.Random() : Skins[Skins.Loop(skinIndex)];

            var characterWrapper = character.InstanceIfNull<CharacterDataWrapper>();
            characterWrapper.Preset = this;

            var skeletonRoot =character.transform.Find("RootTransform");
            
            var renderer=skin.SetRenderer(skeletonRoot);

            characterWrapper.CharacterRenderers.Add(renderer);

            return character;
        }

        [Button]
        public virtual void ChangeCharacter(GameObject character,int skinIndex=0,int matIndex=0)
        {

            Assert.IsNotNull(character);

            skinIndex = Skins.Loop(skinIndex);

            var skin = Skins[skinIndex];

            var characterWrapper = character.InstanceIfNull<CharacterDataWrapper>();
            characterWrapper.Preset = this;

            var skeletonRoot = character.transform.Find("RootTransform");

            var renderer = skin.SetRenderer(skeletonRoot, characterWrapper.CharacterRenderers[0].gameObject);
        }


        #region Obsolete
        public GameObject Prefab;
       
        [ListDrawerSettings(Expanded = true)]
        public List<Mesh> Meshs = new();

        [ListDrawerSettings(Expanded =true)]
        public List<Material> Materials=new();

        [ListDrawerSettings(Expanded = true)]
        public HashSet<BodySlotBinding> BodySlotBindings = new HashSet<BodySlotBinding>();


        [System.Obsolete]
        [Button("Build Character")]
        public virtual GameObject BuildCharacter(PR pr = default,Transform transform=null)
        {
            if (transform.NotNull())
                pr=transform.InverseTransformPR(pr);

            var character =transform? 
                Instantiate(Prefab,pr,pr ,transform):
                Instantiate(Prefab,pr,pr);

            var renderer = character.GetComponentInChildren<SkinnedMeshRenderer>();

            renderer.sharedMesh = Meshs.Random();
            renderer.sharedMaterial = Materials.Random();

            return character;
        }

        public override void LoadModule(GameObject @object)
        {

            var wrapper = @object.InstanceIfNull<CharacterDataWrapper>();
            wrapper.Preset = this;

            var ragdoll = @object.InstanceIfNull<RagdollSystem>();

            //var anim = @object.GetComponent<Animator>();
            //Assert.IsNotNull(anim);

            //foreach (var slotBinding in BodySlotBindings)
            //{
            //    slotBinding.CreateSlotBinding(anim);
            //}

        }
        #endregion
    }
}