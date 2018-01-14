#region header

// Arkane.Zeroconf - ServiceError.cs
// 

#endregion

namespace ArkaneSystems.Arkane.Zeroconf.Providers.Bonjour
{
    public enum ServiceError
    {
        NoError = 0,
        Unknown = -65537, /* 0xFFFE FFFF */
        NoSuchName = -65538,
        NoMemory = -65539,
        BadParam = -65540,
        BadReference = -65541,
        BadState = -65542,
        BadFlags = -65543,
        Unsupported = -65544,
        NotInitialized = -65545,
        AlreadyRegistered = -65547,
        NameConflict = -65548,
        Invalid = -65549,
        Firewall = -65550,
        Incompatible = -65551, /* client library incompatible with daemon */
        BadInterfaceIndex = -65552,
        Refused = -65553,
        NoSuchRecord = -65554,
        NoAuth = -65555,
        NoSuchKey = -65556,
        NatTraversal = -65557,
        DoubleNat = -65558,
        BadTime = -65559
    }
}
