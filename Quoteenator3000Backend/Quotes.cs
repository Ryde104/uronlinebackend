using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure;
using Xceed.Document.NET;
using System.Collections.Generic;
using System.Linq;

namespace Quoteenator3000Backend
{
    public static class Quotes
    {
        [FunctionName("Quotes")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            List<CGeneratedQuote> acg = new List<CGeneratedQuote>();
                
                BlobServiceClient blobServiceClient = new BlobServiceClient("DefaultEndpointsProtocol=https;AccountName=uro;AccountKey=eTihN8FX52PIBmrWV0wb8Le2cXEz50io3WAisRys5o0tb0WSUVz/DxvJE9DxOgcwkL4rA7Ka/jeF+ASttjwHrA==;EndpointSuffix=core.windows.net");

            BlobContainerClient ccWrite = blobServiceClient.GetBlobContainerClient("out");
            var resultSegment = ccWrite.GetBlobsAsync().AsPages(default, 1);

            //https://uro.blob.core.windows.net/out/somebank-03302023.docx
            await foreach (Page<BlobItem> blobPage in resultSegment)
            {
                foreach (BlobItem blobItem in blobPage.Values)
                {
                    acg.Add(new CGeneratedQuote {  Name = Path.GetFileNameWithoutExtension(blobItem.Name), URL= "https://uro.blob.core.windows.net/out/" + blobItem.Name });
                    //Console.WriteLine("Blob name: {0}", blobItem.Name);
                }

                //Console.WriteLine();
            }

            CGeneratedQuotes v = new CGeneratedQuotes();
            v.Quotes = Enumerable.Reverse(acg).ToArray();

            return new JsonResult(v);
        }
    }

    public class CGeneratedQuotes
    {
        public CGeneratedQuote[] Quotes { get; set; }
        
    }
    public class CGeneratedQuote
    {
        public string URL { get; set; }
        public string Name { get; set; }
    }
}
