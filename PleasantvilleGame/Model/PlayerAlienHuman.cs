using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PleasantvilleGame
{
   public class PlayerAlienHuman : Player, IPlayerAlien
   {
      public ITerritory ZebulonLocation{ set; get; } = new Territory();
      //---------------------------------------------------------------
      public PlayerAlienHuman() : base(true)
      {

      }
      //===============================================================
      public bool ChooseStartingHqArea()
      {
         return true;
      }
      public bool GetStartingTownspeople(IGameInstance gi, int die1, int die2)
      {
         return true;
      }
   }
}
