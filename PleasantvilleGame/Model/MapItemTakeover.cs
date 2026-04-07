using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using PleasantvilleRemote;

namespace PleasantvilleGame
{
    [Serializable]
    class MapItemTakeover : IMapItemTakeover
    {
        #region Properties
        private IMapItem myAlien = null;
        public IMapItem Alien
        {
            get { return myAlien; }
            set { myAlien = value; }
        }
        private IMapItem myUncontrolled = null;
        public IMapItem Uncontrolled
        {
            get { return myUncontrolled; }
            set { myUncontrolled = value; }
        }
        private String myObservations = "";
        public  String Observations
        {
            get { return myObservations; }
            set { myObservations = value; }
        }
        #endregion

        #region Constructor
        public MapItemTakeover()
        {
        }
        public MapItemTakeover(IMapItem alien, IMapItem uncontrolled)
        {
            myAlien = alien;
            myUncontrolled = uncontrolled;
        }
        public MapItemTakeover(IMapItemTakeover takeover)
        {
            myAlien = new MapItem (takeover.Alien);
            myUncontrolled = new MapItem (takeover.Uncontrolled);
            myObservations = takeover.Observations;  
        }
        #endregion

        #region ToString()
        public override String ToString()
        {
            StringBuilder sb = new StringBuilder("Alien=<");
            sb.Append(myAlien.Name);
            sb.Append("> Victum=<");
            sb.Append(myUncontrolled.Name);
            sb.Append("> Territory=<");
            sb.Append(myAlien.Territory.ToString());
            sb.Append(">");
            return sb.ToString();
        }
        #endregion
    }
}
