using System.Threading.Tasks;

namespace SignalRFileTransfer_Shared
{
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
}
