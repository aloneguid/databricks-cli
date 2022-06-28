using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Spectre.Console;
using Spectre.Console.Cli;
using Stowage;
using Stowage.Impl.Databricks;

namespace Databricks.Cli
{
   public abstract class ReadyCommand<TSettings> : AsyncCommand<TSettings>
      where TSettings : BaseSettings
   {

      public override async Task<int> ExecuteAsync(CommandContext context, TSettings settings)
      {
         IDatabricksClient? dbc = null;

         try
         {
            dbc = (IDatabricksClient)Files.Of.DatabricksDbfsFromLocalProfile(settings.CliProfile ?? "DEFAULT");
         }
         catch(ArgumentNullException)
         {
            AnsiConsole.MarkupLine("[red]Failed[/] to instantiate the client. Make sure either environment variables are set, or official databricks cli is configured.");
            Environment.Exit(1);
         }

         if(dbc == null)
            return 1;

         await Exec(dbc, settings);

         return 0;
      }

      protected abstract Task Exec(IDatabricksClient dbc, TSettings settings);
   }
}
