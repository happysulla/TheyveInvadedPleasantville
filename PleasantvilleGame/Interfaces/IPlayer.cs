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
      bool IsComputer { set; get; }
   }
   public interface IPlayerTown : IPlayer
   {
      bool GetStartingTownsperson(IGameInstance gi, int die1);
   }
   public interface IPlayerAlien : IPlayer
   {
      ITerritory ZebulonLocation{ set; get; }
      bool ChooseStartingHqArea();
      bool GetStartingTownspeople(IGameInstance gi, int die1, int die2);
   }
}
