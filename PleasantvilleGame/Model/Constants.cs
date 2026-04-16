using System.Windows.Media;

namespace PleasantvilleGame
{
    public class Constants
    {
      public readonly static SolidColorBrush theTownControlledBrush = new SolidColorBrush() { Color = System.Windows.Media.Color.FromArgb(0xFF, 0x33, 0xAA, 0x33) };
      public readonly static SolidColorBrush theAlienControlledBrush = new SolidColorBrush() { Color = System.Windows.Media.Color.FromArgb(0xFF, 0xFF, 0xD5, 0x00) };
      public readonly static SolidColorBrush theSkepticalBrush = new SolidColorBrush() { Color = System.Windows.Media.Color.FromArgb(0xFF, 0xF2, 0xDE, 0x9B) };
      public readonly static SolidColorBrush theWaryBrush = new SolidColorBrush() { Color = System.Windows.Media.Color.FromArgb(0xFF, 0x87, 0xE5, 0x87) };
      public readonly static SolidColorBrush theNeutralBrush = new SolidColorBrush() { Color = System.Windows.Media.Colors.Gray};
      public readonly static int STD_WAIT = 10;
    }
}