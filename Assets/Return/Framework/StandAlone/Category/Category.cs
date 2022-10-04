using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Sirenix.OdinInspector;
using Object = UnityEngine.Object;

namespace Return
{
    /// <summary>
    /// Definition classes that can been multi-type depend and inherit.
    /// </summary>
    public class Category : PresetDatabase, IEquatable<Category>, INode<Category>
    {
        /// <summary>
        /// ?? dynamic serach cache
        /// </summary>
        protected static HashSet<Category> SearchCatch = new (100);

        #if UNITY_EDITOR

        [OnValueChanged(nameof(AddInherit))]
        [PropertySpace(5)]
        [PropertyOrder(2)]
        [BoxGroup("Inherit", ShowLabel = false)]
        [ShowInInspector]
        [DropZone(typeof(Category), "Drop Inherit Category")]
        Object m_AddInherit;

        void AddInherit()
        {
            if (m_AddInherit==null)
                return;

            if (m_AddInherit is Category category)
            {
                if (InheritType == null)
                    InheritType = new();

                if (InheritType.Add(category))
                {
                    category.DeriveType.Add(this);
                    category.Dirty();
                }
            }

            m_AddInherit = null;
        }



        [OnValueChanged(nameof(AddDerive))]
        [PropertySpace(5)]
        [PropertyOrder(2)]
        [BoxGroup("Derive", ShowLabel = false)]
        [ShowInInspector]
        [DropZone(typeof(Category), "Drop Derive Category")]
        Object m_AddDerive;

        void AddDerive()
        {
            if (m_AddDerive == null)
                return;

            if (m_AddDerive is Category category)
            {
                if (DeriveType == null)
                    DeriveType = new();

                if (DeriveType.Add(category))
                {
                    category.InheritType.Add(this);
                    category.Dirty();
                }
            }

            m_AddDerive = null;
        }


#endif
#if UNITY_EDITOR
        [PropertySpace(10, 10)]
        [PropertyOrder(3)]
        [BoxGroup("Inherit")]

        [ListDrawerSettings(Expanded = true, NumberOfItemsPerPage = 10, HideAddButton = true, CustomRemoveElementFunction = nameof(RemoveInherit))]
#endif
        public HashSet<Category> InheritType = new HashSet<Category>();

#if UNITY_EDITOR
        void RemoveInherit(Category category)
        {
            if (!category)
                return;

            if (InheritType.Remove(category))
            {
                category.DeriveType.Remove(this);
                category.Dirty();
            }
        }

#endif

#if UNITY_EDITOR
        [PropertySpace(10, 10)]
        [PropertyOrder(3)]
        [BoxGroup("Derive")]
        [ListDrawerSettings(Expanded = true, NumberOfItemsPerPage = 10, HideAddButton = true, CustomRemoveElementFunction = nameof(RemoveDerive))]
#endif
        public HashSet<Category> DeriveType = new HashSet<Category>();

#if UNITY_EDITOR
        void RemoveDerive(Category category)
        {
            if (!category)
                return;

            if (DeriveType.Remove(category))
            {
                category.InheritType.Remove(this);
                category.Dirty();
            }
        }
#endif


        public IEnumerator<Category> Parents => InheritType.GetEnumerator();
        public IEnumerator<Category> Childs => DeriveType.GetEnumerator();

        /// <summary>
        /// Is this category contains target
        /// </summary>
        public bool Contains(Category other)
        {
            SearchCatch.Clear();
            return SearchContains(other);
        }

        protected bool SearchContains(Category other)
        {
            if (!SearchCatch.Add(this))
                return false;

            if (DeriveType.Contains(other))
            {
                SearchCatch.Clear();
                return true;
            }


            var childs = Childs;

            while (childs.MoveNext())
                if (childs.Current.SearchContains(other))
                    return true;
                else
                    Debug.Log(string.Format("{0} compare to {1} failure.", other, this));
            //else
            //{
            //    var enumerator = InheritType.GetEnumerator();
            //    while (enumerator.MoveNext())
            //        if (enumerator.Current.Contains(other))
            //            return true;
            //}

            return false;
        }

        public bool Equals(Category other)
        {
            if (other == null)
                return false;

            // reference equal
            if (this == other)
                return true;

            // different type
            if (this.GetType() != other.GetType())
                return false;

            // same type same id 
            if (Title.Equals(other.Title))
                return true;


            return false;

            // **check inherit => other func

            var length = InheritType.Count;

            if (length != other.InheritType.Count)
                return false;


            return InheritType.Count != other.InheritType.Count;

            var types = other.InheritType.GetEnumerator();

            while (types.MoveNext())
                if (!InheritType.Contains(types.Current))
                    return false;


            return true;
        }

        
    }


    public interface ICategory
    {
        Category Category { get; }
    }
}