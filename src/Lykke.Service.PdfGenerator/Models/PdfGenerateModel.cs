using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Lykke.Service.PdfGenerator.Models
{
    public class PdfGenerateModel
    {
        public string HtmlSource { get; set; }
        public string BlobName { get; set; }
        public string FileName { get; set; }
    }
}