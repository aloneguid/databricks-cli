using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Spectre.Console;
using Spectre.Console.Cli;
using Stowage.Impl.Databricks;

namespace Databricks.Cli
{
   class ScimListUsersCommand : AsyncCommand<BaseFilterSettings>
   {
      public override async Task<int> ExecuteAsync(CommandContext context, BaseFilterSettings settings)
      {
         IReadOnlyCollection<ScimUser>? users = null;

         await AnsiConsole.Status()
            .StartAsync("Downloading users...", async ctx =>
            {
               users = await settings.Dbc.ScimLsUsers();
            });

         if(users == null)
            return 1;

         AnsiConsole.Markup($"[green]{users.Count}[/] users in total.");


         if(!string.IsNullOrEmpty(settings.Filter))
         {
            users = users
               .Where(u => u.UserName.Contains(settings.Filter, StringComparison.OrdinalIgnoreCase))
               .ToList();
         }

         users = users.OrderBy(u => u.UserName).ToList();

         if(settings.Format == "CSV")
         {
            Console.WriteLine("name,active,group");

            foreach(ScimUser user in users)
            {
               if(user.Groups?.Length > 0)
               {
                  foreach(ScimUser.Group group in user.Groups)
                  {
                     Console.WriteLine($"{user.UserName},{user.IsActive},{group.Display}");
                  }
               }
               else
               {
                  Console.WriteLine($"{user.UserName},{user.IsActive},");
               }
            }
         }
         else
         {
            Table table = Ansi.NewTable("name", "active", "groups");

            foreach(ScimUser user in users)
            {
               string groups = user.Groups == null ? string.Empty : string.Join("; ", user.Groups.Select(g => g.Display));

               table.AddRow(user.UserName, user.IsActive ? "y" : "n", groups);
            }

            AnsiConsole.Write(table);
         }

         return 0;
      }
   }
}
