using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PleasantvilleGame.Interfaces
{
   internal interface IPlayback
   {
      List<EnteredHex> EnteredHexes { get; }
   }
}
