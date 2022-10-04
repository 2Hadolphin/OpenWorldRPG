#if UNITY_EDITOR
using System.Collections.Generic;
using System.Text;
using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;
using UnityEditor.AddressableAssets.Settings.GroupSchemas;
using UnityEngine.AddressableAssets;
using UnityEngine;

namespace Return
{
    public static partial class AddressableEditorUtility
    {



        public static T GetEditorAsset<T>(this AssetReference reference) where T : UnityEngine.Object
        {
            var path = AssetDatabase.GUIDToAssetPath(reference.AssetGUID);

            var subName = reference.SubObjectName;

            if (string.IsNullOrEmpty(subName))
                return AssetDatabase.LoadAssetAtPath<T>(path);

            var assets = AssetDatabase.LoadAllAssetsAtPath(path);

            foreach (var asset in assets)
                if (asset.name == subName)
                    return asset as T;

            return null;
        }

        public static void SetAddressableGroup(UnityEngine.Object obj, string groupName)
        {
            var settings = AddressableAssetSettingsDefaultObject.Settings;

            if (settings)
            {
                var group = settings.FindGroup(groupName);
                if (!group)
                    group = settings.CreateGroup(groupName, false, false, true, null, typeof(ContentUpdateGroupSchema), typeof(BundledAssetGroupSchema));

                var assetpath = AssetDatabase.GetAssetPath(obj);
                var guid = AssetDatabase.AssetPathToGUID(assetpath);

                var e = settings.CreateOrMoveEntry(guid, group, false, false);
                var entriesAdded = new List<AddressableAssetEntry> { e };

                group.SetDirty(AddressableAssetSettings.ModificationEvent.EntryMoved, entriesAdded, false, true);
                settings.SetDirty(AddressableAssetSettings.ModificationEvent.EntryMoved, entriesAdded, true, false);
            }
        }

        public static void LogAssetReference(this AssetReference reference)
        {
            var sb = new StringBuilder();

            sb.AppendLine(nameof(reference.IsValid) + " : " + reference.IsValid());
            sb.AppendLine(nameof(reference.IsDone) + " : " + reference.IsDone);
            sb.AppendLine(nameof(reference.OperationHandle.IsValid) + " : " + reference.OperationHandle.IsValid());
            sb.AppendLine(nameof(reference.OperationHandle.IsDone) + " : " + reference.OperationHandle.IsDone);

            if (reference.OperationHandle.IsValid())
                sb.AppendLine(nameof(reference.OperationHandle.Status) + " : " + reference.OperationHandle.Status);

            Debug.Log(sb);
        }
    }
}
#endif