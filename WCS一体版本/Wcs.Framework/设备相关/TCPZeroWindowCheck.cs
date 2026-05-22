using System;
using System.Net.Sockets;
using System.Runtime.InteropServices;

namespace Wcs.Framework
{
    public class TCPZeroWindowCheck
    {
        [DllImport("winsock2.dll", SetLastError = true)]
        private static extern int WSAIoctl(
            IntPtr socketHandle,
            uint dwIoControlCode,
            ref uint lpvInBuffer,
            uint cbInBuffer,
            out uint lpvOutBuffer,
            uint cbOutBuffer,
            out uint lpcbBytesReturned,
            IntPtr lpOverlapped,
            IntPtr lpCompletionRoutine
        );

        private const uint SIO_TCP_QUERY_INFO = 0x4004730b;
        private const uint TCP_INFO = 11;
        private const int TCP_CA_RECV_WINDOW = 3;

        [StructLayout(LayoutKind.Explicit)]
        private struct TCP_INFO_v
        {
            [FieldOffset(0)]
            public uint State;
            [FieldOffset(4)]
            public uint Ca_state;
            [FieldOffset(8)]
            public uint Mss;
            [FieldOffset(12)]
            public uint Ca_recv_window;
            // ... more fields omitted
        }

        //public static bool IsTCPZeroWindow(Socket socket)
        //{
        //    IntPtr socketHandle = socket.Handle;
        //    uint parameter = 0;
        //    uint returnLength = 0;

        //    int result = WSAIoctl(
        //        socketHandle,
        //        SIO_TCP_QUERY_INFO,
        //        ref parameter,
        //        sizeof(uint),
        //        out parameter,
        //        (uint)Marshal.SizeOf(typeof(TCP_INFO_v)),
        //        out returnLength,
        //        IntPtr.Zero,
        //        IntPtr.Zero
        //    );

        //    if (result == 0 && returnLength == (uint)Marshal.SizeOf(typeof(TCP_INFO_v)))
        //    {
        //        TCP_INFO_v tcpInfo = (TCP_INFO_v)Marshal.PtrToStructure(new IntPtr(&parameter), typeof(TCP_INFO_v));
        //        return tcpInfo.Ca_recv_window == 0;
        //    }

        //    return false;
        //}
        public static bool IsTCPZeroWindow(Socket socket)
        {
            IntPtr socketHandle = socket.Handle;
            uint inValue = 0; // Input value for SIO_TCP_QUERY_INFO, typically not used
            uint outValue; // Output value for SIO_TCP_QUERY_INFO
            uint bytesReturned; // Number of bytes returned by the call

            // Call the WSAIoctl function
            int result = WSAIoctl(
                socketHandle,
                SIO_TCP_QUERY_INFO,
                ref inValue,
                sizeof(uint),
                out outValue,
                (uint)Marshal.SizeOf(typeof(TCP_INFO_v)),
                out bytesReturned,
                IntPtr.Zero,
                IntPtr.Zero
            );

            // Check for success
            if (result == 0 && bytesReturned == (uint)Marshal.SizeOf(typeof(TCP_INFO_v)))
            {
                // Convert the pointer to the TCP_INFO_v structure
                IntPtr tcpInfoPtr = new IntPtr(outValue);
                TCP_INFO_v tcpInfo = (TCP_INFO_v)Marshal.PtrToStructure(tcpInfoPtr, typeof(TCP_INFO_v));

                // Check if the receive window is zero
                return tcpInfo.Ca_recv_window == 0;
            }

            // Handle failure or other error conditions
            return false;
        }
    }
}
