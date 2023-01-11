namespace BuildStamp
{
    public interface ICommand
    {
        ProgramExitCode Run(ProgramOutput output, ProgramArguments args);
    }
}
