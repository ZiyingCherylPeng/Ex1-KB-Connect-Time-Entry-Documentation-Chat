


    // private static readonly string formRecognizerEndpoint = "https://ex1pdftojson.cognitiveservices.azure.com/";
    // private static readonly string formRecognizerKey = "25a11562c03742e791ded8122af16af1";
    // private static readonly string fileUri = "https://ex1kbtimeentry.blob.core.windows.net/pdffiles/test.pdf";

    // private static readonly string computerVisionEndpoint = "https://ex1pdfimagetojson.cognitiveservices.azure.com/";
    // private static readonly string computerVisionKey = "b50900ecdf08417a94498d9a7a105324";


using Azure;
using Azure.AI.FormRecognizer.DocumentAnalysis;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;

class Program
{
    private static readonly string formRecognizerEndpoint = "https://ex1pdftojson.cognitiveservices.azure.com/";
    private static readonly string formRecognizerKey = "25a11562c03742e791ded8122af16af1";
    private static readonly string fileUri = "https://ex1kbtimeentry.blob.core.windows.net/pdffiles/test.pdf"; 

    static async Task Main(string[] args)
    {
        AzureKeyCredential formRecognizerCredential = new AzureKeyCredential(formRecognizerKey);
        DocumentAnalysisClient formRecognizerClient = new DocumentAnalysisClient(new Uri(formRecognizerEndpoint), formRecognizerCredential);

        AnalyzeDocumentOperation operation = await formRecognizerClient.AnalyzeDocumentFromUriAsync(WaitUntil.Completed, "prebuilt-layout", new Uri(fileUri));
        AnalyzeResult result = operation.Value;

        var output = new List<object>();

        foreach (DocumentPage page in result.Pages)
        {
            var pageContent = new List<object>();

            // 提取文字内容
            foreach (DocumentLine line in page.Lines)
            {
                pageContent.Add(new
                {
                    type = "text",
                    content = line.Content,
                    boundingBox = line.BoundingPolygon
                });
            }

            // 提取表格内容
            foreach (DocumentTable table in result.Tables)
            {
                if (table.BoundingRegions.Any(region => region.PageNumber == page.PageNumber))
                {
                    var tableData = new
                    {
                        type = "table",
                        rows = new List<object>()
                    };

                    foreach (DocumentTableCell cell in table.Cells)
                    {
                        if (cell.BoundingRegions.Any(region => region.PageNumber == page.PageNumber))
                        {
                            tableData.rows.Add(new
                            {
                                rowIndex = cell.RowIndex,
                                columnIndex = cell.ColumnIndex,
                                content = cell.Content,
                                boundingBox = cell.BoundingRegions
                            });
                        }
                    }

                    pageContent.Add(tableData);
                }
            }

            output.Add(new
            {
                pageNumber = page.PageNumber,
                content = pageContent
            });
        }

        string jsonString = JsonSerializer.Serialize(output, new JsonSerializerOptions { WriteIndented = true });
        await File.WriteAllTextAsync("output2.json", jsonString);

        Console.WriteLine("Output written to output.json");
    }
}
