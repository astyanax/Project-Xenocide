using System.Collections.Generic;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace ProjectXenocide.UI.Scenes.Facility
{
    /// <summary>
    /// Builds colored lines connecting the placement ghost to adjacent facilities.
    /// Green lines indicate connected neighbors; red lines indicate overlapping or
    /// invalid neighbors.
    /// </summary>
    internal sealed class AdjacencyLines : LineMeshBuilder
    {
        /// <summary>
        /// The endpoints of adjacency lines to draw.
        /// </summary>
        private readonly List<AdjacencyLine> lines = new List<AdjacencyLine>();

        /// <summary>
        /// Add an adjacency line.
        /// </summary>
        /// <param name="from">Start point of the line</param>
        /// <param name="to">End point of the line</param>
        /// <param name="color">Color of the line</param>
        public void AddLine(Vector3 from, Vector3 to, Color color)
        {
            lines.Add(new AdjacencyLine(from, to, color));
        }

        /// <summary>
        /// Clear all lines (call before rebuilding for a new position).
        /// </summary>
        public void Clear()
        {
            lines.Clear();
        }

        /// <summary>
        /// Build the line mesh from the current list of adjacency lines.
        /// </summary>
        /// <param name="verts">the endpoints of the lines</param>
        /// <param name="indexes">order to draw lines</param>
        public override void Build(IList<VertexPositionColor> verts, IList<short> indexes)
        {
            short line = -1;
            foreach (AdjacencyLine al in lines)
            {
                verts.Add(new VertexPositionColor(al.From, al.Color));
                verts.Add(new VertexPositionColor(al.To, al.Color));
                indexes.Add(++line);
                indexes.Add(++line);
            }
        }

        /// <summary>
        /// A single adjacency line with endpoints and color.
        /// </summary>
        private struct AdjacencyLine
        {
            public Vector3 From;
            public Vector3 To;
            public Color Color;

            public AdjacencyLine(Vector3 from, Vector3 to, Color color)
            {
                From = from;
                To = to;
                Color = color;
            }
        }
    }
}
