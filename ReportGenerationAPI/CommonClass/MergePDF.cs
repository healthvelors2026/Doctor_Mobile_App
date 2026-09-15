using iTextSharp.text;
using iTextSharp.text.pdf;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;


namespace ReportGenerationAPI.CommonClass
{
    public class MergePDF
    {
        #region Public Methods

        public static string MergeFiles(
            List<string> pdfFilePaths,
            string outputFilePath)
        {
            if (pdfFilePaths == null || pdfFilePaths.Count == 0)
            {
                throw new ArgumentException("No PDF files provided.");
            }

            if (string.IsNullOrWhiteSpace(outputFilePath))
            {
                throw new ArgumentException("Output PDF path is required.");
            }

            string outputDirectory = Path.GetDirectoryName(outputFilePath);

            if (!Directory.Exists(outputDirectory))
            {
                Directory.CreateDirectory(outputDirectory);
            }

            Document document = new Document();

            try
            {
                using (FileStream fileStream = new FileStream(
                    outputFilePath,
                    FileMode.Create,
                    FileAccess.Write,
                    FileShare.None))
                {
                    PdfCopy copy = new PdfCopy(document, fileStream);

                    document.Open();

                    foreach (string pdfFilePath in pdfFilePaths)
                    {
                        if (string.IsNullOrWhiteSpace(pdfFilePath) ||
                            !File.Exists(pdfFilePath))
                        {
                            continue;
                        }

                        PdfReader reader = new PdfReader(pdfFilePath);

                        try
                        {
                            if (reader.NumberOfPages == 0)
                            {
                                continue;
                            }

                            for (int pageIndex = 1;
                                 pageIndex <= reader.NumberOfPages;
                                 pageIndex++)
                            {
                                PdfImportedPage importedPage =
                                    copy.GetImportedPage(reader, pageIndex);

                                copy.AddPage(importedPage);
                            }

                            copy.FreeReader(reader);
                        }
                        finally
                        {
                            reader.Close();
                        }
                    }

                    document.Close();
                }

                return outputFilePath;
            }
            catch
            {
                if (document.IsOpen())
                {
                    document.Close();
                }

                throw;
            }
        }

        #endregion
    }
}