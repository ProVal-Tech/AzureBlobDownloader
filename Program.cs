using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using CommandLine;
using System;

Parser.Default.ParseArguments<Options>(args)
    .WithParsed(opts => {
        string connectionString = $"DefaultEndpointsProtocol=https;AccountName={opts.StorageAccountName};AccountKey={opts.AccessKey};EndpointSuffix=core.windows.net";

        BlobServiceClient client = new(connectionString);
        BlobContainerClient containerClient = client.GetBlobContainerClient(opts.ContainerName);
        Azure.Pageable<BlobItem> blobs;
        if (string.IsNullOrWhiteSpace(opts.TargetBlobPath)) {
            blobs = containerClient.GetBlobs();
        } else {
            blobs = containerClient.GetBlobs(prefix: $"{opts.TargetBlobPath}");
        }
        foreach (BlobItem blob in blobs) {
            if (blob.Name.EndsWith('/')) {
                continue;
            }
            BlobClient blobClient = containerClient.GetBlobClient(blob.Name);
            DirectoryInfo targetDir = Directory.CreateDirectory(opts.TargetFilePath ?? string.Empty);
            string localFilePath = Path.Combine(targetDir.FullName, blob.Name);
            Directory.CreateDirectory(Path.GetDirectoryName(localFilePath) ?? string.Empty);
            using FileStream downloadStream = File.OpenWrite(localFilePath);
            blobClient.DownloadTo(downloadStream);
            Console.WriteLine($"Blob '{blob.Name}' downloaded to '{localFilePath}'.");
        }
    });

public class Options {
    [Option('a', "storageaccountname", Required = true, HelpText = "Azure storage account name.")]
    public string StorageAccountName { get; set; } = null!;

    [Option('c', "containername", Required = true, HelpText = "Blob container name.")]
    public string ContainerName { get; set; } = null!;

    [Option('b', "targetblobpath", Required = false, HelpText = "Target blob directory or file.")]
    public string TargetBlobPath { get; set; } = string.Empty;

    [Option('f', "targetfilepath", Required = true, HelpText = "Local directory to save the blobs.")]
    public string TargetFilePath { get; set; } = null!;

    [Option('k', "accesskey", Required = true, HelpText = "Azure storage account access key.")]
    public string AccessKey { get; set; } = null!;
}