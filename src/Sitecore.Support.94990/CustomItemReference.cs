namespace Sitecore.Support.Shell.Framework.Commands.Serialization
{
  using Sitecore.Collections;
  using Sitecore.Configuration;
  using Sitecore.Data;
  using Sitecore.Data.Items;
  using Sitecore.Data.Serialization;
  using Sitecore.Data.Serialization.ObjectModel;
  using Sitecore.Diagnostics;
  using Sitecore.Support;
  using System;
  using System.Collections;
  using System.IO;
  using System.Text;

  public class CustomItemReference
  {
    private readonly string _database;

    private Item _item;

    private readonly string _path;

    public string Database => _database;

    public string Path => _path;

    public CustomItemReference(Item item)
    {
      Assert.ArgumentNotNull(item, "item");
      _database = item.Database.Name;
      _path = GetPath(item);
      _item = item;
    }

    public CustomItemReference(string database, string path)
    {
      Assert.ArgumentNotNull(database, "database");
      Assert.ArgumentNotNull(path, "path");
      _database = database;
      _path = path;
    }

    public Item GetItem()
    {
      if (_item == null)
      {
        _item = GetItemInDatabase(Factory.GetDatabase(_database));
      }
      return _item;
    }

    public Item GetItemInDatabase(Database database)
    {
      Assert.ArgumentNotNull(database, "database");
      Item item = null;
      StringBuilder stringBuilder = new StringBuilder(_database);
      string[] array = _path.Split(new char[1]
      {
            '/'
      }, StringSplitOptions.RemoveEmptyEntries);
      foreach (string text in array)
      {
        stringBuilder.Append("/").Append(text);
        ID iD = null;
        if (text.Length > 32)
        {
          string filePath = PathUtils.GetFilePath(stringBuilder.ToString());
          if (File.Exists(filePath))
          {
            using (TextReader reader = new StreamReader(File.Open(filePath, FileMode.Open, FileAccess.Read, FileShare.Read)))
            {
              SyncItem syncItem = SyncItem.ReadItem(new Tokenizer(reader));
              if (syncItem != null && !string.IsNullOrEmpty(syncItem.ID))
              {
                iD = ID.Parse(syncItem.ID);
              }
            }
          }
          else
          {
            string text2 = text.Substring(text.Length - 32, 32);
            if (!string.IsNullOrEmpty(text2) && ShortID.IsShortID(text2))
            {
              iD = ShortID.DecodeID(text2);
            }
          }
        }
        item = ((!ID.IsNullOrEmpty(iD)) ? ((item != null) ? item.Children[iD] : database.GetItem(iD)) : ((item != null) ? item.Children[text] : database.GetRootItem()));
        if (item == null)
        {
          return null;
        }
      }
      return item;
    }

    public Item GetItemStrict()
    {
      Item item = GetItem();
      if (item != null && new CustomItemReference(item).ToString() == ToString())
      {
        return item;
      }
      return null;
    }

    private string GetPath(Item item)
    {
      Assert.ArgumentNotNull(item, "item");
      if (item.Parent == null)
      {
        return "/" + item.Name;
      }
      string text = GetPath(item.Parent) + "/" + CustomPathUtils.RestoreIllegalCharsInPath(item.Name);
      foreach (Item child in item.Parent.GetChildren(ChildListOptions.SkipSorting))
      {
        #region Modified Code
        //original child.Name == item.Name - this returns true if same name but different casing. Need to compare on same casing level.
        if (child.Name.ToLower() == item.Name.ToLower() && child.ID != item.ID)
        #endregion
        {
          return text + "_" + item.ID.ToShortID();
        }
      }
      return text;
    }

    public static CustomItemReference Parse(string path)
    {
      Assert.ArgumentNotNull(path, "path");
      if (path.StartsWith("/", StringComparison.InvariantCulture))
      {
        path = path.Substring(1);
      }
      string[] array = path.Split(new char[1]
      {
            '/'
      }, 2);
      if (array.Length == 2)
      {
        return new CustomItemReference(array[0], "/" + array[1]);
      }
      return null;
    }

    public override string ToString()
    {
      return _database + _path;
    }
  }
}