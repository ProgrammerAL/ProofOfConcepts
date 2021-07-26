using SignalRFileTransfer_Server.Hubs;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SignalRFileTransfer_Shared;

namespace SignalRFileTransfer_Server.Services
{
    public record ReceivedFile(string FileKey, byte[] Buffer);

    public interface IFileTransferHandler : IDisposable
    {
        event Action<ReceivedFile>? FileReceived;
        IReadOnlyCollection<ReceivedFile> ReceivedFiles { get; }
        bool TrackFileChunk(FileChunkDto? fileChunk);
    }

    public class FileTransferHandler : IFileTransferHandler
    {
        public event Action<ReceivedFile>? FileReceived;

        private readonly Dictionary<string, FileChunkTracker> _trackedChunks = new Dictionary<string, FileChunkTracker>(StringComparer.OrdinalIgnoreCase);
        private readonly ILogger<FileTransferHandler> _logger;
        private readonly List<ReceivedFile> _receivedFiles;

        public FileTransferHandler(ILogger<FileTransferHandler> logger)
        {
            _logger = logger;
            _receivedFiles = new List<ReceivedFile>();
        }

        public IReadOnlyCollection<ReceivedFile> ReceivedFiles => _receivedFiles;
        
        public void Dispose()
        {
            _trackedChunks.Clear();
        }

        public bool TrackFileChunk(FileChunkDto? fileChunk)
        {
            if (fileChunk?.CheckIsValid() != true)
            {
                _logger.LogError("Received invalid instance of {FileChunkDto}", nameof(FileChunkDto));
                return false;
            }

            if (fileChunk.TotalFileSizeInBytes!.Value > IFileTransferHub.MaxFileTransferSize)
            {
                _logger.LogError("Received file chunk for a file that is too big. Total file size is {Value}", fileChunk.TotalFileSizeInBytes.Value);
                return false;
            }

            if (!_trackedChunks.TryGetValue(fileChunk.FileKey!, out FileChunkTracker? tracker))
            {
                tracker = new FileChunkTracker(fileChunk.FileKey!, fileChunk.TotalFileSizeInBytes!.Value);
                _trackedChunks.Add(tracker.FileKey, tracker);
            }

            var fileStateAfterAdding = tracker.AddTrackedChunk(fileChunk.ChunkIndex!.Value, fileChunk.FileBytes!);

            if (fileStateAfterAdding.Result == FileChunkTracker.FileChunkStateType.Completed)
            {
                HandleFileTransferCompleted(fileStateAfterAdding, tracker);
            }

            return true;
        }

        private void HandleFileTransferCompleted(FileChunkTracker.AddFileChunkResult fileState, FileChunkTracker tracker)
        {
            _ = _trackedChunks.Remove(tracker.FileKey);

            var file = new ReceivedFile(tracker.FileKey, fileState.FileBytes!);
            _receivedFiles.Add(file);
            FileReceived?.Invoke(file);
        }

        private class FileChunkTracker
        {
            private readonly byte[] _fileBuffer;
            private readonly HashSet<int> _indexesWaitingOn;

            public FileChunkTracker(string fileKey, int totalFileSize)
            {
                FileKey = fileKey;
                _fileBuffer = new byte[totalFileSize];

                int chunksCount = CalculateChunksCount(totalFileSize);
                var chunkIndexes = Enumerable.Range(0, chunksCount);
                _indexesWaitingOn = new HashSet<int>(chunkIndexes);
            }

            public string FileKey { get; }

            internal AddFileChunkResult AddTrackedChunk(int chunkIndex, byte[] chunkBuffer)
            {
                if (_indexesWaitingOn.Contains(chunkIndex))
                {
                    _ = _indexesWaitingOn.Remove(chunkIndex);
                    var fileStartIndex = CalculateChunkCopyStartIndex(chunkIndex);
                    chunkBuffer.CopyTo(_fileBuffer, fileStartIndex);
                }

                if (_indexesWaitingOn.Count == 0)
                {
                    return new AddFileChunkResult(FileChunkStateType.Completed, FileBytes: _fileBuffer);
                }

                return new AddFileChunkResult(FileChunkStateType.WaitingOnChunks, FileBytes: null);
            }

            /// <summary>
            /// Finds the index to start copying to inside the _fileBuffer variable from the chunk buffer
            /// </summary>
            private static int CalculateChunkCopyStartIndex(int chunkIndex)
            {
                var fileStartIndex = chunkIndex * IFileTransferHub.MaxFileBufferChunkSize;
                return fileStartIndex;
            }

            private int CalculateChunksCount(int totalFileSize)
            {
                var chunksCount = totalFileSize / IFileTransferHub.MaxFileBufferChunkSize;

                //If there's some more in the buffer that doesn't fill up a full send, there's one more chunk
                if (IFileTransferHub.MaxFileBufferChunkSize * chunksCount < totalFileSize)
                {
                    chunksCount++;
                }

                return chunksCount;
            }

            internal record AddFileChunkResult(FileChunkStateType Result, byte[]? FileBytes);
            internal enum FileChunkStateType
            {
                Unknown,
                WaitingOnChunks,
                Completed
            }
        }
    }
}
