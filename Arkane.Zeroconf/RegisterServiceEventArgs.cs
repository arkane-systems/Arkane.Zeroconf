#region header

// Arkane.ZeroConf - RegisterServiceEventArgs.cs

#endregion

#region using

using System;

#endregion

namespace ArkaneSystems.Arkane.Zeroconf;

public class RegisterServiceEventArgs : EventArgs
{
  public RegisterServiceEventArgs () { }

  public RegisterServiceEventArgs (IRegisterService service, bool isRegistered, ServiceErrorCode error)
  {
    this.Service      = service;
    this.IsRegistered = isRegistered;
    this.ServiceError = error;
  }

  private IRegisterService? service;

  public IRegisterService Service
  {
    get => this.service ?? throw new InvalidOperationException ("Service was not assigned.");
    set => this.service = value ?? throw new ArgumentNullException (nameof (value));
  }

  public bool IsRegistered { get; set; }

  public ServiceErrorCode ServiceError { get; set; }
}
