using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Security.Cryptography.X509Certificates;

namespace BuildStamp
{
    public class DigitallySignExe_Verify
    {
        private static class NativeMethods
        {
            public static readonly IntPtr INVALID_HANDLE_VALUE = new IntPtr(-1);

            public static Guid WINTRUST_ACTION_GENERIC_VERIFY_V2 = new Guid("{00AAC56B-CD44-11d0-8CC2-00C04FC295EE}");

            public const string szOID_RSA_counterSign = "1.2.840.113549.1.9.6";
            public const string szOID_RSA_signingTime = "1.2.840.113549.1.9.5";

            [Flags]
            public enum EncodingType : UInt32
            {
                PKCS_7_ASN_ENCODING = 0x10000,
                X509_ASN_ENCODING = 0x1
            }


            [StructLayout(LayoutKind.Sequential)]
            public struct WINTRUST_FILE_INFO
            {
                public UInt32 cbStruct;
                [MarshalAs(UnmanagedType.LPWStr)] public string pcwszFilePath;
                public IntPtr hFile;
                public IntPtr pgKnownSubject; // GUID
            }

            [StructLayout(LayoutKind.Sequential)]
            public struct WINTRUST_DATA
            {
                public UInt32 cbStruct;
                public IntPtr pPolicyCallbackData;
                public IntPtr pSIPClientData;
                public WINTRUST_DATA_UIChoice dwUIChoice;
                public WINTRUST_DATA_RevocationChecks fdwRevocationChecks;
                public WINTRUST_DATA_UnionChoice dwUnionChoice;

                /* WINTRUST_FILE_INFO    pFile;
                 * WINTRUST_CATALOG_INFO pCatalog;
                 * WINTRUST_BLOB_INFO    pBlob
                 * WINTRUST_SGNR_INFO    pSgnr;
                 * WINTRUST_CERT_INFO    pCert;
                 */
                public IntPtr Union;

                public WINTRUST_DATA_StateAction dwStateAction;
                public IntPtr hWVTStateData;
                public IntPtr pwszURLReference; // Reserved for future use. Set to NULL.
                public WINTRUST_DATA_ProvFlags dwProvFlags;
                public WINTRUST_DATA_UIContext dwUIContext;
                public IntPtr pSignatureSettings; // WINTRUST_SIGNATURE_SETTINGS
            }

            public enum WINTRUST_DATA_UIChoice : UInt32
            {
                WTD_UI_ALL = 1,
                WTD_UI_NONE = 2,
                WTD_UI_NOBAD = 3,
                WTD_UI_NOGOOD = 4,
            }

            public enum WINTRUST_DATA_RevocationChecks : UInt32
            {
                WTD_REVOKE_NONE = 0,
                WTD_REVOKE_WHOLECHAIN = 1,
            }

            public enum WINTRUST_DATA_UnionChoice : UInt32
            {
                WTD_CHOICE_FILE = 1,
                WTD_CHOICE_CATALOG = 2,
                WTD_CHOICE_BLOB = 3,
                WTD_CHOICE_SIGNER = 4,
                WTD_CHOICE_CERT = 5,
            }

            public enum WINTRUST_DATA_StateAction : UInt32
            {
                WTD_STATEACTION_IGNORE = 0x00000000,
                WTD_STATEACTION_VERIFY = 0x00000001,
                WTD_STATEACTION_CLOSE = 0x00000002,
                WTD_STATEACTION_AUTO_CACHE = 0x00000003,
                WTD_STATEACTION_AUTO_CACHE_FLUSH = 0x00000004,
            }

            [Flags]
            public enum WINTRUST_DATA_ProvFlags : UInt32
            {
                WTD_USE_IE4_TRUST_FLAG = 0x1,
                WTD_NO_IE4_CHAIN_FLAG = 0x2,
                WTD_NO_POLICY_USAGE_FLAG = 0x4,
                WTD_REVOCATION_CHECK_NONE = 0x10,
                WTD_REVOCATION_CHECK_END_CERT = 0x20,
                WTD_REVOCATION_CHECK_CHAIN = 0x40,
                WTD_REVOCATION_CHECK_CHAIN_EXCLUDE_ROOT = 0x80,
                WTD_SAFER_FLAG = 0x100,
                WTD_HASH_ONLY_FLAG = 0x200,
                WTD_USE_DEFAULT_OSVER_CHECK = 0x400,
                WTD_LIFETIME_SIGNING_FLAG = 0x800,
                WTD_CACHE_ONLY_URL_RETRIEVAL = 0x1000,
                WTD_DISABLE_MD2_MD4 = 0x2000,
                WTD_MOTW = 0x4000,
            }

            public enum WINTRUST_DATA_UIContext : UInt32
            {
                WTD_UICONTEXT_EXECUTE = 0,
                WTD_UICONTEXT_INSTALL = 1,
            }

            [StructLayout(LayoutKind.Sequential)]
            public struct WINTRUST_SIGNATURE_SETTINGS
            {
                public UInt32 cbStruct;
                public UInt32 dwIndex;
                public WINTRUST_SIGNATURE_SETTINGS_Flags dwFlags;
                public UInt32 cSecondarySigs;
                public UInt32 dwVerifiedSigIndex;
                public IntPtr pCryptoPolicy; // PCERT_STRONG_SIGN_PARA 
            }

            public enum WINTRUST_SIGNATURE_SETTINGS_Flags : UInt32
            {
                WSS_VERIFY_SPECIFIC = 0x00000001,
                WSS_GET_SECONDARY_SIG_COUNT = 0x00000002,
            }

            [StructLayout(LayoutKind.Sequential)]
            public struct CRYPT_PROVIDER_DATA
            {
                public UInt32 cbStruct;
                public IntPtr pWintrustData; // WINTRUST_DATA
                [MarshalAs(UnmanagedType.Bool)] public bool fOpenedFile;
                public IntPtr hWndParent;
                public IntPtr pgActionID; //GUID
                public IntPtr hProv; //HCRYPTPROV
                public UInt32 dwError;
                public UInt32 dwRegSecuritySettings;
                public UInt32 dwRegPolicySettings;
                public IntPtr psPfns; // CRYPT_PROVIDER_FUNCTIONS

                public UInt32 cdwTrustStepErrors;
                public IntPtr padwTrustStepErrors;

                public UInt32 chStores;
                public IntPtr pahStores;

                public UInt32 dwEncoding;
                public IntPtr hMsg; // HCRYPTMSG

                public UInt32 csSigners;
                public IntPtr pasSigners;

                public UInt32 csProvPrivData;
                public IntPtr pasProvPrivData;

                public UInt32 dwSubjectChoice;
                public IntPtr Subject;

                [MarshalAs(UnmanagedType.LPStr)] public string pszUsageOID;
                [MarshalAs(UnmanagedType.Bool)] public bool fRecallWithState;
                public System.Runtime.InteropServices.ComTypes.FILETIME sftSystemTime;
                [MarshalAs(UnmanagedType.LPStr)] public string pszCTLSignerUsageOID;
                public UInt32 dwProvFlags;
                public UInt32 dwFinalError;
                public IntPtr pRequestUsage; // PCERT_USAGE_MATCH                   
                public UInt32 dwTrustPubSettings;
                public UInt32 dwUIStateFlags;
                public IntPtr pSigState; // CRYPT_PROVIDER_SIGSTATE
                public IntPtr pSigSettings; // WINTRUST_SIGNATURE_SETTINGS
            }

            [StructLayout(LayoutKind.Sequential)]
            public struct CRYPT_PROVIDER_SGNR
            {
                public UInt32 cbStruct;
                public System.Runtime.InteropServices.ComTypes.FILETIME sftVerifyAsOf;

                public UInt32 csCertChain;
                public IntPtr pasCertChain; // CRYPT_PROVIDER_CERT

                public UInt32 dwSignerType;
                public IntPtr psSigner; // CMSG_SIGNER_INFO
                public UInt32 dwError;

                public UInt32 csCounterSigners;
                public IntPtr pasCounterSigners; // CRYPT_PROVIDER_SGNR 

                public IntPtr pChainContext; // CERT_CHAIN_CONTEXT        
            }

            [StructLayout(LayoutKind.Sequential)]
            public struct CMSG_SIGNER_INFO
            {
                public UInt32 dwVersion;
                public CRYPTOAPI_BLOB Issuer;
                public CRYPTOAPI_BLOB SerialNumber;
                public CRYPT_ALGORITHM_IDENTIFIER HashAlgorithm;
                public CRYPT_ALGORITHM_IDENTIFIER HashEncryptionAlgorithm;
                public CRYPTOAPI_BLOB EncryptedHash;
                public CRYPT_ATTRIBUTES AuthAttrs;
                public CRYPT_ATTRIBUTES UnauthAttrs;
            }

            [StructLayout(LayoutKind.Sequential)]
            public struct CRYPTOAPI_BLOB
            {
                public UInt32 cbData;
                public IntPtr pbData;
            }

            [StructLayout(LayoutKind.Sequential)]
            public struct CRYPT_BIT_BLOB
            {
                public UInt32 cbData;
                public IntPtr pbData;
                public UInt32 cUnusedBits;
            }

            [StructLayout(LayoutKind.Sequential)]
            public struct CRYPT_ALGORITHM_IDENTIFIER
            {
                [MarshalAs(UnmanagedType.LPStr)] public string pszObjId;
                public CRYPTOAPI_BLOB Parameters;
            }

            [StructLayout(LayoutKind.Sequential)]
            public struct CRYPT_ATTRIBUTES
            {
                public UInt32 cAttr;
                public IntPtr rgAttr;
            }

            [StructLayout(LayoutKind.Sequential)]
            public struct CRYPT_ATTRIBUTE
            {
                [MarshalAs(UnmanagedType.LPStr)] public string pszObjId;
                public UInt32 cValue;
                public IntPtr rgValue; // CRYPTOAPI_BLOB 
            }

            [StructLayout(LayoutKind.Sequential)]
            public struct CRYPT_PROVIDER_CERT
            {
                public UInt32 cbStruct;
                public IntPtr pCert; // CERT_CONTEXT
                [MarshalAs(UnmanagedType.Bool)] public bool fCommercial;
                [MarshalAs(UnmanagedType.Bool)] public bool fTrustedRoot;
                [MarshalAs(UnmanagedType.Bool)] public bool fSelfSigned;
                [MarshalAs(UnmanagedType.Bool)] public bool fTestCert;
                public UInt32 dwRevokedReason;
                public UInt32 dwConfidence;
                public UInt32 dwError;
                public IntPtr pTrustListContext; // CTL_CONTEXT
                [MarshalAs(UnmanagedType.Bool)] public bool fTrustListSignerCert;
                public IntPtr pCtlContext;// CTL_CONTEXT
                public UInt32 dwCtlError;
                [MarshalAs(UnmanagedType.Bool)] public bool fIsCyclic;
                public IntPtr pChainElement; // CERT_CHAIN_ELEMENT 
            }

            [StructLayout(LayoutKind.Sequential)]
            public struct CERT_CONTEXT
            {
                public UInt32 dwCertEncodingType;
                public IntPtr pbCertEncoded;
                public UInt32 cbCertEncoded;
                public IntPtr pCertInfo;
                public IntPtr hCertStore;
            }

            [DllImport("wintrust.dll", CallingConvention = CallingConvention.StdCall, ExactSpelling = true)]
            public static extern Int32 WinVerifyTrust(IntPtr hWind, IntPtr pgActionID, IntPtr pWVTData);

            [DllImport("wintrust.dll", CallingConvention = CallingConvention.StdCall, ExactSpelling = true)]
            public static extern IntPtr WTHelperProvDataFromStateData(IntPtr hStateData);

            [DllImport("wintrust.dll", CallingConvention = CallingConvention.StdCall, ExactSpelling = true)]
            public static extern IntPtr WTHelperGetProvSignerFromChain(
                IntPtr pProvData, 
                UInt32 idxSigner, 
                [MarshalAs(UnmanagedType.Bool)] bool fCounterSigner, 
                UInt32 idxCounterSigner);

            [DllImport("wintrust.dll", CallingConvention = CallingConvention.StdCall, ExactSpelling = true)]
            public static extern IntPtr WTHelperGetProvCertFromChain(IntPtr pSgnr, UInt32 idxCert);
        }

        public class Signature
        {
            public string HashAlgorithmOid { get; set; }
            public X509Certificate2 SignerCertificate { get; set; }

            public List<CounterSignature> CounterSignatures { get; set; }

            public Signature()
            {
                CounterSignatures = new List<CounterSignature>();
            }
        }

        public class CounterSignature
        {
            public string HashAlgorithmOid { get; set; }
            public X509Certificate2 SignerCertificate { get; set; }
            public DateTime? Timestamp;
        }

        public static List<Signature> RetrieveSignatures(string exeFilename)
        {
            List<Signature> signatures = new List<Signature>();

            IntPtr actionIdBytesPtr = IntPtr.Zero;
            IntPtr fileInfoPtr = IntPtr.Zero;
            IntPtr signatureSettingsPtr = IntPtr.Zero;
            IntPtr trustDataPtr = IntPtr.Zero;
            try
            {
                byte[] actionIdBytes = NativeMethods.WINTRUST_ACTION_GENERIC_VERIFY_V2.ToByteArray();
                actionIdBytesPtr = Marshal.AllocHGlobal(actionIdBytes.Length);
                Marshal.Copy(actionIdBytes, 0, actionIdBytesPtr, actionIdBytes.Length);

                NativeMethods.WINTRUST_FILE_INFO fileInfo = new NativeMethods.WINTRUST_FILE_INFO()
                {
                    cbStruct = (UInt32)Marshal.SizeOf(typeof(NativeMethods.WINTRUST_FILE_INFO)),
                    pcwszFilePath = exeFilename,
                    hFile = IntPtr.Zero,
                    pgKnownSubject = IntPtr.Zero,
                };
                fileInfoPtr = Marshal.AllocHGlobal((int)fileInfo.cbStruct);
                Marshal.StructureToPtr(fileInfo, fileInfoPtr, false);

                NativeMethods.WINTRUST_SIGNATURE_SETTINGS signatureSettings = new NativeMethods.WINTRUST_SIGNATURE_SETTINGS()
                {
                    cbStruct = (UInt32)Marshal.SizeOf(typeof(NativeMethods.WINTRUST_SIGNATURE_SETTINGS)),
                    dwIndex = 0,
                    dwFlags = NativeMethods.WINTRUST_SIGNATURE_SETTINGS_Flags.WSS_GET_SECONDARY_SIG_COUNT,
                    cSecondarySigs = 0,
                    dwVerifiedSigIndex = 0,
                    pCryptoPolicy = IntPtr.Zero,
                };
                signatureSettingsPtr = Marshal.AllocHGlobal((int)signatureSettings.cbStruct);
                Marshal.StructureToPtr(signatureSettings, signatureSettingsPtr, false);

                NativeMethods.WINTRUST_DATA trustData = new NativeMethods.WINTRUST_DATA()
                {
                    cbStruct = (UInt32)Marshal.SizeOf(typeof(NativeMethods.WINTRUST_DATA)),
                    pPolicyCallbackData = IntPtr.Zero,
                    pSIPClientData = IntPtr.Zero,
                    dwUIChoice = NativeMethods.WINTRUST_DATA_UIChoice.WTD_UI_NONE,
                    fdwRevocationChecks = NativeMethods.WINTRUST_DATA_RevocationChecks.WTD_REVOKE_NONE,
                    dwUnionChoice = NativeMethods.WINTRUST_DATA_UnionChoice.WTD_CHOICE_FILE,
                    Union = fileInfoPtr,
                    dwStateAction = NativeMethods.WINTRUST_DATA_StateAction.WTD_STATEACTION_IGNORE,
                    hWVTStateData = IntPtr.Zero,
                    pwszURLReference = IntPtr.Zero,
                    dwProvFlags = NativeMethods.WINTRUST_DATA_ProvFlags.WTD_REVOCATION_CHECK_NONE,
                    dwUIContext = NativeMethods.WINTRUST_DATA_UIContext.WTD_UICONTEXT_EXECUTE,
                    pSignatureSettings = signatureSettingsPtr,
                };
                trustDataPtr = Marshal.AllocHGlobal((int)trustData.cbStruct);
                Marshal.StructureToPtr(trustData, trustDataPtr, false);

                {
                    int result = NativeMethods.WinVerifyTrust(NativeMethods.INVALID_HANDLE_VALUE, actionIdBytesPtr, trustDataPtr);
                    if (result != 0)
                        throw new Exception(string.Format("WinVerifyTrust() failed with 0x{0:X}", result));
                    signatureSettings = (NativeMethods.WINTRUST_SIGNATURE_SETTINGS)Marshal.PtrToStructure(signatureSettingsPtr, typeof(NativeMethods.WINTRUST_SIGNATURE_SETTINGS));
                }

                UInt32 signatureCount = signatureSettings.cSecondarySigs + 1;
                for (UInt32 dwIndex = 0; dwIndex < signatureCount; dwIndex++)
                {
                    signatureSettings.dwIndex = dwIndex;
                    signatureSettings.dwFlags = NativeMethods.WINTRUST_SIGNATURE_SETTINGS_Flags.WSS_VERIFY_SPECIFIC;
                    Marshal.StructureToPtr(signatureSettings, signatureSettingsPtr, false);

                    trustData.dwStateAction = NativeMethods.WINTRUST_DATA_StateAction.WTD_STATEACTION_VERIFY;
                    trustData.hWVTStateData = IntPtr.Zero;
                    Marshal.StructureToPtr(trustData, trustDataPtr, false);

                    {
                        int result = NativeMethods.WinVerifyTrust(NativeMethods.INVALID_HANDLE_VALUE, actionIdBytesPtr, trustDataPtr);
                        if (result != 0)
                            continue;
                        trustData = (NativeMethods.WINTRUST_DATA)Marshal.PtrToStructure(trustDataPtr, typeof(NativeMethods.WINTRUST_DATA));
                    }

                    try
                    {
                        IntPtr cryptProviderDataPtr = NativeMethods.WTHelperProvDataFromStateData(trustData.hWVTStateData);
                        NativeMethods.CRYPT_PROVIDER_DATA cryptProviderData = (NativeMethods.CRYPT_PROVIDER_DATA)Marshal.PtrToStructure(cryptProviderDataPtr, typeof(NativeMethods.CRYPT_PROVIDER_DATA));

                        for (UInt32 idxSigner = 0; idxSigner < cryptProviderData.csSigners; idxSigner++)
                        {
                            IntPtr cryptProviderSgnrPtr = NativeMethods.WTHelperGetProvSignerFromChain(cryptProviderDataPtr, idxSigner, false, 0);
                            if (cryptProviderSgnrPtr == IntPtr.Zero)
                                continue;

                            NativeMethods.CRYPT_PROVIDER_SGNR cryptProviderSgnr = (NativeMethods.CRYPT_PROVIDER_SGNR)Marshal.PtrToStructure(cryptProviderSgnrPtr, typeof(NativeMethods.CRYPT_PROVIDER_SGNR));
                            if (cryptProviderSgnr.psSigner == IntPtr.Zero)
                                continue;
                            NativeMethods.CMSG_SIGNER_INFO signer = (NativeMethods.CMSG_SIGNER_INFO)Marshal.PtrToStructure(cryptProviderSgnr.psSigner, typeof(NativeMethods.CMSG_SIGNER_INFO));

                            IntPtr cryptProviderCertPtr = NativeMethods.WTHelperGetProvCertFromChain(cryptProviderSgnrPtr, idxSigner);
                            if (cryptProviderCertPtr == IntPtr.Zero)
                                continue;
                            NativeMethods.CRYPT_PROVIDER_CERT cryptProviderCert = (NativeMethods.CRYPT_PROVIDER_CERT)Marshal.PtrToStructure(cryptProviderCertPtr, typeof(NativeMethods.CRYPT_PROVIDER_CERT));
                            if (cryptProviderCert.cbStruct == 0)
                                continue;
                            X509Certificate2 signerCertificate;
                            {
                                NativeMethods.CERT_CONTEXT certContext = (NativeMethods.CERT_CONTEXT)Marshal.PtrToStructure(cryptProviderCert.pCert, typeof(NativeMethods.CERT_CONTEXT));

                                byte[] rawData = new byte[certContext.cbCertEncoded];
                                Marshal.Copy(certContext.pbCertEncoded, rawData, 0, (int)certContext.cbCertEncoded);
                                signerCertificate = new X509Certificate2(rawData);
                            }

                            signatures.Add(
                                new Signature()
                                {
                                    HashAlgorithmOid = signer.HashAlgorithm.pszObjId,
                                    SignerCertificate = signerCertificate,
                                    CounterSignatures = GetCounterSignatures(cryptProviderData, cryptProviderSgnr),
                                }); ;
                        }
                    }
                    finally
                    {
                        trustData.dwStateAction = NativeMethods.WINTRUST_DATA_StateAction.WTD_STATEACTION_CLOSE;
                        Marshal.StructureToPtr(trustData, trustDataPtr, false);
                        NativeMethods.WinVerifyTrust(NativeMethods.INVALID_HANDLE_VALUE, actionIdBytesPtr, trustDataPtr);
                    }
                }
            }
            finally
            {
                if (actionIdBytesPtr != IntPtr.Zero)
                    Marshal.FreeHGlobal(actionIdBytesPtr);

                if (signatureSettingsPtr != IntPtr.Zero)
                    Marshal.FreeHGlobal(signatureSettingsPtr);

                if (fileInfoPtr != IntPtr.Zero)
                    Marshal.FreeHGlobal(fileInfoPtr);

                if (trustDataPtr != IntPtr.Zero)
                    Marshal.FreeHGlobal(trustDataPtr);
            }

            return signatures;
        }

        private static List<CounterSignature> GetCounterSignatures(NativeMethods.CRYPT_PROVIDER_DATA cryptProviderData, NativeMethods.CRYPT_PROVIDER_SGNR cryptProviderSgnr)
        {
            List<CounterSignature> counterSignatures = new List<CounterSignature>();

            var CRYPT_PROVIDER_SGNR_Size = Marshal.SizeOf(typeof(NativeMethods.CRYPT_PROVIDER_SGNR));
            IntPtr CounterSignerPtr = cryptProviderSgnr.pasCounterSigners;

            for (UInt32 idxCounterSigner=0; idxCounterSigner < cryptProviderSgnr.csCounterSigners; idxCounterSigner++)
            {
                NativeMethods.CRYPT_PROVIDER_SGNR counterSigner = (NativeMethods.CRYPT_PROVIDER_SGNR)Marshal.PtrToStructure(CounterSignerPtr, typeof(NativeMethods.CRYPT_PROVIDER_SGNR));
                NativeMethods.CMSG_SIGNER_INFO signer = (NativeMethods.CMSG_SIGNER_INFO)Marshal.PtrToStructure(counterSigner.psSigner, typeof(NativeMethods.CMSG_SIGNER_INFO));

                IntPtr cryptProviderCertPtr = NativeMethods.WTHelperGetProvCertFromChain(CounterSignerPtr, idxCounterSigner);
                if (cryptProviderCertPtr != IntPtr.Zero)
                {
                    NativeMethods.CRYPT_PROVIDER_CERT cryptProviderCert = (NativeMethods.CRYPT_PROVIDER_CERT)Marshal.PtrToStructure(cryptProviderCertPtr, typeof(NativeMethods.CRYPT_PROVIDER_CERT));
                    if (cryptProviderCert.cbStruct != 0)
                    {
                        X509Certificate2 signerCertificate;
                        {
                            NativeMethods.CERT_CONTEXT certContext = (NativeMethods.CERT_CONTEXT)Marshal.PtrToStructure(cryptProviderCert.pCert, typeof(NativeMethods.CERT_CONTEXT));

                            byte[] rawData = new byte[certContext.cbCertEncoded];
                            Marshal.Copy(certContext.pbCertEncoded, rawData, 0, (int)certContext.cbCertEncoded);
                            signerCertificate = new X509Certificate2(rawData);
                        }

                        DateTime? Timestamp = null;
                        if (counterSigner.sftVerifyAsOf.dwHighDateTime != cryptProviderData.sftSystemTime.dwHighDateTime ||
                            counterSigner.sftVerifyAsOf.dwLowDateTime != cryptProviderData.sftSystemTime.dwLowDateTime)
                        {
                            long ft2 = (((long)counterSigner.sftVerifyAsOf.dwHighDateTime) << 32) | ((uint)counterSigner.sftVerifyAsOf.dwLowDateTime);
                            Timestamp = DateTime.FromFileTimeUtc(ft2);
                        }

                        counterSignatures.Add(
                            new CounterSignature()
                            {
                                HashAlgorithmOid = signer.HashAlgorithm.pszObjId,
                                SignerCertificate = signerCertificate,
                                Timestamp = Timestamp,
                            });
                    }
                }

                CounterSignerPtr = IntPtr.Add(CounterSignerPtr, CRYPT_PROVIDER_SGNR_Size);
            }

            return counterSignatures;
        }
    }
}
