# ADR CLI tooling

A command-line tool for working with [Architecture Decision Records][ADRs] (ADRs).

This is a Windows equivalent of [adr-tools](https://github.com/npryce/adr-tools).

# CLI Usage

Use the `adr-cli` command to manage ADRs.  Try running `adr-cli --help`.

ADRs are stored in a subdirectory of your project as Markdown files. 
The default directory is `doc/adr`, but you can specify the directory
when you initialise the ADR log.

## 1. Initialize a new ADR log

Create an ADR directory in the root of your project:

        adr-cli init doc/architecture/decisions

This will create a directory named `doc/architecture/decisions` 
containing the first ADR, which records that you are using ADRs
to record architectural decisions and links to 
[Michael Nygard's article on the subject][ADRs].

## 2. Create Architecture Decision Records

        adr-cli new "Implement as .NET Core CLI utility"

This will create a new, numbered ADR file and open it in your editor of choice..

To create a new ADR that supercedes a previous one (ADR 9, for example), use the -s option.

        adr-cli new "Use JSON file for config" -s 9

This will create a new ADR file that is flagged as superceding ADR 9, and changes the status of ADR 9 to indicate that it is superceded by the new ADR.

To create a new ADR that supercedes multiple previous records just use the -s option multiple times.

        adr-cli new "Commit message best practices" -s 3 -s 4

This will create a new ADR file that is flagged as superceding ADR 3 and ADR4, and changes the status of both ADRs to indicate that they are superceded by the new ADR. 

To create a new ADR that links to a previous one (ADR 10, for example), use the -l option.

        adr-cli new "Update library to version 4.3.1" -l "10:Amends:Amended by"

This will create a new ADR file that is flagged as amending ADR 10. Please use the following link format: `TARGET:LINK:REVERSE-LINK` where `TARGET` is the number of linked ADR, `LINK` is the description to add to the new ADR and `REVERSE-LINK` is the description to add to the linked ADR.

To create a new ADR that links multiple previous records just use the -l option multiple times.

        adr-cli new "Commit message best practices" -l "10:Amends:Amended by" -l "11:Clarifies:Clarified by"

Feel free to mix -s and -l options to create appropriate ADR for your needs:

        adr-cli new "Commit message best practices" -s 3 -l "11:Clarifies:Clarified by" -s 4

It then opens the new ADR in your editor of choice.

## 3. Further information about commands and options

For further information, use the built in help:

        adr-cli --help


# Publishing

You can use the `FolderProfile` in this solution to publish the project.

If you're a fan of the dotnet CLI, publish using the following command:

```powershell
dotnet publish -r win-x64 -c Release /p:PublishSingleFile=true /p:PublishTrimmed=true
```

[ADRs]: http://thinkrelevance.com/blog/2011/11/15/documenting-architecture-decisions