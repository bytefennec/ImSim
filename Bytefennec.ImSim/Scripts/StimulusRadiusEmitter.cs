using UnityEngine;

namespace Bytefennec.ImSim
{

public class StimulusRadiusEmitter : MonoBehaviour, IStimulusEmitter
{
    /// <summary>
    /// Type of stimulus emitted.
    /// </summary>
    [Tooltip("Type of stimulus emitted")]
    [SerializeField] private StimulusType _stimulusType;
    /// <summary>
    /// Radius of stimulus.
    /// </summary>
    [Tooltip("Radius of stimulus")]
    [SerializeField] private float _radius = 10.0f;
    /// <summary>
    /// Intensity of stimulus.
    /// </summary>
    [Tooltip("Intensity of stimulus")]
    [SerializeField] private float _intensity = 1.0f;
    /// <summary>
    /// If <c>true</c> stimulus is automatically emitted, otherwise <c>false</c>.
    /// </summary>
    [Tooltip("Is stimulus automatically emitted?")]
    public bool AutoEmit = true;
    /// <summary>
    /// Delay between automatic emission.
    /// </summary>
    [Tooltip("Delay between automatic emission")]
    public float AutoEmitTime = 0.0f;
    private float _emitTime = 0.0f;
    private bool _canEmit = false;

    public Transform Transform => transform;
    public bool CanEmit
    {
        get => _canEmit;
        set
        {
            _canEmit = value;
        }
    }
    public StimulusType StimulusType
    {
        get => _stimulusType;
        set
        {
            _stimulusType = value;
        }
    }
    public float Radius
    {
        get => _radius;
        set
        {
            _radius = value;
        }
    }
    public float Intensity
    {
        get => _intensity;
        set
        {
            _intensity = value;
        }
    }

    private void OnEnable()
    {
        ImSimSystem.RegisterEmitter(this);
    }

    private void OnDisable()
    {
        ImSimSystem.UnregisterEmitter(this);
    }

    private void FixedUpdate()
    {
        if(!AutoEmit || Time.time < _emitTime)
        {
            return;
        }

        MarkForEmission();
        _emitTime = Time.time + AutoEmitTime;
    }

    /// <summary>
    /// Mark this stimulus for emission, so that it is emitted the next time the system processes over stims.
    /// </summary>
    public void MarkForEmission()
    {
        _canEmit = true;
    }
}

}