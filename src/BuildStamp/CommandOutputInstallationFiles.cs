using System.IO;
using System.Text;

namespace BuildStamp
{
    public class CommandOutputInstallationFiles : ICommand
    {
        public ProgramExitCode Run(ProgramOutput output, ProgramArguments args)
        {
            StringBuilder files = new StringBuilder();
            foreach (var filename in Program.installationFilenames)
            {
                files.AppendLine(filename);
            }

            File.WriteAllText(args.FilenameToOutput, files.ToString(), Encoding.ASCII);

            return ProgramExitCode.Success;
        }
    }
}
