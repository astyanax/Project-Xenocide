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

namespace ProjectXenocide.UI.Scenes.Geoscape
{
    /// <summary>
    /// Generate a spherical mesh of VertexPositionNormalTexture vertexes
    /// </summary>
    class SphereMesh
    {
        private int numStrips;
        private int nextIndex;
        private int nextVertex;
        private short[] triangleListIndices;
        private GlobeVertex[] vertexes;

        /// <summary>
        /// construtor
        /// <param name="numStrips">Number of strips we sphere has between pole and equator</param>
        /// </summary>
        public SphereMesh(int numStrips)
        {
            this.numStrips = numStrips;
            Debug.Assert(1 <= numStrips && numStrips < 20);

            AllocateStorage();
            ConstructMeshStructure();

            // check calculations
            Debug.Assert(TotalVertexes == nextVertex);
            Debug.Assert(TotalIndexes == nextIndex * 2);
        }

        /// <summary>
        /// return a vertex buffer that can be used to draw the sphere
        /// </summary>
        public VertexBuffer CreateVertexBuffer(GraphicsDevice device) 
        {
            VertexBuffer vertexBuffer =  new VertexBuffer(
                device,
                GlobeVertex.SizeInBytes * vertexes.Length,
                BufferUsage.None
                );

            vertexBuffer.SetData<GlobeVertex>(vertexes);
            return vertexBuffer;
        }

        /// <summary>
        /// return indexed triangle list that can be used to draw the sphere.
        /// </summary>
        public IndexBuffer CreateIndexBuffer(GraphicsDevice device) 
        {
            IndexBuffer indexBuffer = new IndexBuffer(
                device,
                sizeof(short) * triangleListIndices.Length,
                BufferUsage.None,
                IndexElementSize.SixteenBits
                );
            indexBuffer.SetData<short>(triangleListIndices);
            return indexBuffer;
        }

        /// <summary>
        /// Based on number of strips, calculate number space
        /// needed to store Vertexes and allocate it.  
        /// </summary>
        private void AllocateStorage()
        {
            nextIndex = 0;
            nextVertex = 0;

            int numFaces = 8 * numStrips * numStrips;
            triangleListIndices = new short[numFaces * 3];

            // nodes in a given strip = (4 x (n-1)) + 1
            // and there are 2n - 1 strips. So:
            int numVertexes = ((4 * (numStrips - 1)) + 10) * (numStrips - 1) + 7;
            vertexes = new GlobeVertex[numVertexes];
        }

        private void ConstructMeshStructure()
        {
            AddFirstMeshStrip();

            // first two vertexes in array are the poles, so first vertex of strip will be at [2]
            int previousStripStart = 2;
            for (int strip = 2; strip <= numStrips; ++strip)
            {
                previousStripStart = AddTriangleStrip(strip, previousStripStart);
            }
        }

        /// <summary>
        /// first strip requires special handling, as all vertexes attach to the pole
        /// </summary>
        private void AddFirstMeshStrip()
        {
            Vector3 position = new Vector3(0.0f, 1.0f, 0.0f);
            Vector3 tangent = Vector3.Cross(position, Vector3.UnitX);
            Vector3 binormal = Vector3.Cross(position, tangent);

            // add north (and south) pole vertex
            AddVertex(position, tangent, binormal, new Vector2(0.5f, 0.0f), false);

            // first two vertexes in array are the poles, so first vertex of strip will be at [2]
            int firstIndex = 2;

            // if we're on the equator, then vertexes are adjacent, otherwise they're alternating
            int stepSize = isEquator(1) ? 1 : 2;

            // now compute the Vertices along this strip
            int numVertex = CreateStripVertexes(1, isEquator(1));

            // construct the faces
            for (int i = 0; i < numVertex; ++i)
            {
                AddFace(0, firstIndex, firstIndex + stepSize);
                firstIndex += stepSize;
            }
        }

        /// <summary>
        /// create set of faces, between previous latitude and this one
        /// <param name="strip">Band on sphere we're rendering</param>
        /// <param name="previousStripVertex">Index into vertices to first vertex of pervious "strip"</param>
        /// </summary>
        private int AddTriangleStrip(int strip, int previousStripVertex)
        {
            // the vertices for this strip will start here
            int thisStripVertex = nextVertex;

            // create the strip
            CreateStripVertexes(strip, isEquator(strip));

            // if we're on the equator, then vertexes are adjacent, otherwise they're alternating
            // with their southern hemisphere counterpart
            int stepSize = isEquator(strip) ? 1 : 2;

            // now do the faces between the strips
            // construct the faces
            for (int i = 0; i < 4; ++i)
            {
                for (int j = 0; j < strip - 1; ++j)
                {
                    AddFace(previousStripVertex, thisStripVertex, thisStripVertex + stepSize);
                    thisStripVertex += stepSize;

                    AddFace(thisStripVertex, previousStripVertex + 2, previousStripVertex);
                    previousStripVertex += 2;
                }
                AddFace(previousStripVertex, thisStripVertex, thisStripVertex + stepSize);
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
        private int CreateStripVertexes(int strip, bool isOnEquator)
        {
            // we're going to cheat a bit and go from 0 to 90 degrees
            double longitude = (Math.PI * 0.5 * strip) / numStrips;
            float  y = (float)Math.Cos(longitude);
            double s = Math.Sin(longitude);

            Vector3 position;
            Vector3 tangent;
            Vector3 binormal;

            // number of vertexes in this strip
            int numVertexes = (strip * 4);
            for (int v = 0; v < numVertexes; ++v)
            {
                double latitudue = (Math.PI * 2.0 * v) / numVertexes;
                float  x = (float)(- s * Math.Cos(latitudue));
                float  z = (float)(s * Math.Sin(latitudue));

                position = new Vector3(x, y, z);
                tangent = Vector3.Cross(position, Vector3.UnitX);
                binormal = Vector3.Cross(position, tangent);

                AddVertex ( position,
                            tangent,
                            binormal,
                            new Vector2((float)(v) / numVertexes, (float)(strip) * 0.5f / numStrips),
                            isOnEquator);
            }

            position = new Vector3((float)-s, y, 0.0f);
            tangent = Vector3.Cross(position, Vector3.UnitX);
            binormal = Vector3.Cross(position, tangent);

            // and we need to add one extra one, to let texture wrap around
            AddVertex(position,
                       tangent,
                       binormal,
                       new Vector2(1.0f, (float)(strip) * 0.5f / numStrips),
                       isOnEquator);

            return numVertexes;
         }

        /// <summary>
        /// add the vectorIndexes making up a northern face (and it's southern complement) to the index
        /// </summary>
        private void AddFace(int vectorIndex1, int vectorIndex2, int vectorIndex3)
        {
            // northern face
            AddIndexIntoBuffer(vectorIndex1);
            AddIndexIntoBuffer(vectorIndex3);
            AddIndexIntoBuffer(vectorIndex2);
        }

        /// <summary>
        /// add vectorIndex to end of northern indexes we've added so far
        /// and add the matching southern hemisphere index as well
        /// and update count of number
        /// </summary>
        private void AddIndexIntoBuffer(int vectorIndex)
        {
            // as we need to add a southern face for every northen one
            // we must not fill more than half the index array with northern vertex indices.
            Debug.Assert(nextIndex < (triangleListIndices.Length / 2));


            // yes, I know that the vectorIndex should be a short, but due to the nature of
            // how it's called, its cleaner to put one cast in here than multiple elsewhere
            triangleListIndices[nextIndex] = (short)vectorIndex;

            // we put southern faces at end of list, in reverse order, because
            // vertex order needs to change (because faces are inverted)
            triangleListIndices[TotalIndexes - nextIndex - 1] = GetSouthernVertexIndex((short)vectorIndex);
            ++nextIndex;
        }

        /// <summary>
        /// return the index to where the matching vertex in southern hemisphere is
        /// </summary>
        private short GetSouthernVertexIndex(short index)
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
        private void AddVertex(Vector3 position, Vector3 tangent, Vector3 binormal, Vector2 texture, bool isOnEquator)
        {
            vertexes[nextVertex] = new GlobeVertex(position, position, tangent, binormal, texture);
            
            nextVertex++;
            if (!isOnEquator)
            {
                vertexes[nextVertex] = CreateSouthernVertex(position, tangent, texture);
                nextVertex++;
            }
        }

        /// <summary>
        /// returns a VertexPositionNormalTexture that is the southern hemisphere's complement
        /// to the supplied "Vertex"
        /// </summary>
        private static GlobeVertex CreateSouthernVertex(Vector3 position, Vector3 tangent, Vector2 texture)
        {
            Vector3 southPosition = new Vector3(position.X, -position.Y, position.Z);
            Vector3 southBinormal = Vector3.Cross(southPosition, tangent);
            Vector2 southTexture = new Vector2(texture.X, 1.0f - texture.Y);

            return new GlobeVertex(southPosition, southPosition, tangent, southBinormal, southTexture);
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
    }
}
