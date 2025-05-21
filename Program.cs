using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using CommandLine;
using System;

Parser.Default.ParseArguments<Options>(args)
    .WithParsed(opts => {
        string connectionString = $"DefaultEndpointsProtocol=https;AccountName={opts.StorageAccountName};AccountKey={opts.AccessKey};EndpointSuffix=core.windows.net";

        BlobServiceClient client = new(connectionString);
        BlobContainerClient containerClient = client.GetBlobContainerClient(opts.ContainerName);
        BlobItem targetItem = containerClient.GetBlobs().ToList().Where(blob => blob.Name == opts.TargetBlobPath).FirstOrDefault()
            ?? throw new Exception($"Blob {opts.TargetBlobPath} not found in container {opts.ContainerName}");

        // Download the blob content
        BlobClient blobClient = containerClient.GetBlobClient(opts.TargetBlobPath);
        string localFilePath = Path.GetFullPath(opts.TargetFilePath);
        Directory.CreateDirectory(Path.GetDirectoryName(localFilePath) ?? string.Empty);
        using (var downloadStream = File.OpenWrite(localFilePath)) {
            blobClient.DownloadTo(downloadStream);
        }
        Console.WriteLine($"Blob '{opts.TargetBlobPath}' downloaded to '{localFilePath}'.");
    });

public class Options {
    [Option('a', "storageaccountname", Required = true, HelpText = "Azure Storage Account Name.")]
    public string StorageAccountName { get; set; } = null!;

    [Option('c', "containername", Required = true, HelpText = "Blob Container Name.")]
    public string ContainerName { get; set; } = null!;

    [Option('b', "targetblobpath", Required = true, HelpText = "Target Blob Path.")]
    public string TargetBlobPath { get; set; } = null!;

    [Option('f', "targetfilepath", Required = true, HelpText = "Local file path to save the blob.")]
    public string TargetFilePath { get; set; } = null!;

    [Option('k', "accesskey", Required = true, HelpText = "Azure Storage Account Access Key.")]
    public string AccessKey { get; set; } = null!;
}