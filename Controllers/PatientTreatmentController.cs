using iCareWebApplication.Data;
using iCareWebApplication.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using iTextSharp.text;
using iTextSharp.text.pdf;
using Microsoft.AspNetCore.Hosting;

namespace iCareWebApplication.Controllers
{
    public class PatientTreatmentController : Controller
    {
        private readonly iCareContext _context;
        private readonly IWebHostEnvironment _hostingEnvironment;

        // Constructor to initialize context and hosting environment
        public PatientTreatmentController(iCareContext context, IWebHostEnvironment hostingEnvironment)
        {
            _context = context;
            _hostingEnvironment = hostingEnvironment;
        }

        // GET: /PatientTreatment/Index/{patientId} - Displays a list of treatments for a specific patient
        public async Task<IActionResult> Index(int patientId)
        {
            // Fetch all drugs and store in ViewBag for easy access in views
            var drugs = await _context.Drugs.ToListAsync();
            ViewBag.Drugs = drugs;

            // Retrieve treatments specific to the given patient ID
            var treatments = await _context.Set<PatientTreatment>()
                .Where(t => t.PatientId == patientId)
                .ToListAsync();

            ViewBag.PatientId = patientId; // Pass patientId to view
            return View(treatments); // Render the treatments list
        }

        // GET: /PatientTreatment/Create/{patientId} - Displays a form to create a new treatment for a specific patient
        public async Task<IActionResult> Create(int patientId)
        {
            // Fetch available drugs and pass them to the view
            var drugs = await _context.Drugs.ToListAsync();
            ViewBag.Drugs = drugs;

            // Initialize a new treatment instance with current date
            var treatment = new PatientTreatment
            {
                PatientId = patientId,
                TreatmentDate = DateTime.Now
            };
            return View(treatment);
        }

        // POST: /PatientTreatment/Create - Handles form submission to create a new treatment record
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(PatientTreatment treatment, List<IFormFile> imageFiles)
        {
            if (ModelState.IsValid)
            {
                // Retrieve the worker ID from session and set it in the treatment
                int? workerId = HttpContext.Session.GetInt32("UserId");
                if (workerId == null)
                {
                    return RedirectToAction("Login", "Account");
                }
                treatment.WorkerId = workerId.Value;

                // Add the treatment record to the database
                _context.Set<PatientTreatment>().Add(treatment);
                await _context.SaveChangesAsync();

                // Save images associated with the treatment
                var imageDirectory = Path.Combine(_hostingEnvironment.WebRootPath, "images", "treatments", treatment.PatientTreatmentId.ToString());
                if (!Directory.Exists(imageDirectory))
                {
                    Directory.CreateDirectory(imageDirectory);
                }

                // Process each uploaded image file
                foreach (var imageFile in imageFiles)
                {
                    if (imageFile.Length > 0)
                    {
                        var fileName = Path.GetRandomFileName() + Path.GetExtension(imageFile.FileName);
                        var filePath = Path.Combine(imageDirectory, fileName);

                        // Save the image file to the server
                        using (var stream = new FileStream(filePath, FileMode.Create))
                        {
                            await imageFile.CopyToAsync(stream);
                        }
                    }
                }

                // Redirect to the treatment index for the patient after saving
                return RedirectToAction(nameof(Index), new { patientId = treatment.PatientId });
            }
            return View(treatment); // If model is invalid, redisplay the form
        }

        // GET: /PatientTreatment/DownloadPdf/{id} - Generates and downloads a PDF for a specific treatment
        public async Task<IActionResult> DownloadPdf(int id)
        {
            // Retrieve the treatment record by ID
            var treatment = await _context.Set<PatientTreatment>().FindAsync(id);
            if (treatment == null)
            {
                return NotFound();
            }

            // Fetch the drug associated with the treatment
            var drug = await _context.Drugs.FirstOrDefaultAsync(d => d.DrugId == treatment.DrugId);
            string drugName = drug?.DrugName ?? "N/A"; // Display "N/A" if drug not found
            string drugDescription = drug?.Description ?? "N/A"; // Display "N/A" if drug not found

            using (MemoryStream ms = new MemoryStream())
            {
                // Initialize PDF document
                Document document = new Document(PageSize.A4, 25, 25, 30, 30);
                PdfWriter writer = PdfWriter.GetInstance(document, ms);
                document.Open();

                // Add treatment details to the PDF
                document.Add(new Paragraph($"Treatment Date: {treatment.TreatmentDate}"));
                document.Add(new Paragraph($"Description: {treatment.Description}"));

                // Add drug information to the PDF
                document.Add(new Paragraph($"Drug Name: {drugName}"));
                document.Add(new Paragraph($"Drug Description: {drugDescription}"));

                // Add images associated with the treatment
                var imageDirectory = Path.Combine(_hostingEnvironment.WebRootPath, "images", "treatments", id.ToString());
                if (Directory.Exists(imageDirectory))
                {
                    var imagePaths = Directory.GetFiles(imageDirectory);
                    foreach (var imagePath in imagePaths)
                    {
                        if (System.IO.File.Exists(imagePath))
                        {
                            iTextSharp.text.Image image = iTextSharp.text.Image.GetInstance(imagePath);
                            image.ScaleToFit(400f, 300f);
                            document.Add(image); // Add each image to the PDF
                        }
                    }
                }

                document.Close();
                writer.Close();

                // Return the generated PDF file
                return File(ms.ToArray(), "application/pdf", $"Treatment_{id}.pdf");
            }
        }

        // POST: /PatientTreatment/DeleteSelected - Deletes selected treatments and their associated images
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteSelected(List<int> selectedTreatments, int patientId)
        {
            if (selectedTreatments != null && selectedTreatments.Any())
            {
                // Retrieve the selected treatments from the database
                var treatmentsToDelete = await _context.Set<PatientTreatment>()
                    .Where(t => selectedTreatments.Contains(t.PatientTreatmentId))
                    .ToListAsync();

                foreach (var treatment in treatmentsToDelete)
                {
                    // Delete associated images from the server
                    var imageDirectory = Path.Combine(_hostingEnvironment.WebRootPath, "images", "treatments", treatment.PatientTreatmentId.ToString());
                    if (Directory.Exists(imageDirectory))
                    {
                        Directory.Delete(imageDirectory, true);
                    }
                }

                // Remove treatment records from the database
                _context.Set<PatientTreatment>().RemoveRange(treatmentsToDelete);
                await _context.SaveChangesAsync();

                TempData["Message"] = $"{treatmentsToDelete.Count} treatment(s) have been deleted.";
            }

            // Redirect to the treatment index for the patient
            return RedirectToAction(nameof(Index), new { patientId });
        }

        // GET: /PatientTreatment/Edit/{id} - Displays a form to edit an existing treatment
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            // Retrieve the treatment record by ID
            var patientTreatment = await _context.Set<PatientTreatment>().FindAsync(id);
            if (patientTreatment == null)
            {
                return NotFound();
            }

            // Populate ViewBag with available drugs for dropdown selection
            ViewBag.Drugs = _context.Drugs.Select(d => new { d.DrugId, d.DrugName }).ToList();

            // Retrieve paths of images associated with the treatment
            var imageDirectory = Path.Combine(_hostingEnvironment.WebRootPath, "images", "treatments", id.ToString());
            var imagePaths = Directory.Exists(imageDirectory)
                ? Directory.GetFiles(imageDirectory).Select(Path.GetFileName).ToList()
                : new List<string>();

            ViewBag.ImagePaths = imagePaths;

            return View(patientTreatment); // Render the edit form
        }

        // POST: PatientTreatment/Edit/5 - Handles form submission to edit an existing treatment record
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, PatientTreatment patientTreatment, int drugId, IFormFile[] imageFiles, string[] removeImages)
        {
            // Check if the provided ID matches the PatientTreatment ID
            if (id != patientTreatment.PatientTreatmentId)
            {
                return NotFound(); // Return 404 if IDs do not match
            }

            // Verify that the model state is valid
            if (ModelState.IsValid)
            {
                try
                {
                    // Update the treatment record in the database
                    _context.Update(patientTreatment);

                    // Define the directory where images are stored for this treatment
                    var imageDirectory = Path.Combine(_hostingEnvironment.WebRootPath, "images", "treatments", id.ToString());
                    if (!Directory.Exists(imageDirectory))
                    {
                        Directory.CreateDirectory(imageDirectory); // Create directory if it doesn't exist
                    }

                    // Remove selected images from the directory
                    if (removeImages != null)
                    {
                        foreach (var imagePath in removeImages)
                        {
                            var fullPath = Path.Combine(imageDirectory, imagePath);
                            if (System.IO.File.Exists(fullPath))
                            {
                                System.IO.File.Delete(fullPath); // Delete each selected image
                            }
                        }
                    }

                    // Save any new images uploaded with the form
                    foreach (var imageFile in imageFiles)
                    {
                        if (imageFile.Length > 0) // Check that the image file has content
                        {
                            // Generate a unique name for the image file and define the path
                            var imageFileName = $"{Guid.NewGuid()}_{imageFile.FileName}";
                            var filePathToSave = Path.Combine(imageDirectory, imageFileName);

                            // Save the image file to the server
                            using (var stream = new FileStream(filePathToSave, FileMode.Create))
                            {
                                await imageFile.CopyToAsync(stream);
                            }
                        }
                    }

                    // Save changes to the database
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    // Handle concurrency issues (e.g., if the record was modified by another process)
                    if (!PatientTreatmentExists(patientTreatment.PatientTreatmentId))
                    {
                        return NotFound(); // Return 404 if the treatment record no longer exists
                    }
                    else
                    {
                        throw; // Re-throw exception if it's an unexpected concurrency error
                    }
                }

                // Redirect to the index view for treatments, passing the patient ID for context
                return RedirectToAction(nameof(Index), new { patientId = patientTreatment.PatientId });
            }

            // If model validation fails, repopulate the drugs dropdown and re-render the form
            ViewBag.Drugs = _context.Drugs.Select(d => new { d.DrugId, d.DrugName }).ToList();
            return View(patientTreatment); // Re-render the form with validation errors
        }

        // Helper method to check if a PatientTreatment exists by ID
        private bool PatientTreatmentExists(int id)
        {
            return _context.Set<PatientTreatment>().Any(e => e.PatientTreatmentId == id);
        }

    }
}