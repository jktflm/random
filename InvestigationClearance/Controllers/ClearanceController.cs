using InvestigationClearance.Models;
using Microsoft.AspNetCore.Mvc;
using InvestigationClearance.Models.Domain;
using InvestigationClearance.Data;
using Microsoft.EntityFrameworkCore;
using System.Reflection;
using System.Text.Json.Serialization;
using Newtonsoft.Json.Converters;
using System.Data;
using System.Net.Http.Headers;
using System.Diagnostics;
using Newtonsoft.Json;
using System.Text;
using Microsoft.CodeAnalysis;
using NuGet.Packaging.Licenses;
using System.Configuration;
using Microsoft.WindowsAzure.Storage;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;

namespace InvestigationClearance.Controllers
{

    public class ClearanceController : Controller
    {

        private readonly HttpClient _client;
        private readonly IConfiguration _configuration;

        public ClearanceController(IConfiguration configuration)
        {
            _client = new HttpClient();
            _client.BaseAddress = new Uri("https://localhost:7007/");
            _client.DefaultRequestHeaders.Accept.Clear();
            _client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            _configuration = configuration;
        }

        public async Task<IActionResult> Index()
        {
            try
            {
                List<ClearanceViewModel> clearances = new List<ClearanceViewModel>();
                HttpResponseMessage getData = await _client.GetAsync("api/Clearances");

                if (getData.IsSuccessStatusCode)
                {
                    string results = await getData.Content.ReadAsStringAsync();
                    clearances = JsonConvert.DeserializeObject<List<ClearanceViewModel>>(results);

                    foreach (var clearance in clearances)
                    {
                        if (!string.IsNullOrEmpty(clearance.pictureUrl))
                        {
                            BlobServiceClient blobServiceClient = new BlobServiceClient(_configuration.GetConnectionString("AzureBlobStorage"));
                            Uri blobUri = new Uri(clearance.pictureUrl);
                            string containerName = blobUri.Segments[1].TrimEnd('/');
                            string blobName = string.Join("", blobUri.Segments, 2, blobUri.Segments.Length - 2).TrimEnd('/');

                            BlobContainerClient containerClient = blobServiceClient.GetBlobContainerClient(containerName);
                            BlobClient blobClient = containerClient.GetBlobClient(blobName);

                            BlobDownloadInfo blobDownloadInfo = await blobClient.DownloadAsync();
                            using (var memoryStream = new MemoryStream())
                            {
                                await blobDownloadInfo.Content.CopyToAsync(memoryStream);
                                byte[] blobData = memoryStream.ToArray();

                                BlobContainerClient newContainerClient = blobServiceClient.GetBlobContainerClient("apiimages");
                                BlobClient newBlobClient = newContainerClient.GetBlobClient("apiimages");
                                using (var newStream = new MemoryStream(blobData))
                                {
                                    await newBlobClient.UploadAsync(newStream, overwrite: true);
                                    clearance.pictureUrl = newBlobClient.Uri.ToString(); 
                                }
                            }
                        }
                    }

                    return View(clearances);
                }
                else
                {
                    return View();
                }
            }
            catch (Exception ex)
            {
                return RedirectToAction("Error");
            }
        }

        [HttpGet]
        public async Task<IActionResult> Details(Guid id)
        {
            try
            {
                ClearanceViewModel clearance = new ClearanceViewModel();
                HttpResponseMessage getData = await _client.GetAsync($"api/Clearances/{id}");

                if (getData.IsSuccessStatusCode)
                {
                    string data = getData.Content.ReadAsStringAsync().Result;
                    clearance = JsonConvert.DeserializeObject<ClearanceViewModel>(data);

                    if (!string.IsNullOrEmpty(clearance.pictureUrl))
                    {
                        BlobServiceClient blobServiceClient = new BlobServiceClient(_configuration.GetConnectionString("AzureBlobStorage"));
                        Uri blobUri = new Uri(clearance.pictureUrl);
                        string containerName = blobUri.Segments[1];
                        string blobName = string.Join("", blobUri.Segments, 2, blobUri.Segments.Length - 2);
                        BlobContainerClient containerClient = blobServiceClient.GetBlobContainerClient(containerName);
                        BlobClient blobClient = containerClient.GetBlobClient(blobName);

                        BlobDownloadInfo blobDownloadInfo = await blobClient.DownloadAsync();
                        using (var memoryStream = new MemoryStream())
                        {
                            await blobDownloadInfo.Content.CopyToAsync(memoryStream);
                            byte[] blobData = memoryStream.ToArray();

                            clearance.pictureUrl = "new_image_url_here"; 
                        }
                    }

                    return View(clearance);
                }
                else
                {
                    return View();
                }
            }
            catch (Exception ex)
            {
                return RedirectToAction("Index");
            }
        }
       [HttpGet]
       public async Task<IActionResult> Edit(Guid id)
        {
            try
            {
                ClearanceViewModel clearance = new ClearanceViewModel();
                HttpResponseMessage getData = await _client.GetAsync($"api/clearances/{id}");

                if (getData.IsSuccessStatusCode)
                {
                    string data = getData.Content.ReadAsStringAsync().Result;
                    clearance = JsonConvert.DeserializeObject<ClearanceViewModel>(data);
                }

                return View(clearance);
            }
            catch (Exception ex)
            {
                return RedirectToAction("Error");
            }
        }

        [HttpPost]
        public async Task<IActionResult> Edit(ClearanceViewModel updatedClearance, IFormCollection formCollection)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    foreach (var key in ModelState.Keys)
                    {
                        var state = ModelState[key];
                        if (state.Errors.Any())
                        {
    
                            Console.WriteLine($"Key: {key}, Errors: {string.Join(", ", state.Errors.Select(e => e.ErrorMessage))}");
                        }
                    }
                }


                if (ModelState.IsValid)
                {
                    if (formCollection != null && formCollection.Files.Count > 0)
                    {
                        var blobServiceClient = new BlobServiceClient(_configuration.GetConnectionString("AzureBlobStorage"));
                        var containerName = "apiimages";
                        var newBlobName = "new-picture-" + Guid.NewGuid();
                        var containerClient = blobServiceClient.GetBlobContainerClient(containerName);
                        var blobClient = containerClient.GetBlobClient(newBlobName);

                        using (var memoryStream = new MemoryStream())
                        {
                            var formFile = formCollection.Files[0];
                            await formFile.CopyToAsync(memoryStream);
                            byte[] imageBytes = memoryStream.ToArray();
                            await blobClient.UploadAsync(new MemoryStream(imageBytes));
                        }

                        updatedClearance.pictureUrl = blobClient.Uri.ToString();
                    }

                    var jsonPayload = JsonConvert.SerializeObject(updatedClearance);
                    using (var httpContent = new StringContent(jsonPayload, Encoding.UTF8, "application/json"))
                    {
                        using (HttpResponseMessage response = await _client.PutAsync($"api/Clearances/{updatedClearance.Id}", httpContent))
                        {
                            if (response.IsSuccessStatusCode)
                            {
                                return RedirectToAction("Index");
                            }
                            else
                            {
                                return View(updatedClearance);
                            }
                        }
                    }
                }
                else
                {
                    return View(updatedClearance);
                }
            }
            catch (Exception ex)
            {
                return RedirectToAction("Error");
            }
        }



        [HttpGet]
        public async Task<IActionResult> Delete(Guid id)
        {
            try
            {
                ClearanceViewModel clearance = new ClearanceViewModel();
                HttpResponseMessage getData = await _client.GetAsync($"api/clearances/{id}");

                if (getData.IsSuccessStatusCode)
                {
                    string data = getData.Content.ReadAsStringAsync().Result;
                    clearance = JsonConvert.DeserializeObject<ClearanceViewModel>(data);
                }

                return View(clearance);
            }
            catch (Exception ex)
            {
                return RedirectToAction("Error");
            }
        }

        [HttpPost]
        public async Task<IActionResult> ConfirmDelete(Guid id)
        {
            try
            {
                HttpResponseMessage response = await _client.DeleteAsync($"api/Clearances/{id}");

                if (response.IsSuccessStatusCode)
                {
                    return RedirectToAction("Index");
                }
                else
                {
                    return View();
                }
            }
            catch (Exception ex)
            {
                return RedirectToAction("Error");
            }
        }
        [HttpGet]
        public IActionResult Add()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Add(ClearanceViewModel newClearance, IFormFile picture)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var newBlobServiceClient = new BlobServiceClient(_configuration.GetConnectionString("AzureBlobStorage"));

        
                    if (picture != null)
                    {
                        var newBlobName = Guid.NewGuid().ToString();
                        string newContainerName = "apiimages"; 
                        BlobContainerClient newContainerClient = newBlobServiceClient.GetBlobContainerClient(newContainerName);
                        BlobClient newBlobClient = newContainerClient.GetBlobClient(newBlobName);
                        await newBlobClient.UploadAsync(picture.OpenReadStream(), true);

                      
                        newClearance.pictureUrl = newBlobClient.Uri.ToString();
                    }

                    
                    string jsonPayload = JsonConvert.SerializeObject(newClearance);
                    StringContent httpContent = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

              
                    HttpResponseMessage response = await _client.PostAsync("api/Clearances", httpContent);

                    if (response.IsSuccessStatusCode)
                    {
                        return RedirectToAction("Index");
                    }
                    else
                    {
                        return View(newClearance);
                    }
                }
                else
                {
                    return View(newClearance);
                }
            }
            catch (Exception ex)
            {
                return RedirectToAction("Error");
            }
        }

        public async Task<IActionResult> Update()
        {
            try
            {
                List<ClearanceViewModel> clearances = new List<ClearanceViewModel>();
                HttpResponseMessage getData = await _client.GetAsync("api/Clearances");

                if (getData.IsSuccessStatusCode)
                {
                    string results = await getData.Content.ReadAsStringAsync();
                    clearances = JsonConvert.DeserializeObject<List<ClearanceViewModel>>(results);

                    foreach (var clearance in clearances)
                    {
                        if (!string.IsNullOrEmpty(clearance.pictureUrl))
                        {
                            BlobServiceClient blobServiceClient = new BlobServiceClient(_configuration.GetConnectionString("AzureBlobStorage"));
                            Uri blobUri = new Uri(clearance.pictureUrl);
                            string containerName = blobUri.Segments[1].TrimEnd('/');
                            string blobName = string.Join("", blobUri.Segments, 2, blobUri.Segments.Length - 2).TrimEnd('/');

                            BlobContainerClient containerClient = blobServiceClient.GetBlobContainerClient(containerName);
                            BlobClient blobClient = containerClient.GetBlobClient(blobName);

                            BlobDownloadInfo blobDownloadInfo = await blobClient.DownloadAsync();
                            using (var memoryStream = new MemoryStream())
                            {
                                await blobDownloadInfo.Content.CopyToAsync(memoryStream);
                                byte[] blobData = memoryStream.ToArray();

                                
                                BlobContainerClient newContainerClient = blobServiceClient.GetBlobContainerClient("apiimages");
                                BlobClient newBlobClient = newContainerClient.GetBlobClient("apiimages");
                                using (var newStream = new MemoryStream(blobData))
                                {
                                    await newBlobClient.UploadAsync(newStream, overwrite: true);
                                    clearance.pictureUrl = newBlobClient.Uri.ToString(); 
                                }
                            }
                        }
                    }

                    return View(clearances);
                }
                else
                {
                    return View();
                }
            }
            catch (Exception ex)
            {
                return RedirectToAction("Error");
            }
        }
        public IActionResult Error()
        {
            return View();
        }
        public async Task<IActionResult> List(string searchTerm)
        {
            try
            {
                searchTerm = searchTerm?.Trim();
                HttpResponseMessage getData = await _client.GetAsync("api/Clearances");

                if (getData.IsSuccessStatusCode)
                {
                    string results = await getData.Content.ReadAsStringAsync();
                    List<ClearanceViewModel> clearances = JsonConvert.DeserializeObject<List<ClearanceViewModel>>(results);


                    if (!string.IsNullOrEmpty(searchTerm))
                    {
                        clearances = clearances.Where(c =>
                            c.FirstName.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ||
                            c.LastName.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ||
                            c.MiddleName.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ||
                            c.Email.Contains(searchTerm, StringComparison.OrdinalIgnoreCase)

                        ).ToList();
                    }

                    return View("Index", clearances);
                }
                else
                {
                    return View("Index");
                }
            }
            catch (Exception ex)
            {
                return RedirectToAction("Error");
            }
        }
        public async Task<IActionResult> ListEdit(string searchTerm)
        {
            try
            {
                searchTerm = searchTerm?.Trim();
                HttpResponseMessage getData = await _client.GetAsync("api/Clearances");

                if (getData.IsSuccessStatusCode)
                {
                    string results = await getData.Content.ReadAsStringAsync();
                    List<ClearanceViewModel> clearances = JsonConvert.DeserializeObject<List<ClearanceViewModel>>(results);


                    if (!string.IsNullOrEmpty(searchTerm))
                    {
                        clearances = clearances.Where(c =>
                            c.FirstName.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ||
                            c.LastName.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ||
                            c.MiddleName.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ||
                            c.Email.Contains(searchTerm, StringComparison.OrdinalIgnoreCase)

                        ).ToList();
                    }

                    return View("Update", clearances);
                }
                else
                {
                    return View("Update");
                }
            }
            catch (Exception ex)
            {
                return RedirectToAction("Error");
            }
        }

    }

}


