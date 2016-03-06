using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;

namespace Xenocide.Model.Geoscape
{
    public interface IGeoTimeService
    {
        GeoTime GeoTime { set; }
        string ToString();
        /// <summary>
        /// Stop the progress of game time
        /// </summary>
        void StopTime();
        DateTime Time { get; }
        float TimeRatio { get; set; }
    }

    public class GeoTimeService : GameComponent, IGeoTimeService
    {
        private GeoTime geoTime;

        public GeoTimeService(Game game)
            : base(game)
        {
            Game.Services.AddService(typeof(IGeoTimeService), this);
        }

        public override void Update(GameTime gameTime)
        {
            if (geoTime != null)
                geoTime.Update(gameTime);
        }

        /// <summary>
        /// Delegate definition of callbacks that are called each "gameday"
        /// </summary>
        public delegate void DayPassedHandler();

        /// <summary>
        /// Delegate for "gameday" callbacks
        /// </summary>
        public DayPassedHandler DayPassed;

        #region IGeoTimeService Member


        public GeoTime GeoTime
        {
            set
            { 
                geoTime = value; 
            }
        }

        public string ToString()
        {
            if (geoTime != null)
                return geoTime.ToString();
            else
                return "<NOTIME>";
        }

        /// <summary>
        /// Stop the progress of game time
        /// </summary>
        public void StopTime()
        {
            geoTime.StopTime();
        }

        public DateTime Time
        {
            get
            {
                return geoTime.Time;
            }
        }

        public float TimeRatio 
        {
            get
            {
                return geoTime.TimeRatio;
            }
            set
            {
                geoTime.TimeRatio = value;
            }
        }
        

        #endregion
    }
}
