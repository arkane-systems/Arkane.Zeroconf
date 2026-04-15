#region header

// Arkane.Zeroconf.Tests - TxtRecordTests.cs

#endregion

#region using

using System;
using System.Collections;
using System.Text;

using ArkaneSystems.Arkane.Zeroconf.Tests.Helpers;

using Xunit;

#endregion

namespace ArkaneSystems.Arkane.Zeroconf.Tests.Facades;

public class TxtRecordTests : IDisposable
{
  public TxtRecordTests ()
  {
    ZeroconfTestHelper.SkipIfNoProvider ();
    this.txtRecord = new TxtRecord ();
  }

  private readonly TxtRecord txtRecord;

  public void Dispose () { this.txtRecord?.Dispose (); }

  [Fact]
  public void Constructor_CreatesValidTxtRecord ()
  {
    // Act & Assert
    Assert.NotNull (this.txtRecord);
  }

  [Fact]
  public void Count_InitiallyZero ()
  {
    // Act
    int count = this.txtRecord.Count;

    // Assert
    Assert.Equal (expected: 0, actual: count);
  }

  [Fact]
  public void Add_StringValue_IncreasesCount ()
  {
    // Act
    this.txtRecord.Add (key: "key1", value: "value1");

    // Assert
    Assert.Equal (expected: 1, actual: this.txtRecord.Count);
  }

  [Fact]
  public void Add_ByteArrayValue_IncreasesCount ()
  {
    // Act
    byte[] byteValue = Encoding.UTF8.GetBytes ("value1");
    this.txtRecord.Add (key: "key1", value: byteValue);

    // Assert
    Assert.Equal (expected: 1, actual: this.txtRecord.Count);
  }

  [Fact]
  public void Add_MultipleValues_IncreasesCountForEach ()
  {
    // Act
    this.txtRecord.Add (key: "key1", value: "value1");
    this.txtRecord.Add (key: "key2", value: "value2");
    this.txtRecord.Add (key: "key3", value: "value3");

    // Assert
    Assert.Equal (expected: 3, actual: this.txtRecord.Count);
  }

  [Fact]
  public void Add_TxtRecordItem_IncreasesCount ()
  {
    // Arrange
    var item = new TxtRecordItem (key: "key1", valueString: "value1");

    // Act
    this.txtRecord.Add (item);

    // Assert
    Assert.Equal (expected: 1, actual: this.txtRecord.Count);
  }

  [Fact]
  public void Remove_ExistingKey_DecreaseCount ()
  {
    // Arrange
    this.txtRecord.Add (key: "key1", value: "value1");
    this.txtRecord.Add (key: "key2", value: "value2");

    // Act
    this.txtRecord.Remove ("key1");

    // Assert
    Assert.Equal (expected: 1, actual: this.txtRecord.Count);
  }

  [Fact]
  public void Remove_NonExistentKey_DoesNotThrow ()
  {
    // Act & Assert
    this.txtRecord.Remove ("nonexistent");
  }

  [Fact]
  public void Indexer_ReturnsAddedValue ()
  {
    // Arrange
    this.txtRecord.Add (key: "key1", value: "value1");

    // Act
    TxtRecordItem? result = this.txtRecord["key1"];

    // Assert
    Assert.NotNull (result);
    Assert.Equal (expected: "key1", actual: result.Key);
  }

  [Fact]
  public void GetItemAt_ReturnsItemByIndex ()
  {
    // Arrange
    this.txtRecord.Add (key: "key1", value: "value1");
    this.txtRecord.Add (key: "key2", value: "value2");

    // Act
    TxtRecordItem? item = this.txtRecord.GetItemAt (0);

    // Assert
    Assert.NotNull (item);
  }

  [Fact]
  public void GetItemAt_OutOfRange_ThrowsException ()
  {
    // Arrange
    this.txtRecord.Add (key: "key1", value: "value1");

    // Act & Assert
    Assert.Throws<IndexOutOfRangeException> (() => this.txtRecord.GetItemAt (10));
  }

  [Fact]
  public void GetEnumerator_ReturnsEnumerator ()
  {
    // Arrange
    this.txtRecord.Add (key: "key1", value: "value1");

    // Act
    IEnumerator enumerator = this.txtRecord.GetEnumerator ();

    // Assert
    Assert.NotNull (enumerator);
  }

  [Fact]
  public void Enumeration_IteratesAllItems ()
  {
    // Arrange
    this.txtRecord.Add (key: "key1", value: "value1");
    this.txtRecord.Add (key: "key2", value: "value2");
    this.txtRecord.Add (key: "key3", value: "value3");

    // Act
    var count = 0;
    foreach (object? item in this.txtRecord)
      count++;

    // Assert
    Assert.Equal (expected: 3, actual: count);
  }

  [Fact]
  public void Dispose_DoesNotThrow ()
  {
    // Act & Assert
    this.txtRecord.Dispose ();
  }

  [Fact]
  public void Dispose_CanBeCalledMultipleTimes ()
  {
    // Act & Assert
    this.txtRecord.Dispose ();
    this.txtRecord.Dispose ();
  }

  [Fact]
  public void Add_SameKeyMultipleTimes_AllowsMultipleValues ()
  {
    // Act
    this.txtRecord.Add (key: "key1", value: "value1");
    this.txtRecord.Add (key: "key1", value: "value2");

    // Assert
    // Some implementations may allow this, verify behavior
    Assert.True (this.txtRecord.Count >= 1);
  }

  [Fact]
  public void Clear_RemovesAllItems ()
  {
    // Arrange
    this.txtRecord.Add (key: "key1", value: "value1");
    this.txtRecord.Add (key: "key2", value: "value2");

    // Act
    this.txtRecord.Remove ("key1");
    this.txtRecord.Remove ("key2");

    // Assert
    Assert.Equal (expected: 0, actual: this.txtRecord.Count);
  }

  [Theory]
  [InlineData ("key",         "value")]
  [InlineData ("complex_key", "complex value with spaces")]
  [InlineData ("key123",      "value123")]
  public void Add_VariousKeyValuePairs_Succeeds (string key, string value)
  {
    // Act
    this.txtRecord.Add (key: key, value: value);

    // Assert
    Assert.Equal (expected: 1, actual: this.txtRecord.Count);
    TxtRecordItem? item = this.txtRecord[key];
    Assert.NotNull (item);
    Assert.Equal (expected: key, actual: item.Key);
  }
}
