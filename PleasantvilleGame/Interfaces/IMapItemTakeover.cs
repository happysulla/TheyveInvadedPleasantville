using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PleasantvilleGame
{
    public interface IMapItemTakeover
    {
        IMapItem Alien { get; set; }
        IMapItem Uncontrolled { get; set; }
        String Observations { get; set; }
    }
 }
