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

        public PatientTreatmentController(iCareContext context, IWebHostEnvironment hostingEnvironment)
        {
            _context = context;
            _hostingEnvironment = hostingEnvironment;
        }

        // GET: /PatientTreatment/Index/{patientId}
        public async Task<IActionResult> Index(int patientId)
        {
            var drugs = await _context.Drugs.ToListAsync();
            ViewBag.Drugs = drugs;
            var treatments = await _context.Set<PatientTreatment>()
                .Where(t => t.PatientId == patientId)
                .ToListAsync();

            ViewBag.PatientId = patientId;
            return View(treatments);
        }

        // GET: /PatientTreatment/Create/{patientId}
        public async Task<IActionResult> Create(int patientId)
        {
            var drugs = await _context.Drugs.ToListAsync();
            ViewBag.Drugs = drugs;
            var treatment = new PatientTreatment
            {
                PatientId = patientId,
                TreatmentDate = DateTime.Now
            };
            return View(treatment);
        }

        // POST: /PatientTreatment/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(PatientTreatment treatment, List<IFormFile> imageFiles)
        {
            if (ModelState.IsValid)
            {
                int? workerId = HttpContext.Session.GetInt32("UserId");
                if (workerId == null)
                {
                    return RedirectToAction("Login", "Account");
                }
                treatment.WorkerId = workerId.Value;
                _context.Set<PatientTreatment>().Add(treatment);
                await _context.SaveChangesAsync();

                var imageDirectory = Path.Combine(_hostingEnvironment.WebRootPath, "images", "treatments", treatment.PatientTreatmentId.ToString());
                if (!Directory.Exists(imageDirectory))
                {
                    Directory.CreateDirectory(imageDirectory);
                }

                foreach (var imageFile in imageFiles)
                {
                    if (imageFile.Length > 0)
                    {
                        var fileName = Path.GetRandomFileName() + Path.GetExtension(imageFile.FileName);
                        var filePath = Path.Combine(imageDirectory, fileName);

                        using (var stream = new FileStream(filePath, FileMode.Create))
                        {
                            await imageFile.CopyToAsync(stream);
                        }
                    }
                }

                return RedirectToAction(nameof(Index), new { patientId = treatment.PatientId });
            }
            return View(treatment);
        }

        public async Task<IActionResult> DownloadPdf(int id)
        {
            var treatment = await _context.Set<PatientTreatment>().FindAsync(id);
            if (treatment == null)
            {
                return NotFound();
            }

            using (MemoryStream ms = new MemoryStream())
            {
                Document document = new Document(PageSize.A4, 25, 25, 30, 30);
                PdfWriter writer = PdfWriter.GetInstance(document, ms);
                document.Open();

                document.Add(new Paragraph($"Treatment Date: {treatment.TreatmentDate}"));
                document.Add(new Paragraph($"Description: {treatment.Description}"));

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
                            document.Add(image);
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
                var treatmentsToDelete = await _context.Set<PatientTreatment>()
                    .Where(t => selectedTreatments.Contains(t.PatientTreatmentId))
                    .ToListAsync();

                foreach (var treatment in treatmentsToDelete)
                {
                    var imageDirectory = Path.Combine(_hostingEnvironment.WebRootPath, "images", "treatments", treatment.PatientTreatmentId.ToString());
                    if (Directory.Exists(imageDirectory))
                    {
                        Directory.Delete(imageDirectory, true);
                    }
                }

                _context.Set<PatientTreatment>().RemoveRange(treatmentsToDelete);
                await _context.SaveChangesAsync();

                TempData["Message"] = $"{treatmentsToDelete.Count} treatment(s) have been deleted.";
            }

            return RedirectToAction(nameof(Index), new { patientId });
        }

        // GET: PatientTreatment/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var patientTreatment = await _context.Set<PatientTreatment>().FindAsync(id);
            if (patientTreatment == null)
            {
                return NotFound();
            }

            ViewBag.Drugs = _context.Drugs.Select(d => new { d.DrugId, d.DrugName }).ToList();

            var imageDirectory = Path.Combine(_hostingEnvironment.WebRootPath, "images", "treatments", id.ToString());
            var imagePaths = Directory.Exists(imageDirectory)
                ? Directory.GetFiles(imageDirectory).Select(Path.GetFileName).ToList()
                : new List<string>();

            ViewBag.ImagePaths = imagePaths;

            return View(patientTreatment);
        }

        // POST: PatientTreatment/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, PatientTreatment patientTreatment, int drugId, IFormFile[] imageFiles, string[] removeImages)
        {
            if (id != patientTreatment.PatientTreatmentId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(patientTreatment);

                    var imageDirectory = Path.Combine(_hostingEnvironment.WebRootPath, "images", "treatments", id.ToString());
                    if (!Directory.Exists(imageDirectory))
                    {
                        Directory.CreateDirectory(imageDirectory);
                    }

                    // Remove selected images
                    if (removeImages != null)
                    {
                        foreach (var imagePath in removeImages)
                        {
                            var fullPath = Path.Combine(imageDirectory, imagePath);
                            if (System.IO.File.Exists(fullPath))
                            {
                                System.IO.File.Delete(fullPath);
                            }
                        }
                    }

                    // Save new images
                    foreach (var imageFile in imageFiles)
                    {
                        if (imageFile.Length > 0)
                        {
                            var imageFileName = $"{Guid.NewGuid()}_{imageFile.FileName}";
                            var filePathToSave = Path.Combine(imageDirectory, imageFileName);

                            using (var stream = new FileStream(filePathToSave, FileMode.Create))
                            {
                                await imageFile.CopyToAsync(stream);
                            }
                        }
                    }

                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!PatientTreatmentExists(patientTreatment.PatientTreatmentId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index), new { patientId = patientTreatment.PatientId });
            }
            ViewBag.Drugs = _context.Drugs.Select(d => new { d.DrugId, d.DrugName }).ToList();
            return View(patientTreatment);
        }

        private bool PatientTreatmentExists(int id)
        {
            return _context.Set<PatientTreatment>().Any(e => e.PatientTreatmentId == id);
        }
    }
}