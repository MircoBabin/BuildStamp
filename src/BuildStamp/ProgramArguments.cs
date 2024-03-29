﻿using System;
using System.Globalization;
using System.IO;
using System.Reflection;

namespace BuildStamp
{
    public class ProgramArguments
    {
        public enum CommandType { ShowHelp, StampFile, SignExecutable, StampVersionInfo, OutputVersion, OutputInstallationFiles, Unknown }

        public Version Version { get; set; }
        public CommandType Command { get; set; }
        public Languages Languages { get; set; }

        public string VersionFilename { get; set; }

        public string FilenameToStamp { get; set; }
        public ILanguage ProgrammingLanguage { get; }
        public string FilenameToOutput { get; set; }

        public DateTime Now { get; set; }

        public string CertificatePfxFilename { get; set; }
        public string CertificatePfxPassword { get; set; }

        public string KeePassCommanderPath { get; set; }
        public string KeePassCertificateTitle { get; set; }
        public string KeePassCertificateAttachment { get; set; }
        public string KeePassCertificatePassword { get; set; }

        public string SignWithAuthenticodeTimestampUrl { get; set; }
        public string SignWithSha256TimestampUrl { get; set; }

        public ProgramArguments(Assembly main, string[] args, Languages languages)
        {
            /* stamp
             * sign
             * output-version
             * output-installationfilenames
             * --launchdebugger
             * --filename "c:\projects\...\version.template.pas"
             * --language "pascal"
             * --datetime "1975-09-12T23:30:00+02:00"
             * --outputfilename "c:\projects\...\version.pas"
             * --certificate "codesign.pfx"
             * --certficate-password "secret"
             * --sign-with-sha1-timestamp-url "http://timestamp.comodoca.com"
             * --sign-with-sha256-rfc3161-timestamp-url "http://timestamp.comodoca.com/?td=sha256"
             * --keepasscommander-path "c:\KeePass\Plugins"
             * --keepass-certificate-title "title"
             * --keepass-certificate-attachment "name"
             * --keepass-certificate-password "name"
             */

            Languages = languages;
            Version = main.GetName().Version;
            Now = DateTime.Now;

            Command = CommandType.Unknown;
            VersionFilename = string.Empty;
            FilenameToStamp = string.Empty;
            ProgrammingLanguage = null;
            FilenameToOutput = string.Empty;

            CertificatePfxFilename = string.Empty;
            CertificatePfxPassword = string.Empty;

            KeePassCommanderPath = string.Empty;
            KeePassCertificateTitle = string.Empty;
            KeePassCertificateAttachment = string.Empty;
            KeePassCertificatePassword = string.Empty;

            SignWithAuthenticodeTimestampUrl = string.Empty;
            SignWithSha256TimestampUrl = string.Empty;

            if (args.Length == 0)
            {
                Command = CommandType.ShowHelp;
                return;
            }

            int i = 0;
            while (i < args.Length)
            {
                string cmdname = args[i].Trim().ToLowerInvariant();
                if (cmdname == "stamp")
                {
                    Command = CommandType.StampFile;
                }
                else if (cmdname == "sign")
                {
                    Command = CommandType.SignExecutable;
                }
                else if (cmdname == "stamp-versioninfo")
                {
                    Command = CommandType.StampVersionInfo;
                }
                else if (cmdname == "output-version")
                {
                    Command = CommandType.OutputVersion;
                }
                else if (cmdname == "output-installationfilenames")
                {
                    Command = CommandType.OutputInstallationFiles;
                }
                else if (cmdname == "--versionfilename")
                {
                    i++;
                    if (i < args.Length) VersionFilename = args[i].Trim();
                }
                else if (cmdname == "--filename")
                {
                    i++;
                    if (i < args.Length) FilenameToStamp = args[i].Trim();
                }
                else if (cmdname == "--outputfilename")
                {
                    i++;
                    if (i < args.Length) FilenameToOutput = args[i].Trim();
                }
                else if (cmdname == "--language")
                {
                    i++;
                    if (i < args.Length) ProgrammingLanguage = languages.getForName(args[i]);
                }
                else if (cmdname == "--datetime")
                {
                    i++;
                    if (i < args.Length)
                    {
                        string cmdvalue = args[i].Trim();
                        try
                        {
                            Now = DateTime.Parse(cmdvalue, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind);
                            switch(Now.Kind)
                            {
                                case DateTimeKind.Local:
                                case DateTimeKind.Utc:
                                    break;

                                default:
                                    throw new Exception("Please supply ISO-8601 timezone information, the string was parsed as " + Now.Kind + ".");
                            }
                        }
                        catch (Exception ex)
                        {
                            throw new Exception("--datetime \"" + cmdvalue + "\" is not valid ISO-8601. " + ex.Message);
                        }
                    }
                }
                else if (cmdname == "--certificate")
                {
                    i++;
                    if (i < args.Length) CertificatePfxFilename = args[i].Trim();
                }
                else if (cmdname == "--certificate-password")
                {
                    i++;
                    if (i < args.Length) CertificatePfxPassword = args[i].Trim();
                }
                else if (cmdname == "--keepasscommander-path")
                {
                    i++;
                    if (i < args.Length) KeePassCommanderPath = args[i].Trim();
                }
                else if (cmdname == "--keepass-certificate-title")
                {
                    i++;
                    if (i < args.Length) KeePassCertificateTitle = args[i];
                }
                else if (cmdname == "--keepass-certificate-attachment")
                {
                    i++;
                    if (i < args.Length) KeePassCertificateAttachment = args[i];
                }
                else if (cmdname == "--keepass-certificate-password")
                {
                    i++;
                    if (i < args.Length) KeePassCertificatePassword = args[i];
                }
                else if (cmdname == "--sign-with-authenticode-timestamp-url")
                {
                    i++;
                    if (i < args.Length) SignWithAuthenticodeTimestampUrl = args[i].Trim();
                }
                else if (cmdname == "--sign-with-sha256-rfc3161-timestamp-url")
                {
                    i++;
                    if (i < args.Length) SignWithSha256TimestampUrl = args[i].Trim();
                }
                else if (cmdname == "--launchdebugger")
                {
                    System.Diagnostics.Debugger.Launch();
                }

                i++;
            }

            switch (Command)
            {
                case CommandType.StampFile:
                case CommandType.StampVersionInfo:
                    if (string.IsNullOrEmpty(FilenameToOutput)) FilenameToOutput = FilenameToStamp;
                    break;
            }
        }
    }
}
