using System;
using System.ComponentModel;
using Spectre.Console;
using Spectre.Console.Cli;
using Stowage;
using Stowage.Impl.Databricks;

namespace Databricks.Cli
{
   public class BaseSettings : CommandSettings
   {
      [Description("output format, can be TABLE (default) or JSON")]
      [CommandOption("-f|--format <format>")]
      [DefaultValue("TABLE")]
      public string Format { get; set; }

      [Description("when using databricks cli profiles, and profile is non-default, allows to specify the profile name")]
      [CommandOption("-p|--profile <profile-name>")]
      public string CliProfile { get; set; }

      private const string HostEnvVar = "DATABRICKS_HOST";
      private const string TokenEnvVar = "DATABRICKS_TOKEN";
      private IDatabricksClient _dbc = null;

      public IDatabricksClient Dbc
      {
         get
         {
            if(_dbc == null)
            {
               try
               {
                  _dbc = (IDatabricksClient)Files.Of.DatabricksDbfsFromLocalProfile(CliProfile ?? "DEFAULT");
               }
               catch(ArgumentNullException)
               {
                  AnsiConsole.MarkupLine("[red]Failed[/] to instantiate the client. Make sure either environment variables are set, or official databricks cli is configured.");
                  Environment.Exit(1);
               }
            }

            return _dbc;
         }
      }

   }
}
