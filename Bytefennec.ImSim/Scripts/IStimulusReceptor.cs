using UnityEngine;

namespace Bytefennec.ImSim
{

public interface IStimulusReceptor
{
    public Transform Transform {get;}

    public void ReceiveStimulus(IStimulusEmitter emitter);
}

}