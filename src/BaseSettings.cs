using System.ComponentModel;
using Spectre.Console.Cli;

namespace Databricks.Sql.Cli
{
   public class BaseSettings : CommandSettings
   {
      [Description("output format, can be TABLE (default) or JSON")]
      [CommandOption("-f|--format <format>")]
      [DefaultValue("TABLE")]
      public string Format { get; set; }
   }
}
