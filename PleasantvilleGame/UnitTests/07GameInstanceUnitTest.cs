using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Intrinsics.Arm;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using System.Windows.Shapes;
using System.Xml.Linq;
using Image=System.Windows.Controls.Image;

namespace PleasantvilleGame
{
   internal class GameInstanceUnitTest : IUnitTest
   {
      //--------------------------------------------------------------------
      private DockPanel? myDockPanelTop = null;
      private ScrollViewer? myScrollViewerCanvas = null;
      private Canvas? myCanvasMain = null;
      private Canvas? myCanvasHelper = null;
      private IGameInstance? myGameInstanceSave = null;
      private IGameInstance? myGameInstanceLoad = null;
      //--------------------------------------------------------------------
      private int myIndexName = 0;
      private List<string> myHeaderNames = new List<string>();
      private List<string> myCommandNames = new List<string>();
      public bool CtorError { get; } = false;
      public string HeaderName { get { return myHeaderNames[myIndexName]; } }
      public string CommandName { get { return myCommandNames[myIndexName]; } }
      public GameInstanceUnitTest(DockPanel dp)
      {
         //------------------------------------
         myIndexName = 0;
         myHeaderNames.Add("07-Save Game");
         myHeaderNames.Add("07-Load Game");
         myHeaderNames.Add("07-Compare");
         myHeaderNames.Add("07-Finish");
         //------------------------------------
         myCommandNames.Add("Save Game");
         myCommandNames.Add("Load Game");
         myCommandNames.Add("Compare");
         myCommandNames.Add("Finish");
         //------------------------------------
         myDockPanelTop = dp; // top most dock panel that holds menu, statusbar, left dockpanel, and right dockpanel
         foreach (UIElement ui0 in dp.Children)
         {
            if (ui0 is DockPanel dockPanelInside) // DockPanel showing main play area
            {
               foreach (UIElement ui1 in dockPanelInside.Children)
               {
                  if (ui1 is ScrollViewer)
                  {
                     myScrollViewerCanvas = (ScrollViewer)ui1;
                     if (myScrollViewerCanvas.Content is Canvas)
                        myCanvasMain = (Canvas)myScrollViewerCanvas.Content;  // Find the Canvas in the visual tree
                  }
                  if (ui1 is DockPanel dockPanelControl) // DockPanel that holds the Map Image
                  {
                     foreach (UIElement ui2 in dockPanelControl.Children)
                     {
                        if (ui2 is Canvas)
                        {
                           myCanvasHelper = (Canvas)ui2;
                        }
                     }
                  }
               }
            }
         }
         if (null == myCanvasMain) // log error and return if canvas not found
         {
            Logger.Log(LogEnum.LE_ERROR, "GameViewerCreateUnitTest(): myCanvasMain=null");
            CtorError = true;
            return;
         }
         if (null == myCanvasHelper) // log error and return if canvas not found
         {
            Logger.Log(LogEnum.LE_ERROR, "GameViewerCreateUnitTest(): myCanvasHelper=null");
            CtorError = true;
            return;
         }
      }
      //--------------------------------------------------------------------
      public bool Command(ref IGameInstance gi) // Performs function based on CommandName string
      {
         if (null == myDockPanelTop)
         {
            Logger.Log(LogEnum.LE_ERROR, "Command(): myDockPanelTop=null");
            return false;
         }
         if (null == myCanvasMain)
         {
            Logger.Log(LogEnum.LE_ERROR, "Command(): myCanvas=null");
            return false;
         }
         if (null == myCanvasHelper)
         {
            Logger.Log(LogEnum.LE_ERROR, "Command(): myCanvasTank=null");
            return false;
         }
         if (null == myScrollViewerCanvas)
         {
            Logger.Log(LogEnum.LE_ERROR, "Command(): myScrollViewerCanvas=null");
            return false;
         }
         //----------------------------------------------------b-
         if (CommandName == myCommandNames[0])
         {
            myGameInstanceSave = new GameInstance();
            if (false == SaveLocalGame(myGameInstanceSave))
            {
               Logger.Log(LogEnum.LE_ERROR, "Command(): Save_Game() returned false");
               return false;
            }
            if (null == myGameInstanceSave)
            {
               Logger.Log(LogEnum.LE_ERROR, "Command(): myGameInstanceSave=null");
               return false;
            }
            ++myIndexName;
            GameLoadMgr loadMgr = new GameLoadMgr();
            if (false == loadMgr.SaveGameAsToFile(myGameInstanceSave))
            {
               Logger.Log(LogEnum.LE_ERROR, "Command(): GameLoadMgr.SaveGameAs() returned false");
               return false;
            }
         }
         else if( CommandName == myCommandNames[1])
         {
            GameLoadMgr loadMgr = new GameLoadMgr();
            myGameInstanceLoad = loadMgr.OpenGameFromFile();
            if (null == myGameInstanceLoad)
            {
               Logger.Log(LogEnum.LE_ERROR, "Command(): GameLoadMgr.Open_GameFromFile() returned null");
               return false;
            }
            ++myIndexName;
         }
         else if (CommandName == myCommandNames[2])
         {
            if (false == IsEqual(myGameInstanceSave, myGameInstanceLoad))
            {
               Logger.Log(LogEnum.LE_ERROR, "Command(): IsEqual() returned false");
               return false;
            }
         }
         else if (CommandName == myCommandNames[3])
         {
            if (false == Cleanup(ref gi))
            {
               Logger.Log(LogEnum.LE_ERROR, "Command(): Cleanup() return falsed");
               return false;
            }
         }
         return true;
      }
      public bool NextTest(ref IGameInstance gi) // Move to the next test in this class's unit tests
      {
         if (null == myCanvasMain)
         {
            Logger.Log(LogEnum.LE_ERROR, "NextTest(): myCanvas=null");
            return false;
         }
         if (HeaderName == myHeaderNames[0])
         {
            ++myIndexName;
         }
         else if (HeaderName == myHeaderNames[1])
         {
            ++myIndexName;
         }
         else if (HeaderName == myHeaderNames[2])
         {
            ++myIndexName;
         }
         else if (HeaderName == myHeaderNames[3])
         {
            if (false == Cleanup(ref gi))
            {
               Logger.Log(LogEnum.LE_ERROR, "NextTest(): Cleanup() return falsed");
               return false;
            }
         }
         return true;
      }
      public bool Cleanup(ref IGameInstance gi) // Remove an elipses from the canvas and save off Territories.xml file
      {
         if (null == myCanvasMain)
         {
            Logger.Log(LogEnum.LE_ERROR, "Cleanup(): myCanvas=null");
            return false;
         }
         //--------------------------------------------------
         // Remove any existing UI elements from the Canvas
         List<UIElement> elements = new List<UIElement>();
         foreach (UIElement ui in myCanvasMain.Children)
         {
            if (ui is Image img)
            {
               if (true == img.Name.Contains("Canvas"))
                  continue;
               elements.Add(ui);
            }
            else if (ui is Polygon polygon)
               elements.Add(ui);
            else if (ui is Polyline polyline)
               elements.Add(ui);
            else if (ui is Ellipse ellipse)
               elements.Add(ui);
            else if (ui is TextBlock tb)
               elements.Add(ui);
         }
         foreach (UIElement ui1 in elements)
            myCanvasMain.Children.Remove(ui1);
         //--------------------------------------------------
         ++gi.GameTurn; // moves to next unit test
         return true;
      }
      //--------------------------------------------------------------------
      private bool SaveLocalGame(IGameInstance gi)
      {
         gi.Stacks.Clear();
         if (false == TableMgr.CreateTownspeople(gi))
         {
            Logger.Log(LogEnum.LE_ERROR, "SaveLocalGame(): CreateTownspeople() returned false");
            return false;
         }
         IMapItem? mi1 = gi.Stacks.FindMapItem(Utilities.RemoveSpaces(TableMgr.FIRE_CHIEF));
         if (null == mi1)
         {
            Logger.Log(LogEnum.LE_ERROR, "SaveLocalGame(): gi.Stacks.FindMapItem(TableMgr.FIRE_CHIEF) returned null");
            return false;
         }
         mi1.TopImageName = "BlankWhite";
         mi1.BottomImageName = "Continue";
         mi1.OverlayImageName = "Judge";
         mi1.Zoom = 2.1;
         mi1.IsKilled = true;
         mi1.IsMoved = true;
         mi1.IsKilled = true;
         mi1.Combat = 90;
         mi1.Influence = 91;
         mi1.Movement = 92;
         mi1.MovementOriginal = 93;
         mi1.MovementUsed = 94;
         mi1.IsKnockedout = true;
         mi1.IsKnockedoutThisTurn = true;
         mi1.IsAlienUnknown = true;
         mi1.IsAlienKnown = true;
         mi1.IsControlled = true;
         mi1.IsImplantHeld = true;
         mi1.IsImplantRemovalAttempt = true;
         mi1.IsInterrogated = true;
         mi1.IsSkeptical = true;
         mi1.IsStunned = true;
         mi1.IsStunnedThisTurn = true;
         mi1.IsSurrendered = true;
         mi1.IsTiedUp = true;
         mi1.IsWary = true;
         mi1.IsMovingThisTurn = true;
         mi1.IsConversedThisTurn = true;
         mi1.IsInfluencedThisTurn = true;
         mi1.IsCombatThisTurn = true;
         mi1.IsImplantRemovalAttemptThisTurn = true;
         //---------------------------------------
         IMapItem? mi2 = gi.Stacks.FindMapItem(Utilities.RemoveSpaces(TableMgr.BAR_TENDER));
         if (null == mi2)
         {
            Logger.Log(LogEnum.LE_ERROR, "SaveLocalGame(): gi.Stacks.FindMapItem(TableMgr.BAR_TENDER) returned null");
            return false;
         }
         mi2.TopImageName = "Alien";
         mi2.BottomImageName = "Skulls";
         mi2.OverlayImageName = "Deny";
         mi2.Zoom = 2.3;
         mi2.IsKilled = true;
         mi2.IsMoved = false;
         mi2.IsKilled = true;
         mi2.Combat = 80;
         mi2.Influence = 81;
         mi2.Movement = 82;
         mi2.MovementOriginal = 83;
         mi2.MovementUsed = 84;
         mi2.IsKnockedout = false;
         mi2.IsKnockedoutThisTurn = true;
         mi2.IsAlienUnknown = true;
         mi2.IsAlienKnown = true;
         mi2.IsControlled = true;
         mi2.IsImplantHeld = false;
         mi2.IsImplantRemovalAttempt = true;
         mi2.IsInterrogated = true;
         mi2.IsSkeptical = true;
         mi2.IsStunned = true;
         mi2.IsStunnedThisTurn = true;
         mi2.IsSurrendered = false;
         mi2.IsTiedUp = true;
         mi2.IsWary = true;
         mi2.IsMovingThisTurn = true;
         mi2.IsConversedThisTurn = true;
         mi2.IsInfluencedThisTurn = false;
         mi2.IsCombatThisTurn = true;
         mi2.IsImplantRemovalAttemptThisTurn = true;
         //---------------------------------------
         gi.GameGuid = Guid.NewGuid();
         gi.EventActive = "111";
         gi.EventDisplayed = "222";
         gi.GameTurn = 99;
         gi.GamePhase = GamePhase.AlienTakeovers;
         gi.EndGameReason = "Testing 1, 2, 3";
         gi.NumTownGuessesForZebulonLocation = 55;
         gi.IsAlienAckedRandomMovement = true;
         //---------------------------------------
         gi.Options.Add(new Option("OptionTest1", false));
         gi.Options.Add(new Option("OptionTest2", true));
         gi.Options.Add(new Option("OptionTest3", false));
         gi.Options.Add(new Option("TownSolo", true));
         gi.Statistics.Add(new GameStatistic("Alligator"));
         gi.Statistics.AddOne("Alligator");
         gi.Statistics.AddOne("Alligator");
         gi.Statistics.AddOne("Alligator");
         gi.Statistics.Add(new GameStatistic("Shark"));
         gi.Statistics.AddOne("Shark");
         gi.Statistics.Add(new GameStatistic("Panda"));
         gi.Statistics.AddOne("Panda");
         gi.Statistics.AddOne("Panda");
         //---------------------------------------
         PlayerAlienComputer player = (PlayerAlienComputer)gi.PlayerAlien;
         if( null == player )
         {
            Logger.Log(LogEnum.LE_ERROR, "SaveLocalGame(): player=null");
            return false;
         }
         player.myBehavior.StrategyPrimary = AlienStrategyEnum.DEFEND_ZEBULON;
         player.myBehavior.StrategySecondary = AlienStrategyEnum.ATTACK_TOWNSPEOPLE;
         player.myBehavior.Risky = 88;
         player.myBehavior.Stealthy = 89;
         //---------------------------------------
         RandomMoveData move = new RandomMoveData(TableMgr.FIRE_CHIEF, TableMgr.HOUSE5, 100);
         gi.RandomMoves.Add(move);
         move = new RandomMoveData(TableMgr.BAR_TENDER, TableMgr.HOUSE6, 101);
         gi.RandomMoves.Add(move);
         //---------------------------------------
         gi.AlienTakeovers[mi1] = mi2;
         gi.AlienTakeovers[mi2] = mi1;
         //---------------------------------------
         ITerritory? tOld = Territories.theTerritories[1];
         if (null == tOld)
         {
            Logger.Log(LogEnum.LE_ERROR, "Command(): tOld=null");
            return false;
         }
         ITerritory? tNew = Territories.theTerritories[200];
         if (null == tNew)
         {
            Logger.Log(LogEnum.LE_ERROR, "Command(): tNew=null");
            return false;
         }
         MapItemMove mim = new MapItemMove();
         mim.MapItem = mi1;
         mim.OldTerritory = tOld;
         mim.NewTerritory = tNew;
         mim.BestPath = Territory.GetBestPath(Territories.theTerritories, tOld, tNew, 10);
         gi.MapItemMoves.Add(mim);
         mim = new MapItemMove();
         mim.MapItem = mi2;
         mim.OldTerritory = tNew;
         mim.NewTerritory = tOld;
         mim.BestPath = Territory.GetBestPath(Territories.theTerritories, tNew, tOld, 12);
         gi.MapItemMoves.Add(mim);
         //---------------------------------------
         gi.SelectedStack = null;
         //---------------------------------------
         for(int i=0; i<10; i++)
         {
            ITerritory? t = Territories.theTerritories[i];
            if( null == t )
            {
               Logger.Log(LogEnum.LE_ERROR, "Command(): t=null");
               return false;
            }
            gi.ZebulonTerritories.Add(t);
         }
         //---------------------------------------
         for (int i = 8; i < 12; i++)
         {
            ITerritory? t = Territories.theTerritories[i];
            if (null == t)
            {
               Logger.Log(LogEnum.LE_ERROR, "Command(): t=null");
               return false;
            }
            gi.SelectedTerritories.Add(t);
         }
         //---------------------------------------
         IMapItem? mi3 = gi.Stacks.FindMapItem(Utilities.RemoveSpaces(TableMgr.TEACHER));
         if (null == mi3)
         {
            Logger.Log(LogEnum.LE_ERROR, "SaveLocalGame(): gi.Stacks.FindMapItem(TableMgr.TEACHER) returned null");
            return false;
         }
         gi.SelectedMapItems.Add(mi3);
         IMapItem? mi4 = gi.Stacks.FindMapItem(Utilities.RemoveSpaces(TableMgr.PAPERBOY));
         if (null == mi4)
         {
            Logger.Log(LogEnum.LE_ERROR, "SaveLocalGame(): gi.Stacks.FindMapItem(TableMgr.PAPERBOY) returned null");
            return false;
         }
         gi.SelectedMapItems.Add(mi4);
         //---------------------------------------
         IMapItem? mi5 = gi.Stacks.FindMapItem(Utilities.RemoveSpaces(TableMgr.BANK_PRESIDENT));
         if (null == mi5)
         {
            Logger.Log(LogEnum.LE_ERROR, "SaveLocalGame(): gi.Stacks.FindMapItem(TableMgr.BANK_PRESIDENT) returned null");
            return false;
         }
         gi.DeadPeople.Add(mi5);
         IMapItem? mi6 = gi.Stacks.FindMapItem(Utilities.RemoveSpaces(TableMgr.BANK_GUARD));
         if (null == mi6)
         {
            Logger.Log(LogEnum.LE_ERROR, "SaveLocalGame(): gi.Stacks.FindMapItem(TableMgr.BANK_GUARD) returned null");
            return false;
         }
         gi.DeadPeople.Add(mi6);
         //---------------------------------------
         gi.Zebulon.IsAlienKnown = true;
         //---------------------------------------
         gi.MapItemCombat.Attackers.Add(mi3);
         gi.MapItemCombat.Defenders.Add(mi4);
         gi.MapItemCombat.Attackers.Add(mi5);
         gi.MapItemCombat.Defenders.Add(mi6);
         ITerritory? tcombat = Territories.theTerritories[30];
         if (null == tcombat)
         {
            Logger.Log(LogEnum.LE_ERROR, "SaveLocalGame():tcombat=null");
            return false;
         }
         gi.MapItemCombat.Territory = tcombat;
         gi.MapItemCombat.Result = CombatResult.DefenderWins;
         gi.MapItemCombat.DieRoll = 88;
         return true;
      }
      private bool IsEqual(IGameInstance? left, IGameInstance? right)
      {
         if (null == left)
         {
            Logger.Log(LogEnum.LE_ERROR, "IsEqual(): left=null");
            return false;
         }
         if (null == right)
         {
            Logger.Log(LogEnum.LE_ERROR, "IsEqual(): right=null");
            return false;
         }
         if( left.GameGuid != right.GameGuid )
         {
            Logger.Log(LogEnum.LE_ERROR, "IsEqual(): left.GameGuid != right.GameGuid");
            return false;
         }
         if (left.EventActive != right.EventActive)
         {
            Logger.Log(LogEnum.LE_ERROR, "IsEqual(): left.EventActive != right.EventActive");
            return false;
         }
         if (left.EventDisplayed != right.EventDisplayed)
         {
            Logger.Log(LogEnum.LE_ERROR, "IsEqual(): left.EventDisplayed != right.EventDisplayed");
            return false;
         }
         if (left.GameTurn != right.GameTurn)
         {
            Logger.Log(LogEnum.LE_ERROR, "IsEqual(): left.GameTurn != right.GameTurn");
            return false;
         }
         if (left.GamePhase != right.GamePhase)
         {
            Logger.Log(LogEnum.LE_ERROR, "IsEqual(): left.GamePhase != right.GamePhase");
            return false;
         }
         if (left.EndGameReason != right.EndGameReason)
         {
            Logger.Log(LogEnum.LE_ERROR, "IsEqual(): left.EndGameReason != right.EndGameReason");
            return false;
         }
         if (left.NumTownGuessesForZebulonLocation != right.NumTownGuessesForZebulonLocation)
         {
            Logger.Log(LogEnum.LE_ERROR, "IsEqual(): left.NumTownGuessesForZebulonLocation != right.NumTownGuessesForZebulonLocation");
            return false;
         }
         if (left.IsAlienAckedRandomMovement != right.IsAlienAckedRandomMovement)
         {
            Logger.Log(LogEnum.LE_ERROR, "IsEqual(): left.IsAlienAckedRandomMovement != right.IsAlienAckedRandomMovement");
            return false;
         }
         //---------------------------------------
         if( false == IsEqual(left.RandomMoves, right.RandomMoves))
         {
            Logger.Log(LogEnum.LE_ERROR, "IsEqual(): left.RandomMoves != right.RandomMoves");
            return false;
         }
         //---------------------------------------
         if (false == IsEqual(left.AlienTakeovers, right.AlienTakeovers))
         {
            Logger.Log(LogEnum.LE_ERROR, "IsEqual(): left.AlienTakeovers != right.AlienTakeovers");
            return false;
         }
         //---------------------------------------
         if (false == IsEqual(left.MapItemMoves, right.MapItemMoves))
         {
            Logger.Log(LogEnum.LE_ERROR, "IsEqual(): left.MapItemMoves != right.MapItemMoves");
            return false;
         }
         //---------------------------------------
         if (false == IsEqual(left.Stacks, right.Stacks))
         {
            Logger.Log(LogEnum.LE_ERROR, "IsEqual(): left.Stacks != right.Stacks");
            return false;
         }
         //---------------------------------------
         if (false == IsEqual(left.SelectedStack, right.SelectedStack))
         {
            Logger.Log(LogEnum.LE_ERROR, "IsEqual(): left.SelectedStack != right.SelectedStack");
            return false;
         }
         //---------------------------------------
         if (false == IsEqual(left.ZebulonTerritories, right.ZebulonTerritories))
         {
            Logger.Log(LogEnum.LE_ERROR, "IsEqual(): left.ZebulonTerritories != right.ZebulonTerritories");
            return false;
         }
         //---------------------------------------
         if (false == IsEqual(left.SelectedTerritories, right.SelectedTerritories))
         {
            Logger.Log(LogEnum.LE_ERROR, "IsEqual(): left.SelectedTerritories != right.SelectedTerritories");
            return false;
         }
         //---------------------------------------
         if (null == left.SelectedTerritory && null != right.SelectedTerritory) // mismatch in left=null and right
         {
            Logger.Log(LogEnum.LE_ERROR, "IsEqual(): null == left.SelectedTerritory && null != right.SelectedTerritory");
            return false;
         }
         if (null != left.SelectedTerritory && null == right.SelectedTerritory) // mismatch in left and right=null
         {
            Logger.Log(LogEnum.LE_ERROR, "IsEqual(): null != left.SelectedTerritory && null == right.SelectedTerritory");
            return false;
         }
         if (null != left.SelectedTerritory && null != right.SelectedTerritory)
         {
            if( left.SelectedTerritory.ToString() != right.SelectedTerritory.ToString())
            {
               Logger.Log(LogEnum.LE_ERROR, "IsEqual(): left.SelectedTerritory.ToString() != right.SelectedTerritory.ToString()");
               return false;
            }
         }
         //---------------------------------------
         if (false == IsEqual(left.SelectedMapItems, right.SelectedMapItems))
         {
            Logger.Log(LogEnum.LE_ERROR, "IsEqual(IStack): IsEqual( left.SelectedMapItems, right.SelectedMapItems ) returned false");
            return false;
         }
         //---------------------------------------
         if (false == IsEqual(left.DeadPeople, right.DeadPeople))
         {
            Logger.Log(LogEnum.LE_ERROR, "IsEqual(IStack): IsEqual( left.DeadPeople, right.DeadPeople ) returned false");
            return false;
         }
         //---------------------------------------
         if (false == IsEqual(left.Zebulon, right.Zebulon))
         {
            Logger.Log(LogEnum.LE_ERROR, "IsEqual(IStack): IsEqual( left.Zebulon, right.Zebulon ) returned false");
            return false;
         }
         //---------------------------------------
         if (false == IsEqual(left.MapItemCombat, right.MapItemCombat))
         {
            Logger.Log(LogEnum.LE_ERROR, "IsEqual(IStack): IsEqual( left.MapItemCombat, right.MapItemCombat ) returned false");
            return false;
         }
         return true;
      }
      private bool IsEqual(List<RandomMoveData> left, List<RandomMoveData> right)
      {
         if( left.Count != right.Count)
         {
            Logger.Log(LogEnum.LE_ERROR, "IsEqual(): List<RandomMoveData> left.Count != List<RandomMoveData> right.count");
            return false;
         }
         for(int i=0; i< left.Count; i++)
         {
            RandomMoveData moveLeft = left[i];
            RandomMoveData moveRight = right[i];
            if( moveLeft.myName != moveRight.myName)
            {
               Logger.Log(LogEnum.LE_ERROR, "IsEqual(): moveLeft.myName != moveRight.myName");
               return false;
            }
            if (moveLeft.myBuildingName != moveRight.myBuildingName)
            {
               Logger.Log(LogEnum.LE_ERROR, "IsEqual(): moveLeft.myBuildingName != moveRight.myBuildingName");
               return false;
            }
            if (moveLeft.myBrushIndex != moveRight.myBrushIndex)
            {
               Logger.Log(LogEnum.LE_ERROR, "IsEqual(): moveLeft.myBrushIndex != moveRight.myBrushIndex");
               return false;
            }
         }
         return true;
      }
      private bool IsEqual(Dictionary<IMapItem, IMapItem> left, Dictionary<IMapItem, IMapItem> right)
      {
         if (left.Count != right.Count)
         {
            Logger.Log(LogEnum.LE_ERROR, "IsEqual(): Dictionary<IMapItem, IMapItem> left.Count != right.count");
            return false;
         }
         foreach(KeyValuePair<IMapItem,IMapItem> kvp in left)
         {
            bool isMatch = false;
            foreach (KeyValuePair<IMapItem, IMapItem> kvp1 in right)
            {
               if( kvp.Key.Name == kvp1.Key.Name )
               {
                  if (kvp.Value.Name == kvp1.Value.Name)
                  {
                     isMatch = true;
                     break;
                  }
               }
            }
            if (false == isMatch)
            {
               Logger.Log(LogEnum.LE_ERROR, "IsEqual(): Dictionary<IMapItem, IMapItem> did not match=" + kvp.Key.Name);
               return false;
            }
         }
         return true;
      }
      private bool IsEqual(IMapItemMoves left, IMapItemMoves right)
      {
         if (left.Count != right.Count)
         {
            Logger.Log(LogEnum.LE_ERROR, "IsEqual(IMapItemMoves): (left.Count=" + left.Count.ToString() + ") != (right.Count=" + right.Count.ToString() + ")");
            return false;
         }
         for (int i = 0; i < left.Count; ++i)
         {
            IMapItemMove? mimLeft = left[i];
            IMapItemMove? mimRight = right[i];
            if (null == mimLeft || null == mimRight)
            {
               Logger.Log(LogEnum.LE_ERROR, "IsEqual(IStacks): mimLeft=null or mimRight=null");
               return false;
            }
            //----------------------------------------
            if (false == IsEqual(mimLeft.MapItem, mimRight.MapItem))
            {
               Logger.Log(LogEnum.LE_ERROR, "IsEqual(IStacks): IsEqual(mimLeft.MapItem, mimRight.MapItem) returned false");
               return false;
            }
            //----------------------------------------
            if (null == mimLeft.OldTerritory && null != mimRight.OldTerritory) // mismatch in left=null and right
            {
               Logger.Log(LogEnum.LE_ERROR, "IsEqual(): left.OldTerritory=null");
               return false;
            }
            if (null != mimLeft.OldTerritory && null == mimRight.OldTerritory) // mismatch in left and right=null
            {
               Logger.Log(LogEnum.LE_ERROR, "IsEqual(): right.OldTerritory=null");
               return false;
            }
            if (null != mimLeft.OldTerritory && null != mimRight.OldTerritory)
            {
               if (mimLeft.OldTerritory.ToString() != mimRight.OldTerritory.ToString())
               {
                  Logger.Log(LogEnum.LE_ERROR, "IsEqual(): (mimLeft.OldTerritory=" + mimLeft.OldTerritory.ToString() + ") != (mimRight.OldTerritory=" + mimRight.OldTerritory.ToString() + ")");
                  return false;
               }
            }
            //----------------------------------------
            if (null == mimLeft.NewTerritory && null != mimRight.NewTerritory)
            {
               Logger.Log(LogEnum.LE_ERROR, "IsEqual(): left.NewTerritory=null");
               return false;
            }
            if (null != mimLeft.NewTerritory && null == mimRight.NewTerritory)
            {
               Logger.Log(LogEnum.LE_ERROR, "IsEqual(): right.NewTerritory=null");
               return false;
            }
            if (null != mimLeft.NewTerritory && null != mimRight.NewTerritory)
            {
               if (mimLeft.NewTerritory.ToString() != mimRight.NewTerritory.ToString())
               {
                  Logger.Log(LogEnum.LE_ERROR, "IsEqual(): mimLeft.NewTerritory != mimRight.NewTerritory");
                  return false;
               }
            }
            //----------------------------------------
            if (null == mimLeft.BestPath || null == mimRight.BestPath)
            {
               Logger.Log(LogEnum.LE_ERROR, "IsEqual(): mimLeft.BestPath=null or mimRight.BestPath=null");
               return false;
            }
            if (mimLeft.BestPath.Name != mimRight.BestPath.Name)
            {
               Logger.Log(LogEnum.LE_ERROR, "IsEqual(BestPath): (mimLeft.BestPath.Name=" + mimLeft.BestPath.Name + ") != (mimRight.BestPath.Name=" + mimRight.BestPath.Name + ")");
               return false;
            }
            if (Math.Round(mimLeft.BestPath.Metric, 2) != Math.Round(mimRight.BestPath.Metric, 2))
            {
               Logger.Log(LogEnum.LE_ERROR, "IsEqual(IMapItemMoves): Math.Round(mimLeft.BestPath.Metric,2) != Math.Round(mimRight.BestPath.Metric, 2)");
               return false;
            }
            if (mimLeft.BestPath.Territories.Count != mimRight.BestPath.Territories.Count)
            {
               Logger.Log(LogEnum.LE_ERROR, "IsEqual(IMapItemMoves): mimLeft.BestPath.Territories.Count != mimRight.BestPath.Territories.Count");
               return false;
            }
            for (int j = 0; j < mimLeft.BestPath.Territories.Count; ++j)
            {
               ITerritory tLeft = mimLeft.BestPath.Territories[i];
               ITerritory tRight = mimRight.BestPath.Territories[i];
               if (tLeft.ToString() != tRight.ToString())
               {
                  Logger.Log(LogEnum.LE_ERROR, "IsEqual(IMapItemMoves): tLeft.Name != tRight.Name");
                  return false;
               }
            }
         }
         return true;
      }
      private bool IsEqual(IMapItems left, IMapItems right)
      {
         if (left.Count != right.Count)
         {
            Logger.Log(LogEnum.LE_ERROR, "IsEqual(): left.Count != right.COunt");
            return false;
         }
         for (int i = 0; i < left.Count; ++i)
         {
            if (false == IsEqual(left[i], right[i]))
            {
               Logger.Log(LogEnum.LE_ERROR, "IsEqual(IMapItems): left[i] != right[i]");
               return false;
            }
         }
         return true;
      }
      private bool IsEqual(IMapItem? left, IMapItem? right)
      {
         if (null == left)
         {
            Logger.Log(LogEnum.LE_ERROR, "IsEqual(IMapItem): left=null");
            return false;
         }
         if (null == right)
         {
            Logger.Log(LogEnum.LE_ERROR, "IsEqual(IMapItem): right=null");
            return false;
         }
         if (left.Name != right.Name)
         {
            Logger.Log(LogEnum.LE_ERROR, "IsEqual(IMapItem): left.Name != right.Name");
            return false;
         }
         if (left.TopImageName != right.TopImageName)
         {
            Logger.Log(LogEnum.LE_ERROR, "IsEqual(IMapItem): left.TopImageName != right.TopImageName");
            return false;
         }
         if (left.BottomImageName != right.BottomImageName)
         {
            Logger.Log(LogEnum.LE_ERROR, "IsEqual(IMapItem): left.BottomImageName != right.BottomImageName");
            return false;
         }
         if (left.OverlayImageName != right.OverlayImageName)
         {
            Logger.Log(LogEnum.LE_ERROR, "IsEqual(IMapItem): left.OverlayImageName != right.OverlayImageName");
            return false;
         }
         if (left.Zoom != right.Zoom)
         {
            Logger.Log(LogEnum.LE_ERROR, "IsEqual(IMapItem): left.Zoom != right.Zoom");
            return false;
         }
         if (left.IsMoved != right.IsMoved)
         {
            Logger.Log(LogEnum.LE_ERROR, "IsEqual(IMapItem): left.IsMoved != right.IsMoved");
            return false;
         }
         if (left.IsKilled != right.IsKilled)
         {
            Logger.Log(LogEnum.LE_ERROR, "IsEqual(IMapItem): left.IsKilled != right.IsKilled");
            return false;
         }
         if (left.Location.ToString() != right.Location.ToString())
         {
            Logger.Log(LogEnum.LE_ERROR, "IsEqual(IMapItem): left.Location != right.Location");
            return false;
         }
         //-------------------------------------------------
         if (left.TerritoryCurrent.Name != right.TerritoryCurrent.Name)
         {
            Logger.Log(LogEnum.LE_ERROR, "IsEqual(IMapItem): Name left.TerritoryCurrent != right.TerritoryCurrent");
            return false;
         }
         if (left.TerritoryStarting.Name != right.TerritoryStarting.Name)
         {
            Logger.Log(LogEnum.LE_ERROR, "IsEqual(IMapItem): Name left.TerritoryStarting != right.TerritoryStarting");
            return false;
         }
         if (left.Combat != right.Combat)
         {
            Logger.Log(LogEnum.LE_ERROR, "IsEqual(IMapItem): Name left.Combat != right.Combat");
            return false;
         }
         if (left.Influence != right.Influence)
         {
            Logger.Log(LogEnum.LE_ERROR, "IsEqual(IMapItem): Name left.Influence != right.Influence");
            return false;
         }
         if (left.Movement != right.Movement)
         {
            Logger.Log(LogEnum.LE_ERROR, "IsEqual(IMapItem): Name left.Movement != right.Movement");
            return false;
         }
         if (left.MovementOriginal != right.MovementOriginal)
         {
            Logger.Log(LogEnum.LE_ERROR, "IsEqual(IMapItem): Name left.MovementOriginal != right.MovementOriginal");
            return false;
         }
         if (left.MovementUsed != right.MovementUsed)
         {
            Logger.Log(LogEnum.LE_ERROR, "IsEqual(IMapItem): Name left.MovementUsed != right.MovementUsed");
            return false;
         }
         if (left.IsKnockedout != right.IsKnockedout)
         {
            Logger.Log(LogEnum.LE_ERROR, "IsEqual(IMapItem): Name left.IsKnockedout != right.IsKnockedout");
            return false;
         }
         if (left.IsKnockedoutThisTurn != right.IsKnockedoutThisTurn)
         {
            Logger.Log(LogEnum.LE_ERROR, "IsEqual(IMapItem): Name left.IsKnockedoutThisTurn != right.IsKnockedoutThisTurn");
            return false;
         }
         if (left.IsAlienUnknown != right.IsAlienUnknown)
         {
            Logger.Log(LogEnum.LE_ERROR, "IsEqual(IMapItem): Name left.IsAlienUnknown != right.IsAlienUnknown");
            return false;
         }
         if (left.IsAlienKnown != right.IsAlienKnown)
         {
            Logger.Log(LogEnum.LE_ERROR, "IsEqual(IMapItem): left.IsAlienKnown != right.IsAlienKnown");
            return false;
         }
         if (left.IsControlled != right.IsControlled)
         {
            Logger.Log(LogEnum.LE_ERROR, "IsEqual(IMapItem): (left.IsControlled=" + left.IsControlled.ToString() + ") != (right.IsControlled=" + right.IsControlled.ToString() + ") for mi=" + left.Name);
            return false;
         }
         if (left.IsImplantHeld != right.IsImplantHeld)
         {
            Logger.Log(LogEnum.LE_ERROR, "IsEqual(IMapItem): Name left.IsImplantHeld != right.IsImplantHeld");
            return false;
         }
         if (left.IsImplantRemovalAttempt != right.IsImplantRemovalAttempt)
         {
            Logger.Log(LogEnum.LE_ERROR, "IsEqual(IMapItem): Name left.IsImplantRemovalAttempt != right.IsImplantRemovalAttempt");
            return false;
         }
         if (left.IsInterrogated != right.IsInterrogated)
         {
            Logger.Log(LogEnum.LE_ERROR, "IsEqual(IMapItem): Name left.IsInterrogated != right.IsInterrogated");
            return false;
         }
         if (left.IsSkeptical != right.IsSkeptical)
         {
            Logger.Log(LogEnum.LE_ERROR, "IsEqual(IMapItem): Name left.IsSkeptical != right.IsSkeptical");
            return false;
         }
         if (left.IsStunned != right.IsStunned)
         {
            Logger.Log(LogEnum.LE_ERROR, "IsEqual(IMapItem): Name left.IsStunned != right.IsStunned");
            return false;
         }
         if (left.IsStunnedThisTurn != right.IsStunnedThisTurn)
         {
            Logger.Log(LogEnum.LE_ERROR, "IsEqual(IMapItem): Name left.IsStunnedThisTurn != right.IsStunnedThisTurn");
            return false;
         }
         if (left.IsSurrendered != right.IsSurrendered)
         {
            Logger.Log(LogEnum.LE_ERROR, "IsEqual(IMapItem): Name left.IsSurrendered != right.IsSurrendered");
            return false;
         }
         if (left.IsTiedUp != right.IsTiedUp)
         {
            Logger.Log(LogEnum.LE_ERROR, "IsEqual(IMapItem): Name left.IsTiedUp != right.IsTiedUp");
            return false;
         }
         if (left.IsWary != right.IsWary)
         {
            Logger.Log(LogEnum.LE_ERROR, "IsEqual(IMapItem): Name left.IsWary != right.IsWary");
            return false;
         }
         if (left.IsMovingThisTurn != right.IsMovingThisTurn)
         {
            Logger.Log(LogEnum.LE_ERROR, "IsEqual(IMapItem): Name left.IsMovingThisTurn != right.IsMovingThisTurn");
            return false;
         }
         if (left.IsConversedThisTurn != right.IsConversedThisTurn)
         {
            Logger.Log(LogEnum.LE_ERROR, "IsEqual(IMapItem): Name left.IsConversedThisTurn != right.IsConversedThisTurn");
            return false;
         }
         if (left.IsInfluencedThisTurn != right.IsInfluencedThisTurn)
         {
            Logger.Log(LogEnum.LE_ERROR, "IsEqual(IMapItem): Name left.IsInfluencedThisTurn != right.IsInfluencedThisTurn");
            return false;
         }
         if (left.IsCombatThisTurn != right.IsCombatThisTurn)
         {
            Logger.Log(LogEnum.LE_ERROR, "IsEqual(IMapItem): Name left.IsCombatThisTurn != right.IsCombatThisTurn");
            return false;
         }
         if (left.IsImplantRemovalAttemptThisTurn != right.IsImplantRemovalAttemptThisTurn)
         {
            Logger.Log(LogEnum.LE_ERROR, "IsEqual(IMapItem): Name left.IsImplantRemovalAttemptThisTurn != right.IsImplantRemovalAttemptThisTurn");
            return false;
         }
         return true;
      }
      private bool IsEqual(IStacks left, IStacks right)
      {
         if (left.Count != right.Count)
         {
            Logger.Log(LogEnum.LE_ERROR, "IsEqual(IStacks): left.Count != right.Count");
            return false;
         }
         for (int i = 0; i < left.Count; ++i)
         {
            IStack? sLeft = left[i];
            IStack? sRight = right[i];
            if (null == sLeft || null == sRight)
            {
               Logger.Log(LogEnum.LE_ERROR, "IsEqual(IStacks): sLeft=null or sRight=null");
               return false;
            }
            if( false == IsEqual(sLeft, sRight))
            {
               Logger.Log(LogEnum.LE_ERROR, "IsEqual(IStacks): IsEqual(sLeft, sRight) returned false");
               return false;
            }
         }
         return true;
      }
      private bool IsEqual(IStack? left, IStack? right)
      {
         if (null == left && null != right) // mismatch in left=null and right
         {
            Logger.Log(LogEnum.LE_ERROR, "IsEqual(): left=null");
            return false;
         }
         if (null != left && null == right) // mismatch in left and right=null
         {
            Logger.Log(LogEnum.LE_ERROR, "IsEqual(): right=null");
            return false;
         }
         if (null != left && null != right)
         {
            if (left.Territory.ToString() != right.Territory.ToString())
            {
               Logger.Log(LogEnum.LE_ERROR, "IsEqual(IStack): left.Territory.ToString() != right.Territory.ToString()");
               return false;
            }
            if ( left.IsStacked != right.IsStacked )
            {
               Logger.Log(LogEnum.LE_ERROR, "IsEqual(IStack): left.IsStacked != right.IsStacked");
               return false;
            }
            if (false == IsEqual(left.MapItems, right.MapItems))
            {
               Logger.Log(LogEnum.LE_ERROR, "IsEqual(IStack): IsEqual( left.MapItems, right.MapItems ) returned false");
               return false;
            }
         }
         return true;
      }
      private bool IsEqual(ITerritories left, ITerritories right)
      {
         if (left.Count != right.Count)
         {
            Logger.Log(LogEnum.LE_ERROR, "IsEqual(ITerritories): (left.Count=" + left.Count.ToString() + ") != (right.Count" + right.Count.ToString() + ")");
            return false;
         }
         for (int i = 0; i < left.Count; ++i)
         {
            ITerritory? tLeft = left[i];
            if (null == tLeft)
            {
               Logger.Log(LogEnum.LE_ERROR, "IsEqual(ITerritories): tLeft=null");
               return false;
            }
            ITerritory? tRight = right[i];
            if (null == tRight)
            {
               Logger.Log(LogEnum.LE_ERROR, "IsEqual(ITerritories): tRight=null");
               return false;
            }
            if (tLeft.ToString() != tRight.ToString())
            {
               Logger.Log(LogEnum.LE_ERROR, "IsEqual(ITerritories): tLeft.ToString() != tRight.ToString()");
               return false;
            }
         }
         return true;
      }
      private bool IsEqual(IMapItemCombat left, IMapItemCombat right)
      {
         if( left.Attackers.Count != right.Attackers.Count )
         {
            Logger.Log(LogEnum.LE_ERROR, "IsEqual(IMapItemCombat): left.Attackers.Count != right.Attackers.Count");
            return false;
         }
         if (left.Defenders.Count != right.Defenders.Count)
         {
            Logger.Log(LogEnum.LE_ERROR, "IsEqual(IMapItemCombat): left.Defenders.Count != right.Defenders.Count");
            return false;
         }
         if (left.Result != right.Result)
         {
            Logger.Log(LogEnum.LE_ERROR, "IsEqual(IMapItemCombat): left.Result != right.Result");
            return false;
         }
         if (left.DieRoll != right.DieRoll)
         {
            Logger.Log(LogEnum.LE_ERROR, "IsEqual(IMapItemCombat): left.DieRoll != right.DieRoll");
            return false;
         }
         return true;
      }
   }
}
