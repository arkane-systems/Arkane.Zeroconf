#region header

// Arkane.Zeroconf.Tests - ValueObjectTests.cs
// 

#endregion

#region using

using System ;
using Xunit ;

using ArkaneSystems.Arkane.Zeroconf ;
using ArkaneSystems.Arkane.Zeroconf.Providers.Bonjour ;

#endregion

namespace ArkaneSystems.Arkane.Zeroconf.Tests.ValueObjects ;

public class AddressProtocolTests
{
    [Fact]
    public void AddressProtocol_IPv4_ValueExists ()
    {
        // Assert
        Assert.True (Enum.IsDefined (typeof (AddressProtocol), AddressProtocol.IPv4)) ;
    }

    [Fact]
    public void AddressProtocol_IPv6_ValueExists ()
    {
        // Assert
        Assert.True (Enum.IsDefined (typeof (AddressProtocol), AddressProtocol.IPv6)) ;
    }

    [Fact]
    public void AddressProtocol_Any_ValueExists ()
    {
        // Assert
        Assert.True (Enum.IsDefined (typeof (AddressProtocol), AddressProtocol.Any)) ;
    }
}

public class ServiceTypeTests
{
    [Fact]
    public void ServiceType_HasValidValues ()
    {
        // Assert - ServiceType is an enum in Bonjour provider
        Assert.True (Enum.GetValues (typeof (ServiceType)).Length > 0) ;
    }

    [Fact]
    public void ServiceType_A_ValueExists ()
    {
        // Assert
        Assert.Equal (1, (ushort) ServiceType.A) ;
    }

    [Fact]
    public void ServiceType_MX_ValueExists ()
    {
        // Assert
        Assert.Equal (15, (ushort) ServiceType.MX) ;
    }

    [Fact]
    public void ServiceType_TXT_ValueExists ()
    {
        // Assert
        Assert.Equal (16, (ushort) ServiceType.TXT) ;
    }
}

public class ServiceClassTests
{
    [Fact]
    public void ServiceClass_HasValidValues ()
    {
        // Assert - ServiceClass is an enum in Bonjour provider
        Assert.True (Enum.GetValues (typeof (ServiceClass)).Length > 0) ;
    }

    [Fact]
    public void ServiceClass_IN_ValueExists ()
    {
        // Assert
        Assert.Equal (1, (ushort) ServiceClass.IN) ;
    }
}

public class ServiceErrorTests
{
    [Fact]
    public void ServiceErrorCode_NoError_ValueExists ()
    {
        // Assert
        Assert.True (Enum.IsDefined (typeof (ServiceErrorCode), ServiceErrorCode.None)) ;
    }

    [Fact]
    public void ServiceErrorCode_HasValidValues ()
    {
        // Assert
        Assert.True (Enum.GetValues (typeof (ServiceErrorCode)).Length > 0) ;
    }

    [Fact]
    public void ServiceErrorCode_Unknown_ValueExists ()
    {
        // Assert
        Assert.True (Enum.IsDefined (typeof (ServiceErrorCode), ServiceErrorCode.Unknown)) ;
    }
}

public class TxtRecordItemTests
{
    [Fact]
    public void TxtRecordItem_CanBeCreatedWithStringValue ()
    {
        // Act
        var item = new TxtRecordItem ("key", "value") ;

        // Assert
        Assert.NotNull (item) ;
        Assert.Equal ("key", item.Key) ;
    }

    [Fact]
    public void TxtRecordItem_StringConstructor_StoresKey ()
    {
        // Act
        var item = new TxtRecordItem ("mykey", "myvalue") ;

        // Assert
        Assert.Equal ("mykey", item.Key) ;
    }

    [Fact]
    public void TxtRecordItem_ByteArrayConstructor_CanBeCreated ()
    {
        // Arrange
        var bytes = new byte[] { 0x01, 0x02, 0x03 } ;

        // Act
        var item = new TxtRecordItem ("key", bytes) ;

        // Assert
        Assert.NotNull (item) ;
    }

    [Fact]
    public void TxtRecordItem_WithDifferentKeys_AreNotEqual ()
    {
        // Act
        var item1 = new TxtRecordItem ("key1", "value") ;
        var item2 = new TxtRecordItem ("key2", "value") ;

        // Assert
        Assert.NotEqual (item1, item2) ;
    }
}

public class ServiceFlagsTests
{
    [Fact]
    public void ServiceFlags_HasValidValues ()
    {
        // Assert
        Assert.True (Enum.GetValues (typeof (ServiceFlags)).Length > 0) ;
    }
}
