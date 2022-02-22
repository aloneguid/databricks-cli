using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Spectre.Console;
using Spectre.Console.Cli;
using Stowage.Impl.Databricks;

namespace Databricks.Cli
{
   public class ListJobsCommand : AsyncCommand<BaseSettings>
   {
      public override async Task<int> ExecuteAsync(CommandContext context, BaseSettings settings)
      {
         IReadOnlyCollection<Job> jobs = await settings.Dbc.ListAllJobs(true);

         if(settings.Format == "JSON")
         {
            string json = JsonSerializer.Serialize(jobs);
            Console.WriteLine(json);
         }
         else
         {
            Table table = Ansi.NewTable("Id", "Name", "State", "Result", "Schedule");
            foreach(Job j in jobs)
            {
               string lfState = j.RanAtLeastOnce
                  ? (j.LastRun?.State?.LifecycleState ?? "")
                  : "";
               string rState = j.RanAtLeastOnce
                  ? (j.LastFinishedRun?.State?.ResultState ?? "")
                  : "";

               table.AddRow(
                  "[grey]" + j.Id + "[/]",
                  j.Name.EscapeMarkup(),
                  Ansi.Sparkup(lfState),
                  Ansi.Sparkup(rState),
                  Ansi.Sparkup(j.ScheduleDisplay));
            }

            AnsiConsole.Render(table);
         }

         return 0;
      }
   }

   public class StartStopJobSettings : BaseSettings
   {

      [CommandArgument(0, "<id-or-name>")]
      [Description("part of job id or name, case insensitive")]
      public string IdOrName { get; set; }
   }

   public class UpsertJobSettings : BaseSettings
   {
      [CommandArgument(1, "<json-file-path>")]
      [Description("path to job json file definition")]
      public string JsonFilePath { get; set; }



      public override ValidationResult Validate()
      {
         return File.Exists(JsonFilePath)
            ? ValidationResult.Success()
            : ValidationResult.Error("JSON file must exist");
      }
   }

   public class StartJobCommand : AsyncCommand<StartStopJobSettings>
   {
      public override async Task<int> ExecuteAsync(CommandContext context, StartStopJobSettings settings)
      {
         Job j = await Ansi.FindJob(settings.Dbc, settings.IdOrName);
         if(j == null)
            return 1;

         AnsiConsole.Markup($"starting job [grey]{j.Id}[/] [green]{j.Name}[/]... ");

         await settings.Dbc.RunJobNow(j.Id);
         AnsiConsole.MarkupLine("[green]done[/]");

         return 0;
      }
   }

   public class StopJobCommand : AsyncCommand<StartStopJobSettings>
   {
      public override async Task<int> ExecuteAsync(CommandContext context, StartStopJobSettings settings)
      {
         Job j = await Ansi.FindJob(settings.Dbc, settings.IdOrName);
         if(j == null)
            return 1;

         AnsiConsole.MarkupLine("loading all job details");
         j = await settings.Dbc.LoadJob(j.Id);

         List<Run> runningRuns = j.Runs.Where(r => r.IsRunning).ToList();
         if(runningRuns.Count == 0)
         {
            AnsiConsole.MarkupLine("job has [red]no[/] active runs");
         }
         else
         {
            AnsiConsole.MarkupLine($"stopping [green]{runningRuns.Count}[/] run(s)");

            foreach(Run run in runningRuns)
            {
               AnsiConsole.Markup($"stopping run [grey]{run.RunId}[/]... ");
               await settings.Dbc.CancelRun(run.RunId);
               AnsiConsole.MarkupLine("[green]ok[/]");
            }
         }

         return 0;
      }
   }

   public class UpsertJobCommand : AsyncCommand<UpsertJobSettings>
   {
      public override async Task<int> ExecuteAsync(CommandContext context, UpsertJobSettings settings)
      {
         AnsiConsole.MarkupLine("loading json definition");

         string jobJson = File.ReadAllText(settings.JsonFilePath);
         Dictionary<string, object> jdict = JsonSerializer.Deserialize<Dictionary<string, object>>(jobJson);
         if(!jdict.TryGetValue("name", out object jobNameObj))
         {
            AnsiConsole.MarkupLine("[red]cannot find job name[/]");
            return 1;
         }

         Job? job = await Ansi.FindJob(settings.Dbc, jobNameObj.ToString());

         if(job == null)
         {
            AnsiConsole.Markup("[green]Creating[/] a new job... ");
            long jobId = await settings.Dbc.CreateJob(jobJson);
            AnsiConsole.Markup($" [yellow]{jobId}[/] ");
         }
         else
         {
            AnsiConsole.Markup("[blue]Updating[/] job... ");
            await settings.Dbc.ResetJob(job.Id, jobJson);
         }

         AnsiConsole.MarkupLine("[green]ok[/]");

         return 0;
      }
   }
}
