using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;

namespace GeoBingo.GameModes.CaptureChallenge;

internal static class CaptureChallengeVotingOrderFactory
{
    public static IReadOnlyList<Guid> CreateOrder(
        Guid roundId,
        Guid votingOrderSeed,
        IReadOnlyList<CaptureChallengeCapture> releasedCaptures)
    {
        if (roundId == Guid.Empty)
        {
            throw new ArgumentException(
                "The round identifier cannot be empty.",
                nameof(roundId));
        }

        if (votingOrderSeed == Guid.Empty)
        {
            throw new ArgumentException(
                "The voting-order seed cannot be empty.",
                nameof(votingOrderSeed));
        }

        ArgumentNullException.ThrowIfNull(releasedCaptures);
        if (releasedCaptures.Any(
                capture =>
                    capture.CaptureId == Guid.Empty
                    || capture.RoundId != roundId
                    || capture.OwnerUserId == Guid.Empty
                    || !capture.ReleasedForVoting)
            || releasedCaptures
                .Select(capture => capture.CaptureId)
                .Distinct()
                .Count() != releasedCaptures.Count)
        {
            throw new ArgumentException(
                "Released captures must be valid, unique, and belong to the active round.",
                nameof(releasedCaptures));
        }

        return releasedCaptures
            .Select(
                capture => new OrderedCapture(
                    capture.CaptureId,
                    CreateOrderKey(
                        votingOrderSeed,
                        capture.CaptureId)))
            .OrderBy(
                entry => entry.OrderKey,
                LexicographicByteArrayComparer.Instance)
            .ThenBy(entry => entry.CaptureId)
            .Select(entry => entry.CaptureId)
            .ToArray();
    }

    private static byte[] CreateOrderKey(
        Guid votingOrderSeed,
        Guid captureId)
    {
        Span<byte> source = stackalloc byte[32];
        _ = votingOrderSeed.TryWriteBytes(source[..16]);
        _ = captureId.TryWriteBytes(source[16..]);
        return SHA256.HashData(source);
    }

    private sealed record OrderedCapture(
        Guid CaptureId,
        byte[] OrderKey);

    private sealed class LexicographicByteArrayComparer
        : IComparer<byte[]>
    {
        public static LexicographicByteArrayComparer Instance { get; } =
            new();

        public int Compare(byte[]? left, byte[]? right)
        {
            if (ReferenceEquals(left, right))
            {
                return 0;
            }

            if (left is null)
            {
                return -1;
            }

            if (right is null)
            {
                return 1;
            }

            return left.AsSpan().SequenceCompareTo(right);
        }
    }
}
