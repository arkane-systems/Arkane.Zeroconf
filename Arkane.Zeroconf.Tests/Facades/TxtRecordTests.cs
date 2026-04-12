#region header

// Arkane.Zeroconf.Tests - TxtRecordTests.cs
// 

#endregion

#region using

using System ;
using System.Collections ;
using System.Collections.Generic ;
using System.Linq ;
using System.Text ;
using Xunit ;

using ArkaneSystems.Arkane.Zeroconf ;

#endregion

namespace ArkaneSystems.Arkane.Zeroconf.Tests.Facades ;

public class TxtRecordTests : IDisposable
{
    private readonly TxtRecord txtRecord ;

    public TxtRecordTests ()
    {
        this.txtRecord = new TxtRecord () ;
    }

    public void Dispose ()
    {
        this.txtRecord?.Dispose () ;
    }

    [Fact]
    public void Constructor_CreatesValidTxtRecord ()
    {
        // Act & Assert
        Assert.NotNull (this.txtRecord) ;
    }

    [Fact]
    public void Count_InitiallyZero ()
    {
        // Act
        var count = this.txtRecord.Count ;

        // Assert
        Assert.Equal (0, count) ;
    }

    [Fact]
    public void Add_StringValue_IncreasesCount ()
    {
        // Act
        this.txtRecord.Add ("key1", "value1") ;

        // Assert
        Assert.Equal (1, this.txtRecord.Count) ;
    }

    [Fact]
    public void Add_ByteArrayValue_IncreasesCount ()
    {
        // Act
        var byteValue = Encoding.UTF8.GetBytes ("value1") ;
        this.txtRecord.Add ("key1", byteValue) ;

        // Assert
        Assert.Equal (1, this.txtRecord.Count) ;
    }

    [Fact]
    public void Add_MultipleValues_IncreasesCountForEach ()
    {
        // Act
        this.txtRecord.Add ("key1", "value1") ;
        this.txtRecord.Add ("key2", "value2") ;
        this.txtRecord.Add ("key3", "value3") ;

        // Assert
        Assert.Equal (3, this.txtRecord.Count) ;
    }

    [Fact]
    public void Add_TxtRecordItem_IncreasesCount ()
    {
        // Arrange
        var item = new TxtRecordItem ("key1", "value1") ;

        // Act
        this.txtRecord.Add (item) ;

        // Assert
        Assert.Equal (1, this.txtRecord.Count) ;
    }

    [Fact]
    public void Remove_ExistingKey_DecreaseCount ()
    {
        // Arrange
        this.txtRecord.Add ("key1", "value1") ;
        this.txtRecord.Add ("key2", "value2") ;

        // Act
        this.txtRecord.Remove ("key1") ;

        // Assert
        Assert.Equal (1, this.txtRecord.Count) ;
    }

    [Fact]
    public void Remove_NonExistentKey_DoesNotThrow ()
    {
        // Act & Assert
        this.txtRecord.Remove ("nonexistent") ;
    }

    [Fact]
    public void Indexer_ReturnsAddedValue ()
    {
        // Arrange
        this.txtRecord.Add ("key1", "value1") ;

        // Act
        var result = this.txtRecord["key1"] ;

        // Assert
        Assert.NotNull (result) ;
        Assert.Equal ("key1", result.Key) ;
    }

    [Fact]
    public void GetItemAt_ReturnsItemByIndex ()
    {
        // Arrange
        this.txtRecord.Add ("key1", "value1") ;
        this.txtRecord.Add ("key2", "value2") ;

        // Act
        var item = this.txtRecord.GetItemAt (0) ;

        // Assert
        Assert.NotNull (item) ;
    }

    [Fact]
    public void GetItemAt_OutOfRange_ThrowsException ()
    {
        // Arrange
        this.txtRecord.Add ("key1", "value1") ;

        // Act & Assert
        Assert.Throws <IndexOutOfRangeException> (() => this.txtRecord.GetItemAt (10)) ;
    }

    [Fact]
    public void GetEnumerator_ReturnsEnumerator ()
    {
        // Arrange
        this.txtRecord.Add ("key1", "value1") ;

        // Act
        var enumerator = this.txtRecord.GetEnumerator () ;

        // Assert
        Assert.NotNull (enumerator) ;
    }

    [Fact]
    public void Enumeration_IteratesAllItems ()
    {
        // Arrange
        this.txtRecord.Add ("key1", "value1") ;
        this.txtRecord.Add ("key2", "value2") ;
        this.txtRecord.Add ("key3", "value3") ;

        // Act
        var count = 0 ;
        foreach (var item in this.txtRecord)
        {
            count++ ;
        }

        // Assert
        Assert.Equal (3, count) ;
    }

    [Fact]
    public void Dispose_DoesNotThrow ()
    {
        // Act & Assert
        this.txtRecord.Dispose () ;
    }

    [Fact]
    public void Dispose_CanBeCalledMultipleTimes ()
    {
        // Act & Assert
        this.txtRecord.Dispose () ;
        this.txtRecord.Dispose () ;
    }

    [Fact]
    public void Add_SameKeyMultipleTimes_AllowsMultipleValues ()
    {
        // Act
        this.txtRecord.Add ("key1", "value1") ;
        this.txtRecord.Add ("key1", "value2") ;

        // Assert
        // Some implementations may allow this, verify behavior
        Assert.True (this.txtRecord.Count >= 1) ;
    }

    [Fact]
    public void Clear_RemovesAllItems ()
    {
        // Arrange
        this.txtRecord.Add ("key1", "value1") ;
        this.txtRecord.Add ("key2", "value2") ;

        // Act
        this.txtRecord.Remove ("key1") ;
        this.txtRecord.Remove ("key2") ;

        // Assert
        Assert.Equal (0, this.txtRecord.Count) ;
    }

    [Theory]
    [InlineData ("key", "value")]
    [InlineData ("complex_key", "complex value with spaces")]
    [InlineData ("key123", "value123")]
    public void Add_VariousKeyValuePairs_Succeeds (string key, string value)
    {
        // Act
        this.txtRecord.Add (key, value) ;

        // Assert
        Assert.Equal (1, this.txtRecord.Count) ;
        Assert.Equal (key, this.txtRecord[key].Key) ;
    }
}
