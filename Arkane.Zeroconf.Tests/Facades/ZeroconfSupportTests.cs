#region header

// Arkane.Zeroconf.Tests - ZeroconfSupportTests.cs
// 

#endregion

#region using

using Xunit ;

using ArkaneSystems.Arkane.Zeroconf ;

#endregion

namespace ArkaneSystems.Arkane.Zeroconf.Tests.Facades ;

public class ZeroconfSupportTests
{
    [Fact]
    public void Capabilities_AlwaysIncludeBrowse ()
    {
        Assert.True ((ZeroconfSupport.Capabilities & ZeroconfCapability.Browse) == ZeroconfCapability.Browse) ;
    }

    [Fact]
    public void CanPublish_MatchesPublishFlag ()
    {
        Assert.Equal ((ZeroconfSupport.Capabilities & ZeroconfCapability.Publish) == ZeroconfCapability.Publish,
                      ZeroconfSupport.CanPublish) ;
    }
}
