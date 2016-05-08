using System;
using System.Collections.Generic;
using System.Globalization;

namespace TpmRcDecoder
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public class Decoder
    {
        public Decoder()
        { }

        public string Decode(string inputText)
        {
            string inStr = inputText;
            if (inStr.Trim().StartsWith("0x", StringComparison.CurrentCultureIgnoreCase))
            {
                inStr = inStr.Trim().Substring(2);
            }
            UInt32 input = 0;
            try
            {
                input = UInt32.Parse(inStr, NumberStyles.HexNumber);
            }
            catch (FormatException)
            {
                return "Input format error.";
            }

            // Windows error codes
            string output = "";
            if ((((input & STATUS_SEVERITY_MASK) == STATUS_SEVERITY_ERROR) ||
                 ((input & STATUS_SEVERITY_MASK) == STATUS_SEVERITY_WARNING)) &&
                (((input & STATUS_FACILITY_MASK) == FACILITY_TPM_SERVICE) ||
                 ((input & STATUS_FACILITY_MASK) == FACILITY_TPM_SOFTWARE)))
            {
                output = DecodeWindowsError(input);
            }
            else if ((input & TPM_RC_FMT) == TPM_RC_FMT)
            {
                output += DecodeFormatError(input);
            }
            else if ((input & TPM_RC_VENDOR_ERROR) == TPM_RC_VENDOR_ERROR)
            {
                output += String.Format("Vendor specific error: {0:x}", input & TPM_RC_ERROR_MASK);
            }
            else if ((input & TPM_RC_RSVD) == TPM_RC_RSVD)
            {
                output += "Bit 9 of return code not 0.\n";
                output += String.Format("Cleaned return code is {0:x}.\n", input ^ TPM_RC_RSVD);
            }
            else if ((input & TPM_RC_WARN) == TPM_RC_WARN)
            {
                output += DecodeWarning(input);
            }
            else
            {
                output += DecodeError(input);
            }

            return output;
        }

        /// <summary>
        /// TPM commando codes as defined in TPM 2.0 specification version 0.91.
        /// </summary>
        enum TPM_CC
        {
            TPM20_CC_NV_UndefineSpaceSpecial = 0x0000011f,
            TPM20_CC_EvictControl = 0x00000120,
            TPM20_CC_HierarchyControl = 0x00000121,
            TPM20_CC_NV_UndefineSpace = 0x00000122,
            TPM20_CC_ChangeEPS = 0x00000124,
            TPM20_CC_ChangePPS = 0x00000125,
            TPM20_CC_Clear = 0x00000126,
            TPM20_CC_ClearControl = 0x00000127,
            TPM20_CC_ClockSet = 0x00000128,
            TPM20_CC_HierarchyChangeAuth = 0x00000129,
            TPM20_CC_NV_DefineSpace = 0x0000012a,
            TPM20_CC_PCR_Allocate = 0x0000012b,
            TPM20_CC_PCR_SetAuthPolicy = 0x0000012c,
            TPM20_CC_PP_Commands = 0x0000012d,
            TPM20_CC_SetPrimaryPolicy = 0x0000012e,
            TPM20_CC_FieldUpgradeStart = 0x0000012f,
            TPM20_CC_ClockRateAdjust = 0x00000130,
            TPM20_CC_CreatePrimary = 0x00000131,
            TPM20_CC_NV_GlobalWriteLock = 0x00000132,
            TPM20_CC_GetCommandAuditDigest = 0x00000133,
            TPM20_CC_NV_Increment = 0x00000134,
            TPM20_CC_NV_SetBits = 0x00000135,
            TPM20_CC_NV_Extend = 0x00000136,
            TPM20_CC_NV_Write = 0x00000137,
            TPM20_CC_NV_WriteLock = 0x00000138,
            TPM20_CC_DictionaryAttackLockReset = 0x00000139,
            TPM20_CC_DictionaryAttackParameters = 0x0000013a,
            TPM20_CC_NV_ChangeAuth = 0x0000013b,
            TPM20_CC_PCR_Event = 0x0000013c,
            TPM20_CC_PCR_Reset = 0x0000013d,
            TPM20_CC_SequenceComplete = 0x0000013e,
            TPM20_CC_SetAlgorithmSet = 0x0000013f,
            TPM20_CC_SetCommandCodeAuditStatus = 0x00000140,
            TPM20_CC_FieldUpgradeData = 0x00000141,
            TPM20_CC_IncrementalSelfTest = 0x00000142,
            TPM20_CC_SelfTest = 0x00000143,
            TPM20_CC_Startup = 0x00000144,
            TPM20_CC_Shutdown = 0x00000145,
            TPM20_CC_StirRandom = 0x00000146,
            TPM20_CC_ActivateCredential = 0x00000147,
            TPM20_CC_Certify = 0x00000148,
            TPM20_CC_PolicyNV = 0x00000149,
            TPM20_CC_CertifyCreation = 0x0000014a,
            TPM20_CC_Duplicate = 0x0000014b,
            TPM20_CC_GetTime = 0x0000014c,
            TPM20_CC_GetSessionAuditDigest = 0x0000014d,
            TPM20_CC_NV_Read = 0x0000014e,
            TPM20_CC_NV_ReadLock = 0x0000014f,
            TPM20_CC_ObjectChangeAuth = 0x00000150,
            TPM20_CC_PolicySecret = 0x00000151,
            TPM20_CC_Rewrap = 0x00000152,
            TPM20_CC_Create = 0x00000153,
            TPM20_CC_ECDH_ZGen = 0x00000154,
            TPM20_CC_HMAC = 0x00000155,
            TPM20_CC_Import = 0x00000156,
            TPM20_CC_Load = 0x00000157,
            TPM20_CC_Quote = 0x00000158,
            TPM20_CC_RSA_Decrypt = 0x00000159,
            TPM20_CC_HMAC_Start = 0x0000015b,
            TPM20_CC_SequenceUpdate = 0x0000015c,
            TPM20_CC_Sign = 0x0000015d,
            TPM20_CC_Unseal = 0x0000015e,
            TPM20_CC_PolicySigned = 0x00000160,
            TPM20_CC_ContextLoad = 0x00000161,
            TPM20_CC_ContextSave = 0x00000162,
            TPM20_CC_ECDH_KeyGen = 0x00000163,
            TPM20_CC_EncryptDecrypt = 0x00000164,
            TPM20_CC_FlushContext = 0x00000165,
            TPM20_CC_LoadExternal = 0x00000167,
            TPM20_CC_MakeCredential = 0x00000168,
            TPM20_CC_NV_ReadPublic = 0x00000169,
            TPM20_CC_PolicyAuthorize = 0x0000016a,
            TPM20_CC_PolicyAuthValue = 0x0000016b,
            TPM20_CC_PolicyCommandCode = 0x0000016c,
            TPM20_CC_PolicyCounterTimer = 0x0000016d,
            TPM20_CC_PolicyCpHash = 0x0000016e,
            TPM20_CC_PolicyLocality = 0x0000016f,
            TPM20_CC_PolicyNameHash = 0x00000170,
            TPM20_CC_PolicyOR = 0x00000171,
            TPM20_CC_PolicyTicket = 0x00000172,
            TPM20_CC_ReadPublic = 0x00000173,
            TPM20_CC_RSA_Encrypt = 0x00000174,
            TPM20_CC_StartAuthSession = 0x00000176,
            TPM20_CC_VerifySignature = 0x00000177,
            TPM20_CC_ECC_Parameters = 0x00000178,
            TPM20_CC_FirmwareRead = 0x00000179,
            TPM20_CC_GetCapability = 0x0000017a,
            TPM20_CC_GetRandom = 0x0000017b,
            TPM20_CC_GetTestResult = 0x0000017c,
            TPM20_CC_Hash = 0x0000017d,
            TPM20_CC_PCR_Read = 0x0000017e,
            TPM20_CC_PolicyPCR = 0x0000017f,
            TPM20_CC_PolicyRestart = 0x00000180,
            TPM20_CC_ReadClock = 0x00000181,
            TPM20_CC_PCR_Extend = 0x00000182,
            TPM20_CC_PCR_SetAuthValue = 0x00000183,
            TPM20_CC_NV_Certify = 0x00000184,
            TPM20_CC_EventSequenceComplete = 0x00000185,
            TPM20_CC_HashSequenceStart = 0x00000186,
            TPM20_CC_PolicyPhysicalPresence = 0x00000187,
            TPM20_CC_PolicyDuplicationSelect = 0x00000188,
            TPM20_CC_PolicyGetDigest = 0x00000189,
            TPM20_CC_TestParms = 0x0000018a,
            TPM20_CC_Commit = 0x0000018b,
            TPM20_CC_PolicyPassword = 0x0000018c,
        };

        private const UInt32 TPM_RC_WARN = 0x800;
        private const UInt32 TPM_RC_VENDOR_ERROR = 0x400;
        private const UInt32 TPM_RC_VER1 = 0x100;
        private const UInt32 TPM_RC_FMT = 0x80;

        private const UInt32 TPM_RC_RSVD = 0x200;

        private const UInt32 TPM_RC_ERROR_MASK = 0x7f;

        //
        //  Values are 32 bit values laid out as follows:
        //
        //   3 3 2 2 2 2 2 2 2 2 2 2 1 1 1 1 1 1 1 1 1 1
        //   1 0 9 8 7 6 5 4 3 2 1 0 9 8 7 6 5 4 3 2 1 0 9 8 7 6 5 4 3 2 1 0
        //  +---+-+-+-----------------------+-------------------------------+
        //  |Sev|C|R|     Facility          |               Code            |
        //  +---+-+-+-----------------------+-------------------------------+
        //
        //  where
        //
        //      Sev - is the severity code
        //
        //          00 - Success
        //          01 - Informational
        //          10 - Warning
        //          11 - Error
        //
        //      C - is the Customer code flag
        //
        //      R - is a reserved bit
        //
        //      Facility - is the facility code
        //
        //      Code - is the facility's status code
        //
        private const UInt32 STATUS_SEVERITY_MASK = 0xC0000000;
        private const UInt32 STATUS_SEVERITY_ERROR = 0xC0000000;
        private const UInt32 STATUS_SEVERITY_WARNING = 0x80000000;

        private const UInt32 STATUS_FACILITY_MASK = 0x0FFF0000;
        private const UInt32 FACILITY_TPM_SERVICE = 0x00280000;
        private const UInt32 FACILITY_TPM_SOFTWARE = 0x00290000;

        private const UInt32 TPM_RC_FMT_PARAM = 0x40;
        private const int TPM_RC_FMT_PARAM_SHIFT = 8;
        private const UInt32 TPM_RC_FMT_PARAM_MASK = 0xf;
        private const UInt32 TPM_RC_FMT_SESSION = 8;
        private const UInt32 TPM_RC_FMT_SESSION_MASK = 7;

        private const UInt32 TPM_RC_FMT_ERROR_MASK = 0x3f;

        // format error codes
        enum TPM_RC_FMT_CODES
        {
            TPM_RC_ASYMMETRIC = 0x001, // asymmetric algorithm not supported or not correct
            TPM_RC_ATTRIBUTES = 0x002, // inconsistent attributes
            TPM_RC_HASH = 0x003, // hash algorithm not supported or not appropriate 
            TPM_RC_VALUE = 0x004, // value is out of range or is not correct for the context
            TPM_RC_HIERARCHY = 0x005, // hierarchy is not enabled or is not correct for the use
            TPM_RC_KEY_SIZE = 0x007, // key size is not supported
            TPM_RC_MGF = 0x008, // mask generation function not supported
            TPM_RC_MODE = 0x009, // mode of operation not supported
            TPM_RC_TYPE = 0x00A, // the type of the value is not appropriate for the use
            TPM_RC_HANDLE = 0x00B, // the handle is not correct for the use
            TPM_RC_KDF = 0x00C, // unsupported key derivation function or function not appropriate for use
            TPM_RC_RANGE = 0x00D, // value was out of allowed range.
            TPM_RC_AUTH_FAIL = 0x00E, // the authorization HMAC check failed and DA counter incremented 
            TPM_RC_NONCE = 0x00F, // invalid nonce size
            TPM_RC_PP = 0x010, // authorization requires assertion of PP
            TPM_RC_SCHEME = 0x012, // unsupported or incompatible scheme
            TPM_RC_SIZE = 0x015, // structure is the wrong size
            TPM_RC_SYMMETRIC = 0x016, // unsupported symmetric algorithm or key size, or not appropriate for instance
            TPM_RC_TAG = 0x017, // incorrect structure tag
            TPM_RC_SELECTOR = 0x018, // union selector is incorrect
            TPM_RC_INSUFFICIENT = 0x01A, // the TPM was unable to unmarshal a value because there were not enough bytes in the input buffer 
            TPM_RC_SIGNATURE = 0x01B, // the signature is not valid
            TPM_RC_KEY = 0x01C, // key fields are not compatible with each other
            TPM_RC_POLICY_FAIL = 0x01D, // a policy check failed
            TPM_RC_INTEGRITY = 0x01F, // integrity check failed 
            TPM_RC_TICKET = 0x020, // invalid ticket 
            TPM_RC_RESERVED_BITS = 0x021, // reserved bits not set to zero as required
            TPM_RC_BAD_AUTH = 0x022, // authorization failure without DA implications
            TPM_RC_EXPIRED = 0x023, // the policy has expired
            TPM_RC_POLICY_CC = 0x024, // the commandCode in the policy is not the commandCode of the command
            TPM_RC_BINDING = 0x025,
            TPM_RC_CURVE = 0x026, // curve not supported
            TPM_RC_ECC_POINT = 0x27, // point is not on the required curve.
        };

        private string DecodeFormatError(UInt32 input)
        {
            UInt32 param = (input >> TPM_RC_FMT_PARAM_SHIFT) & TPM_RC_FMT_PARAM_MASK;

            string output = "Format Error:\n";

            if ((input & TPM_RC_FMT_PARAM) == TPM_RC_FMT_PARAM)
            {
                output += " Parameter: " + String.Format("{0:x}", param) + "\n";
            }
            else
            {
                if ((param & TPM_RC_FMT_SESSION) == TPM_RC_FMT_SESSION)
                {
                    output += " Session: " + String.Format("{0:x}", param & TPM_RC_FMT_SESSION_MASK) + "\n";
                }
                else
                {
                    output += " Handle: " + String.Format("{0:x}", param) + "\n";
                }
            }

            output += " Error: " + Enum.GetName(typeof(TPM_RC_FMT_CODES), input & TPM_RC_FMT_ERROR_MASK);

            return output;
        }

        enum TPM_RC_WARN_CODES
        {
            TPM_RC_CONTEXT_GAP = 0x001, //	gap for context ID is too large
            TPM_RC_OBJECT_MEMORY = 0x002, //	out of memory for object contexts
            TPM_RC_SESSION_MEMORY = 0x003, // out of memory for session contexts
            TPM_RC_MEMORY = 0x004, //	out of shared object/session memory or need space for internal operations
            TPM_RC_SESSION_HANDLES = 0x005, //	out of session handles – a session must be flushed before a new session may be created
            TPM_RC_OBJECT_HANDLES = 0x006, //	out of object handles – the handle space for objects is depleted and a reboot is required. NOTE	This cannot occur on the reference implementation.
            TPM_RC_LOCALITY = 0x007, //	bad locality
            TPM_RC_YIELDED = 0x008, //	the TPM has suspended operation on the command; forward progress was made and the command may be retried. See Part 1, “Multi-tasking.” NOTE	This cannot occur on the reference implementation.
            TPM_RC_CANCELLED = 0x009, //	the command was canceled
            TPM_RC_TESTING = 0x00A, // TPM is performing self-tests
            TPM_RC_REFERENCE_H0 = 0x010, // the 1st handle in the handle area references a transient object or session that is not loaded
            TPM_RC_REFERENCE_H1 = 0x011, //	the 2nd handle in the handle area references a transient object or session that is not loaded
            TPM_RC_REFERENCE_H2 = 0x012, //	the 3rd handle in the handle area references a transient object or session that is not loaded
            TPM_RC_REFERENCE_H3 = 0x013, //	the 4th handle in the handle area references a transient object or session that is not loaded
            TPM_RC_REFERENCE_H4 = 0x014, //	the 5th handle in the handle area references a transient object or session that is not loaded
            TPM_RC_REFERENCE_H5 = 0x015, //	the 6th handle in the handle area references a transient object or session that is not loaded
            TPM_RC_REFERENCE_H6 = 0x016, //	the 7th handle in the handle area references a transient object or session that is not loaded
            TPM_RC_REFERENCE_S0 = 0x018, //	the 1st authorization session handle references a session that is not loaded
            TPM_RC_REFERENCE_S1 = 0x019, // the 2nd authorization session handle references a session that is not loaded
            TPM_RC_REFERENCE_S2 = 0x01A, //	the 3rd authorization session handle references a session that is not loaded
            TPM_RC_REFERENCE_S3 = 0x01B, //	the 4th authorization session handle references a session that is not loaded
            TPM_RC_REFERENCE_S4 = 0x01C, //	the 5th session handle references a session that is not loaded
            TPM_RC_REFERENCE_S5 = 0x01D, //	the 6th session handle references a session that is not loaded
            TPM_RC_REFERENCE_S6 = 0x01E, //	the 7th authorization session handle references a session that is not loaded
            TPM_RC_NV_RATE = 0x020, //	the TPM is rate-limiting accesses to prevent wearout of NV
            TPM_RC_LOCKOUT = 0x021, //	authorizations for objects subject to DA protection are not allowed at this time because the TPM is in DA lockout mode
            TPM_RC_RETRY = 0x022, //	the TPM was not able to start the command
            TPM_RC_NV_UNAVAILABLE = 0x023, //	the command may require writing of NV and NV is not current accessible
            TPM_RC_NOT_USED = 0x7F, //	this value is reserved and shall not be returned by the TPM
        };

        enum TPM_12_RC_NON_FATAL
        {
            TPM_RETRY = 0x000, // The TPM is too busy to respond to the command immediately, but the command could be resubmitted at a later time.
            TPM_NEEDS_SELFTEST = 0x001, // TPM_ContinueSelfTest has not been run.
            TPM_DOING_SELFTEST = 0x002, // The TPM is currently executing the actions of TPM_ContinueSelfTest because the ordinal required resources that have not been tested.
            TPM_DEFEND_LOCK_RUNNING = 0x003, // The TPM is defending against dictionary attacks and is in some time-out period.
        };

        private string DecodeWarning(UInt32 input)
        {
            UInt32 warning = input & TPM_RC_ERROR_MASK;

            string output = "TPM Warning:";

            string warn20 = Enum.GetName(typeof(TPM_RC_WARN_CODES), warning);
            string warn12 = Enum.GetName(typeof(TPM_12_RC_NON_FATAL), warning);

            if (!string.IsNullOrEmpty(warn20))
            {
                output += "\n Warning (2.0): " + warn20;
            }
            if (!string.IsNullOrEmpty(warn12))
            {
                output += "\n Warning (1.2): " + warn12;
            }
            if (string.IsNullOrEmpty(warn20) &&
                string.IsNullOrEmpty(warn12))
            {
                output += "\n Unknown warning.";
            }

            return output;
        }

        enum TPM_RC_VER1_CODES
        {
            TPM_RC_INITIALIZE = 0x000,      // TPM not initialized
            TPM_RC_FAILURE = 0x001,         // commands not being accepted because of a TPM failure NOTE
                                            // This may be returned by TPM2_GetTestResult() as the testResult parameter.
            TPM_RC_SEQUENCE = 0x003,        // improper use of a sequence handle 
            TPM_RC_PRIVATE = 0x00B,
            TPM_RC_HMAC = 0x019,
            TPM_RC_DISABLED = 0x020,
            TPM_RC_EXCLUSIVE = 0x021,       // command failed because audit sequence required exclusivity
            TPM_RC_ECC_CURVE = 0x023,       // unsupported curve
            TPM_RC_AUTH_TYPE = 0x024,       // authorization handle is not correct for command
            TPM_RC_AUTH_MISSING = 0x025,    // command requires an authorization session for handle and it is not present.
            TPM_RC_POLICY = 0x026,          // policy Failure In Math Operation or an invalid authPolicy value
            TPM_RC_PCR = 0x027,             // PCR check fail
            TPM_RC_PCR_CHANGED = 0x028,     // PCR have changed since checked.
            TPM_RC_ECC_POINT = 0x02C,       // point is not on the required curve.
            TPM_RC_UPGRADE = 0x02D,         // for all commands other than TPM2_FieldUpgradeData(), this code 
                                            // indicates that the TPM is in field upgrade mode; for TPM2_FieldUpgradeData(), 
                                            // this code indicates that the TPM is not in field upgrade mode
            TPM_RC_TOO_MANY_CONTEXTS = 0x02E,   // context ID counter is at maximum.
            TPM_RC_AUTH_UNAVAILABLE = 0x02F,    // authValue or authPolicy is not available for selected entity.
            TPM_RC_REBOOT = 0x030,          // a _TPM_Init and Startup(CLEAR) is required before the TPM can resume operation.
            TPM_RC_UNBALANCED = 0x031,      // the protection algorithms (hash and symmetric) are not reasonably balanced.
                                            // The digest size of the hash must be larger than the key size of the symmetric
                                            // algorithm.
            TPM_RC_COMMAND_SIZE = 0x042,    // command commandSize value is inconsistent with contents of the command buffer;
                                            // either the size is not the same as the bytes loaded by the hardware interface 
                                            // layer or the value is not large enough to hold a command header
            TPM_RC_COMMAND_CODE = 0x043,    // command code not supported
            TPM_RC_AUTHSIZE = 0x044,        // the value of authorzationSize is out of range or the number of bytes in the 
                                            // authorization area is greater than required
            TPM_RC_AUTH_CONTEXT = 0x045,    // use of an authorization session with a context command
            TPM_RC_NV_RANGE = 0x046,        // NV offset+size is out of range.
            TPM_RC_NV_SIZE = 0x047,         // Requested allocation size is larger than allowed.
            TPM_RC_NV_LOCKED = 0x048,       // NV access locked.
            TPM_RC_NV_AUTHORIZATION = 0x049,    // NV access authorization fails in command actions (this failure does not affect lockout.action)
            TPM_RC_NV_UNINITIALIZED = 0x04A,    // an NV Index is used before being initialized or the state saved by TPM2_Shutdown(STATE) could not be restored
            TPM_RC_NV_SPACE = 0x04B,        // insufficient space for NV allocation
            TPM_RC_NV_DEFINED = 0x04C,      // nv index or persisted object already defined
            TPM_RC_BAD_CONTEXT = 0x050,     // context in TPM2_ContextLoad() is not valid
            TPM_RC_CPHASH = 0x051,          // cpHash value already set or not correct for use
            TPM_RC_PARENT = 0x052,          // handle for parent is not a valid parent
            TPM_RC_NEEDS_TEST = 0x053,      // some function needs testing.
            TPM_RC_NO_RESULT = 0x054,       // returned when an internal function cannot process a request due to an 
                                            // unspecified problem. This code is usually related to invalid parameters 
                                            // that are not properly filtered by the input unmarshaling code.
            TPM_RC_SENSITIVE = 0x055,       // the sensitive area did not unmarshal correctly after decryption – this code
                                            // is used in lieu of the other unmarshaling errors so that an attacker cannot 
                                            // determine where the unmarshaling error occurred
            RC_MAX_FM0 = 0x07F,             // largest format 0 code that is not a warning
        };

        enum TPM_12_ERROR_CODES
        {
            TPM_AUTHFAIL = 1, // Authentication failed
            TPM_BADINDEX = 2, // The index to a PCR, DIR or other register is incorrect
            TPM_BAD_PARAMETER = 3, // One or more parameter is bad
            TPM_AUDITFAILURE = 4, // An operation completed successfully but the auditing of that operation failed. 
            TPM_CLEAR_DISABLED = 5, // The clear disable flag is set and all clear operations now require physical access
            TPM_DEACTIVATED = 6, // The TPM is deactivated
            TPM_DISABLED = 7, // The TPM is disabled
            TPM_DISABLED_CMD = 8, // The target command has been disabled
            TPM_FAIL = 9, // The operation failed
            TPM_BAD_ORDINAL = 10, // The ordinal was unknown or inconsistent
            TPM_INSTALL_DISABLED = 11, // The ability to install an owner is disabled
            TPM_INVALID_KEYHANDLE = 12, // The key handle can not be interpreted
            TPM_KEYNOTFOUND = 13, // The key handle points to an invalid key
            TPM_INAPPROPRIATE_ENC = 14, // Unacceptable encryption scheme
            TPM_MIGRATEFAIL = 15, // Migration authorization failed
            TPM_INVALID_PCR_INFO = 16, // PCR information could not be interpreted
            TPM_NOSPACE = 17, // No room to load key. 
            TPM_NOSRK = 18, // There is no SRK set.  This is an appropriate response when an unowned TPM receives a command that requires a TPM owner. 
            TPM_NOTSEALED_BLOB = 19, // An encrypted blob is invalid or was not created by this TPM
            TPM_OWNER_SET = 20, // There is already an Owner
            TPM_RESOURCES = 21, // The TPM has insufficient internal resources to perform the requested action.
            TPM_SHORTRANDOM = 22, // A random string was too short
            TPM_SIZE = 23, // The TPM does not have the space to perform the operation.
            TPM_WRONGPCRVAL = 24, // The named PCR value does not match the current PCR value.
            TPM_BAD_PARAM_SIZE = 25, // The paramSize argument to the command has the incorrect value
            TPM_SHA_THREAD = 26, // There is no existing SHA-1 thread.
            TPM_SHA_ERROR = 27, // The calculation is unable to proceed because the existing SHA-1 thread has already encountered an error. 
            TPM_FAILEDSELFTEST = 28, // Self-test has failed and the TPM has shutdown. 
            TPM_AUTH2FAIL = 29, // The authorization for the second key in a 2 key function failed authorization
            TPM_BADTAG = 30, // The tag value sent to for a command is invalid
            TPM_IOERROR = 31, // An IO error occurred transmitting information to the TPM
            TPM_ENCRYPT_ERROR = 32, // The encryption process had a problem. 
            TPM_DECRYPT_ERROR = 33, // The decryption process did not complete. 
            TPM_INVALID_AUTHHANDLE = 34, // An invalid handle was used.
            TPM_NO_ENDORSEMENT = 35, // The TPM does not have a EK installed
            TPM_INVALID_KEYUSAGE = 36, // The usage of a key is not allowed
            TPM_WRONG_ENTITYTYPE = 37, // The submitted entity type is not allowed
            TPM_INVALID_POSTINIT = 38, // The command was received in the wrong sequence relative to TPM_Init and a subsequent TPM_Startup
            TPM_INAPPROPRIATE_SIG = 39, // Signed data cannot include additional DER information
            TPM_BAD_KEY_PROPERTY = 40, // The key properties in TPM_KEY_PARMs are not supported by this TPM
            TPM_BAD_MIGRATION = 41, // The migration properties of this key are incorrect.
            TPM_BAD_SCHEME = 42, // The signature or encryption scheme for this key is incorrect or not permitted in this situation.
            TPM_BAD_DATASIZE = 43, // The size of the data (or blob) parameter is bad or inconsistent with the referenced key
            TPM_BAD_MODE = 44, // A parameter is bad, such as capArea or subCapArea for TPM_GetCapability, physicalPresence parameter for TPM_PhysicalPresence, or migrationType for TPM_CreateMigrationBlob.
            TPM_BAD_PRESENCE = 45, // Either the physicalPresence or physicalPresenceLock bits have the wrong value
            TPM_BAD_VERSION = 46, // The TPM cannot perform this version of the capability
            TPM_NO_WRAP_TRANSPORT = 47, // The TPM does not allow for wrapped transport sessions
            TPM_AUDITFAIL_UNSUCCESSFUL = 48, // TPM audit construction failed and the underlying command was returning a failure code also
            TPM_AUDITFAIL_SUCCESSFUL = 49, // TPM audit construction failed and the underlying command was returning success
            TPM_NOTRESETABLE = 50, // Attempt to reset a PCR register that does not have the resettable attribute
            TPM_NOTLOCAL = 51, // Attempt to reset a PCR register that requires locality and locality modifier not part of command transport
            TPM_BAD_TYPE = 52, // Make identity blob not properly typed
            TPM_INVALID_RESOURCE = 53, // When saving context identified resource type does not match actual resource
            TPM_NOTFIPS = 54, // The TPM is attempting to execute a command only available when in FIPS mode
            TPM_INVALID_FAMILY = 55, // The command is attempting to use an invalid family ID
            TPM_NO_NV_PERMISSION = 56, // The permission to manipulate the NV storage is not available
            TPM_REQUIRES_SIGN = 57, // The operation requires a signed command
            TPM_KEY_NOTSUPPORTED = 58, // Wrong operation to load an NV key
            TPM_AUTH_CONFLICT = 59, // NV_LoadKey blob requires both owner and blob authorization
            TPM_AREA_LOCKED = 60, // The NV area is locked and not writable
            TPM_BAD_LOCALITY = 61, // The locality is incorrect for the attempted operation
            TPM_READ_ONLY = 62, // The NV area is read only and can’t be written to
            TPM_PER_NOWRITE = 63, // There is no protection on the write to the NV area
            TPM_FAMILYCOUNT = 64, // The family count value does not match
            TPM_WRITE_LOCKED = 65, // The NV area has already been written to
            TPM_BAD_ATTRIBUTES = 66, // The NV area attributes conflict
            TPM_INVALID_STRUCTURE = 67, // The structure tag and version are invalid or inconsistent
            TPM_KEY_OWNER_CONTROL = 68, // The key is under control of the TPM Owner and can only be evicted by the TPM Owner.
            TPM_BAD_COUNTER = 69, // The counter handle is incorrect
            TPM_NOT_FULLWRITE = 70, // The write is not a complete write of the area
            TPM_CONTEXT_GAP = 71, // The gap between saved context counts is too large
            TPM_MAXNVWRITES = 72, // The maximum number of NV writes without an owner has been exceeded
            TPM_NOOPERATOR = 73, // No operator AuthData value is set
            TPM_RESOURCEMISSING = 74, // The resource pointed to by context is not loaded
            TPM_DELEGATE_LOCK = 75, // The delegate administration is locked
            TPM_DELEGATE_FAMILY = 76, // Attempt to manage a family other then the delegated family
            TPM_DELEGATE_ADMIN = 77, // Delegation table management not enabled
            TPM_TRANSPORT_NOTEXCLUSIVE = 78, // There was a command executed outside of an exclusive transport session
            TPM_OWNER_CONTROL = 79, // Attempt to context save a owner evict controlled key
            TPM_DAA_RESOURCES = 80, // The DAA command has no resources available to execute the command
            TPM_DAA_INPUT_DATA0 = 81, // The consistency check on DAA parameter inputData0 has failed.
            TPM_DAA_INPUT_DATA1 = 82, // The consistency check on DAA parameter inputData1 has failed.
            TPM_DAA_ISSUER_SETTINGS = 83, // The consistency check on DAA_issuerSettings has failed.
            TPM_DAA_TPM_SETTINGS = 84, // The consistency check on DAA_tpmSpecific has failed.
            TPM_DAA_STAGE = 85, // The atomic process indicated by the submitted DAA command is not the expected process. 
            TPM_DAA_ISSUER_VALIDITY = 86, // The issuer’s validity check has detected an inconsistency
            TPM_DAA_WRONG_W = 87, // The consistency check on w has failed.
            TPM_BAD_HANDLE = 88, // The handle is incorrect
            TPM_BAD_DELEGATE = 89, // Delegation is not correct
            TPM_BADCONTEXT = 90, // The context blob is invalid
            TPM_TOOMANYCONTEXTS = 91, // Too many contexts held by the TPM
            TPM_MA_TICKET_SIGNATURE = 92, // Migration authority signature validation failure
            TPM_MA_DESTINATION = 93, // Migration destination not authenticated
            TPM_MA_SOURCE = 94, // Migration source incorrect
            TPM_MA_AUTHORITY = 95, // Incorrect migration authority
            TPM_PERMANENTEK = 97, // Attempt to revoke the EK and the EK is not revocable
            TPM_BAD_SIGNATURE = 98, // Bad signature of CMK ticket
            TPM_NOCONTEXTSPACE = 99, // There is no room in the context list for additional contexts
        };

        private string DecodeError(UInt32 input)
        {
            UInt32 error = input & TPM_RC_ERROR_MASK;

            string output = "TPM Error:";

            string error20 = Enum.GetName(typeof(TPM_RC_VER1_CODES), error);
            string error12 = Enum.GetName(typeof(TPM_12_ERROR_CODES), error);

            if (!string.IsNullOrEmpty(error20))
            {
                output += "\n Error (2.0): " + error20;
            }
            if (!string.IsNullOrEmpty(error12))
            {
                output += "\n Error (1.2): " + error12;
            }
            if (string.IsNullOrEmpty(error20) &&
                string.IsNullOrEmpty(error12))
            {
                output += "\n Unknown error.";
            }

            return output;
        }

        enum TPM_WIN_ERROR : uint
        {
            TPM_E_ERROR_MASK = 0x80280000,
            TPM_E_AUTHFAIL = 0x80280001,
            TPM_E_BADINDEX = 0x80280002,
            TPM_E_BAD_PARAMETER = 0x80280003,
            TPM_E_AUDITFAILURE = 0x80280004,
            TPM_E_CLEAR_DISABLED = 0x80280005,
            TPM_E_DEACTIVATED = 0x80280006,
            TPM_E_DISABLED = 0x80280007,
            TPM_E_DISABLED_CMD = 0x80280008,
            TPM_E_FAIL = 0x80280009,
            TPM_E_BAD_ORDINAL = 0x8028000A,
            TPM_E_INSTALL_DISABLED = 0x8028000B,
            TPM_E_INVALID_KEYHANDLE = 0x8028000C,
            TPM_E_KEYNOTFOUND = 0x8028000D,
            TPM_E_INAPPROPRIATE_ENC = 0x8028000E,
            TPM_E_MIGRATEFAIL = 0x8028000F,
            TPM_E_INVALID_PCR_INFO = 0x80280010,
            TPM_E_NOSPACE = 0x80280011,
            TPM_E_NOSRK = 0x80280012,
            TPM_E_NOTSEALED_BLOB = 0x80280013,
            TPM_E_OWNER_SET = 0x80280014,
            TPM_E_RESOURCES = 0x80280015,
            TPM_E_SHORTRANDOM = 0x80280016,
            TPM_E_SIZE = 0x80280017,
            TPM_E_WRONGPCRVAL = 0x80280018,
            TPM_E_BAD_PARAM_SIZE = 0x80280019,
            TPM_E_SHA_THREAD = 0x8028001A,
            TPM_E_SHA_ERROR = 0x8028001B,
            TPM_E_FAILEDSELFTEST = 0x8028001C,
            TPM_E_AUTH2FAIL = 0x8028001D,
            TPM_E_BADTAG = 0x8028001E,
            TPM_E_IOERROR = 0x8028001F,
            TPM_E_ENCRYPT_ERROR = 0x80280020,
            TPM_E_DECRYPT_ERROR = 0x80280021,
            TPM_E_INVALID_AUTHHANDLE = 0x80280022,
            TPM_E_NO_ENDORSEMENT = 0x80280023,
            TPM_E_INVALID_KEYUSAGE = 0x80280024,
            TPM_E_WRONG_ENTITYTYPE = 0x80280025,
            TPM_E_INVALID_POSTINIT = 0x80280026,
            TPM_E_INAPPROPRIATE_SIG = 0x80280027,
            TPM_E_BAD_KEY_PROPERTY = 0x80280028,
            TPM_E_BAD_MIGRATION = 0x80280029,
            TPM_E_BAD_SCHEME = 0x8028002A,
            TPM_E_BAD_DATASIZE = 0x8028002B,
            TPM_E_BAD_MODE = 0x8028002C,
            TPM_E_BAD_PRESENCE = 0x8028002D,
            TPM_E_BAD_VERSION = 0x8028002E,
            TPM_E_NO_WRAP_TRANSPORT = 0x8028002F,
            TPM_E_AUDITFAIL_UNSUCCESSFUL = 0x80280030,
            TPM_E_AUDITFAIL_SUCCESSFUL = 0x80280031,
            TPM_E_NOTRESETABLE = 0x80280032,
            TPM_E_NOTLOCAL = 0x80280033,
            TPM_E_BAD_TYPE = 0x80280034,
            TPM_E_INVALID_RESOURCE = 0x80280035,
            TPM_E_NOTFIPS = 0x80280036,
            TPM_E_INVALID_FAMILY = 0x80280037,
            TPM_E_NO_NV_PERMISSION = 0x80280038,
            TPM_E_REQUIRES_SIGN = 0x80280039,
            TPM_E_KEY_NOTSUPPORTED = 0x8028003A,
            TPM_E_AUTH_CONFLICT = 0x8028003B,
            TPM_E_AREA_LOCKED = 0x8028003C,
            TPM_E_BAD_LOCALITY = 0x8028003D,
            TPM_E_READ_ONLY = 0x8028003E,
            TPM_E_PER_NOWRITE = 0x8028003F,
            TPM_E_FAMILYCOUNT = 0x80280040,
            TPM_E_WRITE_LOCKED = 0x80280041,
            TPM_E_BAD_ATTRIBUTES = 0x80280042,
            TPM_E_INVALID_STRUCTURE = 0x80280043,
            TPM_E_KEY_OWNER_CONTROL = 0x80280044,
            TPM_E_BAD_COUNTER = 0x80280045,
            TPM_E_NOT_FULLWRITE = 0x80280046,
            TPM_E_CONTEXT_GAP = 0x80280047,
            TPM_E_MAXNVWRITES = 0x80280048,
            TPM_E_NOOPERATOR = 0x80280049,
            TPM_E_RESOURCEMISSING = 0x8028004A,
            TPM_E_DELEGATE_LOCK = 0x8028004B,
            TPM_E_DELEGATE_FAMILY = 0x8028004C,
            TPM_E_DELEGATE_ADMIN = 0x8028004D,
            TPM_E_TRANSPORT_NOTEXCLUSIVE = 0x8028004E,
            TPM_E_OWNER_CONTROL = 0x8028004F,
            TPM_E_DAA_RESOURCES = 0x80280050,
            TPM_E_DAA_INPUT_DATA0 = 0x80280051,
            TPM_E_DAA_INPUT_DATA1 = 0x80280052,
            TPM_E_DAA_ISSUER_SETTINGS = 0x80280053,
            TPM_E_DAA_TPM_SETTINGS = 0x80280054,
            TPM_E_DAA_STAGE = 0x80280055,
            TPM_E_DAA_ISSUER_VALIDITY = 0x80280056,
            TPM_E_DAA_WRONG_W = 0x80280057,
            TPM_E_BAD_HANDLE = 0x80280058,
            TPM_E_BAD_DELEGATE = 0x80280059,
            TPM_E_BADCONTEXT = 0x8028005A,
            TPM_E_TOOMANYCONTEXTS = 0x8028005B,
            TPM_E_MA_TICKET_SIGNATURE = 0x8028005C,
            TPM_E_MA_DESTINATION = 0x8028005D,
            TPM_E_MA_SOURCE = 0x8028005E,
            TPM_E_MA_AUTHORITY = 0x8028005F,
            TPM_E_PERMANENTEK = 0x80280061,
            TPM_E_BAD_SIGNATURE = 0x80280062,
            TPM_E_NOCONTEXTSPACE = 0x80280063,
            TPM_E_COMMAND_BLOCKED = 0x80280400,
            TPM_E_INVALID_HANDLE = 0x80280401,
            TPM_E_DUPLICATE_VHANDLE = 0x80280402,
            TPM_E_EMBEDDED_COMMAND_BLOCKED = 0x80280403,
            TPM_E_EMBEDDED_COMMAND_UNSUPPORTED = 0x80280404,
            TPM_E_RETRY = 0x80280800,
            TPM_E_NEEDS_SELFTEST = 0x80280801,
            TPM_E_DOING_SELFTEST = 0x80280802,
            TPM_E_DEFEND_LOCK_RUNNING = 0x80280803,
            TBS_E_INTERNAL_ERROR = 0x80284001,
            TBS_E_BAD_PARAMETER = 0x80284002,
            TBS_E_INVALID_OUTPUT_POINTER = 0x80284003,
            TBS_E_INVALID_CONTEXT = 0x80284004,
            TBS_E_INSUFFICIENT_BUFFER = 0x80284005,
            TBS_E_IOERROR = 0x80284006,
            TBS_E_INVALID_CONTEXT_PARAM = 0x80284007,
            TBS_E_SERVICE_NOT_RUNNING = 0x80284008,
            TBS_E_TOO_MANY_TBS_CONTEXTS = 0x80284009,
            TBS_E_TOO_MANY_RESOURCES = 0x8028400A,
            TBS_E_SERVICE_START_PENDING = 0x8028400B,
            TBS_E_PPI_NOT_SUPPORTED = 0x8028400C,
            TBS_E_COMMAND_CANCELED = 0x8028400D,
            TBS_E_BUFFER_TOO_LARGE = 0x8028400E,
            TBS_E_TPM_NOT_FOUND = 0x8028400F,
            TBS_E_SERVICE_DISABLED = 0x80284010,
            TBS_E_NO_EVENT_LOG = 0x80284011,
            TBS_E_ACCESS_DENIED = 0x80284012,
            TBS_E_PROVISIONING_NOT_ALLOWED = 0x80284013,
            TBS_E_PPI_FUNCTION_UNSUPPORTED = 0x80284014,
            TBS_E_OWNERAUTH_NOT_FOUND = 0x80284015,
            TBS_E_PROVISIONING_INCOMPLETE = 0x80284016,
            TPMAPI_E_INVALID_STATE = 0x80290100,
            TPMAPI_E_NOT_ENOUGH_DATA = 0x80290101,
            TPMAPI_E_TOO_MUCH_DATA = 0x80290102,
            TPMAPI_E_INVALID_OUTPUT_POINTER = 0x80290103,
            TPMAPI_E_INVALID_PARAMETER = 0x80290104,
            TPMAPI_E_OUT_OF_MEMORY = 0x80290105,
            TPMAPI_E_BUFFER_TOO_SMALL = 0x80290106,
            TPMAPI_E_INTERNAL_ERROR = 0x80290107,
            TPMAPI_E_ACCESS_DENIED = 0x80290108,
            TPMAPI_E_AUTHORIZATION_FAILED = 0x80290109,
            TPMAPI_E_INVALID_CONTEXT_HANDLE = 0x8029010A,
            TPMAPI_E_TBS_COMMUNICATION_ERROR = 0x8029010B,
            TPMAPI_E_TPM_COMMAND_ERROR = 0x8029010C,
            TPMAPI_E_MESSAGE_TOO_LARGE = 0x8029010D,
            TPMAPI_E_INVALID_ENCODING = 0x8029010E,
            TPMAPI_E_INVALID_KEY_SIZE = 0x8029010F,
            TPMAPI_E_ENCRYPTION_FAILED = 0x80290110,
            TPMAPI_E_INVALID_KEY_PARAMS = 0x80290111,
            TPMAPI_E_INVALID_MIGRATION_AUTHORIZATION_BLOB = 0x80290112,
            TPMAPI_E_INVALID_PCR_INDEX = 0x80290113,
            TPMAPI_E_INVALID_DELEGATE_BLOB = 0x80290114,
            TPMAPI_E_INVALID_CONTEXT_PARAMS = 0x80290115,
            TPMAPI_E_INVALID_KEY_BLOB = 0x80290116,
            TPMAPI_E_INVALID_PCR_DATA = 0x80290117,
            TPMAPI_E_INVALID_OWNER_AUTH = 0x80290118,
            TPMAPI_E_FIPS_RNG_CHECK_FAILED = 0x80290119,
            TPMAPI_E_EMPTY_TCG_LOG = 0x8029011A,
            TPMAPI_E_INVALID_TCG_LOG_ENTRY = 0x8029011B,
            TPMAPI_E_TCG_SEPARATOR_ABSENT = 0x8029011C,
            TPMAPI_E_TCG_INVALID_DIGEST_ENTRY = 0x8029011D,
            TPMAPI_E_POLICY_DENIES_OPERATION = 0x8029011E,
            TBSIMP_E_BUFFER_TOO_SMALL = 0x80290200,
            TBSIMP_E_CLEANUP_FAILED = 0x80290201,
            TBSIMP_E_INVALID_CONTEXT_HANDLE = 0x80290202,
            TBSIMP_E_INVALID_CONTEXT_PARAM = 0x80290203,
            TBSIMP_E_TPM_ERROR = 0x80290204,
            TBSIMP_E_HASH_BAD_KEY = 0x80290205,
            TBSIMP_E_DUPLICATE_VHANDLE = 0x80290206,
            TBSIMP_E_INVALID_OUTPUT_POINTER = 0x80290207,
            TBSIMP_E_INVALID_PARAMETER = 0x80290208,
            TBSIMP_E_RPC_INIT_FAILED = 0x80290209,
            TBSIMP_E_SCHEDULER_NOT_RUNNING = 0x8029020A,
            TBSIMP_E_COMMAND_CANCELED = 0x8029020B,
            TBSIMP_E_OUT_OF_MEMORY = 0x8029020C,
            TBSIMP_E_LIST_NO_MORE_ITEMS = 0x8029020D,
            TBSIMP_E_LIST_NOT_FOUND = 0x8029020E,
            TBSIMP_E_NOT_ENOUGH_SPACE = 0x8029020F,
            TBSIMP_E_NOT_ENOUGH_TPM_CONTEXTS = 0x80290210,
            TBSIMP_E_COMMAND_FAILED = 0x80290211,
            TBSIMP_E_UNKNOWN_ORDINAL = 0x80290212,
            TBSIMP_E_RESOURCE_EXPIRED = 0x80290213,
            TBSIMP_E_INVALID_RESOURCE = 0x80290214,
            TBSIMP_E_NOTHING_TO_UNLOAD = 0x80290215,
            TBSIMP_E_HASH_TABLE_FULL = 0x80290216,
            TBSIMP_E_TOO_MANY_TBS_CONTEXTS = 0x80290217,
            TBSIMP_E_TOO_MANY_RESOURCES = 0x80290218,
            TBSIMP_E_PPI_NOT_SUPPORTED = 0x80290219,
            TBSIMP_E_TPM_INCOMPATIBLE = 0x8029021A,
            TBSIMP_E_NO_EVENT_LOG = 0x8029021B,
            TPM_E_PPI_ACPI_FAILURE = 0x80290300,
            TPM_E_PPI_USER_ABORT = 0x80290301,
            TPM_E_PPI_BIOS_FAILURE = 0x80290302,
            TPM_E_PPI_NOT_SUPPORTED = 0x80290303,
            TPM_E_PPI_BLOCKED_IN_BIOS = 0x80290304,
            TPM_E_PCP_ERROR_MASK = 0x80290400,
            TPM_E_PCP_DEVICE_NOT_READY = 0x80290401,
            TPM_E_PCP_INVALID_HANDLE = 0x80290402,
            TPM_E_PCP_INVALID_PARAMETER = 0x80290403,
            TPM_E_PCP_FLAG_NOT_SUPPORTED = 0x80290404,
            TPM_E_PCP_NOT_SUPPORTED = 0x80290405,
            TPM_E_PCP_BUFFER_TOO_SMALL = 0x80290406,
            TPM_E_PCP_INTERNAL_ERROR = 0x80290407,
            TPM_E_PCP_AUTHENTICATION_FAILED = 0x80290408,
            TPM_E_PCP_AUTHENTICATION_IGNORED = 0x80290409,
            TPM_E_PCP_POLICY_NOT_FOUND = 0x8029040A,
            TPM_E_PCP_PROFILE_NOT_FOUND = 0x8029040B,
            TPM_E_PCP_VALIDATION_FAILED = 0x8029040C,
            STATUS_TPM_ERROR_MASK = 0xC0290000,
            STATUS_TPM_AUTHFAIL = 0xC0290001,
            STATUS_TPM_BADINDEX = 0xC0290002,
            STATUS_TPM_BAD_PARAMETER = 0xC0290003,
            STATUS_TPM_AUDITFAILURE = 0xC0290004,
            STATUS_TPM_CLEAR_DISABLED = 0xC0290005,
            STATUS_TPM_DEACTIVATED = 0xC0290006,
            STATUS_TPM_DISABLED = 0xC0290007,
            STATUS_TPM_DISABLED_CMD = 0xC0290008,
            STATUS_TPM_FAIL = 0xC0290009,
            STATUS_TPM_BAD_ORDINAL = 0xC029000A,
            STATUS_TPM_INSTALL_DISABLED = 0xC029000B,
            STATUS_TPM_INVALID_KEYHANDLE = 0xC029000C,
            STATUS_TPM_KEYNOTFOUND = 0xC029000D,
            STATUS_TPM_INAPPROPRIATE_ENC = 0xC029000E,
            STATUS_TPM_MIGRATEFAIL = 0xC029000F,
            STATUS_TPM_INVALID_PCR_INFO = 0xC0290010,
            STATUS_TPM_NOSPACE = 0xC0290011,
            STATUS_TPM_NOSRK = 0xC0290012,
            STATUS_TPM_NOTSEALED_BLOB = 0xC0290013,
            STATUS_TPM_OWNER_SET = 0xC0290014,
            STATUS_TPM_RESOURCES = 0xC0290015,
            STATUS_TPM_SHORTRANDOM = 0xC0290016,
            STATUS_TPM_SIZE = 0xC0290017,
            STATUS_TPM_WRONGPCRVAL = 0xC0290018,
            STATUS_TPM_BAD_PARAM_SIZE = 0xC0290019,
            STATUS_TPM_SHA_THREAD = 0xC029001A,
            STATUS_TPM_SHA_ERROR = 0xC029001B,
            STATUS_TPM_FAILEDSELFTEST = 0xC029001C,
            STATUS_TPM_AUTH2FAIL = 0xC029001D,
            STATUS_TPM_BADTAG = 0xC029001E,
            STATUS_TPM_IOERROR = 0xC029001F,
            STATUS_TPM_ENCRYPT_ERROR = 0xC0290020,
            STATUS_TPM_DECRYPT_ERROR = 0xC0290021,
            STATUS_TPM_INVALID_AUTHHANDLE = 0xC0290022,
            STATUS_TPM_NO_ENDORSEMENT = 0xC0290023,
            STATUS_TPM_INVALID_KEYUSAGE = 0xC0290024,
            STATUS_TPM_WRONG_ENTITYTYPE = 0xC0290025,
            STATUS_TPM_INVALID_POSTINIT = 0xC0290026,
            STATUS_TPM_INAPPROPRIATE_SIG = 0xC0290027,
            STATUS_TPM_BAD_KEY_PROPERTY = 0xC0290028,
            STATUS_TPM_BAD_MIGRATION = 0xC0290029,
            STATUS_TPM_BAD_SCHEME = 0xC029002A,
            STATUS_TPM_BAD_DATASIZE = 0xC029002B,
            STATUS_TPM_BAD_MODE = 0xC029002C,
            STATUS_TPM_BAD_PRESENCE = 0xC029002D,
            STATUS_TPM_BAD_VERSION = 0xC029002E,
            STATUS_TPM_NO_WRAP_TRANSPORT = 0xC029002F,
            STATUS_TPM_AUDITFAIL_UNSUCCESSFUL = 0xC0290030,
            STATUS_TPM_AUDITFAIL_SUCCESSFUL = 0xC0290031,
            STATUS_TPM_NOTRESETABLE = 0xC0290032,
            STATUS_TPM_NOTLOCAL = 0xC0290033,
            STATUS_TPM_BAD_TYPE = 0xC0290034,
            STATUS_TPM_INVALID_RESOURCE = 0xC0290035,
            STATUS_TPM_NOTFIPS = 0xC0290036,
            STATUS_TPM_INVALID_FAMILY = 0xC0290037,
            STATUS_TPM_NO_NV_PERMISSION = 0xC0290038,
            STATUS_TPM_REQUIRES_SIGN = 0xC0290039,
            STATUS_TPM_KEY_NOTSUPPORTED = 0xC029003A,
            STATUS_TPM_AUTH_CONFLICT = 0xC029003B,
            STATUS_TPM_AREA_LOCKED = 0xC029003C,
            STATUS_TPM_BAD_LOCALITY = 0xC029003D,
            STATUS_TPM_READ_ONLY = 0xC029003E,
            STATUS_TPM_PER_NOWRITE = 0xC029003F,
            STATUS_TPM_FAMILYCOUNT = 0xC0290040,
            STATUS_TPM_WRITE_LOCKED = 0xC0290041,
            STATUS_TPM_BAD_ATTRIBUTES = 0xC0290042,
            STATUS_TPM_INVALID_STRUCTURE = 0xC0290043,
            STATUS_TPM_KEY_OWNER_CONTROL = 0xC0290044,
            STATUS_TPM_BAD_COUNTER = 0xC0290045,
            STATUS_TPM_NOT_FULLWRITE = 0xC0290046,
            STATUS_TPM_CONTEXT_GAP = 0xC0290047,
            STATUS_TPM_MAXNVWRITES = 0xC0290048,
            STATUS_TPM_NOOPERATOR = 0xC0290049,
            STATUS_TPM_RESOURCEMISSING = 0xC029004A,
            STATUS_TPM_DELEGATE_LOCK = 0xC029004B,
            STATUS_TPM_DELEGATE_FAMILY = 0xC029004C,
            STATUS_TPM_DELEGATE_ADMIN = 0xC029004D,
            STATUS_TPM_TRANSPORT_NOTEXCLUSIVE = 0xC029004E,
            STATUS_TPM_OWNER_CONTROL = 0xC029004F,
            STATUS_TPM_DAA_RESOURCES = 0xC0290050,
            STATUS_TPM_DAA_INPUT_DATA0 = 0xC0290051,
            STATUS_TPM_DAA_INPUT_DATA1 = 0xC0290052,
            STATUS_TPM_DAA_ISSUER_SETTINGS = 0xC0290053,
            STATUS_TPM_DAA_TPM_SETTINGS = 0xC0290054,
            STATUS_TPM_DAA_STAGE = 0xC0290055,
            STATUS_TPM_DAA_ISSUER_VALIDITY = 0xC0290056,
            STATUS_TPM_DAA_WRONG_W = 0xC0290057,
            STATUS_TPM_BAD_HANDLE = 0xC0290058,
            STATUS_TPM_BAD_DELEGATE = 0xC0290059,
            STATUS_TPM_BADCONTEXT = 0xC029005A,
            STATUS_TPM_TOOMANYCONTEXTS = 0xC029005B,
            STATUS_TPM_MA_TICKET_SIGNATURE = 0xC029005C,
            STATUS_TPM_MA_DESTINATION = 0xC029005D,
            STATUS_TPM_MA_SOURCE = 0xC029005E,
            STATUS_TPM_MA_AUTHORITY = 0xC029005F,
            STATUS_TPM_PERMANENTEK = 0xC0290061,
            STATUS_TPM_BAD_SIGNATURE = 0xC0290062,
            STATUS_TPM_NOCONTEXTSPACE = 0xC0290063,
            STATUS_TPM_COMMAND_BLOCKED = 0xC0290400,
            STATUS_TPM_INVALID_HANDLE = 0xC0290401,
            STATUS_TPM_DUPLICATE_VHANDLE = 0xC0290402,
            STATUS_TPM_EMBEDDED_COMMAND_BLOCKED = 0xC0290403,
            STATUS_TPM_EMBEDDED_COMMAND_UNSUPPORTED = 0xC0290404,
            STATUS_TPM_RETRY = 0xC0290800,
            STATUS_TPM_NEEDS_SELFTEST = 0xC0290801,
            STATUS_TPM_DOING_SELFTEST = 0xC0290802,
            STATUS_TPM_DEFEND_LOCK_RUNNING = 0xC0290803,
            STATUS_TPM_COMMAND_CANCELED = 0xC0291001,
            STATUS_TPM_TOO_MANY_CONTEXTS = 0xC0291002,
            STATUS_TPM_NOT_FOUND = 0xC0291003,
            STATUS_TPM_ACCESS_DENIED = 0xC0291004,
            STATUS_TPM_INSUFFICIENT_BUFFER = 0xC0291005,
            STATUS_TPM_PPI_FUNCTION_UNSUPPORTED = 0xC0291006,
            STATUS_PCP_ERROR_MASK = 0xC0292000,
            STATUS_PCP_DEVICE_NOT_READY = 0xC0292001,
            STATUS_PCP_INVALID_HANDLE = 0xC0292002,
            STATUS_PCP_INVALID_PARAMETER = 0xC0292003,
            STATUS_PCP_FLAG_NOT_SUPPORTED = 0xC0292004,
            STATUS_PCP_NOT_SUPPORTED = 0xC0292005,
            STATUS_PCP_BUFFER_TOO_SMALL = 0xC0292006,
            STATUS_PCP_INTERNAL_ERROR = 0xC0292007,
            STATUS_PCP_AUTHENTICATION_FAILED = 0xC0292008,
            STATUS_PCP_AUTHENTICATION_IGNORED = 0xC0292009,
            STATUS_PCP_POLICY_NOT_FOUND = 0xC029200A,
            STATUS_PCP_PROFILE_NOT_FOUND = 0xC029200B,
            STATUS_PCP_VALIDATION_FAILED = 0xC029200C,
            STATUS_PCP_DEVICE_NOT_FOUND = 0xC029200D,
        };

        private static readonly Dictionary<UInt32, string> TPM_WIN_ERROR_DESC = new Dictionary<UInt32, string>
        {
            { 0x80280000, "This is an error mask to convert TPM hardware errors to win errors." },
            { 0x80280001, "Authentication failed." },
            { 0x80280002, "The index to a PCR, DIR or other register is incorrect." },
            { 0x80280003, "One or more parameter is bad." },
            { 0x80280004, "An operation completed successfully but the auditing of that operation failed." },
            { 0x80280005, "The clear disable flag is set and all clear operations now require physical access." },
            { 0x80280006, "Activate the Trusted Platform Module (TPM)." },
            { 0x80280007, "Enable the Trusted Platform Module (TPM)." },
            { 0x80280008, "The target command has been disabled." },
            { 0x80280009, "The operation failed." },
            { 0x8028000A, "The ordinal was unknown or inconsistent." },
            { 0x8028000B, "The ability to install an owner is disabled." },
            { 0x8028000C, "The key handle cannot be interpreted." },
            { 0x8028000D, "The key handle points to an invalid key." },
            { 0x8028000E, "Unacceptable encryption scheme." },
            { 0x8028000F, "Migration authorization failed." },
            { 0x80280010, "PCR information could not be interpreted." },
            { 0x80280011, "No room to load key." },
            { 0x80280012, "There is no Storage Root Key (SRK) set." },
            { 0x80280013, "An encrypted blob is invalid or was not created by this TPM." },
            { 0x80280014, "The Trusted Platform Module (TPM) already has an owner." },
            { 0x80280015, "The TPM has insufficient internal resources to perform the requested action." },
            { 0x80280016, "A random string was too short." },
            { 0x80280017, "The TPM does not have the space to perform the operation." },
            { 0x80280018, "The named PCR value does not match the current PCR value." },
            { 0x80280019, "The paramSize argument to the command has the incorrect value ." },
            { 0x8028001A, "There is no existing SHA-1 thread." },
            { 0x8028001B, "The calculation is unable to proceed because the existing SHA-1 thread has already encountered an error." },
            { 0x8028001C, "The TPM hardware device reported a failure during its internal self test. Try restarting the computer to resolve the problem. If the problem continues, you might need to replace your TPM hardware or motherboard." },
            { 0x8028001D, "The authorization for the second key in a 2 key function failed authorization." },
            { 0x8028001E, "The tag value sent to for a command is invalid." },
            { 0x8028001F, "An IO error occurred transmitting information to the TPM." },
            { 0x80280020, "The encryption process had a problem." },
            { 0x80280021, "The decryption process did not complete." },
            { 0x80280022, "An invalid handle was used." },
            { 0x80280023, "The TPM does not have an Endorsement Key (EK) installed." },
            { 0x80280024, "The usage of a key is not allowed." },
            { 0x80280025, "The submitted entity type is not allowed." },
            { 0x80280026, "The command was received in the wrong sequence relative to TPM_Init and a subsequent TPM_Startup." },
            { 0x80280027, "Signed data cannot include additional DER information." },
            { 0x80280028, "The key properties in TPM_KEY_PARMs are not supported by this TPM." },
            { 0x80280029, "The migration properties of this key are incorrect." },
            { 0x8028002A, "The signature or encryption scheme for this key is incorrect or not permitted in this situation." },
            { 0x8028002B, "The size of the data (or blob) parameter is bad or inconsistent with the referenced key." },
            { 0x8028002C, "A mode parameter is bad, such as capArea or subCapArea for TPM_GetCapability, phsicalPresence parameter for TPM_PhysicalPresence, or migrationType for TPM_CreateMigrationBlob." },
            { 0x8028002D, "Either the physicalPresence or physicalPresenceLock bits have the wrong value." },
            { 0x8028002E, "The TPM cannot perform this version of the capability." },
            { 0x8028002F, "The TPM does not allow for wrapped transport sessions." },
            { 0x80280030, "TPM audit construction failed and the underlying command was returning a failure code also." },
            { 0x80280031, "TPM audit construction failed and the underlying command was returning success." },
            { 0x80280032, "Attempt to reset a PCR register that does not have the resettable attribute." },
            { 0x80280033, "Attempt to reset a PCR register that requires locality and locality modifier not part of command transport." },
            { 0x80280034, "Make identity blob not properly typed." },
            { 0x80280035, "When saving context identified resource type does not match actual resource." },
            { 0x80280036, "The TPM is attempting to execute a command only available when in FIPS mode." },
            { 0x80280037, "The command is attempting to use an invalid family ID." },
            { 0x80280038, "The permission to manipulate the NV storage is not available." },
            { 0x80280039, "The operation requires a signed command." },
            { 0x8028003A, "Wrong operation to load an NV key." },
            { 0x8028003B, "NV_LoadKey blob requires both owner and blob authorization." },
            { 0x8028003C, "The NV area is locked and not writtable." },
            { 0x8028003D, "The locality is incorrect for the attempted operation." },
            { 0x8028003E, "The NV area is read only and can't be written to." },
            { 0x8028003F, "There is no protection on the write to the NV area." },
            { 0x80280040, "The family count value does not match." },
            { 0x80280041, "The NV area has already been written to." },
            { 0x80280042, "The NV area attributes conflict." },
            { 0x80280043, "The structure tag and version are invalid or inconsistent." },
            { 0x80280044, "The key is under control of the TPM Owner and can only be evicted by the TPM Owner." },
            { 0x80280045, "The counter handle is incorrect." },
            { 0x80280046, "The write is not a complete write of the area." },
            { 0x80280047, "The gap between saved context counts is too large." },
            { 0x80280048, "The maximum number of NV writes without an owner has been exceeded." },
            { 0x80280049, "No operator AuthData value is set." },
            { 0x8028004A, "The resource pointed to by context is not loaded." },
            { 0x8028004B, "The delegate administration is locked." },
            { 0x8028004C, "Attempt to manage a family other than the delegated family." },
            { 0x8028004D, "Delegation table management not enabled." },
            { 0x8028004E, "There was a command executed outside of an exclusive transport session." },
            { 0x8028004F, "Attempt to context save a owner evict controlled key." },
            { 0x80280050, "The DAA command has no resources availble to execute the command." },
            { 0x80280051, "The consistency check on DAA parameter inputData0 has failed." },
            { 0x80280052, "The consistency check on DAA parameter inputData1 has failed." },
            { 0x80280053, "The consistency check on DAA_issuerSettings has failed." },
            { 0x80280054, "The consistency check on DAA_tpmSpecific has failed." },
            { 0x80280055, "The atomic process indicated by the submitted DAA command is not the expected process." },
            { 0x80280056, "The issuer's validity check has detected an inconsistency." },
            { 0x80280057, "The consistency check on w has failed." },
            { 0x80280058, "The handle is incorrect." },
            { 0x80280059, "Delegation is not correct." },
            { 0x8028005A, "The context blob is invalid." },
            { 0x8028005B, "Too many contexts held by the TPM." },
            { 0x8028005C, "Migration authority signature validation failure." },
            { 0x8028005D, "Migration destination not authenticated." },
            { 0x8028005E, "Migration source incorrect." },
            { 0x8028005F, "Incorrect migration authority." },
            { 0x80280061, "Attempt to revoke the EK and the EK is not revocable." },
            { 0x80280062, "Bad signature of CMK ticket." },
            { 0x80280063, "There is no room in the context list for additional contexts." },
            { 0x80280400, "The command was blocked." },
            { 0x80280401, "The specified handle was not found." },
            { 0x80280402, "The TPM returned a duplicate handle and the command needs to be resubmitted." },
            { 0x80280403, "The command within the transport was blocked." },
            { 0x80280404, "The command within the transport is not supported." },
            { 0x80280800, "The TPM is too busy to respond to the command immediately, but the command could be resubmitted at a later time." },
            { 0x80280801, "SelfTestFull has not been run." },
            { 0x80280802, "The TPM is currently executing a full selftest." },
            { 0x80280803, "The TPM is defending against dictionary attacks and is in a time-out period." },
            { 0x80284001, "An internal software error has been detected." },
            { 0x80284002, "One or more input parameters is bad." },
            { 0x80284003, "A specified output pointer is bad." },
            { 0x80284004, "The specified context handle does not refer to a valid context." },
            { 0x80284005, "A specified output buffer is too small." },
            { 0x80284006, "An error occurred while communicating with the TPM." },
            { 0x80284007, "One or more context parameters is invalid." },
            { 0x80284008, "The TBS service is not running and could not be started." },
            { 0x80284009, "A new context could not be created because there are too many open contexts." },
            { 0x8028400A, "A new virtual resource could not be created because there are too many open virtual resources." },
            { 0x8028400B, "The TBS service has been started but is not yet running." },
            { 0x8028400C, "The physical presence interface is not supported." },
            { 0x8028400D, "The command was canceled." },
            { 0x8028400E, "The input or output buffer is too large." },
            { 0x8028400F, "A compatible Trusted Platform Module (TPM) Security Device cannot be found on this computer." },
            { 0x80284010, "The TBS service has been disabled." },
            { 0x80284011, "No TCG event log is available." },
            { 0x80284012, "The caller does not have the appropriate rights to perform the requested operation." },
            { 0x80284013, "The TPM provisioning action is not allowed by the specified flags. For provisioning to be successful, one of several actions may be required. The TPM management console (tpm.msc) action to make the TPM Ready may help. For further information, see the documentation for the Win32_Tpm WMI method 'Provision'. (The actions that may be required include importing the TPM Owner Authorization value into the system, calling the Win32_Tpm WMI method for provisioning the TPM and specifying TRUE for either 'ForceClear_Allowed' or 'PhysicalPresencePrompts_Allowed' (as indicated by the value returned in the Additional Information), or enabling the TPM in the system BIOS.)"},
            { 0x80284014, "The Physical Presence Interface of this firmware does not support the requested method." },
            { 0x80284015, "The requested TPM OwnerAuth value was not found." },
            { 0x80284016, "The TPM provisioning did not complete. For more information on completing the provisioning, call the Win32_Tpm WMI method for provisioning the TPM ('Provision') and check the returned Information." },
            { 0x80290100, "The command buffer is not in the correct state." },
            { 0x80290101, "The command buffer does not contain enough data to satisfy the request." },
            { 0x80290102, "The command buffer cannot contain any more data." },
            { 0x80290103, "One or more output parameters was NULL or invalid." },
            { 0x80290104, "One or more input parameters is invalid." },
            { 0x80290105, "Not enough memory was available to satisfy the request." },
            { 0x80290106, "The specified buffer was too small." },
            { 0x80290107, "An internal error was detected." },
            { 0x80290108, "The caller does not have the appropriate rights to perform the requested operation." },
            { 0x80290109, "The specified authorization information was invalid." },
            { 0x8029010A, "The specified context handle was not valid." },
            { 0x8029010B, "An error occurred while communicating with the TBS." },
            { 0x8029010C, "The TPM returned an unexpected result." },
            { 0x8029010D, "The message was too large for the encoding scheme." },
            { 0x8029010E, "The encoding in the blob was not recognized." },
            { 0x8029010F, "The key size is not valid." },
            { 0x80290110, "The encryption operation failed." },
            { 0x80290111, "The key parameters structure was not valid."},
            { 0x80290112, "The requested supplied data does not appear to be a valid migration authorization blob." },
            { 0x80290113, "The specified PCR index was invalid."},
            { 0x80290114, "The data given does not appear to be a valid delegate blob." },
            { 0x80290115, "One or more of the specified context parameters was not valid." },
            { 0x80290116, "The data given does not appear to be a valid key blob."},
            { 0x80290117, "The specified PCR data was invalid." },
            { 0x80290118, "The format of the owner auth data was invalid." },
            { 0x80290119, "The random number generated did not pass FIPS RNG check." },
            { 0x8029011A, "The TCG Event Log does not contain any data." },
            { 0x8029011B, "An entry in the TCG Event Log was invalid." },
            { 0x8029011C, "A TCG Separator was not found." },
            { 0x8029011D, "A digest value in a TCG Log entry did not match hashed data." },
            { 0x8029011E, "The requested operation was blocked by current TPM policy. Please contact your system administrator for assistance." },
            { 0x80290200, "The specified buffer was too small." },
            { 0x80290201, "The context could not be cleaned up." },
            { 0x80290202, "The specified context handle is invalid." },
            { 0x80290203, "An invalid context parameter was specified." },
            { 0x80290204, "An error occurred while communicating with the TPM."},
            { 0x80290205, "No entry with the specified key was found." },
            { 0x80290206, "The specified virtual handle matches a virtual handle already in use." },
            { 0x80290207, "The pointer to the returned handle location was NULL or invalid."},
            { 0x80290208, "One or more parameters is invalid." },
            { 0x80290209, "The RPC subsystem could not be initialized." },
            { 0x8029020A, "The TBS scheduler is not running." },
            { 0x8029020B, "The command was canceled." },
            { 0x8029020C, "There was not enough memory to fulfill the request." },
            { 0x8029020D, "The specified list is empty, or the iteration has reached the end of the list." },
            { 0x8029020E, "The specified item was not found in the list." },
            { 0x8029020F, "The TPM does not have enough space to load the requested resource." },
            { 0x80290210, "There are too many TPM contexts in use." },
            { 0x80290211, "The TPM command failed." },
            { 0x80290212, "The TBS does not recognize the specified ordinal." },
            { 0x80290213, "The requested resource is no longer available." },
            { 0x80290214, "The resource type did not match." },
            { 0x80290215, "No resources can be unloaded." },
            { 0x80290216, "No new entries can be added to the hash table." },
            { 0x80290217, "A new TBS context could not be created because there are too many open contexts." },
            { 0x80290218, "A new virtual resource could not be created because there are too many open virtual resources." },
            { 0x80290219, "The physical presence interface is not supported." },
            { 0x8029021A, "TBS is not compatible with the version of TPM found on the system." },
            { 0x8029021B, "No TCG event log is available." },
            { 0x80290300, "A general error was detected when attempting to acquire the BIOS's response to a Physical Presence command." },
            { 0x80290301, "The user failed to confirm the TPM operation request." },
            { 0x80290302, "The BIOS failure prevented the successful execution of the requested TPM operation (e.g. invalid TPM operation request, BIOS communication error with the TPM)." },
            { 0x80290303, "The BIOS does not support the physical presence interface." },
            { 0x80290304, "The Physical Presence command was blocked by current BIOS settings. The system owner may be able to reconfigure the BIOS settings to allow the command." },
            { 0x80290400, "This is an error mask to convert Platform Crypto Provider errors to win errors." },
            { 0x80290401, "The Platform Crypto Device is currently not ready. It needs to be fully provisioned to be operational." },
            { 0x80290402, "The handle provided to the Platform Crypto Provider is invalid." },
            { 0x80290403, "A parameter provided to the Platform Crypto Provider is invalid." },
            { 0x80290404, "A provided flag to the Platform Crypto Provider is not supported." },
            { 0x80290405, "The requested operation is not supported by this Platform Crypto Provider." },
            { 0x80290406, "The buffer is too small to contain all data. No information has been written to the buffer." },
            { 0x80290407, "An unexpected internal error has occurred in the Platform Crypto Provider." },
            { 0x80290408, "The authorization to use a provider object has failed." },
            { 0x80290409, "The Platform Crypto Device has ignored the authorization for the provider object, to mitigate against a dictionary attack." },
            { 0x8029040A, "The referenced policy was not found." },
            { 0x8029040B, "The referenced profile was not found." },
            { 0x8029040C, "The validation was not successful." },
        };

        private string DecodeWindowsError(uint input)
        {
            string output = "Windows Error Code:\n";

            string name = Enum.GetName(typeof(TPM_WIN_ERROR), input);
            if (string.IsNullOrEmpty(name))
            {
                output += " Unknown Windows error code.\n";
            }
            else
            { 
                output += " " + name + "\n ";
                if ((input & (uint)TPM_WIN_ERROR.STATUS_TPM_ERROR_MASK) == (uint)TPM_WIN_ERROR.STATUS_TPM_ERROR_MASK)
                {
                    input = (input ^ (uint)TPM_WIN_ERROR.STATUS_TPM_ERROR_MASK) | (uint)TPM_WIN_ERROR.TPM_E_ERROR_MASK;
                }
                string description;
                if (TPM_WIN_ERROR_DESC.TryGetValue(input, out description))
                {
                    output += " " + description;
                }
            }
            return output;
        }
    }
}
