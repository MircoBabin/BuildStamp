using System.IO;
using System.Text;

namespace BuildStamp
{
    public class CommandOutputVersion : ICommand
    {
        public ProgramExitCode Run(ProgramOutput output, ProgramArguments args)
        {
            File.WriteAllText(args.FilenameToOutput, args.Version.Major + "." + args.Version.Minor, Encoding.ASCII);

            return ProgramExitCode.Success;
        }
    }
}
