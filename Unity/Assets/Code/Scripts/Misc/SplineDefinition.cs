using UnityEngine;
using NobunAtelier;
using UnityEngine.Splines;

public class SplineDefinition : DataDefinition
{
    [SerializeField] private Spline m_spline;
    public Spline Spline => m_spline;

    private void OnValidate()
    {
        m_spline.SetDirtyNoNotify();
    }
}