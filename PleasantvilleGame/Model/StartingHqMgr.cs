using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PleasantvilleGame
{
   public static class StartingHqMgr
   {
      private static List<string> theStartHqTerritories = new List<string>();
      private static List<int> theProbabilities = new List<int>();
      public static bool SetInitial() // read in name and probability name exists
      {
         string filename = ConfigFileReader.theConfigDirectory + @"StartingHqTerritories.txt";
         try
         {
            StreamReader sr = File.OpenText(filename);
            int i = 0;
            while (i < 1500)
            {
               string? line = sr.ReadLine();
               if (null == line)
                  break;
               line = line.Trim();
               if (0 < line.Length)
               {
                  string[] aStringArray = line.Split(new char[] { ',' });
                  theStartHqTerritories.Add(aStringArray[0]);
                  theProbabilities.Add(int.Parse(aStringArray[1]));
               }
            }
            return true;
         }
         catch (Exception e)
         {
            Logger.Log(LogEnum.LE_ERROR, "InitNames(): e=" + e.ToString());
            return false;
         }
      }
      public static string GetStartingHqTerritory()
      {
         int randomNum = Utilities.RandomGenerator.Next(114470);
         //int randomNum = Utilities.RandomGenerator.Next(8000); // for testing - make sure all territories are being returned
         string retValue = theStartHqTerritories[0];
         for (int i = 0; i < theProbabilities.Count; ++i) // step through probablities - find when the random number is less than probablility for that number. In that event, return that name.
         {
            if (randomNum < theProbabilities[i])
               return retValue;
            retValue = theStartHqTerritories[i];
         }
         return theStartHqTerritories[66];
      }
   }
}
