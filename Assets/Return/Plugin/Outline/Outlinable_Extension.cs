using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace EPOOutline
{
    public partial class Outlinable // Extension
    {


        public void AddAllChildRenderersToRenderingList(Transform root,RenderersAddingMode renderersAddingMode = RenderersAddingMode.All)
        {
            if (root == null)
                root = transform;

            outlineTargets.Clear();
            var renderers = root.GetComponentsInChildren<Renderer>(true);

            foreach (var renderer in renderers)
            {
                if (!MatchingMode(renderer, renderersAddingMode))
                    continue;

                var submeshesCount = GetSubmeshCount(renderer);

                var max = 5;

                if(submeshesCount> max)
                {
                    Debug.LogErrorFormat("Outline target {0} has {1} sub mesh, combine mesh will be ignore.",renderer,submeshesCount);
                    submeshesCount = max;
                }

                for (var index = 0; index < submeshesCount; index++)
                    TryAddTarget(new OutlineTarget(renderer, index));
            }
        }

        public void Clean()
        {
            enabled = false;
            outlineTargets.Clear();
        }


        [Obsolete]
        public virtual void SetData(Outlinable preset)
        {
            this.outlineParameters=preset.outlineParameters;
        }

        [Obsolete]
        static OutlineProperties Copy(OutlineProperties preset)
        {
            var prop = new OutlineProperties()
            {
                Enabled=preset.Enabled,
                Color=preset.Color,
                DilateShift=preset.DilateShift,
                BlurShift=preset.BlurShift,
            };

            return prop;
        }
    }
}


