using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PleasantvilleGame
{
   public abstract class PlayerBase : IPlayer
   {
      public String[] StartingTownspeople { get; set; } = new String[2];
      public List<string> BlockedRandomMoves { set; get; } = new List<string>(); // a townsperson name who is blocked by owner from moving in random movement
      public bool IsComputer { set; get; } = false;
      public PlayerBase(bool isComputer)
      {
         IsComputer = isComputer;
      }
      //===============================================================
      public virtual bool GetNextState(IGameInstance gi)
      {
         return true;
      }
   }
}
