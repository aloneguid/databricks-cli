using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;
using Databricks.Sql.Cli;
using Spectre.Console;
using Spectre.Console.Cli;
using Stowage;
using Stowage.Impl.Databricks;

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
               query.AddCommand<ListQueriesCommand>("list").WithDescription("lists all queries");
            });
         });

         return await app.RunAsync(args);
      }
   }
}