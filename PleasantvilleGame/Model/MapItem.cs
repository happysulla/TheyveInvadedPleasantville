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
namespace PleasantvilleGame
{
    public class MapItem : IMapItem
    {
        [NonSerialized] public static MapItemImages theImages = new MapItemImages();
        private string myName;
        public string Name
        {
            get { return myName; }
            set { myName = value; }
        }
        private int myCombat;
        public int Combat
        {
            get { return myCombat; }
            set { myCombat = value; }
        }
        private int myInfluence;
        public int Influence
        {
            get { return myInfluence; }
            set { myInfluence = value; }
        }
        private int myMovement;
        public int Movement
        {
            get { return myMovement; }
            set { myMovement = value; }
        }
        private string myTopImageName;
        public string TopImageName
        {
            get { return myTopImageName; }
            set { myTopImageName = value; }
        }
        private string myBottomImageName;
        public string BottomImageName
        {
            get { return myBottomImageName; }
            set { myBottomImageName = value; }
        }
        private bool myIsAlienUnknown = false;
        public bool IsAlienUnknown
        {
            get { return myIsAlienUnknown; }
            set { myIsAlienUnknown = value; }
        }
        private bool myIsAlienKnown = false;
        public bool IsAlienKnown
        {
            get { return myIsAlienKnown; }
            set { myIsAlienKnown = value; }
        }
        private bool myIsControlled = false;
        public bool IsControlled
        {
            get { return myIsControlled; }
            set { myIsControlled = value; }
        }
        private IMapPoint myLocation=new MapPoint(0.0,0.0);
        public IMapPoint Location
        {
            get { return myLocation; }
            set { myLocation = value; }
        }
        private int myMovementUsed;
        public int MovementUsed
        {
            get { return myMovementUsed; }
            set { myMovementUsed = value; }
        }
        [NonSerialized] private ITerritory myTerritory;
        public ITerritory Territory
        {
            set { myTerritory = value; }
            get { return myTerritory; }
        }
        [NonSerialized] private ITerritory myTerritoryStarting = null;
        public ITerritory TerritoryStarting
        {
            set { myTerritoryStarting = value; }
            get { return myTerritoryStarting; }
        }
        //----------------------------------------------------------------------------
        private bool myIsConscious = true;
        public bool IsConscious
        {
            get { return myIsConscious; }
            set { myIsConscious = value; }
        }
        private bool myIsMoved = false;
        public bool IsMoved
        {
            set { myIsMoved = value; }
            get { return myIsMoved; }
        }
        private bool myIsImplantHeld = false;
        public bool IsImplantHeld
        {
            set { myIsImplantHeld = value; }
            get { return myIsImplantHeld; }
        }
        private bool myIsInterrogated = false;
        public bool IsInterrogated
        {
            get { return myIsInterrogated; }
            set { myIsInterrogated = value; }
        }
        private bool myIsKilled = false;
        public bool IsKilled
        {
            get { return myIsKilled; }
            set { myIsKilled = value; }
        }
        private bool myIsSkeptical = false;
        public bool IsSkeptical
        {
            get { return myIsSkeptical; }
            set { myIsSkeptical = value; }
        }
        private bool myIsStunned = false;
        public bool IsStunned
        {
            get { return myIsStunned; }
            set { myIsStunned = value; }
        }
        private bool myIsSurrendered = false;
        public bool IsSurrendered
        {
            get { return myIsSurrendered; }
            set { myIsSurrendered = value; }
        }
        private bool myIsTiedUp = false;
        public bool IsTiedUp
        {
            get { return myIsTiedUp; }
            set { myIsTiedUp = value; }
        }
        private bool myIsWary = false;
        public bool IsWary
        {
            get { return myIsWary; }
            set { myIsWary = value; }
        }
        private bool myIsMoveStoppedThisTurn = false;
        public bool IsMoveStoppedThisTurn
        {
            set { myIsMoveStoppedThisTurn = value; }
            get { return myIsMoveStoppedThisTurn; }
        }
        private bool myIsMoveAllowedToResetThisTurn = true;
        public bool IsMoveAllowedToResetThisTurn
        {
            set { myIsMoveAllowedToResetThisTurn = value; }
            get { return myIsMoveAllowedToResetThisTurn; }
        }
        private bool myIsConversedThisTurn = false;
        public bool IsConversedThisTurn
        {
            set { myIsConversedThisTurn = value; }
            get { return myIsConversedThisTurn; }
        }
        private bool myIsInfluencedThisTurn = false;
        public bool IsInfluencedThisTurn
        {
            set { myIsInfluencedThisTurn = value; }
            get { return myIsInfluencedThisTurn; }
        }
        private bool myIsCombatThisTurn = false;
        public bool IsCombatThisTurn
        {
            set { myIsCombatThisTurn = value; }
            get { return myIsCombatThisTurn; }
        }
        private bool myIsInterrogatedThisTurn = false;
        public bool IsInterrogatedThisTurn
        {
            set { myIsInterrogatedThisTurn = value; }
            get { return myIsInterrogatedThisTurn; }
        }
        private bool myIsImplantRemovalAttemptedThisTurn = false;
        public bool IsImplantRemovalThisTurn
        {
            set { myIsImplantRemovalAttemptedThisTurn = value; }
            get { return myIsImplantRemovalAttemptedThisTurn; }
        }
        private bool myIsTakeoverThisTurn = false;
        public bool IsTakeoverThisTurn
        {
            set { myIsTakeoverThisTurn = value; }
            get { return myIsTakeoverThisTurn; }
        }
        public MapItem() 
        {
        }
        public MapItem(IMapItem mi)
        {
            myName = mi.Name;
            myCombat = mi.Combat;
            myInfluence = mi.Influence;
            myMovement = mi.Movement;
            myTopImageName = mi.TopImageName;
            myBottomImageName = mi.BottomImageName;
            myIsAlienUnknown = mi.IsAlienUnknown;
            myIsAlienKnown = mi.IsAlienKnown;
            myIsControlled = mi.IsControlled;
            myLocation = new MapPoint (mi.Location.X, mi.Location.Y );
            myMovementUsed = mi.MovementUsed;
            myTerritory = mi.Territory;
            myTerritoryStarting = mi.TerritoryStarting;
            myIsConscious = mi.IsConscious;
            myIsMoved = mi.IsMoved;
            myIsMoveAllowedToResetThisTurn = mi.IsMoveAllowedToResetThisTurn;
            myIsImplantHeld = mi.IsImplantHeld;
            myIsInterrogated = mi.IsInterrogated;
            myIsKilled = mi.IsKilled;
            myIsSkeptical = mi.IsSkeptical;
            myIsStunned = mi.IsStunned;
            myIsSurrendered = mi.IsSurrendered;
            myIsTiedUp = mi.IsTiedUp;
            myIsWary = mi.IsWary;

            myIsMoveStoppedThisTurn = mi.IsMoveStoppedThisTurn;
            myIsImplantRemovalAttemptedThisTurn = mi.IsImplantRemovalThisTurn;
            myIsConversedThisTurn = mi.IsConversedThisTurn;
            myIsInfluencedThisTurn = mi.IsInfluencedThisTurn;
            myIsCombatThisTurn = mi.IsCombatThisTurn;
            myIsInterrogatedThisTurn = mi.IsInterrogatedThisTurn;
            myIsTakeoverThisTurn = mi.IsTakeoverThisTurn;
        }
        protected MapItem(string name)
        {
            this.Name = name;
        }
        public MapItem(string aName, IMapPoint aStartingPoint, string topImageName, string bottomImageName)
        {
            this.Name = aName;
            this.Location = aStartingPoint;
            this.TopImageName = topImageName;
            if (null == theImages.Find(topImageName))
                theImages.Add(new MapItemImage(topImageName));

            this.BottomImageName = bottomImageName;
            if (null == theImages.Find(bottomImageName))
                theImages.Add(new MapItemImage(bottomImageName));
            this.Territory = null;
        }
        public MapItem(string aName, MapPoint aStartingPoint, string topImageName, string BottomImageName, ITerritory territory) :
            this(aName, aStartingPoint, topImageName, BottomImageName)
        {
            myTerritory = territory;
            myTerritoryStarting = territory;
        }
        public MapItem(string aName, 
                       string topImageName, string BottomImageName, 
                        ITerritory territory,
                       int movement, int influence, int combat) :
            this(aName, territory.CenterPoint, topImageName, BottomImageName)
        {
            myTerritory = territory;
            myTerritoryStarting = territory;
            myMovement = movement;
            myInfluence = influence;
            myCombat = combat;
        }
        public override String ToString()
        {
            StringBuilder sb = new StringBuilder("Name=<");
            sb.Append(this.Name);
            sb.Append("> Location=<");
            sb.Append(this.Location.ToString());
            sb.Append("> Territory=<");
            sb.Append(this.Territory.Name);
            sb.Append(":");
            sb.Append(this.Territory.Sector.ToString());
            sb.Append(">\n");
            return sb.ToString();
        }
        public static void SetButtonContent(Button b, IMapItem mi, bool isAlienView = false)
        {
            Grid buttonContent = new Grid();
            Image img = new Image();
            img.Source = theImages.GetImage(mi.TopImageName); 
            buttonContent.Children.Add(img);

            // Add an alien head if the Controlled person knowns.

            if (true == mi.IsAlienKnown)
            {
                Image overlay = new Image();
                overlay.Source = theImages.GetImage("Alien");
                buttonContent.Children.Add(overlay);
            }

            // Add an tied up icon if tied up

            if (true == mi.IsTiedUp)
            {
                Image overlay = new Image();
                overlay.Source = theImages.GetImage("TiedUp");
                buttonContent.Children.Add(overlay);
            }

            if (true == mi.IsImplantHeld)
            {
                Image overlay = new Image();
                overlay.Source = theImages.GetImage("Implant");
                buttonContent.Children.Add(overlay);
            }

            // Add additional words on image based on status.
            // Only one of the following images is allowed.

            if (true == mi.IsKilled)
            {
                Image overlay = new Image();
                overlay.Source = theImages.GetImage("KIA");
                buttonContent.Children.Add(overlay);
            }
            else if (true == mi.IsSurrendered)
            {
                Image overlay = new Image();
                overlay.Source = theImages.GetImage("Surrendered");
                buttonContent.Children.Add(overlay);
            }
            else if (false == mi.IsConscious)
            {
                Image overlay = new Image();
                overlay.Source = theImages.GetImage("KnockedOut");
                buttonContent.Children.Add(overlay);
            }
            else if (true == mi.IsStunned)
            {
                Image overlay = new Image();
                overlay.Source = theImages.GetImage("Stunned");
                buttonContent.Children.Add(overlay);
            }

            b.Content = buttonContent;

            if ("Zebulon" == mi.Name)
                b.Background = Brushes.Black;
            else if (true == mi.IsAlienKnown)
                b.Background = Utilities.theAlienControlledBrush;
            else if ((true == mi.IsAlienUnknown) && (true == isAlienView))
                b.Background = Utilities.theAlienControlledBrush;
            else if (true == mi.IsControlled)
                b.Background = Utilities.theTownControlledBrush;
            else if (true == mi.IsSkeptical)
                b.Background = Utilities.theSkepticalBrush;
            else if (true == mi.IsWary)
                b.Background = Utilities.theWaryBrush;
            else
                b.Background = Brushes.White;

        }
        public string RemoveSpaces(string aLine)
        {
            string[] aStringArray1 = aLine.Split(new char[] { '"' });
            int length = aStringArray1.Length;
            if (0 == length % 2)
                throw new Exception("Syntax Error: Invalid number of quotes");

            for (int i = 0; i < aStringArray1.Length; i += 2)
            {
                string aSubString = "";
                string[] aStringArray2 = aStringArray1[i].Split(new char[] { ' ' });
                foreach (string aString in aStringArray2)
                    aSubString += aString;
                aStringArray1[i] = aSubString;
            }

            StringBuilder sb = new StringBuilder();
            foreach (string aString in aStringArray1)
                sb.Append(aString);
            aLine = sb.ToString();

            return aLine;
        }
        static public void Shuffle(ref List<IMapItem> mapItems)
        {
            for (int j = 0; j < 10; ++j)
            {
                List<IMapItem> newOrder = new List<IMapItem>();

                // Random select card in myCards list and
                // remove it.  Then add it to new list. 

                int count = mapItems.Count;
                for (int i = 0; i < count; i++)
                {
                    int index = GameEngine.RandomGenerator.Next(mapItems.Count);
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
        static public IMapItems CreatePeople()
        {
            if (null == theImages.Find("TiedUp"))
                theImages.Add(new MapItemImage("TiedUp"));
            if (null == theImages.Find("KIA"))
                theImages.Add(new MapItemImage("KIA"));
            if (null == theImages.Find("KnockedOut"))
                theImages.Add(new MapItemImage("KnockedOut"));
            if (null == theImages.Find("Stunned"))
                theImages.Add(new MapItemImage("Stunned"));
            if (null == theImages.Find("Surrendered"))
                theImages.Add(new MapItemImage("Surrendered"));
            if (null == theImages.Find("Alien"))
                theImages.Add(new MapItemImage("Alien"));
            if (null == theImages.Find("Implant"))
                theImages.Add(new MapItemImage("Implant"));

            IMapItems people = new MapItems();
            XmlTextReader reader = null;
            try
            {
                // Load the reader with the data file and ignore all white space nodes.         
                reader = new XmlTextReader("People.xml");
                reader.WhitespaceHandling = WhitespaceHandling.None;
                while (reader.Read())
                {
                    if (reader.Name == "Person")
                    {
                        if (reader.IsStartElement())
                        {
                            string name = reader.GetAttribute("value");
                            reader.Read(); // read the territory
                            string territoryName = reader.GetAttribute("value");
                            reader.Read(); // read the sector
                            string sector = reader.GetAttribute("value");
                            reader.Read(); // read the movement
                            string movement = reader.GetAttribute("value");
                            reader.Read(); // read the influence
                            string influence = reader.GetAttribute("value");
                            reader.Read(); // read the combat
                            string combat = reader.GetAttribute("value");
                            ITerritory t = Pleasantville.Territory.Find(territoryName, Int32.Parse(sector));
                            IMapItem person = new MapItem(name, name, name, t, Int32.Parse(movement), Int32.Parse(influence), Int32.Parse(combat));
                            people.Add(person);
                        }
                    }

                 }   // end while

                 return people;

            } // try
            catch (Exception e)
            {
                Console.WriteLine("MapItem.CreatePeople(): Exception:  e.Message={0} while reading reader.Name={1}", e.Message, reader.Name);
                return people;
            }

            finally
            {
                if (reader != null)
                    reader.Close();
            }
        }
    }
   //===========================================
    public class MapItems : IEnumerable, IMapItems
    {
        private ArrayList myList;
        public MapItems() { myList = new ArrayList(); }
        public void Add(IMapItem mi) { myList.Add(mi); }
        public IMapItem RemoveAt(int index)
        {
            IMapItem mi = (IMapItem)myList[index];
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
        public IMapItem Find(string miName)
        {
            int i=0;
            foreach (Object o in myList)
            {
                IMapItem mi = (IMapItem)o;
                if (miName == mi.Name)
                    return mi;
                ++i;
            }
            return null;
        }
        public IMapItem Remove(string miName)
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
        public IMapItem this[int index]
        {
            get { return (IMapItem)(myList[index]); }
            set { myList[index] = value; }
        }
        public IMapItems Shuffle()
        {
            IMapItems newOrder = new MapItems();

            // Random select card in myCards list and
            // remove it.  Then add it to new list. 

            int count = myList.Count;
            for (int i = 0; i < count; i++)
            {
                int index = GameEngine.RandomGenerator.Next(myList.Count);
                if (index < myList.Count)
                {
                    IMapItem randomIndex = (IMapItem)myList[index];
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
