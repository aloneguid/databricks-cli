using System.Threading.Tasks;
using Databricks.Sql.Cli;
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
            config.AddBranch<QuerySetings>("query", query =>
            {
               query
                  .AddCommand<ListQueriesCommand>("list")
                  .WithDescription("lists all queries");

               query
                  .AddCommand<TransferOwnershipCommand>("takeover")
                  .WithDescription("transfers ownership of a query to another person");
            });
         });

         return await app.RunAsync(args);
      }
   }
}