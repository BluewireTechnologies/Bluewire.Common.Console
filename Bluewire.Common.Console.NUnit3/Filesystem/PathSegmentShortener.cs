using System;
using System.Diagnostics;

namespace Bluewire.Common.Console.NUnit3.Filesystem
{
    public class PathSegmentShortener
    {
        public struct Result
        {
            public string OriginalSegment { get; set; }
            public string ShortenedSegment { get; set; }
            public string HashSuffix { get; set; }
        }

        public string ShortenDottedSegment(string segment, int limit)
        {
            var result = GetShorteningResult(segment, limit);
            if (result.ShortenedSegment == null) return result.OriginalSegment;
            return result.ShortenedSegment + result.HashSuffix;
        }

        public Result GetShorteningResult(string segment, int limit)
        {
            // Minimum 'length is 1 character, plus 4 hex digits of shortened hash.
            Debug.Assert(limit > 5);
            if (segment.Length <= limit) return new Result { OriginalSegment = segment };

            // We want to keep a prefix, in order to retain the expected approximate sort order. But we don't
            // want to just truncate since that risks obliterating entire test names. So we're going to assume
            // that our path segment takes the form /\w+(.\w+)+/ and truncate each bit individually.
            var parts = segment.Split('.');

            var charCountToRemove = segment.Length - limit + 4;
            var totalDiscardableLength = segment.Length - parts.Length - parts.Length + 1; // One char per part, plus separators.
            if (totalDiscardableLength <= 0) return new Result { OriginalSegment = segment };
            var fractionToRemove = (float)charCountToRemove / totalDiscardableLength;

            for (var i = 0; i < parts.Length; i++)
            {
                var discardable = parts[i].Length - 1;
                var targetLength = Math.Floor((1 - fractionToRemove) * discardable) + 1;
                parts[i] = parts[i].Substring(0, (int)Math.Max(1, targetLength));
            }
            var shortenedParts = String.Join(".", parts);
            // If shortened plus short hash is longer than the input, return the input.
            if (shortenedParts.Length + 4 > segment.Length) return new Result { OriginalSegment = segment };

            return new Result {
                OriginalSegment = segment,
                ShortenedSegment = shortenedParts,
                HashSuffix = GetShortHashHex(segment)
            };
        }

        public string GetShortHashHex(string segment)
        {
            // Hashcode only needs to remain static within process lifetime, so we don't care if GetHashCode's
            // implementation changes in the future:
            return $"{segment.GetHashCode() & 0xFFFF:X4}";
        }
    }
}
