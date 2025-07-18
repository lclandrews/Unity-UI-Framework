using UnityEditor;

using UnityEngine;

namespace UIFramework.Editor
{
    public static class Layout
    {
        public const float IndentPerLevel = 15.0F;

        public static float StandardVerticalHeight
        {
            get
            {
                return EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
            }
        }

        public static int CountChildProperties(this SerializedProperty property)
        {
            int count = 0;
            SerializedProperty childProperty = property.Copy();
            while(childProperty.NextVisible(true))
            {
                if (SerializedProperty.EqualContents(childProperty, property.GetEndProperty()))
                {
                    break;
                }
                count++;
            }
            return count;
        }

        public static float DefaultHeight(this SerializedProperty property, bool pad = false)
        {
            int count = 0;
            SerializedProperty childProperty = property.Copy();
            while (childProperty.NextVisible(true))
            {
                if (SerializedProperty.EqualContents(childProperty, property.GetEndProperty()))
                {
                    break;
                }
                count++;
            }

            if(pad)
            {
                return count * StandardVerticalHeight;
            }
            else
            {
                return (count * StandardVerticalHeight) - EditorGUIUtility.standardVerticalSpacing;
            }            
        }

        public static void Indent(this ref Rect rect)
        {
            rect.x += IndentPerLevel;
            rect.width -= IndentPerLevel;
        }

        public static void Outdent(this ref Rect rect)
        {
            rect.x += IndentPerLevel;
            rect.width -= IndentPerLevel;
        }

        public static void Indent(this ref Rect rect, int level)
        {
            rect.x += IndentPerLevel * level;
            rect.width -= IndentPerLevel * level;
        }

        public static void NewLine(this ref Rect rect)
        {
            rect.y += EditorGUIUtility.singleLineHeight;
        }

        public static void AddLines(this ref Rect rect, int count)
        {
            rect.y += EditorGUIUtility.singleLineHeight * count;
        }

        public static void Space(this ref Rect rect)
        {
            rect.y += EditorGUIUtility.standardVerticalSpacing;
        }

        public static void AddSpaces(this ref Rect rect, int count)
        {
            rect.y += EditorGUIUtility.standardVerticalSpacing * count;
        }

        public static Rect PropertyBlock(this ref Rect rect, SerializedProperty property, bool pad = false)
        {
            int childCount = CountChildProperties(property);
            float height = (EditorGUIUtility.singleLineHeight * childCount) + (EditorGUIUtility.standardVerticalSpacing * (childCount - 1));
            Rect ret = new Rect(rect.x, rect.y, rect.width, height);
            rect.y += height;
            if (pad)
            {
                rect.y += EditorGUIUtility.standardVerticalSpacing;
            }
            return ret;
        }

        public static Rect Property(this ref Rect rect, bool pad = false)
        {
            float height = EditorGUIUtility.singleLineHeight;
            Rect ret = new Rect(rect.x, rect.y, rect.width, height);
            rect.y += height;
            if (pad)
            {
                rect.y += EditorGUIUtility.standardVerticalSpacing;
            }
            return ret;
        }
    }
}
