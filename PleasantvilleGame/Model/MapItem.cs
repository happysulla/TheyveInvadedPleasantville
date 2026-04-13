using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Xml;
using System.Collections;
using Button = System.Windows.Controls.Button;
using Image = System.Windows.Controls.Image;

namespace PleasantvilleGame
{
   public class MapItem : IMapItem
   {
      [NonSerialized] private static Random theRandom = new Random();
      [NonSerialized] public static IMapImages theMapImages = new MapImages();
      [NonSerialized] protected static BitmapImage? theImgAlien = theMapImages.GetBitmapImage("OAlien");
      [NonSerialized] protected static BitmapImage? theImgBloodSpot = theMapImages.GetBitmapImage("OBloodSpot1");
      [NonSerialized] protected static BitmapImage? theImgImplant = theMapImages.GetBitmapImage("OImplant");
      [NonSerialized] protected static BitmapImage? theImgKia = theMapImages.GetBitmapImage("OKia");
      [NonSerialized] protected static BitmapImage? theKnockedOut = theMapImages.GetBitmapImage("OKnockedOut");
      [NonSerialized] protected static BitmapImage? theStunned = theMapImages.GetBitmapImage("OStunned");
      [NonSerialized] protected static BitmapImage? theSurrendered = theMapImages.GetBitmapImage("OSurrendered");
      [NonSerialized] protected static BitmapImage? theTieUp = theMapImages.GetBitmapImage("OTiedUp");
      [NonSerialized] protected static BitmapImage? theBloodSpot = theMapImages.GetBitmapImage("OWary");
      //--------------------------------------------------
      public string Name { get; set; } = string.Empty;
      public string TopImageName { get; set; } = string.Empty;
      public string BottomImageName { get; set; } = string.Empty;
      public string OverlayImageName { get; set; } = string.Empty;
      public List<BloodSpot> myWoundSpots = new List<BloodSpot>();
      public List<BloodSpot> WoundSpots { get => myWoundSpots; }
      public double Zoom { get; set; } = 1.0;
      public bool IsAnimated
      {
         set
         {
            if (null == TopImageName)
               return;
            IMapImage? mii = theMapImages.Find(TopImageName);
            if (null == mii)
            {
               Logger.Log(LogEnum.LE_ERROR, "IsAnimated.set() could not find map image for " + TopImageName);
               return;
            }
            mii.IsAnimated = value;
         }
         get
         {
            if (null == TopImageName)
               return false;
            IMapImage? mii = theMapImages.Find(TopImageName);
            if (null == mii)
            {
               Logger.Log(LogEnum.LE_ERROR, "IsAnimated.get() could not find map image for " + TopImageName);
               return false;
            }
            return mii.IsAnimated;
         }
      }
      public bool IsMoved { get; set; } = false;  
      public bool IsKilled { get; set; } = false;
      public int Count { get; set; } = 0;
      //--------------------------------------------------
      private IMapPoint myLocation = new MapPoint();  // top left corner of MapItem
      public IMapPoint Location
      {
         get => myLocation;
         set => myLocation = value;
      }
      protected ITerritory myTerritoryCurrent = new Territory();
      public ITerritory TerritoryCurrent
      {
         get => myTerritoryCurrent;
         set => myTerritoryCurrent = value;
      }
      protected ITerritory myTerritoryStarting = new Territory();
      public ITerritory TerritoryStarting { get => myTerritoryStarting; set => myTerritoryStarting = value; }
      //----------------------------------------
      public int Combat { get; set; } = 0;
      public int Influence { get; set; } = 0;
      public int Movement { get; set; } = 0;
      public int MovementUsed { get; set; } = 0;
      //----------------------------------------
      public bool IsConscious { get; set; } = false;
      public bool IsAlienUnknown { get; set; } = false;
      public bool IsAlienKnown { get; set; } = false;
      public bool IsControlled { get; set; } = false;
      public bool IsImplantHeld { get; set; } = false;
      public bool IsInterrogated { get; set; } = false;
      public bool IsSkeptical { get; set; } = false;
      public bool IsStunned { get; set; } = false;
      public bool IsSurrendered { get; set; } = false;
      public bool IsTiedUp { get; set; } = false;
      public bool IsWary { get; set; } = false;
      public bool IsMoveStoppedThisTurn { get; set; } = false;
      public bool IsMoveAllowedToResetThisTurn { get; set; } = false;
      public bool IsConversedThisTurn { get; set; } = false;
      public bool IsInfluencedThisTurn { get; set; } = false;
      public bool IsCombatThisTurn { get; set; } = false;
      public bool IsInterrogatedThisTurn { get; set; } = false;
      public bool IsImplantRemovalThisTurn { get; set; } = false;
      public bool IsTakeoverThisTurn { get; set; } = false;
      //----------------------------------------
      public MapItem(string name)
      {
         Name = name;
      }
      protected MapItem(string aName, double zoom, bool isAnimated, string topImageName)
      {
         try
         {
            Name = aName;
            Zoom = zoom;
            TopImageName = topImageName;
            IMapImage? mii = theMapImages.Find(topImageName);
            if (null == mii)
            {
               mii = new MapImage(topImageName);
               theMapImages.Add(mii);
            }
            IsAnimated = isAnimated;
         }
         catch (Exception ex)
         {
            Logger.Log(LogEnum.LE_ERROR, "MapItem(): aName=" + aName + "\n Ex=" + ex.ToString());
            return;
         }
      }
      protected MapItem(string aName, double zoom, bool isAnimated, string topImageName, string buttomImageName)
      {
         try
         {
            Name = aName;
            Zoom = zoom;
            TopImageName = topImageName;
            IMapImage? miiTop = theMapImages.Find(topImageName);
            if (null == miiTop)
            {
               miiTop = new MapImage(topImageName);
               theMapImages.Add(miiTop);
            }
            TopImageName = topImageName;
            IMapImage? miiBottom = theMapImages.Find(buttomImageName);
            if (null == miiBottom)
            {
               miiBottom = new MapImage(buttomImageName);
               theMapImages.Add(miiBottom);
            }
            IsAnimated = isAnimated;
         }
         catch (Exception ex)
         {
            Logger.Log(LogEnum.LE_ERROR, "MapItem(): aName=" + aName + "\n Ex=" + ex.ToString());
            return;
         }
      }
      public MapItem()  // used in MapItemMove constructor & GameLoadMgr.ReadXml()
      {
      }
      public MapItem(string name, double zoom, string topImageName, ITerritory territory) :
         this(name, zoom, false, topImageName)
      {
         TerritoryCurrent = territory;
         TerritoryStarting = territory;
         Location.X = territory.CenterPoint.X - zoom * Utilities.theMapItemOffset;
         Location.Y = territory.CenterPoint.Y - zoom * Utilities.theMapItemOffset;
      }
      public void Copy(IMapItem mi)
      {
         this.Name = mi.Name;
         this.TopImageName = mi.TopImageName;
         this.BottomImageName = mi.BottomImageName;
         this.OverlayImageName = mi.OverlayImageName;
         foreach (BloodSpot bs in mi.WoundSpots)
            this.WoundSpots.Add(bs);
         this.Zoom = mi.Zoom;
         this.IsMoved = mi.IsMoved;
         this.IsKilled = mi.IsKilled;
         this.Count = mi.Count;
         this.Location.X = mi.Location.X;
         this.Location.Y = mi.Location.Y;
         //--------------------------------------
         this.TerritoryCurrent = mi.TerritoryCurrent;
         this.TerritoryStarting = mi.TerritoryStarting;
         //--------------------------------------
         this.Combat = mi.Combat;
         this.Influence = mi.Influence;
         this.Movement = mi.Movement;
         this.MovementUsed = mi.MovementUsed;
         //--------------------------------------
         this.IsConscious = mi.IsConscious;
         this.IsAlienUnknown = mi.IsAlienUnknown;
         this.IsAlienKnown = mi.IsAlienKnown;
         this.IsControlled = mi.IsControlled;
         this.IsImplantHeld = mi.IsImplantHeld;
         this.IsSkeptical = mi.IsSkeptical;
         this.IsStunned = mi.IsStunned;
         this.IsSurrendered = mi.IsSurrendered;
         this.IsTiedUp = mi.IsTiedUp;
         this.IsWary = mi.IsWary;
         //--------------------------------------
         this.IsMoveStoppedThisTurn = mi.IsMoveStoppedThisTurn;
         this.IsMoveAllowedToResetThisTurn = mi.IsMoveAllowedToResetThisTurn;
         this.IsConversedThisTurn = mi.IsConversedThisTurn;
         this.IsInfluencedThisTurn = mi.IsInfluencedThisTurn;
         this.IsCombatThisTurn = mi.IsCombatThisTurn;
         this.IsInterrogatedThisTurn = mi.IsInterrogatedThisTurn;
         this.IsImplantRemovalThisTurn = mi.IsImplantRemovalThisTurn;
         this.IsTakeoverThisTurn = mi.IsTakeoverThisTurn;
      }
      public void Sync(IMapItem mi)
      {
         foreach (BloodSpot bs in mi.WoundSpots)
            this.WoundSpots.Add(bs);
         this.Zoom = mi.Zoom;
         this.IsMoved = mi.IsMoved;
         this.Count = mi.Count;
         this.Location.X = mi.Location.X;
         this.Location.Y = mi.Location.Y;
         //--------------------------------------
         //--------------------------------------
         this.TerritoryCurrent = mi.TerritoryCurrent;
         this.TerritoryStarting = mi.TerritoryStarting;
         //--------------------------------------
         this.Combat = mi.Combat;
         this.Influence = mi.Influence;
         this.Movement = mi.Movement;
         this.MovementUsed = mi.MovementUsed;
         //--------------------------------------
         this.IsConscious = mi.IsConscious;
         this.IsAlienUnknown = mi.IsAlienUnknown;
         this.IsAlienKnown = mi.IsAlienKnown;
         this.IsControlled = mi.IsControlled;
         this.IsImplantHeld = mi.IsImplantHeld;
         this.IsSkeptical = mi.IsSkeptical;
         this.IsStunned = mi.IsStunned;
         this.IsSurrendered = mi.IsSurrendered;
         this.IsTiedUp = mi.IsTiedUp;
         this.IsWary = mi.IsWary;
         //--------------------------------------
         this.IsMoveStoppedThisTurn = mi.IsMoveStoppedThisTurn;
         this.IsMoveAllowedToResetThisTurn = mi.IsMoveAllowedToResetThisTurn;
         this.IsConversedThisTurn = mi.IsConversedThisTurn;
         this.IsInfluencedThisTurn = mi.IsInfluencedThisTurn;
         this.IsCombatThisTurn = mi.IsCombatThisTurn;
         this.IsInterrogatedThisTurn = mi.IsInterrogatedThisTurn;
         this.IsImplantRemovalThisTurn = mi.IsImplantRemovalThisTurn;
         this.IsTakeoverThisTurn = mi.IsTakeoverThisTurn;

      } // sync this mapitem data with passed-in parameter during spotting
      public void SetBloodSpots(int percent = 30)
      {
         if (0 == percent) // heal if set to zero
         {
            myWoundSpots.Clear();
            return;
         }
         for (int spots = 0; spots < percent; ++spots) // splatter the MapItem with random blood spots
         {
            int range = (int)(Utilities.theMapItemSize);
            BloodSpot spot = new BloodSpot(range, theRandom);
            myWoundSpots.Add(spot);
         }
      }
      //----------------------------------------------------------------------------
      static public void Shuffle(ref List<IMapItem> mapItems)
      {
         for (int j = 0; j < 10; ++j)
         {
            List<IMapItem> newOrder = new List<IMapItem>();
            int count = mapItems.Count;
            for (int i = 0; i < count; i++)
            {
               int index = Utilities.RandomGenerator.Next(mapItems.Count);
               if (index < mapItems.Count)
               {
                  IMapItem randomIndex = (IMapItem)mapItems[index];
                  mapItems.RemoveAt(index);
                  newOrder.Add(randomIndex);
               }
            }
            mapItems = newOrder;
         }
      }
      public static void SetButtonContent(Button b, IMapItem mi, bool isAlienView, bool isMapItemZoom = false, bool isDecoration = true, bool isBloodSpotsShown = true)
      {
         double zoom = Utilities.ZOOM;
         if (true == isMapItemZoom)
            zoom = mi.Zoom;
         //------------------------------------------
         Grid g = new Grid() { };
         if (false == mi.IsAnimated)
         {
            Image img = new Image() { Source = theMapImages.GetBitmapImage(mi.TopImageName), Stretch = Stretch.Fill };
            img.Source = theMapImages.GetBitmapImage(mi.TopImageName);
            g.Children.Add(img);
         }
            //Grid buttonContent = new Grid();
            //Image img = new Image();
            //img.Source = theImages.GetImage(mi.TopImageName);
            //buttonContent.Children.Add(img);

            //// Add an alien head if the Controlled person knowns.

            //if (true == mi.IsAlienKnown)
            //{
            //   Image overlay = new Image();
            //   overlay.Source = theImages.GetImage("Alien");
            //   buttonContent.Children.Add(overlay);
            //}

            //// Add an tied up icon if tied up

            //if (true == mi.IsTiedUp)
            //{
            //   Image overlay = new Image();
            //   overlay.Source = theImages.GetImage("TiedUp");
            //   buttonContent.Children.Add(overlay);
            //}

            //if (true == mi.IsImplantHeld)
            //{
            //   Image overlay = new Image();
            //   overlay.Source = theImages.GetImage("Implant");
            //   buttonContent.Children.Add(overlay);
            //}

            //// Add additional words on image based on status.
            //// Only one of the following images is allowed.

            //if (true == mi.IsKilled)
            //{
            //   Image overlay = new Image();
            //   overlay.Source = theImages.GetImage("KIA");
            //   buttonContent.Children.Add(overlay);
            //}
            //else if (true == mi.IsSurrendered)
            //{
            //   Image overlay = new Image();
            //   overlay.Source = theImages.GetImage("Surrendered");
            //   buttonContent.Children.Add(overlay);
            //}
            //else if (false == mi.IsConscious)
            //{
            //   Image overlay = new Image();
            //   overlay.Source = theImages.GetImage("KnockedOut");
            //   buttonContent.Children.Add(overlay);
            //}
            //else if (true == mi.IsStunned)
            //{
            //   Image overlay = new Image();
            //   overlay.Source = theImages.GetImage("Stunned");
            //   buttonContent.Children.Add(overlay);
            //}

            //b.Content = buttonContent;

            //if ("Zebulon" == mi.Name)
            //   b.Background = Brushes.Black;
            //else if (true == mi.IsAlienKnown)
            //   b.Background = Utilities.theAlienControlledBrush;
            //else if ((true == mi.IsAlienUnknown) && (true == isAlienView))
            //   b.Background = Utilities.theAlienControlledBrush;
            //else if (true == mi.IsControlled)
            //   b.Background = Utilities.theTownControlledBrush;
            //else if (true == mi.IsSkeptical)
            //   b.Background = Utilities.theSkepticalBrush;
            //else if (true == mi.IsWary)
            //   b.Background = Utilities.theWaryBrush;
            //else
            //   b.Background = Brushes.White;

         }
      public override string ToString()
      {
         StringBuilder sb = new StringBuilder();
         sb.Append("Name=<");
         sb.Append(Name);
         sb.Append(">T=<");
         sb.Append(TerritoryCurrent.Name);
         return sb.ToString();
      }
   }
   //===========================================
   public class MapItems : IEnumerable, IMapItems
   {
      public static MapItems theMapItems = new MapItems();
      private ArrayList myList;
      public MapItems() { myList = new ArrayList(); }
      public void Add(IMapItem mi) { myList.Add(mi); }
      public IMapItem? RemoveAt(int index)
      {
         Object? o = myList[index];
         if( null == o)
         {
            Logger.Log(LogEnum.LE_ERROR, "MapItems.RemoveAt(): index=" + index + " is out of range.");
            return null;
         }
         IMapItem? mi = (IMapItem)o;
         myList.RemoveAt(index);
         return mi;
      }
      public void Insert(int index, IMapItem mi) { myList.Insert(index, mi); }
      public int Count { get { return myList.Count; } }
      public void Reverse() { myList.Reverse(); }
      public void Clear() { myList.Clear(); }
      public bool Contains(IMapItem mi) { return myList.Contains(mi); }
      public IEnumerator GetEnumerator() { return myList.GetEnumerator(); }
      public int IndexOf(IMapItem mi) { return myList.IndexOf(mi); }
      public void Remove(IMapItem mi) { myList.Remove(mi); }
      public IMapItem? Find(string miName)
      {
         int i = 0;
         foreach (Object o in myList)
         {
            IMapItem mi = (IMapItem)o;
            if (miName == mi.Name)
               return mi;
            ++i;
         }
         return null;
      }
      public IMapItem? Remove(string miName)
      {
         foreach (Object o in myList)
         {
            IMapItem mi = (IMapItem)o;
            if (miName == mi.Name)
            {
               myList.Remove(mi);
               return mi;
            }
         }
         return null;
      }
      public IMapItem? this[int index]
      {
         get 
         {
            Object? o = myList[index];
            if (null == o)
            {
               Logger.Log(LogEnum.LE_ERROR, "MapItemCombats.RemoveAt(): null object at index " + index.ToString());
               return null;
            }
            IMapItem mi = (IMapItem)o;
            return mi;
         }
         set { myList[index] = value; }
      }
      public IMapItems Shuffle()
      {
         IMapItems newOrder = new MapItems();
         int count = myList.Count;
         for (int i = 0; i < count; i++) // Random select card in myCards list and remove it.  Then add it to new list. 
         {
            int index = Utilities.RandomGenerator.Next(myList.Count);
            if (index < myList.Count)
            {
               Object? o = myList[index];
               if (null == o)
               {
                  Logger.Log(LogEnum.LE_ERROR, "MapItemCombats.RemoveAt(): null object at index " + index.ToString());
                  myList.RemoveAt(index);
                  continue;
               }
               IMapItem randomIndex = (IMapItem)o;
               myList.RemoveAt(index);
               newOrder.Add(randomIndex);
            }
         }
         return newOrder;
      }
      public IMapItems Sort()
      {
         MapItems sortedMapItems = new MapItems();
         foreach (Object o in myList)
         {
            IMapItem mi1 = (IMapItem)o;
            bool isMapItemInserted = false;
            if ((true == mi1.IsConscious) && (false == mi1.IsTiedUp) && (false == mi1.IsStunned))
            {
               int metric1 = mi1.Movement + mi1.Combat + mi1.Influence;
               int index = 0;
               foreach (IMapItem mi2 in sortedMapItems)
               {
                  int metric2 = mi2.Movement + mi2.Combat + mi2.Influence;
                  if (metric2 < metric1)
                  {
                     sortedMapItems.Insert(index, mi1);
                     isMapItemInserted = true;
                     break;
                  }
                  ++index;
               }
            }
            if (false == isMapItemInserted) // If not inserted, add to end
               sortedMapItems.Add(mi1);
         }

         return sortedMapItems;
      }
      public IMapItems SortOnCombat()
      {
         MapItems sortedMapItems = new MapItems();
         foreach (Object o in myList)
         {
            IMapItem mi1 = (IMapItem)o;
            bool isMapItemInserted = false;
            if ((true == mi1.IsConscious) && (false == mi1.IsTiedUp) && (false == mi1.IsStunned))
            {
               int index = 0;
               foreach (IMapItem mi2 in sortedMapItems)
               {
                  if (mi2.Combat < mi1.Combat)
                  {
                     sortedMapItems.Insert(index, mi1);
                     isMapItemInserted = true;
                     break;
                  }
                  ++index;
               }
            }
            if (false == isMapItemInserted) // If not inserted, add to end
               sortedMapItems.Add(mi1);
         }

         return sortedMapItems;
      }
      //------------------------------------------------------------
      public void Rotate(int numOfRotates)
      {
         for (int j = 0; j < numOfRotates; j++)
         {
            Object temp = myList[0];
            for (int i = 0; i < myList.Count - 1; i++)
               myList[i] = myList[i + 1];
            myList[myList.Count - 1] = temp;
         }
      }
      //------------------------------------------------------------
      public override String ToString()
      {
         StringBuilder sb = new StringBuilder();
         foreach (Object o in myList)
         {
            IMapItem mi = (IMapItem)o;
            sb.Append("Name=<");
            sb.Append(mi.Name);
            sb.Append("> Location=");
            sb.Append(mi.Location.ToString());
            sb.Append(">\n");
         }
         return sb.ToString();
      }
   }
}
