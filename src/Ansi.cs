using System;
using System.Linq;
using System.Threading.Tasks;
using Spectre.Console;
using Stowage.Impl.Databricks;

namespace Databricks.Cli
{
   static class Ansi
   {
      public static Table NewTable(params string[] columns)
      {
         var table = new Table();
         table.Border = TableBorder.Rounded;
         table.AddColumns(columns);
         table.Expand();
         return table;
      }

      public static string Sparkup(string s)
      {
         if(s == null)
            return "";

         string color = s switch
         {
            "RUNNING" or "SUCCESS" => "green",
            "FAILED" => "red",
            "CANCELED" or "TERMINATED" or "None" => "grey",
            _ => null
         };

         if(color != null)
         {
            return $"[{color}]{s.EscapeMarkup()}[/]";
         }

         return s.EscapeMarkup();
      }

      public static async Task<ClusterInfo> FindCluster(IDatabricksClient client, string substr)
      {
         AnsiConsole.Markup($"Looking for cluster having [bold yellow]{substr}[/] in it's id or name... ");
         var clusters = (await client.ListAllClusters())
            .Where(c =>
               c.Id.Contains(substr, StringComparison.InvariantCultureIgnoreCase) ||
               c.Name.Contains(substr, StringComparison.InvariantCultureIgnoreCase))
            .ToList();

         if(clusters.Count == 0)
         {
            AnsiConsole.MarkupLine("[red]none.[/]");
            return null;
         }
         else if(clusters.Count > 1)
         {
            AnsiConsole.MarkupLine($"[red]{clusters.Count}[/] matches (need 1)");
            return null;
         }

         ClusterInfo cluster = clusters.First();
         AnsiConsole.MarkupLine($"[green]found[/] ({cluster.State})");
         return cluster;
      }

      public static async Task<Job> FindJob(IDatabricksClient client, string substr)
      {
         AnsiConsole.Markup($"Looking for a job having [bold yellow]{substr}[/] in it's id or name... ");
         var jobs = (await client.ListAllJobs(false))
            .Where(j =>
               j.Id.ToString().Contains(substr, StringComparison.InvariantCultureIgnoreCase) ||
               j.Name.Contains(substr, StringComparison.InvariantCultureIgnoreCase))
            .ToList();

         if(jobs.Count == 0)
         {
            AnsiConsole.MarkupLine("[red]none.[/]");
            return null;
         }
         else if(jobs.Count > 1)
         {
            AnsiConsole.MarkupLine($"[red]{jobs.Count}[/] matches (need 1)");
            return null;
         }

         Job job = jobs.First();
         AnsiConsole.MarkupLine($"[green]found[/]");
         return job;
      }
   }
}
