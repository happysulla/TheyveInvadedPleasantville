using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PleasantvilleGame
{
   public class PlayerAlienComputer : Player, IPlayerAlien
   {
      public ITerritory ZebulonLocation{ set; get; } = new Territory();
      //---------------------------------------------------------------
      public PlayerAlienComputer() : base(true)
      {

      }
      //===============================================================
      public bool ChooseStartingHqArea()
      {
         return true;
      }
   }
}
