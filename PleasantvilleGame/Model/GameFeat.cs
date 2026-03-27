using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Security.RightsManagement;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Xml.Serialization;

namespace PleasantvilleGame
{
   [Serializable]
   public class GameFeat
   {
      public string Key { get; set; } = string.Empty;
      public int Value { get; set; } = 0;
      public int Threshold { get; set; } = 1;
      public GameFeat()
      {
      }
      public GameFeat(string name)
      {
         Key = name;
      }
      public GameFeat(string name, int value)
      {
         Key = name;
         Value = value;
      }
      //----------------------------------
      public GameFeat Clone()
      {
         return new GameFeat(this.Key, this.Value);
      }
      public override string ToString()
      {
         StringBuilder sb = new StringBuilder();
         sb.Append("(k=");
         sb.Append(this.Key.ToString());
         sb.Append("->");
         sb.Append(this.Value.ToString());
         sb.Append(")");
         return sb.ToString();
      }
   }
   //========================================
   [XmlInclude(typeof(GameFeat))]
   [Serializable]
   public class GameFeats : IEnumerable
   {
      [NonSerialized] public static string theGameFeatDirectory = "";
      [NonSerialized] public static string[] theDefaults =
      {

      };
      private readonly ArrayList myList;
      public static string GetFeatMessage(GameFeat feat, bool isThreshold = false)
      {
         StringBuilder sb = new StringBuilder();
         switch (feat.Key)
         {
            case "StiffBack":
               if (0 < feat.Value)
                  return "Stiff Spine: Win a Combat with no controlled US sectors";
               else
                  return "Stiff Spine";
            default:
               Logger.Log(LogEnum.LE_ERROR, "GetFeatMessage(): Unknown key=" + feat.Key);
               return "UNKNOWN: " + feat.Key;
         }
      }
      //---------------------------------------------------
      public GameFeats() { myList = new ArrayList(); }
      public int Count { get => myList.Count; }
      public void Add(GameFeat o) { myList.Add(o); }
      public void Add(object o) { myList.Add(o); }
      public void Insert(int index, GameFeat o) { myList.Insert(index, o); }
      public void Clear() { myList.Clear(); }
      public bool Contains(GameFeat o) { return myList.Contains(o); }
      public IEnumerator GetEnumerator() { return myList.GetEnumerator(); }
      public int IndexOf(GameFeat o) { return myList.IndexOf(o); }
      public GameFeat? Find(string key)
      {
         int i = 0;
         foreach (object o in myList)
         {
            GameFeat? feat = (GameFeat)o;
            if (null == feat)
               continue;
            if (key == feat.Key)
               return feat;
            ++i;
         }
         return null;
      }
      public GameFeat? RemoveAt(int index)
      {
         GameFeat? feat = myList[index] as GameFeat;
         myList.RemoveAt(index);
         return feat;
      }
      public GameFeat? this[int index]
      {
         get { GameFeat? o = myList[index] as GameFeat; return o; }
         set { myList[index] = value; }
      }
      public GameFeats Clone()
      {
         GameFeats copy = new GameFeats();
         foreach (object o in myList)
         {
            GameFeat feat = (GameFeat)o;
            GameFeat copyFeat = new GameFeat(feat.Key, feat.Value);
            copy.Add(copyFeat);
         }
         return copy;
      }
      public bool GetFeatChange(GameFeats rightFeats, out GameFeat changedFeat)
      {
         changedFeat = new GameFeat();
         if (this.Count < rightFeats.Count) // this should not happen
         {
            Logger.Log(LogEnum.LE_ERROR, "Get_FeatChange(): (rightFeats.Count=" + rightFeats.Count.ToString() + ") > (this.Count=" + this.Count.ToString() + ")");
            return false;
         }
         if (rightFeats.Count < this.Count)
         {
            for (int i = rightFeats.Count; i < this.Count; ++i)
            {
               GameFeat? feat = this[i];
               if (null == feat)
               {
                  Logger.Log(LogEnum.LE_ERROR, "Get_FeatChange(): feat is null for i=" + i.ToString());
                  return false;
               }
               rightFeats.Add(feat);
            }
         }
         //--------------------------------------------
         for (int i = 0; i < rightFeats.Count; ++i)
         {
            GameFeat? right = rightFeats[i];
            if (null == right)
            {
               Logger.Log(LogEnum.LE_ERROR, "Get_FeatChange(): right=null for i=" + i.ToString());
               return false;
            }
            GameFeat? left = this[i];
            if (null == left)
            {
               Logger.Log(LogEnum.LE_ERROR, "Get_FeatChange(): left=null for i=" + i.ToString());
               return false;
            }
            if (left.Key != right.Key)
            {
               Logger.Log(LogEnum.LE_ERROR, "Get_FeatChange(): left.key=" + left.Key + " right.key=" + right.Key);
               return false;
            }
            if (left.Value != right.Value)
            {
               if (0 == left.Threshold)
               {
                  if (1 == left.Value) // only want to show this GameFeat one time when value is 1
                  {
                     Logger.Log(LogEnum.LE_VIEW_SHOW_FEATS, "Get_FeatChange(): No Threshold Key=" + right.Key + " (left.Value=" + left.Value.ToString() + ") != (right.Value =" + right.Value.ToString() + ")");
                     changedFeat = left;
                  }
                  else
                  {
                     right.Value = left.Value; // if not at threshold, ignore but update to current value
                  }
                  return true;
               }
               else if (0 == left.Value % left.Threshold) // when the value reaches an iterative threshold, show feat
               {
                  Logger.Log(LogEnum.LE_VIEW_SHOW_FEATS, "Get_FeatChange(): Reached Threshold=" + left.Threshold.ToString() + " Key=" + right.Key + " (left.Value=" + left.Value.ToString() + ") != (right.Value =" + right.Value.ToString() + ")");
                  changedFeat = left;
                  return true;
               }
               else
               {
                  right.Value = left.Value; // if not at threshold, ignore but update to current value
               }
            }
         }
         return true;
      }
      public void SetOriginalGameFeats()
      {
         Clear();
         foreach (string s in theDefaults)
            Add(new GameFeat(s));
      }
      public void SetGameFeatThreshold()
      {
         foreach (GameFeat feat in this)
         {
            int threshold = 0;
            switch (feat.Key)
            {
               case "NumShermanExplodes": threshold = 3; break;
               default: threshold = 0; break;
            }
            feat.Threshold = threshold;
         }
      }
      public void SetValue(string key, int value)
      {
         GameFeat? o = Find(key);
         if (null == o)
         {
            Logger.Log(LogEnum.LE_ERROR, "Set_Value(): null for key=" + key);
            o = new GameFeat(key);
            this.myList.Add(o);
         }
         o.Value = value;
      }
      public void AddOne(string key)
      {
         GameFeat? o = Find(key);
         if (null == o)
         {
            Logger.Log(LogEnum.LE_ERROR, "Add_One(): null for key=" + key);
            o = new GameFeat(key);
            this.myList.Add(o);
         }
         o.Value++;
         Logger.Log(LogEnum.LE_VIEW_SHOW_FEATS, "Add_One():  key=" + o.Key + " value=" + o.Value);
      }
      public override string ToString()
      {
         StringBuilder sb = new StringBuilder();
         sb.Append("[");
         foreach (Object obj in myList)
         {
            GameFeat feat = (GameFeat)obj;
            sb.Append(feat.ToString());
         }
         sb.Append("]");
         return sb.ToString();
      }
   }
}
