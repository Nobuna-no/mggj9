using UnityEngine;
using NobunAtelier;

[CreateAssetMenu(fileName ="[AugmentTier]", menuName = "SIMULACRA/Collection/AugmentTier")]
public class AugmentTierCollection : DataCollection<AugmentTierDefinition>
{
    public AugmentTierDefinition GetRandomBonusTier()
    {
        float totalProbability = 0f;
        foreach (var tier in Definitions)
        {
            totalProbability += tier.Probability;
        }

        float randomValue = Random.Range(0f, totalProbability);

        // Pick the bonus tier based on the random value
        float cumulativeProbability = 0f;
        foreach (var tier in Definitions)
        {
            cumulativeProbability += tier.Probability;
            if (randomValue <= cumulativeProbability)
            {
                return tier;
            }
        }

        Debug.LogError($"No tier has been picked - cumulativeProbability is over {cumulativeProbability}!!!");
        return null;
    }
}