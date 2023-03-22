using System;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Security;
using System.Security.Cryptography.X509Certificates;

namespace BuildStamp
{
    public class DigitallySignExe
    {
        private static class NativeMethods
        {
            // https://learn.microsoft.com/en-us/windows/win32/seccrypto/signertimestampex2
            //
            // https://stackoverflow.com/questions/19293651/cryptoapis-signertimestampex2-using-pinvoke

            [Flags]
            public enum SignerTimeStampEx2_Flags : UInt32
            {
                SIGNER_TIMESTAMP_AUTHENTICODE = 0x1,
                SIGNER_TIMESTAMP_RFC3161 = 0x2,
            }

            [StructLayout(LayoutKind.Sequential)]
            public struct SIGNER_SUBJECT_INFO
            {
                public UInt32 cbSize;
                public IntPtr pdwIndex;
                public SIGNER_SUBJECT_INFO_Choice dwSubjectChoice;

                /* public IntPtr pSignerFileInfo; // SIGNER_FILE_INFO
                 * public IntPtr pSignerBlobInfo; // SIGNER_BLOB_INFO
                 */
                public IntPtr Subject;
            }

            [Flags]
            public enum SIGNER_SUBJECT_INFO_Choice : UInt32
            {
                SIGNER_SUBJECT_FILE = 0x1,
                SIGNER_SUBJECT_BLOB = 0x2,
            }

            [StructLayoutAttribute(LayoutKind.Sequential)]
            public struct SIGNER_FILE_INFO
            {
                public UInt32 cbSize;
                /* [MarshalAs(UnmanagedType.LPWStr)] string */ public IntPtr pwszFileName;
                public IntPtr hFile;
            }

            [DllImport("Mssign32.dll", SetLastError = true)]
            public static extern UInt32 SignerTimeStampEx2(
                SignerTimeStampEx2_Flags dwFlags,
                /* SIGNER_SUBJECT_INFO */ IntPtr pSubjectInfo,
                [MarshalAs(UnmanagedType.LPWStr)] string pwszHttpTimeStamp,
                [MarshalAs(UnmanagedType.LPStr)] string pszTimeStampAlgorithmOid,
                /* CRYPT_ATTRIBUTES */ IntPtr psRequest,
                IntPtr pSipData,
                ref IntPtr ppSignerContext
             );
        }

        public static X509Certificate2 LoadCertificate(string pfxFilename, string pfxPassword)
        {
            return new X509Certificate2(pfxFilename, pfxPassword);
        }

        private class SignerSubjectInfo
        {
            public IntPtr infoPtr = IntPtr.Zero;
            private NativeMethods.SIGNER_SUBJECT_INFO info;

            private IntPtr exeFilenamePtr = IntPtr.Zero;

            private NativeMethods.SIGNER_FILE_INFO fileInfo;
            private IntPtr fileInfoPtr = IntPtr.Zero;

            private UInt32 pdwIndex;
            private IntPtr pdwIndexPtr = IntPtr.Zero;

            public SignerSubjectInfo(string exeFilename)
            {
                try
                {
                    exeFilenamePtr = Marshal.StringToHGlobalUni(exeFilename);

                    fileInfo = new NativeMethods.SIGNER_FILE_INFO
                    {
                        cbSize = (uint)Marshal.SizeOf(typeof(NativeMethods.SIGNER_FILE_INFO)),
                        pwszFileName = exeFilenamePtr,
                        hFile = IntPtr.Zero
                    };

                    fileInfoPtr = Marshal.AllocHGlobal((int) fileInfo.cbSize);
                    Marshal.StructureToPtr(fileInfo, fileInfoPtr, false);

                    pdwIndex = 0;
                    pdwIndexPtr = Marshal.AllocHGlobal(Marshal.SizeOf(pdwIndex));
                    Marshal.StructureToPtr(pdwIndex, pdwIndexPtr, false);

                    info = new NativeMethods.SIGNER_SUBJECT_INFO
                    {
                        cbSize = (uint)Marshal.SizeOf(typeof(NativeMethods.SIGNER_SUBJECT_INFO)),
                        pdwIndex = pdwIndexPtr,
                        dwSubjectChoice = NativeMethods.SIGNER_SUBJECT_INFO_Choice.SIGNER_SUBJECT_FILE,
                        Subject = fileInfoPtr,
                    };

                    infoPtr = Marshal.AllocHGlobal((int) info.cbSize);
                    Marshal.StructureToPtr(info, infoPtr, false);
                }
                catch
                {
                    FreeMemory();
                    throw;
                }
            }

            public void FreeMemory()
            {
                if (exeFilenamePtr != IntPtr.Zero)
                {
                    Marshal.FreeHGlobal(exeFilenamePtr);
                    exeFilenamePtr = IntPtr.Zero;
                }

                if (fileInfoPtr != IntPtr.Zero)
                {
                    Marshal.FreeHGlobal(fileInfoPtr);
                    fileInfoPtr = IntPtr.Zero;
                }

                if (pdwIndexPtr != IntPtr.Zero)
                {
                    Marshal.FreeHGlobal(pdwIndexPtr);
                    pdwIndexPtr = IntPtr.Zero;
                }

                if (infoPtr != IntPtr.Zero)
                {
                    Marshal.FreeHGlobal(infoPtr);
                    infoPtr = IntPtr.Zero;
                }
            }
        }

        public enum SignWith { Sha1, Sha256 }
        public static void Sign(string exeFilename, X509Certificate2 signingCertificate, SignWith hash, string timestampUrl)
        {
            // Doesn't work, signingCertificate is not used.
            // Doesn't add a digital signature to the exeFilename.

            // timestampUrl should be http://timestamp.comodoca.com for Sha1 (Doesn't work, gives SignerTimeStampEx2() - Error code: 0x800B0100)
            // timestampUrl should be http://timestamp.comodoca.com for Sha256

            var SignerSubjectInfo = new SignerSubjectInfo(exeFilename);
            try
            {
                NativeMethods.SignerTimeStampEx2_Flags flags;
                string hashOid;
                switch (hash)
                {
                    case SignWith.Sha1:
                        flags = NativeMethods.SignerTimeStampEx2_Flags.SIGNER_TIMESTAMP_AUTHENTICODE;
                        hashOid = "1.3.14.3.2.26"; // szOID_OIWSEC_sha1 - OID for sha1
                        break;

                    case SignWith.Sha256:
                        flags = NativeMethods.SignerTimeStampEx2_Flags.SIGNER_TIMESTAMP_RFC3161;
                        hashOid = "2.16.840.1.101.3.4.2.1"; // szOID_NIST_sha256 - OID for sha256
                        break;

                    default:
                        throw new Exception("Unknown timestamp type: " + hash.ToString());
                }

                IntPtr context = IntPtr.Zero;
                uint hResult = NativeMethods.SignerTimeStampEx2(
                    flags,
                    SignerSubjectInfo.infoPtr,
                    timestampUrl,
                    hashOid,
                    IntPtr.Zero,
                    IntPtr.Zero,
                    ref context
                 );

                if (hResult != 0)
                {
                    throw new Exception(string.Format("SignerTimeStampEx2() - Error code: 0x{0:X}", hResult));
                }
            }
            finally
            {
                SignerSubjectInfo.FreeMemory();
            }
        }
    }
}
