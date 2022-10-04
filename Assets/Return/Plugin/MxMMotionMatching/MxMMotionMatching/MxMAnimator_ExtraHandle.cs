using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using Return;

namespace MxM
{

    //public static class ArrayExtension
    //{
    //    public static void CreateIfNull<T>(ref this T array,int num = 1) where T:class,IList<T>,new()
    //    {
    //        if (array.IsNull())
    //            array = new();
    //    }
    //}
    public partial class MxMAnimator
    {
        //============================================================================================
        /**
        *  @brief Sets up any MxM extension components that are attached. This is called during the
        *  Initialize function.
        *         
        *********************************************************************************************/
        public virtual void UpdateExtensions()
        {
            //Extensions
            if (m_phase1Extensions == null)
                m_phase1Extensions = new List<IMxMExtension>(2);

            if (m_phase2Extensions == null)
                m_phase2Extensions = new List<IMxMExtension>(2);

            if (m_postExtensions == null)
                m_postExtensions = new List<IMxMExtension>(2);

            var attachedExtensions = GetComponents<IMxMExtension>();

            foreach (IMxMExtension extension in attachedExtensions)
            {
                extension.Initialize();

                if (extension.DoUpdatePhase1)
                    m_phase1Extensions.CheckAdd(extension);

                if (extension.DoUpdatePhase2)
                    m_phase2Extensions.CheckAdd(extension);

                if (extension.DoUpdatePost)
                    m_postExtensions.CheckAdd(extension);
            }
        }

        /// <summary>
        /// Set mxm extension to mxm animator.
        /// </summary>
        public virtual void SetExtension(IMxMExtension extension)
        {
            extension.Initialize();

            if (extension.DoUpdatePhase1)
                m_phase1Extensions.CheckAdd(extension);

            if (extension.DoUpdatePhase2)
                m_phase2Extensions.CheckAdd(extension);

            if (extension.DoUpdatePost)
                m_postExtensions.CheckAdd(extension);
        }

        /// <summary>
        /// Set mxm extension to mxm animator.
        /// </summary>
        public virtual void RemoveExtension(IMxMExtension extension)
        {
            extension.Initialize();

            if (extension.DoUpdatePhase1)
                m_phase1Extensions.Remove(extension);

            if (extension.DoUpdatePhase2)
                m_phase2Extensions.Remove(extension);

            if (extension.DoUpdatePost)
                m_postExtensions.Remove(extension);
        }

    }
}
