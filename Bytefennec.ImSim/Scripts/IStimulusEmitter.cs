using UnityEngine;

namespace Bytefennec.ImSim
{

public interface IStimulusEmitter
{
    public Transform Transform {get;}
    public bool CanEmit {get; set;}
    public StimulusType StimulusType {get; set;}
    public float Radius {get; set;}
    public float Intensity {get; set;}
}

}