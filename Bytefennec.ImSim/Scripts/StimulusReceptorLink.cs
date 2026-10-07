#nullable enable
using System;
using System.Reflection;
using UnityEngine;

namespace Bytefennec.ImSim
{

[Serializable]
public class StimulusReceptorLink
{
    private static readonly Type[] PARAM_TYPES = {typeof(IStimulusEmitter)};
    private const BindingFlags BIND_FLAGS = BindingFlags.Instance 
        | BindingFlags.Public 
        | BindingFlags.NonPublic
        | BindingFlags.DeclaredOnly;

    #pragma warning disable CS0414
    [SerializeField] private GameObject? _targetObject = null;
    #pragma warning restore CS0414
    [SerializeField] private Component? _targetComponent = null;
    [SerializeField] private string _targetMethod = string.Empty;
    [NonSerialized] private Action<IStimulusEmitter>? _method = null;

    public void Bind()
    {
        _method = null;
        if(_targetComponent == null || string.IsNullOrWhiteSpace(_targetMethod))
        {
            return;
        }

        MethodInfo? method = FindMethod(_targetComponent.GetType(), _targetMethod);
        if(method == null)
        {
            #if UNITY_EDITOR
            Debug.LogWarning($"\"{_targetMethod}\" not found on \"{_targetComponent.GetType().Name}\"", _targetComponent);
            #endif

            return;
        }

        _method = (Action<IStimulusEmitter>)Delegate.CreateDelegate(typeof(Action<IStimulusEmitter>), _targetComponent, method);
    }

    /// <returns>The method or null</returns>
    private static MethodInfo? FindMethod(Type startType, string methodName)
    {
        for(Type type = startType; type != null; type = type.BaseType)
        {
            MethodInfo methodInfo = type.GetMethod(methodName, BIND_FLAGS, null, PARAM_TYPES, null);
            if(methodInfo != null)
            {
                return methodInfo;
            }
        }

        return null;
    }

    public void Invoke(IStimulusEmitter emitter)
    {
        if(_method == null || _targetComponent == null)
        {
            return;
        }

        _method(emitter);
    }
}

}