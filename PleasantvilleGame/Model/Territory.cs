using System.Collections;
using System.Text;
using System.Windows.Media;
using Point = System.Windows.Point;

namespace PleasantvilleGame
{
	[Serializable]
	public class Territory : ITerritory
	{
		public string Name { get; set; } = "Offboard";
		public string CanvasName { get; set; } = "Main";
		public string Subname { get; set; } = "ERROR";
		public IMapPoint CenterPoint { get; set; } = new MapPoint();
		public List<IMapPoint> Points { get; set; } = new List<IMapPoint>();
		public List<string> Adjacents { get; set; } = new List<string>();
		public List<string> PavedRoads { get; set; } = new List<string>();
		public List<string> Observations { get; set; } = new List<string>();
		public bool IsBuilding()
		{
			return false;
		}
		//---------------------------------------------------------------
		public static IMapPath? GetBestPath(ITerritories territories, ITerritory startT, ITerritory endT, int moveFactor)
		{
			IMapPaths paths = new MapPaths();
			if (moveFactor < 1)
				return new MapPath(endT.Name);
			IMapPaths adjPaths = new MapPaths();
         string? endPathName = endT.ToString();
			if(null == endPathName)
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
					path.Metric = GetDistance(adjT, endT);
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
								double altDistanceMetric = GetDistance(adj2T, endT); // Calculate the metric between this adjacent territory and the end territory.  If it results in a lower path metric, set it at the low water mark.
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
							double altDistanceMetric2 = GetDistance(adj2T, endT); // Calculate the metric between this adjacent territory and the end territory.  If it results in a lower path metric, set it at the low water mark.
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
		static private double GetDistance(ITerritory startT, ITerritory endT)
		{
			Point startPoint = new Point(startT.CenterPoint.X, startT.CenterPoint.Y);
			Point endPoint = new Point(endT.CenterPoint.X, endT.CenterPoint.Y);
			double xDelta = endPoint.X - startPoint.X;
			double yDelta = endPoint.Y - startPoint.Y;
			double distance = Math.Sqrt(xDelta * xDelta + yDelta * yDelta);
			return distance;
		}
		public static IMapPoint GetRandomPoint(ITerritory t, double offset) // return the top left location of a MapItem, not the center point
		{
			if (0 == t.Points.Count)
			{
				Logger.Log(LogEnum.LE_ERROR, "GetRandomPoint(): t.Points.Count=0 for t.Name=" + t.Name);
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
			Logger.Log(LogEnum.LE_ERROR, "GetRandomPoint(): Cannot find a random point in t.Name=" + t.Name + " rect=" + rect.ToString());
			return new MapPoint(t.CenterPoint.X - offset, t.CenterPoint.Y - offset);
		}
		public static IMapPoint GetClosestPointInTerritory(ITerritory t, Point pCenter, double offset)
		{
			if (0 == t.Points.Count)
			{
				Logger.Log(LogEnum.LE_ERROR, "GetRandomPoint(): t.Points.Count=0 for t.Name=" + t.Name);
				return t.CenterPoint;
			}
			//---------------------------------
			int count = 20;
			while (0 < --count) // offset is the difference between MapItem location on screen and the center of the  MapItem.
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
			Logger.Log(LogEnum.LE_ERROR, "GetRandomPoint(): Cannot find a random point in t.Name=" + t.Name);
			return new MapPoint(t.CenterPoint.X - offset, t.CenterPoint.Y - offset);

		}
		public static bool IsPointInPolygon(ITerritory t, Point point)
		{
			if (0 == t.Points.Count)
			{
				//Logger.Log(LogEnum.LE_ERROR, "GetClosestPoint(): t.Points.Count=0 for t.Name=" + t.Name);
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
		public static IMapPoint GetClosestPoint(ITerritory t, Point p, double offset)
		{
			if (0 == t.Points.Count)
			{
				Logger.Log(LogEnum.LE_ERROR, "GetClosestPoint(): t.Points.Count=0 for t.Name=" + t.Name);
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
		public void Add(ITerritory t) { myList.Add(t); }
		public void Insert(int index, ITerritory t) { myList.Insert(index, t); }
		public int Count { get { return myList.Count; } }
		public void Clear() { myList.Clear(); }
		public bool Contains(ITerritory t)
		{
			foreach (object o in myList)
			{
				ITerritory t1 = (ITerritory)o;
				if (Utilities.RemoveSpaces(t.Name) == Utilities.RemoveSpaces(t1.Name)) // match on name
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
				sb.Append(t.Name);
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
