
using Microsoft.AspNetCore.SignalR;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SignalRFileTransfer_Server.Services;
using SignalRFileTransfer_Shared;

namespace SignalRFileTransfer_Server.Hubs
{
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
