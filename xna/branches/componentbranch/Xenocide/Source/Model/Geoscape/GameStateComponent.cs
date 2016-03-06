using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;

namespace Xenocide.Model.Geoscape
{
    [Serializable]
    public class GameStateComponent : IDisposable
    {
        [NonSerialized]
        private Game game;

        public Game Game
        {
            get
            {
                return game;
            }
            set
            {
                game = value;
                OnGameSet();
            }
        }

        protected virtual void OnGameSet()
        {

        }

        #region IDisposable Member

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public virtual void Dispose(bool disposing)
        {
        }

        #endregion
    }
}
