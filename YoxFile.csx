#r "nuget: System.Text.Encoding.CodePages, 4.7.0"

using System.Runtime.InteropServices;

public class YoxFile
{
    ScriptHeader Header;
    byte[] Commands;
    public string[] Strings;

    static YoxFile()
    {
        System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);
    }

    public YoxFile(byte[] raw)
    {
        ParseHeader(raw);
        ParseCommands(raw);
        ParseStrings(raw);
    }

    private unsafe void ParseHeader(byte[] raw)
    {
        var header_raw = raw[0..sizeof(ScriptHeader)];

        fixed (byte* ptr = &header_raw[0])
        {
            Header = (ScriptHeader)Marshal.PtrToStructure((IntPtr)ptr, typeof(ScriptHeader));
        }
    }

    private unsafe void ParseCommands(byte[] raw)
    {
        Commands = raw[sizeof(ScriptHeader)..(sizeof(ScriptHeader) + Header.CommandsLength)];
    }

    private unsafe void ParseStrings(byte[] raw)
    {
        var count = Header.OffsetTableLength / 4;

        var table_start = sizeof(ScriptHeader) + Header.CommandsLength + Header.StringsLength;
        var strings_start = sizeof(ScriptHeader) + Header.CommandsLength;

        var offset_buffer = raw[table_start..(table_start + Header.OffsetTableLength)];
        var string_buffer = raw[strings_start..(strings_start + Header.StringsLength)];

        Strings = new String[count];
        int p = 0;

        for (int idx_offset = 0; idx_offset < offset_buffer.Length; idx_offset += 4)
        {
            var idx_string = BitConverter.ToInt32(offset_buffer, idx_offset);
            Strings[p++] = CollectString(idx_string, string_buffer);
        }

        if (p != count)
            throw new Exception("Number of strings is less than it should be.");

        string CollectString(int idx, byte[] buffer)
        {
            int end = idx;
            while (buffer[end] != 0x00)
                end++;

            return Encoding.GetEncoding(932).GetString(buffer, idx, end - idx);
        }
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    struct ScriptHeader
    {
        public uint Magic;
        public uint Unknown1;
        public int CommandsLength;
        public int StringsLength;
        public int OffsetTableLength;
        public uint Unknown2;
        public uint Unknown3;
        public uint Unknown4;
        public uint Unknown5;
        public uint Unknown6;
        public uint Unknown7;
        public uint Unknown8;
    }
}