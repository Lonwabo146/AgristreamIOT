using AgristreamIOT.Models;
using Azure.Data.Tables;
using Azure.Storage.Blobs;
using Azure.Storage.Sas;
using Microsoft.AspNetCore.Mvc;
using Azure.Storage.Sas;
public class FarmController : Controller
{
    private readonly string _conn;
    public FarmController(IConfiguration config)
        => _conn = config.GetConnectionString("StorageConnection");
    public IActionResult Gallery()
    {
        var container = new BlobContainerClient(_conn, "farm-media");
        var urls = new List<string>();
        foreach (var item in container.GetBlobs())
        {
            var sas = container.GetBlobClient(item.Name)
                .GenerateSasUri(BlobSasPermissions.Read, DateTimeOffset.UtcNow.AddMinutes(30));
            urls.Add(sas.ToString());
        }
        return View(urls);
    }

    public IActionResult Readings(string sensorId)
    {
        var table = new TableClient(_conn, "SensorReadings");
        var list = string.IsNullOrEmpty(sensorId)
            ? table.Query<SensorReadings>().Take(20).ToList()
            : table.Query<SensorReadings>(r => r.PartitionKey == sensorId).Take(20).ToList();
        ViewBag.SensorId = sensorId;
        return View(list);
    }
    [HttpGet]
    public IActionResult Upload() => View();

    [HttpPost]
    public async Task<IActionResult> Upload(IFormFile file)
    {
        var container = new BlobContainerClient(_conn, "farm-media");
        await container.CreateIfNotExistsAsync();
        await container.GetBlobClient(file.FileName)
            .UploadAsync(file.OpenReadStream(), overwrite: true);
        ViewBag.Message = "Uploaded " + file.FileName;
        return View();
    }


    [HttpGet]
    public IActionResult Telemetry() => View();

    [HttpPost]
    public async Task<IActionResult> Telemetry(string sensorId, string type, double value)
    {
        var table = new TableClient(_conn, "SensorReadings");
        await table.CreateIfNotExistsAsync();
        await table.AddEntityAsync(new SensorReadings
        {
            PartitionKey = sensorId,
            RowKey = (DateTime.MaxValue.Ticks - DateTime.UtcNow.Ticks).ToString("d19"),
            Type = type,
            Value = value
        });
        ViewBag.Message = "Reading saved";
        return View();
    }
}