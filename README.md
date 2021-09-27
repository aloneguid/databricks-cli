# Enhanced Databricks CLI

This tool was created as a missing addition to [Databricks CLI](https://docs.databricks.com/dev-tools/cli/index.html) to manage it from the terminal or CI/CD pipeline. It might be rough around the edges but does the job.

For most commands you should prefer using standard CLI, this tool is only containing enhancements and missing parts.

Although it's written in C# (mostly due to the fact that I already have databricks API exposed via [stowage](https://github.com/aloneguid/stowage)) it is a fully self-contained single-file executable for Linux and Windows - raise a PR to build for Mac if you want to.

## Installation

Download from the **releases** section i.e. (replace version)

```bash
wget https://github.com/aloneguid/databricks-cli/releases/download/1.1.2/dbx-linux-x64.zip
unzip dbx-linux-x64.zip
./dbx ...
```



## Using

First, set `DATABRICKS_HOST` and `DATABRICKS_TOKEN` environment variables to your selected host. Those are standard variables that standard CLI requires. Optionally, you can 

Commands are self-explanatory and can be listed by launching `dbx` without parameters. Run appropriate executable (`dbx` on Linux or `dbx.exe` on Windows) .

### Managing Queries (Databricks SQL)

#### List

```bash
dbx query list
```

returns

| Id   | Name    |
| ---- | ------- |
| 1    | Query 1 |
| 2    | Query 2 |
| 3    | Query 3 |

Optionally, you can pas `--format=JSON` to almost any command, so it can be parsed with **jq**.

#### Take Ownership

```bash
dbx query takeover 1 me@domain.com
```

Takes over an ownership from current owner to `me@domain.com` for query `1` (raw id). This is useful when a person is not available and query needs to be edited/changed.

### Managing Clusters

#### List

```bas
dbx cluster list
```

#### Start/Stop

You can start/stop clusters by passing their id's or name's substring:

```bash
dbx cluster start <substring>
dbx cluster stop <substring>
```

