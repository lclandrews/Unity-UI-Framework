#if UNITY_EDITOR
using System;

using UIFramework.UGUI;

using UnityEditor;
using UnityEditor.SceneManagement;

using UnityEngine;

namespace UIFramework.Editor.UGUI
{
    public static class ImagePlusPlusMenuItem
    {
        [MenuItem("GameObject/UI/ImagePlusPlus", false, 9)]
        public static void AddUIDocument(MenuCommand menuCommand)
        {
            GameObject parent = menuCommand.context as GameObject;
            Type type = typeof(ImagePlusPlus);
            var root = ObjectFactory.CreateGameObject(type.Name, type);
            GameObjectUtility.EnsureUniqueNameForSibling(root);

            // Works for all stages.
            StageUtility.PlaceGameObjectInCurrentStage(root);
            PrefabStage prefabStage = PrefabStageUtility.GetCurrentPrefabStage();
            if (prefabStage != null)
            {
                Undo.SetTransformParent(root.transform, prefabStage.prefabContentsRoot.transform, "");
            }

            Undo.SetCurrentGroupName("Create " + root.name);
            Selection.activeGameObject = root;
        }
    }
}
#endif