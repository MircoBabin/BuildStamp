using System;
using System.Globalization;
using System.IO;
using System.Security.Cryptography.X509Certificates;
using System.Threading;

namespace BuildStamp
{
    public class CommandSignExecutable : ICommand
    {
        public const string Sha1Oid = "1.3.14.3.2.26";
        public const string Sha256Oid = "2.16.840.1.101.3.4.2.1";
        public const string Sha384Oid = "2.16.840.1.101.3.4.2.2";

        public ProgramExitCode Run(ProgramOutput output, ProgramArguments args)
        {
            output.WriteOutputLine("File: \"" + args.FilenameToStamp + "\"");

            if (!File.Exists(args.FilenameToStamp))
            {
                output.WriteOutputLine("File does not exist.");
                return ProgramExitCode.FileNotFound;
            }

            WaitForFileNotInUse(args.FilenameToStamp);

            X509Certificate2 signingCertificate;
            if (!string.IsNullOrEmpty(args.CertificatePfxFilename))
            {
                if (!string.IsNullOrEmpty(args.KeePassCertificateTitle))
                {
                    output.WriteOutputLine("Use --certificate OR --keepass-certificate-title, but not both.");
                    return ProgramExitCode.CertificateError;
                }

                output.WriteOutputLine("Code signing certificate: \"" + args.CertificatePfxFilename + "\"");
                if (!File.Exists(args.CertificatePfxFilename))
                {
                    output.WriteOutputLine("Certificate file does not exist.");
                    return ProgramExitCode.CertificateError;
                }

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
            }
            else if (!string.IsNullOrEmpty(args.KeePassCertificateTitle))
            {
                string dll = Path.Combine(Path.GetFullPath(args.KeePassCommanderPath), "KeePassCommandDll.dll");
                if (!File.Exists(dll))
                {
                    output.WriteOutputLine("KeePassCommander not found:");
                    output.WriteOutputLine(dll);
                    return ProgramExitCode.CertificateError;
                }

                KeePassCommand.KeePassEntry.Initialize(dll);
                KeePassCommand.KeePassEntry entry = null;

                string[] fieldnames = null;
                if (!string.IsNullOrEmpty(args.KeePassCertificatePassword))
                    fieldnames = new string[1] { args.KeePassCertificatePassword };

                try
                {
                    entry = KeePassCommand.KeePassEntry.getfirst(args.KeePassCertificateTitle,
                        fieldnames,
                        new string[] { args.KeePassCertificateAttachment });

                    if (entry != null)
                    {
                        if (entry.CommunicationVia != null && entry.CommunicationVia.Count > 0)
                        {
                            var via = entry.CommunicationVia[0];
                            Console.WriteLine();
                            Console.WriteLine("Communicated with KeePass via " + via.SendVia.ToString() + ".");

                            if (via.XmlConfigFilename != null)
                            {
                                string config = Path.GetFullPath(via.XmlConfigFilename);
                                Console.WriteLine("Used configuration: " + config);
                            }

                            if (via.SendVia == KeePassCommand.CommunicationType.FileSystem)
                            {
                                Console.WriteLine("Used filesystem: " + via.FileSystemDirectory);
                            }

                            Console.WriteLine();
                        }

                        if (entry.Title != args.KeePassCertificateTitle)
                        {
                            Console.WriteLine("Could not retrieve KeePass entry.");
                            Console.WriteLine("When using filesystem communication, has KeePass entry \"KeePassCommander.FileSystem ...\"");
                            Console.WriteLine("the following line in the notes section:");
                            Console.WriteLine("KeePassCommanderListAddItem=" + args.KeePassCertificateTitle);
                            Console.WriteLine();

                            entry = null;
                        }
                    }
                }
                catch (Exception ex)
                {
                    output.WriteOutputLine(ex.Message);
                    output.WriteOutputLine();
                }

                if (entry == null)
                {
                    output.WriteOutputLine("Error retrieving certificate from KeePass.");
                    output.WriteOutputLine("1) KeePass is closed or locked.");
                    output.WriteOutputLine("2) --keepass-certificate-title \"" + args.KeePassCertificateTitle + "\" does not exist or is not allowed to be queried.");
                    return ProgramExitCode.CertificateError;
                }

                if (entry.Attachments.Count != 1)
                {
                    output.WriteOutputLine("Error retrieving certificate from KeePass.");
                    output.WriteOutputLine("1) --keepass-certificate-attachment does not exist");
                    return ProgramExitCode.CertificateError;
                }

                string password;
                if (!string.IsNullOrEmpty(args.KeePassCertificatePassword))
                { 
                    if (entry.Fields.Count != 1)
                    {
                        output.WriteOutputLine("Error retrieving certificate from KeePass.");
                        output.WriteOutputLine("1) --keepass-certificate-password does not exist");
                        return ProgramExitCode.CertificateError;
                    }
                    password = entry.Fields[0].Value;
                }
                else
                {
                    password = entry.Password;
                }

                try
                {
                    signingCertificate = new X509Certificate2(entry.Attachments[0].Value, password);
                }
                catch (Exception ex)
                {
                    output.WriteOutputLine("Error loading certificate retrieved from KeePass.");
                    output.WriteOutputLine("Wrong --keepass-certificate-password ?");
                    output.WriteOutputLine(ex.Message);
                    return ProgramExitCode.CertificateError;
                }
            }
            else
            {
                output.WriteOutputLine("Use --certificate OR --keepass-certificate-title.");
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

                    var notValid = CheckCertificateValid(signingCertificate, output);
                    if (notValid != ProgramExitCode.Success) return notValid;

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

                    var notValid = CheckCertificateValid(signingCertificate, output);
                    if (notValid != ProgramExitCode.Success) return notValid;

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

        private ProgramExitCode CheckCertificateValid(X509Certificate2 signingCertificate, ProgramOutput output)
        {
            DateTime validFrom;
            DateTime validUpto;
            {
                var saveCulture = Thread.CurrentThread.CurrentCulture;
                try
                {
                    Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
                    validFrom = DateTime.Parse(signingCertificate.GetEffectiveDateString(), CultureInfo.InvariantCulture).ToUniversalTime();
                    validUpto = DateTime.Parse(signingCertificate.GetExpirationDateString(), CultureInfo.InvariantCulture).ToUniversalTime();
                }
                finally
                {
                    Thread.CurrentThread.CurrentCulture = saveCulture;
                }
            }

            var now = DateTime.Now.ToUniversalTime();
            if (now < validFrom || now > validUpto)
            {
                output.WriteOutputLine();
                output.WriteOutputLine("Current time: " + DateTimeToHumanString(now) + " UTC.");
                output.WriteOutputLine("The certificate is not valid.");

                if (now < validFrom)
                {
                    output.WriteOutputLine("The valid from time (effective time) of the certificate is in the future. The certificate is not yet valid.");
                    output.WriteOutputLine("Valid from: " + DateTimeToHumanString(validFrom) + " UTC.");
                }

                if (now > validUpto)
                {
                    output.WriteOutputLine("The valid upto time (expiration time) of the certificate is in the past. The certificate has expired.");
                    output.WriteOutputLine("Valid upto: " + DateTimeToHumanString(validUpto) + " UTC.");
                }

                return ProgramExitCode.CertificateError;
            }

            return ProgramExitCode.Success;
        }

        public string OidToHumanString(string inputOid)
        {
            if (inputOid == Sha1Oid)
                return "Sha-1";

            if (inputOid == Sha256Oid)
                return "Sha-256";

            if (inputOid == Sha384Oid)
                return "Sha-384";

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
