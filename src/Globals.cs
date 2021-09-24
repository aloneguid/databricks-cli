using System;
using Stowage;
using Stowage.Impl.Databricks;

namespace Databricks.Sql.Cli
{
   static class Globals
   {
      private static IDatabricksClient _dbc = null;

      public static IDatabricksClient Dbc
      {
         get
         {
            if(_dbc == null)
            {
               string host = Environment.GetEnvironmentVariable("DATABRICKS_HOST");
               string token = Environment.GetEnvironmentVariable("DATABRICKS_TOKEN");
               _dbc = (IDatabricksClient)Files.Of.DatabricksDbfs(new Uri(host), token);
            }

            return _dbc;
         }
      }
   }
}
