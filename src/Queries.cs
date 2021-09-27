using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text.Json;
using System.Threading.Tasks;
using Spectre.Console;
using Spectre.Console.Cli;
using Stowage.Impl.Databricks;

namespace Databricks.Sql.Cli
{

   public class TakeoverSettings : BaseSettings
   {
      [CommandArgument(0, "<query-id>")]
      [Description("query id")]
      public string QueryId { get; set; }

      [CommandArgument(1, "<new-owner>")]
      [Description("new owner email")]
      public string NewOwner { get; set; }
   }

   public class ListQueriesCommand : AsyncCommand<BaseSettings>
   {
      public override async Task<int> ExecuteAsync(CommandContext context, BaseSettings settings)
      {
         IReadOnlyCollection<SqlQueryBase> queries = await Globals.Dbc.ListSqlQueries();

         if(settings.Format == "JSON")
         {
            string json = JsonSerializer.Serialize(queries);
            Console.WriteLine(json);
         }
         else
         {
            var table = new Table();
            table.AddColumns("Id", "Name");
            foreach(SqlQueryBase q in queries)
            {
               table.AddRow(q.Id.EscapeMarkup(), q.Name.EscapeMarkup());
            }
            AnsiConsole.Render(table);
         }

         return 0;
      }
   }

   public class TransferOwnershipCommand : AsyncCommand<TakeoverSettings>
   {
      public override async Task<int> ExecuteAsync(CommandContext context, TakeoverSettings settings)
      {
         await Globals.Dbc.TransferQueryOwnership(settings.QueryId, settings.NewOwner);

         return 0;
      }
   }
}
