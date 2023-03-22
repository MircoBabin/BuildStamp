using System;
using System.ComponentModel;
using System.IO;
using System.Runtime.InteropServices;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;

namespace BuildStamp
{
    public class DigitallySignExe_Rfc3161
    {
        private static class NativeMethods
        {
            [Flags]
            public enum SignerSignEx2_Flags : UInt32
            {
                SPC_EXC_PE_PAGE_HASHES_FLAG = 0x10,
                SPC_INC_PE_IMPORT_ADDR_TABLE_FLAG = 0x20,
                SPC_INC_PE_DEBUG_INFO_FLAG = 0x40,
                SPC_INC_PE_RESOURCES_FLAG = 0x80,
                SPC_INC_PE_PAGE_HASHES_FLAG = 0x100,
                SIG_APPEND = 0x1000,
            }

            [Flags]
            public enum SignerSignEx2_TimestampFlags : UInt32
            {
                SIGNER_TIMESTAMP_AUTHENTICODE = 0x0,
                SIGNER_TIMESTAMP_RFC3161 = 0x2,
            }

            [StructLayout(LayoutKind.Sequential)]
            public struct SIGNER_SUBJECT_INFO
            {
                public UInt32 cbSize;
                public IntPtr pdwIndex;
                public SIGNER_SUBJECT_INFO_Subject dwSubjectChoice;

                /* public IntPtr pSignerFileInfo; // SIGNER_FILE_INFO
                 * public IntPtr pSignerBlobInfo; // SIGNER_BLOB_INFO
                 */
                public IntPtr Subject;
            }

            [Flags]
            public enum SIGNER_SUBJECT_INFO_Subject : UInt32
            {
                SIGNER_SUBJECT_FILE = 0x1,
                SIGNER_SUBJECT_BLOB = 0x2,
            }

            [StructLayout(LayoutKind.Sequential)]
            public struct SIGNER_FILE_INFO
            {
                public UInt32 cbSize;
                /* [MarshalAs(UnmanagedType.LPWStr)] string */
                public IntPtr pwszFileName;
                public IntPtr hFile;
            }

            [StructLayout(LayoutKind.Sequential)]
            public struct SIGNER_CERT
            {
                public UInt32 cbSize;
                public SIGNER_CERT_Cert dwCertChoice;

                /* [MarshalAs(UnmanagedType.LPWStr)] string pwszSpcFile;
                 * public IntPtr pCertStoreInfo; // SIGNER_CERT_STORE_INFO
                 * public IntPtr pSpcChainInfo; // SIGNER_SPC_CHAIN_INFO 
                 */
                public IntPtr Cert;

                public IntPtr hwnd;
            }

            [Flags]
            public enum SIGNER_CERT_Cert : UInt32
            {
                SIGNER_CERT_SPC_FILE = 0x1,
                SIGNER_CERT_STORE = 0x2,
                SIGNER_CERT_SPC_CHAIN = 0x3,
            }

            [Flags]
            public enum SIGNER_CERT_POLICY : UInt32
            {
                SIGNER_CERT_POLICY_STORE = 0x1,
                SIGNER_CERT_POLICY_CHAIN = 0x2,
                SIGNER_CERT_POLICY_CHAIN_NO_ROOT = 0x8,
            }

            [StructLayout(LayoutKind.Sequential)]
            public struct SIGNER_CERT_STORE_INFO
            {
                public UInt32 cbSize;
                public IntPtr pSigningCert; /* CERT_CONTEXT */
                public SIGNER_CERT_POLICY dwCertPolicy;
                public IntPtr hCertStore;
            }


            [StructLayout(LayoutKind.Sequential)]
            public struct SIGNER_SIGNATURE_INFO
            {
                public UInt32 cbSize;
                public UInt32 algidHash;
                public SIGNER_SIGNATURE_INFO_Attr dwAttrChoice;

                /* public IntPtr pAttrAuthcode; // SIGNER_ATTR_AUTHCODE
                 */
                public IntPtr Attr;

                public IntPtr psAuthenticated; // PCRYPT_ATTRIBUTES 
                public IntPtr psUnauthenticated; // PCRYPT_ATTRIBUTES 
            }

            [Flags]
            public enum SIGNER_SIGNATURE_INFO_Attr : UInt32
            {
                SIGNER_NO_ATTR = 0x0,
                SIGNER_AUTHCODE_ATTR = 0x1,
            }

            [StructLayout(LayoutKind.Sequential)]
            public struct SIGNER_PROVIDER_INFO
            {
                public UInt32 cbSize;
                [MarshalAs(UnmanagedType.LPWStr)] public string pwszProviderName;
                public UInt32 dwProviderType;
                public UInt32 dwKeySpec;
                public SIGNER_PROVIDER_INFO_Pvk dwPvkChoice;

                /* [MarshalAs(UnmanagedType.LPWStr)] public string pwszPvkFileName;
                 * [MarshalAs(UnmanagedType.LPWStr)] public string pwszKeyContainer;
                 */
                public IntPtr Pvk;
            }

            [Flags]
            public enum SIGNER_PROVIDER_INFO_Pvk : UInt32
            {
                PVK_TYPE_FILE_NAME = 0x1,
                PVK_TYPE_KEYCONTAINER = 0x2,
            }

            [DllImport("mssign32.dll", ExactSpelling = true, CallingConvention = CallingConvention.StdCall)]
            public static extern UInt32 SignerSignEx2(
                SignerSignEx2_Flags dwFlags,
                /* SIGNER_SUBJECT_INFO */ IntPtr pSubjectInfo,
                /* SIGNER_CERT */ IntPtr pSignerCert,
                /* SIGNER_SIGNATURE_INFO */ IntPtr pSignatureInfo,
                /* SIGNER_PROVIDER_INFO */ IntPtr pProviderInfo,
                SignerSignEx2_TimestampFlags dwTimestampFlags,
                [MarshalAs(UnmanagedType.LPStr)] string pszTimeStampAlgorithmOid,
                [MarshalAs(UnmanagedType.LPWStr)] string pwszHttpTimeStamp,
                /* CRYPT_ATTRIBUTES */ IntPtr psRequest,
                IntPtr pSipData,
                out IntPtr ppSignerContext,
                /* CERT_STRONG_SIGN_PARA */ IntPtr pCryptoPolicy,
                IntPtr reserved
             );

            [DllImport("mssign32.dll", ExactSpelling = true, CallingConvention = CallingConvention.StdCall)]
            public static extern UInt32 SignerFreeSignerContext(IntPtr pSignerContext);

            /*
            public const UInt32 X509_ASN_ENCODING = 0x1;
            public const UInt32 PKCS_7_ASN_ENCODING = 0x10000;
            [DllImport("crypt32.dll", ExactSpelling = true, SetLastError = true, CallingConvention = CallingConvention.StdCall)]
            public static extern IntPtr CertCreateCertificateContext(
                UInt32 dwCertEncodingType,
                byte[] pbCertEncoded,
                UInt32 cbCertEncoded);

            [DllImport("crypt32.dll", ExactSpelling = true, CallingConvention = CallingConvention.StdCall)]
            [return: MarshalAs(UnmanagedType.Bool)]
            public static extern bool CertFreeCertificateContext(IntPtr pCertContext);
            */

            public static readonly IntPtr INVALID_HANDLE_VALUE = new IntPtr(-1);
            [DllImport("kernel32.dll", ExactSpelling = true, CharSet = CharSet.Unicode, SetLastError = true, CallingConvention = CallingConvention.StdCall)]
            public static extern IntPtr CreateFileW(
                 [MarshalAs(UnmanagedType.LPWStr)] string filename,
                 [MarshalAs(UnmanagedType.U4)] FileAccess access,
                 [MarshalAs(UnmanagedType.U4)] FileShare share,
                 IntPtr securityAttributes,
                 [MarshalAs(UnmanagedType.U4)] FileMode creationDisposition,
                 [MarshalAs(UnmanagedType.U4)] FileAttributes flagsAndAttributes,
                 IntPtr templateFile);

            [DllImport("kernel32.dll", ExactSpelling = true, SetLastError = true, CallingConvention = CallingConvention.StdCall)]
            [return: MarshalAs(UnmanagedType.Bool)]
            public static extern bool CloseHandle(IntPtr hObject);
        }

        private class SignerSubjectInfo
        {
            public IntPtr marshalPtr = IntPtr.Zero;
            private NativeMethods.SIGNER_SUBJECT_INFO info;

            private IntPtr exeFilenamePtr = IntPtr.Zero;
            private IntPtr exeFilenameHandle = IntPtr.Zero;

            private NativeMethods.SIGNER_FILE_INFO fileInfo;
            private IntPtr fileInfoPtr = IntPtr.Zero;

            private UInt32 pdwIndex;
            private IntPtr pdwIndexPtr = IntPtr.Zero;

            public SignerSubjectInfo(string exeFilename)
            {
                try
                {
                    if (!File.Exists(exeFilename))
                        throw new Exception("File does not exist: " + exeFilename);

                    {
                        var StartTime = DateTime.Now;
                        var timeoutSeconds = 15;
                        //Because of virusscanners retried open

                        while (true)
                        {
                            exeFilenameHandle = NativeMethods.CreateFileW(exeFilename, FileAccess.ReadWrite, FileShare.None, IntPtr.Zero, FileMode.Open, FileAttributes.Normal, IntPtr.Zero);
                            if (exeFilenameHandle != NativeMethods.INVALID_HANDLE_VALUE) break;

                            if (DateTime.Now.Subtract(StartTime).Seconds > timeoutSeconds)
                                throw new Win32Exception(Marshal.GetLastWin32Error());

                            Thread.Sleep(100);
                        }
                    }

                    exeFilenamePtr = Marshal.StringToHGlobalUni(exeFilename);

                    fileInfo = new NativeMethods.SIGNER_FILE_INFO()
                    {
                        cbSize = (uint)Marshal.SizeOf(typeof(NativeMethods.SIGNER_FILE_INFO)),
                        pwszFileName = exeFilenamePtr,
                        hFile = exeFilenameHandle
                    };

                    fileInfoPtr = Marshal.AllocHGlobal((int)fileInfo.cbSize);
                    Marshal.StructureToPtr(fileInfo, fileInfoPtr, false);

                    pdwIndex = 0;
                    pdwIndexPtr = Marshal.AllocHGlobal(Marshal.SizeOf(pdwIndex));
                    Marshal.StructureToPtr(pdwIndex, pdwIndexPtr, false);

                    info = new NativeMethods.SIGNER_SUBJECT_INFO()
                    {
                        cbSize = (uint)Marshal.SizeOf(typeof(NativeMethods.SIGNER_SUBJECT_INFO)),
                        pdwIndex = pdwIndexPtr,
                        dwSubjectChoice = NativeMethods.SIGNER_SUBJECT_INFO_Subject.SIGNER_SUBJECT_FILE,
                        Subject = fileInfoPtr,
                    };

                    marshalPtr = Marshal.AllocHGlobal((int)info.cbSize);
                    Marshal.StructureToPtr(info, marshalPtr, false);
                }
                catch
                {
                    FreeMemory();
                    throw;
                }
            }

            public void FreeMemory()
            {
                if (exeFilenameHandle != IntPtr.Zero)
                {
                    NativeMethods.CloseHandle(exeFilenameHandle);
                    exeFilenameHandle = IntPtr.Zero;
                }

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

                if (marshalPtr != IntPtr.Zero)
                {
                    Marshal.FreeHGlobal(marshalPtr);
                    marshalPtr = IntPtr.Zero;
                }
            }
        }

        private class SignerCert
        {
            public IntPtr marshalPtr = IntPtr.Zero;
            private NativeMethods.SIGNER_CERT cert;

            // private IntPtr certContextPtr = IntPtr.Zero;

            private NativeMethods.SIGNER_CERT_STORE_INFO certStoreInfo;
            private IntPtr certStoreInfoPtr = IntPtr.Zero;

            public SignerCert(X509Certificate2 signingCertificate)
            {
                try
                {
                    /*
                    byte[] rawCertificate = signingCertificate.GetRawCertData();

                    certContextPtr = NativeMethods.CertCreateCertificateContext(
                        NativeMethods.X509_ASN_ENCODING | NativeMethods.PKCS_7_ASN_ENCODING,
                        rawCertificate,
                        (UInt32) rawCertificate.Length);
                    if (certContextPtr == IntPtr.Zero)
                        throw new Win32Exception(Marshal.GetLastWin32Error());
                     */

                    certStoreInfo = new NativeMethods.SIGNER_CERT_STORE_INFO()
                    {
                        cbSize = (uint)Marshal.SizeOf(typeof(NativeMethods.SIGNER_CERT_STORE_INFO)),
                        pSigningCert = signingCertificate.Handle,
                        // pSigningCert = certContextPtr,
                        dwCertPolicy = NativeMethods.SIGNER_CERT_POLICY.SIGNER_CERT_POLICY_CHAIN,
                        hCertStore = IntPtr.Zero,
                    };

                    certStoreInfoPtr = Marshal.AllocHGlobal((int)certStoreInfo.cbSize);
                    Marshal.StructureToPtr(certStoreInfo, certStoreInfoPtr, false);

                    cert = new NativeMethods.SIGNER_CERT()
                    {
                        cbSize = (uint)Marshal.SizeOf(typeof(NativeMethods.SIGNER_CERT)),
                        dwCertChoice = NativeMethods.SIGNER_CERT_Cert.SIGNER_CERT_STORE,
                        Cert = certStoreInfoPtr,
                        hwnd = IntPtr.Zero,
                    };

                    marshalPtr = Marshal.AllocHGlobal((int)cert.cbSize);
                    Marshal.StructureToPtr(cert, marshalPtr, false);
                }
                catch
                {
                    FreeMemory();
                    throw;
                }
            }

            public void FreeMemory()
            {
                /*
                if (certContextPtr != IntPtr.Zero)
                {
                    NativeMethods.CertFreeCertificateContext(certContextPtr);
                    certContextPtr = IntPtr.Zero;
                }
                */

                if (certStoreInfoPtr != IntPtr.Zero)
                {
                    Marshal.FreeHGlobal(certStoreInfoPtr);
                    certStoreInfoPtr = IntPtr.Zero;
                }

                if (marshalPtr != IntPtr.Zero)
                {
                    Marshal.FreeHGlobal(marshalPtr);
                    marshalPtr = IntPtr.Zero;
                }
            }
        }

        private class SignerSignatureInfo
        {
            public IntPtr marshalPtr = IntPtr.Zero;
            private NativeMethods.SIGNER_SIGNATURE_INFO info;

            public SignerSignatureInfo(SignWith hash)
            {
                try
                {
                    UInt32 alg;
                    switch (hash)
                    {
                        case SignWith.Sha256:
                            alg = 0x0000800c; // CALG_SHA_256
                            break;

                        default:
                            throw new Exception("Unknown hash: " + hash.ToString());
                    }

                    info = new NativeMethods.SIGNER_SIGNATURE_INFO()
                    {
                        cbSize = (uint)Marshal.SizeOf(typeof(NativeMethods.SIGNER_SIGNATURE_INFO)),
                        algidHash = alg,
                        dwAttrChoice = NativeMethods.SIGNER_SIGNATURE_INFO_Attr.SIGNER_NO_ATTR,
                        Attr = IntPtr.Zero,
                        psAuthenticated = IntPtr.Zero,
                        psUnauthenticated = IntPtr.Zero,
                    };

                    marshalPtr = Marshal.AllocHGlobal((int)info.cbSize);
                    Marshal.StructureToPtr(info, marshalPtr, false);
                }
                catch
                {
                    FreeMemory();
                    throw;
                }
            }

            public void FreeMemory()
            {
                if (marshalPtr != IntPtr.Zero)
                {
                    Marshal.FreeHGlobal(marshalPtr);
                    marshalPtr = IntPtr.Zero;
                }
            }
        }

        public enum SignWith { Sha256 }
        public enum TimestampWith { RFC3161_Sha256 }
        public static void Sign(string exeFilename, X509Certificate2 signingCertificate, SignWith hash, bool appendHash,
            TimestampWith timestampType, string timestampUrl)
        {
            //timestampUrl must be RFC-3161 timestamp server like http://timestamp.digicert.com

            var SignerSubjectInfo = new SignerSubjectInfo(exeFilename);
            try
            {
                var SignerCert = new SignerCert(signingCertificate);
                try
                {
                    var SignerSignatureInfo = new SignerSignatureInfo(hash);
                    try
                    {
                        NativeMethods.SignerSignEx2_Flags flags = 0;
                        NativeMethods.SignerSignEx2_TimestampFlags timestampFlags;
                        string timestampHashOid;
                        switch (timestampType)
                        {
                            /* This does not work. Timestamp server is not called.
                             * 
                            case TimestampWith.Authenticode:
                                if (appendHash)
                                    throw new Exception("Authenticode can't be appended to existing digital signatures.");
                                timestampFlags = NativeMethods.SignerSignEx2_TimestampFlags.SIGNER_TIMESTAMP_AUTHENTICODE;
                                timestampHashOid = null;
                                break;
                            */

                            case TimestampWith.RFC3161_Sha256:
                                if (appendHash)
                                    flags = NativeMethods.SignerSignEx2_Flags.SIG_APPEND;
                                timestampFlags = NativeMethods.SignerSignEx2_TimestampFlags.SIGNER_TIMESTAMP_RFC3161;
                                timestampHashOid = "2.16.840.1.101.3.4.2.1"; // szOID_NIST_sha256 - OID for sha256
                                break;

                            default:
                                throw new Exception("Unknown hash type: " + hash.ToString());
                        }

                        IntPtr context = IntPtr.Zero;
                        try
                        {
                            UInt32 hResult = NativeMethods.SignerSignEx2(
                                flags,
                                SignerSubjectInfo.marshalPtr,
                                SignerCert.marshalPtr,
                                SignerSignatureInfo.marshalPtr,
                                IntPtr.Zero,
                                timestampFlags,
                                timestampHashOid,
                                timestampUrl,
                                IntPtr.Zero,
                                IntPtr.Zero,
                                out context,
                                IntPtr.Zero,
                                IntPtr.Zero
                            );

                            if (hResult != 0)
                            {
                                throw new Exception(string.Format("SignerSignEx2() - Error code: 0x{0:X}", hResult));
                            }
                        }
                        finally
                        {
                            if (context != IntPtr.Zero)
                            {
                                NativeMethods.SignerFreeSignerContext(context);
                                context = IntPtr.Zero;
                            }
                        }
                    }
                    finally
                    {
                        SignerSignatureInfo.FreeMemory();
                    }
                }
                finally
                {
                    SignerCert.FreeMemory();
                }
            }
            finally
            {
                SignerSubjectInfo.FreeMemory();
            }
        }
    }
}
