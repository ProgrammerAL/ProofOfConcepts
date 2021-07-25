using MessagePack;

using Microsoft.AspNetCore.SignalR;

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;
using SignalRFileTransfer_Server.Services;

namespace SignalRFileTransfer_Server.Hubs
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

    public interface IFileTransferHub
    {
        /// <summary>
        /// Max size a file can transfer is 10 MB
        /// This translates to a total of 500 chunk messages sent over SignalR to transfer a file of this size
        /// </summary>
        public const int MaxFileTransferSize = 10_000_000;

        /// <summary>
        /// Max amount of bytes for a single chunk to be sent in a file transfer message
        ///   Default max size for a SignalR Core message is 32KB
        ///   Max out the file-chunk buffer at 20KB to give us some space for other properties sent in the same message
        /// </summary>
        public const int MaxFileBufferChunkSize = 20_000;

        Task<bool?> UploadFileChunkAsync(FileChunkDto? fileChunk);
    }

    public class FileTransferHub : Hub, IFileTransferHub
    {
        private readonly IFileTransferHandler _tracker;

        public FileTransferHub(IFileTransferHandler tracker)
        {
            _tracker = tracker;
        }

        public Task<bool?> UploadFileChunkAsync(FileChunkDto? fileChunk)
        {
            var result = _tracker.TrackFileChunk(fileChunk);
            return Task.FromResult<bool?>(result);
        }
    }
}
