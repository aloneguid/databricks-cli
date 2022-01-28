using System.Threading.Tasks;
using Databricks.Cli;
using Spectre.Console.Cli;

namespace dbc
{
   class Program
   {
      static async Task<int> Main(string[] args)
      {
         var app = new CommandApp();
         app.Configure(config =>
         {
#if DEBUG
            config.PropagateExceptions();
#endif

            config.AddBranch<BaseSettings>("query", query =>
            {
               query
                  .AddCommand<ListQueriesCommand>("list")
                  .WithDescription("lists all queries");

               query
                  .AddCommand<TransferOwnershipCommand>("takeover")
                  .WithDescription("transfers ownership of a query to another person");
            });

            config.AddBranch<BaseSettings>("dash", query =>
            {
               query
                  .AddCommand<ListDashboardsCommand>("list")
                  .WithDescription("list all dashboards");
            });

            config.AddBranch<BaseSettings>("cluster", query =>
            {
               query
                  .AddCommand<ListClustersCommand>("list")
                  .WithDescription("list all clusters");

               query
                  .AddCommand<StartClusterCommand>("start")
                  .WithDescription("start cluster by id or name");

               query
                  .AddCommand<StopClusterCommand>("stop")
                  .WithDescription("stop cluster by id or name");

            });

            config.AddBranch<BaseSettings>("job", query =>
            {
               query
                  .AddCommand<ListJobsCommand>("list")
                  .WithDescription("list all jobs");

               query
                  .AddCommand<StartJobCommand>("start")
                  .WithDescription("start a job by id or name");

               query
                  .AddCommand<StopJobCommand>("stop")
                  .WithDescription("stop a job by id or name");

               query
                  .AddCommand<UpsertJobCommand>("upsert")
                  .WithDescription("create or update job given it's JSON definition");
            });

            config.AddBranch<BaseSettings>("exec", query =>
            {
               query
                  .AddCommand<ExecPythonCommand>("python")
                  .WithDescription("execute Python code");

               query
                  .AddCommand<ExecScalaCommand>("scala")
                  .WithDescription("execute Scala code");

               query
                  .AddCommand<ExecSqlCommand>("sql")
                  .WithDescription("execute SQL code");
            });
         });

         return await app.RunAsync(args);
      }
   }
}