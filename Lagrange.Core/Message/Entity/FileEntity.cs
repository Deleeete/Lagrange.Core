using Lagrange.Core.Internal.Packets.Message.Component;
using Lagrange.Core.Internal.Packets.Message.Component.Extra;
using Lagrange.Core.Internal.Packets.Message.Element;
using Lagrange.Core.Internal.Packets.Message.Element.Implementation;
using Lagrange.Core.Utility.Binary;
using Lagrange.Core.Utility.Extension;

namespace Lagrange.Core.Message.Entity;

[MessageElement(typeof(TransElem))]
public class FileEntity : IMessageEntity
{
    public bool IsGroup { get; internal set; }
    
    public long FileSize { get; internal set; }
    
    public string FileName { get; internal set; }
    
    public byte[] FileMd5 { get; internal set; }
    
    public string? FileUrl { get; internal set; }
    
    internal string? FileUuid { get; set; }
    
    internal string? FileHash { get; set; }
    
    internal Stream? FileStream { get; set; }
    
    public FileEntity()
    {
        FileName = "";
        FileMd5 = Array.Empty<byte>();
    }
    
    public FileEntity(string path)
    {
        FileStream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read);
        FileMd5 = FileStream.Md5().UnHex();
        FileSize = FileStream.Length;
        FileName = Path.GetFileName(path);
    }

    public FileEntity(byte[] payload, string fileName)
    {
        FileStream = new MemoryStream(payload);
        FileMd5 = payload.Md5().UnHex();
        FileSize = payload.Length;
        FileName = fileName;
    }

    internal FileEntity(long fileSize, string fileName, byte[] fileMd5, string fileUuid, string fileHash)
    {
        FileSize = fileSize;
        FileName = fileName;
        FileMd5 = fileMd5;
        FileUuid = fileUuid;
        FileHash = fileHash;
    }
    
    IEnumerable<Elem> IMessageEntity.PackElement() => Array.Empty<Elem>();

    object IMessageEntity.PackMessageContent() => new FileExtra
    {
        File = new NotOnlineFile
        { 
            FileType = 0, 
            FileUuid = "",
            FileMd5 = FileMd5,
            FileName = FileName,
            FileSize = FileSize,
            Subcmd = 1,
            DangerEvel = 0,
            ExpireTime = DateTime.Now.AddDays(7).Second,
            FileHash = "" // TODO: Send out Oidb
        }
    };
    
    IMessageEntity? IMessageEntity.UnpackElement(Elem elems)
    {
        if (elems.TransElem is { ElemType: 24 } trans)
        {
            var payload = new BinaryPacket(trans.ElemValue);
            payload.Skip(1);
            var protobuf = payload.ReadBytes(BinaryPacket.Prefix.Uint16 | BinaryPacket.Prefix.LengthOnly);
            Console.WriteLine(protobuf.Hex());
        }

        return null;
    }

    public string ToPreviewString() => $"[File] {FileName} ({FileSize}): {FileUrl ?? "failed to receive file url"}";
}