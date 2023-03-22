namespace BuildStamp
{
    public enum ProgramExitCode : int
    {
        Success = 0,
        FileNotFound = 1,
        UnknownFileEncoding = 2,
        UnknownLanguage = 3,
        CertificateError = 10,
        SignSha1Error = 11,
        SignSha256Error = 12,
        VerifySignatureError = 19,
        ShowHelp = 98,
        UnknownError = 99
    }
}
