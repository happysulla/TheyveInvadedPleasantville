
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Text;
using System.Xml;

namespace PleasantvilleGame
{
   internal class GameLoadMgr
   {
      public static string theGamesDirectory = "";
      public static IMapItems theMapItems = new MapItems();
      //--------------------------------------------------
      public GameLoadMgr() { }
      //--------------------------------------------------
      public IGameInstance? OpenGame(string filename)
      {
         try
         {
            if (false == Directory.Exists(theGamesDirectory)) // create directory if does not exists
               Directory.CreateDirectory(theGamesDirectory);
            string filenamePlusFilepath = theGamesDirectory + filename;
            //-------------------------------------
            CultureInfo currentCulture = CultureInfo.CurrentCulture;
            System.Threading.Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
            IGameInstance? gi = ReadXmlGameInstance(filenamePlusFilepath);
            System.Threading.Thread.CurrentThread.CurrentCulture = currentCulture;
            //-------------------------------------
            if (null == gi)
            {
               Logger.Log(LogEnum.LE_ERROR, "Open_Game(): ReadXmlGameInstance() returned null for " + filename);
               return null;
            }
            Logger.Log(LogEnum.LE_GAME_INIT, "Open_Game(): gi=" + gi.ToString());
            return gi;
         }
         catch (Exception e)
         {
            Logger.Log(LogEnum.LE_ERROR, "Open_Game(): path=" + theGamesDirectory + " e =" + e.ToString());
            return new GameInstance();
         }
      }
      //--------------------------------------------------
      public bool SaveGame(IGameInstance gi, string filename)
      {
         try
         {
            if (false == Directory.Exists(theGamesDirectory)) // create directory if does not exists
               Directory.CreateDirectory(theGamesDirectory);
         }
         catch (Exception e)
         {
            Logger.Log(LogEnum.LE_ERROR, "Save_Game(): path=" + theGamesDirectory + " e=" + e.ToString());
            return false;
         }
         try
         {
            //--------------------------------------
            CultureInfo currentCulture = CultureInfo.CurrentCulture;
            System.Threading.Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
            XmlDocument? aXmlDocument = CreateXmlGameInstance(gi); // create a new XML document
            System.Threading.Thread.CurrentThread.CurrentCulture = currentCulture;
            //--------------------------------------
            if (null == aXmlDocument)
            {
               Logger.Log(LogEnum.LE_ERROR, "Save_Game(): CreateXmlGameInstance() returned null for path=" + theGamesDirectory);
               return false;
            }
            string filenamePlusPath = theGamesDirectory + filename;
            using (FileStream writer = new FileStream(filenamePlusPath, FileMode.OpenOrCreate, FileAccess.Write))
            {
               XmlWriterSettings settings = new XmlWriterSettings { Indent = true, OmitXmlDeclaration = true, NewLineOnAttributes = false };
               using (XmlWriter xmlWriter = XmlWriter.Create(writer, settings)) // For XmlWriter, it uses the stream that was created: writer.
               {
                  aXmlDocument.Save(xmlWriter);
               }
            }
            return true;
         }
         catch (Exception ex)
         {
            Logger.Log(LogEnum.LE_ERROR, "Save_Game(): path=" + theGamesDirectory + " e =" + ex.ToString());
            System.Diagnostics.Debug.WriteLine(ex.ToString());
            return false;
         }
      }
      //--------------------------------------------------
      public IGameInstance? OpenGameFromFile()
      {
         try
         {
            if (false == Directory.Exists(theGamesDirectory)) // create directory if does not exists
               Directory.CreateDirectory(theGamesDirectory);
            Directory.SetCurrentDirectory(theGamesDirectory);
         }
         catch (Exception e)
         {
            Logger.Log(LogEnum.LE_ERROR, "Open_GameFromFile(): path=" + theGamesDirectory + " e=" + e.ToString());
            return null;
         }
         try
         {
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();
            dlg.InitialDirectory = theGamesDirectory;
            dlg.RestoreDirectory = true;
            dlg.Filter = "Patton's Best Games|*.pbg";
            if (true == dlg.ShowDialog())
            {
               CultureInfo currentCulture = CultureInfo.CurrentCulture;
               System.Threading.Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
               IGameInstance? gi = ReadXmlGameInstance(dlg.FileName);
               System.Threading.Thread.CurrentThread.CurrentCulture = currentCulture;
               if (null == gi)
               {
                  Directory.SetCurrentDirectory(MainWindow.theAssemblyDirectory);
                  Logger.Log(LogEnum.LE_ERROR, "Open_GameFromFile(): ReadXmlGameInstance() returned null for " + dlg.FileName);
                  return null;
               }
               Logger.Log(LogEnum.LE_GAME_INIT, "Open_GameFromFile(): gi=" + gi.ToString());
               string? gamePath = Path.GetDirectoryName(dlg.FileName); // save off the directory user chosen
               if (null == gamePath)
               {
                  Directory.SetCurrentDirectory(MainWindow.theAssemblyDirectory);
                  Logger.Log(LogEnum.LE_ERROR, "Open_GameFromFile(): Path.GetDirectoryName() returned null for fn=" + dlg.FileName);
                  return null;
               }
               theGamesDirectory = gamePath;
               theGamesDirectory += "\\";
               Directory.SetCurrentDirectory(MainWindow.theAssemblyDirectory);
               return gi;
            }
         }
         catch (Exception e)
         {
            Logger.Log(LogEnum.LE_ERROR, "Open_GameFromFile(): path=" + theGamesDirectory + " e =" + e.ToString());
         }
         Directory.SetCurrentDirectory(MainWindow.theAssemblyDirectory);
         return null;
      }
      //--------------------------------------------------
      public bool SaveGameAsToFile(IGameInstance gi)
      {
         try
         {
            if (false == Directory.Exists(theGamesDirectory)) // create directory if does not exists
               Directory.CreateDirectory(theGamesDirectory);
         }
         catch (Exception e)
         {
            Logger.Log(LogEnum.LE_ERROR, "SaveGameAsToFile(): path=" + theGamesDirectory + " e=" + e.ToString());
            return false;
         }
         try
         {
            Microsoft.Win32.SaveFileDialog dlg = new Microsoft.Win32.SaveFileDialog();
            string filename = GetFileName(gi);
            dlg.FileName = filename;
            dlg.InitialDirectory = theGamesDirectory;
            dlg.RestoreDirectory = true;
            if (true == dlg.ShowDialog())
            {
               CultureInfo currentCulture = CultureInfo.CurrentCulture;
               System.Threading.Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
               XmlDocument? aXmlDocument = CreateXmlGameInstance(gi); // create a new XML document
               System.Threading.Thread.CurrentThread.CurrentCulture = currentCulture;
               if (null == aXmlDocument)
               {
                  Logger.Log(LogEnum.LE_ERROR, "SaveGameAsToFile(): CreateXmlGameInstance() returned null for path=" + theGamesDirectory);
                  return false;
               }
               using (FileStream writer = new FileStream(dlg.FileName, FileMode.OpenOrCreate, FileAccess.Write))
               {
                  XmlWriterSettings settings = new XmlWriterSettings { Indent = true, OmitXmlDeclaration = true, NewLineOnAttributes = false };
                  using (XmlWriter xmlWriter = XmlWriter.Create(writer, settings)) // For XmlWriter, it uses the stream that was created: writer.
                  {
                     aXmlDocument.Save(xmlWriter);
                  }
               }
               string? gamePath = Path.GetDirectoryName(dlg.FileName); // save off the directory user chosen
               if (null == gamePath)
               {
                  Logger.Log(LogEnum.LE_ERROR, "SaveGameAsToFile(): Path.GetDirectoryName() returned null for fn=" + dlg.FileName);
                  return false;
               }
               theGamesDirectory = gamePath; // save off the directory user chosen
               theGamesDirectory += "\\";
            }
         }
         catch (Exception ex)
         {
            Logger.Log(LogEnum.LE_ERROR, "SaveGameAsToFile(): path=" + theGamesDirectory + " e =" + ex.ToString());
            return false;
         }
         return true;
      }
      //--------------------------------------------------
      private string GetFileName(IGameInstance gi)
      {
         StringBuilder sb = new StringBuilder();
         sb.Append(DateTime.Now.ToString("yyyyMMdd-HHmmss"));
         sb.Append("-D");
         int Day = gi.Day + 1;
         if (Day < 100)
            sb.Append("0");
         if (Day < 10)
            sb.Append("0");
         sb.Append(Day.ToString());
         IGameCommand? command = gi.GameCommands.GetLast();
         if (null != command)
            sb.Append("-" + command.Action.ToString());
         sb.Append(".pbg");
         return sb.ToString();
      }
      //--------------------------------------------------
      private int GetMajorVersion()
      {
         Assembly assembly = Assembly.GetExecutingAssembly();
         if (null == assembly)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXmlGameInstance(): Assembly.GetExecutingAssembly()=null");
            return -1;
         }
         Version? versionRunning = assembly.GetName().Version;
         if (null == versionRunning)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXmlGameInstance():  assembly.GetName().Version=null");
            return -1;
         }
         return versionRunning.Major;
      }
      public bool ReadXmlTerritories(XmlReader reader, ITerritories territories) // initial loading of Territories.theTerritories
      {
         CultureInfo currentCulture = CultureInfo.CurrentCulture;
         System.Threading.Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
         try
         {
            reader.Read();
            if (false == reader.IsStartElement())
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXml_Territories(): IsStartElement(Territories)=false");
               return false;
            }
            if (reader.Name != "Territories")
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXml_Territories(): Territories != (node=" + reader.Name + ")");
               return false;
            }
            //-----------------------------------------------------------------
            string? sCount = reader.GetAttribute("count");
            if (null == sCount)
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXml_Territories(): Territories.Count=null");
               return false;
            }
            int count = int.Parse(sCount);
            //-----------------------------------------------------------------
            for (int i = 0; i < count; ++i)
            {
               ITerritory territory = new Territory();
               reader.Read();
               if (false == reader.IsStartElement())
               {
                  Logger.Log(LogEnum.LE_ERROR, "ReadXml_Territories(): IsStartElement(Territory)=false count=" + count.ToString() + " i=" + i.ToString());
                  return false;
               }
               if (reader.Name != "Territory")
               {
                  Logger.Log(LogEnum.LE_ERROR, "ReadXml_Territories(): Territory != (node=" + reader.Name + ")");
                  return false;
               }
               string? tName = reader.GetAttribute("value");
               if (null == tName)
               {
                  Logger.Log(LogEnum.LE_ERROR, "ReadXml_Territories(): GetAttribute() returned false");
                  return false;
               }
               territory.Name = tName;
               //--------------------------------------
               reader.Read();
               if (false == reader.IsStartElement())
               {
                  Logger.Log(LogEnum.LE_ERROR, "ReadXml_Territories(): IsStartElement(Parent)=false tName=" + tName);
                  return false;
               }
               if (reader.Name != "Parent")
               {
                  Logger.Log(LogEnum.LE_ERROR, "ReadXml_Territories(): Parent != (node=" + reader.Name + ")");
                  return false;
               }
               string? sAttribute = reader.GetAttribute("value");
               if (null == sAttribute)
               {
                  Logger.Log(LogEnum.LE_ERROR, "ReadXml_Territories(): GetAttribute(Parent)=null");
                  return false;
               }
               territory.CanvasName = sAttribute;
               //--------------------------------------
               reader.Read();
               if (false == reader.IsStartElement())
               {
                  Logger.Log(LogEnum.LE_ERROR, "ReadXml_Territories(): IsStartElement(Type)=false");
                  return false;
               }
               if (reader.Name != "Type")
               {
                  Logger.Log(LogEnum.LE_ERROR, "ReadXml_Territories(): Type != (node=" + reader.Name + ")");
                  return false;
               }
               string? sAttribute1 = reader.GetAttribute("value");
               if (null == sAttribute1)
               {
                  Logger.Log(LogEnum.LE_ERROR, "ReadXml_Territories(): GetAttribute(Type)=null");
                  return false;
               }
               territory.Type = sAttribute1;
               //--------------------------------------
               reader.Read();
               if (false == reader.IsStartElement())
               {
                  Logger.Log(LogEnum.LE_ERROR, "ReadXml_Territories(): IsStartElement(CenterPoint)=false");
                  return false;
               }
               if (reader.Name != "CenterPoint")
               {
                  Logger.Log(LogEnum.LE_ERROR, "ReadXml_Territories(): CenterPoint != (node=" + reader.Name + ")");
                  return false;
               }
               string? sX = reader.GetAttribute("X");
               if (null == sX)
               {
                  Logger.Log(LogEnum.LE_ERROR, "ReadXml_Territories(): GetAttribute(sX)=null");
                  return false;
               }
               territory.CenterPoint.X = double.Parse(sX);
               string? sY = reader.GetAttribute("Y");
               if (null == sY)
               {
                  Logger.Log(LogEnum.LE_ERROR, "ReadXml_Territories(): GetAttribute(sX)=null");
                  return false;
               }
               territory.CenterPoint.Y = double.Parse(sY);
               //--------------------------------------
               reader.Read();
               if (false == reader.IsStartElement())
               {
                  Logger.Log(LogEnum.LE_ERROR, "ReadXml_Territories(): IsStartElement(Points)=false");
                  return false;
               }
               if (reader.Name != "Points")
               {
                  Logger.Log(LogEnum.LE_ERROR, "ReadXml_Territories(): Points != (node=" + reader.Name + ")");
                  return false;
               }
               string? sCount0 = reader.GetAttribute("count");
               if (null == sCount0)
               {
                  Logger.Log(LogEnum.LE_ERROR, "ReadXml_Territories(): GetAttribute(sCount0)=null");
                  return false;
               }
               int count0 = int.Parse(sCount0);
               for (int i1 = 0; i1 < count0; ++i1)
               {
                  reader.Read();
                  if (false == reader.IsStartElement())
                  {
                     Logger.Log(LogEnum.LE_ERROR, "ReadXml_Territories(): IsStartElement(regionPoint)=false");
                     return false;
                  }
                  if (reader.Name != "regionPoint")
                  {
                     Logger.Log(LogEnum.LE_ERROR, "ReadXml_Territories(): regionPoint != (node=" + reader.Name + ")");
                     return false;
                  }
                  string? sX1 = reader.GetAttribute("X");
                  if (null == sX1)
                  {
                     Logger.Log(LogEnum.LE_ERROR, "ReadXml_Territories(): GetAttribute(sX1)=null");
                     return false;
                  }
                  string? sY1 = reader.GetAttribute("Y");
                  if (null == sY1)
                  {
                     Logger.Log(LogEnum.LE_ERROR, "ReadXml_Territories(): GetAttribute(sY1)=null");
                     return false;
                  }
                  double x = double.Parse(sX1);
                  double y = double.Parse(sY1);
                  IMapPoint mp = new MapPoint(x, y);
                  territory.Points.Add(mp);
               }
               if (0 < count0)
                  reader.Read(); // get past </Points> tag
               //--------------------------------------
               reader.Read();
               if (false == reader.IsStartElement())
               {
                  Logger.Log(LogEnum.LE_ERROR, "ReadXml_Territories(): IsStartElement(Adjacents)=false");
                  return false;
               }
               if (reader.Name != "Adjacents")
               {
                  Logger.Log(LogEnum.LE_ERROR, "ReadXml_Territories(): Adjacents != (node=" + reader.Name + ")");
                  return false;
               }
               string? sCount3 = reader.GetAttribute("count");
               if (null == sCount3)
               {
                  Logger.Log(LogEnum.LE_ERROR, "ReadXml_Territories(): GetAttribute(sCount3)=null");
                  return false;
               }
               int count3 = int.Parse(sCount3);
               for (int i3 = 0; i3 < count3; ++i3)
               {
                  reader.Read();
                  if (false == reader.IsStartElement())
                  {
                     Logger.Log(LogEnum.LE_ERROR, "ReadXml_Territories(): IsStartElement(adjacent)=false");
                     return false;
                  }
                  if (reader.Name != "adjacent")
                  {
                     Logger.Log(LogEnum.LE_ERROR, "ReadXml_Territories(): adjacent != (node=" + reader.Name + ")");
                     return false;
                  }
                  string? sAdjacent = reader.GetAttribute("value");
                  if (null == sAdjacent)
                  {
                     Logger.Log(LogEnum.LE_ERROR, "ReadXml_Territories(): GetAttribute(sAdjacent)=null");
                     return false;
                  }
                  territory.Adjacents.Add(sAdjacent);
               }
               if (0 < count3)
                  reader.Read(); // get past </Adjacents> tag
               //--------------------------------------
               territories.Add(territory);
               reader.Read(); // get past </Territory> tag
            }
            if (0 < count)
               reader.Read(); // get past </Territories> tag
         }
         finally
         {
            System.Threading.Thread.CurrentThread.CurrentCulture = currentCulture;
         }
         System.Threading.Thread.CurrentThread.CurrentCulture = currentCulture;
         return true;
      }
      public bool CreateXmlTerritories(XmlDocument aXmlDocument, ITerritories territories) // initial creation of Territories.theTerritories during unit testing
      {
         CultureInfo currentCulture = CultureInfo.CurrentCulture;
         System.Threading.Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
         try
         {
            XmlNode? root = aXmlDocument.DocumentElement;
            if (null == root)
            {
               Logger.Log(LogEnum.LE_ERROR, "CreateXml_Territories(): root is null");
               return false;
            }
            XmlAttribute xmlAttribute = aXmlDocument.CreateAttribute("count");
            xmlAttribute.Value = territories.Count.ToString();
            if (null == root.Attributes)
            {
               Logger.Log(LogEnum.LE_ERROR, "CreateXml_Territories(): root.Attributes is null");
               return false;
            }
            root.Attributes.Append(xmlAttribute);
            //--------------------------------
            foreach (Territory t in territories)
            {
               XmlElement? terrElem = aXmlDocument.CreateElement("Territory");  // name of territory
               if (null == terrElem)
               {
                  Logger.Log(LogEnum.LE_ERROR, "CreateXml_Territories(): CreateElement(terrElem) returned null");
                  return false;
               }
               terrElem.SetAttribute("value", t.Name);
               XmlNode? territoryNode = root.AppendChild(terrElem);
               if (null == territoryNode)
               {
                  Logger.Log(LogEnum.LE_ERROR, "CreateXml_Territories(): AppendChild(territoryNode) returned null");
                  return false;
               }
               //---------------------------------
               XmlElement? elem = aXmlDocument.CreateElement("Parent");
               if (null == elem)
               {
                  Logger.Log(LogEnum.LE_ERROR, "CreateXml_Territories(): CreateElement(terrElem) returned null");
                  return false;
               }
               elem.SetAttribute("value", t.CanvasName);
               XmlNode? node = territoryNode.AppendChild(elem);
               if (null == node)
               {
                  Logger.Log(LogEnum.LE_ERROR, "CreateXml_Territories(): AppendChild(node) returned null");
                  return false;
               }
               //---------------------------------
               elem = aXmlDocument.CreateElement("Type");
               if (null == elem)
               {
                  Logger.Log(LogEnum.LE_ERROR, "CreateXml_Territories(): CreateElement(terrElem) returned null");
                  return false;
               }
               elem.SetAttribute("value", t.Type);
               node = territoryNode.AppendChild(elem);
               if (null == node)
               {
                  Logger.Log(LogEnum.LE_ERROR, "CreateXml_Territories(): AppendChild(node) returned null");
                  return false;
               }
               //---------------------------------
               elem = aXmlDocument.CreateElement("CenterPoint");
               if (null == elem)
               {
                  Logger.Log(LogEnum.LE_ERROR, "CreateXml_Territories(): CreateElement(CenterPoint) returned null");
                  return false;
               }
               elem.SetAttribute("X", t.CenterPoint.X.ToString("0000.00"));
               elem.SetAttribute("Y", t.CenterPoint.Y.ToString("0000.00"));
               node = territoryNode.AppendChild(elem);
               if (null == node)
               {
                  Logger.Log(LogEnum.LE_ERROR, "CreateXml_Territories(): AppendChild(node) returned null");
                  return false;
               }
               //---------------------------------
               XmlElement? elemPoints = aXmlDocument.CreateElement("Points");
               if (null == elemPoints)
               {
                  Logger.Log(LogEnum.LE_ERROR, "CreateXml_Territories(): CreateElement(elemPoints) returned null");
                  return false;
               }
               elemPoints.SetAttribute("count", t.Points.Count.ToString());
               XmlNode? nodePoints = territoryNode.AppendChild(elemPoints);
               if (null == nodePoints)
               {
                  Logger.Log(LogEnum.LE_ERROR, "CreateXml_Territories(): AppendChild(nodePoints) returned null");
                  return false;
               }
               //---------------------------------
               foreach (IMapPoint mp in t.Points)
               {
                  elem = aXmlDocument.CreateElement("regionPoint");
                  if (null == elem)
                  {
                     Logger.Log(LogEnum.LE_ERROR, "CreateXml_Territories(): CreateElement(terrElem) returned null");
                     return false;
                  }
                  elem.SetAttribute("X", mp.X.ToString("0000.00"));
                  elem.SetAttribute("Y", mp.Y.ToString("0000.00"));
                  node = nodePoints.AppendChild(elem);
                  if (null == node)
                  {
                     Logger.Log(LogEnum.LE_ERROR, "CreateXml_Territories(): AppendChild(node) returned null");
                     return false;
                  }
               }
               //-----------------------------------------------------------
               XmlElement? elemAdjacents = aXmlDocument.CreateElement("Adjacents");
               if (null == elemAdjacents)
               {
                  Logger.Log(LogEnum.LE_ERROR, "CreateXml_Territories(): CreateElement(elemAdjacents) returned null");
                  return false;
               }
               elemAdjacents.SetAttribute("count", t.Adjacents.Count.ToString());
               XmlNode? nodeAdjacents = territoryNode.AppendChild(elemAdjacents);
               if (null == nodeAdjacents)
               {
                  Logger.Log(LogEnum.LE_ERROR, "CreateXml_Territories(): AppendChild(nodePoints) returned null");
                  return false;
               }
               //---------------------------------
               foreach (string s in t.Adjacents)
               {
                  elem = aXmlDocument.CreateElement("adjacent");
                  if (null == elem)
                  {
                     Logger.Log(LogEnum.LE_ERROR, "CreateXml_Territories(): CreateElement(adjacent) returned null");
                     return false;
                  }
                  elem.SetAttribute("value", s);
                  node = nodeAdjacents.AppendChild(elem);
                  if (null == node)
                  {
                     Logger.Log(LogEnum.LE_ERROR, "CreateXml_Territories(): AppendChild(nodeAdjacents) returned null");
                     return false;
                  }
               }
            }
         }
         finally
         {

         }
         return true;
      }
      public bool ReadXmlTownspeople(XmlReader reader, IMapItems townspeople)
      {
         CultureInfo currentCulture = CultureInfo.CurrentCulture;
         System.Threading.Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
         try
         {
            reader.Read();
            if (false == reader.IsStartElement())
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXml_Townspeople(): IsStartElement(Territories)=false");
               return false;
            }
            if (reader.Name != "Townspeople")
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXml_Townspeople(): Territories != (node=" + reader.Name + ")");
               return false;
            }
            //-----------------------------------------------------------------
            string? sCount = reader.GetAttribute("count");
            if (null == sCount)
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXml_Townspeople(): Territories.Count=null");
               return false;
            }
            int count = int.Parse(sCount);
            //-----------------------------------------------------------------
            for (int i = 0; i < count; ++i)
            {
               IMapItem townperson = new MapItem();
               reader.Read();
               if (false == reader.IsStartElement())
               {
                  Logger.Log(LogEnum.LE_ERROR, "ReadXml_Townspeople(): IsStartElement(Territory)=false count=" + count.ToString() + " i=" + i.ToString());
                  return false;
               }
               if (reader.Name != "Name")
               {
                  Logger.Log(LogEnum.LE_ERROR, "ReadXml_Townspeople(): Name != (node=" + reader.Name + ")");
                  return false;
               }
               string? name = reader.GetAttribute("value");
               if (null == name)
               {
                  Logger.Log(LogEnum.LE_ERROR, "ReadXml_Townspeople(): GetAttribute() returned false");
                  return false;
               }
               townperson.Name = name;
               //--------------------------------------
               reader.Read();
               if (false == reader.IsStartElement())
               {
                  Logger.Log(LogEnum.LE_ERROR, "ReadXml_Townspeople(): IsStartElement(Parent)=false for name=" + name);
                  return false;
               }
               if (reader.Name != "TopImageName")
               {
                  Logger.Log(LogEnum.LE_ERROR, "ReadXml_Townspeople(): TopImageName != (node=" + reader.Name + ")");
                  return false;
               }
               string? sAttribute = reader.GetAttribute("value");
               if (null == sAttribute)
               {
                  Logger.Log(LogEnum.LE_ERROR, "ReadXml_Townspeople(): GetAttribute(TopImageName)=null");
                  return false;
               }
               townperson.TopImageName = sAttribute;
               //--------------------------------------
               reader.Read();
               if (false == reader.IsStartElement())
               {
                  Logger.Log(LogEnum.LE_ERROR, "ReadXml_Townspeople(): IsStartElement(Type)=false");
                  return false;
               }
               if (reader.Name != "BottomImageName")
               {
                  Logger.Log(LogEnum.LE_ERROR, "ReadXml_Townspeople(): BottomImageName != (node=" + reader.Name + ")");
                  return false;
               }
               string? sAttribute1 = reader.GetAttribute("value");
               if (null == sAttribute1)
               {
                  Logger.Log(LogEnum.LE_ERROR, "ReadXml_Townspeople(): GetAttribute(BottomImageName)=null");
                  return false;
               }
               townperson.BottomImageName = sAttribute1;
               //--------------------------------------
               reader.Read();
               if (false == reader.IsStartElement())
               {
                  Logger.Log(LogEnum.LE_ERROR, "ReadXml_Townspeople(): IsStartElement(Zoom)=false");
                  return false;
               }
               if (reader.Name != "Zoom")
               {
                  Logger.Log(LogEnum.LE_ERROR, "ReadXml_Townspeople(): Zoom != (node=" + reader.Name + ")");
                  return false;
               }
               string? sZoom = reader.GetAttribute("value");
               if (null == sX)
               {
                  Logger.Log(LogEnum.LE_ERROR, "ReadXml_Townspeople(): GetAttribute(Zoom)=null");
                  return false;
               }
               townperson.Zoom = double.Parse(sZoom);
               //--------------------------------------
               reader.Read();
               if (false == reader.IsStartElement())
               {
                  Logger.Log(LogEnum.LE_ERROR, "ReadXml_Townspeople(): IsStartElement(Points)=false");
                  return false;
               }
               if (reader.Name != "Points")
               {
                  Logger.Log(LogEnum.LE_ERROR, "ReadXml_Townspeople(): Points != (node=" + reader.Name + ")");
                  return false;
               }
               string? sCount0 = reader.GetAttribute("count");
               if (null == sCount0)
               {
                  Logger.Log(LogEnum.LE_ERROR, "ReadXml_Townspeople(): GetAttribute(sCount0)=null");
                  return false;
               }
               int count0 = int.Parse(sCount0);
               for (int i1 = 0; i1 < count0; ++i1)
               {
                  reader.Read();
                  if (false == reader.IsStartElement())
                  {
                     Logger.Log(LogEnum.LE_ERROR, "ReadXml_Townspeople(): IsStartElement(regionPoint)=false");
                     return false;
                  }
                  if (reader.Name != "regionPoint")
                  {
                     Logger.Log(LogEnum.LE_ERROR, "ReadXml_Townspeople(): regionPoint != (node=" + reader.Name + ")");
                     return false;
                  }
                  string? sX1 = reader.GetAttribute("X");
                  if (null == sX1)
                  {
                     Logger.Log(LogEnum.LE_ERROR, "ReadXml_Townspeople(): GetAttribute(sX1)=null");
                     return false;
                  }
                  string? sY1 = reader.GetAttribute("Y");
                  if (null == sY1)
                  {
                     Logger.Log(LogEnum.LE_ERROR, "ReadXml_Townspeople(): GetAttribute(sY1)=null");
                     return false;
                  }
                  double x = double.Parse(sX1);
                  double y = double.Parse(sY1);
                  IMapPoint mp = new MapPoint(x, y);
                  territory.Points.Add(mp);
               }
               if (0 < count0)
                  reader.Read(); // get past </Points> tag
               //--------------------------------------
               reader.Read();
               if (false == reader.IsStartElement())
               {
                  Logger.Log(LogEnum.LE_ERROR, "ReadXml_Townspeople(): IsStartElement(Adjacents)=false");
                  return false;
               }
               if (reader.Name != "Adjacents")
               {
                  Logger.Log(LogEnum.LE_ERROR, "ReadXml_Townspeople(): Adjacents != (node=" + reader.Name + ")");
                  return false;
               }
               string? sCount3 = reader.GetAttribute("count");
               if (null == sCount3)
               {
                  Logger.Log(LogEnum.LE_ERROR, "ReadXml_Townspeople(): GetAttribute(sCount3)=null");
                  return false;
               }
               int count3 = int.Parse(sCount3);
               for (int i3 = 0; i3 < count3; ++i3)
               {
                  reader.Read();
                  if (false == reader.IsStartElement())
                  {
                     Logger.Log(LogEnum.LE_ERROR, "ReadXml_Townspeople(): IsStartElement(adjacent)=false");
                     return false;
                  }
                  if (reader.Name != "adjacent")
                  {
                     Logger.Log(LogEnum.LE_ERROR, "ReadXml_Townspeople(): adjacent != (node=" + reader.Name + ")");
                     return false;
                  }
                  string? sAdjacent = reader.GetAttribute("value");
                  if (null == sAdjacent)
                  {
                     Logger.Log(LogEnum.LE_ERROR, "ReadXml_Townspeople(): GetAttribute(sAdjacent)=null");
                     return false;
                  }
                  territory.Adjacents.Add(sAdjacent);
               }
               if (0 < count3)
                  reader.Read(); // get past </Adjacents> tag
                                 //--------------------------------------
               reader.Read();
               if (false == reader.IsStartElement())
               {
                  Logger.Log(LogEnum.LE_ERROR, "ReadXml_Townspeople(): IsStartElement(PavedRoads)=false");
                  return false;
               }
               if (reader.Name != "PavedRoads")
               {
                  Logger.Log(LogEnum.LE_ERROR, "ReadXml_Townspeople(): PavedRoads != (node=" + reader.Name + ")");
                  return false;
               }
               string? sCount4 = reader.GetAttribute("count");
               if (null == sCount4)
               {
                  Logger.Log(LogEnum.LE_ERROR, "ReadXml_Townspeople(): GetAttribute(sCount4)=null");
                  return false;
               }
               int count4 = int.Parse(sCount4);
               for (int i4 = 0; i4 < count4; ++i4)
               {
                  reader.Read();
                  if (false == reader.IsStartElement())
                  {
                     Logger.Log(LogEnum.LE_ERROR, "ReadXml_Townspeople(): IsStartElement(paved)=false");
                     return false;
                  }
                  if (reader.Name != "paved")
                  {
                     Logger.Log(LogEnum.LE_ERROR, "ReadXml_Townspeople(): paved != (node=" + reader.Name + ")");
                     return false;
                  }
                  string? sPaved = reader.GetAttribute("value");
                  if (null == sPaved)
                  {
                     Logger.Log(LogEnum.LE_ERROR, "ReadXml_Townspeople(): GetAttribute(sPaved)=null");
                     return false;
                  }
                  territory.PavedRoads.Add(sPaved);
               }
               if (0 < count4)
                  reader.Read(); // get past </PavedRoads> tag
                                 //--------------------------------------
               reader.Read();
               if (false == reader.IsStartElement())
               {
                  Logger.Log(LogEnum.LE_ERROR, "ReadXml_Townspeople(): IsStartElement(UnpavedRoads)=false");
                  return false;
               }
               if (reader.Name != "UnpavedRoads")
               {
                  Logger.Log(LogEnum.LE_ERROR, "ReadXml_Townspeople(): UnpavedRoads != (node=" + reader.Name + ")");
                  return false;
               }
               string? sCount5 = reader.GetAttribute("count");
               if (null == sCount5)
               {
                  Logger.Log(LogEnum.LE_ERROR, "ReadXml_Townspeople(): GetAttribute(sCount5)=null");
                  return false;
               }
               int count5 = int.Parse(sCount5);
               for (int i5 = 0; i5 < count5; ++i5)
               {
                  reader.Read();
                  if (false == reader.IsStartElement())
                  {
                     Logger.Log(LogEnum.LE_ERROR, "ReadXml_Townspeople(): IsStartElement(unpaved)=false");
                     return false;
                  }
                  if (reader.Name != "unpaved")
                  {
                     Logger.Log(LogEnum.LE_ERROR, "ReadXml_Townspeople(): unpaved != (node=" + reader.Name + ")");
                     return false;
                  }
                  string? sUnpaved = reader.GetAttribute("value");
                  if (null == sUnpaved)
                  {
                     Logger.Log(LogEnum.LE_ERROR, "ReadXml_Townspeople(): GetAttribute(sUnpaved)=null");
                     return false;
                  }
                  territory.UnpavedRoads.Add(sUnpaved);
               }
               if (0 < count5)
                  reader.Read(); // get past </UnpavedRoads> tag
                                 //--------------------------------------
               territories.Add(territory);
               reader.Read(); // get past </Territory> tag
            }
            if (0 < count)
               reader.Read(); // get past </Territories> tag
         }
         finally
         {
            System.Threading.Thread.CurrentThread.CurrentCulture = currentCulture;
         }
         System.Threading.Thread.CurrentThread.CurrentCulture = currentCulture;
         return true;
      }
      public bool CreateXmlTownspeople(XmlDocument aXmlDocument, IMapItems townspersons) // initial creation of Territories.theTerritories during unit testing
      {
         CultureInfo currentCulture = CultureInfo.CurrentCulture;
         System.Threading.Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
         try
         {
            XmlNode? root = aXmlDocument.DocumentElement;
            if (null == root)
            {
               Logger.Log(LogEnum.LE_ERROR, "CreateXml_Townspeople(): root is null");
               return false;
            }
            XmlAttribute xmlAttribute = aXmlDocument.CreateAttribute("count");
            xmlAttribute.Value = townspersons.Count.ToString();
            if (null == root.Attributes)
            {
               Logger.Log(LogEnum.LE_ERROR, "CreateXml_Townspeople(): root.Attributes is null");
               return false;
            }
            root.Attributes.Append(xmlAttribute);
            //--------------------------------
            foreach (IMapItem mi in townspersons)
            {
               XmlElement? terrElem = aXmlDocument.CreateElement("MapItem");
               if (null == terrElem)
               {
                  Logger.Log(LogEnum.LE_ERROR, "CreateXml_Townspeople(): CreateElement(MapItem) returned null");
                  return false;
               }
               terrElem.SetAttribute("value", mi.Name);
               XmlNode? territoryNode = root.AppendChild(terrElem);
               if (null == territoryNode)
               {
                  Logger.Log(LogEnum.LE_ERROR, "CreateXml_Townspeople(): AppendChild(territoryNode) returned null");
                  return false;
               }
               //---------------------------------
               XmlElement? elem = aXmlDocument.CreateElement("Territory");
               if (null == elem)
               {
                  Logger.Log(LogEnum.LE_ERROR, "CreateXml_Townspeople(): CreateElement(Territory) returned null");
                  return false;
               }
               elem.SetAttribute("value", mi.TerritoryCurrent.Name);
               XmlNode? node = territoryNode.AppendChild(elem);
               if (null == node)
               {
                  Logger.Log(LogEnum.LE_ERROR, "CreateXml_Townspeople(): AppendChild(node) returned null");
                  return false;
               }
               //---------------------------------
               elem = aXmlDocument.CreateElement("Sector");
               if (null == elem)
               {
                  Logger.Log(LogEnum.LE_ERROR, "CreateXml_Townspeople(): CreateElement(Sector) returned null");
                  return false;
               }
               elem.SetAttribute("value", mi.TerritoryCurrent.Sector.ToString());
               node = territoryNode.AppendChild(elem);
               if (null == node)
               {
                  Logger.Log(LogEnum.LE_ERROR, "CreateXml_Townspeople(): AppendChild(node) returned null");
                  return false;
               }
               //---------------------------------
               elem = aXmlDocument.CreateElement("Movement");
               if (null == elem)
               {
                  Logger.Log(LogEnum.LE_ERROR, "CreateXml_Townspeople(): CreateElement(Movement) returned null");
                  return false;
               }
               elem.SetAttribute("value", mi.Movement.ToString());
               node = territoryNode.AppendChild(elem);
               if (null == node)
               {
                  Logger.Log(LogEnum.LE_ERROR, "CreateXml_Townspeople(): AppendChild(node) returned null");
                  return false;
               }
               //---------------------------------
               elem = aXmlDocument.CreateElement("Influence");
               if (null == elem)
               {
                  Logger.Log(LogEnum.LE_ERROR, "CreateXml_Townspeople(): CreateElement(Influence) returned null");
                  return false;
               }
               elem.SetAttribute("value", mi.Influence.ToString());
               node = territoryNode.AppendChild(elem);
               if (null == node)
               {
                  Logger.Log(LogEnum.LE_ERROR, "CreateXml_Townspeople(): AppendChild(node) returned null");
                  return false;
               }
               //---------------------------------
               elem = aXmlDocument.CreateElement("Combat");
               if (null == elem)
               {
                  Logger.Log(LogEnum.LE_ERROR, "CreateXml_Townspeople(): CreateElement(Combat) returned null");
                  return false;
               }
               elem.SetAttribute("value", mi.Combat.ToString());
               node = territoryNode.AppendChild(elem);
               if (null == node)
               {
                  Logger.Log(LogEnum.LE_ERROR, "CreateXml_Townspeople(): AppendChild(node) returned null");
                  return false;
               }
            }
         }
         finally
         {

         }
         return true;
      }
      private GameAction StringToGameAction(string sGameAction)
      {
         switch (sGameAction)
         {
            case "RemoveSplashScreen": return GameAction.RemoveSplashScreen;
            case "UpdateStatusBar": return GameAction.UpdateStatusBar;
            case "UpdateTankCard": return GameAction.UpdateTankCard;
            case "UpdateAfterActionReport": return GameAction.UpdateAfterActionReport;
            case "UpdateBattleBoard": return GameAction.UpdateBattleBoard;
            case "UpdateTankExplosion": return GameAction.UpdateTankExplosion;
            case "UpdateTankBrewUp": return GameAction.UpdateTankBrewUp;
            case "UpdateShowRegion": return GameAction.UpdateShowRegion;
            case "UpdateEventViewerDisplay": return GameAction.UpdateEventViewerDisplay;
            case "UpdateEventViewerActive": return GameAction.UpdateEventViewerActive;
            case "DieRollActionNone": return GameAction.DieRollActionNone;

            case "UpdateNewGame": return GameAction.UpdateNewGame;
            case "UpdateNewGameEnd": return GameAction.UpdateNewGameEnd;
            case "UpdateGameOptions": return GameAction.UpdateGameOptions;
            case "UpdateLoadingGame": return GameAction.UpdateLoadingGame;
            case "UpdateUndo": return GameAction.UpdateUndo;
            default: Logger.Log(LogEnum.LE_ERROR, " String_ToGameAction(): reached default sGameAction=" + sGameAction); return GameAction.Error;
         }
      }
      //--------------------------------------------------
      private IGameInstance? ReadXmlGameInstance(string filename)
      {
         IGameInstance gi = new GameInstance();
         IMapItems mapItems1 = new MapItems();
         ITerritories territories = new Territories();
         XmlTextReader? reader = null;
         try
         {
            // Load the reader with the data file and ignore all white space nodes.
            reader = new XmlTextReader(filename) { WhitespaceHandling = WhitespaceHandling.None };
            reader.Read();
            if (false == reader.IsStartElement())
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXml_GameInstance(): IsStartElement(GameInstance) returned false");
               return null;
            }
            if (reader.Name != "GameInstance")
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXml_GameInstance(): first node is not GameInstance");
               return null;
            }
            //----------------------------------------------
            reader.Read();
            if (false == reader.IsStartElement())
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXml_GameInstance(): IsStartElement(Guid) returned false");
               return null;
            }
            if (reader.Name != "Guid")
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXml_GameInstance(): Guid != (node=" + reader.Name + ")");
               return null;
            }
            string? sGuid = reader.GetAttribute("value");
            if (null == sGuid)
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXml_GameInstance(): sGuid=null");
               return null;
            }
            gi.GameGuid = Guid.Parse(sGuid);
            //----------------------------------------------
            reader.Read();
            if (false == reader.IsStartElement())
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXml_GameInstance(): IsStartElement(Version) returned false");
               return null;
            }
            if (reader.Name != "Version")
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXml_GameInstance(): Version != (node=" + reader.Name + ")");
               return null;
            }
            string? sVersion = reader.GetAttribute("value");
            if (null == sVersion)
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXml_GameInstance(): version=null");
               return null;
            }
            int version = int.Parse(sVersion);
            if (version != GetMajorVersion())
            {
               System.Windows.MessageBox.Show("Unable to open due to version mismatch. File v" + version + " does not match running v" + GetMajorVersion() + ".");
               return null;
            }
            //----------------------------------------------
            if (false == ReadXmlListingMapItems(reader))
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXml_GameInstance(): ReadXmlListingMapItems() returned false");
               return null;
            }
            //----------------------------------------------
            if (false == ReadXmlGameCommands(reader, gi.GameCommands))
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXml_GameInstance(): ReadXmlGameCommands() returned false");
               return null;
            }
            //----------------------------------------------
            if (false == ReadXmlOptions(reader, gi.Options))
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXml_GameInstance(): ReadXmlOptions() returned false");
               return null;
            }
            //----------------------------------------------
            if (false == ReadXmlGameStatistics(reader, gi.Statistics))
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXml_GameInstance(): ReadXmlGameStatistics() returned false");
               return null;
            }
            //----------------------------------------------
            if (false == ReadXmlEnteredHexes(reader, gi.EnteredHexes))
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXml_GameInstance(): ReadXmlEnteredHexes() returned false");
               return null;
            }
            return gi;
         } // try
         //==========================================
         catch (Exception e)
         {
            Logger.Log(LogEnum.LE_ERROR, "ReadXml_GameInstance():\n" + e.ToString());
            return null;
         }
         finally
         {
            if (reader != null)
               reader.Close();
         }
      }
      private bool ReadXmlListingMapItems(XmlReader reader)
      {
         theMapItems.Clear();
         reader.Read();
         if (false == reader.IsStartElement())
         {
            Logger.Log(LogEnum.LE_ERROR, "ReadXmlListingMapItems(): IsStartElement(MapItems)=null");
            return false;
         }
         if (reader.Name != "MapItems")
         {
            Logger.Log(LogEnum.LE_ERROR, "ReadXmlListingMapItems(): MapItems != (node=" + reader.Name + ")");
            return false;
         }
         string? sNumber = reader.GetAttribute("count");
         if (null == sNumber)
         {
            Logger.Log(LogEnum.LE_ERROR, "ReadXmlListingMapItems(): Count=null");
            return false;
         }
         int number = int.Parse(sNumber);
         //=================================
         for (int i = 0; i < number; ++i)
         {
            reader.Read();
            if (false == reader.IsStartElement())
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXmlListingMapItems(): reader.IsStartElement(Name) = false");
               return false;
            }
            if (reader.Name != "MapItem")
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXmlListingMapItems(): Name != (node=" + reader.Name + ")");
               return false;
            }
            string? sName = reader.GetAttribute("value");
            if (null == sName)
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXmlListingMapItems(): sName=null");
               return false;
            }
            IMapItem mi = new MapItem(sName);
            //---------------------------------------------
            reader.Read();
            if (false == reader.IsStartElement())
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXmlListingMapItems(): reader.IsStartElement(TopImageName) = false");
               return false;
            }
            if (reader.Name != "TopImageName")
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXmlListingMapItems(): TopImageName != (node=" + reader.Name + ")");
               return false;
            }
            string? sTopImageName = reader.GetAttribute("value");
            if (null == sTopImageName)
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXmlListingMapItems(): sTopImageName=null");
               return false;
            }
            mi.TopImageName = sTopImageName;
            MapItem.theMapImages.GetBitmapImage(sTopImageName); // map images should be loaded in memory for MapItem already created
            //---------------------------------------------
            reader.Read();
            if (false == reader.IsStartElement())
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXmlListingMapItems(): reader.IsStartElement(BottomImageName) = false");
               return false;
            }
            if (reader.Name != "BottomImageName")
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXmlListingMapItems(): BottomImageName != (node=" + reader.Name + ")");
               return false;
            }
            string? sBottomImageName = reader.GetAttribute("value");
            if (null == sBottomImageName)
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXmlListingMapItems(): BottomImageName=null");
               return false;
            }
            mi.BottomImageName = sBottomImageName;
            //---------------------------------------------
            reader.Read();
            if (false == reader.IsStartElement())
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXmlListingMapItems(): reader.IsStartElement(OverlayImageName) = false");
               return false;
            }
            if (reader.Name != "OverlayImageName")
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXmlListingMapItems(): OverlayImageName != (node=" + reader.Name + ")");
               return false;
            }
            string? sOverlayImageName = reader.GetAttribute("value");
            if (null == sOverlayImageName)
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXmlListingMapItems(): sOverlayImageName=null");
               return false;
            }
            mi.OverlayImageName = sOverlayImageName;
            //---------------------------------------------
            if (false == ReadXmlListingMapItemsWoundSpots(reader, mi.WoundSpots))
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXmlListingMapItems(): ReadXmlListingMapItemsWoundSpots() returned false");
               return false;
            }
            //---------------------------------------------
            reader.Read();
            if (false == reader.IsStartElement())
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXmlListingMapItems(): reader.IsStartElement(Zoom) = false");
               return false;
            }
            if (reader.Name != "Zoom")
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXmlListingMapItems(): Zoom != (node=" + reader.Name + ")");
               return false;
            }
            string? sZoom = reader.GetAttribute("value");
            if (null == sZoom)
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXmlListingMapItems(): sZoom=null");
               return false;
            }
            mi.Zoom = Convert.ToDouble(sZoom);
            //---------------------------------------------
            reader.Read();
            if (false == reader.IsStartElement())
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXmlListingMapItems(): reader.IsStartElement(IsMoved) = false");
               return false;
            }
            if (reader.Name != "IsMoved")
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXmlListingMapItems(): IsMoved != (node=" + reader.Name + ")");
               return false;
            }
            string? sIsMoved = reader.GetAttribute("value");
            if (null == sIsMoved)
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXmlListingMapItems(): IsMoved=null");
               return false;
            }
            mi.IsMoved = Convert.ToBoolean(sIsMoved);
            //---------------------------------------------
            reader.Read();
            if (false == reader.IsStartElement())
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXmlListingMapItems(): reader.IsStartElement(Count) = false");
               return false;
            }
            if (reader.Name != "Count")
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXmlListingMapItems(): Count != (node=" + reader.Name + ")");
               return false;
            }
            string? sCount = reader.GetAttribute("value");
            if (null == sCount)
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXmlListingMapItems(): sCount=null");
               return false;
            }
            mi.Count = Convert.ToInt32(sCount);
            //---------------------------------------------
            reader.Read();
            if (false == reader.IsStartElement())
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXmlListingMapItems(): reader.IsStartElement(LocationX) = false");
               return false;
            }
            if (reader.Name != "LocationX")
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXmlListingMapItems(): LocationX != (node=" + reader.Name + ")");
               return false;
            }
            string? sLocationX = reader.GetAttribute("value");
            if (null == sLocationX)
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXmlListingMapItems(): sLocationX=null");
               return false;
            }
            double x = Convert.ToDouble(sLocationX);
            reader.Read();
            if (false == reader.IsStartElement())
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXmlListingMapItems(): reader.IsStartElement(LocationY) = false");
               return false;
            }
            if (reader.Name != "LocationY")
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXmlListingMapItems(): LocationY != (node=" + reader.Name + ")");
               return false;
            }
            string? sLocationY = reader.GetAttribute("value");
            if (null == sLocationY)
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXmlListingMapItems(): sLocationY=null");
               return false;
            }
            double y = Convert.ToDouble(sLocationY);
            mi.Location = new MapPoint(x, y);
            //---------------------------------------------
            reader.Read();
            if (false == reader.IsStartElement())
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXmlListingMapItems(): reader.IsStartElement(TerritoryCurrent) = false");
               return false;
            }
            if (reader.Name != "TerritoryCurrent")
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXmlListingMapItems(): TerritoryCurrent != (node=" + reader.Name + ")");
               return false;
            }
            string? sTerritoryCurrentName = reader.GetAttribute("value");
            if (null == sTerritoryCurrentName)
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXmlListingMapItems(): sTerritoryCurrentName=null");
               return false;
            }
            string? sTerritoryCurrentType = reader.GetAttribute("type");
            if (null == sTerritoryCurrentType)
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXmlListingMapItems(): sTerritoryCurrentType=null");
               return false;
            }
            if ("Offboard" == sTerritoryCurrentName)
            {
               mi.TerritoryCurrent = new Territory();
            }
            else
            {
               ITerritory? tCurrent = Territories.theTerritories.Find(sTerritoryCurrentName, sTerritoryCurrentType);
               if (null == tCurrent)
               {
                  Logger.Log(LogEnum.LE_ERROR, "ReadXmlListingMapItems(): tCurrent=null for sTerritoryCurrentName=" + sTerritoryCurrentName + " sTerritoryCurrentType=" + sTerritoryCurrentType);
                  return false;
               }
               mi.TerritoryCurrent = tCurrent;
            }
            //---------------------------------------------
            reader.Read();
            if (false == reader.IsStartElement())
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXmlListingMapItems(): reader.IsStartElement(TerritoryStarting) = false");
               return false;
            }
            if (reader.Name != "TerritoryStarting")
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXmlListingMapItems(): TerritoryStarting != (node=" + reader.Name + ")");
               return false;
            }
            string? sTerritoryStartingName = reader.GetAttribute("value");
            string? sTerritoryStartingType = reader.GetAttribute("type");
            if (null == sTerritoryStartingName)
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXmlListingMapItems(): sTerritoryStartingName=null");
               return false;
            }
            if (null == sTerritoryStartingType)
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXmlListingMapItems(): sTerritoryStartingType=null");
               return false;
            }
            if ("Offboard" == sTerritoryStartingName)
            {
               mi.TerritoryStarting = mi.TerritoryCurrent;
            }
            else
            {
               ITerritory? tStart = Territories.theTerritories.Find(sTerritoryStartingName, sTerritoryStartingType);
               if (null == tStart)
               {
                  Logger.Log(LogEnum.LE_ERROR, "ReadXmlListingMapItems(): tStart=null for sTerritoryStartingName=" + sTerritoryStartingName + " sTerritoryStartingType=" + sTerritoryStartingType);
                  return false;
               }
               mi.TerritoryStarting = tStart;
            }

            reader.Read(); // get past </MapItem>
            theMapItems.Add(mi);
         }
         if (0 < number)
            reader.Read(); // get past </MapItems>
         return true;
      }
      private bool ReadXmlListingMapItemsWoundSpots(XmlReader reader, List<BloodSpot> bloodSpots)
      {
         bloodSpots.Clear();
         reader.Read();
         if (false == reader.IsStartElement())
         {
            Logger.Log(LogEnum.LE_ERROR, "ReadXmlListingMapItemsWoundSpots(): reader.IsStartElement(WoundSpots) = false");
            return false;
         }
         if (reader.Name != "WoundSpots")
         {
            Logger.Log(LogEnum.LE_ERROR, "ReadXmlListingMapItemsWoundSpots(): WoundSpots != (node=" + reader.Name + ")");
            return false;
         }
         string? sCount = reader.GetAttribute("count");
         if (null == sCount)
         {
            Logger.Log(LogEnum.LE_ERROR, "ReadXmlListingMapItemsWoundSpots(): Count=null");
            return false;
         }
         int count = int.Parse(sCount);
         for (int i = 0; i < count; ++i)
         {

            //---------------------------------------------
            reader.Read();
            if (false == reader.IsStartElement())
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXmlListingMapItemsWoundSpots(): reader.IsStartElement(WoundSpot) = false");
               return false;
            }
            if (reader.Name != "WoundSpot")
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXmlListingMapItemsWoundSpots(): WoundSpot != (node=" + reader.Name + ")");
               return false;
            }
            //---------------------------------------------
            string? sSize = reader.GetAttribute("size");
            if (null == sSize)
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXmlListingMapItemsWoundSpots(): sSize=null");
               return false;
            }
            int size = Convert.ToInt32(sSize);
            //---------------------------------------------
            string? sLeft = reader.GetAttribute("left");
            if (null == sLeft)
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXmlListingMapItemsWoundSpots(): sLeft=null");
               return false;
            }
            double left = Convert.ToInt32(sLeft);
            //---------------------------------------------
            string? sTop = reader.GetAttribute("top");
            if (null == sTop)
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXmlListingMapItemsWoundSpots(): sTop=null");
               return false;
            }
            double top = Convert.ToInt32(sTop);
            //---------------------------------------------
            BloodSpot bloodSpot = new BloodSpot(size, left, top);
            bloodSpots.Add(bloodSpot);
         }
         if (0 < count)
            reader.Read(); // get past </WoundSpots> tag
         return true;
      }
      private bool ReadXmlListingMapItemsEnemyAcquiredShots(XmlReader reader, Dictionary<string, int> enemyAcquiredShots)
      {
         enemyAcquiredShots.Clear();
         reader.Read();
         if (false == reader.IsStartElement())
         {
            Logger.Log(LogEnum.LE_ERROR, "ReadXmlListingMapItemsEnemyAcquiredShots(): IsStartElement(EnemyAcquiredShots) returned false");
            return false;
         }
         if (reader.Name != "EnemyAcquiredShots")
         {
            Logger.Log(LogEnum.LE_ERROR, "ReadXmlListingMapItemsEnemyAcquiredShots(): EnemyAcquiredShots != (node=" + reader.Name + ")");
            return false;
         }
         string? sCount = reader.GetAttribute("count");
         if (null == sCount)
         {
            Logger.Log(LogEnum.LE_ERROR, "ReadXmlListingMapItemsEnemyAcquiredShots(): Count=null");
            return false;
         }
         int count = int.Parse(sCount);
         for (int i = 0; i < count; i++)
         {
            reader.Read();
            if (false == reader.IsStartElement())
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXmlListingMapItemsEnemyAcquiredShots(): reader.IsStartElement(EnemyAcqShot) = false");
               return false;
            }
            if (reader.Name != "EnemyAcqShot")
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXmlListingMapItemsEnemyAcquiredShots(): IsSpotted != (node=" + reader.Name + ")");
               return false;
            }
            string? sEnemy = reader.GetAttribute("enemy");
            if (null == sEnemy)
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXmlListingMapItemsEnemyAcquiredShots(): sEnemy=null");
               return false;
            }
            string? sValue = reader.GetAttribute("value");
            if (null == sValue)
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXmlListingMapItemsEnemyAcquiredShots(): sValue=null");
               return false;
            }
            enemyAcquiredShots[sEnemy] = Convert.ToInt32(sValue);
         }
         if (0 < count)
            reader.Read(); // get past </EnemyAcquiredShots> tag
         return true;
      }
      private bool ReadXmlGameCommands(XmlReader reader, IGameCommands gameCmds)
      {
         gameCmds.Clear();
         reader.Read();
         if (false == reader.IsStartElement())
         {
            Logger.Log(LogEnum.LE_ERROR, "ReadXmlGameCommands(): reader.IsStartElement(GameCommands) = false");
            return false;
         }
         if (reader.Name != "GameCommands")
         {
            Logger.Log(LogEnum.LE_ERROR, "ReadXmlGameCommands(): GameCommands != (node=" + reader.Name + ")");
            return false;
         }
         string? sCount = reader.GetAttribute("count");
         if (null == sCount)
         {
            Logger.Log(LogEnum.LE_ERROR, "ReadXmlGameCommands(): Count=null");
            return false;
         }
         //-------------------------------------
         int count = int.Parse(sCount);
         for (int i = 0; i < count; ++i)
         {
            reader.Read();
            if (false == reader.IsStartElement())
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXmlGameCommands(): reader.IsStartElement(GameCommand) = false");
               return false;
            }
            if (reader.Name != "GameCommand")
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXmlGameCommands(): GameCommand != (node=" + reader.Name + ")");
               return false;
            }
            string? sAction = reader.GetAttribute("Action");
            if (sAction == null)
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXmlGameCommands(): sAction=null");
               return false;
            }
            GameAction action = StringToGameAction(sAction);
            if (GameAction.Error == action)
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXmlGameCommands(): String_ToGameAction() returned false");
               return false;
            }
            //------------------------------------
            string? sActionDieRoll = reader.GetAttribute("ActionDieRoll");
            if (sActionDieRoll == null)
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXmlGameCommands(): sActionDieRoll=null");
               return false;
            }
            GameAction dieRollAction = StringToGameAction(sActionDieRoll);
            if (GameAction.Error == dieRollAction)
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXmlGameCommands(): String_ToGameAction() returned false");
               return false;
            }
            //------------------------------------
            string? sEventActive = reader.GetAttribute("EventActive");
            if (sEventActive == null)
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXmlGameCommands(): sEventActive=null");
               return false;
            }
            //------------------------------------
            string? sGamePhase = reader.GetAttribute("Phase");
            if (null == sGamePhase)
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXmlGameCommands(): sGamePhase=null");
               return false;
            }
            GamePhase phase = GamePhase.Error;
            switch (sGamePhase)
            {
               case "UnitTests": phase = GamePhase.UnitTests; break;
               case "GameSetup": phase = GamePhase.GameSetup; break;
               default: Logger.Log(LogEnum.LE_ERROR, "ReadXmlGameCommands(): reached default sGamePhase=" + sGamePhase); return false;
            }

            //------------------------------------
            IGameCommand gameCmd = new GameCommand(phase, dieRollAction, sEventActive, action);
            gameCmds.Add(gameCmd);
         }
         if (0 < count)
            reader.Read(); // get past </GameCommands>
         return true;
      }
      private bool ReadXmlOptions(XmlReader reader, Options options)
      {
         options.Clear();
         reader.Read();
         if (false == reader.IsStartElement())
         {
            Logger.Log(LogEnum.LE_ERROR, "ReadXmlOptions(): reader.IsStartElement(Options) = false");
            return false;
         }
         if (reader.Name != "Options")
         {
            Logger.Log(LogEnum.LE_ERROR, "ReadXmlOptions(): Options != (node=" + reader.Name + ")");
            return false;
         }
         string? sCount = reader.GetAttribute("count");
         if (null == sCount)
         {
            Logger.Log(LogEnum.LE_ERROR, "ReadXmlOptions(): Count=null");
            return false;
         }
         //-------------------------------------
         int count = int.Parse(sCount);
         for (int i = 0; i < count; ++i)
         {
            reader.Read();
            if (false == reader.IsStartElement())
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXmlOptions(): IsStartElement(Option) returned false");
               return false;
            }
            if (reader.Name != "Option")
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXmlOptions(): Option != " + reader.Name);
               return false;
            }
            string? name = reader.GetAttribute("Name");
            if (name == null)
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXmlOptions(): Name=null");
               return false;
            }
            string? sEnabled = reader.GetAttribute("IsEnabled");
            if (sEnabled == null)
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXmlOptions(): IsEnabled=null");
               return false;
            }
            bool isEnabled = bool.Parse(sEnabled);
            Option option = new Option(name, isEnabled);
            options.Add(option);
         }
         if (0 < count)
            reader.Read(); // get past </Options>
         return true;
      }
      private bool ReadXmlGameStatistics(XmlReader reader, GameStatistics statistics)
      {
         statistics.Clear();
         reader.Read();
         if (false == reader.IsStartElement())
         {
            Logger.Log(LogEnum.LE_ERROR, "Read_XmlGameStatistics(): reader.IsStartElement(GameStatistics) = false");
            return false;
         }
         if (reader.Name != "GameStatistics")
         {
            Logger.Log(LogEnum.LE_ERROR, "Read_XmlGameStatistics(): GameStatistics != (node=" + reader.Name + ")");
            return false;
         }
         string? sCount = reader.GetAttribute("count");
         if (null == sCount)
         {
            Logger.Log(LogEnum.LE_ERROR, "Read_XmlGameStatistics(): Count=null");
            return false;
         }
         //-------------------------------------
         int count = int.Parse(sCount);
         for (int i = 0; i < count; ++i)
         {
            reader.Read();
            if (false == reader.IsStartElement())
            {
               Logger.Log(LogEnum.LE_ERROR, "Read_XmlGameStatistics(): IsStartElement(GameStatistic) returned false");
               return false;
            }
            if (reader.Name != "GameStatistic")
            {
               Logger.Log(LogEnum.LE_ERROR, "Read_XmlGameStatistics(): GameStatistic != " + reader.Name);
               return false;
            }
            string? key = reader.GetAttribute("Key");
            if (key == null)
            {
               Logger.Log(LogEnum.LE_ERROR, "Read_XmlGameStatistics(): key=null");
               return false;
            }
            string? sValue = reader.GetAttribute("Value");
            if (sValue == null)
            {
               Logger.Log(LogEnum.LE_ERROR, "Read_XmlGameStatistics(): sValue=null");
               return false;
            }
            int value = Int32.Parse(sValue);
            GameStatistic stat = new GameStatistic(key, value);
            statistics.Add(stat);
         }
         if (0 < count)
            reader.Read(); // get past </GameStatistics>
         return true;
      }
      private bool ReadXmlMapItems(XmlReader reader, IMapItems mapItems, string attribute)
      {
         mapItems.Clear();
         reader.Read();
         if (false == reader.IsStartElement())
         {
            Logger.Log(LogEnum.LE_ERROR, "ReadXml_MapItems(): IsStartElement(MapItems)=null");
            return false;
         }
         if (reader.Name != "MapItems")
         {
            Logger.Log(LogEnum.LE_ERROR, "ReadXml_MapItems(): MapItems != (node=" + reader.Name + ")");
            return false;
         }
         string? sAttribute = reader.GetAttribute("value");
         if (sAttribute != attribute)
         {
            Logger.Log(LogEnum.LE_ERROR, "ReadXml_MapItems(): (sAttribute=" + sAttribute + ") != (attribute=" + attribute + ")");
            return false;
         }
         string? sCount = reader.GetAttribute("count");
         if (null == sCount)
         {
            Logger.Log(LogEnum.LE_ERROR, "ReadXml_MapItems(): Count=null");
            return false;
         }
         int count = int.Parse(sCount);
         for (int i = 0; i < count; ++i)
         {
            IMapItem? mapItem = null;
            if (false == ReadXmlMapItem(reader, ref mapItem))
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXml_MapItems(): ReadXml_MapItem() returned false");
               return false;
            }
            if (null == mapItem)
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXml_MapItems(): mapItem=null");
               return false;
            }
            mapItems.Add(mapItem);
         }
         if (0 < count)
            reader.Read();
         return true;
      }
      private bool ReadXmlMapItem(XmlReader reader, ref IMapItem? mi)
      {
         reader.Read();
         if (false == reader.IsStartElement())
         {
            Logger.Log(LogEnum.LE_ERROR, "ReadXmlMapItem(): reader.IsStartElement(Name) = false");
            return false;
         }
         if (reader.Name != "MapItem")
         {
            Logger.Log(LogEnum.LE_ERROR, "ReadXmlMapItem(): Name != (node=" + reader.Name + ")");
            return false;
         }
         string? sValue = reader.GetAttribute("value");
         if (null == sValue)
         {
            Logger.Log(LogEnum.LE_ERROR, "ReadXmlMapItem(): sValue=null");
            return false;
         }
         string? sName = reader.GetAttribute("name");
         if (null == sName)
         {
            Logger.Log(LogEnum.LE_ERROR, "ReadXmlMapItem(): sName=null for sValue=" + sValue);
            return false;
         }
         if ("null" == sName)
         {
            mi = null;
         }
         else
         {
            mi = theMapItems.Find(sName);
            if (null == mi)
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXmlMapItem(): unable to find mapitem=" + sName);
               return false;
            }
         }
         return true;
      }
      private bool ReadXmlMapItemMoves(XmlReader reader, IMapItemMoves mapItemMoves)
      {
         mapItemMoves.Clear();
         reader.Read();
         if (false == reader.IsStartElement())
         {
            Logger.Log(LogEnum.LE_ERROR, "ReadXmlMapItemMoves(): IsStartElement(MapItemMoves) returned false");
            return false;
         }
         if (reader.Name != "MapItemMoves")
         {
            Logger.Log(LogEnum.LE_ERROR, "ReadXmlMapItemMoves(): MapItemMoves != (node=" + reader.Name + ")");
            return false;
         }
         string? sCount = reader.GetAttribute("count");
         if (null == sCount)
         {
            Logger.Log(LogEnum.LE_ERROR, "ReadXmlMapItemMoves(): count=null");
            return false;
         }
         int count = int.Parse(sCount);
         for (int i = 0; i < count; ++i)
         {
            IMapItemMove mim = new MapItemMove();
            reader.Read();
            if (false == reader.IsStartElement())
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXmlMapItemMoves(): IsStartElement(MapItemMove) returned false");
               return false;
            }
            if (reader.Name != "MapItemMove")
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXmlMapItemMoves(): MapItemMove != (node=" + reader.Name + ")");
               return false;
            }
            string? miName = reader.GetAttribute("value");
            if (null == miName)
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXmlMapItemMoves(): miName=null");
               return false;
            }
            //----------------------------------------------
            IMapItem? mi = null;
            if (false == ReadXmlMapItem(reader, ref mi))
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXmlMapItemMoves(): ReadXmlMapItem() returned false");
               return false;
            }
            if (null == mi)
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXmlMapItemMoves(): mi=null");
               return false;
            }
            mim.MapItem = mi;
            //----------------------------------------------
            reader.Read();
            if (false == reader.IsStartElement())
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXmlMapItemMoves(): IsStartElement(OldTerritory) returned false");
               return false;
            }
            if (reader.Name != "OldTerritory")
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXmlMapItemMoves(): OldTerritory != (node=" + reader.Name + ")");
               return false;
            }
            string? sOldTerritory = reader.GetAttribute("value");
            if ("null" == sOldTerritory)
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXmlMapItemMoves(): sOldTerritory=*null*");
               return false;
            }
            if (null == sOldTerritory)
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXmlMapItemMoves(): sOldTerritory=null");
               return false;
            }
            mim.OldTerritory = Territories.theTerritories.Find(sOldTerritory);
            if (null == mim.OldTerritory)
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXmlMapItemMoves(): sOldTerritory=null for name=" + sOldTerritory);
               return false;
            }
            //----------------------------------------------
            reader.Read();
            if (false == reader.IsStartElement())
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXmlMapItemMoves(): IsStartElement(NewTerritory) returned false");
               return false;
            }
            if (reader.Name != "NewTerritory")
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXmlMapItemMoves(): NewTerritory != (node=" + reader.Name + ")");
               return false;
            }
            string? sNewTerritory = reader.GetAttribute("value");
            if ("null" == sNewTerritory)
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXmlMapItemMoves(): sNewTerritory=*null*");
               return false;
            }
            if (null == sNewTerritory)
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXmlMapItemMoves(): sNewTerritory=null");
               return false;
            }
            mim.NewTerritory = Territories.theTerritories.Find(sNewTerritory);
            if (null == mim.NewTerritory)
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXmlMapItemMoves(): sOldTerritory=null for name=" + sNewTerritory);
               return false;
            }
            //----------------------------------------------
            IMapPath? path = null;
            if (false == ReadXmlMapItemMoveBestPath(reader, ref path))
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXmlMapItemMoves(): ReadXmlMapItemMoveBestPath() returned false");
               return false;
            }
            if (null == path)
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXmlMapItemMoves(): ReadXmlMapItemMoveBestPath() returned path=null");
               return false;
            }
            reader.Read(); // get past </MapItemMove>
            mim.BestPath = path;
            //----------------------------------------------
            mapItemMoves.Add(mim);
         }
         if (0 < count)
            reader.Read(); // get past </MapItemMoves>
         return true;
      }
      private bool ReadXmlMapItemMoveBestPath(XmlReader reader, ref IMapPath? path)
      {
         reader.Read();
         if (false == reader.IsStartElement())
         {
            Logger.Log(LogEnum.LE_ERROR, "ReadXmlMapItemMoveBestPath(): IsStartElement(BestPath) returned false");
            return false;
         }
         if (reader.Name != "BestPath")
         {
            Logger.Log(LogEnum.LE_ERROR, "ReadXmlMapItemMoveBestPath(): BestPath != (node=" + reader.Name + ")");
            return false;
         }
         string? sName = reader.GetAttribute("name");
         if (null == sName)
         {
            Logger.Log(LogEnum.LE_ERROR, "ReadXmlMapItemMoveBestPath(): sName=null");
            return false;
         }
         //------------------------------
         string? sMetric = reader.GetAttribute("metric");
         if (null == sMetric)
         {
            Logger.Log(LogEnum.LE_ERROR, "ReadXmlMapItemMoveBestPath(): sMetric=null");
            return false;
         }
         double metric = Convert.ToDouble(sMetric);
         //------------------------------
         string? sCount = reader.GetAttribute("count");
         if (null == sCount)
         {
            Logger.Log(LogEnum.LE_ERROR, "ReadXmlMapItemMoveBestPath(): sCount=null");
            return false;
         }
         int count = int.Parse(sCount);
         //------------------------------
         List<ITerritory> territories = new List<ITerritory>();
         for (int i = 0; i < count; ++i)
         {
            reader.Read();
            if (false == reader.IsStartElement())
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXmlMapItemMoveBestPath(): IsStartElement(Territory) returned false");
               return false;
            }
            if (reader.Name != "Territory")
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXmlMapItemMoveBestPath(): MapItemMoves != (node=" + reader.Name + ")");
               return false;
            }
            string? tName = reader.GetAttribute("name");
            if (null == tName)
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXmlMapItemMoveBestPath(): tName=null");
               return false;
            }
            ITerritory? t = Territories.theTerritories.Find(tName);
            if (null == t)
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXmlMapItemMoveBestPath(): tName=null for tName=" + tName);
               return false;
            }
            territories.Add(t);
         }
         if (0 < count)
            reader.Read(); // get past </BestPath>
         path = new MapPath(sName);
         path.Metric = metric;
         path.Territories = territories;
         return true;
      }
      private bool ReadXmlStacks(XmlReader reader, IStacks stacks, string attribute)
      {
         stacks.Clear();
         reader.Read();
         if (false == reader.IsStartElement())
         {
            Logger.Log(LogEnum.LE_ERROR, "ReadXmlStacks(): IsStartElement(Stacks) returned false");
            return false;
         }
         if (reader.Name != "Stacks")
         {
            Logger.Log(LogEnum.LE_ERROR, "ReadXmlStacks(): Stacks != (node=" + reader.Name + ")");
            return false;
         }
         string? sName = reader.GetAttribute("value");
         if (null == sName)
         {
            Logger.Log(LogEnum.LE_ERROR, "ReadXmlStacks(): sName=null for attribute=" + attribute);
            return false;
         }
         if (attribute != sName)
         {
            Logger.Log(LogEnum.LE_ERROR, "ReadXmlStacks(): sName=" + sName + " not equal to attribute=" + attribute);
            return false;
         }
         string? sCount = reader.GetAttribute("count");
         if (null == sCount)
         {
            Logger.Log(LogEnum.LE_ERROR, "ReadXmlStacks(): count=null");
            return false;
         }
         int count = int.Parse(sCount);
         //---------------------------------------------
         for (int i = 0; i < count; ++i)
         {
            reader.Read();
            if (false == reader.IsStartElement())
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXmlStacks(): IsStartElement(Stack) returned false");
               return false;
            }
            if (reader.Name != "Stack")
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXmlStacks(): Stack != (node=" + reader.Name + ")");
               return false;
            }
            string? tName = reader.GetAttribute("value");
            if (null == tName)
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXmlStacks(): tName=null");
               return false;
            }
            ITerritory? t = Territories.theTerritories.Find(tName);
            if (null == t)
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXmlStacks(): t=null for tName=" + tName);
               return false;
            }
            //---------------------------------------------
            reader.Read();
            if (false == reader.IsStartElement())
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXmlStacks(): IsStartElement(IsStacked) returned false");
               return false;
            }
            if (reader.Name != "IsStacked")
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXmlStacks(): IsStacked != (node=" + reader.Name + ")");
               return false;
            }
            string? sIsStacked = reader.GetAttribute("value");
            if (null == sIsStacked)
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXmlStacks(): sIsStacked=null");
               return false;
            }
            bool isStacked = Convert.ToBoolean(sIsStacked);
            //---------------------------------------------
            IMapItems mapItems = new MapItems();
            if (false == ReadXmlMapItems(reader, mapItems, tName))
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXmlStacks(): ReadXml_MapItems() returned false for tName=" + tName);
               return false;
            }
            //--------------------------------------------
            IStack stack = new Stack(t);
            stack.IsStacked = isStacked;
            stack.MapItems = mapItems;
            stacks.Add(stack);
            reader.Read(); // get past </Stack>
         }
         if (0 < count)
            reader.Read(); // get past </Stacks>
         return true;
      }
      private bool ReadXmlEnteredHexes(XmlReader reader, List<EnteredHex> hexes)
      {
         hexes.Clear();
         reader.Read();
         if (false == reader.IsStartElement())
         {
            Logger.Log(LogEnum.LE_ERROR, "ReadXmlEnteredHexes(): IsStartElement(EnteredHexes) returned false");
            return false;
         }
         if (reader.Name != "EnteredHexes")
         {
            Logger.Log(LogEnum.LE_ERROR, "ReadXmlEnteredHexes(): EnteredHexes != (node=" + reader.Name + ")");
            return false;
         }
         string? sCount = reader.GetAttribute("count");
         if (null == sCount)
         {
            Logger.Log(LogEnum.LE_ERROR, "ReadXmlEnteredHexes(): count=null");
            return false;
         }
         int count = int.Parse(sCount);
         for (int i = 0; i < count; ++i)
         {
            reader.Read();
            if (false == reader.IsStartElement())
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXmlEnteredHexes(): IsStartElement(EnteredHex) returned false");
               return false;
            }
            if (reader.Name != "EnteredHex")
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXmlEnteredHexes(): EnteredHexes != (node=" + reader.Name + ")");
               return false;
            }
            string? sId = reader.GetAttribute("value");
            if (null == sId)
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXmlEnteredHexes(): sId=null");
               return false;
            }
            //-----------------------------------
            reader.Read();
            if (false == reader.IsStartElement())
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXmlEnteredHexes(): IsStartElement(Day) returned false");
               return false;
            }
            if (reader.Name != "Day")
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXmlEnteredHexes(): Day != (node=" + reader.Name + ")");
               return false;
            }
            string? sDay = reader.GetAttribute("value");
            if (null == sDay)
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXmlEnteredHexes(): sDay=null");
               return false;
            }
            int day = Convert.ToInt32(sDay);
            //-----------------------------------
            reader.Read();
            if (false == reader.IsStartElement())
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXmlEnteredHexes(): IsStartElement(Date) returned false");
               return false;
            }
            if (reader.Name != "Date")
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXmlEnteredHexes(): Date != (node=" + reader.Name + ")");
               return false;
            }
            string? date = reader.GetAttribute("value");
            if (null == date)
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXmlEnteredHexes(): Date=null");
               return false;
            }
            //-----------------------------------
            reader.Read();
            if (false == reader.IsStartElement())
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXmlEnteredHexes(): IsStartElement(Time) returned false");
               return false;
            }
            if (reader.Name != "Time")
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXmlEnteredHexes(): Time != (node=" + reader.Name + ")");
               return false;
            }
            string? time = reader.GetAttribute("value");
            if (null == time)
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXmlEnteredHexes(): time=null");
               return false;
            }
            //-----------------------------------
            reader.Read();
            if (false == reader.IsStartElement())
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXmlEnteredHexes(): IsStartElement(TerritoryName) returned false");
               return false;
            }
            if (reader.Name != "TerritoryName")
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXmlEnteredHexes(): TerritoryName != (node=" + reader.Name + ")");
               return false;
            }
            string? territoryName = reader.GetAttribute("value");
            if (null == territoryName)
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXmlEnteredHexes(): territoryName=null");
               return false;
            }
            //-----------------------------------
            reader.Read();
            if (false == reader.IsStartElement())
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXmlEnteredHexes(): IsStartElement(MapPoint) returned false");
               return false;
            }
            if (reader.Name != "MapPoint")
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXmlEnteredHexes(): MapPoint != (node=" + reader.Name + ")");
               return false;
            }
            string? sX = reader.GetAttribute("X");
            if (null == sX)
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXmlEnteredHexes(): sX=null");
               return false;
            }
            double x = Convert.ToDouble(sX);
            string? sY = reader.GetAttribute("Y");
            if (null == sY)
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXmlEnteredHexes(): sY=null");
               return false;
            }
            double y = Convert.ToDouble(sY);
            IMapPoint mp = new MapPoint(x, y);
            //-----------------------------------
            reader.Read();
            if (false == reader.IsStartElement())
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXmlEnteredHexes(): IsStartElement(ColorAction) returned false");
               return false;
            }
            if (reader.Name != "ColorAction")
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXmlEnteredHexes(): ColorAction != (node=" + reader.Name + ")");
               return false;
            }
            string? sColorAction = reader.GetAttribute("value");
            if (null == sColorAction)
            {
               Logger.Log(LogEnum.LE_ERROR, "ReadXmlEnteredHexes(): sColorAction=null");
               return false;
            }
            ColorActionEnum colorAction = ColorActionEnum.CAE_START;
            switch (sColorAction)
            {
               case "CAE_START": colorAction = ColorActionEnum.CAE_START; break;
               case "CAE_ENTER": colorAction = ColorActionEnum.CAE_ENTER; break;
               case "CAE_RETREAT": colorAction = ColorActionEnum.CAE_RETREAT; break;
               case "CAE_STOP": colorAction = ColorActionEnum.CAE_STOP; break;
               default: Logger.Log(LogEnum.LE_ERROR, "ReadXmlEnteredHexes(): reached default sColorAction=" + sColorAction); return false;
            }
            reader.Read(); // get past </EnteredHex>
            EnteredHex hex = new EnteredHex(mp);
            hex.Identifer = sId;
            hex.Day = day;
            hex.Date = date;
            hex.Time = time;
            hex.TerritoryName = territoryName;
            hex.ColorAction = colorAction;
            hexes.Add(hex);
         }
         if (0 < count)
            reader.Read(); // get past </EnteredHexes>
         return true;
      }
      //--------------------------------------------------
      private XmlDocument? CreateXmlGameInstance(IGameInstance gi)
      {
         XmlDocument aXmlDocument = new XmlDocument();
         aXmlDocument.LoadXml("<GameInstance></GameInstance>");
         if (null == aXmlDocument.DocumentElement)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXml_GameInstance(): aXmlDocument.DocumentElement=null");
            return null;
         }
         XmlNode? root = aXmlDocument.DocumentElement;
         if (null == root)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXml_GameInstance(): root is null");
            return null;
         }
         //------------------------------------------
         XmlElement? guidElem = aXmlDocument.CreateElement("Guid");
         if (null == guidElem)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXml_GameInstance(): aXmlDocument.DocumentElement.LastChild=null");
            return null;
         }
         guidElem.SetAttribute("value", gi.GameGuid.ToString());
         XmlNode? guidNode = root.AppendChild(guidElem);
         if (null == guidNode)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXml_GameInstance(): AppendChild(guidNode) returned null");
            return null;
         }
         //------------------------------------------
         XmlElement? versionElem = aXmlDocument.CreateElement("Version");
         if (null == versionElem)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXml_GameInstance(): aXmlDocument.DocumentElement.LastChild=null");
            return null;
         }
         int majorVersion = GetMajorVersion();
         if (majorVersion < 0)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXml_GameInstance():  0 > majorVersion=" + majorVersion.ToString());
            return null;
         }
         versionElem.SetAttribute("value", majorVersion.ToString());
         XmlNode? versionNode = root.AppendChild(versionElem);
         if (null == versionNode)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXml_GameInstance(): AppendChild(versionNode) returned null");
            return null;
         }
         //------------------------------------------
         if (false == CreateXmlListingOfMapItems(aXmlDocument, gi))
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXml_GameInstance(): Create_XmlListingOfMapItems() returned false");
            return null;
         }
         //------------------------------------------
         if (false == CreateXmlGameCommands(aXmlDocument, gi.GameCommands))
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXml_GameInstance(): Create_XmlGameCommands() returned false");
            return null;
         }
         //------------------------------------------
         if (false == CreateXmlGameOptions(aXmlDocument, gi.Options))
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXml_GameInstance(): Create_XmlOptions() returned false");
            return null;
         }
         //------------------------------------------
         if (false == CreateXmlGameStatistics(aXmlDocument, gi.Statistics))
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXml_GameInstance(): Create_XmlGameStat() returned false");
            return null;
         }
         //------------------------------------------
         if (false == CreateXmlMapItemMoves(aXmlDocument, gi.MapItemMoves))
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXml_GameInstance(): CreateXmlMapItemMoves(MapItemMoves) returned false");
            return null;
         }
         //------------------------------------------
         if (false == CreateXmlStacks(aXmlDocument, gi.Stacks, "Stacks"))
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXml_GameInstance(): CreateXmlStacks(Stacks) returned false");
            return null;
         }
         //------------------------------------------
         if (false == CreateXmlEnteredHexes(aXmlDocument, gi.EnteredHexes))
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXml_GameInstance(): CreateXmlEnteredHexes() returned false");
            return null;
         }
         return aXmlDocument;
      }
      private bool CreateXmlListingOfMapItems(XmlDocument aXmlDocument, IGameInstance gi)
      {
         theMapItems.Clear();
         //-----------------------------------
         foreach (IMapItemMove mim in gi.MapItemMoves)
         {
            if (null == theMapItems.Find(mim.MapItem.Name))
               theMapItems.Add(mim.MapItem);
         }
         foreach (IStack stack in gi.Stacks)
         {
            foreach (IMapItem mi in stack.MapItems)
            {
               if (null == theMapItems.Find(mi.Name))
                  theMapItems.Add(mi);
            }
         }
         //======================================================
         XmlNode? root = aXmlDocument.DocumentElement;
         if (null == root)
         {
            Logger.Log(LogEnum.LE_ERROR, "Create_XmlListingOfMapItems(): root is null");
            return false;
         }
         XmlElement? mapItemsElem = aXmlDocument.CreateElement("MapItems");
         if (null == mapItemsElem)
         {
            Logger.Log(LogEnum.LE_ERROR, "Create_XmlListingOfMapItems(): CreateElement(MapItemsElem) returned null");
            return false;
         }
         mapItemsElem.SetAttribute("count", theMapItems.Count.ToString());
         XmlNode? mapItemsNode = root.AppendChild(mapItemsElem);
         if (null == mapItemsNode)
         {
            Logger.Log(LogEnum.LE_ERROR, "Create_XmlListingOfMapItems(): AppendChild(MapItemsNode) returned null");
            return false;
         }
         //--------------------------------
         foreach (IMapItem mi in theMapItems)
         {
            XmlElement? miElem = aXmlDocument.CreateElement("MapItem");
            if (null == miElem)
            {
               Logger.Log(LogEnum.LE_ERROR, "Create_XmlListingOfMapItems(): CreateElement(miElem) returned null");
               return false;
            }
            XmlNode? miNode = mapItemsNode.AppendChild(miElem);
            if (null == miNode)
            {
               Logger.Log(LogEnum.LE_ERROR, "Create_XmlListingOfMapItems(): AppendChild(miNode) returned null");
               return false;
            }
            if (null == mi)
            {
               miElem.SetAttribute("value", "null"); // MapItem
               return true;
            }
            else
            {
               miElem.SetAttribute("value", mi.Name);
            }
            //--------------------------------
            XmlElement? elem = aXmlDocument.CreateElement("TopImageName");
            if (null == elem)
            {
               Logger.Log(LogEnum.LE_ERROR, "Create_XmlListingOfMapItems(): CreateElement(TopImageName) returned null");
               return false;
            }
            elem.SetAttribute("value", mi.TopImageName);
            XmlNode? node = miNode.AppendChild(elem);
            if (null == node)
            {
               Logger.Log(LogEnum.LE_ERROR, "Create_XmlListingOfMapItems(): AppendChild(TopImageName) returned null");
               return false;
            }
            //--------------------------------
            elem = aXmlDocument.CreateElement("BottomImageName");
            if (null == elem)
            {
               Logger.Log(LogEnum.LE_ERROR, "Create_XmlListingOfMapItems(): CreateElement(BottomImageName) returned null");
               return false;
            }
            elem.SetAttribute("value", mi.BottomImageName);
            node = miNode.AppendChild(elem);
            if (null == node)
            {
               Logger.Log(LogEnum.LE_ERROR, "Create_XmlListingOfMapItems(): AppendChild(BottomImageName) returned null");
               return false;
            }
            //--------------------------------
            elem = aXmlDocument.CreateElement("OverlayImageName");
            if (null == elem)
            {
               Logger.Log(LogEnum.LE_ERROR, "Create_XmlListingOfMapItems(): CreateElement(OverlayImageName) returned null");
               return false;
            }
            elem.SetAttribute("value", mi.OverlayImageName);
            node = miNode.AppendChild(elem);
            if (null == node)
            {
               Logger.Log(LogEnum.LE_ERROR, "CreateXmlGameInstance(): AppendChild(OverlayImageName) returned null");
               return false;
            }
            //--------------------------------
            if (false == CreateXmlListingOfMapItemsWoundSpots(aXmlDocument, miNode, mi.WoundSpots))
            {
               Logger.Log(LogEnum.LE_ERROR, "Create_XmlListingOfMapItems(): Create_XmlListingOfMapItemsWoundSpots() returned false");
               return false;
            }
            //--------------------------------
            elem = aXmlDocument.CreateElement("Zoom");
            if (null == elem)
            {
               Logger.Log(LogEnum.LE_ERROR, "Create_XmlListingOfMapItems(): CreateElement(Zoom) returned null");
               return false;
            }
            elem.SetAttribute("value", mi.Zoom.ToString());
            node = miNode.AppendChild(elem);
            if (null == node)
            {
               Logger.Log(LogEnum.LE_ERROR, "Create_XmlListingOfMapItems(): AppendChild(Zoom) returned null");
               return false;
            }
            //--------------------------------
            elem = aXmlDocument.CreateElement("IsMoved");
            if (null == elem)
            {
               Logger.Log(LogEnum.LE_ERROR, "Create_XmlListingOfMapItems(): CreateElement(IsMoved) returned null");
               return false;
            }
            elem.SetAttribute("value", mi.IsMoved.ToString());
            node = miNode.AppendChild(elem);
            if (null == node)
            {
               Logger.Log(LogEnum.LE_ERROR, "Create_XmlListingOfMapItems(): AppendChild(IsMoved) returned null");
               return false;
            }
            //--------------------------------
            elem = aXmlDocument.CreateElement("Count");
            if (null == elem)
            {
               Logger.Log(LogEnum.LE_ERROR, "Create_XmlListingOfMapItems(): CreateElement(Count) returned null");
               return false;
            }
            elem.SetAttribute("value", mi.Count.ToString());
            node = miNode.AppendChild(elem);
            if (null == node)
            {
               Logger.Log(LogEnum.LE_ERROR, "Create_XmlListingOfMapItems(): AppendChild(Count) returned null");
               return false;
            }
            //--------------------------------
            elem = aXmlDocument.CreateElement("LocationX");
            if (null == elem)
            {
               Logger.Log(LogEnum.LE_ERROR, "Create_XmlListingOfMapItems(): CreateElement(LocationX) returned null");
               return false;
            }
            elem.SetAttribute("value", mi.Location.X.ToString("F3"));
            node = miNode.AppendChild(elem);
            if (null == node)
            {
               Logger.Log(LogEnum.LE_ERROR, "Create_XmlListingOfMapItems(): AppendChild(LocationX) returned null");
               return false;
            }
            //--------------------------------
            elem = aXmlDocument.CreateElement("LocationY");
            if (null == elem)
            {
               Logger.Log(LogEnum.LE_ERROR, "Create_XmlListingOfMapItems(): CreateElement(LocationY) returned null");
               return false;
            }
            elem.SetAttribute("value", mi.Location.Y.ToString("F3"));
            node = miNode.AppendChild(elem);
            if (null == node)
            {
               Logger.Log(LogEnum.LE_ERROR, "Create_XmlListingOfMapItems(): AppendChild(LocationY) returned null");
               return false;
            }
            //--------------------------------
            elem = aXmlDocument.CreateElement("TerritoryCurrent");
            if (null == elem)
            {
               Logger.Log(LogEnum.LE_ERROR, "Create_XmlListingOfMapItems(): CreateElement(TerritoryCurrent) returned null");
               return false;
            }
            if ("OffBoard" == mi.TerritoryCurrent.Name)
            {
               Logger.Log(LogEnum.LE_ERROR, "Create_XmlListingOfMapItems(): Invalid State mi.Name=" + mi.Name + " is Offbaord!");
               return false;
            }
            elem.SetAttribute("value", mi.TerritoryCurrent.Name);
            elem.SetAttribute("type", mi.TerritoryCurrent.Type);
            node = miNode.AppendChild(elem);
            if (null == node)
            {
               Logger.Log(LogEnum.LE_ERROR, "Create_XmlListingOfMapItems(): AppendChild(TerritoryCurrent) returned null");
               return false;
            }
            //--------------------------------
            elem = aXmlDocument.CreateElement("TerritoryStarting");
            if (null == elem)
            {
               Logger.Log(LogEnum.LE_ERROR, "Create_XmlListingOfMapItems(): CreateElement(TerritoryStarting) returned null");
               return false;
            }
            elem.SetAttribute("value", mi.TerritoryStarting.Name);
            elem.SetAttribute("type", mi.TerritoryStarting.Type);
            node = miNode.AppendChild(elem);
            if (null == node)
            {
               Logger.Log(LogEnum.LE_ERROR, "Create_XmlListingOfMapItems(): AppendChild(TerritoryStarting) returned null");
               return false;
            }
         }
         return true;
      }
      private bool CreateXmlListingOfMapItemsWoundSpots(XmlDocument aXmlDocument, XmlNode topNode, List<BloodSpot> woundSpots)
      {
         XmlElement? woundSpotsElem = aXmlDocument.CreateElement("WoundSpots");
         if (null == woundSpotsElem)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXmlListingOfMapItemsWoundSpots(): CreateElement(woundsElement) returned null");
            return false;
         }
         woundSpotsElem.SetAttribute("count", woundSpots.Count.ToString());
         XmlNode? woundSpotsNode = topNode.AppendChild(woundSpotsElem);
         if (null == woundSpotsNode)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXmlListingOfMapItemsWoundSpots(): AppendChild(woundSpotsNode) returned null");
            return false;
         }
         for (int k = 0; k < woundSpots.Count; ++k)
         {
            BloodSpot bloodSpot = woundSpots[k];
            XmlElement? spotElem = aXmlDocument.CreateElement("WoundSpot");
            if (null == spotElem)
            {
               Logger.Log(LogEnum.LE_ERROR, "CreateXmlListingOfMapItemsWoundSpots(): CreateElement(spotElem) returned null");
               return false;
            }
            spotElem.SetAttribute("size", bloodSpot.mySize.ToString());
            spotElem.SetAttribute("left", bloodSpot.myLeft.ToString());
            spotElem.SetAttribute("top", bloodSpot.myTop.ToString());
            XmlNode? spotNode = woundSpotsNode.AppendChild(spotElem);
            if (null == spotNode)
            {
               Logger.Log(LogEnum.LE_ERROR, "CreateXmlListingOfMapItemsWoundSpots(): AppendChild(miNode) returned null");
               return false;
            }
         }
         return true;
      }
      private bool CreateXmlListingOfMapItemsAcquiredShots(XmlDocument aXmlDocument, XmlNode topNode, Dictionary<string, int> enemyAcquiredShots)
      {
         XmlElement? enemyShotsElem = aXmlDocument.CreateElement("EnemyAcquiredShots");
         if (null == enemyShotsElem)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXmlListingOfMapItemsAcquiredShots(): CreateElement(enemyShotsElem) returned null");
            return false;
         }
         enemyShotsElem.SetAttribute("count", enemyAcquiredShots.Count.ToString());
         XmlNode? enemyAcquireShotsNode = topNode.AppendChild(enemyShotsElem);
         if (null == enemyAcquireShotsNode)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXmlListingOfMapItemsAcquiredShots(): AppendChild(enemyAcquireShotsNode) returned null");
            return false;
         }
         int count = 0;
         foreach (var kvp in enemyAcquiredShots)
         {
            XmlElement? enemyAcqShotElem = aXmlDocument.CreateElement("EnemyAcqShot");
            if (null == enemyAcqShotElem)
            {
               Logger.Log(LogEnum.LE_ERROR, "CreateXmlListingOfMapItemsAcquiredShots(): CreateElement(spotElem) returned null");
               return false;
            }
            enemyAcqShotElem.SetAttribute("enemy", kvp.Key);
            enemyAcqShotElem.SetAttribute("value", kvp.Value.ToString());
            XmlNode? enemyAcqShotNode = enemyAcquireShotsNode.AppendChild(enemyAcqShotElem);
            if (null == enemyAcqShotNode)
            {
               Logger.Log(LogEnum.LE_ERROR, "CreateXmlListingOfMapItemsAcquiredShots(): AppendChild(miNode) returned null");
               return false;
            }
            count++;
         }
         if (count != enemyAcquiredShots.Count)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXmlListingOfMapItemsAcquiredShots(): count=" + count.ToString() + " enemyAcquiredShots=" + enemyAcquiredShots.Count.ToString());
            return false;
         }
         return true;
      }
      private bool CreateXmlGameCommands(XmlDocument aXmlDocument, IGameCommands gameCommands)
      {
         XmlNode? root = aXmlDocument.DocumentElement;
         if (null == root)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXmlGameInstance(): root is null");
            return false;
         }
         XmlElement? gamecmdsElem = aXmlDocument.CreateElement("GameCommands");
         if (null == gamecmdsElem)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXmlGameInstance(): CreateElement(GameCommands) returned null");
            return false;
         }
         gamecmdsElem.SetAttribute("count", gameCommands.Count.ToString());
         XmlNode? gameCmdsNode = root.AppendChild(gamecmdsElem);
         if (null == gameCmdsNode)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXmlGameInstance(): AppendChild(gameCmdsNode) returned null");
            return false;
         }
         //--------------------------------
         for (int i = 0; i < gameCommands.Count; ++i)
         {
            IGameCommand? gameCmd = gameCommands[i];
            if (null == gameCmd)
            {
               Logger.Log(LogEnum.LE_ERROR, "CreateXmlGameInstance(): gameCmd=null");
               return false;
            }
            XmlElement? gameCmdElem = aXmlDocument.CreateElement("GameCommand");
            if (null == gameCmdElem)
            {
               Logger.Log(LogEnum.LE_ERROR, "CreateXmlGameInstance(): CreateElement(OptGameCommandion) returned null");
               return false;
            }
            //---------------------------------------
            gameCmdElem.SetAttribute("Action", gameCmd.Action.ToString());
            gameCmdElem.SetAttribute("ActionDieRoll", gameCmd.ActionDieRoll.ToString());
            gameCmdElem.SetAttribute("EventActive", gameCmd.EventActive.ToString());
            gameCmdElem.SetAttribute("Phase", gameCmd.Phase.ToString());
            XmlNode? gameCmdNode = gameCmdsNode.AppendChild(gameCmdElem);
            if (null == gameCmdNode)
            {
               Logger.Log(LogEnum.LE_ERROR, "CreateXmlGameInstance(): AppendChild(gameCmdNode) returned null");
               return false;
            }
         }
         return true;
      }
      private bool CreateXmlGameOptions(XmlDocument aXmlDocument, Options options)
      {
         XmlNode? root = aXmlDocument.DocumentElement;
         if (null == root)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXmlGameOptions(): root is null");
            return false;
         }
         XmlElement? optionsElem = aXmlDocument.CreateElement("Options");
         if (null == optionsElem)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXmlGameOptions(): CreateElement(Options) returned null");
            return false;
         }
         optionsElem.SetAttribute("count", options.Count.ToString());
         XmlNode? optionsNode = root.AppendChild(optionsElem);
         if (null == optionsNode)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXmlGameOptions(): AppendChild(optionsNode) returned null");
            return false;
         }
         //--------------------------------
         foreach (Option option in options)
         {
            XmlElement? optionElem = aXmlDocument.CreateElement("Option");
            if (null == optionElem)
            {
               Logger.Log(LogEnum.LE_ERROR, "CreateXmlGameOptions(): CreateElement(Option) returned null");
               return false;
            }
            optionElem.SetAttribute("Name", option.Name);
            optionElem.SetAttribute("IsEnabled", option.IsEnabled.ToString());
            XmlNode? optionNode = optionsNode.AppendChild(optionElem);
            if (null == optionNode)
            {
               Logger.Log(LogEnum.LE_ERROR, "CreateXmlGameOptions(): AppendChild(optionNode) returned null");
               return false;
            }
         }
         return true;
      }
      private bool CreateXmlGameStatistics(XmlDocument aXmlDocument, GameStatistics statistics)
      {
         XmlNode? root = aXmlDocument.DocumentElement;
         if (null == root)
         {
            Logger.Log(LogEnum.LE_ERROR, "Create_XmlGameStatistics(): root is null");
            return false;
         }
         XmlElement? gameStatElem = aXmlDocument.CreateElement("GameStatistics");
         if (null == gameStatElem)
         {
            Logger.Log(LogEnum.LE_ERROR, "Create_XmlGameStatistics(): CreateElement(gameStatElem) returned null");
            return false;
         }
         gameStatElem.SetAttribute("count", statistics.Count.ToString());
         XmlNode? statsNode = root.AppendChild(gameStatElem);
         if (null == statsNode)
         {
            Logger.Log(LogEnum.LE_ERROR, "Create_XmlGameStatistics(): AppendChild(statsNode) returned null");
            return false;
         }
         //--------------------------------
         foreach (GameStatistic stat in statistics)
         {
            XmlElement? statElem = aXmlDocument.CreateElement("GameStatistic");
            if (null == statElem)
            {
               Logger.Log(LogEnum.LE_ERROR, "Create_XmlGameStatistics(): CreateElement(GameStatistic) returned null");
               return false;
            }
            statElem.SetAttribute("Key", stat.Key);
            statElem.SetAttribute("Value", stat.Value.ToString());
            XmlNode? statNode = statsNode.AppendChild(statElem);
            if (null == statNode)
            {
               Logger.Log(LogEnum.LE_ERROR, "Create_XmlGameStatistics(): AppendChild(statNode) returned null");
               return false;
            }
         }
         return true;
      }
      private bool CreateXmlMapItems(XmlDocument aXmlDocument, XmlNode parent, IMapItems mapItems, string attribute)
      {
         XmlElement? mapItemsElem = aXmlDocument.CreateElement("MapItems");
         if (null == mapItemsElem)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXml_MapItems(): CreateElement(MapItemsElem) returned null");
            return false;
         }
         mapItemsElem.SetAttribute("value", attribute);
         mapItemsElem.SetAttribute("count", mapItems.Count.ToString());
         XmlNode? mapItemsNode = parent.AppendChild(mapItemsElem);
         if (null == mapItemsNode)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXml_MapItems(): AppendChild(MapItemsNode) returned null");
            return false;
         }
         //--------------------------------
         foreach (IMapItem mi in mapItems)
         {
            if (false == CreateXmlMapItem(aXmlDocument, mapItemsNode, mi))
            {
               Logger.Log(LogEnum.LE_ERROR, "CreateXml_MapItems(): CreateXmlMapItem() returned false");
               return false;
            }
         }
         return true;
      }
      private bool CreateXmlMapItem(XmlDocument aXmlDocument, XmlNode parent, IMapItem? mi, string attribute = "")
      {
         XmlElement? miElem = aXmlDocument.CreateElement("MapItem");
         if (null == miElem)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXmlMapItem(): CreateElement(miElem) returned null");
            return false;
         }
         XmlNode? miNode = parent.AppendChild(miElem);
         if (null == miNode)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXmlMapItem(): AppendChild(miNode) returned null");
            return false;
         }
         miElem.SetAttribute("value", attribute);
         if (null == mi)
            miElem.SetAttribute("name", "null");
         else
            miElem.SetAttribute("name", mi.Name);
         return true;
      }
      private bool CreateXmlMapItemMoves(XmlDocument aXmlDocument, IMapItemMoves mapItemMoves)
      {
         XmlNode? root = aXmlDocument.DocumentElement;
         if (null == root)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXmlMapItemMoves(): root is null");
            return false;
         }
         XmlElement? mapItemMovesElem = aXmlDocument.CreateElement("MapItemMoves");
         if (null == mapItemMovesElem)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXmlMapItemMoves(): CreateElement(MapItemMoves) returned null");
            return false;
         }
         mapItemMovesElem.SetAttribute("count", mapItemMoves.Count.ToString());
         XmlNode? mapItemMovesNode = root.AppendChild(mapItemMovesElem);
         if (null == mapItemMovesNode)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXmlMapItemMoves(): AppendChild(MapItemMoves) returned null");
            return false;
         }
         for (int i = 0; i < mapItemMoves.Count; ++i)
         {
            IMapItemMove? mim = mapItemMoves[i];
            if (null == mim)
            {
               Logger.Log(LogEnum.LE_ERROR, "CreateXmlMapItemMoves(): mim is null");
               return false;
            }
            XmlElement? mimElem = aXmlDocument.CreateElement("MapItemMove");
            if (null == mimElem)
            {
               Logger.Log(LogEnum.LE_ERROR, "CreateXmlMapItemMoves(): CreateElement(mimElem) returned null");
               return false;
            }
            mimElem.SetAttribute("value", mim.MapItem.Name);
            XmlNode? mimNode = mapItemMovesNode.AppendChild(mimElem);
            if (null == mimNode)
            {
               Logger.Log(LogEnum.LE_ERROR, "CreateXmlMapItemMoves(): AppendChild(MapItemMove) returned null");
               return false;
            }
            //--------------------------------------------
            if (false == CreateXmlMapItem(aXmlDocument, mimNode, mim.MapItem))
            {
               Logger.Log(LogEnum.LE_ERROR, "CreateXmlMapItemMoves(): CreateXmlMapItem() returned null");
               return false;
            }
            //--------------------------------------------
            XmlElement? elem = aXmlDocument.CreateElement("OldTerritory");
            if (null == elem)
            {
               Logger.Log(LogEnum.LE_ERROR, "CreateXmlStacks(): CreateElement(OldTerritory) returned false");
               return false;
            }
            if (null == mim.OldTerritory)
            {
               elem.SetAttribute("value", "null");
               elem.SetAttribute("type", "null");
            }
            else
            {
               elem.SetAttribute("value", mim.OldTerritory.Name);
               elem.SetAttribute("type", mim.OldTerritory.Type);
            }
            XmlNode? node = mimNode.AppendChild(elem);
            if (null == node)
            {
               Logger.Log(LogEnum.LE_ERROR, "CreateXmlStacks(): AppendChild(OldTerritory) returned false");
               return false;
            }
            //--------------------------------------------
            elem = aXmlDocument.CreateElement("NewTerritory");
            if (null == elem)
            {
               Logger.Log(LogEnum.LE_ERROR, "CreateXmlStacks(): CreateElement(NewTerritory) returned false");
               return false;
            }
            if (null == mim.NewTerritory)
            {
               elem.SetAttribute("value", "null");
               elem.SetAttribute("type", "null");
            }
            else
            {
               elem.SetAttribute("value", mim.NewTerritory.Name);
               elem.SetAttribute("type", mim.NewTerritory.Type);
            }
            node = mimNode.AppendChild(elem);
            if (null == node)
            {
               Logger.Log(LogEnum.LE_ERROR, "CreateXmlStacks(): AppendChild(NewTerritory) returned false");
               return false;
            }
            //--------------------------------------------
            if (false == CreateXmlMapItemMovesBestPath(aXmlDocument, mimNode, mim.BestPath))
            {
               Logger.Log(LogEnum.LE_ERROR, "CreateXmlMapItemMoves(): CreateXmlMapItemMoveBestPath() returned null");
               return false;
            }
         }
         return true;
      }
      private bool CreateXmlMapItemMovesBestPath(XmlDocument aXmlDocument, XmlNode parent, IMapPath? bestPath)
      {
         XmlElement? bestPathElem = aXmlDocument.CreateElement("BestPath");
         if (null == bestPathElem)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXmlMapItemMovesBestPath(): CreateElement(MapItemMoves) returned null");
            return false;
         }
         XmlNode? mapItemMovesNode = parent.AppendChild(bestPathElem);
         if (null == mapItemMovesNode)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXmlMapItemMovesBestPath(): AppendChild(MapItemMoves) returned null");
            return false;
         }
         if (null == bestPath)
         {
            bestPathElem.SetAttribute("value", "null"); // BestPath
            return true;
         }
         bestPathElem.SetAttribute("name", bestPath.Name);
         bestPathElem.SetAttribute("metric", bestPath.Metric.ToString("F2"));
         bestPathElem.SetAttribute("count", bestPath.Territories.Count.ToString());
         for (int i = 0; i < bestPath.Territories.Count; ++i)
         {
            ITerritory? t = bestPath.Territories[i];
            if (null == t)
            {
               Logger.Log(LogEnum.LE_ERROR, "CreateXmlMapItemMovesBestPath(): t is null");
               return false;
            }
            XmlElement? tElem = aXmlDocument.CreateElement("Territory");
            if (null == tElem)
            {
               Logger.Log(LogEnum.LE_ERROR, "CreateXmlMapItemMovesBestPath(): CreateElement(Territory) returned null");
               return false;
            }
            tElem.SetAttribute("name", t.Name);
            XmlNode? tNode = mapItemMovesNode.AppendChild(tElem);
            if (null == tNode)
            {
               Logger.Log(LogEnum.LE_ERROR, "CreateXmlMapItemMovesBestPath(): AppendChild(tNode) returned null");
               return false;
            }
         }
         return true;
      }
      private bool CreateXmlStacks(XmlDocument aXmlDocument, IStacks stacks, string attribute)
      {
         XmlNode? root = aXmlDocument.DocumentElement;
         if (null == root)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXmlStacks(): root is null");
            return false;
         }
         XmlElement? stacksElem = aXmlDocument.CreateElement("Stacks");
         if (null == stacksElem)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXmlStacks(): CreateElement(Stacks) returned null");
            return false;
         }
         stacksElem.SetAttribute("value", attribute);
         stacksElem.SetAttribute("count", stacks.Count.ToString());
         XmlNode? stacksNode = root.AppendChild(stacksElem);
         if (null == stacksNode)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXmlStacks(): AppendChild(Stacks) returned null");
            return false;
         }
         for (int i = 0; i < stacks.Count; ++i)
         {
            IStack? stack = stacks[i];
            if (null == stack)
            {
               Logger.Log(LogEnum.LE_ERROR, "CreateXmlStacks(): stack is null");
               return false;
            }
            XmlElement? stackElem = aXmlDocument.CreateElement("Stack");
            if (null == stackElem)
            {
               Logger.Log(LogEnum.LE_ERROR, "CreateXmlStacks(): CreateElement(stackElem) returned null");
               return false;
            }
            stackElem.SetAttribute("value", stack.Territory.Name);
            XmlNode? stackNode = stacksNode.AppendChild(stackElem);
            if (null == stackNode)
            {
               Logger.Log(LogEnum.LE_ERROR, "CreateXmlStacks(): AppendChild(stackNode) returned null");
               return false;
            }
            //------------------------------------------
            XmlElement? elem = aXmlDocument.CreateElement("IsStacked");
            if (null == elem)
            {
               Logger.Log(LogEnum.LE_ERROR, "CreateXmlStacks(): CreateElement(IsStacked) returned false");
               return false;
            }
            elem.SetAttribute("value", stack.IsStacked.ToString());
            XmlNode? node = stackNode.AppendChild(elem);
            if (null == node)
            {
               Logger.Log(LogEnum.LE_ERROR, "CreateXmlStacks(): AppendChild(IsStacked) returned false");
               return false;
            }
            //------------------------------------------
            if (false == CreateXmlMapItems(aXmlDocument, stackNode, stack.MapItems, stack.Territory.Name))
            {
               Logger.Log(LogEnum.LE_ERROR, "CreateXmlStacks(): CreateXmlMapItems() returned false");
               return false;
            }
         }
         return true;
      }
      private bool CreateXmlEnteredHexes(XmlDocument aXmlDocument, List<EnteredHex> enteredHexes)
      {
         XmlNode? root = aXmlDocument.DocumentElement;
         if (null == root)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXml_EnteredHexes(): root is null");
            return false;
         }
         XmlElement? enteredHexesElem = aXmlDocument.CreateElement("EnteredHexes");
         if (null == enteredHexesElem)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXml_EnteredHexes(): CreateElement(EnteredHexes) returned null");
            return false;
         }
         enteredHexesElem.SetAttribute("count", enteredHexes.Count.ToString());
         XmlNode? enteredHexesNode = root.AppendChild(enteredHexesElem);
         if (null == enteredHexesNode)
         {
            Logger.Log(LogEnum.LE_ERROR, "CreateXml_EnteredHexes(): AppendChild(EnteredHexes) returned null");
            return false;
         }
         for (int i = 0; i < enteredHexes.Count; ++i)
         {
            EnteredHex enteredHex = enteredHexes[i];
            XmlElement? enteredHexElem = aXmlDocument.CreateElement("EnteredHex");  // name of territory
            if (null == enteredHexElem)
            {
               Logger.Log(LogEnum.LE_ERROR, "CreateXml_EnteredHexes(): CreateElement(enteredHexElem) returned null");
               return false;
            }
            enteredHexElem.SetAttribute("value", enteredHex.Identifer.ToString());
            XmlNode? enteredHexNode = enteredHexesNode.AppendChild(enteredHexElem);
            if (null == enteredHexNode)
            {
               Logger.Log(LogEnum.LE_ERROR, "CreateXml_EnteredHexes(): AppendChild(enteredHexNode) returned null");
               return false;
            }
            //------------------------------------------
            XmlElement? elem = aXmlDocument.CreateElement("Day");
            if (null == elem)
            {
               Logger.Log(LogEnum.LE_ERROR, "CreateXml_EnteredHexes(): CreateElement(Day) returned false");
               return false;
            }
            elem.SetAttribute("value", enteredHex.Day.ToString());
            XmlNode? node = enteredHexNode.AppendChild(elem);
            if (null == node)
            {
               Logger.Log(LogEnum.LE_ERROR, "CreateXml_EnteredHexes(): AppendChild(Day) returned false");
               return false;
            }
            //------------------------------------------
            elem = aXmlDocument.CreateElement("Date");
            if (null == elem)
            {
               Logger.Log(LogEnum.LE_ERROR, "CreateXml_EnteredHexes(): CreateElement(Date) returned false");
               return false;
            }
            elem.SetAttribute("value", enteredHex.Date);
            node = enteredHexNode.AppendChild(elem);
            if (null == node)
            {
               Logger.Log(LogEnum.LE_ERROR, "CreateXml_EnteredHexes(): AppendChild(Date) returned false");
               return false;
            }
            //------------------------------------------
            elem = aXmlDocument.CreateElement("Time");
            if (null == elem)
            {
               Logger.Log(LogEnum.LE_ERROR, "CreateXml_EnteredHexes(): CreateElement(Time) returned false");
               return false;
            }
            elem.SetAttribute("value", enteredHex.Time);
            node = enteredHexNode.AppendChild(elem);
            if (null == node)
            {
               Logger.Log(LogEnum.LE_ERROR, "CreateXml_EnteredHexes(): AppendChild(Time) returned false");
               return false;
            }
            //------------------------------------------
            elem = aXmlDocument.CreateElement("TerritoryName");
            if (null == elem)
            {
               Logger.Log(LogEnum.LE_ERROR, "CreateXml_EnteredHexes(): CreateElement(TerritoryName) returned false");
               return false;
            }
            elem.SetAttribute("value", enteredHex.TerritoryName);
            node = enteredHexNode.AppendChild(elem);
            if (null == node)
            {
               Logger.Log(LogEnum.LE_ERROR, "CreateXml_EnteredHexes(): AppendChild(TerritoryName) returned false");
               return false;
            }
            //------------------------------------------
            elem = aXmlDocument.CreateElement("MapPoint");
            if (null == elem)
            {
               Logger.Log(LogEnum.LE_ERROR, "CreateXml_EnteredHexes(): CreateElement(MapPoint) returned false");
               return false;
            }
            elem.SetAttribute("X", enteredHex.MapPoint.X.ToString());
            elem.SetAttribute("Y", enteredHex.MapPoint.Y.ToString());
            node = enteredHexNode.AppendChild(elem);
            if (null == node)
            {
               Logger.Log(LogEnum.LE_ERROR, "CreateXml_EnteredHexes(): AppendChild(MapPoint) returned false");
               return false;
            }
            //------------------------------------------
            elem = aXmlDocument.CreateElement("ColorAction");
            if (null == elem)
            {
               Logger.Log(LogEnum.LE_ERROR, "CreateXml_EnteredHexes(): CreateElement(ColorAction) returned false");
               return false;
            }
            elem.SetAttribute("value", enteredHex.ColorAction.ToString());
            node = enteredHexNode.AppendChild(elem);
            if (null == node)
            {
               Logger.Log(LogEnum.LE_ERROR, "CreateXml_EnteredHexes(): AppendChild(ColorAction) returned false");
               return false;
            }
         }
         return true;
      }
      //private bool ReadXmlDieRollResults(XmlReader reader, Dictionary<string, int[]> dieResults)
      //{
      //   try // resync the gi.DieResults[] to initial conditions
      //   {
      //      foreach (string key in myRulesMgr.Events.Keys)
      //         dieResults[key] = new int[3] { Utilities.NO_RESULT, Utilities.NO_RESULT, Utilities.NO_RESULT };
      //   }
      //   catch (Exception e)
      //   {
      //      Logger.Log(LogEnum.LE_ERROR, "ReadXmlDieRollResults(): e=" + e.ToString());
      //      return false;
      //   }
      //   //------------------------------------------
      //   reader.Read();
      //   if (false == reader.IsStartElement())
      //   {
      //      Logger.Log(LogEnum.LE_ERROR, "ReadXmlDieRollResults(): IsStartElement(EnemyAcquiredShots) returned false");
      //      return false;
      //   }
      //   if (reader.Name != "DieRollResults")
      //   {
      //      Logger.Log(LogEnum.LE_ERROR, "ReadXmlDieRollResults(): DieRollResults != (node=" + reader.Name + ")");
      //      return false;
      //   }
      //   string? sCount = reader.GetAttribute("count");
      //   if (null == sCount)
      //   {
      //      Logger.Log(LogEnum.LE_ERROR, "ReadXmlDieRollResults(): Count=null");
      //      return false;
      //   }
      //   int count = int.Parse(sCount);
      //   for (int i = 0; i < count; i++)
      //   {
      //      reader.Read();
      //      if (false == reader.IsStartElement())
      //      {
      //         Logger.Log(LogEnum.LE_ERROR, "ReadXmlDieRollResults(): reader.IsStartElement(EnemyAcqShot) = false");
      //         return false;
      //      }
      //      if (reader.Name != "DieRollResult")
      //      {
      //         Logger.Log(LogEnum.LE_ERROR, "ReadXmlDieRollResults(): DieRollResult != (node=" + reader.Name + ")");
      //         return false;
      //      }
      //      //-------------------------------
      //      string? sKey = reader.GetAttribute("key");
      //      if (null == sKey)
      //      {
      //         Logger.Log(LogEnum.LE_ERROR, "ReadXmlDieRollResults(): sKey=null");
      //         return false;
      //      }
      //      //-------------------------------
      //      string? sRoll0 = reader.GetAttribute("r0");
      //      if (null == sRoll0)
      //      {
      //         Logger.Log(LogEnum.LE_ERROR, "ReadXmlDieRollResults(): sRoll0=null");
      //         return false;
      //      }
      //      dieResults[sKey][0] = Convert.ToInt32(sRoll0);
      //      //-------------------------------
      //      string? sRoll1 = reader.GetAttribute("r1");
      //      if (null == sRoll1)
      //      {
      //         Logger.Log(LogEnum.LE_ERROR, "ReadXmlDieRollResults(): sRoll1=null");
      //         return false;
      //      }
      //      dieResults[sKey][1] = Convert.ToInt32(sRoll1);
      //      //-------------------------------
      //      string? sRoll2 = reader.GetAttribute("r2");
      //      if (null == sRoll2)
      //      {
      //         Logger.Log(LogEnum.LE_ERROR, "ReadXmlDieRollResults(): sRoll2=null");
      //         return false;
      //      }
      //      dieResults[sKey][2] = Convert.ToInt32(sRoll2);
      //   }
      //   reader.Read(); // get past </DieRollResults> tag
      //   return true;
      //}
      //private bool CreateXmlDieRollResults(XmlDocument aXmlDocument, XmlNode topNode, Dictionary<string, int[]> dieResults)
      //{
      //   //------------------------------------------------------
      //   XmlElement? dieRollResultsElem = aXmlDocument.CreateElement("DieRollResults");
      //   if (null == dieRollResultsElem)
      //   {
      //      Logger.Log(LogEnum.LE_ERROR, "CreateXmlDieRollResults(): CreateElement(dieRollResultsElem) returned null");
      //      return false;
      //   }
      //   dieRollResultsElem.SetAttribute("count", dieResults.Count.ToString());
      //   XmlNode? dieRollResultsNode = topNode.AppendChild(dieRollResultsElem);
      //   if (null == dieRollResultsNode)
      //   {
      //      Logger.Log(LogEnum.LE_ERROR, "CreateXmlDieRollResults(): AppendChild(dieRollResultsNode) returned null");
      //      return false;
      //   }
      //   int count = 0;
      //   foreach (var kvp in dieResults)
      //   {
      //      XmlElement? dieRollResultElem = aXmlDocument.CreateElement("DieRollResult");
      //      if (null == dieRollResultElem)
      //      {
      //         Logger.Log(LogEnum.LE_ERROR, "CreateXmlDieRollResults(): CreateElement(dieRollResultElem) returned null");
      //         return false;
      //      }
      //      dieRollResultElem.SetAttribute("key", kvp.Key);
      //      dieRollResultElem.SetAttribute("r0", kvp.Value[0].ToString());
      //      dieRollResultElem.SetAttribute("r1", kvp.Value[1].ToString());
      //      dieRollResultElem.SetAttribute("r2", kvp.Value[2].ToString());
      //      XmlNode? dieResultNode = dieRollResultsNode.AppendChild(dieRollResultElem);
      //      if (null == dieResultNode)
      //      {
      //         Logger.Log(LogEnum.LE_ERROR, "CreateXmlDieRollResults(): AppendChild(dieResultNode) returned null");
      //         return false;
      //      }
      //      count++;
      //   }
      //   if (count != dieResults.Count)
      //   {
      //      Logger.Log(LogEnum.LE_ERROR, "CreateXmlDieRollResults(): count=" + count.ToString() + " dieResults.Count=" + dieResults.Count.ToString());
      //      return false;
      //   }
      //   return true;
      //}
   }
}
