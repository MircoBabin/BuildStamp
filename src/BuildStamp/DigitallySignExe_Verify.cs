using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Security.Cryptography.X509Certificates;

namespace BuildStamp
{
    public class DigitallySignExe_Verify
    {
        private static class NativeMethods
        {
            public enum CryptQueryObjectType : UInt32
            {
                CERT_QUERY_OBJECT_FILE = 0x1,
                CERT_QUERY_OBJECT_BLOB = 0x2,
            }

            [Flags]
            public enum CryptQueryContentFlagType : UInt32
            {
                CERT_QUERY_CONTENT_FLAG_CERT = 1u << (int)CryptQueryContentType.CERT_QUERY_CONTENT_CERT,
                CERT_QUERY_CONTENT_FLAG_CTL = 1u << (int)CryptQueryContentType.CERT_QUERY_CONTENT_CTL,
                CERT_QUERY_CONTENT_FLAG_CRL = 1u << (int)CryptQueryContentType.CERT_QUERY_CONTENT_CRL,
                CERT_QUERY_CONTENT_FLAG_SERIALIZED_STORE = 1u << (int)CryptQueryContentType.CERT_QUERY_CONTENT_SERIALIZED_STORE,
                CERT_QUERY_CONTENT_FLAG_SERIALIZED_CERT = 1u << (int)CryptQueryContentType.CERT_QUERY_CONTENT_SERIALIZED_CERT,
                CERT_QUERY_CONTENT_FLAG_SERIALIZED_CTL = 1u << (int)CryptQueryContentType.CERT_QUERY_CONTENT_SERIALIZED_CTL,
                CERT_QUERY_CONTENT_FLAG_SERIALIZED_CRL = 1u << (int)CryptQueryContentType.CERT_QUERY_CONTENT_SERIALIZED_CRL,
                CERT_QUERY_CONTENT_FLAG_PKCS7_SIGNED = 1u << (int)CryptQueryContentType.CERT_QUERY_CONTENT_PKCS7_SIGNED,
                CERT_QUERY_CONTENT_FLAG_PKCS7_UNSIGNED = 1u << (int)CryptQueryContentType.CERT_QUERY_CONTENT_PKCS7_UNSIGNED,
                CERT_QUERY_CONTENT_FLAG_PKCS7_SIGNED_EMBED = 1u << (int)CryptQueryContentType.CERT_QUERY_CONTENT_PKCS7_SIGNED_EMBED,
                CERT_QUERY_CONTENT_FLAG_PKCS10 = 1u << (int)CryptQueryContentType.CERT_QUERY_CONTENT_PKCS10,
                CERT_QUERY_CONTENT_FLAG_PFX = 1u << (int)CryptQueryContentType.CERT_QUERY_CONTENT_PFX,
                CERT_QUERY_CONTENT_FLAG_CERT_PAIR = 1u << (int)CryptQueryContentType.CERT_QUERY_CONTENT_CERT_PAIR,
                CERT_QUERY_CONTENT_FLAG_PFX_AND_LOAD = 1u << (int)CryptQueryContentType.CERT_QUERY_CONTENT_PFX_AND_LOAD,
                CERT_QUERY_CONTENT_FLAG_ALL =
                    CERT_QUERY_CONTENT_FLAG_CERT |
                    CERT_QUERY_CONTENT_FLAG_CTL |
                    CERT_QUERY_CONTENT_FLAG_CRL |
                    CERT_QUERY_CONTENT_FLAG_SERIALIZED_STORE |
                    CERT_QUERY_CONTENT_FLAG_SERIALIZED_CERT |
                    CERT_QUERY_CONTENT_FLAG_SERIALIZED_CTL |
                    CERT_QUERY_CONTENT_FLAG_SERIALIZED_CRL |
                    CERT_QUERY_CONTENT_FLAG_PKCS7_SIGNED |
                    CERT_QUERY_CONTENT_FLAG_PKCS7_UNSIGNED |
                    CERT_QUERY_CONTENT_FLAG_PKCS7_SIGNED_EMBED |
                    CERT_QUERY_CONTENT_FLAG_PKCS10 |
                    CERT_QUERY_CONTENT_FLAG_PFX |
                    CERT_QUERY_CONTENT_FLAG_CERT_PAIR, //wincrypt.h purposefully omits CERT_QUERY_CONTENT_FLAG_PFX_AND_LOAD
                CERT_QUERY_CONTENT_FLAG_ALL_ISSUER_CERT =
                    CERT_QUERY_CONTENT_FLAG_CERT |
                    CERT_QUERY_CONTENT_FLAG_SERIALIZED_STORE |
                    CERT_QUERY_CONTENT_FLAG_SERIALIZED_CERT |
                    CERT_QUERY_CONTENT_FLAG_PKCS7_SIGNED |
                    CERT_QUERY_CONTENT_FLAG_PKCS7_UNSIGNED
            }

            public enum CryptQueryContentType : UInt32
            {
                CERT_QUERY_CONTENT_CERT = 1,
                CERT_QUERY_CONTENT_CTL = 2,
                CERT_QUERY_CONTENT_CRL = 3,
                CERT_QUERY_CONTENT_SERIALIZED_STORE = 4,
                CERT_QUERY_CONTENT_SERIALIZED_CERT = 5,
                CERT_QUERY_CONTENT_SERIALIZED_CTL = 6,
                CERT_QUERY_CONTENT_SERIALIZED_CRL = 7,
                CERT_QUERY_CONTENT_PKCS7_SIGNED = 8,
                CERT_QUERY_CONTENT_PKCS7_UNSIGNED = 9,
                CERT_QUERY_CONTENT_PKCS7_SIGNED_EMBED = 10,
                CERT_QUERY_CONTENT_PKCS10 = 11,
                CERT_QUERY_CONTENT_PFX = 12,
                CERT_QUERY_CONTENT_CERT_PAIR = 13,
                CERT_QUERY_CONTENT_PFX_AND_LOAD = 14
            }

            public enum CryptQueryFormatType : UInt32
            {
                CERT_QUERY_FORMAT_BINARY = 1,
                CERT_QUERY_FORMAT_BASE64_ENCODED = 2,
                CERT_QUERY_FORMAT_ASN_ASCII_HEX_ENCODED = 3
            }

            [Flags]
            public enum CryptQueryFormatFlagType : UInt32
            {
                CERT_QUERY_FORMAT_FLAG_BINARY = 1u << (int)CryptQueryFormatType.CERT_QUERY_FORMAT_BINARY,
                CERT_QUERY_FORMAT_FLAG_BASE64_ENCODED = 1u << (int)CryptQueryFormatType.CERT_QUERY_FORMAT_BASE64_ENCODED,
                CERT_QUERY_FORMAT_FLAG_ASN_ASCII_HEX_ENCODED = 1u << (int)CryptQueryFormatType.CERT_QUERY_FORMAT_ASN_ASCII_HEX_ENCODED,
                CERT_QUERY_FORMAT_FLAG_ALL =
                    CERT_QUERY_FORMAT_FLAG_BINARY |
                    CERT_QUERY_FORMAT_FLAG_BASE64_ENCODED |
                    CERT_QUERY_FORMAT_FLAG_ASN_ASCII_HEX_ENCODED
            }

            [Flags]
            public enum CryptQueryObjectFlags : UInt32
            {
                NONE = 0
            }

            [Flags]
            public enum EncodingType : UInt32
            {
                PKCS_7_ASN_ENCODING = 0x10000,
                X509_ASN_ENCODING = 0x1
            }

            [Flags]
            public enum CryptDecodeFlags : UInt32
            {
                CRYPT_DECODE_ALLOC_FLAG = 0x8000
            }

            public enum CryptMsgParamType : UInt32
            {
                CMSG_TYPE_PARAM = 1,
                CMSG_CONTENT_PARAM = 2,
                CMSG_BARE_CONTENT_PARAM = 3,
                CMSG_INNER_CONTENT_TYPE_PARAM = 4,
                CMSG_SIGNER_COUNT_PARAM = 5,
                CMSG_SIGNER_INFO_PARAM = 6,
                CMSG_SIGNER_CERT_INFO_PARAM = 7,
                CMSG_SIGNER_HASH_ALGORITHM_PARAM = 8,
                CMSG_SIGNER_AUTH_ATTR_PARAM = 9,
                CMSG_SIGNER_UNAUTH_ATTR_PARAM = 10,
                CMSG_CERT_COUNT_PARAM = 11,
                CMSG_CERT_PARAM = 12,
                CMSG_CRL_COUNT_PARAM = 13,
                CMSG_CRL_PARAM = 14,
                CMSG_ENVELOPE_ALGORITHM_PARAM = 15,
                CMSG_RECIPIENT_COUNT_PARAM = 17,
                CMSG_RECIPIENT_INDEX_PARAM = 18,
                CMSG_RECIPIENT_INFO_PARAM = 19,
                CMSG_HASH_ALGORITHM_PARAM = 20,
                CMSG_HASH_DATA_PARAM = 21,
                CMSG_COMPUTED_HASH_PARAM = 22,
                CMSG_ENCRYPT_PARAM = 26,
                CMSG_ENCRYPTED_DIGEST = 27,
                CMSG_ENCODED_SIGNER = 28,
                CMSG_ENCODED_MESSAGE = 29,
                CMSG_VERSION_PARAM = 30,
                CMSG_ATTR_CERT_COUNT_PARAM = 31,
                CMSG_ATTR_CERT_PARAM = 32,
                CMSG_CMS_RECIPIENT_COUNT_PARAM = 33,
                CMSG_CMS_RECIPIENT_INDEX_PARAM = 34,
                CMSG_CMS_RECIPIENT_ENCRYPTED_KEY_INDEX_PARAM = 35,
                CMSG_CMS_RECIPIENT_INFO_PARAM = 36,
                CMSG_UNPROTECTED_ATTR_PARAM = 37,
                CMSG_SIGNER_CERT_ID_PARAM = 38,
                CMSG_CMS_SIGNER_INFO_PARAM = 39,
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

            // cert info flags.
            public const uint CERT_INFO_VERSION_FLAG = 1;
            public const uint CERT_INFO_SERIAL_NUMBER_FLAG = 2;
            public const uint CERT_INFO_SIGNATURE_ALGORITHM_FLAG = 3;
            public const uint CERT_INFO_ISSUER_FLAG = 4;
            public const uint CERT_INFO_NOT_BEFORE_FLAG = 5;
            public const uint CERT_INFO_NOT_AFTER_FLAG = 6;
            public const uint CERT_INFO_SUBJECT_FLAG = 7;
            public const uint CERT_INFO_SUBJECT_PUBLIC_KEY_INFO_FLAG = 8;
            public const uint CERT_INFO_ISSUER_UNIQUE_ID_FLAG = 9;
            public const uint CERT_INFO_SUBJECT_UNIQUE_ID_FLAG = 10;
            public const uint CERT_INFO_EXTENSION_FLAG = 11;

            // cert compare flags.
            public const uint CERT_COMPARE_MASK = 0xFFFF;
            public const uint CERT_COMPARE_SHIFT = 16;
            public const uint CERT_COMPARE_ANY = 0;
            public const uint CERT_COMPARE_SHA1_HASH = 1;
            public const uint CERT_COMPARE_NAME = 2;
            public const uint CERT_COMPARE_ATTR = 3;
            public const uint CERT_COMPARE_MD5_HASH = 4;
            public const uint CERT_COMPARE_PROPERTY = 5;
            public const uint CERT_COMPARE_PUBLIC_KEY = 6;
            public const uint CERT_COMPARE_HASH = CERT_COMPARE_SHA1_HASH;
            public const uint CERT_COMPARE_NAME_STR_A = 7;
            public const uint CERT_COMPARE_NAME_STR_W = 8;
            public const uint CERT_COMPARE_KEY_SPEC = 9;
            public const uint CERT_COMPARE_ENHKEY_USAGE = 10;
            public const uint CERT_COMPARE_CTL_USAGE = CERT_COMPARE_ENHKEY_USAGE;
            public const uint CERT_COMPARE_SUBJECT_CERT = 11;
            public const uint CERT_COMPARE_ISSUER_OF = 12;
            public const uint CERT_COMPARE_EXISTING = 13;
            public const uint CERT_COMPARE_SIGNATURE_HASH = 14;
            public const uint CERT_COMPARE_KEY_IDENTIFIER = 15;
            public const uint CERT_COMPARE_CERT_ID = 16;
            public const uint CERT_COMPARE_CROSS_CERT_DIST_POINTS = 17;
            public const uint CERT_COMPARE_PUBKEY_MD5_HASH = 18;

            // cert find flags.
            public const uint CERT_FIND_ANY = ((int)CERT_COMPARE_ANY << (int)CERT_COMPARE_SHIFT);
            public const uint CERT_FIND_SHA1_HASH = ((int)CERT_COMPARE_SHA1_HASH << (int)CERT_COMPARE_SHIFT);
            public const uint CERT_FIND_MD5_HASH = ((int)CERT_COMPARE_MD5_HASH << (int)CERT_COMPARE_SHIFT);
            public const uint CERT_FIND_SIGNATURE_HASH = ((int)CERT_COMPARE_SIGNATURE_HASH << (int)CERT_COMPARE_SHIFT);
            public const uint CERT_FIND_KEY_IDENTIFIER = ((int)CERT_COMPARE_KEY_IDENTIFIER << (int)CERT_COMPARE_SHIFT);
            public const uint CERT_FIND_HASH = CERT_FIND_SHA1_HASH;
            public const uint CERT_FIND_PROPERTY = ((int)CERT_COMPARE_PROPERTY << (int)CERT_COMPARE_SHIFT);
            public const uint CERT_FIND_PUBLIC_KEY = ((int)CERT_COMPARE_PUBLIC_KEY << (int)CERT_COMPARE_SHIFT);
            public const uint CERT_FIND_SUBJECT_NAME = ((int)CERT_COMPARE_NAME << (int)CERT_COMPARE_SHIFT | (int)CERT_INFO_SUBJECT_FLAG);
            public const uint CERT_FIND_SUBJECT_ATTR = ((int)CERT_COMPARE_ATTR << (int)CERT_COMPARE_SHIFT | (int)CERT_INFO_SUBJECT_FLAG);
            public const uint CERT_FIND_ISSUER_NAME = ((int)CERT_COMPARE_NAME << (int)CERT_COMPARE_SHIFT | (int)CERT_INFO_ISSUER_FLAG);
            public const uint CERT_FIND_ISSUER_ATTR = ((int)CERT_COMPARE_ATTR << (int)CERT_COMPARE_SHIFT | (int)CERT_INFO_ISSUER_FLAG);
            public const uint CERT_FIND_SUBJECT_STR_A = ((int)CERT_COMPARE_NAME_STR_A << (int)CERT_COMPARE_SHIFT | (int)CERT_INFO_SUBJECT_FLAG);
            public const uint CERT_FIND_SUBJECT_STR_W = ((int)CERT_COMPARE_NAME_STR_W << (int)CERT_COMPARE_SHIFT | (int)CERT_INFO_SUBJECT_FLAG);
            public const uint CERT_FIND_SUBJECT_STR = CERT_FIND_SUBJECT_STR_W;
            public const uint CERT_FIND_ISSUER_STR_A = ((int)CERT_COMPARE_NAME_STR_A << (int)CERT_COMPARE_SHIFT | (int)CERT_INFO_ISSUER_FLAG);
            public const uint CERT_FIND_ISSUER_STR_W = ((int)CERT_COMPARE_NAME_STR_W << (int)CERT_COMPARE_SHIFT | (int)CERT_INFO_ISSUER_FLAG);
            public const uint CERT_FIND_ISSUER_STR = CERT_FIND_ISSUER_STR_W;
            public const uint CERT_FIND_KEY_SPEC = ((int)CERT_COMPARE_KEY_SPEC << (int)CERT_COMPARE_SHIFT);
            public const uint CERT_FIND_ENHKEY_USAGE = ((int)CERT_COMPARE_ENHKEY_USAGE << (int)CERT_COMPARE_SHIFT);
            public const uint CERT_FIND_CTL_USAGE = CERT_FIND_ENHKEY_USAGE;
            public const uint CERT_FIND_SUBJECT_CERT = ((int)CERT_COMPARE_SUBJECT_CERT << (int)CERT_COMPARE_SHIFT);
            public const uint CERT_FIND_ISSUER_OF = ((int)CERT_COMPARE_ISSUER_OF << (int)CERT_COMPARE_SHIFT);
            public const uint CERT_FIND_EXISTING = ((int)CERT_COMPARE_EXISTING << (int)CERT_COMPARE_SHIFT);
            public const uint CERT_FIND_CERT_ID = ((int)CERT_COMPARE_CERT_ID << (int)CERT_COMPARE_SHIFT);
            public const uint CERT_FIND_CROSS_CERT_DIST_POINTS = ((int)CERT_COMPARE_CROSS_CERT_DIST_POINTS << (int)CERT_COMPARE_SHIFT);
            public const uint CERT_FIND_PUBKEY_MD5_HASH = ((int)CERT_COMPARE_PUBKEY_MD5_HASH << (int)CERT_COMPARE_SHIFT);

            [StructLayout(LayoutKind.Sequential)]
            public struct CERT_INFO
            {
                public UInt32 dwVersion;
                public CRYPTOAPI_BLOB SerialNumber;
                public CRYPT_ALGORITHM_IDENTIFIER SignatureAlgorithm;
                public CRYPTOAPI_BLOB Issuer;
                public FILETIME NotBefore;
                public FILETIME NotAfter;
                public CRYPTOAPI_BLOB Subject;
                public CERT_PUBLIC_KEY_INFO SubjectPublicKeyInfo;
                public CRYPT_BIT_BLOB IssuerUniqueId;
                public CRYPT_BIT_BLOB SubjectUniqueId;
                public UInt32 cExtension;
                public IntPtr rgExtension;
            }

            [StructLayout(LayoutKind.Sequential)]
            public struct FILETIME
            {
                public UInt32 DateTimeLow;
                public UInt32 DateTimeHigh;
            }

            [StructLayout(LayoutKind.Sequential)]
            public struct CERT_PUBLIC_KEY_INFO
            {
                public CRYPT_ALGORITHM_IDENTIFIER Algorithm;
                public CRYPT_BIT_BLOB PublicKey;
            }

            [StructLayout(LayoutKind.Sequential)]
            public struct CERT_CONTEXT
            {
                public Int32 dwCertEncodingType;
                public IntPtr pbCertEncoded;
                public Int32 cbCertEncoded;
                public IntPtr pCertInfo;
                public IntPtr hCertStore;
            }

            public static byte[] GetBytesFromBlob(CRYPTOAPI_BLOB blob)
            {
                byte[] result = new byte[blob.cbData];

                Marshal.PtrToStructure(blob.pbData, result);

                return result;
            }

            [method: DllImport("crypt32.dll", CallingConvention = CallingConvention.StdCall, EntryPoint = "CryptQueryObject", SetLastError = true)]
            [return: MarshalAs(UnmanagedType.Bool)]
            public static extern bool CryptQueryObject(
                CryptQueryObjectType dwObjectType,
                [MarshalAs(UnmanagedType.LPWStr)] string pvObject,
                CryptQueryContentFlagType dwExpectedContentTypeFlags,
                CryptQueryFormatFlagType dwExpectedFormatTypeFlags,
                CryptQueryObjectFlags dwFlags,
                out EncodingType pdwMsgAndCertEncodingType,
                out CryptQueryContentType pdwContentType,
                out CryptQueryFormatType pdwFormatType,
                out IntPtr phCertStore,
                out IntPtr phMsg,
                IntPtr ppvContext);

            [method: DllImport("crypt32.dll", CallingConvention = CallingConvention.StdCall, EntryPoint = "CryptMsgClose", SetLastError = true)]
            [return: MarshalAs(UnmanagedType.Bool)]
            public static extern bool CryptMsgClose(IntPtr hCryptMsg);

            [method: DllImport("crypt32.dll", CallingConvention = CallingConvention.StdCall, EntryPoint = "CryptMsgGetParam", SetLastError = true)]
            [return: MarshalAs(UnmanagedType.Bool)]
            public static extern bool CryptMsgGetParam(
                IntPtr hCryptMsg,
                CryptMsgParamType dwParamType,
                UInt32 dwIndex,
                IntPtr pvData,
                ref UInt32 pcbData);

            [method: DllImport("crypt32.dll", CallingConvention = CallingConvention.StdCall, EntryPoint = "CertFindCertificateInStore", SetLastError = true)]
            public static extern IntPtr CertFindCertificateInStore(
                IntPtr hCertStore,
                EncodingType dwCertEncodingType,
                UInt32 dwFindFlags,
                UInt32 dwFindType,
                IntPtr pvFindPara,
                IntPtr pPrevCertContext);

            [method: DllImport("crypt32.dll", CallingConvention = CallingConvention.StdCall, EntryPoint = "CertFreeCertificateContext")]
            [return: MarshalAs(UnmanagedType.Bool)]
            public static extern bool CertFreeCertificateContext(IntPtr pCertContext);

            [method: DllImport("crypt32.dll", CallingConvention = CallingConvention.StdCall, EntryPoint = "CertCloseStore", SetLastError = true)]
            [return: MarshalAs(UnmanagedType.Bool)]
            public static extern bool CertCloseStore(
                IntPtr hCertStore,
                UInt32 dwFlags);
        }


        public static void RetrieveSignatures(string exeFilename)
        {
            // Todo: use WinVerifyTrust
            // https://stackoverflow.com/questions/24892531/reading-multiple-signatures-from-executable-file
            // https://stackoverflow.com/questions/21547311/how-to-dual-sign-a-binary-with-authenticode

            IntPtr hMsg = IntPtr.Zero;
            IntPtr hCertStore = IntPtr.Zero;
            try
            {
                NativeMethods.EncodingType MsgAndCertEncodingType;
                NativeMethods.CryptQueryContentType ContentType;
                NativeMethods.CryptQueryFormatType FormatType;
                if (!NativeMethods.CryptQueryObject(
                    NativeMethods.CryptQueryObjectType.CERT_QUERY_OBJECT_FILE,
                    exeFilename,
                    NativeMethods.CryptQueryContentFlagType.CERT_QUERY_CONTENT_FLAG_PKCS7_SIGNED | NativeMethods.CryptQueryContentFlagType.CERT_QUERY_CONTENT_FLAG_PKCS7_UNSIGNED | NativeMethods.CryptQueryContentFlagType.CERT_QUERY_CONTENT_FLAG_PKCS7_SIGNED_EMBED,
                    NativeMethods.CryptQueryFormatFlagType.CERT_QUERY_FORMAT_FLAG_BINARY,
                    NativeMethods.CryptQueryObjectFlags.NONE,
                    out MsgAndCertEncodingType,
                    out ContentType,
                    out FormatType,
                    out hCertStore,
                    out hMsg,
                    IntPtr.Zero))
                {
                    throw new Win32Exception(Marshal.GetLastWin32Error());
                }

                // GetSignatures(hCertStore, hMsg);
            }
            finally
            {
                NativeMethods.CryptMsgClose(hMsg);
                NativeMethods.CertCloseStore(hCertStore, 0);
            }
        }

        /*
         * Does not work. When signed with both SHA-1 and SHA-256 only the SHA-1 signature is returned.
         *
        public class Signature
        {
            public string HashAlgorithmOid { get; set; }
            public string HashEncryptionAlgorithmOid { get; set; }
            public X509Certificate2 SignerCertificate { get; set; }
        }

        private static List<Signature> GetSignatures(IntPtr hCertStore, IntPtr hMsg)
        {
            // https://learn.microsoft.com/en-us/troubleshoot/windows/win32/get-information-authenticode-signed-executables

            UInt32 signerCount = 0;
            UInt32 signerCountSize = (UInt32)Marshal.SizeOf(signerCount);
            IntPtr signerCountPtr;
            signerCountPtr = Marshal.AllocHGlobal((int)signerCountSize);
            Marshal.StructureToPtr(signerCount, signerCountPtr, false);
            try
            {
                if (!NativeMethods.CryptMsgGetParam(
                    hMsg,
                    NativeMethods.CryptMsgParamType.CMSG_SIGNER_COUNT_PARAM,
                    0,
                    signerCountPtr,
                    ref signerCountSize))
                {
                    throw new Win32Exception(Marshal.GetLastWin32Error());
                }
                signerCount = (UInt32)Marshal.PtrToStructure(signerCountPtr, typeof(UInt32));
                if (signerCount == 0)
                {
                    return new List<Signature>();
                }

                var result = new List<Signature>();
                for (UInt32 i = 0; i < signerCount; i++)
                {
                    UInt32 signerSize = 0;
                    IntPtr signerPtr;
                    if (!NativeMethods.CryptMsgGetParam(
                        hMsg,
                        NativeMethods.CryptMsgParamType.CMSG_SIGNER_INFO_PARAM,
                        i,
                        IntPtr.Zero,
                        ref signerSize))
                    {
                        continue;
                    }

                    signerPtr = Marshal.AllocHGlobal((int)signerSize);
                    try
                    {
                        if (!NativeMethods.CryptMsgGetParam(
                            hMsg,
                            NativeMethods.CryptMsgParamType.CMSG_SIGNER_INFO_PARAM,
                            i,
                            signerPtr,
                            ref signerSize))
                        {
                            continue;
                        }

                        NativeMethods.CMSG_SIGNER_INFO signerInfo = (NativeMethods.CMSG_SIGNER_INFO)Marshal.PtrToStructure(signerPtr, typeof(NativeMethods.CMSG_SIGNER_INFO));
                        X509Certificate2 signerCertificate = null;

                        //
                        // Search for the signer certificate in the temporary certificate store.
                        //
                        {
                            NativeMethods.CERT_INFO CertInfo = new NativeMethods.CERT_INFO()
                            {
                                dwVersion = 0,
                                SerialNumber = signerInfo.SerialNumber,
                                SignatureAlgorithm = new NativeMethods.CRYPT_ALGORITHM_IDENTIFIER()
                                {
                                    pszObjId = "",
                                    Parameters = new NativeMethods.CRYPTOAPI_BLOB()
                                    {
                                        cbData = 0,
                                        pbData = IntPtr.Zero,
                                    }
                                },
                                Issuer = signerInfo.Issuer,
                                NotBefore = new NativeMethods.FILETIME()
                                {
                                    DateTimeHigh = 0,
                                    DateTimeLow = 0,
                                },
                                NotAfter = new NativeMethods.FILETIME()
                                {
                                    DateTimeHigh = 0,
                                    DateTimeLow = 0,
                                },
                                Subject = new NativeMethods.CRYPTOAPI_BLOB()
                                {
                                    cbData = 0,
                                    pbData = IntPtr.Zero,
                                },
                                SubjectPublicKeyInfo = new NativeMethods.CERT_PUBLIC_KEY_INFO()
                                {
                                    Algorithm = new NativeMethods.CRYPT_ALGORITHM_IDENTIFIER()
                                    {
                                        pszObjId = "",
                                        Parameters = new NativeMethods.CRYPTOAPI_BLOB()
                                        {
                                            cbData = 0,
                                            pbData = IntPtr.Zero,
                                        }
                                    },
                                    PublicKey = new NativeMethods.CRYPT_BIT_BLOB()
                                    {
                                        cbData = 0,
                                        pbData = IntPtr.Zero,
                                        cUnusedBits = 0,
                                    },
                                },
                                IssuerUniqueId = new NativeMethods.CRYPT_BIT_BLOB()
                                {
                                    cbData = 0,
                                    pbData = IntPtr.Zero,
                                    cUnusedBits = 0,
                                },
                                SubjectUniqueId = new NativeMethods.CRYPT_BIT_BLOB()
                                {
                                    cbData = 0,
                                    pbData = IntPtr.Zero,
                                    cUnusedBits = 0,
                                },
                                cExtension = 0,
                                rgExtension = IntPtr.Zero,
                            };

                            IntPtr CertInfoPtr = IntPtr.Zero;
                            IntPtr CertContextPtr = IntPtr.Zero;
                            try
                            {
                                CertInfoPtr = Marshal.AllocHGlobal(Marshal.SizeOf(CertInfo));
                                Marshal.StructureToPtr(CertInfo, CertInfoPtr, false);

                                CertContextPtr = NativeMethods.CertFindCertificateInStore(
                                    hCertStore,
                                    NativeMethods.EncodingType.X509_ASN_ENCODING | NativeMethods.EncodingType.PKCS_7_ASN_ENCODING,
                                    0,
                                    NativeMethods.CERT_FIND_SUBJECT_CERT, // uses CERT_INFO.Issuer and CERT_INFO.SerialNumber
                                    CertInfoPtr,
                                    IntPtr.Zero);
                                if (CertContextPtr == IntPtr.Zero)
                                    throw new Win32Exception(Marshal.GetLastWin32Error());

                                {
                                    NativeMethods.CERT_CONTEXT CertContext = (NativeMethods.CERT_CONTEXT)Marshal.PtrToStructure(CertContextPtr, typeof(NativeMethods.CERT_CONTEXT));
                                    byte[] rawData = new byte[CertContext.cbCertEncoded];
                                    Marshal.Copy(CertContext.pbCertEncoded, rawData, 0, CertContext.cbCertEncoded);

                                    signerCertificate = new X509Certificate2(rawData);
                                }
                            }
                            finally
                            {
                                if (CertContextPtr != IntPtr.Zero)
                                    NativeMethods.CertFreeCertificateContext(CertContextPtr);

                                if (CertInfoPtr != IntPtr.Zero)
                                {
                                    Marshal.FreeHGlobal(CertInfoPtr);
                                    CertInfoPtr = IntPtr.Zero;
                                }
                            }
                        }

                        result.Add(new Signature()
                        {
                            HashAlgorithmOid = signerInfo.HashAlgorithm.pszObjId,
                            HashEncryptionAlgorithmOid = signerInfo.HashEncryptionAlgorithm.pszObjId,
                            SignerCertificate = signerCertificate,
                        });
                    }
                    finally
                    {
                        Marshal.FreeHGlobal(signerPtr);
                    }
                }

                return result;
            }
            finally
            {
                Marshal.FreeHGlobal(signerCountPtr);
            }
        }
        */


        /*
         * Does not work. When signed with both SHA-1 and SHA-256 only the SHA-1 signature is returned.
         *
         * Add reference to: system.security.dll
         * using System.Security.Cryptography.Pkcs;
         *
        private static SignerInfoCollection GetSignatures(IntPtr hCertStore, IntPtr hMsg)
        {
            UInt32 encodedMessageSize = 0;
            IntPtr encodedMessagePtr = IntPtr.Zero;
            try
            {
                if (!NativeMethods.CryptMsgGetParam(
                    hMsg,
                    NativeMethods.CryptMsgParamType.CMSG_ENCODED_MESSAGE,
                    0,
                    IntPtr.Zero,
                    ref encodedMessageSize))
                {
                    throw new Win32Exception(Marshal.GetLastWin32Error());
                }

                encodedMessagePtr = Marshal.AllocHGlobal((int)encodedMessageSize);

                if (!NativeMethods.CryptMsgGetParam(
                    hMsg,
                    NativeMethods.CryptMsgParamType.CMSG_ENCODED_MESSAGE,
                    0,
                    encodedMessagePtr,
                    ref encodedMessageSize))
                {
                    throw new Win32Exception(Marshal.GetLastWin32Error());
                }

                byte[] encodedMessageBytes = new byte[encodedMessageSize];
                Marshal.Copy(encodedMessagePtr, encodedMessageBytes, 0, (int)encodedMessageSize);

                var signedCms = new SignedCms();
                signedCms.Decode(encodedMessageBytes);

                return signedCms.SignerInfos;
            }
            finally
            {
                if (encodedMessagePtr != IntPtr.Zero)
                    Marshal.FreeHGlobal(encodedMessagePtr);
            }
        }
        */
    }
}
