using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Spectre.Console;
using Spectre.Console.Cli;
using Stowage.Impl.Databricks;

namespace Databricks.Sql.Cli
{
   public class ClusterSettings : BaseSettings
   {
      [CommandArgument(0, "<id-or-name>")]
      [Description("part of cluster id or name, case insensitive")]
      public string IdOrName { get; set; }
   }

   public class ListClustersCommand : AsyncCommand<BaseSettings>
   {
      public override async Task<int> ExecuteAsync(CommandContext context, BaseSettings settings)
      {
         IReadOnlyCollection<ClusterInfo> queries = await Globals.Dbc.ListAllClusters();

         if(settings.Format == "JSON")
         {
            string json = JsonSerializer.Serialize(queries);
            Console.WriteLine(json);
         }
         else
         {
            var table = new Table();
            table.AddColumns("Id", "Name");
            foreach(ClusterInfo q in queries)
            {
               table.AddRow(q.Id.EscapeMarkup(), q.Name.EscapeMarkup());
            }
            AnsiConsole.Render(table);
         }

         return 0;
      }
   }

   public class StartClusterCommand : AsyncCommand<ClusterSettings>
   {
      public override async Task<int> ExecuteAsync(CommandContext context, ClusterSettings settings)
      {
         AnsiConsole.Markup($"Looking for cluster having [bold yellow]{settings.IdOrName}[/] in it's id or name... ");
         var clusters = (await Globals.Dbc.ListAllClusters())
            .Where(c =>
               c.Id.Contains(settings.IdOrName, StringComparison.InvariantCultureIgnoreCase) ||
               c.Name.Contains(settings.IdOrName, StringComparison.InvariantCultureIgnoreCase))
            .ToList();

         if(clusters.Count == 0)
         {
            AnsiConsole.MarkupLine("[red]none.[/]");
            return 1;
         }
         else if(clusters.Count > 1)
         {
            AnsiConsole.MarkupLine($"[red]{clusters.Count}[/] matches (need 1)");
            return 2;
         }

         ClusterInfo cluster = clusters.First();
         AnsiConsole.MarkupLine($"[green]found[/] ({cluster.State})");
         if(!cluster.IsRunning)
         {
            AnsiConsole.Markup($"starting cluster {cluster.Id} [green]{cluster.Name}[/]...");
            await Globals.Dbc.StartCluster(cluster.Id);
         }

         return 0;
      }
   }

   public class StopClusterCommand : AsyncCommand<ClusterSettings>
   {
      public override async Task<int> ExecuteAsync(CommandContext context, ClusterSettings settings)
      {
         AnsiConsole.Markup($"Looking for cluster having [bold yellow]{settings.IdOrName}[/] in it's id or name... ");
         var clusters = (await Globals.Dbc.ListAllClusters())
            .Where(c =>
               c.Id.Contains(settings.IdOrName, StringComparison.InvariantCultureIgnoreCase) ||
               c.Name.Contains(settings.IdOrName, StringComparison.InvariantCultureIgnoreCase))
            .ToList();

         if(clusters.Count == 0)
         {
            AnsiConsole.MarkupLine("[red]none.[/]");
            return 1;
         }
         else if(clusters.Count > 1)
         {
            AnsiConsole.MarkupLine($"[red]{clusters.Count}[/] matches (need 1)");
            return 2;
         }

         ClusterInfo cluster = clusters.First();
         AnsiConsole.MarkupLine($"[green]found[/] ({cluster.State})");
         if(cluster.IsRunning)
         {
            AnsiConsole.Markup($"stopping cluster {cluster.Id} [green]{cluster.Name}[/]...");
            await Globals.Dbc.TerminateCluster(cluster.Id);
         }

         return 0;
      }
   }
}
