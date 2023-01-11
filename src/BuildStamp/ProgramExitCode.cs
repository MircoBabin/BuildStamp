namespace BuildStamp
{
    public enum ProgramExitCode : int
    {
        Success = 0,
        FileNotFound = 1,
        UnknownFileEncoding = 2,
        UnknownLanguage = 3,
        ShowHelp = 98,
        UnknownError = 99
    }
}
