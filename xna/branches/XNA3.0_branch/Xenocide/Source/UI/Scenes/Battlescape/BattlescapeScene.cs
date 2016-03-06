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
* @file BattlescapeScene.cs
* @date Created: 2008/01/01
* @author File creator: David Teviotdale
* @author Credits: none
*/
#endregion

#region Using Statements

using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;

using ProjectXenocide.Model;
using ProjectXenocide.Model.Battlescape;
using ProjectXenocide.Model.Battlescape.Combatants;
using ProjectXenocide.UI.Scenes.Common;
using ProjectXenocide.UI.Scenes.Battlescape;

#endregion

namespace ProjectXenocide.UI.Scenes.Battlescape
{
    /// <summary>
    /// This is the 3D scene that appears on the Battlescape screen
    /// </summary>
    public class BattlescapeScene : IDisposable
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public BattlescapeScene()
        {
        }

        /// <summary>
        /// Implement IDisposable
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Implement IDisposable
        /// </summary>
        /// <param name="disposing">false when called from a finalizer</param>
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (basicEffect != null)
                {
                    basicEffect.Dispose();
                    basicEffect = null;
                }
                if (content != null)
                {
                    content.Dispose();
                    content = null;
                }
                if (terrainMesh != null)
                {
                    terrainMesh.Dispose();
                    terrainMesh = null;
                }
                if (cellCursor != null)
                {
                    cellCursor.Dispose();
                    cellCursor = null;
                }
                if (pathLine != null)
                {
                    pathLine.Dispose();
                    pathLine = null;
                }
            }
        }

        /// <summary>
        /// Load the graphic content of the scene
        /// </summary>
        /// <param name="device">device to render to</param>
        
        /// <param name="scape">the battlescape</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:ValidateArgumentsOfPublicMethods",
            Justification="Will throw if scape is null")]
        public void LoadContent(GraphicsDevice device, Battle scape)
        {
            
            {
                battlescape = scape;
                basicEffect = new BasicEffect(device, null);
                pathLine.LoadContent(device, pathBuilder);
                cellCursor.LoadContent(device, new CellCursorLineMeshBuilder(wantedCellCursorColor));
                terrainMesh.LoadContent(device, battlescape.Terrain);
                combatantMeshes.LoadContent(content, battlescape);
                projectileMesh.LoadContent(content);
                facility.LoadContent(content);
            }
        }

        /// <summary>
        /// Perform processing which updates the scene
        /// </summary>
        /// <param name="gameTime">snapshot of timing values</param>
        public void Update(GameTime gameTime)
        {
            camera.Update(gameTime);
        }

        /// <summary>
        /// Render the scene to the display
        /// </summary>
        /// <param name="device">device to render to</param>
        /// <param name="sceneWindow">child window to render scene to</param>
        /// <param name="topLevel">topmost level of terrain to draw</param>
        /// <param name="cursorPosition">cell of terrian the cursor is over</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:ValidateArgumentsOfPublicMethods",
            Justification="will throw if device is null")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2233:OperationsShouldNotOverflow",
            MessageId = "topLevel*8", Justification="value won't overflow")]
        public void Draw(GraphicsDevice device, CeGui.Rect sceneWindow, int topLevel, Vector3 cursorPosition)
        {
            // we're only interested in cell it's over
            cursorPosition = RoundToCell(cursorPosition);

            // only draw in area we've been told to
            Viewport oldview = device.Viewport;
            device.Viewport = CalcViewportForSceneWindow(sceneWindow, device.Viewport);

            SetupBasicEffect(device);

            // draw suggested path for selected unit
            pathLine.ConfigureEffect(basicEffect);
            DrawPath(device);

            // Draw Terrain
            terrainMesh.ConfigureEffect(basicEffect);
            basicEffect.VertexColorEnabled = false;
            terrainMesh.Draw(device, basicEffect, topLevel);

            // facility.Draw(basicEffect);

            // Draw combatants
            DrawCombatants();

            // Draw projectile (if there is one)
            if (null != battlescape.Trajectory)
            {
                projectileMesh.Draw(battlescape.Trajectory, basicEffect);
            }

            // Draw Cell Cursor
            //... Calculate World transform to put the cell Cursor at the right place
            cellCursor.ConfigureEffect(basicEffect);
            DrawCellCursor(device, topLevel, cursorPosition);

            // ToDo: Draw everything else

            // restore viewport
            device.Viewport = oldview;
        }

        /// <summary>
        /// Convert a position in the viewport to cell location on battlescape
        /// </summary>
        /// <param name="coords">The position in the viewport (in relative co-ords)</param>
        /// <param name="level">level of the battlescape to find intersection at</param>
        /// <returns>corresponding position, or undefined if not on Battlescape</returns>
        public Vector3 WindowToBattlescapeCell(System.Drawing.PointF coords, int level)
        {
            // convert screen position to point on near clipping plane in view space
            float x = TanViewAngle * (coords.X - 0.5f) * aspectRatio;
            float y = TanViewAngle * (0.5f - coords.Y);
            Vector3 viewSpace = new Vector3(x, y, -1);

            // convert vector into worldspace
            Vector3 direction = Vector3.TransformNormal(viewSpace, Matrix.Invert(camera.View));
            direction.Normalize();

            // if we're looking upwards, there is no intercept
            if (-0.01 < direction.Y)
            {
                return new Vector3(float.NaN, float.NaN, float.NaN);
            }

            // and project to level
            Vector3 intercept = camera.Position;
            float scale = ((level * LevelHeight) - intercept.Y) / direction.Y;
            intercept += (direction * scale);
            intercept.Y = level;
            return intercept;
        }

        /// <summary>
        /// Move the camera
        /// </summary>
        /// <param name="delta">relative displacement</param>
        public void MoveCamera(Vector3 delta)
        {
            camera.Position += delta;
        }

        /// <summary>
        /// Convert a position in cell co-ordinates to "world" co-ordinates
        /// </summary>
        /// <param name="v">position in "Cell" co-ordiantes</param>
        /// <returns>position in "world" co-ordinates</returns>
        public static Vector3 CellToWorld(Vector3 v)
        {
            // Basically, each level of battlescape is 2.5 units high
            return new Vector3(v.X, v.Y * LevelHeight, v.Z);
        }

        /// <summary>
        /// Put basic effect into correct state to render the scene
        /// </summary>
        /// <param name="device">device to render to</param>
        private void SetupBasicEffect(GraphicsDevice device)
        {
            basicEffect.View  = camera.View;
            basicEffect.World = Matrix.Identity;
            basicEffect.DiffuseColor = new Vector3(1.0f, 1.0f, 1.0f);
            basicEffect.Projection   = Matrix.CreatePerspectiveFieldOfView(viewAngle, aspectRatio, 1.0f, 100.0f);
            device.RenderState.CullMode = CullMode.CullCounterClockwiseFace;
            device.RenderState.DepthBufferEnable = true;
            device.RenderState.DepthBufferWriteEnable = true;
        }

        /// <summary>
        /// convert Window's co-ordinates to viewport co-ordinates
        /// </summary>
        /// <param name="windowCoords">Window co-ords to translate</param>
        /// <param name="viewport">The current viewport</param>
        /// <returns>Viewport co-ordinates</returns>
        private Viewport CalcViewportForSceneWindow(CeGui.Rect windowCoords, Viewport viewport)
        {
            int fullHeight = viewport.Height;
            int fullWidth = viewport.Width;
            viewport.X = (int)(fullWidth * windowCoords.Left);
            viewport.Y = (int)(fullHeight * windowCoords.Top);
            viewport.Width = (int)(fullWidth * windowCoords.Width);
            viewport.Height = (int)(fullHeight * windowCoords.Height);

            // compute the aspect ratio while we're about it
            aspectRatio = (float)viewport.Width / (float)viewport.Height;

            return viewport;
        }

        /// <summary>
        /// Convert vector's components into integers (give us a cell index)
        /// </summary>
        /// <param name="v">vector to process</param>
        /// <returns>Rounded vecor</returns>
        public static Vector3 RoundToCell(Vector3 v)
        {
            return new Vector3((float)Math.Truncate(v.X), (float)Math.Round(v.Y), (float)Math.Truncate(v.Z));
        }

        /// <summary>
        /// Draw the combatants
        /// </summary>
        private void DrawCombatants()
        {
            foreach (Team team in battlescape.Teams)
            {
                foreach (Combatant combatant in team.Combatants)
                {
                    if (InViewingFustrum(combatant.Position))
                    {
                        bool visible = IsVisibleToXCorp(combatant);
                        if (visible || Xenocide.StaticTables.StartSettings.Cheats.ShowAllAliens)
                        {
                            combatantMeshes.Draw(combatant, basicEffect, visible);
                        }
                    }
                }
            }
        }

        /// <summary>Is combatant currently visible to the X-Corp forces?</summary>
        /// <param name="combatant">to check</param>
        /// <returns>true if visible</returns>
        private static bool IsVisibleToXCorp(Combatant combatant)
        {
            return (combatant.TeamId != Team.Aliens) || (0 != combatant.OponentsViewing);
        }

        /// <summary>Checks if a cell is within the volume being drawn on the display</summary>
        /// <param name="position">position of cell, in cell co-ords</param>
        /// <returns>true if cell and it's contents should be drawn on screen</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "position",
            Justification = "ToDo: function needs implementation")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic",
           Justification = "ToDo: function needs implementation")]
        private bool InViewingFustrum(Vector3 position)
        {
            //ToDo: implement
            return true;
        }

        /// <summary>Draw selected combatant's proposed path on battlescape</summary>
        /// <param name="device">device to render to</param>
        private void DrawPath(GraphicsDevice device)
        {
            if (showPath)
            {
                if (pathBuilder.Dirty)
                {
                    pathLine.BuildMesh(device, pathBuilder);
                }
                pathLine.Draw(device, basicEffect);
            }
        }

        /// <summary>Draw selected combatant's proposed path on battlescape</summary>
        /// <param name="device">device to render to</param>
        /// <param name="topLevel">topmost level of terrain to draw</param>
        /// <param name="cursorPosition">cell of terrian the cursor is over</param>
        private void DrawCellCursor(GraphicsDevice device, int topLevel, Vector3 cursorPosition)
        {
            if (wantedCellCursorColor != currentCellCursorColor)
            {
                cellCursor.BuildMesh(device, new CellCursorLineMeshBuilder(wantedCellCursorColor));
                currentCellCursorColor = wantedCellCursorColor;
            }
            basicEffect.World = Matrix.CreateTranslation(CellToWorld(cursorPosition));
            cellCursor.Draw(device, basicEffect, 12 + (topLevel * 8));
        }

        #region Fields

        /// <summary>Each "cell" of the battlescape is 2.5 units high (and 1 unit wide and long)</summary>
        public const float LevelHeight = 2.5f;

        /// <summary>Draw a Path on the screen?</summary>
        public bool ShowPath { get { return showPath; } set { showPath = value; } }

        /// <summary>Show pathfinder output</summary>
        public PathMeshBuilder PathMeshBuilder { get { return pathBuilder; } }

        /// <summary>Color we want to draw the CellCursor with</summary>
        public Color WantedCellCursorColor { get { return wantedCellCursorColor; } set { wantedCellCursorColor = value; } }

        /// <summary>Bullet (if any) in flight</summary>
        public ProjectileMesh ProjectileMesh { get { return projectileMesh; } }

        /// <summary>Aspect ratio of the window showing this scene</summary>
        private float aspectRatio;

        /// <summary>Used to define projection matrix</summary>
        private const float viewAngle = MathHelper.PiOver4;

        /// <summary>Used to convert from Screen space to view space, for picking</summary>
        private static readonly float TanViewAngle = (float)(Math.Tan(viewAngle * 0.5) * 2.0);

        /// <summary>Used to render the scene</summary>
        private BasicEffect basicEffect;

        /// <summary>The terrain</summary>
        private TerrainMesh terrainMesh = new TerrainMesh();

        /// <summary>Camera used to view the scene</summary>
        private Camera camera = new Camera();

        /// <summary>Show pathfinder output</summary>
        private LineMesh pathLine = new LineMesh();

        /// <summary>Show pathfinder output</summary>
        private PathMeshBuilder pathBuilder = new PathMeshBuilder();

        /// <summary>Draw a Path on the screen?</summary>
        private bool showPath;

        /// <summary>Simple cube, marking the cell the mouse cursor is currently over</summary>
        private LineMesh cellCursor = new LineMesh();

        /// <summary>The battlescape</summary>
        private Battle battlescape;

        /// <summary>The 3D models for the combatants</summary>
        private CombatantMeshes combatantMeshes = new CombatantMeshes();

        /// <summary>Will want to unload the content when we close this screen</summary>
        private ContentManager content = new ContentManager(Xenocide.Instance.Services);

        /// <summary>Color we want to draw the CellCursor with</summary>
        private Color wantedCellCursorColor = Color.Blue;

        /// <summary>Color we the CellCursor is currently set up as</summary>
        private Color currentCellCursorColor = Color.Blue;

        /// <summary>Bullet (if any) in flight</summary>
        private ProjectileMesh projectileMesh = new ProjectileMesh();

        /// <summary>3D model of X-Corp facility to draw on battlescape</summary>
        private FacilityMesh facility = new FacilityMesh();

        #endregion Fields
    }
}
