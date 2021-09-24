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
   public class QuerySetings : CommandSettings
   {
      [Description("output format, can be TABLE (default) or JSON")]
      [CommandOption("-f|--format <FORMAT>")]
      [DefaultValue("TABLE")]
      public string Format { get; set; }
   }

   public class ListQueriesCommand : AsyncCommand<QuerySetings>
   {
      public override async Task<int> ExecuteAsync(CommandContext context, QuerySetings settings)
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
               table.AddRow(q.Id, q.Name);
            }
            AnsiConsole.Render(table);
         }

         return 0;
      }
   }
}
