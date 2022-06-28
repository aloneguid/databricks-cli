using System.ComponentModel;
using Spectre.Console.Cli;

namespace Databricks.Cli.Settings
{
   public class ListSettings : BaseSettings
   {
      [CommandOption("-t|--tag <tag-name>")]
      [Description("filter by tag (case insensitive); specify mulitiple tags by comma-separating them.")]
      public string? Tag { get; set; }

      [CommandOption("-n|--name <name-or-substring>")]
      [Description("filter by name or name substring (case insensitive)")]
      public string? Name { get; set; }
   }
}
