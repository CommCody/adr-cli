using System;
using System.Runtime.CompilerServices;
using adr.Adr;
using adr.Core;
using McMaster.Extensions.CommandLineUtils;

namespace adr
{
    internal static class Program
    {
        private const string HelpOption = "-?|-h|--help";

        private static void Main(string[] args)
        {
            var app = new CommandLineApplication();
            app.Name = "adr-cli";
            app.Description = "A Windows equivalent of adr-tools (https://github.com/npryce/adr-tools)";

            app.HelpOption(HelpOption);

            app.VersionOption("-v|--version", () =>
            {
                var assemblyName = typeof(AdrEntry).Assembly.GetName();

                return $"{assemblyName.Name} {assemblyName.Version}";
            });

            app.Command("init", (command) =>
            {
                command.Description = "Initialize an ADR repository in the given directory";
                var directory = command.Argument("[directory]", "");
                command.HelpOption(HelpOption);
                command.OnExecute(() =>
                {
                    var settings = AdrSettings.Current;
                    settings.DocFolder = directory.Value ?? settings.DocFolder;
                    settings.Write();
                    new AdrEntry(TemplateType.Adr)
                        .Write()
                        .Launch();
                    return (int)ExitCode.Success;
                });
            });

            app.Command("list", (command) =>
            {
                command.Description = "Show a list of ADRs";
                command.HelpOption(HelpOption);

                command.OnExecute(() =>
                {
                    var docFolder = AdrSettings.Current.DocFolder;

                    AdrManager manager = new AdrManager(System.IO.Path.GetFullPath(docFolder));

                    var records = manager.GetList();

                    foreach (var record in records)
                    {
                        app.Out.WriteLine(record.FullName);
                    }

                    return (int)ExitCode.Success;
                });
            });

            app.Command("new", (command) =>
            {
                command.Description = "Create a new ADR file";
                var title = command.Argument("title", "ADR title");
                var supercedes = command.Option("-s|--supercedes", "", CommandOptionType.MultipleValue);
                command.HelpOption(HelpOption);

                command.OnExecute(() =>
                {
                    var docFolder = AdrSettings.Current.DocFolder;

                    AdrManager manager = new AdrManager(System.IO.Path.GetFullPath(docFolder));

                    var newEntry = new AdrEntry(TemplateType.New) { Title = title.Value ?? "" }
                        .Write()
                        .Launch();

                    if (supercedes.Values.Count > 0)
                    {
                        foreach (var sup in supercedes.Values)
                        {
                            int number = 0;

                            if (int.TryParse(sup, out number))
                            {
                                manager.SupercedesAdr(number, newEntry);
                            }
                        }
                    }

                    return (int)ExitCode.Success;
                });
            });

            app.Command("link", (command) =>
            {
                command.Description = "(Command not implemented)";
                command.OnExecute(() =>
                {
                    app.Out.WriteLine("Command not implemented");

                    return (int)ExitCode.NotImplemented;
                });
            });

            app.Command("generate", (command) =>
            {
                command.Description = "(Command not implemented)";
                command.OnExecute(() =>
                {
                    app.Out.WriteLine("Command not implemented");

                    return (int)ExitCode.NotImplemented;
                });
            });

            app.OnExecute(() =>
            {
                app.ShowHelp();
                return (int)ExitCode.Success;
            });

            try
            {
                Environment.ExitCode = app.Execute(args);
            }
            catch (CommandParsingException ex)
            {
                app.Out.WriteLine($"[ERROR] Cannot parse or recognize the given command: {ex.Message}");
                app.ShowHelp();

                Environment.ExitCode = (int)ExitCode.ParsingError;
            }
            catch (Exception ex)
            {
                app.Out.WriteLine($"[ERROR] Cannot execute the given command: {ex.Message}");
                app.ShowHelp();

                Environment.ExitCode = (int)ExitCode.Error;
            }
        }
    }
}
