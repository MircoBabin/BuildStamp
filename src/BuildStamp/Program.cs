using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;

namespace BuildStamp
{
    class Program
    {
        public static List<string> installationFilenames = new List<string>()
        {
                "BuildStamp.exe",
                "BuildStamp.exe.config",
        };

        static void Main(string[] commandlineArgs)
        {
            var args = new ProgramArguments(Assembly.GetExecutingAssembly(), commandlineArgs, new Languages());
            ICommand command;

            switch(args.Command)
            {
                default:
                case ProgramArguments.CommandType.ShowHelp:
                case ProgramArguments.CommandType.Unknown:
                    command = new CommandShowHelp();
                    break;

                case ProgramArguments.CommandType.StampFile:
                    command = new CommandStampFile();
                    break;

                case ProgramArguments.CommandType.OutputVersion:
                    command = new CommandOutputVersion();
                    break;

                case ProgramArguments.CommandType.OutputInstallationFiles:
                    command = new CommandOutputInstallationFiles();
                    break;
            }

            ProgramExitCode exitcode = ProgramExitCode.UnknownError;
            ProgramOutput output = new ProgramOutput();
            try
            {
                exitcode = command.Run(output, args);
            }
            catch (Exception ex)
            {
                output.WriteErrorLine(ex.ToString());
            }

            Environment.Exit((int)exitcode);
        }
    }
}
