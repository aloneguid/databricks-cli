using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Spectre.Console;
using Spectre.Console.Cli;
using Stowage.Impl.Databricks;

namespace Databricks.Cli
{
   public class ListDashboardsCommand : AsyncCommand<BaseFilterSettings>
   {
      public override async Task<int> ExecuteAsync(CommandContext context, BaseFilterSettings settings)
      {
         IReadOnlyCollection<SqlDashboardBase> dashes = await settings.Dbc.ListSqlDashboards();

         if(!string.IsNullOrEmpty(settings.Filter))
            dashes = dashes.Where(
               d => d.Id.Contains(settings.Filter, StringComparison.InvariantCultureIgnoreCase) ||
               d.Name.Contains(settings.Filter, StringComparison.InvariantCultureIgnoreCase)).ToList();

         if(settings.Format == "JSON")
         {
            string json = JsonSerializer.Serialize(dashes);
            Console.WriteLine(json);
         }
         else
         {
            Table table = Ansi.NewTable("Id", "V", "Name", "By");
            foreach(SqlDashboardBase q in dashes)
            {
               string name = "";
               if(q.IsFavourite)
                  name += "🌟";
               name += q.Name;

               if(q.IsDraft)
                  name += "📝";

               table.AddRow(
                  "[grey]" + q.Id.EscapeMarkup() + "[/]",
                  q.Version.ToString(),
                  name.EscapeMarkup(),
                  q.User?.Email?.EscapeMarkup());
            }
            AnsiConsole.Write(table);
         }

         return 0;
      }
   }
}
