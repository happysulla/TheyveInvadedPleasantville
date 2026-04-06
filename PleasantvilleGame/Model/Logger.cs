using System;
using System.IO;
using System.Threading;

namespace PleasantvilleGame
{
   public enum LogEnum
   {
      LE_ERROR,
      LE_GAME_INIT,
      LE_GAME_INIT_VERSION,
      LE_GAME_END,
      LE_GAME_END_CHECK,
      LE_NEXT_ACTION,
      LE_UNDO_COMMAND,
      LE_MOVE_STACKING,
      LE_MOVE_COUNT,
      LE_SHOW_STACK_ADD,
      LE_SHOW_STACK_DEL,
      LE_SHOW_STACK_VIEW,
      LE_SHOW_MAIN_CLEAR,
      LE_SHOW_ENTERED_HEX,
      LE_SHOW_BUTTON_MOVE,
      //-------------
      LE_VIEW_SHOW_OPTIONS,
      LE_VIEW_SHOW_FEATS,
      LE_VIEW_SHOW_STATS,
      LE_VIEW_SHOW_GAMESAVES,
      LE_VIEW_SHOW_STATS_MIN,
      LE_VIEW_SHOW_SETTINGS,
      LE_SHOW_UPLOAD_GAME,
      LE_SHOW_VP_TOTAL,
      //-------------
      LE_RESET_ROLL_STATE,
      LE_VIEW_DICE_MOVING,
      LE_VIEW_UPDATE_MENU,
      LE_VIEW_UPDATE_STATUS_BAR,
      LE_VIEW_UPDATE_EVENTVIEWER,
      LE_VIEW_APPEND_EVENT,
      LE_VIEW_MIM,
      LE_VIEW_MIM_ADD,
      LE_VIEW_MIM_CLEAR,
      LE_VIEW_CONTROL_NAME,
      //-------------
      LE_TIMER_ELAPED,
      LE_COMBAT_SUMS,
      LE_INFLUENCE_CHANGE,
      LE_GAMESTATE_CHECKER,
      LE_GAMESTATE_CHECKER_TIED_UP,
      LE_END_ENUM
   }
   //----------------------------------------------------------------------------
   public class Logger
   {
      const int NUM_LOG_LEVELS = (int)LogEnum.LE_END_ENUM;
      public static bool[] theLogLevel = new bool[NUM_LOG_LEVELS];
      public static string theLogDirectory = "";
      private static string theFileName = "";
      private static bool theIsLogFileCreated = false;
      private static Mutex theMutex = new Mutex();
      //--------------------------------------------------
      static public bool SetInitial()
      {
         try // create the file
         {
            if (false == Directory.Exists(theLogDirectory))
               Directory.CreateDirectory(theLogDirectory);
            theFileName = theLogDirectory + DateTime.Now.ToString("yyyyMMdd-HHmmss") + ".txt";
            FileInfo f = new FileInfo(theFileName);
            f.Create();
            theIsLogFileCreated = true;
         }
         catch (DirectoryNotFoundException dirException)
         {
            Console.WriteLine("SetInitial(): create file\n" + dirException.ToString());
         }
         catch (FileNotFoundException fileException)
         {
            Console.WriteLine("SetInitial(): create file\n" + fileException.ToString());
         }
         catch (IOException ioException)
         {
            Console.WriteLine("SetInitial(): create file\n" + ioException.ToString());
         }
         catch (Exception ex)
         {
            Console.WriteLine("SetInitial(): create file\n" + ex.ToString());
         }
         SetOn(LogEnum.LE_ERROR);
         SetOn(LogEnum.LE_GAME_INIT);
         SetOn(LogEnum.LE_GAME_INIT_VERSION);
         SetOn(LogEnum.LE_GAME_END);
         SetOn(LogEnum.LE_GAME_END_CHECK);
         SetOn(LogEnum.LE_NEXT_ACTION);
         //-------------
         SetOn(LogEnum.LE_MOVE_STACKING);
         SetOn(LogEnum.LE_MOVE_COUNT);
         SetOn(LogEnum.LE_SHOW_STACK_ADD);
         SetOn(LogEnum.LE_SHOW_STACK_DEL);
         SetOn(LogEnum.LE_SHOW_STACK_VIEW);
         SetOn(LogEnum.LE_SHOW_MAIN_CLEAR);
         SetOn(LogEnum.LE_SHOW_ENTERED_HEX);
         SetOn(LogEnum.LE_SHOW_BUTTON_MOVE);
         //-------------
         SetOn(LogEnum.LE_VIEW_SHOW_OPTIONS);
         SetOn(LogEnum.LE_VIEW_SHOW_FEATS);
         SetOn(LogEnum.LE_VIEW_SHOW_STATS);
         SetOn(LogEnum.LE_VIEW_SHOW_GAMESAVES);
         SetOn(LogEnum.LE_VIEW_SHOW_STATS_MIN);
         //-------------
         SetOn(LogEnum.LE_VIEW_SHOW_SETTINGS);
         SetOn(LogEnum.LE_SHOW_UPLOAD_GAME);
         SetOn(LogEnum.LE_SHOW_VP_TOTAL);
         //-------------
         SetOn(LogEnum.LE_RESET_ROLL_STATE);
         SetOn(LogEnum.LE_VIEW_DICE_MOVING);
         SetOn(LogEnum.LE_VIEW_UPDATE_MENU);
         SetOn(LogEnum.LE_VIEW_UPDATE_STATUS_BAR);
         SetOn(LogEnum.LE_VIEW_UPDATE_EVENTVIEWER);
         SetOn(LogEnum.LE_VIEW_APPEND_EVENT);
         SetOn(LogEnum.LE_VIEW_MIM);
         SetOn(LogEnum.LE_VIEW_MIM_ADD);
         SetOn(LogEnum.LE_VIEW_UPDATE_MENU);
         SetOn(LogEnum.LE_VIEW_MIM_CLEAR);
         SetOn(LogEnum.LE_VIEW_CONTROL_NAME);
         return true;
      }
      static public void SetOn(LogEnum logLevel)
      {
         if ((int)logLevel < NUM_LOG_LEVELS)
            theLogLevel[(int)logLevel] = true;
      }
      static public void SetOff(LogEnum logLevel)
      {
         if ((int)logLevel < NUM_LOG_LEVELS)
            theLogLevel[(int)logLevel] = false;
      }
      static public void Log(LogEnum logLevel, string description)
      {
         if (true == theLogLevel[(int)logLevel])
         {
            theMutex.WaitOne();
            System.Diagnostics.Debug.WriteLine("{0} {1}", logLevel.ToString(), description);
            if (false == theIsLogFileCreated)
            {
               theMutex.ReleaseMutex();
               return;
            }
            try
            {
               FileInfo file = new FileInfo(theFileName);
               if (true == File.Exists(theFileName))
               {
                  StreamWriter swriter = File.AppendText(theFileName);
                  swriter.Write(logLevel.ToString());
                  swriter.Write(" ");
                  swriter.Write(description);
                  swriter.Write("\n");
                  swriter.Close();
               }
            }
            catch (FileNotFoundException fileex)
            {
              System.Diagnostics.Debug.WriteLine("Log(): ll=" + logLevel.ToString() + " desc=" + description + "\n" + fileex.ToString());
            }
            catch (IOException)
            {
               System.Diagnostics.Debug.WriteLine("Log(): ll=" + logLevel.ToString() + " desc=" + description);
            }
            catch (Exception ex)
            {
               System.Diagnostics.Debug.WriteLine("Log(): ll=" + logLevel.ToString() + " desc=" + description + "\n" + ex.ToString());
            }
            theMutex.ReleaseMutex();
         }
      }
   }
}
