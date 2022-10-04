// (c) Copyright Cleverous 2020. All rights reserved.


using UnityEditor;
using UnityEditor.IMGUI.Controls;
using Return.Database;
using Return.Drawer;

namespace Cleverous.VaultDashboard
{
    internal class DataEntityDropdownItem : AdvancedDropdownItem
    {
        public DataEntity Entity;
        public DataEntityDropdownItem(string name, DataEntity entity) : base(name)
        {
            Entity = entity;
        }
    }

    internal class VaultAdvancedDropdown : AdvancedDropdown
    {
        /*
         TODO
         https://forum.unity.com/threads/dropdownfield-popup-height-in-runtime-how-to-limit.1197157/
         m_OuterContainer.style.height = Mathf.Min(300,
                    m_MenuContainer.layout.height - m_MenuContainer.layout.y - m_OuterContainer.layout.y,
                    m_ScrollView.layout.height + m_OuterContainer.resolvedStyle.borderBottomWidth + m_OuterContainer.resolvedStyle.borderTopWidth);
         */

        public SerializedProperty TargetProperty;
        public VaultAdvancedDropdown(AdvancedDropdownState state) : base(state)
        {
        }

        protected override AdvancedDropdownItem BuildRoot()
        {
            AdvancedDropdownItem root = new AdvancedDropdownItem(AssetDropdownDrawer.CurrentFilterType.ToString());
            foreach (DataEntity data in AssetDropdownDrawer.CurrentContent)
            {
                root.AddChild(new DataEntityDropdownItem(data.Title, data));
            }

            return root;
        }

        protected override void ItemSelected(AdvancedDropdownItem item)
        {
            DataEntity entity = ((DataEntityDropdownItem) item).Entity;
            AssetDropdownDrawer.ItemSelected(TargetProperty, entity);
            TargetProperty.serializedObject.ApplyModifiedProperties();
        }
    }
}