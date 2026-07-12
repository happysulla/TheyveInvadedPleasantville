using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PleasantvilleGame
{
   public interface IPlayer
   {
      bool IsComputer { set; get; }
      bool GetNextState(IGameInstance gi, ref GameAction action);
   }
   public interface IPlayerTown : IPlayer
   {
      bool GetStartingTownCounter(IGameInstance gi, int die1);
      bool BlockRandomMoves(IGameInstance gi);
      bool PerformTownMove(IGameInstance gi, ref GameAction outAction);
   }
   public interface IPlayerAlien : IPlayer
   {
      ITerritory ZebulonLocation{ set; get; }
      abstract bool ChooseStartingHqArea();
      bool GetStartingAlienCounters(IGameInstance gi);   
      bool BlockRandomMoves(IGameInstance gi);
      bool PerformAlienMoves(IGameInstance gi);
      bool PerformAlienTakeover(IGameInstance gi, IMapItems aliens, IMapItems possibleVictims, ref GameAction action);
   }
}
