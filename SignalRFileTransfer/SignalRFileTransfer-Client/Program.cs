using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.SignalR.Client;
using System.IO;
using System.Diagnostics;

namespace SignalRFileTransfer_Client
{
    class Program
    {
        static async Task Main()
        {
            Console.WriteLine("Press Enter to begin...");
            _ = Console.ReadLine();

            var hubEndpoint = "https://localhost:44380/FileTransferHub";
            var connection = new HubConnectionBuilder()
                .WithUrl(hubEndpoint)
                .AddMessagePackProtocol()//MessagePack not on by default
                .WithAutomaticReconnect()
                .Build();

            await connection.StartAsync();

            var uploader = new FilesUploader(connection);
            using (var stream = new FileStream(path: "./pew.mp3", FileMode.Open))
            {
                Console.WriteLine("Uploading...");
                var stopwatch = new Stopwatch();

                stopwatch.Start();
                var result = await uploader.UploadFileAsync("pew", stream);
                stopwatch.Stop();

                Console.WriteLine($"Upload Result: {result}");
                Console.WriteLine($"\tCompleted in: {stopwatch.ElapsedMilliseconds} ms");
            }
        }
    }
}
