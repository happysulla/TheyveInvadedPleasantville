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
   }
   public interface IPlayerTown : IPlayer
   {
   }
   public interface IPlayerAlien : IPlayer
   {
      ITerritory ZebulonLocation{ set; get; }
      bool ChooseStartingHqArea();
   }
}
