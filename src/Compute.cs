using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
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

      [CommandOption("-w|--wait")]
      [Description("wait until operation is complete")]
      public bool Wait { get; set; }
   }

   public class ListClustersCommand : AsyncCommand<BaseSettings>
   {
      public override async Task<int> ExecuteAsync(CommandContext context, BaseSettings settings)
      {
         IReadOnlyCollection<ClusterInfo> clusters = await settings.Dbc.LsClusters();

         var orderedClusters = clusters
            .OrderBy(c => c.Source switch
            {
               "UI" => 0,
               "API" => 1,
               _ => 2
            })
            .ThenBy(c => c.State)
            .ThenBy(c => c.Name.ToLower())
            .ToList();

         if(settings.Format == "JSON")
         {
            string json = JsonSerializer.Serialize(orderedClusters);
            Console.WriteLine(json);
         }
         else
         {
            Table table = Ansi.NewTable("Id", "Name", "Source", "State");
            foreach(ClusterInfo c in orderedClusters)
            {
               table.AddRow(
                  "[grey]" + c.Id.EscapeMarkup() + "[/]",
                  c.Name.EscapeMarkup(),
                  "[grey]" + c.Source + "[/]",
                  Ansi.Sparkup(c.State));
            }
            AnsiConsole.Write(table);
         }

         return 0;
      }
   }

   public class StartClusterCommand : ReadyCommand<ClusterSettings>
   {
      protected override async Task Exec(IDatabricksClient dbc, ClusterSettings settings)
      {
         ClusterInfo? cluster = await Ansi.FindCluster(dbc, settings.IdOrName);
         if(cluster == null) return;

         await Ansi.StartCluster(dbc, cluster, settings.Wait);
      }
   }

   public class StopClusterCommand : AsyncCommand<ClusterSettings>
   {
      public override async Task<int> ExecuteAsync(CommandContext context, ClusterSettings settings)
      {
         AnsiConsole.Markup($"Looking for cluster having [bold yellow]{settings.IdOrName}[/] in it's id or name... ");
         var clusters = (await settings.Dbc.LsClusters())
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
