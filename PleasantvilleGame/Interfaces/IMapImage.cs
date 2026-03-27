using System.Windows.Controls;
using System.Windows.Media.Imaging;
using WpfAnimatedGif;

namespace PleasantvilleGame
{
   public interface IMapItemImage
   {
      string Name { get; set; }
      bool IsAnimated { get; set; }
      Image ImageControl { get; set; }
      ImageAnimationController? AnimationController { get; set; }
   }
   public interface IMapItemImages : System.Collections.IEnumerable
   {
      int Count { get; }
      void Add(IMapItemImage mii);
      void Insert(int index, IMapItemImage mii);
      void Clear();
      bool Contains(IMapItemImage mii);
      int IndexOf(IMapItemImage mii);
      void Remove(IMapItemImage mii);
      IMapItemImage? RemoveAt(int index);
      IMapItemImage? this[int index] { get; set; }
      IMapItemImage? Find(string pathToMatch);
      BitmapImage? GetBitmapImage(string name);
   }
}
