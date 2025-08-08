using System.Runtime.InteropServices;

namespace LegacyServices.Users;

internal class NativeWindows : UserList
{
    [DllImport("wtsapi32.dll")]
    private static extern IntPtr WTSOpenServer([MarshalAs(UnmanagedType.LPStr)] string pServerName);

    [DllImport("wtsapi32.dll")]
    private static extern void WTSCloseServer(IntPtr hServer);

    [DllImport("wtsapi32.dll")]
    private static extern int WTSEnumerateSessions(
        IntPtr hServer,
        [MarshalAs(UnmanagedType.U4)] int Reserved,
        [MarshalAs(UnmanagedType.U4)] int Version,
        ref IntPtr ppSessionInfo,
        [MarshalAs(UnmanagedType.U4)] ref int pCount);

    [DllImport("wtsapi32.dll")]
    private static extern void WTSFreeMemory(IntPtr pMemory);

    [DllImport("wtsapi32.dll")]
    private static extern bool WTSQuerySessionInformation(
        IntPtr hServer, int sessionId, WTS_INFO_CLASS wtsInfoClass, out IntPtr ppBuffer, out uint pBytesReturned);

    [StructLayout(LayoutKind.Sequential)]
    private struct WTS_SESSION_INFO
    {
        public int SessionID;

        [MarshalAs(UnmanagedType.LPStr)]
        public string pWinStationName;

        public WTS_CONNECTSTATE_CLASS State;
    }

    private enum WTS_INFO_CLASS
    {
        WTSInitialProgram,
        WTSApplicationName,
        WTSWorkingDirectory,
        WTSOEMId,
        WTSSessionId,
        WTSUserName,
        WTSWinStationName,
        WTSDomainName,
        WTSConnectState,
        WTSClientBuildNumber,
        WTSClientName,
        WTSClientDirectory,
        WTSClientProductId,
        WTSClientHardwareId,
        WTSClientAddress,
        WTSClientDisplay,
        WTSClientProtocolType
    }

    private enum WTS_CONNECTSTATE_CLASS
    {
        WTSActive,
        WTSConnected,
        WTSConnectQuery,
        WTSShadow,
        WTSDisconnected,
        WTSIdle,
        WTSListen,
        WTSReset,
        WTSDown,
        WTSInit
    }

    public override UserInfo[] GetUsers(string serverName)
    {
        if (!OperatingSystem.IsWindows())
        {
            throw new PlatformNotSupportedException();
        }
        IntPtr serverHandle;
        List<UserInfo> ret = [];
        serverHandle = WTSOpenServer(serverName);

        try
        {
            IntPtr sessionInfoPtr = IntPtr.Zero;
            IntPtr userPtr = IntPtr.Zero;
            IntPtr domainPtr = IntPtr.Zero;
            int sessionCount = 0;
            int retVal = WTSEnumerateSessions(serverHandle, 0, 1, ref sessionInfoPtr, ref sessionCount);
            if (retVal != 0)
            {
                int dataSize = Marshal.SizeOf(typeof(WTS_SESSION_INFO));
                IntPtr currentSession = sessionInfoPtr;

                for (int i = 0; i < sessionCount; i++)
                {
                    WTS_SESSION_INFO si = Marshal.PtrToStructure<WTS_SESSION_INFO>(currentSession);
                    currentSession += dataSize;

                    WTSQuerySessionInformation(serverHandle, si.SessionID, WTS_INFO_CLASS.WTSUserName, out userPtr, out _);
                    WTSQuerySessionInformation(serverHandle, si.SessionID, WTS_INFO_CLASS.WTSDomainName, out domainPtr, out _);

                    try
                    {
                        if (userPtr == IntPtr.Zero || domainPtr == IntPtr.Zero)
                        {
                            throw new Exception("Unable to query user or domain name");
                        }
                        var username = Marshal.PtrToStringAnsi(userPtr) ?? "";
                        var domainname = Marshal.PtrToStringAnsi(domainPtr);
                        if (string.IsNullOrEmpty(username))
                        {
                            continue;
                        }

                        if (string.IsNullOrWhiteSpace(domainname) || Environment.MachineName.EqualsCI(domainname))
                        {
                            domainname = null;
                        }

                        ret.Add(new(username, domainname));
                    }
                    finally
                    {
                        WTSFreeMemory(userPtr);
                        WTSFreeMemory(domainPtr);
                    }
                }

                WTSFreeMemory(sessionInfoPtr);
            }
        }
        finally
        {
            WTSCloseServer(serverHandle);
        }
        return [.. ret];
    }
}
