#region header

// Arkane.ZeroConf - ServiceErrorException.cs

#endregion

#region using

using System;

#endregion

namespace ArkaneSystems.Arkane.Zeroconf.Providers.Bonjour;

/// <summary>
/// Exception thrown when a Bonjour/DNS-SD operation fails with a service error code.
/// </summary>
/// <remarks>
/// <para>
/// This exception is thrown when a low-level Bonjour/DNS-SD API call returns a non-zero error code.
/// The exception message contains the error code name (e.g., "NameConflict", "NoMemory").
/// </para>
/// </remarks>
public class ServiceErrorException : ZeroconfException
{
  /// <summary>
  /// Initializes a new instance of the <see cref="ServiceErrorException"/> class with a service error code.
  /// </summary>
  /// <param name="error">The service error code.</param>
  public ServiceErrorException (ServiceError error) : base (error.ToString ()) { }

  /// <summary>
  /// Initializes a new instance of the <see cref="ServiceErrorException"/> class with a specified error message.
  /// </summary>
  /// <param name="error">The error message.</param>
  public ServiceErrorException (string error) : base (error) { }
}
