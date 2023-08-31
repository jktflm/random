using Azure.Storage.Blobs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using WebAPI.Data;
using WebAPI.Models;

namespace WebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ClearancesController : Controller
    {
        private readonly ClearancesAPIDbContext dbContext;
        private readonly IConfiguration _configuration;

        public ClearancesController(ClearancesAPIDbContext dbContext, IConfiguration configuration)
        {
            this.dbContext = dbContext;
            _configuration = configuration;
        }
        [HttpGet]
        public async Task<IActionResult> GetClearances()
        {
            return Ok(await dbContext.Clearances.ToListAsync());
        }

        [HttpGet]
        [Route("{id:guid}")]
        public async Task<IActionResult> GetClearance([FromRoute] Guid id)
        {
            var clearance = await dbContext.Clearances.FindAsync(id);

            if (clearance == null)
            {
                return NotFound();
            }
            return Ok(clearance);
        }
        [HttpPost]
        public async Task<IActionResult> AddClearances([FromForm] AddClearanceRequest addClearanceRequest)
        {
            var pictureFile = addClearanceRequest.Picture;
            var clearance = new Clearance()
            {
                Id = Guid.NewGuid(),
                BranchName = addClearanceRequest.BranchName,
                Date = addClearanceRequest.Date,
                IdPresented = addClearanceRequest.IdPresented,
                LastName = addClearanceRequest.LastName,
                FirstName = addClearanceRequest.FirstName,
                MiddleName = addClearanceRequest.MiddleName,
                DateOfBirth = addClearanceRequest.DateOfBirth,
                Gender = addClearanceRequest.Gender,
                CivilStatus = addClearanceRequest.CivilStatus,
                EducationalAttainment = addClearanceRequest.EducationalAttainment,
                LandlineNumber = addClearanceRequest.LandlineNumber,
                MobileNumber = addClearanceRequest.MobileNumber,
                Email = addClearanceRequest.Email,
                Complexion = addClearanceRequest.Complexion,
                Marks = addClearanceRequest.Marks,
                Religion = addClearanceRequest.Religion,
                Height = addClearanceRequest.Height,
                Weight = addClearanceRequest.Weight,
                SpouseName = addClearanceRequest.SpouseName,
                FatherName = addClearanceRequest.FatherName,
                FatherPlaceOfBirth = addClearanceRequest.FatherPlaceOfBirth,
                MotherName = addClearanceRequest.MotherName,
                MotherPlaceOfBirth = addClearanceRequest.MotherPlaceOfBirth
            };

            if (pictureFile != null && pictureFile.Length > 0)
            {
                var blobServiceClient = new BlobServiceClient(_configuration.GetConnectionString("AzureBlobStorage"));
                var blobContainerClient = blobServiceClient.GetBlobContainerClient("apiimages");

                var fileName = Guid.NewGuid().ToString() + Path.GetExtension(pictureFile.FileName);
                var blobClient = blobContainerClient.GetBlobClient(fileName);

                using (var stream = pictureFile.OpenReadStream())
                {
                    await blobClient.UploadAsync(stream, true);
                }

                clearance.PictureUrl = blobClient.Uri.ToString();
            }
            await dbContext.Clearances.AddAsync(clearance);
            await dbContext.SaveChangesAsync();

            return Ok(clearance);
        }

        [HttpPut]
        [Route("{id:guid}")]
        public async Task<IActionResult> UpdateClearance([FromRoute] Guid id, UpdateClearanceRequest updateClearanceRequest, IFormFile newPicture)
        {
            var clearance = await dbContext.Clearances.FindAsync(id);

            if (clearance != null)
            {
                clearance.BranchName = updateClearanceRequest.BranchName;
                clearance.Date = updateClearanceRequest.Date;
                clearance.IdPresented = updateClearanceRequest.IdPresented;
                clearance.LastName = updateClearanceRequest.LastName;
                clearance.FirstName = updateClearanceRequest.FirstName;
                clearance.MiddleName = updateClearanceRequest.MiddleName;
                clearance.PictureUrl = updateClearanceRequest.PictureUrl;
                clearance.DateOfBirth = updateClearanceRequest.DateOfBirth;
                clearance.Gender = updateClearanceRequest.Gender;
                clearance.CivilStatus = updateClearanceRequest.CivilStatus;
                clearance.EducationalAttainment = updateClearanceRequest.EducationalAttainment;
                clearance.LandlineNumber = updateClearanceRequest.LandlineNumber;
                clearance.MobileNumber = updateClearanceRequest.MobileNumber;
                clearance.Email = updateClearanceRequest.Email;
                clearance.Complexion = updateClearanceRequest.Complexion;
                clearance.Marks = updateClearanceRequest.Marks;
                clearance.Religion = updateClearanceRequest.Religion;
                clearance.Height = updateClearanceRequest.Height;
                clearance.Weight = updateClearanceRequest.Weight;
                clearance.SpouseName = updateClearanceRequest.SpouseName;
                clearance.FatherName = updateClearanceRequest.FatherName;
                clearance.FatherPlaceOfBirth = updateClearanceRequest.FatherPlaceOfBirth;
                clearance.MotherName = updateClearanceRequest.MotherName;
                clearance.MotherPlaceOfBirth = updateClearanceRequest.MotherPlaceOfBirth;

                if (newPicture != null)
                {
                    var newBlobServiceClient = new BlobServiceClient(_configuration.GetConnectionString("AzureBlobStorage"));

                    if (!string.IsNullOrEmpty(clearance.PictureUrl))
                    {
                        var blobUri = new Uri(clearance.PictureUrl);
                        string containerName = blobUri.Segments[1];
                        string blobName = string.Join("", blobUri.Segments, 2, blobUri.Segments.Length - 2);

                        BlobContainerClient existingContainerClient = newBlobServiceClient.GetBlobContainerClient(containerName);
                        BlobClient existingBlobClient = existingContainerClient.GetBlobClient(blobName);

                        await existingBlobClient.DeleteIfExistsAsync();
                    }

                    var newBlobName = Guid.NewGuid().ToString();
                    string newContainerName = "apiimages"; 
                    BlobContainerClient newContainerClient = newBlobServiceClient.GetBlobContainerClient(newContainerName);
                    BlobClient newBlobClient = newContainerClient.GetBlobClient(newBlobName);
                    await newBlobClient.UploadAsync(newPicture.OpenReadStream(), true);

                    clearance.PictureUrl = newBlobClient.Uri.ToString();
                }

                await dbContext.SaveChangesAsync();

                return Ok(clearance);
            }
            return NotFound();
        }

        [HttpDelete]
        [Route("{id:guid}")]
        public async Task<IActionResult> DeleteClearance([FromRoute] Guid id)
        {
            var clearance = await dbContext.Clearances.FindAsync(id);

            if (clearance != null)
            {
                if (!string.IsNullOrEmpty(clearance.PictureUrl))
                {

                    var blobUri = new Uri(clearance.PictureUrl);
                    string containerName = blobUri.Segments[1];
                    string blobName = string.Join("", blobUri.Segments, 2, blobUri.Segments.Length - 2);

                    BlobServiceClient blobServiceClient = new BlobServiceClient(_configuration.GetConnectionString("AzureBlobStorage"));
                    BlobContainerClient containerClient = blobServiceClient.GetBlobContainerClient(containerName);
                    BlobClient blobClient = containerClient.GetBlobClient(blobName);
                    await blobClient.DeleteIfExistsAsync();
                }

                dbContext.Remove(clearance);
                await dbContext.SaveChangesAsync();
                return Ok(clearance);
            }
            return NotFound();
        }

    }
}
