using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Databricks.Cli;
using Spectre.Console;
using Spectre.Console.Cli;
using Stowage.Impl.Databricks;

namespace Databricks.Cli
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
         IReadOnlyCollection<ClusterInfo> clusters = await settings.Dbc.ListAllClusters();

         if(settings.Format == "JSON")
         {
            string json = JsonSerializer.Serialize(clusters);
            Console.WriteLine(json);
         }
         else
         {
            Table table = Ansi.NewTable("Id", "Name", "Source", "State");
            foreach(ClusterInfo c in clusters)
            {
               table.AddRow(
                  "[grey]" + c.Id.EscapeMarkup() + "[/]",
                  c.Name.EscapeMarkup(),
                  "[grey]" + c.Source + "[/]",
                  Ansi.Sparkup(c.State));
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
         ClusterInfo cluster = await Ansi.FindCluster(settings.Dbc, settings.IdOrName);
         if(cluster == null)
            return 1;

         if(!cluster.IsRunning)
         {
            AnsiConsole.MarkupLine($"starting cluster {cluster.Id} [green]{cluster.Name}[/]...");
            await settings.Dbc.StartCluster(cluster.Id);
         }

         return 0;
      }
   }

   public class StopClusterCommand : AsyncCommand<ClusterSettings>
   {
      public override async Task<int> ExecuteAsync(CommandContext context, ClusterSettings settings)
      {
         AnsiConsole.Markup($"Looking for cluster having [bold yellow]{settings.IdOrName}[/] in it's id or name... ");
         var clusters = (await settings.Dbc.ListAllClusters())
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
            AnsiConsole.MarkupLine($"stopping cluster {cluster.Id} [green]{cluster.Name}[/]...");
            await settings.Dbc.TerminateCluster(cluster.Id);
         }

         return 0;
      }
   }
}
