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
using Newtonsoft.Json;
using Microsoft.AspNetCore.Hosting;

namespace iCareWebApplication.Controllers
{
    public class PatientTreatmentController : Controller
    {
        private readonly iCareContext _context;
        private readonly IWebHostEnvironment _environment;

        public PatientTreatmentController(iCareContext context, IWebHostEnvironment environment)
        {
            _context = context;
            _environment = environment;
        }

        // GET: /PatientTreatment/Index/{patientId} - View all treatments for a specific patient
        public async Task<IActionResult> Index(int patientId)
        {
            var drugs = await _context.Drugs.ToListAsync();
            ViewBag.Drugs = drugs;
            // Retrieve all treatments for the specified patient
            var treatments = await _context.PatientTreatment
                .Where(t => t.PatientId == patientId)
                .ToListAsync();

            ViewBag.PatientId = patientId; // Pass the PatientId to the view for linking
            return View(treatments);
        }

        // GET: /PatientTreatment/Create/{patientId} - Display the form to create a new treatment for a specific patient
        public async Task<IActionResult> Create(int patientId)
        {
            var drugs = await _context.Drugs.ToListAsync();

            // Set ViewBag.Drugs to pass the list of drugs to the view
            ViewBag.Drugs = drugs;
            var treatment = new PatientTreatment
            {
                PatientId = patientId,
                TreatmentDate = DateTime.Now // Set default treatment date to today
            };
            return View(treatment);
        }

        // POST: /PatientTreatment/Create - Handle form submission to create a new treatment record
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(PatientTreatment treatment, List<IFormFile> imageFiles)
        {
            if (ModelState.IsValid)
            {
                // Retrieve the worker ID from the session
                int? workerId = HttpContext.Session.GetInt32("UserId");
                if (workerId == null)
                {
                    return RedirectToAction("Login", "Account");
                }
                ViewBag.Drugs = await _context.Drugs.ToListAsync();
                treatment.WorkerId = workerId.Value; // Set the worker ID
                _context.PatientTreatment.Add(treatment); // Add the new treatment record
                await _context.SaveChangesAsync();

                // Store image files temporarily
                var tempImagePaths = new List<string>();
                foreach (var imageFile in imageFiles)
                {
                    if (imageFile.Length > 0)
                    {
                        var fileName = Path.GetRandomFileName() + Path.GetExtension(imageFile.FileName);
                        var filePath = Path.Combine(_environment.WebRootPath, "temp", fileName);

                        Directory.CreateDirectory(Path.GetDirectoryName(filePath)); // Ensure the directory exists

                        using (var stream = new FileStream(filePath, FileMode.Create))
                        {
                            await imageFile.CopyToAsync(stream);
                        }
                        tempImagePaths.Add(filePath);
                    }
                }

                // Store temp image paths in TempData
                TempData["TempImagePaths"] = JsonConvert.SerializeObject(tempImagePaths);

                return RedirectToAction(nameof(Index), new { patientId = treatment.PatientId });
            }
            return View(treatment);
        }

        public async Task<IActionResult> DownloadPdf(int id)
        {
            var treatment = await _context.PatientTreatment.FindAsync(id);
            if (treatment == null)
            {
                return NotFound();
            }

            using (MemoryStream ms = new MemoryStream())
            {
                Document document = new Document(PageSize.A4, 25, 25, 30, 30);
                PdfWriter writer = PdfWriter.GetInstance(document, ms);
                document.Open();

                // Add text
                document.Add(new Paragraph($"Treatment Date: {treatment.TreatmentDate}"));
                document.Add(new Paragraph($"Description: {treatment.Description}"));

                // Add images if available
                var tempImagePathsJson = TempData["TempImagePaths"] as string;
                if (!string.IsNullOrEmpty(tempImagePathsJson))
                {
                    var tempImagePaths = JsonConvert.DeserializeObject<List<string>>(tempImagePathsJson);
                    foreach (var imagePath in tempImagePaths)
                    {
                        if (System.IO.File.Exists(imagePath))
                        {
                            iTextSharp.text.Image image = iTextSharp.text.Image.GetInstance(imagePath);
                            image.ScaleToFit(400f, 300f);
                            document.Add(image);

                            // Delete the temporary file
                            System.IO.File.Delete(imagePath);
                        }
                    }
                }

                document.Close();
                writer.Close();

                return File(ms.ToArray(), "application/pdf", $"Treatment_{id}.pdf");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteSelected(List<int> selectedTreatments, int patientId)
        {
            if (selectedTreatments != null && selectedTreatments.Any())
            {
                var treatmentsToDelete = await _context.PatientTreatment
                    .Where(t => selectedTreatments.Contains(t.PatientTreatmentId))
                    .ToListAsync();

                _context.PatientTreatment.RemoveRange(treatmentsToDelete);
                await _context.SaveChangesAsync();

                TempData["Message"] = $"{treatmentsToDelete.Count} treatment(s) have been deleted.";
            }

            return RedirectToAction(nameof(Index), new { patientId });
        }
    }
}