using System;
using System.Collections.Generic;
using System.Text;

namespace Lykke.Service.PdfGenerator.Contract
{
    public class PdfDataResponse
    {
        public byte[] Data { get; set; }
        public string FileName { get; set; }
    }
}
