using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PleasantvilleGame
{
   public interface IPlayer
   {
      String[] StartingTownspeople { get; set; }
      List<string> BlockedRandomMoves { set; get; } // a townsperson name who is blocked by owner from moving in random movement
      bool IsComputer { set; get; }
      bool GetNextState(IGameInstance gi);
   }
   public interface IPlayerTown : IPlayer
   {
      bool GetStartingTownsperson(IGameInstance gi, int die1);
   }
   public interface IPlayerAlien : IPlayer
   {
      ITerritory ZebulonLocation{ set; get; }
      bool ChooseStartingHqArea();
      bool GetStartingAlien(IGameInstance gi);
   }
}
