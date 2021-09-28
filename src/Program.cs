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
            config.PropagateExceptions();

            config.AddBranch<BaseSettings>("query", query =>
            {
               query
                  .AddCommand<ListQueriesCommand>("list")
                  .WithDescription("lists all queries");

               query
                  .AddCommand<TransferOwnershipCommand>("takeover")
                  .WithDescription("transfers ownership of a query to another person");
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
         });

         return await app.RunAsync(args);
      }
   }
}