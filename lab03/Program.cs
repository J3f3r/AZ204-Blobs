 using Azure.Storage;
 using Azure.Storage.Blobs;
 using Azure.Storage.Blobs.Models;
 using System;
 using System.Threading.Tasks;    
 public class Program
 {
     //Atualize o valor blobServiceEndpoint que você registrar este laboratório.        
     private const string blobServiceEndpoint = $"https://{storageAccountName}.blob.core.windows.net";

     //Atualize o valor storageAccountName que você registrar este laboratório.
     private const string storageAccountName = "mediastorje";

     //Atualize o valor storageAccountKey que você registrar quando criar este laboratório.
     private const string storageAccountKey = "Environment.GetEnvironmentVariable("AZURE_STORAGE_ACCOUNT_KEY");

     //O código a seguir para criar um novo método Main assíncrono
     public static async Task Main(string[] args)
     {
         //A linha de código a seguir para criar uma nova instância da classe StorageSharedKeyCredential usando as constantes storageAccountName e storageAccountKey como parâmetros construtores
        StorageSharedKeyCredential accountCredentials = new StorageSharedKeyCredential(storageAccountName, storageAccountKey);

        //A linha de código a seguir para criar uma nova instância da classe BlobServiceClient usando a constante blobServiceEndpoint e a variável accountCredentials como parâmetros do construtor
        BlobServiceClient serviceClient = new BlobServiceClient(new Uri(blobServiceEndpoint), accountCredentials);

        //A linha de código a seguir para invocar o método GetAccountInfoAsync da classe BlobServiceClient para recuperar metadados da conta do serviço
        AccountInfo info = await serviceClient.GetAccountInfoAsync();

        //Renderizar uma mensagem de boas-vindas
        await Console.Out.WriteLineAsync($"Connected to Azure Storage Account");

        //Renderiza o nome da conta de armazenamento
        await Console.Out.WriteLineAsync($"Account name:\t{storageAccountName}");

        //Renderiza o tipo de conta de armazenamento
        await Console.Out.WriteLineAsync($"Account kind:\t{info?.AccountKind}");

        //Renderizar a unidade de manutenção de estoque (SKU) selecionada atualmente para a conta de armazenamento
        await Console.Out.WriteLineAsync($"Account sku:\t{info?.SkuName}");

        // chama o método de criar um contêiner antes de enumera-lo
        await GetContainerAsync(serviceClient, "novo-container-je");

        // chama o  método de enumerar o contêiner
        await EnumerateContainersAsync(serviceClient);

        // Nome do blob que você deseja procurar
        string blobNameToFind = "eu2.jpg";

        // Procura o blob em todos os contêineres
        BlobClient foundBlob = await FindBlobInAnyContainerAsync(serviceClient, blobNameToFind);

        if (foundBlob != null)
        {
            await Console.Out.WriteLineAsync($"Blob encontrado! URI:\t{foundBlob.Uri}");
        }
        else
        {
            await Console.Out.WriteLineAsync($"Blob '{blobNameToFind}' não foi encontrado em nenhum contêiner.");
        }

        // só funcionou o método FindBlobInAnyContainerAsync
        //uma forma de pegar um blob descrito do Microsot Learning mas a variável containerClient não foi declarada ou instanciada 
        //string uploadedBlobName = "graph.svg";
        //BlobClient blobClient = await GetBlobAsync(containerClient, uploadedBlobName);
        //await Console.Out.WriteLineAsync($"Blob Url:\t{blobClient.Uri}");

        //pegar um blob que já existe, mas o parametro result não exite porém funcionou com o professor Enrique
        //var blob = await GetBlobAsync(result, "arvore.jfif");

        //string existingContainerName = "raster-graphics";

        //await EnumerateBlobsAsync(serviceClient, existingContainerName);
     }

    private static async Task EnumerateContainersAsync(BlobServiceClient client)
    {
        /*Crie um loop foreach assíncrono que itera sobre os resultados de
            uma invocação do método GetBlobContainersAsync da classe BlobServiceClient. */
        await foreach (BlobContainerItem container in client.GetBlobContainersAsync())
        {
            //Imprime o nome de cada container
            await Console.Out.WriteLineAsync($"Container:\t{container.Name}");
            await EnumerateBlobsAsync(client, container.Name);
        }
    }

    private static async Task EnumerateBlobsAsync(BlobServiceClient client, string containerName)
    {
        /* Obtenha uma nova instância da classe BlobContainerClient usando o
            método GetBlobContainerClient da classe BlobServiceClient, 
            passando o parâmetro containerName */
        BlobContainerClient container = client.GetBlobContainerClient(containerName);

        /* Renderize o nome do contêiner que será enumerado */
        await Console.Out.WriteLineAsync($"Searching:\t{container.Name}");

        /* Crie um loop foreach assíncrono que itere sobre os resultados de
            uma invocação do método GetBlobsAsync da classe BlobContainerClient */
        await foreach (BlobItem blob in container.GetBlobsAsync())
        {
            //Imprime o nome de cada blob   
            await Console.Out.WriteLineAsync($"Existing Blob:\t{blob.Name}");
        }
    }

    private static async Task<BlobContainerClient> GetContainerAsync(BlobServiceClient client, string containerName)
    {
        /* Obtenha uma nova instância da classe BlobContainerClient usando o
            método GetBlobContainerClient da classe BlobServiceClient,
            passando o parâmetro containerName */
        BlobContainerClient container = client.GetBlobContainerClient(containerName);

        /* Invocar o método CreateIfNotExistsAsync da classe BlobContainerClient */
        await container.CreateIfNotExistsAsync(PublicAccessType.Blob);

        /* Renderize o nome do contêiner que foi potencialmente criado */
        await Console.Out.WriteLineAsync($"New Container:\t{container.Name}");

        /* Retorna o contêiner como resultado do GetContainerAsync */
        return container;
    }

    // método para procurar se um blob existe e, se caso exista, retorna sua Uri
    /*private static async Task<BlobClient> GetBlobAsync(BlobContainerClient client, string blobName)
    {
        BlobClient blob = client.GetBlobClient(blobName);
        bool exists = await blob.ExistsAsync();
        if (!exists)
        {
            await Console.Out.WriteLineAsync($"Blob {blob.Name} not found!");

        }
        else
            await Console.Out.WriteLineAsync($"Blob Found, URI:\t{blob.Uri}");
        return blob;
    }*/

    // Método para procurar o blob em qualquer contêiner
    private static async Task<BlobClient> FindBlobInAnyContainerAsync(BlobServiceClient client, string blobName)
    {
        await foreach (BlobContainerItem container in client.GetBlobContainersAsync())
        {
            BlobContainerClient containerClient = client.GetBlobContainerClient(container.Name);
            BlobClient blobClient = containerClient.GetBlobClient(blobName);

            bool exists = await blobClient.ExistsAsync();
            if (exists)
            {
                return blobClient;
            }
        }

        return null;
    }
}
