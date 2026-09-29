using System;
using System.Collections.Generic;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using ProjectXenocide.Model.Geoscape;

namespace ProjectXenocide.UI.Scenes
{
    /// <summary>
    /// Builds the line mesh for X-Corp craft patrol routes drawn on the globe:
    /// a great-circle polyline through the waypoints, plus a small cross marker
    /// at each waypoint.
    /// </summary>
    public sealed class WaypointRouteMeshBuilder : LineMeshBuilder
    {
        /// <summary>Routes to draw. Each route is an ordered list of waypoints.</summary>
        public IList<IReadOnlyList<GeoPosition>> Routes { get; set; }

        /// <summary>Globe radius used for the route (just above the surface).</summary>
        private const float RouteRadius = 1.01f;

        /// <summary>Number of line segments used to approximate each great-circle leg.</summary>
        private const int ArcSegments = 16;

        /// <summary>Half-size of a waypoint cross marker, in world units.</summary>
        private const float MarkerSize = 0.02f;

        private static readonly Color RouteColor = new Color(120, 220, 255);
        private static readonly Color MarkerColor = new Color(120, 255, 120);

        public override void Build(IList<VertexPositionColor> verts, IList<short> indexes)
        {
            if (Routes == null)
            {
                return;
            }

            foreach (var route in Routes)
            {
                if ((route == null) || (route.Count == 0))
                {
                    continue;
                }

                for (int i = 0; i + 1 < route.Count; ++i)
                {
                    AddArc(verts, indexes, route[i], route[i + 1]);
                }

                foreach (GeoPosition waypoint in route)
                {
                    AddMarker(verts, indexes, waypoint);
                }
            }
        }

        private static void AddArc(IList<VertexPositionColor> verts, IList<short> indexes,
            GeoPosition from, GeoPosition to)
        {
            float azimuth = from.GetAzimuth(to);
            float distance = from.Distance(to);
            if (distance <= 0.0001f)
            {
                return;
            }

            for (int i = 0; i < ArcSegments; ++i)
            {
                GeoPosition a = from.GetEndpoint(azimuth, distance * i / ArcSegments);
                GeoPosition b = from.GetEndpoint(azimuth, distance * (i + 1) / ArcSegments);
                AddSegment(verts, indexes, a.Cartesian * RouteRadius, b.Cartesian * RouteRadius, RouteColor);
            }
        }

        private static void AddMarker(IList<VertexPositionColor> verts, IList<short> indexes, GeoPosition position)
        {
            Vector3 normal = position.Cartesian;
            Vector3 tangent = Vector3.Cross(normal, Vector3.Up);
            if (tangent.LengthSquared() < 0.0001f)
            {
                tangent = Vector3.Cross(normal, Vector3.Right);
            }
            tangent.Normalize();
            Vector3 bitangent = Vector3.Cross(normal, tangent);

            Vector3 center = normal * RouteRadius;
            AddSegment(verts, indexes, center - (tangent * MarkerSize), center + (tangent * MarkerSize), MarkerColor);
            AddSegment(verts, indexes, center - (bitangent * MarkerSize), center + (bitangent * MarkerSize), MarkerColor);
        }

        private static void AddSegment(IList<VertexPositionColor> verts, IList<short> indexes,
            Vector3 a, Vector3 b, Color color)
        {
            short start = (short)verts.Count;
            verts.Add(new VertexPositionColor(a, color));
            verts.Add(new VertexPositionColor(b, color));
            indexes.Add(start);
            indexes.Add((short)(start + 1));
        }
    }
}
