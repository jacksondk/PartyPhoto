using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static System.Net.WebRequestMethods;

public class AzureBlobRandomFileRetriever
{
    private static BlobContainerClient _containerClient;
    private static Random _random = new Random();

    public AzureBlobRandomFileRetriever(string connectionString, string containerName)
    {
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new ArgumentNullException(nameof(connectionString), "Connection string cannot be null or empty.");
        }
        if (string.IsNullOrWhiteSpace(containerName))
        {
            throw new ArgumentNullException(nameof(containerName), "Container name cannot be null or empty.");
        }

        // Create a BlobServiceClient objectn
        BlobServiceClient blobServiceClient = new BlobServiceClient(connectionString);

        // Get a reference to a container
        _containerClient = blobServiceClient.GetBlobContainerClient(containerName);

        // Ensure the container exists (optional, but good practice if you're unsure)
        // You might want to handle container creation elsewhere or ensure it always exists.
        // _containerClient.CreateIfNotExists();
    }

    public async Task<BlobItem> GetRandomBlobItemAsync()
    {
        // List all blobs in the container
        var blobItems = new List<BlobItem>();
        try
        {
            await foreach (BlobItem blobItem in _containerClient.GetBlobsAsync())
            {
                blobItems.Add(blobItem);
            }
        }
        catch (Azure.RequestFailedException rfEx)
        {
            Console.WriteLine($"Azure RequestFailedException while listing blobs: {rfEx.ToString()}");
            Console.WriteLine($"Error Code: {rfEx.ErrorCode}, Status: {rfEx.Status}");
            // This is where you'd typically see "ContainerNotFound" if the container doesn't exist
            // or other permission-related issues.
            throw;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Generic exception while listing blobs: {ex.ToString()}");
            throw;
        }


        if (blobItems.Count == 0)
        {
            // Handle the case where the container is empty
            return null; // Or throw an exception, e.g., throw new InvalidOperationException("The container is empty.");
        }

        // Select a random blob item
        int randomIndex = _random.Next(blobItems.Count);
        return blobItems[randomIndex];
    }

    public async Task<BlobClient> GetRandomBlobClientAsync()
    {
        BlobItem randomBlobItem = await GetRandomBlobItemAsync();
        if (randomBlobItem != null)
        {
            return _containerClient.GetBlobClient(randomBlobItem.Name);
        }
        return null;
    }

    // Example Usage:
    public static async Task Main(string[] args)
    {
        // Replace with your actual connection string and container name
        
        string containerName = "photos";
        string url = "";
        string sas = "";
        string connectionString = $"{url}?{sas}";

        string conStr = "";
        try
        {
            AzureBlobRandomFileRetriever retriever = new AzureBlobRandomFileRetriever(conStr, containerName);

            Console.WriteLine("Attempting to get a random blob item...");
            BlobItem randomBlob = await retriever.GetRandomBlobItemAsync();

            if (randomBlob != null)
            {
                Console.WriteLine($"Found random blob: {randomBlob.Name}");
                Console.WriteLine($"  Size: {randomBlob.Properties.ContentLength} bytes");
                Console.WriteLine($"  Last Modified: {randomBlob.Properties.LastModified}");

                // If you need to work with the blob (e.g., download), get a BlobClient
                BlobClient blobClient = _containerClient.GetBlobClient(randomBlob.Name);
                Console.WriteLine($"  Blob URI: {blobClient.Uri}");

                // Example: Get properties using the specific BlobClient
                // BlobProperties properties = await blobClient.GetPropertiesAsync();
                // Console.WriteLine($"  ETag from BlobClient: {properties.ETag}");
            }
            else
            {
                Console.WriteLine("No blobs found in the container or the container is empty.");
            }

            // Alternatively, directly get a BlobClient for a random blob
            Console.WriteLine("\nAttempting to get a random BlobClient...");
            BlobClient randomClient = await retriever.GetRandomBlobClientAsync();
            if (randomClient != null)
            {
                Console.WriteLine($"Found random blob client for: {randomClient.Name}");
                Console.WriteLine($"  URI: {randomClient.Uri}");
            }
            else
            {
                Console.WriteLine("No blobs found to create a client for.");
            }

        }
        catch (ArgumentNullException ex)
        {
            Console.WriteLine($"Configuration Error: {ex.Message}");
        }
        catch (Azure.RequestFailedException ex)
        {
            Console.WriteLine($"Azure Storage Error: {ex.Message}");
            Console.WriteLine($"Status Code: {ex.Status}");
            Console.WriteLine($"Error Code: {ex.ErrorCode}");
            // For more details, check ex.ToString()
        }
        catch (Exception ex)
        {
            Console.WriteLine($"An unexpected error occurred: {ex.Message}");
        }
    }
}