using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Splines;

/// <summary>
/// Store the random data waiting for later use (the player to open the chest...).
/// </summary>
[RequireComponent(typeof(SplineAnimate))]
public class CrystalAugment : MonoBehaviour
{
    public SplineAnimate Animator { get; private set; }
    private AugmentDefinition m_augmentDefinition;
    private AugmentTierDefinition m_tierDefinition;
    [SerializeField] private UnityEvent m_onCrystalOpening;
    [SerializeField] private bool m_destroyAfterOpening = true;
    [SerializeField] private float m_destructionDelay = 1f;

    public AugmentDefinition Augment => m_augmentDefinition;
    public AugmentTierDefinition Tier => m_tierDefinition;

    public void SetData(AugmentDefinition augment, AugmentTierDefinition augmentTier)
    {
        m_augmentDefinition = augment;
        m_tierDefinition = augmentTier;
        Animator.Loop = SplineAnimate.LoopMode.Loop;
    }

    public void Open(SplineContainer openingPath, float openingDuration)
    {
        Animator.Container = openingPath;
        Animator.StartOffset = 0;
        Animator.Duration = openingDuration;
        Animator.Loop = SplineAnimate.LoopMode.Once;
        Animator.Easing = SplineAnimate.EasingMode.EaseOut;
        Animator.Restart(true);
        StartCoroutine(CrystalOpeningRoutine(openingDuration));
    }

    private IEnumerator CrystalOpeningRoutine(float delay)
    {
        yield return new WaitForSeconds(delay);
        m_onCrystalOpening?.Invoke();
        yield return new WaitForSeconds(m_destructionDelay);
        Destroy(this.gameObject);
    }

    private void Awake()
    {
        Animator = GetComponent<SplineAnimate>();
    }
}
