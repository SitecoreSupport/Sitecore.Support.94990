namespace Sitecore.Support.Shell.Framework.Commands.Serialization
{
  using Sitecore;
  using Sitecore.Collections;
  using Sitecore.Data.Items;
  using Sitecore.Data.Serialization;
  using Sitecore.Diagnostics;
  using Sitecore.IO;
  using Sitecore.Jobs;
  using Sitecore.StringExtensions;
  using Sitecore.Support;
  using System;
  using System.IO;
  using System.Reflection;

  public static class CustomManager
  {
    public static void CleanupPath(string path, bool recursive)
    {
      Assert.ArgumentNotNullOrEmpty(path, "path");
      #region Added Code
      Manager.Initialize();
      #endregion
      MethodInfo method = typeof(Manager).GetMethod("RemoveDirectory", BindingFlags.Static | BindingFlags.NonPublic);
      MethodInfo method2 = typeof(Manager).GetMethod("RaiseEvent", BindingFlags.Static | BindingFlags.NonPublic);
      CustomItemReference customItemReference;
      try
      {
        customItemReference = CustomItemReference.Parse(PathUtils.MakeItemPath(path + PathUtils.Extension));
      }
      catch (Exception ex)
      {
        Log.Info("Possibly serializing sitecore root (" + ex.Message + ")", (object)"Sitecore.Data.Serialization.Manager, ItemSync");
        return;
      }
      Item item = customItemReference.GetItem();
      LogLocalized("Cleaning up {0}.", customItemReference);
      string shortPath = PathUtils.GetShortPath(PathUtils.Root + customItemReference.ToString().Replace('/', Path.DirectorySeparatorChar));
      if (item != null)
      {
        if (item.Children.Count != 0)
        {
          if (Directory.Exists(path))
          {
            string[] files = Directory.GetFiles(path);
            foreach (string text in files)
            {
              if (!text.EndsWith("\\link", StringComparison.InvariantCulture) && CustomItemReference.Parse(PathUtils.MakeItemPath(text)).GetItemStrict() == null)
              {
                File.Delete(text);
                method2.Invoke(null, new object[2]
                {
                                FileOperationType.Deleted,
                                text
                });
                string text2 = PathUtils.StripPath(text);
                if (Directory.Exists(text2))
                {
                  method.Invoke(null, new object[1]
                  {
                                    text2
                  });
                }
                string shortPath2 = PathUtils.GetShortPath(text2);
                if (Directory.Exists(shortPath2))
                {
                  method.Invoke(null, new object[1]
                  {
                                    shortPath2
                  });
                }
              }
            }
          }
          if (Directory.Exists(shortPath))
          {
            string[] files2 = Directory.GetFiles(shortPath, "*" + (PathUtils.Extension.StartsWith(".", StringComparison.InvariantCulture) ? string.Empty : ".") + PathUtils.Extension);
            foreach (string text3 in files2)
            {
              CustomItemReference customItemReference2 = null;
              try
              {
                customItemReference2 = CustomItemReference.Parse(PathUtils.MakeItemPath(text3));
              }
              catch
              {
                continue;
              }
              if (customItemReference2 != null && customItemReference2.GetItemStrict() == null)
              {
                File.Delete(text3);
                method2.Invoke(null, new object[2]
                {
                                FileOperationType.Deleted,
                                text3
                });
                string shortPath3 = PathUtils.GetShortPath(PathUtils.Root + Path.Combine(customItemReference.ToString(), PathUtils.StripPath(new FileInfo(text3).Name)).Replace('/', Path.DirectorySeparatorChar));
                if (Directory.Exists(shortPath3))
                {
                  method.Invoke(null, new object[1]
                  {
                                    shortPath3
                  });
                }
                string text4 = PathUtils.StripPath(text3);
                if (Directory.Exists(text4))
                {
                  method.Invoke(null, new object[1]
                  {
                                    text4
                  });
                }
                string shortPath4 = PathUtils.GetShortPath(text4);
                if (Directory.Exists(shortPath4))
                {
                  method.Invoke(null, new object[1]
                  {
                                    shortPath4
                  });
                }
              }
            }
            if (recursive)
            {
              string[] directories = PathUtils.GetDirectories(shortPath);
              foreach (string path2 in directories)
              {
                if ((File.GetAttributes(path2) & FileAttributes.Hidden) != FileAttributes.Hidden)
                {
                  CleanupPath(path2, true);
                }
              }
            }
            string[] files3 = Directory.GetFiles(shortPath);
            if (files3 != null && files3.Length == 1 && files3[0].ToLowerInvariant().EndsWith("\\link", StringComparison.InvariantCulture))
            {
              method.Invoke(null, new object[1]
              {
                            shortPath
              });
            }
          }
          if (recursive)
          {
            string[] directories2 = PathUtils.GetDirectories(path);
            foreach (string path3 in directories2)
            {
              if (Directory.Exists(path3) && (File.GetAttributes(path3) & FileAttributes.Hidden) != FileAttributes.Hidden)
              {
                CleanupPath(path3, true);
              }
            }
          }
        }
        else
        {
          if (Directory.Exists(path))
          {
            method.Invoke(null, new object[1]
            {
                        path
            });
          }
          if (Directory.Exists(shortPath))
          {
            method.Invoke(null, new object[1]
            {
                        shortPath
            });
          }
        }
      }
      else
      {
        try
        {
          method.Invoke(null, new object[1]
          {
                    path
          });
          method.Invoke(null, new object[1]
          {
                    shortPath
          });
        }
        catch (DirectoryNotFoundException ex2)
        {
          object owner = typeof(Manager).GetNestedType("Failure").GetConstructor(new Type[2]
          {
                    typeof(string),
                    typeof(DirectoryNotFoundException)
          }).Invoke(new object[2]
          {
                    path,
                    ex2
          });
          Log.Warn("Directory " + path + " was not found during the cleaning path.", owner);
        }
      }
    }

    public static void DumpItem(Item item)
    {
      Assert.ArgumentNotNull(item, "item");
      CustomItemReference customItemReference = new CustomItemReference(item);
      LogLocalized("Serializing {0}", customItemReference);
      Manager.DumpItem(PathUtils.GetFilePath(customItemReference.ToString()), item);
    }

    public static void DumpTree(Item item)
    {
      Assert.ArgumentNotNull(item, "item");
      string directoryPath = PathUtils.GetDirectoryPath(new CustomItemReference(item).ToString());
      DumpTreeInternal(item);
      CleanupPath(directoryPath, true);
      CleanupObsoleteShortens();
    }

    public static void DumpTreeInternal(Item item)
    {
      Assert.ArgumentNotNull(item, "item");
      DumpItem(item);
      foreach (Item child in item.GetChildren(ChildListOptions.None))
      {
        DumpTreeInternal(child);
      }
    }

    private static void LogLocalized(string message, params object[] parameters)
    {
      Assert.IsNotNullOrEmpty(message, "message");
      Job job = Context.Job;
      if (job != null)
      {
        job.Status.LogInfo(message, parameters);
      }
      else
      {
        Log.Info(message.FormatWith(parameters), new object());
      }
    }

    private static void CleanupObsoleteShortens()
    {
      string[] directories = Directory.GetDirectories(PathUtils.Root);
      foreach (string text in directories)
      {
        string path = FileUtil.MakePath(text, "link");
        if (File.Exists(path))
        {
          string str = default(string);
          using (TextReader textReader = new StreamReader(File.Open(path, FileMode.Open, FileAccess.Read, FileShare.Read)))
          {
            str = textReader.ReadToEnd();
          }
          ItemReference itemReference = ItemReference.Parse(CustomPathUtils.RestoreIllegalCharsInPath(PathUtils.MakeItemPath(PathUtils.Root + str)));
          if (itemReference != null && itemReference.GetItem() == null)
          {
            FileUtil.DeleteDirectory(text, true);
          }
        }
      }
    }
  }
}
