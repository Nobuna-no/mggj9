using UnityEngine;
using NobunAtelier;
using UnityEngine.Splines;

public class SplineDefinition : DataDefinition
{
    [SerializeField] private Spline m_spline;
    public Spline Spline => m_spline;

    // If we have a spline that do weird things,
    // need to manually edit the spline package to allows to reset the length...
    //private void OnValidate()
    //{
    //    m_spline.SetDirtyNoNotify();
    //}
}