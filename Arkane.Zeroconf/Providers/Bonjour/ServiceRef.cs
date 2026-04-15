#region header

// Arkane.Zeroconf - ServiceRef.cs

#endregion

#region using

using System;
using System.Threading;

using JetBrains.Annotations;

#endregion

namespace ArkaneSystems.Arkane.Zeroconf.Providers.Bonjour;

public readonly struct ServiceRef (IntPtr raw)
{
  public static readonly ServiceRef Zero = new (IntPtr.Zero);

  public void Deallocate () => Native.DNSServiceRefDeallocate (this.Raw);

  public ServiceError ProcessSingle () => Native.DNSServiceProcessResult (this.Raw);

  public void Process () => this.Process (CancellationToken.None);

  public void Process (CancellationToken cancellationToken)
  {
    cancellationToken.ThrowIfCancellationRequested ();
    while (this.ProcessSingle () == ServiceError.NoError)
      cancellationToken.ThrowIfCancellationRequested ();
  }

  public int SocketFD => Native.DNSServiceRefSockFD (this.Raw);

  public IntPtr Raw { get; } = raw;

  public override bool Equals (object? o) => o is ServiceRef serviceRef && (serviceRef.Raw == this.Raw);

  public override int GetHashCode () => this.Raw.GetHashCode ();

  public static bool operator == (ServiceRef a, ServiceRef b) => a.Raw == b.Raw;

  public static bool operator != (ServiceRef a, ServiceRef b) => a.Raw != b.Raw;

  public static explicit operator IntPtr (ServiceRef value) => value.Raw;

  public static explicit operator ServiceRef (IntPtr value) => new (value);
}
