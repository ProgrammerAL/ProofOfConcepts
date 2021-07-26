using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.AspNetCore.SignalR.Client;

using SignalRFileTransfer_Shared;

namespace SignalRFileTransfer_Client
{
    public class FilesUploader
    {
        private readonly byte[] _fullSizeChunkBuffer = new byte[IFileTransferHub.MaxFileBufferChunkSize];
        private readonly HubConnection _connection;

        public FilesUploader(HubConnection connection)
        {
            _connection = connection;
        }

        public async Task<bool> UploadFileAsync(string fileKey, Stream fileStream)
        {
            EnsureCanRunFileUpload(fileKey, fileStream);

            int i = 0;
            int index = 0;
            int totalFileSize = (int)fileStream.Length;

            while (i < fileStream.Length)
            {
                var fileChunk = await ReadNextFileChunkAsync(i, totalFileSize, fileStream);

                var message = new FileChunkDto
                {
                    FileKey = fileKey,
                    FileBytes = fileChunk,
                    ChunkIndex = index,
                    TotalFileSizeInBytes = totalFileSize
                };

                var wasSuccessful = await UploadFileAsync(message);

                if (!wasSuccessful)
                {
                    return false;
                }

                i += IFileTransferHub.MaxFileBufferChunkSize;
                index++;
            }

            return true;
        }

        private void EnsureCanRunFileUpload(string fileKey, Stream stream)
        {
            if (stream.Length > IFileTransferHub.MaxFileTransferSize)
            {
                throw new Exception($"File too big to transfer: {fileKey} - File Size In Bytes: {stream.Length}");
            }
        }

        private async Task<bool> UploadFileAsync(FileChunkDto message)
        {
            bool sentSuccessfully = false;
            int sendCount = 0;

            //Try to upload the file multiple times
            while (!sentSuccessfully
                && sendCount < 3)
            {
                sentSuccessfully = await _connection.InvokeAsync<bool>(nameof(IFileTransferHub.UploadFileChunkAsync), arg1: message);
                sendCount++;
            }

            return sentSuccessfully;
        }

        private async Task<byte[]> ReadNextFileChunkAsync(int startIndex, int totalFileSize, Stream stream)
        {
            var chunkLength = CalculateNextFileLength(startIndex, totalFileSize);
            var outBuffer = GetFileTransferBufferInstance(chunkLength);
            _ = await stream.ReadAsync(outBuffer, startIndex, chunkLength);

            return outBuffer;
        }

        private byte[] GetFileTransferBufferInstance(int bufferLength)
        {
            //Most file transfers will use the max buffer (usually all but the last chunk transfer)
            //  Only create a new buffer instance if we need to
            if (bufferLength == IFileTransferHub.MaxFileBufferChunkSize)
            {
                return _fullSizeChunkBuffer;
            }

            return new byte[bufferLength];
        }

        private static int CalculateNextFileLength(int startIndex, int totalFileSize)
        {
            int lengthAfterStartIndex = totalFileSize - startIndex - 1;
            if (lengthAfterStartIndex >= IFileTransferHub.MaxFileBufferChunkSize)
            {
                return IFileTransferHub.MaxFileBufferChunkSize;
            }

            return lengthAfterStartIndex;
        }
    }
}
