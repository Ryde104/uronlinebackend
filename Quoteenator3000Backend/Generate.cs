using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Xceed.Document.NET;
using Xceed.Words.NET;
using Azure.Storage.Blobs;
using System.Text;
using System.Collections.Generic;
using System.Reflection.Metadata;
using System.Xml.Linq;
using System.Linq;

namespace Quoteenator3000Backend
{
    public static class Generate
    {
        static List<CProductInfo> m_api = new List<CProductInfo>();

        [FunctionName("Generate")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequest req,
            ILogger log)
        {


            m_api.Add(new CProductInfo { Name = "ABBIRB4600", Description = "ABB IRB 4600-20kg 2.5-meter robotic welding arm", Image = "car1.jpg" });

            //CQuote cq = new CQuote();
            //cq.ObjectDesc = "new obj";
            //cq.ObjectPrice = 89.99m;
            //cq.CompanyName = "somebank2";

            //List<CProduct> acp = new List<CProduct>();
            //acp.Add(new CProduct { Name = "ABB IRB 4600-20kg 2.5-meter robotic welding arm", Price = 60500.00m, Quantity = 3, WordDoc = "ABBIRB4600-20kg2.5-meterroboticweldingarm.docx", Image = "booboo.jpg", ItemDescription="This is a cat!!!" });

            //acp.Add(new CProduct { Name = "IRB-P 1000L head/tail stock positioners", Price = 16500.00m, Quantity = 2, WordDoc = "IRB-P1000Lheadtailstockpositioners.docx", Image= "20201006_075749.jpg", ItemDescription="Me dude!!!" });
            //cq.Products = acp.ToArray();

            try
            {
                string strBody = await new StreamReader(req.Body).ReadToEndAsync();

                CQuote cq = JsonConvert.DeserializeObject<CQuote>(strBody);

                BlobServiceClient blobServiceClient = new BlobServiceClient("DefaultEndpointsProtocol=https;AccountName=urostorage;AccountKey=RPc4nXEDB05dD3JyBf7djuxvxZfz0SQJnDNIy0BTDBgtkDtZoXul4AG6np4qPMsWoiYkveXcRr4/+AStsLXYAg==;EndpointSuffix=core.windows.net");
                BlobContainerClient containerClient = blobServiceClient.GetBlobContainerClient("template");
                var blobClient = containerClient.GetBlobClient("Main3.docx");

                var docMain = DocX.Load(blobClient.OpenRead());



                decimal dTotal = 0;

                //Add service scope
                Xceed.Document.NET.List xList = null;
                foreach (object item in cq.Products)
                {
                    CProduct cp = (CProduct)item;
                    if (cp.Quantity > 0)
                    {
                        if (xList == null)
                        {
                            xList = docMain.AddList("(" + cp.Quantity.ToString() + ") " + cp.Name, 0, ListItemType.Bulleted);
                        }
                        else
                            docMain.AddListItem(xList, "(" + cp.Quantity.ToString() + ") " + cp.Name, 0);

                        dTotal += (cp.Price * cp.Quantity);
                    }
                }

                if (xList != null)
                {
                    var table = docMain.Tables[0];
                    table.Rows[0].Cells[0].InsertList(xList);
                }

                //Add sections
                foreach (object item in cq.Products)
                {
                    CProduct cp = (CProduct)item;
                    if (cp.Quantity > 0)
                        AddDocument(cp.Name, docMain);
                    //AddDocument(cp.Image, cp.ItemDescription, docMain);
                }



                dTotal = dTotal + (dTotal * .4m); //Add 40%
                docMain.ReplaceText("<#TOTAL#>", dTotal.ToString("C2"));
                docMain.ReplaceText("<#NAME#>", cq.CompanyName);
                docMain.ReplaceText("<#OBJECTDESC#>", cq.ObjectDesc);
                docMain.ReplaceText("<#OBJECTPRICE#>", cq.ObjectPrice.ToString("C2"));
                docMain.ReplaceText("<#SHORTDATE#>", DateTime.Now.ToShortDateString());
                docMain.ReplaceText("<#FULLDATE#>", DateTime.Now.ToLongDateString());
                docMain.ReplaceText("<#PROPOSAL#>", GenerateProposalID(cq.CompanyName));

                string fileName = Path.GetFileName(GenerateProposalID(cq.CompanyName) + ".docx");

                BlobContainerClient ccWrite = blobServiceClient.GetBlobContainerClient("out");
                BlobClient bcWrite = ccWrite.GetBlobClient(fileName);


                byte[] baReturn = null;
                using (MemoryStream stream = new MemoryStream())
                {
                    docMain.SaveAs(stream);
                    baReturn = stream.ToArray();
                }



                using (var ms = new MemoryStream(baReturn, false))
                {
                    await bcWrite.UploadAsync(ms);
                }




                ////byte[] baReturn = null;
                //using (MemoryStream stream = new MemoryStream())
                //{
                //    await bcWrite.UploadAsync(stream, true);

                //    //docMain.SaveAs(stream);
                //    //baReturn = stream.ToArray();
                //}








                //return new FileContentResult(baReturn, "application/octet-stream")
                //{
                //    FileDownloadName = GenerateProposalID(cq.CompanyName) + ".docx"
                //};

                return new OkResult();

            }
            catch (Exception ex)
            {
                string str = ex.Message;
            }

            return new OkObjectResult("error");
        }

        public static string GenerateProposalID(string CustomerName)
        {
            return CustomerName + "-" + DateTime.Now.ToString("MMddyyyy");
        }

        //private static void AddDocument(string Image, string Text, DocX MainDoc)
        private static void AddDocument(string Name, DocX MainDoc)
        {
            CProductInfo cpi = m_api.FirstOrDefault(v => v.Name == Name);
            if (cpi == null) { return; }


            BlobServiceClient blobServiceClient = new BlobServiceClient("DefaultEndpointsProtocol=https;AccountName=urostorage;AccountKey=RPc4nXEDB05dD3JyBf7djuxvxZfz0SQJnDNIy0BTDBgtkDtZoXul4AG6np4qPMsWoiYkveXcRr4/+AStsLXYAg==;EndpointSuffix=core.windows.net");
            BlobContainerClient containerClient = blobServiceClient.GetBlobContainerClient("template");
            var blobClient = containerClient.GetBlobClient(cpi.Image);


            var tNew = MainDoc.AddTable(1, 2);
            tNew.Design = TableDesign.TableGrid;


            var image = MainDoc.AddImage(blobClient.OpenRead());
            var picture = image.CreatePicture(150, 150);


            var p = tNew.Rows[0].Cells[1].InsertParagraph(cpi.Description);

            p = tNew.Rows[0].Cells[0].InsertParagraph("");
            p.AppendPicture(picture);



            var vt = MainDoc.Tables[1];
            vt.InsertTableAfterSelf(tNew);

        }
    }

    public class CProductInfo
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string Image { get; set; }
    }
}
