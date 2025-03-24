using System.Linq;
using DG.DemiEditor;
using DG.Tweening;
using UnityEditor;
using UnityEngine;

namespace Dott.Editor
{
    [CustomPropertyDrawer(typeof(DOTweenAnimation))]
    public class DOTweenAnimationPropertyDrawer : DOTweenComponentPropertyDrawer<DOTweenAnimation>
    {
        protected override string GetId(DOTweenAnimation component) => component.id;
    }

    [CustomPropertyDrawer(typeof(DOTweenCallback))]
    public class DOTweenCallbackPropertyDrawer : DOTweenComponentPropertyDrawer<DOTweenCallback>
    {
        protected override string GetId(DOTweenCallback component) => component.id;
    }

    public abstract class DOTweenComponentPropertyDrawer<T> : PropertyDrawer where T : MonoBehaviour
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var selected = property.objectReferenceValue as T;
            if (selected == null)
            {
                DrawDefault(position, property, label);
                return;
            }

            var components = selected.GetComponents<T>();
            if (components.Length == 1)
            {
                DrawDefault(position, property, label);
                return;
            }

            EditorGUI.BeginProperty(position, label, property);

            var controlRect = EditorGUI.PrefixLabel(position, label);
            var halfWidth = controlRect.width / 2;

            var popupRect = controlRect.SetWidth(halfWidth);
            var index = DrawIdPopup(popupRect, components, selected);
            if (index >= 0)
            {
                property.objectReferenceValue = components[index];
            }

            var selectedRect = controlRect.ShiftX(halfWidth);
            EditorGUI.PropertyField(selectedRect, property, GUIContent.none);

            EditorGUI.EndProperty();

            if (GUI.changed)
            {
                property.serializedObject.ApplyModifiedProperties();
            }
        }

        private int DrawIdPopup(Rect popupRect, T[] options, T selected)
        {
            var ids = options.Select((component, i) => $"{i}: {GetId(component)}").ToArray();
            var index = options.IndexOf(selected);
            index = EditorGUI.Popup(popupRect, index, ids);
            return index;
        }

        protected abstract string GetId(T component);

        private static void DrawDefault(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);
            EditorGUI.PropertyField(position, property, label);
            EditorGUI.EndProperty();
        }
    }
}