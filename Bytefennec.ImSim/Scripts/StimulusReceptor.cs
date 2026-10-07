using System.Collections.Generic;
using UnityEngine;

namespace Bytefennec.ImSim
{

public class StimulusReceptor : MonoBehaviour, IStimulusReceptor
{
    [SerializeField] private StimulusResponseRule[] _rules;
    public Transform Transform => transform;

    private void OnEnable()
    {
        ImSimSystem.RegisterReceptor(this);
    }

    private void OnDisable()
    {
        ImSimSystem.UnregisterReceptor(this);
    }

    private void Awake()
    {
        for(int i = 0; i < _rules.Length; i++)
        {
            List<StimulusReceptorLink> links = _rules[i].Links;
            for(int j = 0; j < links.Count; j++)
            {
                links[j].Bind();
            }
        }
    }

    public void ReceiveStimulus(IStimulusEmitter emitter)
    {
        for(int i = 0; i < _rules.Length; i++)
        {
            StimulusResponseRule rule = _rules[i];
            if(rule.StimulusType != emitter.StimulusType)
            {
                continue;
            }

            if(rule.Threshold < emitter.Intensity)
            {
                continue;
            }

            for(int j = 0; j < rule.Links.Count; j++)
            {
                rule.Links[j].Invoke(emitter);
            }
        }
    }
}

}