using Unity.Scripting.LifecycleManagement;
using UnityEngine.LowLevel;
using UnityEngine.PlayerLoop;
using UnityEngine;
using System.Collections.Generic;
using Unity.Profiling;

namespace Bytefennec.ImSim
{

/// <summary>
/// Type idenfitier
/// </summary>
public class ImSimUpdate
{
}

public static partial class ImSimSystem
{
    #if UNITY_EDITOR
    [AutoStaticsCleanup] private static readonly ProfilerMarker PROFILE_STIM_MARKER = new("ImSim Stims");
    #endif
    [AutoStaticsCleanup] private static float _accumulator = 0.0f;
    [AutoStaticsCleanup] private static readonly List<IStimulusEmitter> _emitters = new();
    [AutoStaticsCleanup] private static readonly List<IStimulusReceptor> _receptors = new();

    [RuntimeInitializeOnLoadMethod]
    private static void AppStart()
    {
        PlayerLoopSystem defaultLoop = PlayerLoop.GetDefaultPlayerLoop();

        PlayerLoopSystem imSimLoop = new()
        {
            subSystemList = null,
            updateDelegate = ImSimUpdate,
            type = typeof(ImSimUpdate)
        };

        PlayerLoopSystem modifiedLoop = InsertSystemAfter<FixedUpdate>(in defaultLoop, imSimLoop);
        PlayerLoop.SetPlayerLoop(modifiedLoop);
    }

    private static PlayerLoopSystem InsertSystemBefore<T>(in PlayerLoopSystem loopSystem, PlayerLoopSystem newSystem) where T : struct
    {
        PlayerLoopSystem newLoopSystem = new()
        {
            loopConditionFunction = loopSystem.loopConditionFunction,
            type = loopSystem.type,
            updateDelegate = loopSystem.updateDelegate,
            updateFunction = loopSystem.updateFunction,
        };

        List<PlayerLoopSystem> newSubSystems = new();
        if(loopSystem.subSystemList != null)
        {
            for(int i = 0; i < loopSystem.subSystemList.Length; i++)
            {
                if(loopSystem.subSystemList[i].type == typeof(T))
                {
                    newSubSystems.Add(newSystem);
                }

                newSubSystems.Add(loopSystem.subSystemList[i]);
            }
        }

        newLoopSystem.subSystemList = newSubSystems.ToArray();
        return newLoopSystem;
    }

    private static PlayerLoopSystem InsertSystemAfter<T>(in PlayerLoopSystem loopSystem, PlayerLoopSystem newSystem) where T : struct
    {
        PlayerLoopSystem newLoopSystem = new()
        {
            loopConditionFunction = loopSystem.loopConditionFunction,
            type = loopSystem.type,
            updateDelegate = loopSystem.updateDelegate,
            updateFunction = loopSystem.updateFunction,
        };

        List<PlayerLoopSystem> newSubSystems = new();
        if(loopSystem.subSystemList != null)
        {
            for(int i = 0; i < loopSystem.subSystemList.Length; i++)
            {
                newSubSystems.Add(loopSystem.subSystemList[i]);
                if(loopSystem.subSystemList[i].type == typeof(T))
                {
                    newSubSystems.Add(newSystem);
                }
            }
        }

        newLoopSystem.subSystemList = newSubSystems.ToArray();
        return newLoopSystem;
    }

    public static void RegisterEmitter(IStimulusEmitter emitter)
    {
        _emitters.Add(emitter);
    }

    public static void UnregisterEmitter(IStimulusEmitter emitter)
    {
        _emitters.Remove(emitter);
    }

    public static void RegisterReceptor(IStimulusReceptor receptor)
    {
        _receptors.Add(receptor);
    }

    public static void UnregisterReceptor(IStimulusReceptor receptor)
    {
        _receptors.Remove(receptor);
    }

    private static void ImSimUpdate()
    {
        _accumulator += Time.deltaTime;
        float step = Time.fixedDeltaTime;
        while(_accumulator >= step)
        {
            #if UNITY_EDITOR
            PROFILE_STIM_MARKER.Begin();
            #endif
            StimulusUpdate();
            #if UNITY_EDITOR
            PROFILE_STIM_MARKER.End();
            #endif
            _accumulator -= step;
        }
    }

    private static void StimulusUpdate()
    {
        for(int i = 0; i < _emitters.Count; i++)
        {
            IStimulusEmitter emitter = _emitters[i];
            if(!_emitters[i].CanEmit)
            {
                continue;
            }

            for(int j = 0; j < _receptors.Count; j++)
            {
                IStimulusReceptor receptor = _receptors[j];
                if((emitter.Transform.position - receptor.Transform.position).sqrMagnitude > emitter.Radius * emitter.Radius)
                {
                    continue;
                }

                receptor.ReceiveStimulus(emitter);
            }

            emitter.CanEmit = false;
        }
    }
}

}