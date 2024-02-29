using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Splines;

[RequireComponent(typeof(SplineAnimate))]
public class CrystalAmalgam : MonoBehaviour
{
    SplineAnimate m_splineAnimate;
    // Start is called before the first frame update
    void Awake()
    {
        m_splineAnimate = GetComponent<SplineAnimate>();
        m_splineAnimate.Container = GetComponentInParent<SplineContainer>();
        m_splineAnimate.Play();
    }

    // Update is called once per frame
    void Update()
    {

    }
}
