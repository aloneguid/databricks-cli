using System;
using Stowage;
using Stowage.Impl.Databricks;

namespace Databricks.Sql.Cli
{
   static class Globals
   {
      private const string HostEnvVar = "DATABRICKS_HOST";
      private const string TokenEnvVar = "DATABRICKS_TOKEN";
      private static IDatabricksClient _dbc = null;

      public static IDatabricksClient Dbc
      {
         get
         {
            if(_dbc == null)
            {
               string host = Environment.GetEnvironmentVariable(HostEnvVar);
               string token = Environment.GetEnvironmentVariable(TokenEnvVar);

               if(string.IsNullOrEmpty(host) || string.IsNullOrEmpty(token))
               {
                  throw new ArgumentException($"{HostEnvVar} or {TokenEnvVar} variable is not set.");
               }   

               _dbc = (IDatabricksClient)Files.Of.DatabricksDbfs(new Uri(host), token);
            }

            return _dbc;
         }
      }
   }
}
