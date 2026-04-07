using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Button = System.Windows.Controls.Button;

namespace PleasantvilleGame
{
   [Serializable]
   class MapItemTakeover : IMapItemTakeover
   {
      public IMapItem Alien { get; set; }
      public IMapItem Uncontrolled { get; set; }
      public String Observations { get; set; } = "";
      //----------------------------------------------------------------------
      public MapItemTakeover(IMapItem alien, IMapItem uncontrolled)
      {
         Alien = alien;
         Uncontrolled = uncontrolled;
      }
      //----------------------------------------------------------------------
      public override String ToString()
      {
         StringBuilder sb = new StringBuilder("Alien=<");
         sb.Append(Alien.Name);
         sb.Append("> Victum=<");
         sb.Append(Uncontrolled.Name);
         sb.Append("> Territory=<");
         sb.Append(Alien.TerritoryCurrent.ToString());
         sb.Append(">");
         return sb.ToString();
      }
   }
}
