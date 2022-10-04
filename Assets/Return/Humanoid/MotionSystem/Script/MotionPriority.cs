using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Sirenix.OdinInspector;
using System.Linq;


#if UNITY_EDITOR
using Sirenix.Utilities;
using Sirenix.OdinInspector.Editor;
#endif


namespace Return.Humanoid
{
    [Serializable]
    public struct MotionPriority : IEquatable<MotionPriority>
    {
        [HideInInspector]
        public MotionType Type;


        [Range(0, 255)]
        [LabelWidth(70)]
        [LabelText("$Type")]
        public byte Priority;
        public ActionType ActionType;

        public MotionPriority(MotionType type, byte value)
        {
            Type = type;
            Priority = value;
            ActionType = default;
        }

        public MotionPriority(MotionType type)
        {
            Type = type;
            Priority = 0;
            ActionType = default;
        }

        public bool Equals(MotionPriority other)
        {
            return Type == other.Type && Priority == other.Priority;
        }

    }

    [Serializable]
    public class MotionPriorityList
    {
        [SerializeField]
        [ValueDropdown("CustomAddPriorityButton", IsUniqueList = true, DrawDropdownForListElements = false, DropdownTitle = "Modify Stats")]
        [ListDrawerSettings(DraggableItems = false, Expanded = true)]
        private List<MotionPriority> stats = new List<MotionPriority>();

        public MotionPriority this[int index]
        {
            get { return this.stats[index]; }
            set { this.stats[index] = value; }
        }

        public int Count
        {
            get { return this.stats.Count; }
        }

        public byte this[MotionType type]
        {
            get
            {
                for (int i = 0; i < this.stats.Count; i++)
                {
                    if (this.stats[i].Type == type)
                    {
                        return this.stats[i].Priority;
                    }
                }

                return 0;
            }
            set
            {
                for (int i = 0; i < this.stats.Count; i++)
                {
                    if (this.stats[i].Type == type)
                    {
                        var val = this.stats[i];
                        val.Priority = value;
                        this.stats[i] = val;
                        return;
                    }
                }

                this.stats.Add(new MotionPriority(type, value));
            }
        }

#if UNITY_EDITOR
        // Finds all available stat-BindingTypes and excludes the BindingTypes that the statList already contains, so we don't get multiple entries of the same type.
        private IEnumerable CustomAddPriorityButton()
        {
            return Enum.GetValues(typeof(MotionType)).Cast<MotionType>()
                .Except(this.stats.Select(x => x.Type))
                .Select(x => new MotionPriority(x))
                .AppendWith(this.stats)
                .Select(x => new ValueDropdownItem(x.Type.ToString(), x));
        }
#endif
    }

#if UNITY_EDITOR
    internal class StatListValueDrawer : OdinValueDrawer<MotionPriorityList>
    {
        protected override void DrawPropertyLayout(GUIContent label)
        {
            // This would be the "private List<StatValue> stats" field.
            this.Property.Children[0].Draw(label);
        }
    }

#endif
}
