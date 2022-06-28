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

   public class TakeoverSettings : BaseSettings
   {
      [CommandArgument(0, "<query-id>")]
      [Description("query id")]
      public string QueryId { get; set; }

      [CommandArgument(1, "<new-owner>")]
      [Description("new owner email")]
      public string NewOwner { get; set; }
   }

   public class GetSettings : BaseSettings
   {
      [CommandArgument(0, "<id>")]
      [Description("query id")]
      public string? Id { get; set; }
   }

   public class DeleteSettings : BaseSettings
   {
      [CommandArgument(0, "<id-or-name>")]
      [Description("id or name of the query, or substring of id or name (case insensitive)")]
      public string? IdOrName { get; set; }
   }

   public class ListQueriesCommand : ReadyCommand<ListSettings>
   {
      protected override async Task Exec(IDatabricksClient dbc, ListSettings settings)
      {
         IReadOnlyCollection<SqlQuery> queries = await Ansi.ListQueries(dbc, settings.Tag, settings.Name);

         if(settings.Format == "JSON")
         {
            string json = JsonSerializer.Serialize(queries);
            Console.WriteLine(json);
         }
         else
         {
            Table table = Ansi.NewTable("Id", "Name", "Tags");
            foreach(SqlQuery q in queries)
            {
               table.AddRow(q.Id,
                  q.Name.EscapeMarkup(),
                  q.Tags?.Length == 0 ? string.Empty : string.Join(",", q.Tags).EscapeMarkup());
            }
            AnsiConsole.Write(table);
         }
      }
   }

   public class GetQueryCommand : ReadyCommand<GetSettings>
   {
      protected override async Task Exec(IDatabricksClient dbc, GetSettings settings)
      {
         string? q = await dbc.GetSqlQueryRaw(settings.Id);

         Console.WriteLine(q);
      }
   }

   public class BackupQueriesCommand : ReadyCommand<ListSettings>
   {
      protected override async Task Exec(IDatabricksClient dbc, ListSettings settings)
      {
         IReadOnlyCollection<SqlQuery> queries = await Ansi.ListQueries(dbc, settings.Tag, settings.Name);

         await AnsiConsole.Progress()
            .StartAsync(async ctx =>
            {
               ProgressTask? task = ctx.AddTask("backing up...");
               task.MaxValue = queries.Count;
               task.Value = 0;

               foreach(SqlQuery q in queries)
               {
                  string safeName = new string(q.Name.Select(c => Path.GetInvalidFileNameChars().Contains(c) ? '_' : c).ToArray());

                  string fileName = Path.Combine(Environment.CurrentDirectory, $"Q-{safeName}.json");
                  string json = await dbc.GetSqlQueryRaw(q.Id);
                  await File.WriteAllTextAsync(fileName, json);

                  task.Value += 1;
               }
            });
      }
   }

   public class DeleteQueryCommand : ReadyCommand<DeleteSettings>
   {
      protected override async Task Exec(IDatabricksClient dbc, DeleteSettings settings)
      {
         IReadOnlyCollection<SqlQuery>? queries = null;

         await AnsiConsole.Progress()
            .StartAsync(async ctx =>
            {
               ProgressTask? task = ctx.AddTask("downloading...");

               queries = await dbc.LsSqlQueries(async (current, total) =>
               {
                  task.MaxValue = total;
                  task.Value = current;
               });
            });

         if(queries == null)
            return;

         List<SqlQuery>? toDelete = queries
            .Where(q => 
               !string.IsNullOrEmpty(settings.IdOrName) && (
                  q.Id.Contains(settings.IdOrName, StringComparison.InvariantCultureIgnoreCase) ||
                  q.Name.Contains(settings.IdOrName, StringComparison.InvariantCultureIgnoreCase)))
            .ToList();

         if(toDelete.Count == 0)
         {
            AnsiConsole.WriteLine("nothing to delete");
            return;
         }

         AnsiConsole.WriteLine($"Queries to delete ({toDelete.Count}):");
         foreach(SqlQuery q in toDelete)
         {
            AnsiConsole.MarkupLine($"{q.Id}: {q.Name.EscapeMarkup()}");
         }


      }
   }
}
