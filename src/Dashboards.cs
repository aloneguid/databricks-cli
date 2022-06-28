using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Databricks.Cli.Settings;
using Spectre.Console;
using Spectre.Console.Cli;
using Stowage.Impl.Databricks;

namespace Databricks.Cli
{

   public class ListDashboardsCommand : ReadyCommand<ListSettings>
   {
      protected override async Task Exec(IDatabricksClient dbc, ListSettings settings)
      {
         IReadOnlyCollection<SqlDashboard> dashes = await Ansi.ListDashboards(dbc, settings.Tag, settings.Name);

         if(settings.Format == "JSON")
         {
            string json = JsonSerializer.Serialize(dashes);
            Console.WriteLine(json);
         }
         else
         {
            Table table = Ansi.NewTable("Id", "Name", "By");
            foreach(SqlDashboard dash in dashes)
            {
               string name = "";
               if(dash.IsFavourite)
                  name += Emoji.Known.GlowingStar;
               name += dash.Name.EscapeMarkup();

               if(dash.Tags != null)
               {
                  string tj = string.Join(", ", dash.Tags.OrderBy(t => t).Select(t => $"[grey]{t.EscapeMarkup()}[/]"));
                  name += $"{Environment.NewLine}   {tj}";
               }

               if(dash.IsDraft)
                  name += Emoji.Known.Pencil;

               table.AddRow(
                  "[grey]" + dash.Id.EscapeMarkup() + "[/]",
                  name,
                  dash.User.Email.EscapeMarkup());
            }
            AnsiConsole.Write(table);
         }
      }
   }

   public class BackupDashboardCommand : ReadyCommand<ListSettings>
   {
      protected override async Task Exec(IDatabricksClient dbc, ListSettings settings)
      {
         IReadOnlyCollection<SqlDashboard> queries = await Ansi.ListDashboards(dbc, settings.Tag, settings.Name);

         await AnsiConsole.Progress()
            .StartAsync(async ctx =>
            {
               ProgressTask? task = ctx.AddTask("backing up...");
               task.MaxValue = queries.Count;
               task.Value = 0;

               foreach(SqlDashboard q in queries)
               {
                  string safeName = new string(q.Name.Select(c => Path.GetInvalidFileNameChars().Contains(c) ? '_' : c).ToArray());

                  string fileName = Path.Combine(Environment.CurrentDirectory, $"D-{safeName}.json");
                  string json = await dbc.GetSqlDashboardRaw(q.Id);
                  await File.WriteAllTextAsync(fileName, json);

                  task.Value += 1;
               }
            });
      }
   }
}
