#region header

// Arkane.Zeroconf.Tests - ZeroconfSupportTests.cs

#endregion

#region using

using ArkaneSystems.Arkane.Zeroconf.Tests.Helpers;

using Xunit;

#endregion

namespace ArkaneSystems.Arkane.Zeroconf.Tests.Facades;

public class ZeroconfSupportTests
{
  [Fact]
  public void Capabilities_AlwaysIncludeBrowse ()
  {
    ZeroconfTestHelper.SkipIfNoProvider ();
    Assert.True ((ZeroconfSupport.Capabilities & ZeroconfCapability.Browse) == ZeroconfCapability.Browse);
  }

  [Fact]
  public void CanPublish_MatchesPublishFlag ()
  {
    ZeroconfTestHelper.SkipIfNoProvider ();
    Assert.Equal (expected: (ZeroconfSupport.Capabilities & ZeroconfCapability.Publish) == ZeroconfCapability.Publish,
                  actual: ZeroconfSupport.CanPublish);
  }
}
