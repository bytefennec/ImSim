#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace Bytefennec.ImSim
{

[CustomPropertyDrawer(typeof(StimulusReceptorLink))]
public class StimulusReceptorLinkDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        SerializedProperty objectProperty = property.FindPropertyRelative("_targetObject");
        SerializedProperty componentProperty = property.FindPropertyRelative("_targetComponent");
        SerializedProperty methodProperty = property.FindPropertyRelative("_targetMethod");

        EditorGUI.BeginProperty(position, label, property);
        float halfWidth = (position.width - 5.0f) * 0.5f;
        Rect leftRect = new(position.x, position.y, halfWidth, position.height);
        Rect rightRect = new(leftRect.xMax + 5.0f, position.y, halfWidth, position.height);

        EditorGUI.BeginChangeCheck();
        EditorGUI.PropertyField(leftRect, objectProperty, GUIContent.none);
        if(EditorGUI.EndChangeCheck())
        {
            componentProperty.objectReferenceValue = null;
            methodProperty.stringValue = string.Empty;
        }

        GameObject targetObject = objectProperty.hasMultipleDifferentValues
            ? null
            : objectProperty.objectReferenceValue as GameObject;
        if(targetObject != null)
        {
            Component targetComponent = componentProperty.objectReferenceValue as Component;
            string text = targetComponent != null && !string.IsNullOrWhiteSpace(methodProperty.stringValue)
                ? $"{targetComponent.GetType().Name}.{methodProperty.stringValue}"
                : "Select method..";

            if(EditorGUI.DropdownButton(rightRect, new GUIContent(text), FocusType.Keyboard))
            {
                ShowMenu(rightRect, targetObject, componentProperty, methodProperty);
            }
        }
        else
        {
            using(new EditorGUI.DisabledScope(true))
            EditorGUI.DropdownButton(rightRect,
                new GUIContent(objectProperty.hasMultipleDifferentValues
                    ? "-"
                    : "No GameObject selected"),
                FocusType.Passive
            );
        }

        EditorGUI.EndProperty();
    }

    private static void ShowMenu(Rect rect, GameObject gameObject, SerializedProperty componentProperty, SerializedProperty methodProperty)
    {
        SerializedObject componentSerializedObject = componentProperty.serializedObject;
        string componentPath = componentProperty.propertyPath;
        string methodPath = methodProperty.propertyPath;
        Component currentComponent = componentProperty.objectReferenceValue as Component;
        string currentMethod = methodProperty.stringValue;

        GenericMenu menu = new();
        bool hasAnyMethod = false;
        Dictionary<Type, int> seenTypes = new();
        TypeCache.MethodCollection methodCandidates = TypeCache.GetMethodsWithAttribute<ImSimTargetAttribute>();
        foreach(Component component in gameObject.GetComponents<Component>())
        {
            if(component == null)
            {
                continue;
            }

            Type type = component.GetType();
            seenTypes.TryGetValue(type, out int seenCount);
            seenTypes[type] = seenCount + 1;
            
            string typeLabel = seenCount == 0
                ? type.Name
                : $"{type.Name} ({seenCount + 1})";
            foreach(MethodInfo methodInfo in methodCandidates)
            {
                if(!methodInfo.DeclaringType.IsAssignableFrom(type) || !StimulusReceptorMethodHasValidSignature(methodInfo))
                {
                    continue;
                }

                hasAnyMethod = true;
                string methodName = methodInfo.Name;
                bool selected = component == currentComponent && methodName == currentMethod;
                menu.AddItem(new GUIContent($"{typeLabel}/{methodName}"), selected, () =>
                {
                    componentSerializedObject.Update();
                    componentSerializedObject.FindProperty(componentPath).objectReferenceValue = component;
                    componentSerializedObject.FindProperty(methodPath).stringValue = methodName;
                    componentSerializedObject.ApplyModifiedProperties();
                });
            }
        }

        if(!hasAnyMethod)
        {
            menu.AddDisabledItem(new GUIContent("No [StimulusReceptorTarget] methods found"));
        }

        menu.DropDown(rect);
    }

    private static bool StimulusReceptorMethodHasValidSignature(MethodInfo methodInfo)
    {
        if(methodInfo.IsStatic || methodInfo.ReturnType != typeof(void))
        {
            return false;
        }

        ParameterInfo[] parameterInfo = methodInfo.GetParameters();
        return parameterInfo.Length == 1 && parameterInfo[0].ParameterType == typeof(IStimulusEmitter);
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        return EditorGUIUtility.singleLineHeight;
    }
}

}
#endif