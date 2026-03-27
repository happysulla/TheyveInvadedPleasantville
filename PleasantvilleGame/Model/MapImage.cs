using System;
using System.Collections;
using System.Data.Common;
using System.Diagnostics.Eventing.Reader;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using WpfAnimatedGif;

namespace PleasantvilleGame
{
   [Serializable]
   public class MapItemImage : IMapItemImage
   {
      public static string theImageDirectory = "";
      public BitmapImage myBitmapImage;
      public string Name { get; set; } = string.Empty;
      public bool IsAnimated { get; set; } = false;
      public bool IsMoving { get; set; } = false;
      public bool IsHullDown { get; set; } = false;
      private Image myImage = new Image();
      public Image ImageControl { get => myImage; set => myImage = value; }
      private ImageAnimationController? myAnimationController = null;
      public ImageAnimationController? AnimationController { get => myAnimationController; set => myAnimationController = value; }
      //--------------------------------------------
      public MapItemImage(string imageName)
      {
         string fullImagePath = theImageDirectory + Utilities.RemoveSpaces(imageName) + ".gif";
         Name = imageName;
         myBitmapImage = new BitmapImage();
         try
         {
            myBitmapImage.BeginInit();
            myBitmapImage.UriSource = new Uri(fullImagePath, UriKind.Absolute);
            myBitmapImage.EndInit();
            ImageControl = new Image { Source = myBitmapImage, Stretch = Stretch.Fill, Name = imageName };
            if (null == ImageControl)
            {
               Logger.Log(LogEnum.LE_ERROR, "MapItemImage(): 0 imageName=" + imageName);
            }
            else
            {
               ImageBehavior.SetAnimatedSource(ImageControl, myBitmapImage);
               ImageBehavior.SetAutoStart(ImageControl, true);
               ImageBehavior.SetRepeatBehavior(ImageControl, new RepeatBehavior(1));
               ImageBehavior.AddAnimationCompletedHandler(ImageControl, ImageAnimationLoaded);
               return;
            }
         }
         catch (DirectoryNotFoundException dirException)
         {
            Logger.Log(LogEnum.LE_ERROR, "MapItemImage(): 1 imageName=" + fullImagePath + "\n" + dirException.ToString());
         }
         catch (FileNotFoundException fileException)
         {
            Logger.Log(LogEnum.LE_ERROR, "MapItemImage(): 2  imageName=" + fullImagePath + "\n" + fileException.ToString());
         }
         catch (IOException ioException)
         {
            Logger.Log(LogEnum.LE_ERROR, "MapItemImage(): 3 imageName=" + fullImagePath + "\n" + ioException.ToString());
         }
         catch (Exception e)
         {
            Logger.Log(LogEnum.LE_ERROR, "MapItemImage(): 4 imageName=" + fullImagePath + "\n" + e.ToString());
         }
      }
      public MapItemImage(MapItemImage mii)
      {
         Name = mii.Name;
         myBitmapImage = mii.myBitmapImage;
         ImageControl = mii.ImageControl;
         AnimationController = mii.AnimationController;
      }
      private void ImageAnimationLoaded(object sender, RoutedEventArgs e)
      {
         try
         {
            ImageControl = (Image)sender;
            AnimationController = ImageBehavior.GetAnimationController(ImageControl);
            if (null == AnimationController)
               Logger.Log(LogEnum.LE_ERROR, "ImageAnimationCompleted(): controller=null");
            else
               IsAnimated = true;
         }
         catch (DirectoryNotFoundException dirException)
         {
            if (null == ImageControl)
               Logger.Log(LogEnum.LE_ERROR, "ImageAnimationLoaded(): 1 imagecontrol=null \n" + dirException.ToString());
            else
               Logger.Log(LogEnum.LE_ERROR, "ImageAnimationLoaded(): 1 imageName=" + ImageControl.Name + "\n" + dirException.ToString());
            return;
         }
         catch (FileNotFoundException fileException)
         {
            if (null == ImageControl)
               Logger.Log(LogEnum.LE_ERROR, "ImageAnimationLoaded(): 2 imagecontrol=null \n" + fileException.ToString());
            else
               Logger.Log(LogEnum.LE_ERROR, "ImageAnimationLoaded(): 2 imageName=" + ImageControl.Name + "\n" + fileException.ToString());
            return;
         }
         catch (IOException ioException)
         {
            if (null == ImageControl)
               Logger.Log(LogEnum.LE_ERROR, "ImageAnimationLoaded(): 3 imagecontrol=null \n" + ioException.ToString());
            else
               Logger.Log(LogEnum.LE_ERROR, "ImageAnimationLoaded(): 3 imageName=" + ImageControl.Name + "\n" + ioException.ToString());
            return;
         }
         catch (Exception ex)
         {
            if (null == ImageControl)
               Logger.Log(LogEnum.LE_ERROR, "ImageAnimationLoaded(): 4 imagecontrol=null \n" + ex.ToString());
            else
               Logger.Log(LogEnum.LE_ERROR, "ImageAnimationLoaded(): 4 imageName=" + ImageControl.Name + "\n" + ex.ToString());
            return;
         }
      }
   }
   //--------------------------------------------------------------------------
   [Serializable]
   public class MapItemImages : IEnumerable, IMapItemImages
   {
      private readonly ArrayList myList;
      public MapItemImages() { myList = new ArrayList(); }
      public void Add(IMapItemImage mii) { myList.Add(mii); }
      public void Insert(int index, IMapItemImage mii) { myList.Insert(index, mii); }
      public int Count { get { return myList.Count; } }
      public void Clear() { myList.Clear(); }
      public bool Contains(IMapItemImage mii) { return myList.Contains(mii); }
      public IEnumerator GetEnumerator() { return myList.GetEnumerator(); }
      public int IndexOf(IMapItemImage mii) { return myList.IndexOf(mii); }
      public void Remove(IMapItemImage mii) { myList.Remove(mii); }
      public IMapItemImage? RemoveAt(int index)
      {
         IMapItemImage? mii = myList[index] as IMapItemImage;
         if (null == mii) return null;
         myList.RemoveAt(index);
         return mii;
      }
      public IMapItemImage? this[int index]
      {
         get
         {
            IMapItemImage? image = myList[index] as IMapItemImage;
            return image;
         }
         set { myList[index] = value; }
      }
      public IMapItemImage? Find(string pathToMatch)
      {
         foreach (object o in myList)
         {
            IMapItemImage mii = (IMapItemImage)o;
            if (mii.Name == pathToMatch)
               return mii;
         }
         return null;
      }
      public BitmapImage? GetBitmapImage(string imageName)
      {
         foreach (object o in myList)
         {
            MapItemImage? mii = o as MapItemImage;
            if (null == mii)
            {
               Logger.Log(LogEnum.LE_ERROR, "GetBitmapImage(): mii is not a MapItemImage");
               return null;
            }
            if (mii.Name == imageName)
            {
               if (null == mii.myBitmapImage)
               {
                  Logger.Log(LogEnum.LE_ERROR, "GetBitmapImage(): mii.myBitmapImage=null");
                  return null;
               }
               return mii.myBitmapImage;
            }
         }
         MapItemImage miiToAdd = new MapItemImage(imageName);
         myList.Add(miiToAdd);
         return miiToAdd.myBitmapImage;
      }
   }
}
