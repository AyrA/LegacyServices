using System.Runtime.InteropServices;

namespace LegacyServices.Services.Users;

internal class NativeWindows : UserList
{
    [DllImport("wtsapi32.dll")]
    private static extern nint WTSOpenServer([MarshalAs(UnmanagedType.LPStr)] string pServerName);

    [DllImport("wtsapi32.dll")]
    private static extern void WTSCloseServer(nint hServer);

    [DllImport("wtsapi32.dll")]
    private static extern int WTSEnumerateSessions(
        nint hServer,
        [MarshalAs(UnmanagedType.U4)] int Reserved,
        [MarshalAs(UnmanagedType.U4)] int Version,
        ref nint ppSessionInfo,
        [MarshalAs(UnmanagedType.U4)] ref int pCount);

    [DllImport("wtsapi32.dll")]
    private static extern void WTSFreeMemory(nint pMemory);

    [DllImport("wtsapi32.dll")]
    private static extern bool WTSQuerySessionInformation(
        nint hServer, int sessionId, WTS_INFO_CLASS wtsInfoClass, out nint ppBuffer, out uint pBytesReturned);

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
        nint serverHandle;
        List<UserInfo> ret = [];
        serverHandle = WTSOpenServer(serverName);

        try
        {
            nint sessionInfoPtr = nint.Zero;
            nint userPtr = nint.Zero;
            nint domainPtr = nint.Zero;
            int sessionCount = 0;
            int retVal = WTSEnumerateSessions(serverHandle, 0, 1, ref sessionInfoPtr, ref sessionCount);
            if (retVal != 0)
            {
                int dataSize = Marshal.SizeOf(typeof(WTS_SESSION_INFO));
                nint currentSession = sessionInfoPtr;

                for (int i = 0; i < sessionCount; i++)
                {
                    WTS_SESSION_INFO si = Marshal.PtrToStructure<WTS_SESSION_INFO>(currentSession);
                    currentSession += dataSize;

                    WTSQuerySessionInformation(serverHandle, si.SessionID, WTS_INFO_CLASS.WTSUserName, out userPtr, out _);
                    WTSQuerySessionInformation(serverHandle, si.SessionID, WTS_INFO_CLASS.WTSDomainName, out domainPtr, out _);

                    try
                    {
                        if (userPtr == nint.Zero || domainPtr == nint.Zero)
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
