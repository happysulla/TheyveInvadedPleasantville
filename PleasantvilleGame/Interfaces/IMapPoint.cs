using System;

namespace PleasantvilleGame
{
    public interface IMapPoint
    {
      double X { get; set; }
      double Y { get; set; }
      String ToString();
    }
}
