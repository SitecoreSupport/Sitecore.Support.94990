namespace Sitecore.Support.Shell.Framework.Commands.Serialization
{
  using Sitecore.Data.Items;
  using Sitecore.Diagnostics;
  using Sitecore.Shell.Applications.Dialogs.ProgressBoxes;
  using Sitecore.Shell.Framework.Commands;
  using Sitecore.Shell.Framework.Commands.Serialization;
  using System;

  [Serializable]
  public class CustomDumpItemCommand : DumpItemCommand
  {
    protected new void AuditDump(Item item)
    {
      Log.Audit(this, "Serializing item {0}", item.Paths.FullPath);
    }

    private void Dump(params object[] parameters)
    {
      Item item = parameters[0] as Item;
      if (item != null)
      {
        CustomManager.DumpItem(item);
      }
    }

    public override void Execute(CommandContext context)
    {
      Assert.ArgumentNotNull(context, "context");
      Item item = context.Items[0];
      AuditDump(item);
      ProgressBox.Execute("ItemSync", GetName(), "business/16x16/data_disk.png", Dump, item);
    }
  }
}
