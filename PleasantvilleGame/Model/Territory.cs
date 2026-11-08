using System.Collections;
using System.Text;
using System.Windows.Media;
using System.Xml.Linq;
using Point = System.Windows.Point;

namespace PleasantvilleGame
{
   [Serializable]
	public class Territory : ITerritory
	{
      public static int ERROR = -1000;
      public static int MAX_PATH_COUNT = 30;
		public string Name { get; set; } = "Offboard";
		public string CanvasName { get; set; } = "Main";
		public string Subname { get; set; } = "";
		public IMapPoint CenterPoint { get; set; } = new MapPoint();
		public List<IMapPoint> Points { get; set; } = new List<IMapPoint>();
		public List<String> Adjacents { get; set; } = new List<String>();
		public Dictionary<String, double> Observations { get; set; } = new Dictionary<String, double>();
      public bool IsBuilding()
      {
         int arraySize = TableMgr.theBuildingSizes.GetLength(0);
         for (int i = 0; i < arraySize; i++)
         {
            string matchingName = Utilities.RemoveSpaces(TableMgr.theBuildingSizes[i, 0]);
            if (true == Name.Contains(matchingName))
               return true;
         }
         return false;
      }
      //---------------------------------------------------------------
      public static bool IsPointInPolygon(ITerritory t, Point point)
      {
         if (0 == t.Points.Count)
         {
            //Logger.Log(LogEnum.LE_ERROR, "Is_PointInPolygon(): t.Points.Count=0 for t.Name=" + t.Name);
            return false;
         }
         int intersections = 0;
         int count = t.Points.Count;
         for (int i = 0; i < count; i++)
         {
            Point vertex1 = new Point(t.Points[i].X, t.Points[i].Y);
            Point vertex2 = new Point(t.Points[(i + 1) % count].X, t.Points[(i + 1) % count].Y);
            if ((point.Y > vertex1.Y) != (point.Y > vertex2.Y)) // Check if the ray intersects the edge
            {
               double slope = (vertex2.X - vertex1.X) / (vertex2.Y - vertex1.Y);
               double intersectX = vertex1.X + slope * (point.Y - vertex1.Y);
               if (point.X < intersectX)
                  intersections++;
            }
         }
         return (intersections % 2) != 0; // Odd number of intersections means the point is inside
      }
      public static IMapPoint GetClosestPointFromTerritorySegmentPoints(ITerritory t, Point p, double offset)
      {
         if (0 == t.Points.Count)
         {
            Logger.Log(LogEnum.LE_ERROR, "Get_ClosestPointFromTerritoryPoints(): t.Points.Count=0 for t.Name=" + t.Name);
            return new MapPoint(t.CenterPoint.X - offset, t.CenterPoint.Y - offset);
         }
         //-----------------------------------
         PointCollection points = new PointCollection();
         foreach (IMapPoint mp1 in t.Points)
            points.Add(new System.Windows.Point(mp1.X, mp1.Y));
         //-----------------------------------
         double minDistance = double.MaxValue;
         Point minClosestPoint = new Point(0, 0);
         for (int i = 0; i < t.Points.Count; i++)
         {
            Point start = points[i];
            Point end = points[(i + 1) % points.Count];           // Loop back to the first point
            Point closestPoint;
            double distance = DistanceToSegment(p, start, end, out closestPoint);   // Calculate distance to the edge
            if (distance < minDistance)
            {
               minDistance = distance;
               minClosestPoint = closestPoint;
            }
         }
         return new MapPoint(minClosestPoint.X - offset, minClosestPoint.Y - offset);
      }
      public static IMapPoint GetRandomPointBoundingRect(ITerritory t, double offset) // return the top left location of a MapItem, not the center point
      {
         if (0 == t.Points.Count)
         {
            Logger.Log(LogEnum.LE_ERROR, "Get_RandomPointBoundingRect(): t.Points.Count=0 for t.Name=" + t.ToString());
            return t.CenterPoint;
         }
         //----------------------------------------------------
         // Make a StreamGeometry object from t.Points 
         StreamGeometry geometry = new StreamGeometry();
         using (StreamGeometryContext ctx = geometry.Open())
         {
            IMapPoint mp0 = t.Points[0];
            System.Windows.Point point0 = new System.Windows.Point(mp0.X, mp0.Y);
            ctx.BeginFigure(point0, true, true); //  filled and closed
            for (int i = 1; i < t.Points.Count; ++i)
            {
               IMapPoint mpI = t.Points[i];
               System.Windows.Point pointI = new System.Windows.Point(mpI.X, mpI.Y);
               ctx.LineTo(pointI, true, false);
            }
            geometry.Freeze();
         }
         System.Windows.Rect rect = geometry.Bounds;
         //----------------------------------------------------
         int count = 20;
         while (0 < --count) // offset is the difference between MapItem location on screen and the center of the  MapItem.
         {
            double XCenter = Utilities.RandomGenerator.Next((int)rect.Left, (int)rect.Right) + offset; // Get a random point in the bounding box
            double YCenter = Utilities.RandomGenerator.Next((int)rect.Top, (int)rect.Bottom) + offset;
            System.Windows.Point pCenter = new System.Windows.Point(XCenter, YCenter);
            if (true == IsPointInPolygon(t, pCenter))
            {
               System.Windows.Point p1 = new System.Windows.Point(XCenter - offset, YCenter - offset);
               System.Windows.Point p2 = new System.Windows.Point(XCenter + offset, YCenter - offset);
               System.Windows.Point p3 = new System.Windows.Point(XCenter - offset, YCenter + offset);
               System.Windows.Point p4 = new System.Windows.Point(XCenter + offset, YCenter + offset);
               bool isP1In = IsPointInPolygon(t, p1);
               bool isP2In = IsPointInPolygon(t, p2);
               bool isP3In = IsPointInPolygon(t, p3);
               bool isP4In = IsPointInPolygon(t, p4);
               if (false == isP1In && false == isP2In)  // try to adjust location so that four corners are inside the region
               {
                  YCenter += offset;
               }
               else if (false == isP3In && false == isP4In)
               {
                  YCenter -= offset;
               }
               else if (false == isP1In && false == isP3In)
               {
                  XCenter += offset;
               }
               else if (false == isP2In && false == isP4In)
               {
                  XCenter -= offset;
               }
               else if (false == isP1In && true == isP2In)
               {
                  XCenter += offset;
               }
               else if (true == isP1In && false == isP2In)
               {
                  XCenter -= offset;
               }
               else if (true == isP3In && false == isP4In)
               {
                  YCenter -= offset;
               }
               else if (false == isP3In && true == isP4In)
               {
                  YCenter -= offset;
               }
               System.Windows.Point p5 = new System.Windows.Point(XCenter - offset, YCenter - offset); // do a final check to make sure point is in region
               if (true == IsPointInPolygon(t, p5))
                  return new MapPoint(p5.X, p5.Y);
            }
         }
         Logger.Log(LogEnum.LE_ERROR, "Get_RandomPointBoundingRect(): Cannot find a random point in t.Name=" + t.Name + " rect=" + rect.ToString());
         return new MapPoint(t.CenterPoint.X - offset, t.CenterPoint.Y - offset);
      }
      public static IMapPoint GetRandomPointInTerritoryRegion(ITerritory t, Point pCenter, double offset)
      {
         if (0 == t.Points.Count)
         {
            Logger.Log(LogEnum.LE_ERROR, "Get_RandomPointInTerritoryRegion(): t.Points.Count=0 for t.Name=" + t.Name);
            return t.CenterPoint;
         }
         //---------------------------------
         int count = 20;
         while (0 < --count) // offset is the difference between MapItem location on screen and the center of the MapItem.
         {
            double XCenter = pCenter.X + offset; // Get a random point in the bounding box
            double YCenter = pCenter.Y + offset;
            if (true == IsPointInPolygon(t, pCenter))
            {
               System.Windows.Point p1 = new System.Windows.Point(XCenter - offset, YCenter - offset);
               System.Windows.Point p2 = new System.Windows.Point(XCenter + offset, YCenter - offset);
               System.Windows.Point p3 = new System.Windows.Point(XCenter - offset, YCenter + offset);
               System.Windows.Point p4 = new System.Windows.Point(XCenter + offset, YCenter + offset);
               bool isP1In = IsPointInPolygon(t, p1);
               bool isP2In = IsPointInPolygon(t, p2);
               bool isP3In = IsPointInPolygon(t, p3);
               bool isP4In = IsPointInPolygon(t, p4);
               if (false == isP1In && false == isP2In)  // try to adjust location so that four corners are inside the region
               {
                  YCenter += offset;
               }
               else if (false == isP3In && false == isP4In)
               {
                  YCenter -= offset;
               }
               else if (false == isP1In && false == isP3In)
               {
                  XCenter += offset;
               }
               else if (false == isP2In && false == isP4In)
               {
                  XCenter -= offset;
               }
               else if (false == isP1In && true == isP2In)
               {
                  XCenter += offset;
               }
               else if (true == isP1In && false == isP2In)
               {
                  XCenter -= offset;
               }
               else if (true == isP3In && false == isP4In)
               {
                  YCenter -= offset;
               }
               else if (false == isP3In && true == isP4In)
               {
                  YCenter -= offset;
               }
               System.Windows.Point p5 = new System.Windows.Point(XCenter - offset, YCenter - offset); // do a final check to make sure point is in region
               if (true == IsPointInPolygon(t, p5))
                  return new MapPoint(p5.X, p5.Y);
            }
         }
         Logger.Log(LogEnum.LE_ERROR, "Get_RandomPointInTerritoryRegion(): Cannot find a random point in t.Name=" + t.Name);
         return new MapPoint(t.CenterPoint.X - offset, t.CenterPoint.Y - offset);
      }
      //---------------------------------------------------------------
      public static IMapPath? GetBestPath(ITerritories territories, ITerritory startT, ITerritory endT, int moveFactor)
      {
         // <cgs> This function created by me to get shorted path. However, the Get_ShortestRandomPath() was created by OpenAi so using that method. Keeping this for contingency.
         IMapPaths paths = new MapPaths();
         if (moveFactor < 1)
            return new MapPath(endT.Name);
         IMapPaths adjPaths = new MapPaths();
         string? endPathName = endT.ToString();
         if (null == endPathName)
         {
            Logger.Log(LogEnum.LE_ERROR, "Get_BestPath(): endPathName=null");
            return null;
         }
         if (startT.ToString() == endT.ToString())
         {

            IMapPath path = new MapPath(endPathName);
            path.Territories.Add(endT);
            paths.Add(path);
            return path;
         }
         else
         {
            foreach (string adjTerritoryName in startT.Adjacents) // Setup a path map for each adjacent territory
            {
               IMapPath path = new MapPath(adjTerritoryName);
               ITerritory? adjT = territories.Find(adjTerritoryName);
               if (adjT == null)
               {
                  Logger.Log(LogEnum.LE_SHOW_MIM_BEST_PATH, "Get_BestPath(): adj=null");
                  return null;
               }
               path.Territories.Add(adjT);
               path.Metric = GetPixelDistance(adjT, endT);
               paths.Add(path);
               adjPaths.Add(path);
               if (adjTerritoryName == endT.ToString())  // If the adjacent territory is the end territory, no need to continue.  It is the best path.
               {
                  Logger.Log(LogEnum.LE_SHOW_MIM_BEST_PATH, "Get_BestPath(): Adjacent Move moving from " + startT.Name + " to " + endT.Name);
                  return path;
               }
            }
            //-------------------------------------------------------------------
            bool isEndTerritoryReached = false; // For each IMapPath object, determine the next Territory that  moves the object closer to the end goal.
            for (int i = 1; i < moveFactor; ++i)
            {
               //System.Diagnostics.Debug.WriteLine("---------------->>MF="+ i.ToString() + "<<-------------------------");
               if (true == isEndTerritoryReached) // Perform no more movement if end territory is reached by one of the paths.
                  break;
               foreach (IMapPath path in paths) // Iterate through the IMapPath objects trying to find the lowest metric score for each adjacent territory.
               {
                  //System.Diagnostics.Debug.WriteLine("==> Adding to " + path.ToString() );
                  if (path.Metric == double.MaxValue)
                     continue;
                  //------------------------------------------------------
                  double lowestMetricScore = double.MaxValue;                 // Set a threshold for the lowest metric score. Set it to a very high number because the first interation of the following loop determines what metric score to bcontinue. If a metric score is less than this number, it is set as the new threshold, i.e. trying to find the minimum metric score.
                  ITerritory? lowestTerritory = null;
                  ITerritory adj1T = path.Territories[path.Territories.Count - 1]; // A Territory is better if the distance between the center point of the territory and all other alternatives is the smallest.
                  foreach (string alternativeName in adj1T.Adjacents)
                  {
                     //System.Diagnostics.Debug.WriteLine("     ==> Trying " + alternativeName);
                     ITerritory? adj2T = territories.Find(alternativeName);
                     if (adj2T == null)
                     {
                        Logger.Log(LogEnum.LE_ERROR, "Get_BestPath(): adj2T=null for alternative=" + alternativeName);
                        return null;
                     }
                     if (adj2T.ToString() == endT.ToString()) // If the end territory is reached, no need to continue looking at alternates.
                     {
                        //System.Diagnostics.Debug.WriteLine("     ==> ==>Reached End Territory " + adj2T.ToString() + " for PATH=" + path.ToString());
                        double altDistanceMetric = GetPixelDistance(adj2T, endT); // Calculate the metric between this adjacent territory and the end territory.  If it results in a lower path metric, set it at the low water mark.
                        altDistanceMetric += path.Metric;
                        if (altDistanceMetric <= lowestMetricScore)
                        {
                           lowestMetricScore = altDistanceMetric;
                           lowestTerritory = adj2T;
                        }
                        isEndTerritoryReached = true;
                        break;
                     }
                     if (adj2T.ToString() == startT.ToString()) // Exclude alternative paths that fold back to start territory
                     {
                        //System.Diagnostics.Debug.WriteLine("     ==> ==>" + adj2T.ToString() +" is start territory");
                        continue;
                     }
                     bool isMatchFound = false; // Exclude alternative paths that fold back to other adjacent territories
                     foreach (IMapPath aPath in adjPaths)
                     {
                        if (alternativeName == aPath.ToString())
                        {
                           isMatchFound = true;
                           break;
                        }
                     }
                     if (true == isMatchFound)
                     {
                        //System.Diagnostics.Debug.WriteLine("     ==> ==> "+ adj2T.ToString()+" is already adjacent "+ path.ToString());
                        continue;
                     }
                     IEnumerable<ITerritory> results1 = from territory in path.Territories where territory.ToString() == adj2T.ToString() select territory; // Exclude alternative paths that fold back on themselves, i.e. do not choose a Territory that is already on this MapPath.
                     if (0 < results1.Count())
                     {
                        //System.Diagnostics.Debug.WriteLine("     ==> ==> " + adj2T.ToString()+" is already in "+ path.ToString());
                        continue;
                     }
                     //----------------------------------------
                     double altDistanceMetric2 = GetPixelDistance(adj2T, endT); // Calculate the metric between this adjacent territory and the end territory.  If it results in a lower path metric, set it at the low water mark.
                     altDistanceMetric2 += path.Metric;
                     if (altDistanceMetric2 <= lowestMetricScore)
                     {
                        lowestMetricScore = altDistanceMetric2;
                        lowestTerritory = adj2T;
                     }
                  }
                  //----------------------------------------------------
                  if (double.MaxValue == lowestMetricScore) // Check if a territory was added to Map Path for this instance. If not, then this map path needs to be deleted.
                  {
                     //System.Diagnostics.Debug.WriteLine("     ==> Skipping {0} at Max Value", path.ToString());
                     path.Metric = double.MaxValue;
                     continue;
                  }
                  else // Add the Territory with the lowest Metric to the path
                  {
                     if (null != lowestTerritory)
                     {
                        path.Territories.Add(lowestTerritory);
                        path.Metric = lowestMetricScore;
                     }
                     //System.Diagnostics.Debug.WriteLine("     ==> Appending to " + path.ToString());
                  }
               }
            }
         }
         //--------------------------------------------
         int i1 = 1;
         int count = paths.Count; // Determine from all paths which is the lowest metric
         if (count < 1)
         {
            Logger.Log(LogEnum.LE_ERROR, "Get_BestPath(): did not reach " + startT.Name + " from " + endT.Name);
            return null;
         }
         IMapPath? bestPath = paths[0];
         if (bestPath == null)
         {
            Logger.Log(LogEnum.LE_ERROR, "Get_BestPath(): bestpath= null & did not reach " + startT.Name + " from " + endT.Name);
            return null;
         }
         foreach (IMapPath path in paths)
         {
            //System.Diagnostics.Debug.WriteLine("{0}.) {1}", i1.ToString(), path.ToString());
            if (path.Metric < bestPath.Metric)
               bestPath = path;
            ++i1;
         }
         Logger.Log(LogEnum.LE_SHOW_MIM_BEST_PATH, "Get_BestPath(): moving from " + startT.Name + " to " + endT.Name + " using " + bestPath.ToString());
         return bestPath;
      }
      public static IMapPath? GetShortestRandomPath(ITerritories territories, ITerritory startT, ITerritory endT, int moveFactor)
      {
         string? endPathName = endT.ToString();
         if (null == endPathName)
         {
            Logger.Log(LogEnum.LE_ERROR, "Get_ShortestRandomPath(): endPathName=null");
            return null;
         }
         if (moveFactor < 1)
            return new MapPath(endPathName);
         if (startT.ToString() == endPathName)
         {
            IMapPath path = new MapPath(endPathName);
            path.Territories.Add(endT);
            return path;
         }
         if (false == TryGetRandomShortestPathTerritories(territories, startT, endT, out List<ITerritory>? shortestPathTerritories))
         {
            Logger.Log(LogEnum.LE_ERROR, "Get_ShortestRandomPath(): unable to find path from " + startT.ToString() + " to " + endT.ToString());
            return null;
         }
         if (null == shortestPathTerritories)
         {
            Logger.Log(LogEnum.LE_ERROR, "Get_ShortestRandomPath(): shortestPathTerritories=null for startT=" + startT.ToString() + " endT=" + endT.ToString());
            return null;
         }
         IMapPath bestPath = new MapPath(endPathName);
         bestPath.Metric = shortestPathTerritories.Count;
         int territoriesToAdd = Math.Min(moveFactor, shortestPathTerritories.Count);
         for (int i = 0; i < territoriesToAdd; ++i)
            bestPath.Territories.Add(shortestPathTerritories[i]);
         Logger.Log(LogEnum.LE_SHOW_MIM_BEST_PATH, "Get_ShortestRandomPath(): moving from " + startT.ToString() + " to " + endT.ToString() + " using " + bestPath.ToString());
         return bestPath;
      }
      public static bool IsMapItemWithingRange(IMapItem target, IMapItem moving)
      {
         IMapPath? bestPath = GetBestPath(Territories.theTerritories, moving.TerritoryCurrent, target.TerritoryCurrent, moving.Movement);
         if (null == bestPath)
         {
            Logger.Log(LogEnum.LE_ERROR, "IsMapItemWithingRange(): returned error for mi=" + moving.Name + " from=" + moving.TerritoryCurrent.ToString() + " to target=" + target.TerritoryCurrent.ToString());
            return false;
         }
         if (moving.Movement < bestPath.Territories.Count)
            return false;
         else
            return true;
      }
      //---------------------------------------------------------------
      public static ITerritories GetTerritoriesWithinRange(ITerritories territories, ITerritory tStart, int range)
      {
         ITerritories masterList = new Territories();
         Queue<string> tQueue = new Queue<string>();
         Queue<int> depthQueue = new Queue<int>();
         Dictionary<string, bool> visited = new Dictionary<string, bool>();
         string tName = tStart.Name + "_" + tStart.Subname;
         tQueue.Enqueue(tName);
         depthQueue.Enqueue(0);
         visited[tName] = false;
         if( true == territories.Contains(tStart))  // only add the tStart territory if it is within territories
            masterList.Add(tStart);
         while (0 < tQueue.Count)
         {
            String name = tQueue.Dequeue();
            int depth = depthQueue.Dequeue();
            if (true == visited[name])
               continue;
            if (range <= depth)
               continue;
            visited[name] = true;
            ITerritory? t = territories.Find(name);
            if (null == t) // If list is not full list of territories, might hit end of existing territories - do not return error
               t = Territories.theTerritories.Find(name);
            if (null == t) 
            {
               Logger.Log(LogEnum.LE_ERROR, "GetTerritoriesWithinRange() t=null");
               return masterList;
            }
            foreach (string adj in t.Adjacents)
            {
               ITerritory? adjT = territories.Find(adj);
               if (null == adjT) // If list is not full list of territories, might hit end of existing territories - do not return error
                  adjT = territories.Find(name);
               if (null == adjT) // If list is not full list of territories, might hit end of existing territories - do not return error
                  adjT = Territories.theTerritories.Find(name);
               if (null == adjT)
               {
                  Logger.Log(LogEnum.LE_ERROR, "GetTerritoriesWithinRange() adjT=null");
                  return masterList;
               }
               tQueue.Enqueue(adj);
               depthQueue.Enqueue(depth + 1);
               if (false == masterList.Contains(adjT))
               {
                  masterList.Add(adjT);
                  visited[adj] = false;
               }
            }
         }
         return masterList;
      }
      public static IMapItems? GetMapItemsWithinRange(IStacks stacks, ITerritories sTerritories, ITerritory targetT, int range)
      {
         ITerritories territories1 = Territory.GetTerritoriesWithinRange(sTerritories, targetT, range);
         Logger.Log(LogEnum.LE_SHOW_TERRITORIES, "Get_MapItemsWithinRange(): territories=" + territories1.ToString());
         IMapItems mapItemCandidates = new MapItems();
         foreach (ITerritory t in territories1)
         {
            IStack? stack = stacks.Find(t);
            if (null == stack)
               continue;
            Logger.Log(LogEnum.LE_SHOW_TERRITORIES, "Get_MapItemsWithinRange(): t=" + stack.Territory.ToString() + " has mapitems=" + stack.MapItems.ToString());
            foreach (IMapItem mapItem in stack.MapItems)
               mapItemCandidates.Add(mapItem);
         }
         return mapItemCandidates;
      }
      public static ITerritories GetTerritoriesOfInterest(ITerritories sTerritories, IMapItem mi1, IMapItem mi2, bool isOverlapping = true)
      {
         ITerritories returnTerritories;
         ITerritories mi1Territories = Territory.GetTerritoriesWithinRange(sTerritories, mi1.TerritoryCurrent, mi1.Movement);
         if (true == isOverlapping)
         {
            returnTerritories = Territory.GetTerritoriesWithinRange(mi1Territories, mi2.TerritoryCurrent, mi2.Movement); // return overlapping territories between 
         }
         else
         {
            returnTerritories = new Territories();
            ITerritories mi2Territories = Territory.GetTerritoriesWithinRange(sTerritories, mi2.TerritoryCurrent, mi2.Movement);
            if (mi1Territories.Count < mi2Territories.Count)
            {
               foreach (ITerritory t in mi2Territories)
               {
                  if (false == mi1Territories.Contains(t))
                     returnTerritories.Add(t); // add territory that doesnt overlap mi1 and mi2
               }
            }
            else
            {
               foreach (ITerritory t in mi1Territories)
               {
                  if (false == mi2Territories.Contains(t))
                     returnTerritories.Add(t); // add territory that doesnt overlap mi1 and mi2
               }
            }
         }
         return returnTerritories;
      }
      public static IMapItems GetMapItemsOfInterest(IStacks stacks, ITerritories sTerritories, IMapItem mi1, bool isOverlapping = true)
      {
         IMapItems overlappingMapItemCandidates = new MapItems();
         IMapItems nonoverlappingMapItemCandidates = new MapItems();
         foreach (IStack stack in stacks)
         {
            foreach (IMapItem mi2 in stack.MapItems)
            {
               if (mi1.Name != mi2.Name)
               {
                  ITerritories territories2 = GetTerritoriesOfInterest(sTerritories, mi1, mi2, true);
                  if (0 < territories2.Count)
                     overlappingMapItemCandidates.Add(mi2); // if overlapping territories that map items can move to, add mapitem
                  else
                     nonoverlappingMapItemCandidates.Add(mi2); // if overlapping territories that map items can move to, add mapitem
               }
            }
         }
         if( true == isOverlapping )
           return overlappingMapItemCandidates;
         else
            return nonoverlappingMapItemCandidates;
      }
      public static IMapItems? GetMapItemsThatCanMoveToThisMapItem(IStacks stacks, ITerritories sTerritories, IMapItem target)
      {
         ITerritories territories1 = Territory.GetTerritoriesWithinRange(sTerritories, target.TerritoryCurrent, target.Movement);
         IMapItems mapItemCandidates = new MapItems();
         foreach (ITerritory t in territories1)
         {
            IStack? stack = stacks.Find(t);
            if (null == stack)
               continue;
            foreach (IMapItem mapItem in stack.MapItems)
				{
					if( target.Name != mapItem.Name)
					   mapItemCandidates.Add(mapItem);
				}
         }
         //--------------------------------------
         IMapItems mapItems = new MapItems();
         foreach (IMapItem mi in mapItemCandidates) // Verify that mapitem can get to the target territory
			{
            ITerritories territories2 = Territory.GetTerritoriesWithinRange(territories1, mi.TerritoryCurrent, mi.Movement);
				if (true == territories2.Contains(target.TerritoryCurrent))
					mapItems.Add(mi);
         }
         return mapItems;
      }
      public static IMapItems? GetMapItemsThatThisMapItemCanMoveTo(IStacks stacks, ITerritories sTerritories, IMapItem target)
      {
         ITerritories territories1 = Territory.GetTerritoriesWithinRange(sTerritories, target.TerritoryCurrent, target.Movement);
         IMapItems mapItemCandidates = new MapItems();
         foreach (ITerritory t in territories1)
         {
            IStack? stack = stacks.Find(t);
            if (null == stack)
               continue;
            foreach (IMapItem mapItem in stack.MapItems)
            {
               if (target.Name != mapItem.Name)
                  mapItemCandidates.Add(mapItem);
            }
         }
         return mapItemCandidates;
      }
      //---------------------------------------------------------------
      public static int GetStackingCountUncontrolled(IGameInstance gi, ITerritory t, IMapItemMoves pendingMoves)
		{
         int count = 0;
         IStack? stack = gi.Stacks.Find(t);
			if (null != stack)
			{
            foreach (IMapItem mi in stack.MapItems)
            {
               if ((false == mi.IsAlienKnown) && (false == mi.IsControlled)) //
                  ++count;
            }
         }
         foreach (IMapItemMove mim in pendingMoves)
         {
            ITerritory? newT = mim.NewTerritory;
            if (null != newT)
            {
               if ((false == mim.MapItem.IsAlienKnown) && (false == mim.MapItem.IsControlled) && (t.ToString() == newT.ToString()))
                  ++count;
            }
         }
         return count;
		}
      public static int GetStackingCountAlien(IGameInstance gi, ITerritory t, IMapItemMoves pendingMoves)
      {
         int count = 0;
         IStack? stack = gi.Stacks.Find(t);
         if (null != stack)
         {
            foreach (IMapItem mi in stack.MapItems)
            {
               if ((true == mi.IsAlienKnown) && (false == mi.IsTiedUp)) // tied up people do not count against stacking limits
                  ++count;
            }
         }
         foreach (IMapItemMove mim in pendingMoves) 
         {
            ITerritory? newT = mim.NewTerritory;
				if (null != newT)
				{
					if ((true == mim.MapItem.IsAlienKnown) && (t.ToString() == newT.ToString()))
						++count;
				}
         }
         return count;
      }
      public static int GetStackingCountControlled(IGameInstance gi, ITerritory t, IMapItemMoves pendingMoves)
      {
         int count = 0;
         IStack? stack = gi.Stacks.Find(t);
         if (null != stack)
         {
            foreach (IMapItem mi in stack.MapItems)
            {
               if ((true == mi.IsControlled) && (false == mi.IsTiedUp)) // tied up people do not count against stacking limits - very rare to have tied up Town Player counter if at all
                  ++count;
            }
         }
         foreach (IMapItemMove mim in pendingMoves)
         {
            ITerritory? newT = mim.NewTerritory;
				if (null != newT)
				{
					if ((true == mim.MapItem.IsControlled) && (t.ToString() == newT.ToString()))
						++count;
				}
         }
         return count;
      }
		public static IMapPoint GetCenterPointControlled(IGameInstance gi)
		{
			double xTotal = 0.0;
			double yTotal = 0.0;
			int count = 0;
			foreach(IStack stack in gi.Stacks)
			{
				foreach(IMapItem mi in stack.MapItems)
				{
                if ((true == mi.IsControlled) && (false == mi.IsStunned) && (false==mi.IsKnockedout) )
                {
						count++;
						xTotal += mi.TerritoryCurrent.CenterPoint.X;
						yTotal += mi.TerritoryCurrent.CenterPoint.Y;
					 }
            }
			}
			IMapPoint centerPoint = new MapPoint(xTotal/(double)count, yTotal/(double)count);
			return centerPoint;
		}
      public static IMapPoint GetCenterPointAlien(IGameInstance gi)
      {
         double xTotal = 0.0;
         double yTotal = 0.0;
         int count = 0;
         foreach (IStack stack in gi.Stacks)
         {
            foreach (IMapItem mi in stack.MapItems)
            {
               if ((true == mi.IsAlien()) && (false == mi.IsStunned) && (false == mi.IsKnockedout) && (false == mi.IsTiedUp) && (false == mi.IsSurrendered))
               {
                  count++;
                  xTotal += mi.TerritoryCurrent.CenterPoint.X;
                  yTotal += mi.TerritoryCurrent.CenterPoint.Y;
               }
            }
         }
         IMapPoint centerPoint = new MapPoint(xTotal / (double)count, yTotal / (double)count);
         return centerPoint;
      }
      //---------------------------------------------------------------
      private static double DistanceToSegment(Point p, Point a, Point b, out Point closestPoint)
		{
			double dx = b.X - a.X;
			double dy = b.Y - a.Y;
			if (dx == 0 && dy == 0) // a and b are the same point
				return Math.Sqrt(Math.Pow(p.X - a.X, 2) + Math.Pow(p.Y - a.Y, 2));
			double t = ((p.X - a.X) * dx + (p.Y - a.Y) * dy) / (dx * dx + dy * dy); // Project point p onto the line segment [a, b], clamping to the segment
			t = Math.Max(0, Math.Min(1, t));
			Point closest = new Point(a.X + t * dx, a.Y + t * dy); // Find the closest point on the segment                                 
			return Math.Sqrt(Math.Pow(p.X - closest.X, 2) + Math.Pow(p.Y - closest.Y, 2)); // Return the distance from p to the closest point
		}
      private static IMapPath? GetBestPathRandom(IGameInstance gi, ITerritory startT, ITerritory endT)
      {
         return GetShortestRandomPath(Territories.theTerritories, startT, endT, MAX_PATH_COUNT);
      }
      private static bool TryGetRandomShortestPathTerritories(ITerritories territories, ITerritory startT, ITerritory endT, out List<ITerritory>? shortestPathTerritories)
      {
         shortestPathTerritories = null;
         string? startName = startT.ToString();
         string? endName = endT.ToString();
         if ((null == startName) || (null == endName))
         {
            Logger.Log(LogEnum.LE_ERROR, "TryGet_RandomShortestPathTerritories(): invalid start or end territory name");
            return false;
         }
         Dictionary<string, int> shortestDistanceByTerritory = new Dictionary<string, int>();
         Dictionary<string, List<ITerritory>> predecessorsByTerritory = new Dictionary<string, List<ITerritory>>();
         Queue<ITerritory> frontier = new Queue<ITerritory>();
         frontier.Enqueue(startT);
         shortestDistanceByTerritory[startName] = 0;
         int? shortestDistanceToEnd = null;
         while (0 < frontier.Count)
         {
            ITerritory currentTerritory = frontier.Dequeue();
            string? currentName = currentTerritory.ToString();
            if (null == currentName)
            {
               Logger.Log(LogEnum.LE_ERROR, "TryGet_RandomShortestPathTerritories(): currentName=null");
               return false;
            }
            int currentDistance = shortestDistanceByTerritory[currentName];
            if ((true == shortestDistanceToEnd.HasValue) && (currentDistance >= shortestDistanceToEnd.Value))
               continue;
            foreach (string adjacentName in currentTerritory.Adjacents)
            {
               ITerritory? adjacentTerritory = territories.Find(adjacentName);
               if (null == adjacentTerritory)
               {
                  Logger.Log(LogEnum.LE_ERROR, "TryGet_RandomShortestPathTerritories(): adjacentTerritory=null for adjacentName=" + adjacentName);
                  return false;
               }
               string? adjacentPathName = adjacentTerritory.ToString();
               if (null == adjacentPathName)
               {
                  Logger.Log(LogEnum.LE_ERROR, "TryGet_RandomShortestPathTerritories(): adjacentPathName=null for adjacentName=" + adjacentName);
                  return false;
               }
               int nextDistance = currentDistance + 1;
               if (false == shortestDistanceByTerritory.TryGetValue(adjacentPathName, out int bestKnownDistance))
               {
                  shortestDistanceByTerritory[adjacentPathName] = nextDistance;
                  predecessorsByTerritory[adjacentPathName] = new List<ITerritory>() { currentTerritory };
                  if ((false == shortestDistanceToEnd.HasValue) || (nextDistance < shortestDistanceToEnd.Value))
                     frontier.Enqueue(adjacentTerritory);
               }
               else if (nextDistance == bestKnownDistance)
               {
                  List<ITerritory> predecessors = predecessorsByTerritory[adjacentPathName];
                  bool isDuplicatePredecessor = false;
                  foreach (ITerritory predecessor in predecessors)
                  {
                     if (predecessor.ToString() == currentName)
                     {
                        isDuplicatePredecessor = true;
                        break;
                     }
                  }
                  if (false == isDuplicatePredecessor)
                     predecessors.Add(currentTerritory);
               }
               if (adjacentPathName == endName)
               {
                  if ((false == shortestDistanceToEnd.HasValue) || (nextDistance < shortestDistanceToEnd.Value))
                     shortestDistanceToEnd = nextDistance;
               }
            }
         }
         if (false == shortestDistanceByTerritory.ContainsKey(endName))
         {
            Logger.Log(LogEnum.LE_ERROR, "TryGet_RandomShortestPathTerritories(): no path found from " + startName + " to " + endName);
            return false;
         }
         List<ITerritory> reversePath = new List<ITerritory>();
         ITerritory currentPathTerritory = endT;
         string currentPathName = endName;
         while (currentPathName != startName)
         {
            reversePath.Add(currentPathTerritory);
            if (false == predecessorsByTerritory.TryGetValue(currentPathName, out List<ITerritory>? predecessors))
            {
               Logger.Log(LogEnum.LE_ERROR, "TryGet_RandomShortestPathTerritories(): predecessors missing for territory=" + currentPathName);
               return false;
            }
            if (0 == predecessors.Count)
            {
               Logger.Log(LogEnum.LE_ERROR, "TryGet_RandomShortestPathTerritories(): predecessors.Count=0 for territory=" + currentPathName);
               return false;
            }
            int randomPredecessorIndex = Utilities.RandomGenerator.Next(predecessors.Count);
            currentPathTerritory = predecessors[randomPredecessorIndex];
            string? predecessorName = currentPathTerritory.ToString();
            if (null == predecessorName)
            {
               Logger.Log(LogEnum.LE_ERROR, "TryGet_RandomShortestPathTerritories(): predecessorName=null");
               return false;
            }
            currentPathName = predecessorName;
         }
         reversePath.Reverse();
         shortestPathTerritories = reversePath;
         return true;
      }
      private static double GetPixelDistance(ITerritory startT, ITerritory endT)
      {
         Point startPoint = new Point(startT.CenterPoint.X, startT.CenterPoint.Y);
         Point endPoint = new Point(endT.CenterPoint.X, endT.CenterPoint.Y);
         double xDelta = endPoint.X - startPoint.X;
         double yDelta = endPoint.Y - startPoint.Y;
         double distance = Math.Sqrt(xDelta * xDelta + yDelta * yDelta);
         return distance;
      }
      //---------------------------------------------------------------
      public Territory()
		{
		}
		public override string ToString()
		{
			string returnVal = Name + "_" + Subname;
			return returnVal;
		}
	}
	//---------------------------------------------------------------
	[Serializable]
	public class Territories : IEnumerable, ITerritories
	{
		[NonSerialized] public const string FILENAME = "Territories.xml";
		[NonSerialized] static public ITerritories theTerritories = new Territories();
		private readonly ArrayList myList;
		public Territories() { myList = new ArrayList(); }
		public void Add(ITerritory t) 
		{ 
			myList.Add(t);
		}
		public void Insert(int index, ITerritory t) { myList.Insert(index, t); }
		public int Count { get { return myList.Count; } }
		public void Clear() { myList.Clear(); }
		public bool Contains(ITerritory t)
		{
			string? tName = t.ToString();
			if( null == tName )
			{
				Logger.Log(LogEnum.LE_ERROR, "Territory.Contains() tName=null");
				return false;
			}
			tName = Utilities.RemoveSpaces(tName);
			//--------------------------------------
         foreach (object o in myList)
			{
				ITerritory t1 = (ITerritory)o;
				string? t1Name = t1.ToString();
            if (null == t1Name)
            {
               Logger.Log(LogEnum.LE_ERROR, "Territory.Contains() t1Name=null");
               return false;
            }
            t1Name = Utilities.RemoveSpaces(t1Name);
            if (tName == t1Name) // match on name
					return true;
			}
			return false;
		}
		public IEnumerator GetEnumerator() { return myList.GetEnumerator(); }
		public int IndexOf(ITerritory t) { return myList.IndexOf(t); }
		public void Remove(ITerritory t) { myList.Remove(t); }
		public ITerritory? Find(string tName)
		{
			foreach (object o in myList)
			{
				ITerritory t = (ITerritory)o;
            if ((true == t.Name.Contains(tName)) || (t.ToString() == tName))
               return t;
			}
			return null;
		}
		public ITerritory? Find(string tName, string tSubName)
		{
			foreach (object o in myList)
			{
				ITerritory t = (ITerritory)o;
				string territoryName = Utilities.RemoveSpaces(t.Name);
				string territoryType = Utilities.RemoveSpaces(t.Subname);
				if ((tName == territoryName) && (tSubName == territoryType))
					return t;
			}
			return null;
		}
		public ITerritory? RemoveAt(int index)
		{
			ITerritory? t = myList[index] as ITerritory;
			myList.RemoveAt(index);
			return t;
		}
		public ITerritory? Remove(string tName)
		{
			foreach (object o in myList)
			{
				ITerritory t = (ITerritory)o;
            if ((true == t.Name.Contains(tName)) || (t.ToString() == tName) )
            {
					myList.Remove(t);
					return t;
				}
			}
			return null;
		}
		public ITerritory? this[int index]
		{
			get
			{
				ITerritory? t = myList[index] as ITerritory;
				return t;
			}
			set { myList[index] = value; }
		}
		public override string ToString()
		{
			StringBuilder sb = new StringBuilder();
			sb.Append("[ ");
			foreach (object o in myList)
			{
				ITerritory t = (ITerritory)o;
				sb.Append(t.ToString());
				sb.Append(" ");
			}
			sb.Append("]");
			return sb.ToString();
		}
	}
	//---------------------------------------------------------------
	public static class TerritoryExtensions
	{
		public static ITerritory? Find(this IList<ITerritory> territories, string name)
		{
			try
			{
				IEnumerable<ITerritory> results = from territory in territories where territory.Name == name select territory;
				if (0 < results.Count())
					return results.First();
			}
			catch (Exception e)
			{
				Logger.Log(LogEnum.LE_ERROR, "MyTerritoryExtensions.Find(list, name): name=" + name + " e.Message=\n" + e.ToString()); ;
			}
			return null;
		}
		public static ITerritory? Find(this IList<ITerritory> territories, string name, string subname)
		{
			try
			{
				IEnumerable<ITerritory> results = from territory in territories where (territory.Name == name && territory.Subname == subname) select territory;
				if (0 < results.Count())
					return results.First();
			}
			catch (Exception e)
			{
				Logger.Log(LogEnum.LE_ERROR, "MyTerritoryExtensions.Find(list, name): name=" + name + " subname=" + subname + " e.Message=\n" + e.ToString()); ;
			}
			return null;
		}
	}
}
