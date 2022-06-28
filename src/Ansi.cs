using System;
using System.Collections.Generic;
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

         string? color = s switch
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

      public static async Task<ClusterInfo?> FindCluster(IDatabricksClient client, string substr)
      {
         AnsiConsole.Markup($"Looking for cluster having [bold yellow]{substr}[/] in it's id or name... ");
         var clusters = (await client.LsClusters())
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
            AnsiConsole.MarkupLine($"[red]{clusters.Count}[/] matches (inconclusive)");
            return null;
         }

         ClusterInfo cluster = clusters.First();
         AnsiConsole.MarkupLine($"[green]found[/] ({cluster.State})");
         return cluster;
      }

      public static async Task StartCluster(IDatabricksClient client, ClusterInfo cluster, bool wait)
      {
         if(cluster.State != "RUNNING" && cluster.State != "PENDING")
         {
            AnsiConsole.MarkupLine($"starting cluster {cluster.Id} [green]{cluster.Name}[/]...");
            await client.StartCluster(cluster.Id);
         }

         if(wait)
         {
            while(true)
            {
               await Task.Delay(TimeSpan.FromSeconds(1));

               cluster = await client.LoadCluster(cluster.Id);

               AnsiConsole.Write($".{cluster.State}");

               if(cluster.State == "RUNNING")
               {
                  AnsiConsole.WriteLine();
                  return;
               }

               if(cluster.State == "TERMINATED")
                  throw new Exception("could not start cluster");
            }
         }
      }

      public static async Task StopCluster(IDatabricksClient client, ClusterInfo cluster, bool wait)
      {
         if(cluster.State != "TERMINATED")
         {
            AnsiConsole.MarkupLine($"terminating cluster {cluster.Id} [green]{cluster.Name}[/]...");
            await client.TerminateCluster(cluster.Id);
         }

         if(wait)
         {
            while(true)
            {
               await Task.Delay(TimeSpan.FromSeconds(1));

               cluster = await client.LoadCluster(cluster.Id);

               AnsiConsole.Write($".{cluster.State}");

               if(cluster.State == "TERMINATED")
               {
                  AnsiConsole.WriteLine();
                  return;
               }
            }
         }
      }

      public static async Task<Job?> FindJob(IDatabricksClient client, string substr)
      {
         AnsiConsole.Markup($"Looking for a job having [bold yellow]{substr}[/] in it's id or name... ");
         var jobs = (await client.LsJobs(false))
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

      public static async Task<IReadOnlyCollection<SqlQuery>> ListQueries(IDatabricksClient dbc, string? rawTags, string? name)
      {
         IReadOnlyCollection<SqlQuery>? queries = null;

         await AnsiConsole.Progress()
            .StartAsync(async ctx =>
            {
               ProgressTask? task = ctx.AddTask("downloading list...");

               queries = await dbc.LsSqlQueries((current, total) => {
                  task.MaxValue = total;
                  task.Value = current;
                  return Task.CompletedTask;
               });
            });

         if(queries == null)
            return Enumerable.Empty<SqlQuery>().ToList();

         if(!string.IsNullOrEmpty(rawTags))
         {
            string[] tags = rawTags.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            queries = queries
               .Where(q => q.Tags != null && tags.All(t => q.Tags.Contains(t, StringComparer.InvariantCultureIgnoreCase)))
               .ToList();
         }

         if(!string.IsNullOrEmpty(name))
            queries = queries.Where(q => q.Name.Contains(name, StringComparison.InvariantCultureIgnoreCase)).ToList();

         return queries;
      }

      public static async Task<IReadOnlyCollection<SqlDashboard>> ListDashboards(IDatabricksClient dbc, string? rawTags, string? name)
      {
         IReadOnlyCollection<SqlDashboard>? dashboards = null;

         await AnsiConsole.Progress()
            .StartAsync(async ctx =>
            {
               ProgressTask? task = ctx.AddTask("downloading list...");

               dashboards = await dbc.LsSqlDashboards((current, total) => {
                  task.MaxValue = total;
                  task.Value = current;
                  return Task.CompletedTask;
               });
            });

         if(dashboards == null)
            return Enumerable.Empty<SqlDashboard>().ToList();

         if(!string.IsNullOrEmpty(rawTags))
         {
            string[] tags = rawTags.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            dashboards = dashboards
               .Where(q => q.Tags != null && tags.All(t => q.Tags.Contains(t, StringComparer.InvariantCultureIgnoreCase)))
               .ToList();
         }

         if(!string.IsNullOrEmpty(name))
            dashboards = dashboards.Where(q => q.Name.Contains(name, StringComparison.InvariantCultureIgnoreCase)).ToList();

         return dashboards;
      }
   }
}
