using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace HackF5.UnitySpy.Gui.Services
{
    public enum PathRootKind
    {
        Type,
        Service,
        NetCache,
    }

    public sealed class PathSegment
    {
        public PathSegment(string value, bool isIndex)
        {
            this.Value = value;
            this.IsIndex = isIndex;
        }

        public string Value { get; }

        public bool IsIndex { get; }
    }

    public static class PathFormatter
    {
        public static string ToDisplay(IReadOnlyList<PathSegment> segments)
        {
            if (segments == null || segments.Count == 0)
            {
                return string.Empty;
            }

            return string.Join(".", segments.Select(s => s.Value));
        }

        public static string ToCSharp(PathRootKind rootKind, IReadOnlyList<PathSegment> segments)
        {
            if (segments == null || segments.Count == 0)
            {
                return string.Empty;
            }

            var builder = new StringBuilder();
            var start = 0;
            if (rootKind == PathRootKind.Service)
            {
                builder.Append("GetService(\"").Append(Escape(segments[0].Value)).Append("\")");
                start = 1;
            }
            else if (rootKind == PathRootKind.NetCache)
            {
                builder.Append("GetNetCacheService(\"").Append(Escape(segments[0].Value)).Append("\")");
                start = 1;
            }

            for (var i = start; i < segments.Count; i++)
            {
                var segment = segments[i];
                if (segment.IsIndex)
                {
                    builder.Append('[').Append(segment.Value).Append(']');
                }
                else
                {
                    builder.Append("[\"").Append(Escape(segment.Value)).Append("\"]");
                }
            }

            return builder.ToString();
        }

        public static IReadOnlyList<string> SplitDisplay(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                return new string[0];
            }

            return path.Split('.')
                .Select(p => p.Trim())
                .Where(p => p.Length > 0)
                .ToList();
        }

        public static PathSegment FromToken(string token)
        {
            return int.TryParse(token, NumberStyles.Integer, CultureInfo.InvariantCulture, out _)
                ? new PathSegment(token, true)
                : new PathSegment(token, false);
        }

        private static string Escape(string value) => (value ?? string.Empty).Replace("\\", "\\\\").Replace("\"", "\\\"");
    }
}
