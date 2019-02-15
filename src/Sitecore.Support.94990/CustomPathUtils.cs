namespace Sitecore.Support
{
  using Sitecore.Data.Serialization;
  using Sitecore.Diagnostics;
  using System;
  using System.Collections.Generic;
  using System.Linq;
  using System.Reflection;
  using System.Text;

  public static class CustomPathUtils
  {
    public static string HandleIllegalSymbols(string str, Func<KeyValuePair<char, string>, string> keySelector, Func<KeyValuePair<char, string>, string> valueSelector) =>
        ((List<KeyValuePair<char, string>>)typeof(PathUtils).GetField("IllegalSymbolsToReplace", BindingFlags.NonPublic | BindingFlags.Static).GetValue(null)).Aggregate<KeyValuePair<char, string>, StringBuilder>(new StringBuilder(str), (current, pair) => current.Replace(keySelector(pair), valueSelector(pair))).ToString();

    public static string RestoreIllegalCharsInPath(string path)
    {
      Assert.ArgumentNotNull(path, "path");
      return HandleIllegalSymbols(path, pair => pair.Value, pair => pair.Key.ToString());
    }
  }
}