using System;
using UnityEngine;

public static class ChainGenerator
{
    public static GameObject GenerateChain(
        int chainLength,
        float segmentSpacing,
        string prefabName,
        GameObject chainRoot,
        GameObject chainSegmentA,
        GameObject chainSegmentB,
        GameObject chainHook = null)
    {
        if (chainLength < 1)
            throw new ArgumentOutOfRangeException(
                nameof(chainLength),
                "Chain length must be greater than zero.");

        if (segmentSpacing <= 0)
            throw new ArgumentOutOfRangeException(
                nameof(segmentSpacing),
                "Segment spacing must be greater than zero.");

        if (string.IsNullOrWhiteSpace(prefabName))
            throw new ArgumentException(
                "Prefab name cannot be empty.",
                nameof(prefabName));

        if (chainRoot == null)
            throw new ArgumentNullException(nameof(chainRoot));

        if (chainSegmentA == null)
            throw new ArgumentNullException(nameof(chainSegmentA));

        if (chainSegmentB == null)
            throw new ArgumentNullException(nameof(chainSegmentB));

        ValidateRootPrefab(chainRoot);
        ValidateSegmentPrefab(chainSegmentA);
        ValidateSegmentPrefab(chainSegmentB);

        if (chainHook != null)
            ValidateSegmentPrefab(chainHook);

        GameObject newChain = new GameObject(prefabName);

        GameObject root = UnityEngine.Object.Instantiate(
            chainRoot,
            newChain.transform);

        root.name = "Chain Root";
        root.transform.localPosition = Vector3.zero;

        Rigidbody2D previousLink =
            root.GetComponentInChildren<Rigidbody2D>(true);

        for (int i = 0; i < chainLength; i++)
        {
            GameObject segmentPrefab =
                i % 2 == 0 ? chainSegmentB : chainSegmentA;

            GameObject newSegment = UnityEngine.Object.Instantiate(
                segmentPrefab,
                newChain.transform);

            newSegment.name = $"Chain Segment {i + 1}";

            newSegment.transform.localPosition =
                root.transform.localPosition
                + Vector3.down * ((i + 1) * segmentSpacing);

            HingeJoint2D hingeJoint =
                newSegment.GetComponentInChildren<HingeJoint2D>(true);
            Rigidbody2D currentLink =
                newSegment.GetComponentInChildren<Rigidbody2D>(true);

            hingeJoint.connectedBody = previousLink;
            previousLink = currentLink;
        }

        if (chainHook != null)
        {
            GameObject newHook = UnityEngine.Object.Instantiate(
                chainHook,
                newChain.transform);

            newHook.name = "Chain Hook";

            newHook.transform.localPosition =
                root.transform.localPosition
                + Vector3.down * ((chainLength + 1) * segmentSpacing);

            HingeJoint2D hookHingeJoint =
                newHook.GetComponentInChildren<HingeJoint2D>(true);

            hookHingeJoint.connectedBody = previousLink;
        }

        return newChain;
    }

    private static void ValidateRootPrefab(GameObject prefab)
    {
        if (prefab.GetComponentInChildren<Rigidbody2D>(true) == null)
            throw new InvalidOperationException(
                $"The chain root prefab '{prefab.name}' must contain a Rigidbody2D.");
    }

    private static void ValidateSegmentPrefab(GameObject prefab)
    {
        if (prefab.GetComponentInChildren<HingeJoint2D>(true) == null
            || prefab.GetComponentInChildren<Rigidbody2D>(true) == null)
        {
            throw new InvalidOperationException(
                $"The chain segment prefab '{prefab.name}' must contain "
                + "a HingeJoint2D and a Rigidbody2D.");
        }
    }
}
