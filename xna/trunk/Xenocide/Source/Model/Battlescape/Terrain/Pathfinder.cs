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
* @file Pathfinder.cs
* @date Created: 2008/01/20
* @author File creator: David Teviotdale
* @author Credits: none
*/
#endregion

using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using System.Globalization;

using Microsoft.Xna.Framework;

namespace ProjectXenocide.Model.Battlescape
{
    /// <summary>
    /// The pathfinding functions associated with a terrain
    /// </summary>
    public partial class Pathfinder
    {
        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="terrain">The terrain to examine</param>
        public Pathfinder(Terrain terrain)
        {
            this.terrain = terrain;
        }

        /// <summary>
        /// Find a path from start to destination cell
        /// </summary>
        /// <param name="sx">location of starting cell</param>
        /// <param name="sy">location of starting cell</param>
        /// <param name="sz">location of starting cell</param>
        /// <param name="canFly">allowed to use flying to get to destination cell?</param>
        /// <param name="dx">location of destination cell</param>
        /// <param name="dy">location of destination cell</param>
        /// <param name="dz">location of destination cell</param>
        /// <param name="path">path from source to destination, or closest tile</param>
        /// <returns>true if a path to destination found</returns>
        public bool FindPath(int sx, int sy, int sz, bool canFly, int dx, int dy, int dz, IList<MoveData> path)
        {
            return FindPath(sx, sy, sz, canFly, dx, dy, dz, path, 1000000);
        }

        /// <summary>
        /// Find a path from start to destination cell
        /// </summary>
        /// <param name="sx">location of starting cell</param>
        /// <param name="sy">location of starting cell</param>
        /// <param name="sz">location of starting cell</param>
        /// <param name="canFly">allowed to use flying to get to destination cell?</param>
        /// <param name="dx">location of destination cell</param>
        /// <param name="dy">location of destination cell</param>
        /// <param name="dz">location of destination cell</param>
        /// <param name="path">path from source to destination, or closest tile</param>
        /// <param name="maxCost">path must cost less than this amount</param>
        /// <returns>true if a path to destination found</returns>
        public bool FindPath(int sx, int sy, int sz, bool canFly, int dx, int dy, int dz, IList<MoveData> path, int maxCost)
        {
            path.Clear();
            // basic error handling
            if (!terrain.IsOnTerrain(dx, dy, dz) || !terrain.IsOnTerrain(sx, sy, sz))
            {
                return false;
            }

            // setup for run
            MoveData start  = new MoveData(sx, sy, sz, sentinalCost);
            MoveData end    = new MoveData(dx, dy, dz, 0);
            bool found = false;
            ClearClosedList();
            openList.Clear();
            int index = terrain.CellIndex(sx, sy, sz);
            closedList[index] = start;
            openList.Add(start.Cost + Heuristic(start, dx, dy, dz), start);
            List<MoveData> neighbours = new List<MoveData>();

            // start processing cells on path
            while (!openList.IsEmpty)
            {
                MoveData active = openList.Pop();
                if (active == end)
                {
                    // we've found the path
                    found = true;

                    // collect nodes in the path (this is reading it in reverse order)
                    MoveData current = end;
                    path.Add(current);
                    while (current != start)
                    {
                        current = closedList[terrain.CellIndex(current.X, current.Y, current.Z)];
                        path.Add(current);
                    }

                    // reverse path
                    for (int i = 0; i < path.Count / 2; ++i)
                    {
                        MoveData temp = path[i];
                        path[i] = path[path.Count - 1 - i];
                        path[path.Count - 1 - i] = temp;
                    }
                    break;
                }

                if (maxCost < active.Cost)
                {
                    // we've failed
                    break;
                }

                // fetch and process neighbour cells
                terrain.ListAccessbleNeighbours(active.X, active.Y, active.Z, canFly, neighbours);
                for (int i = 0; i < neighbours.Count; ++i)
                {
                    MoveData node = neighbours[i];
                    index = terrain.CellIndex(node.X, node.Y, node.Z);

                    // calc total cost to travel to node
                    node.Cost += active.Cost;

                    if (0 == closedList[index].Cost)
                    {
                        // node isn't on open or closed list, so add to both lists
                        closedList[index] = new MoveData(active.X, active.Y, active.Z, node.Cost);
                        openList.Add(node.Cost + Heuristic(node, dx, dy, dz), node);
                    }
                    else if (node.Cost < closedList[index].Cost)
                    {
                        // we've found a cheaper way to this node, so update table
                        closedList[index] = new MoveData(active.X, active.Y, active.Z, node.Cost);
                        openList.Update(node.Cost + Heuristic(node, dx, dy, dz), node);
                    }
                }
            }

            return found;
        }

        /// <summary>
        /// Estimate cost of going from source to destination
        /// </summary>
        /// <param name="start">location of starting cell</param>
        /// <param name="dx">location of destination cell</param>
        /// <param name="dy">location of destination cell</param>
        /// <param name="dz">location of destination cell</param>
        /// <returns>estimated cost</returns>
        public static float Heuristic(MoveData start, int dx, int dy, int dz)
        {
            return ((Math.Abs(start.X - dx) + Math.Abs(start.Z - dz)) * Terrain.HorizontalMoveCost)
                + (Math.Abs(start.Y - dy) * Terrain.VerticalMoveCost);
        }

        /// <summary>Set closedList to initial (empty) state</summary>
        private void ClearClosedList()
        {
            if (null == closedList)
            {
                closedList = new MoveData[terrain.Length * terrain.Width * terrain.Levels];
            }
            else
            {
                Array.Clear(closedList, 0, closedList.Length);
            }
        }

        /// <summary>List of cells to examine</summary>
        private partial class OpenList
        {
            /// <summary>
            /// Add node to list, keeping list in sorted order
            /// </summary>
            /// <param name="cost">Actual cost + Heuristic</param>
            /// <param name="node">address of node in closedList</param>
            public void Add(float cost, MoveData node)
            {
                list.Add(new OpenListElement(cost, node));

                // bubblesort
                for(int i = list.Count - 1; 0 < i; --i)
                {
                    if (list[i].cost < list[i - 1].cost)
                    {
                        OpenListElement temp = list[i];
                        list[i] = list[i-1];
                        list[i - 1] = temp;
                    }
                    else
                    {
                        break;
                    }
                }
            }

            /// <summary>Empty the list</summary>
            public void Clear()
            {
                list.Clear();
            }

            /// <summary>true if list is empty</summary>
            public bool IsEmpty { get { return (0 == list.Count); } }

            /// <summary>Return topmost item in list</summary>
            public MoveData Pop()
            {
                MoveData temp = list[0].node;
                list.RemoveAt(0);
                return temp;
            }

            /// <summary>
            /// Look for node with this index.  If found, update cost
            /// </summary>
            /// <param name="cost">Actual cost + Heuristic</param>
            /// <param name="node">address of node in closedList</param>
            public void Update(float cost, MoveData node)
            {
                for (int i = 0; i < list.Count - 1; ++i)
                {
                    if (list[i].node == node)
                    {
                        list[i] = new OpenListElement(cost, node);
                        break;
                    }
                }
            }

            /// <summary>
            /// The entries in the closed list
            /// </summary>
            private struct OpenListElement
            {
                public OpenListElement(float cost, MoveData node)
                {
                    this.cost  = cost;
                    this.node = node;
                }

                /// <summary>Actual cost + Heuristic</summary>
                public float cost;

                /// <summary>address of node in closedList for rest of details</summary>
                public MoveData node;

                public override string ToString()
                {
                    return String.Format(CultureInfo.InvariantCulture, "totalcost={0}, node=[{1}]", cost, node);
                }
            }

            #region Fields

            /// <summary>the cells we've examined, with their "move to cost"</summary>
            private List<OpenListElement> list = new List<OpenListElement>();

            #endregion
        }

        #region Fields

        /// <summary>tags the starting cell</summary>
        private const int sentinalCost = 1;

        /// <summary>the cells we've examined, with their "move to cost"</summary>
        private MoveData[] closedList;

        /// <summary>List of cells to examine</summary>
        private OpenList openList = new OpenList();

        /// <summary>The terrain to examine</summary>
        private Terrain terrain;

        #endregion
    }
}
