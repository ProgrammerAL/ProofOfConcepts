using MessagePack;

using System.Diagnostics.CodeAnalysis;

namespace SignalRFileTransfer_Shared
{
    [MessagePackObject]
    public class FileChunkDto
    {
        [Key(0)]
        public string? FileKey { get; set; }
        [Key(1)]
        public int? ChunkIndex { get; set; }
        [Key(2)]
        public int? TotalFileSizeInBytes { get; set; }
        [Key(3)]
        public byte[]? FileBytes { get; set; }

        [MemberNotNullWhen(true,
            nameof(FileKey),
            nameof(ChunkIndex),
            nameof(TotalFileSizeInBytes),
            nameof(FileBytes))]
        public bool CheckIsValid()
            => !string.IsNullOrWhiteSpace(FileKey)
               && ChunkIndex >= 0
               && TotalFileSizeInBytes >= 0
               && FileBytes is { Length: > 0 };
    }
}
