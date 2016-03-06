#region Copyright
/*
--------------------------------------------------------------------------------
This source file is part of Xenocide
  by  Project Xenocide Team

For the latest info on Xenocide, see http://www.projectxenocide.com/

This work is licensed under the Creative Commons
Attribution-NonCommercial-ShareAlike 2.5 License.

To view a copy of this license, visit
http://creativecommons.org/licenses/by-nc-sa/2.5/
or send a letter to Creative Commons, 543 Howard Street, 5th Floor,
San Francisco, California, 94105, USA.
--------------------------------------------------------------------------------
*/

/*
* @file SphereMesh.cs
* @date Created: 2007/01/25
* @author File creator: dteviot
* @author Credits: none
*/
#endregion

using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using System.Collections;
using System.Diagnostics;

namespace Xenocide.UI.Scenes.Geoscape
{
    /// <summary>
    /// Generate a spherical mesh of VertexPositionNormalTexture vertexes
    /// </summary>
    class SphereMesh
    {
        /// <summary>
        /// construtor
        /// <param name="numStrips">Number of strips we sphere has between pole and equator</param>
        /// </summary>
        public SphereMesh(int numStrips)
        {
            this.numStrips = numStrips;
            Debug.Assert(1 <= numStrips && numStrips < 20);

            allocateStorage();
            constructMesh();

            // check calculations
            Debug.Assert(TotalVertexes == nextVertex);
            Debug.Assert(TotalIndexes == nextIndex * 2);
        }

        /// <summary>
        /// return a vertex buffer that can be used to draw the sphere
        /// </summary>
        public VertexBuffer getVertexBuffer(GraphicsDevice device) 
        {
            VertexBuffer vertexBuffer =  new VertexBuffer(
                device,
                VertexPositionNormalTexture.SizeInBytes * vertexes.Length,
                ResourceUsage.None,
                ResourceManagementMode.Automatic
                );
            vertexBuffer.SetData<VertexPositionNormalTexture>(vertexes);
            return vertexBuffer;
        }

        /// <summary>
        /// return indexed triangle list that can be used to draw the sphere.
        /// </summary>
        public IndexBuffer getIndexBuffer(GraphicsDevice device) 
        {
            IndexBuffer indexBuffer = new IndexBuffer(
                device,
                sizeof(short) * triangleListIndices.Length,
                ResourceUsage.None,
                ResourceManagementMode.Automatic,
                IndexElementSize.SixteenBits
                );
            indexBuffer.SetData<short>(triangleListIndices);
            return indexBuffer;
        }

        /// <summary>
        /// Based on number of strips, calculate number space
        /// needed to store Vertexes and allocate it.  
        /// </summary>
        private void allocateStorage()
        {
            nextIndex = 0;
            nextVertex = 0;

            int numFaces = 8 * numStrips * numStrips;
            triangleListIndices = new short[numFaces * 3];

            // nodes in a given strip = (4 x (n-1)) + 1
            // and there are 2n - 1 strips. So:
            int numVertexes = ((4 * (numStrips - 1)) + 10) * (numStrips - 1) + 7;
            vertexes = new VertexPositionNormalTexture[numVertexes];
        }

        private void constructMesh()
        {
            addFirstStrip();

            // first two vertexes in array are the poles, so first vertex of strip will be at [2]
            int previousStripStart = 2;
            for (int strip = 2; strip <= numStrips; ++strip)
            {
                previousStripStart = addStrip(strip, previousStripStart);
            }
        }

        /// <summary>
        /// first strip requires special handling, as all vertexes attach to the pole
        /// </summary>
        private void addFirstStrip()
        {
            // add north (and south) pole vertex
            addVertex(new Vector3(0.0f, 1.0f, 0.0f), new Vector2(0.5f, 0.0f), false);

            // first two vertexes in array are the poles, so first vertex of strip will be at [2]
            int firstIndex = 2;

            // if we're on the equator, then vertexes are adjacent, otherwise they're alternating
            int stepSize = isEquator(1) ? 1 : 2;

            // now compute the Vertices along this strip
            int numVertex = computeVertexes(1, isEquator(1));

            // construct the faces
            for (int i = 0; i < numVertex; ++i)
            {
                addFace(0, firstIndex, firstIndex + stepSize);
                firstIndex += stepSize;
            }
        }

        /// <summary>
        /// create set of faces, between previous latitude and this one
        /// <param name="strip">Band on sphere we're rendering</param>
        /// <param name="previousStripVertex">Index into vertices to first vertex of pervious "strip"</param>
        /// </summary>
        private int addStrip(int strip, int previousStripVertex)
        {
            // the vertices for this strip will start here
            int thisStripVertex = nextVertex;

            // create the strip
            computeVertexes(strip, isEquator(strip));

            // if we're on the equator, then vertexes are adjacent, otherwise they're alternating
            // with their southern hemisphere counterpart
            int stepSize = isEquator(strip) ? 1 : 2;

            // now do the faces between the strips
            // construct the faces
            for (int i = 0; i < 4; ++i)
            {
                for (int j = 0; j < strip - 1; ++j)
                {
                    addFace(previousStripVertex, thisStripVertex, thisStripVertex + stepSize);
                    thisStripVertex += stepSize;

                    addFace(thisStripVertex, previousStripVertex + 2, previousStripVertex);
                    previousStripVertex += 2;
                }
                addFace(previousStripVertex, thisStripVertex, thisStripVertex + stepSize);
                thisStripVertex += stepSize;
            }

            // return pointer to first vectex in this strip
            return previousStripVertex + 2;
        }

        /// <summary>
        /// compute the vertexes in this strip, and load into the array
        /// <param name="strip">Band on sphere we're rendering</param>
        /// <param name="isOnEquator">Does this band touch the equator?</param>
        /// <returns>number of vertexes added</returns>
        /// </summary>
        private int computeVertexes(int strip, bool isOnEquator)
        {
            // we're going to cheat a bit and go from 0 to 90 degrees
            double longitude = (Math.PI * 0.5 * strip) / numStrips;
            float  y = (float)Math.Cos(longitude);
            double s = Math.Sin(longitude);

            // number of vertexes in this strip
            int numVertexes = (strip * 4);
            for (int v = 0; v < numVertexes; ++v)
            {
                double latitudue = (Math.PI * 2.0 * v) / numVertexes;
                float  x = (float)(- s * Math.Cos(latitudue));
                float  z = (float)(s * Math.Sin(latitudue));

                Vector3 vertex = new Vector3(x, y, z);
                addVertex(vertex, 
                    new Vector2((float)(v) / numVertexes, (float)(strip) * 0.5f / numStrips),
                    isOnEquator);
            }

            // and we need to add one extra one, to let texture wrap around
            addVertex(new Vector3((float)-s, y, 0.0f),
                new Vector2(1.0f, (float)(strip) * 0.5f / numStrips),
                isOnEquator);
            return numVertexes;
         }

        /// <summary>
        /// add the vectorIndexes making up a northern face (and it's southern complement) to the index
        /// </summary>
        private void addFace(int vectorIndex1, int vectorIndex2, int vectorIndex3)
        {
            // northern face
            addIndex(vectorIndex1);
            addIndex(vectorIndex2);
            addIndex(vectorIndex3);
        }

        /// <summary>
        /// add vectorIndex to end of northern indexes we've added so far
        /// and add the matching southern hemisphere index as well
        /// and update count of number
        /// </summary>
        private void addIndex(int vectorIndex)
        {
            // as we need to add a southern face for every northen one
            // we must not fill more than half the index array with northern vertex indices.
            Debug.Assert(nextIndex < (triangleListIndices.Length / 2));


            // yes, I know that the vectorIndex should be a short, but due to the nature of
            // how it's called, its cleaner to put one cast in here than multiple elsewhere
            triangleListIndices[nextIndex] = (short)vectorIndex;

            // we put southern faces at end of list, in reverse order, because
            // vertex order needs to change (because faces are inverted)
            triangleListIndices[TotalIndexes - nextIndex - 1] = getSouthernVertexIndex((short)vectorIndex);
            ++nextIndex;
        }

        /// <summary>
        /// return the index to where the matching vertex in southern hemisphere is
        /// </summary>
        private short getSouthernVertexIndex(short index)
        {
            // if we're on the equator then vertex is it's own complement
            // otherwise, it's the next vertex in the array
            if (index < TotalVertexes - ((4 * numStrips) + 1))
            {
                ++index;
            }
            return index;
        }

        /// <summary>
        /// add VertexPositionNormalTexture to end of vertexes we've calculated so far
        /// note that vertexes will be loaded into array as pairs, with the matching
        /// southern hemisphere immediately after the northern one.
        /// </summary>
        private void addVertex(Vector3 position, Vector2 texture, bool isOnEquator)
        {
            vertexes[nextVertex] = new VertexPositionNormalTexture(position, position, texture);
            ++nextVertex;
            if (!isOnEquator)
            {
                vertexes[nextVertex] = createSouthernVertex(position, texture);
                ++nextVertex;
            }
        }

        /// <summary>
        /// returns a VertexPositionNormalTexture that is the southern hemisphere's complement
        /// to the supplied "Vertex"
        /// </summary>
        private static VertexPositionNormalTexture createSouthernVertex(Vector3 position, Vector2 texture)
        {
            Vector3 southPosition = new Vector3(position.X, -position.Y, position.Z);
            Vector2 southTexture = new Vector2(texture.X, 1.0f - texture.Y);
            return new VertexPositionNormalTexture(southPosition, southPosition, southTexture);
        }

        /// <summary>
        /// <returns>true if this strip is the "equator"</returns>
        /// </summary>
        private bool isEquator(int strip)
        {
            return (strip == numStrips);
        }


        /*
        /// <summary>
        /// Diagnostic output, dump the list of vertices
        /// </summary>
        private void dumpVertexes()
        {
            foreach (VertexPositionNormalTexture v in vertexes)
            {
                Debug.WriteLine(v.ToString());
            }
        }

        /// <summary>
        /// Diagnostic output, dump the vertex indexes making up the faces
        /// </summary>
        private void dumpFaces()
        {
            for (int i = 0; i < TotalIndexes; i += 3)
            {
                Debug.WriteLine(
                    String.Format("( {0}, {1}, {2} )",
                        triangleListIndices[i],
                        triangleListIndices[i+1],
                        triangleListIndices[i+2]
                        ) );
            }
        }
        */

        public int TotalVertexes { get { return vertexes.Length; } }
        public int TotalIndexes { get { return triangleListIndices.Length; } }
        public int TotalFaces { get { return TotalIndexes / 3; } }

        private int numStrips;
        private int nextIndex;
        private int nextVertex;
        private short[] triangleListIndices;
        private VertexPositionNormalTexture[] vertexes;
    }
}
