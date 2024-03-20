using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace UIFramework
{
    internal static class UIExtensions
    {
        // Generic
        public static Color GetContrastingColor(in Color color)
        {
            float luminance = color.r * 0.299F + color.g * 0.587F + color.b * 0.114F;
            return luminance > 0.729F ? Color.black : Color.white;
        }

        // UGUI
        public static void FocusVertically(this ScrollRect instance, RectTransform target)
        {
            Vector2 viewportLocalPosition = instance.viewport.localPosition;
            Vector2 childLocalPosition = target.localPosition;
            instance.content.localPosition = new Vector2(
                instance.content.localPosition.x,
                0 - (viewportLocalPosition.y + childLocalPosition.y)
            );
        }

        public static T CreateOrGetFromCache<T>(in List<T> cache, in T template, in RectTransform parent) where T : UnityEngine.Object
        {
            T unityObject = null;
            if (cache.Count > 0)
            {
                unityObject = cache[cache.Count - 1];
                cache.RemoveAt(cache.Count - 1);
            }
            else
            {
                unityObject = Object.Instantiate(template, parent);
            }
            return unityObject;
        }
    }
}