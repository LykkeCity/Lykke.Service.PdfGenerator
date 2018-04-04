namespace Lykke.Service.PdfGenerator.Contract
{
    public class PdfGenerateModel
    {
        /// <summary>
        /// HTML which should be converted to PDF
        /// </summary>
        public string HtmlSource { get; set; }
        /// <summary>
        /// Name of the blob file in the storage
        /// </summary>
        public string BlobName { get; set; }
        /// <summary>
        /// Additional metadata name
        /// </summary>
        public string FileName { get; set; }
    }
}
