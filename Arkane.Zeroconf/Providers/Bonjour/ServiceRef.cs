#region header

// Arkane.Zeroconf - ServiceRef.cs
// 

#endregion

#region using

using System ;

#endregion

namespace ArkaneSystems.Arkane.Zeroconf.Providers.Bonjour
{
    public struct ServiceRef
    {
        public static readonly ServiceRef Zero ;

        public ServiceRef (IntPtr raw) { this.Raw = raw ; }

        public void Deallocate () { Native.DNSServiceRefDeallocate (this.Raw) ; }

        public ServiceError ProcessSingle () { return Native.DNSServiceProcessResult (this.Raw) ; }

        public void Process ()
        {
            while (this.ProcessSingle () == ServiceError.NoError)
                ;
        }

        public int SocketFD { get { return Native.DNSServiceRefSockFD (this.Raw) ; } }

        public IntPtr Raw { get ; }

        public override bool Equals (object o)
        {
            if (!(o is ServiceRef))
                return false ;

            return ((ServiceRef) o).Raw == this.Raw ;
        }

        public override int GetHashCode () { return this.Raw.GetHashCode () ; }

        public static bool operator == (ServiceRef a, ServiceRef b) { return a.Raw == b.Raw ; }

        public static bool operator != (ServiceRef a, ServiceRef b) { return a.Raw != b.Raw ; }

        public static explicit operator IntPtr (ServiceRef value) { return value.Raw ; }

        public static explicit operator ServiceRef (IntPtr value) { return new ServiceRef (value) ; }
    }
}
