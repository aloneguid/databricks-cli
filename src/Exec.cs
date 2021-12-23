using System;
using System.ComponentModel;
using System.IO;
using System.Threading.Tasks;
using Spectre.Console;
using Spectre.Console.Cli;
using Stowage.Impl.Databricks;

namespace Databricks.Cli
{
   public class ExecSettings : BaseSettings
   {
      [CommandOption("-c|--cluster")]
      [Description("part of cluster id or name, case insensitive")]
      public string Cluster { get; set; }

      [CommandOption("--code")]
      [Description("code to execute")]
      public string Code { get; set; }

      [CommandOption("--path")]
      [Description("code filename to execute, basic patterns are supported")]
      public string Path { get; set; }

      public override ValidationResult Validate()
      {
         if(string.IsNullOrEmpty(Cluster))
            return ValidationResult.Error("cluster is required");

         if(string.IsNullOrEmpty(Code) && string.IsNullOrEmpty(Path))
            return ValidationResult.Error("code or path is required");

         return ValidationResult.Success();
      }
   }

   static class CommonExec
   {
      static async Task Exec(string clusterId, Language lang, ExecSettings settings, string code = null)
      {
         if(code == null) code = settings.Code;

         string abs = code.Replace("\r", " ").Replace("\n", " ").Replace("  ", "");
         abs = abs.Substring(0, Math.Min(60, abs.Length)) + "...";
         AnsiConsole.WriteLine(abs);

         string result = await settings.Dbc.Exec(
            clusterId,
            lang,
            code ?? settings.Code,
            msg => AnsiConsole.MarkupLine($"  .. [grey]{msg}[/]"));

         AnsiConsole.MarkupLine("[green]success[/]");

         if(!string.IsNullOrEmpty(result))
         {
            AnsiConsole.MarkupLine("result:");
            Console.WriteLine(result);
         }
      }

      public static async Task<int> ExecuteAsync(CommandContext context, ExecSettings settings, Language lang)
      {
         ClusterInfo cluster = await Ansi.FindCluster(settings.Dbc, settings.Cluster);
         if(cluster == null)
            return 1;

         if(!string.IsNullOrEmpty(settings.Code))
         {
            AnsiConsole.MarkupLine("executing...");

            await Exec(cluster.Id, lang, settings);
         }
         else
         {
            string dir = settings.Path.Contains(Path.DirectorySeparatorChar)
               ? Path.GetDirectoryName(settings.Path)
               : Directory.GetCurrentDirectory();

            string file = Path.GetFileName(settings.Path);

            string[] files = Directory.GetFiles(dir, file);

            foreach(string fileEntry in files)
            {
               AnsiConsole.MarkupLine($"executing [white]{Path.GetFileName(fileEntry)}[/] [grey]({fileEntry})[/]...");

               string code = File.ReadAllText(fileEntry);

               await Exec(cluster.Id, lang, settings, code);
            }
         }

         return 0;
      }
   }

   public class ExecPythonCommand : AsyncCommand<ExecSettings>
   {
      public override Task<int> ExecuteAsync(CommandContext context, ExecSettings settings)
      {
         return CommonExec.ExecuteAsync(context, settings, Language.Python);
      }
   }

   public class ExecScalaCommand : AsyncCommand<ExecSettings>
   {
      public override Task<int> ExecuteAsync(CommandContext context, ExecSettings settings)
      {
         return CommonExec.ExecuteAsync(context, settings, Language.Scala);
      }
   }

   public class ExecSqlCommand : AsyncCommand<ExecSettings>
   {
      public override Task<int> ExecuteAsync(CommandContext context, ExecSettings settings)
      {
         return CommonExec.ExecuteAsync(context, settings, Language.Sql);
      }
   }
}
