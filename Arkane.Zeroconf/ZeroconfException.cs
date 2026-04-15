#region header

// Arkane.ZeroConf - ZeroconfException.cs

#endregion

#region using

using System;

using JetBrains.Annotations;

#endregion

namespace ArkaneSystems.Arkane.Zeroconf;

/// <summary>
/// Base exception for all Zeroconf/mDNS provider errors.
/// </summary>
/// <remarks>
/// <para>
/// This is the base exception class for errors originating from the Zeroconf library.
/// Applications should catch this exception to handle mDNS/Bonjour-related errors uniformly.
/// More specific exceptions may derive from this base class to provide additional context
/// about particular failure scenarios (e.g., <see cref="ServiceErrorException"/> for DNS-SD service errors).
/// </para>
/// </remarks>
[PublicAPI]
public class ZeroconfException : Exception
{
  /// <summary>
  /// Initializes a new instance of the <see cref="ZeroconfException"/> class.
  /// </summary>
  public ZeroconfException () { }

  /// <summary>
  /// Initializes a new instance of the <see cref="ZeroconfException"/> class with a specified error message.
  /// </summary>
  /// <param name="message">The message that describes the error.</param>
  public ZeroconfException (string? message) : base (message) { }

  /// <summary>
  /// Initializes a new instance of the <see cref="ZeroconfException"/> class with a specified error message
  /// and a reference to the inner exception that is the cause of this exception.
  /// </summary>
  /// <param name="message">The message that describes the error.</param>
  /// <param name="innerException">The exception that is the cause of the current exception.</param>
  public ZeroconfException (string? message, Exception? innerException) : base (message, innerException) { }
}
