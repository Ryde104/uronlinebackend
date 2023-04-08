using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Quoteenator3000Backend
{
    public class CQuote
    {
        public string CompanyName { get; set; }
        public string ObjectDesc { get; set; }
        public decimal ObjectPrice { get; set; }

        public CProduct[] Products { get; set; }
    }

    public class CProduct
    {
        public decimal Price { get; set; }
        public string WordDoc { get; set; }
        public string Name { get; set; }
        public int Quantity { get; set; }

        public string Image { get; set; }

        public string ItemDescription { get; set; }
    }
}
