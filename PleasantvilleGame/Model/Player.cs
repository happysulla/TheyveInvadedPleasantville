using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PleasantvilleGame
{
   public class Player : IPlayer
   {
      public bool IsComputer { set; get; } = false;
      public Player(bool isComputer)
      {
         IsComputer = isComputer;
      }  
   }
}
