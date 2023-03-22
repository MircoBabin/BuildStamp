using System;
using System.IO;
using System.Security.Cryptography.X509Certificates;

namespace BuildStamp
{
    public class CommandSignExecutable : ICommand
    {
        public const string Sha1Oid = "1.3.14.3.2.26";
        public const string Sha256Oid = "2.16.840.1.101.3.4.2.1";

        public ProgramExitCode Run(ProgramOutput output, ProgramArguments args)
        {
            output.WriteOutputLine("File: \"" + args.FilenameToStamp + "\"");

            if (!File.Exists(args.FilenameToStamp))
            {
                output.WriteOutputLine("File does not exist.");
                return ProgramExitCode.FileNotFound;
            }

            WaitForFileNotInUse(args.FilenameToStamp);

            output.WriteOutputLine("Code signing certificate: \"" + args.CertificatePfxFilename + "\"");
            if (!File.Exists(args.FilenameToStamp))
            {
                output.WriteOutputLine("Certificate file does not exist.");
                return ProgramExitCode.FileNotFound;
            }

            X509Certificate2 signingCertificate;
            try
            {
                signingCertificate = new X509Certificate2(args.CertificatePfxFilename, args.CertificatePfxPassword);
            }
            catch (Exception ex)
            {
                output.WriteOutputLine("Error loading ertificate file. Wrong --certificate-password ?");
                output.WriteOutputLine(ex.Message);
                return ProgramExitCode.CertificateError;
            }

            bool signSha1 = false;
            bool signSha256 = false;
            if (!string.IsNullOrWhiteSpace(args.SignWithAuthenticodeTimestampUrl))
            {
                signSha1 = true;

                output.WriteOutputLine();
                output.WriteOutputLine("Replace digital signature with authenticode (sha-1 for older Windows versions).");
                output.WriteOutputLine("Timestamp server url (authenticode): " + args.SignWithAuthenticodeTimestampUrl);
                try
                {
                    DigitallySignExe_Authenticode.Sign(args.FilenameToStamp, signingCertificate, args.SignWithAuthenticodeTimestampUrl);
                }
                catch (Exception ex)
                {
                    output.WriteOutputLine("Error signing with authenticode.");
                    output.WriteOutputLine(ex.Message);
                    return ProgramExitCode.SignSha1Error;
                }
                output.WriteOutputLine("Success: authenticode digital signature.");
            }

            if (!string.IsNullOrWhiteSpace(args.SignWithSha256TimestampUrl))
            {
                signSha256 = true;

                output.WriteOutputLine();
                if (signSha1)
                    output.WriteOutputLine("Append digital signature with sha-256.");
                else
                    output.WriteOutputLine("Replace digital signature with sha-256.");
                output.WriteOutputLine("Timestamp server url (RFC 3161 with sha-256): " + args.SignWithSha256TimestampUrl);
                try
                {
                    DigitallySignExe_Rfc3161.Sign(args.FilenameToStamp, signingCertificate, DigitallySignExe_Rfc3161.SignWith.Sha256, signSha1,
                        DigitallySignExe_Rfc3161.TimestampWith.RFC3161_Sha256, args.SignWithSha256TimestampUrl);
                }
                catch (Exception ex)
                {
                    output.WriteOutputLine("Error signing with sha-256.");
                    output.WriteOutputLine(ex.Message);
                    return ProgramExitCode.SignSha256Error;
                }
                output.WriteOutputLine("Success: sha-256 digital signature.");
            }

            var signatures = DigitallySignExe_Verify.RetrieveSignatures(args.FilenameToStamp);
            bool sha1found = false;
            bool sha256found = false;
            foreach(var signature in signatures)
            {
                output.WriteOutputLine();
                output.WriteOutputLine("Signer                 : " + signature.SignerCertificate.Subject);
                output.WriteOutputLine("Signer-algorithm       : " + OidToHumanString(signature.HashAlgorithmOid));
                foreach (var counterSigner in signature.CounterSignatures)
                {
                    output.WriteOutputLine("    Timestamped by     : " + counterSigner.SignerCertificate.Subject);
                    output.WriteOutputLine("    Timestamp-algorithm: " + OidToHumanString(counterSigner.HashAlgorithmOid));
                    output.WriteOutputLine("    Timestamp          : " + DateTimeToHumanString(counterSigner.Timestamp));
                }

                if (signature.SignerCertificate.Equals(signingCertificate) && signature.CounterSignatures.Count == 1)
                {
                    if (signature.HashAlgorithmOid == Sha1Oid)
                    {
                        sha1found = true;
                        output.WriteOutputLine("Success: sha-1 signature is verified.");
                    }
                    else if (signature.HashAlgorithmOid == Sha256Oid)
                    {
                        sha256found = true;
                        output.WriteOutputLine("Success: sha-256 signature is verified.");
                    }
                }
            }

            ProgramExitCode exitcode = ProgramExitCode.Success;
            output.WriteOutputLine();
            if (signSha1)
            {
                if (!sha1found)
                {
                    output.WriteOutputLine("Error: sha-1 signature could not be verified!");
                    exitcode = ProgramExitCode.VerifySignatureError;
                }
            }
            if (signSha256)
            {
                if (!sha256found)
                {
                    output.WriteOutputLine("Error: sha-256 signature could not be verified!");
                    exitcode = ProgramExitCode.VerifySignatureError;
                }
            }

            return exitcode;
        }

        public string OidToHumanString(string inputOid)
        {
            if (inputOid == Sha1Oid)
                return "Sha-1";

            if (inputOid == Sha256Oid)
                return "Sha-256";

            return inputOid;
        }

        public string DateTimeToHumanString(DateTime? input)
        {
            if (input == null || !input.HasValue)
            {
                return string.Empty;
            }

            return DateTimeToHumanDateString(input) + " at " + DateTimeToHumanTimeString(input) + "h";
        }

        public string DateTimeToHumanDateString(DateTime? input)
        {
            if (input == null || !input.HasValue)
            {
                return string.Empty;
            }
            DateTime time = input.Value;

            string dayname;
            switch (time.DayOfWeek)
            {
                case DayOfWeek.Monday: dayname = "monday"; break;
                case DayOfWeek.Tuesday: dayname = "tuesday"; break;
                case DayOfWeek.Wednesday: dayname = "wednesday"; break;
                case DayOfWeek.Thursday: dayname = "thursday"; break;
                case DayOfWeek.Friday: dayname = "friday"; break;
                case DayOfWeek.Saturday: dayname = "saturday"; break;
                case DayOfWeek.Sunday: dayname = "sunday"; break;
                default: dayname = String.Empty; break;
            }

            string monthname;
            switch (time.Month)
            {
                case 1: monthname = "january"; break;
                case 2: monthname = "february"; break;
                case 3: monthname = "march"; break;
                case 4: monthname = "april"; break;
                case 5: monthname = "may"; break;
                case 6: monthname = "june"; break;
                case 7: monthname = "july"; break;
                case 8: monthname = "august"; break;
                case 9: monthname = "september"; break;
                case 10: monthname = "october"; break;
                case 11: monthname = "november"; break;
                case 12: monthname = "december"; break;
                default: monthname = String.Empty; break;
            }

            return dayname + " " + time.Day + " " + monthname + " " + time.Year.ToString("0000");
        }

        public string DateTimeToHumanTimeString(DateTime? input)
        {
            if (input == null || !input.HasValue)
            {
                return string.Empty;
            }
            DateTime time = input.Value;

            return time.Hour.ToString("00") + ":" + time.Minute.ToString("00") + ":" + time.Second.ToString("00");
        }

        public void WaitForFileNotInUse(string filename, int timeoutSeconds = 60)
        {
            var StartTime = DateTime.Now;

            while (true)
            {
                try
                {
                    using (var fs = File.Open(filename, FileMode.Open, FileAccess.ReadWrite, FileShare.None))
                    {
                        return;
                    }
                }
                catch (Exception ex)
                {
                    if (DateTime.Now.Subtract(StartTime).Seconds > timeoutSeconds)
                        throw new Exception("File is not closed: " + filename + ".\n\n" + ex.Message);
                }
            }
        }
    }
}
