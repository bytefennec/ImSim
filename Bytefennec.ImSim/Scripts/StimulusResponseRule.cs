using UnityEngine.Events;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Bytefennec.ImSim
{

[Serializable]
public struct StimulusResponseRule
{
    /// <summary>
    /// Type of stimulus to look out for.
    /// </summary>
    [Tooltip("Type of stimulus to look out for")]
    public StimulusType StimulusType;
    /// <summary>
    /// Threshold for the intensity required before any link is invoked.
    /// </summary>
    [Tooltip("Threshold for the intensity required before any link is invoked")]
    public float Threshold;
    /// <summary>
    /// Links invoked by this rule.
    /// </summary>
    [Tooltip("Links invoked by this rule")]
    public List<StimulusReceptorLink> Links;
}

}