using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PleasantvilleGame
{
   public interface IUndo
   {
      bool Undo(IGameInstance gi, IGameEngine ge, GameViewerWindow gvw);
   }
}
