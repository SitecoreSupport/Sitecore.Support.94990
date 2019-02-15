namespace Sitecore.Support.Shell.Framework.Commands.Serialization
{
  using Sitecore.Data.Items;
  using Sitecore.Diagnostics;
  using Sitecore.Shell.Framework.Commands.Serialization;
  using System;

  [Serializable]
  public class CustomDumpTreeCommand : DumpTreeCommand
  {
    protected override void Dump(Item item)
    {
      Assert.ArgumentNotNull(item, "item");
      CustomManager.DumpTree(item);
    }
  }
}