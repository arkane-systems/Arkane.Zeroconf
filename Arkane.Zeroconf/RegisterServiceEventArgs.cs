#region header

// Arkane.ZeroConf - RegisterServiceEventArgs.cs

#endregion

#region using

using System;

using JetBrains.Annotations;

#endregion

namespace ArkaneSystems.Arkane.Zeroconf;

/// <summary>
///   Provides data for service registration response events.
/// </summary>
[PublicAPI]
public class RegisterServiceEventArgs : EventArgs
{
  /// <summary>
  ///   Initializes a new instance of the <see cref="RegisterServiceEventArgs" /> class.
  /// </summary>
  public RegisterServiceEventArgs () { }

  /// <summary>
  ///   Initializes a new instance of the <see cref="RegisterServiceEventArgs" /> class.
  /// </summary>
  /// <param name="service">The service associated with the registration response.</param>
  /// <param name="isRegistered"><see langword="true" /> when registration succeeded; otherwise, <see langword="false" />.</param>
  /// <param name="error">The provider error code associated with the response.</param>
  public RegisterServiceEventArgs (IRegisterService service, bool isRegistered, ServiceErrorCode error)
  {
    this.Service      = service;
    this.IsRegistered = isRegistered;
    this.ServiceError = error;
  }

  private IRegisterService? service;

  /// <summary>
  ///   Gets or sets the service associated with the registration response.
  /// </summary>
  public IRegisterService Service
  {
    get => this.service ?? throw new InvalidOperationException ("Service was not assigned.");
    set => this.service = value ?? throw new ArgumentNullException (nameof (value));
  }

  /// <summary>
  ///   Gets or sets a value indicating whether the service was successfully registered.
  /// </summary>
  public bool IsRegistered { get; set; }

  /// <summary>
  ///   Gets or sets the provider-reported service error code.
  /// </summary>
  public ServiceErrorCode ServiceError { get; set; }
}
