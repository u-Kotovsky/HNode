using System.Runtime.InteropServices;

namespace Generators.MAVLinkDrone
{
        [StructLayout(LayoutKind.Sequential, Pack = 1, Size = 251)]
        public struct FTPMessage
        {
            public ushort seq_number;
            public byte session;
            public ftp_opcode opcode;
            public byte size;
            public ftp_opcode req_opcode;
            public byte burst_complete;
            public byte padding;
            public uint offset;

            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 251 - 12)]
            public byte[] data;

            public FTPMessage(ushort seq_number, byte session, ftp_opcode opcode, byte size, ftp_opcode req_opcode, byte burst_complete, uint offset, byte[] data)
            {
                this.seq_number = seq_number;
                this.session = session;
                this.opcode = opcode;
                this.size = size;
                this.req_opcode = req_opcode;
                this.burst_complete = burst_complete;
                this.padding = 0; //padding to align to 4 bytes
                this.offset = offset;
                this.data = data ?? new byte[251 - 12];
            }

            public enum ftp_opcode : byte
            {
                None = 0,
                TerminateSession = 1,
                ResetSessions = 2,
                ListDirectory = 3,
                OpenFileRO = 4,
                ReadFile = 5,
                CreateFile = 6,
                WriteFile = 7,
                RemoveFile = 8,
                CreateDirectory = 9,
                RemoveDirectory = 10,
                OpenFileWO = 11,
                TruncateFile = 12,
                Rename = 13,
                CalcFileCRC32 = 14,
                BurstReadFile = 15,



                ACK = 128,
                NAK = 129,
            }
        }
    }
