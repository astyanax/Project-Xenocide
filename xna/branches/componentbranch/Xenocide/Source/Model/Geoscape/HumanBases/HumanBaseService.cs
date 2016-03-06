using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;

namespace Xenocide.Model.Geoscape.HumanBases
{
    interface IHumanBaseService
    {
        void AddHumanBase(float longitude, float latitude, String name);
        IList<HumanBase> HumanBases { get; set; }
    }

    public class HumanBaseService : IHumanBaseService
    {
        private IList<HumanBase> humanBases;

        public HumanBaseService(Game game)
        {
            game.Services.AddService(typeof(IHumanBaseService), this);
        }

        #region IHumanBaseService Member

        /// <summary>
        /// Add a new base to the list of bases
        /// </summary>
        /// <param name="longitude">longitude of the base (in radians)</param>
        /// <param name="latitude">latitude of the base (in radians)</param>
        /// <param name="name">Name for the base</param>
        public void AddHumanBase(float longitude, float latitude, String name)
        {
            GeoPosition position = new GeoPosition(longitude, latitude);
            HumanBase humanBase = new HumanBase(position, name);
            humanBases.Add(humanBase);
        }

        public IList<HumanBase> HumanBases
        {
            get
            {
                return humanBases;
            }
            set
            {
                humanBases = value;
            }
        }

        #endregion
    }
}
