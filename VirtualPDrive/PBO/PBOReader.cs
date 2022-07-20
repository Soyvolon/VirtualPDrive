using Serilog;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace VirtualPDrive.PBO;
public class PBOReader : BinaryReader, IDisposable, IAsyncDisposable
{
    public const int VERSION_HEADER_LENGTH = 15;

    private FileStream PBOStream { get; init; }
    public List<PBOFile> Files { get; set; } = new();
    public IReadOnlyDictionary<string, string> Headers { get; set; } = new Dictionary<string, string>();

    public PBOReader(FileStream pboStream)
        : base(pboStream)
    {
        PBOStream = pboStream;
    }

    /// <summary>
    /// Keeps reading bytes until it encounters 0 then returns as string
    /// </summary>
    /// <returns></returns>
    public override string ReadString()
    {
        //Initialize Byte list
        List<byte> bytes = new List<byte>();
        //Run through bytes from current index until we encounter 0
        for (byte i = ReadByte(); i != 0; i = ReadByte())
            bytes.Add(i);
        return Encoding.ASCII.GetString(bytes.ToArray());
    }

    private Task ReadHeaderAsync()
    {
        Dictionary<string, string> headers = new();
        // Get header
        string head;
        do
        {
            head = ReadString();

            if (head.Length != 0)
            {
                if (head == "sreV")
                {
                    BaseStream.Seek(VERSION_HEADER_LENGTH, SeekOrigin.Current);
                }
                else
                {
                    string value = ReadString();
                    _ = headers.TryAdd(head, value);
                }
            }
        } while (head.Length != 0);

        Headers = headers;

        return Task.CompletedTask;
    }

    private Task ReadFileTableAsync()
    {
        string file;
        do
        {
            file = ReadString();

            // Dump this data - we dont need it.
            for (int i = 0; i < 5; i++)
                _ = ReadInt32();

            if (file.Length != 0)
            {
                Files.Add(new(file));
            }
        } while (file.Length != 0);

        return Task.CompletedTask;
    }

    public async Task InitalizeAsync()
    {
        if (ReadString().Length == 0)
            await ReadHeaderAsync();
        else BaseStream.Seek(0, SeekOrigin.Begin);

        await ReadFileTableAsync();
    }

    public void Dispose()
    {
        PBOStream.Dispose();
    }

    public async ValueTask DisposeAsync()
    {
        await PBOStream.DisposeAsync();
    }
}
