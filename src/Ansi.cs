using Spectre.Console;

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
   }
}
