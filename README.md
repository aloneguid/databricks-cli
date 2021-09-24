# Databricks SQL CLI

This tool was created as a missing addition to [Databricks CLI](https://docs.databricks.com/dev-tools/cli/index.html) to manage it from the terminal or CI/CD pipeline. It might be rough around the edges but does the job.

Although it's written in C# (mostly due to the fact that I already have databricks API exposed via [stowage](https://github.com/aloneguid/stowage)) it is a fully self-contained single-file executable for Linux and Windows - raise a PR to build for Mac if you want to.

## Installation

Download from the **releases** section. Integration with package managers may be coming in future.

## Using

First, set `DATABRICKS_HOST` and `DATABRICKS_TOKEN` environment variables to your selected host. Those are standard variables that standard CLI requires.

Commands are self-explanatory and can be listed by launching `dbsql` without parameters. Run appropriate executable (`dbsql` on Linux or `dbsql.exe` on Windows) .

### Managing Queries

#### List

```bash
dbsql queries list
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
dbsql query takeover 1 me@domain.com
```

Takes over an ownership from current owner to `me@domain.com` for query `1` (raw id). This is useful when a person is not available and query needs to be edited/changed.