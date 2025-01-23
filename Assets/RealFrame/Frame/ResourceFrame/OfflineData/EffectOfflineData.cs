using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EffectOfflineData : OfflineData
{
    public ParticleSystem[] m_AllParticle;
    public TrailRenderer[] m_AllTrailRenderer;

    public override void ResetProp()
    {
        base.ResetProp();
        foreach (ParticleSystem particle in m_AllParticle)
        {
            particle.Clear();
            particle.Play();
        }

        foreach (TrailRenderer trail in m_AllTrailRenderer)
        {
            trail.Clear();
        }
    }

    public override void BindData()
    {
        base.BindData();
        m_AllParticle = GetComponentsInChildren<ParticleSystem>(true);
        m_AllTrailRenderer = GetComponentsInChildren<TrailRenderer>(true);
    }
}
