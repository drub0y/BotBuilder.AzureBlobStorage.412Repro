using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Azure;

namespace App
{
    class Program
    {
        private const string AzureStorageConnectionStringEnvironmentVariableName = "AzureStorageConnectionString";
        private static readonly string[] StorageKeys = new[] { "MyAmazingStorageKey" };

        static async Task Main(string[] args)
        {
            var azureStorageConnectionString = default(string);
            var azureStorageContainerName = default(string);

            if (args.Length == 1)
            {
                azureStorageConnectionString = args[0];
            }
            else
            {
                azureStorageConnectionString = Environment.GetEnvironmentVariable(AzureStorageConnectionStringEnvironmentVariableName);
            }

            if (azureStorageConnectionString == default(string)
                    ||
               azureStorageConnectionString.Length == 0)
            {
                Console.WriteLine($"ERROR: No valid connection string supplied. Pass as an arg or set \"{AzureStorageConnectionStringEnvironmentVariableName}\" environment variable.");

                return;
            }

            if (args.Length == 2)
            {
                azureStorageContainerName = args[1];
            }

            if (azureStorageContainerName == null)
            {
                azureStorageContainerName = "testbotstorage";
            }

            var azureBlobStorage = new AzureBlobStorage(azureStorageConnectionString, azureStorageContainerName);

            var cts = new CancellationTokenSource();

            Console.WriteLine("Starting reader/writer...");

            var readerTask = StartReaderAsync(azureBlobStorage, cts.Token);
            var writerTask = StartWriterAsync(azureBlobStorage, cts.Token);

            Console.WriteLine("Reader/writer now running...");

            Console.WriteLine("Press any key to stop.");

            Console.ReadKey(true);

            cts.Cancel();

            try
            {
                await Task.WhenAll(readerTask, writerTask);
            }
            catch (TaskCanceledException)
            {
            }

            Console.WriteLine("Done.");
        }

        private static async Task StartReaderAsync(AzureBlobStorage azureBlobStorage, CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    await azureBlobStorage.ReadAsync(StorageKeys, cancellationToken);
                }
                catch (Exception exception)
                {
                    Console.WriteLine("READER EXCEPTION: {0}", exception);
                }
            }
        }

        private static async Task StartWriterAsync(AzureBlobStorage azureBlobStorage, CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    await azureBlobStorage.WriteAsync(
                        new Dictionary<string, object> {
                            {
                                StorageKeys[0],
                                new Dictionary<string, object>()
                                {
                                    { "Value1", 13177 },
                                    { "Value2", Guid.NewGuid() },
                                    { "Value3", new { SubValue = "Sub" } },
                                    { "Value4", DateTime.UtcNow }
                                }
                            }
                        }, 
                        cancellationToken);
                }
                catch (Exception exception)
                {
                    Console.WriteLine("WRITER EXCEPTION: {0}", exception);
                }
            }
        }
    }
}
