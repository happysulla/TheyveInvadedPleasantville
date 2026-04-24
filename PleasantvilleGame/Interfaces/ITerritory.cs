using System;
using System.Collections.Generic;
namespace PleasantvilleGame
{
   public interface ITerritory
   {
      string Name { get; set; }
      string CanvasName { get; set; } // There may be multiple canvases, so this is the name of the canvas that contains this territory
      string ImageNum { get; set; }  // There may be multiple images on a canvas, so this is the name of the image that represents this territory
      IMapPoint CenterPoint { get; set; }
      List<IMapPoint> Points { set; get; }
      List<String> Adjacents { get; }
      List<String> PavedRoads { get; }
      List<String> Observations { get; }
      bool IsBuilding();
   }
   //--------------------------------------------------------
   public interface ITerritories : System.Collections.IEnumerable
   {
      int Count { get; }
      void Add(ITerritory t);
      void Insert(int index, ITerritory t);
      void Clear();
      bool Contains(ITerritory t);
      int IndexOf(ITerritory t);
      void Remove(ITerritory tName);
      ITerritory? Find(string tName);
      ITerritory? Find(string tName, string tSubname);
      ITerritory? Remove(string tName);
      ITerritory? RemoveAt(int index);
      ITerritory? this[int index] { get; set; }
   }
}
