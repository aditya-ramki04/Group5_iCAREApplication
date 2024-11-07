using iCareWebApplication.Data;
using iCareWebApplication.Models;
using iTextSharp.text;
using iTextSharp.text.pdf;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace iCareWebApplication.Controllers
{
    public class iCareDocumentController : Controller
    {
        private readonly iCareContext _context;
        private readonly IWebHostEnvironment _hostingEnvironment;

        public iCareDocumentController(iCareContext context, IWebHostEnvironment hostingEnvironment)
        {
            _context = context;
            _hostingEnvironment = hostingEnvironment;
        }

        // GET: iCareDocument/Index
        public async Task<IActionResult> Index()
        {
            var documents = await _context.iCareDocuments.ToListAsync();
            return View(documents);
        }

        // GET: iCareDocument/CreateDocument
        public IActionResult CreateDocument()
        {
            int? currentUserId = HttpContext.Session.GetInt32("UserId");
            if (currentUserId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            // Perform a join between PatientAssignment and Patient to get assigned patients for the current user
            var assignedPatients = _context.PatientAssignment
                .Where(pa => pa.WorkerId == currentUserId && pa.Active) // Only active assignments
                .Join(_context.Patient,
                      pa => pa.PatientId,
                      p => p.PatientId,
                      (pa, p) => p) // Select the Patient entity from the join result
                .ToList();

            // Populate ViewBag with the filtered patient list
            ViewBag.Patients = new SelectList(assignedPatients, "PatientId", "Name");
            ViewBag.Drugs = _context.Drugs.Select(d => new { d.DrugId, d.DrugName }).ToList();

            return View();
        }


        // POST: iCareDocument/RegisterDocument
        [HttpPost]
        public async Task<IActionResult> RegisterDocument(
            int patientId, string fileType, int createdBy, string description, int drugId, IFormFile[] imageFiles)
        {
            if (string.IsNullOrEmpty(fileType) || patientId <= 0)
            {
                ModelState.AddModelError("", "Invalid file type or patient ID.");
                return View("CreateDocument");
            }

            var selectedDrug = await _context.Drugs.FirstOrDefaultAsync(d => d.DrugId == drugId);
            if (selectedDrug == null)
            {
                ModelState.AddModelError("", "Selected drug not found.");
                return View("CreateDocument");
            }

            try
            {
                // Create a new document entry with placeholder FilePath to get an ID
                var newDocument = new iCareDocument
                {
                    PatientId = patientId,
                    FileType = fileType,
                    CreatedBy = createdBy,
                    Description = description,
                    CreationDate = DateTime.Now,
                    LastModified = DateTime.Now,
                    ModifiedBy = createdBy,
                    FilePath = "placeholder"
                };

                _context.iCareDocuments.Add(newDocument);
                await _context.SaveChangesAsync(); // Get the auto-generated ID

                // Define the accurate PDF path
                string pdfFilePath = Path.Combine(_hostingEnvironment.WebRootPath, "documents", $"{newDocument.iCareDocumentId}.pdf");

                // Create and save the PDF
                using (var stream = new FileStream(pdfFilePath, FileMode.Create))
                {
                    var pdfDoc = new Document(PageSize.A4);
                    PdfWriter.GetInstance(pdfDoc, stream);
                    pdfDoc.Open();

                    // Document content
                    pdfDoc.Add(new Paragraph("iCare Document"));
                    pdfDoc.Add(new Paragraph($"Patient ID: {newDocument.PatientId}"));
                    pdfDoc.Add(new Paragraph($"File Type: {newDocument.FileType}"));
                    pdfDoc.Add(new Paragraph($"Created By: {newDocument.CreatedBy}"));
                    pdfDoc.Add(new Paragraph($"Creation Date: {newDocument.CreationDate}"));
                    pdfDoc.Add(new Paragraph($"Last Modified: {newDocument.LastModified}"));
                    pdfDoc.Add(new Paragraph($"Modified By: {newDocument.ModifiedBy}"));
                    pdfDoc.Add(new Paragraph($"Description: {newDocument.Description}"));
                    pdfDoc.Add(new Paragraph("Drug Information:"));
                    if (selectedDrug != null)
                    {
                        pdfDoc.Add(new Paragraph($"Drug ID: {selectedDrug.DrugId}"));
                        pdfDoc.Add(new Paragraph($"Drug Name: {selectedDrug.DrugName}"));
                        pdfDoc.Add(new Paragraph($"Drug Description: {selectedDrug.Description}"));
                    }

                    // Image handling
                    string imageDirectory = Path.Combine(_hostingEnvironment.WebRootPath, "images", newDocument.iCareDocumentId.ToString());
                    if (!Directory.Exists(imageDirectory))
                    {
                        Directory.CreateDirectory(imageDirectory);
                    }

                    // Save and include images in PDF
                    foreach (var imageFile in imageFiles)
                    {
                        if (imageFile.Length > 0)
                        {
                            var imageFileName = $"{Guid.NewGuid()}_{imageFile.FileName}";
                            var filePathToSave = Path.Combine(imageDirectory, imageFileName);

                            using (var imgStream = new FileStream(filePathToSave, FileMode.Create))
                            {
                                await imageFile.CopyToAsync(imgStream);
                            }

                            // Add image to PDF
                            var img = iTextSharp.text.Image.GetInstance(filePathToSave);
                            img.ScaleToFit(PageSize.A4.Width - 50, PageSize.A4.Height - 50);
                            img.Alignment = Element.ALIGN_CENTER;
                            pdfDoc.Add(img);
                        }
                    }

                    pdfDoc.Close();
                }

                // Confirm the PDF path
                newDocument.FilePath = pdfFilePath;
                _context.Update(newDocument);

                // Update DocumentCount for the associated patient
                var patient = await _context.Patient.FindAsync(patientId);
                if (patient != null)
                {
                    patient.DocumentCount++;
                    _context.Update(patient);
                }

                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Error generating PDF or saving images: " + ex.Message);
                return View("CreateDocument");
            }

            return RedirectToAction("Index");
        }

        // Get: iCareDocument/EditDocument/5
        public async Task<IActionResult> EditDocument(int id)
        {
            int? currentUserId = HttpContext.Session.GetInt32("UserId");
            if (currentUserId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var document = await _context.iCareDocuments.FindAsync(id);
            if (document == null)
            {
                return NotFound();
            }

            // Perform a join between PatientAssignment and Patient to get assigned patients for the current user
            var assignedPatients = await _context.PatientAssignment
                .Where(pa => pa.WorkerId == currentUserId && pa.Active) // Only active assignments
                .Join(_context.Patient,
                      pa => pa.PatientId,
                      p => p.PatientId,
                      (pa, p) => p) // Select the Patient entity from the join result
                .ToListAsync();

            // Populate ViewBag with the filtered patient list
            ViewBag.Patients = new SelectList(assignedPatients, "PatientId", "Name", document.PatientId);
            ViewBag.Drugs = _context.Drugs.Select(d => new { d.DrugId, d.DrugName }).ToList();

            var imageDirectory = Path.Combine(_hostingEnvironment.WebRootPath, "images", id.ToString());
            var imagePaths = Directory.Exists(imageDirectory)
                ? Directory.GetFiles(imageDirectory).Select(Path.GetFileName).ToList()
                : new List<string>();

            ViewBag.ImagePaths = imagePaths;

            return View(document);
        }



        // POST: iCareDocument/EditDocument/5
        [HttpPost]
        public async Task<IActionResult> EditDocument(
            int iCareDocumentId, int patientId, string filePath, string fileType,
            int createdBy, DateTime creationDate, int modifiedBy, DateTime lastModified, string description, int drugId,
            IFormFile[] imageFiles, string[] removeImages)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Patients = new SelectList(_context.Patient, "PatientId", "Name", patientId);
                ViewBag.Drugs = _context.Drugs.Select(d => new { d.DrugId, d.DrugName }).ToList();
                return View(await _context.iCareDocuments.FindAsync(iCareDocumentId));
            }

            var existingDocument = await _context.iCareDocuments.FindAsync(iCareDocumentId);
            if (existingDocument == null)
            {
                return NotFound();
            }

            // Remove the existing PDF if it exists
            var existingFilePath = Path.Combine(_hostingEnvironment.WebRootPath, "documents", $"{existingDocument.iCareDocumentId}.pdf");
            if (System.IO.File.Exists(existingFilePath))
            {
                System.IO.File.Delete(existingFilePath);
            }

            // Update document properties
            existingDocument.PatientId = patientId;
            existingDocument.FilePath = filePath;
            existingDocument.FileType = fileType;
            existingDocument.Description = description;
            existingDocument.LastModified = DateTime.Now;
            existingDocument.ModifiedBy = modifiedBy;

            var imageDirectory = Path.Combine(_hostingEnvironment.WebRootPath, "images", iCareDocumentId.ToString());
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

            // Regenerate the PDF with updated information and new images
            string pdfFilePath = Path.Combine(_hostingEnvironment.WebRootPath, "documents", $"{iCareDocumentId}.pdf");
            using (var docStream = new FileStream(pdfFilePath, FileMode.Create))
            {
                Document pdfDoc = new Document(PageSize.A4);
                PdfWriter.GetInstance(pdfDoc, docStream);
                pdfDoc.Open();

                // Add document information to the PDF
                pdfDoc.Add(new Paragraph("iCare Document"));
                pdfDoc.Add(new Paragraph($"Patient ID: {existingDocument.PatientId}"));
                pdfDoc.Add(new Paragraph($"File Type: {existingDocument.FileType}"));
                pdfDoc.Add(new Paragraph($"Created By: {existingDocument.CreatedBy}"));
                pdfDoc.Add(new Paragraph($"Creation Date: {existingDocument.CreationDate}"));
                pdfDoc.Add(new Paragraph($"Last Modified: {existingDocument.LastModified}"));
                pdfDoc.Add(new Paragraph($"Modified By: {existingDocument.ModifiedBy}"));
                pdfDoc.Add(new Paragraph($"Description: {existingDocument.Description}"));

                // Add selected drug information
                var selectedDrug = await _context.Drugs.FirstOrDefaultAsync(d => d.DrugId == drugId);
                if (selectedDrug != null)
                {
                    pdfDoc.Add(new Paragraph("Drug Information:"));
                    pdfDoc.Add(new Paragraph($"Drug ID: {selectedDrug.DrugId}"));
                    pdfDoc.Add(new Paragraph($"Drug Name: {selectedDrug.DrugName}"));
                    pdfDoc.Add(new Paragraph($"Drug Description: {selectedDrug.Description}"));
                }

                // Add still existing images to the PDF
                var remainingImages = Directory.GetFiles(imageDirectory);
                foreach (var imagePath in remainingImages)
                {
                    var img = iTextSharp.text.Image.GetInstance(imagePath);
                    img.ScaleToFit(PageSize.A4.Width - 50, PageSize.A4.Height - 50);
                    img.Alignment = Element.ALIGN_CENTER;
                    pdfDoc.Add(img);
                }

                pdfDoc.Close();
            }

            existingDocument.FilePath = pdfFilePath;
            _context.Update(existingDocument);
            await _context.SaveChangesAsync();

            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> DeleteSelected(int[] selectedDocumentIds)
        {
            foreach (var docId in selectedDocumentIds)
            {
                var document = await _context.iCareDocuments.FindAsync(docId);
                if (document != null)
                {
                    // Delete associated PDF file
                    var filePath = Path.Combine(_hostingEnvironment.WebRootPath, "documents", $"{docId}.pdf");
                    if (System.IO.File.Exists(filePath))
                    {
                        System.IO.File.Delete(filePath);
                    }

                    // Remove images associated with the document
                    var imageDirectory = Path.Combine(_hostingEnvironment.WebRootPath, "images", docId.ToString());
                    if (Directory.Exists(imageDirectory))
                    {
                        Directory.Delete(imageDirectory, true);
                    }

                    // Update DocumentCount for the associated patient
                    var patient = await _context.Patient.FindAsync(document.PatientId);
                    if (patient != null)
                    {
                        patient.DocumentCount = Math.Max(0, patient.DocumentCount - 1);
                        _context.Update(patient);
                    }

                    // Remove document record from the database
                    _context.iCareDocuments.Remove(document);
                }
            }

            // Save changes to the database
            await _context.SaveChangesAsync();

            return RedirectToAction("Index");
        }

        public IActionResult Download(int id)
        {
            // Locate the PDF file
            string filePath = Path.Combine(_hostingEnvironment.WebRootPath, "documents", $"{id}.pdf");

            if (System.IO.File.Exists(filePath))
            {
                return PhysicalFile(filePath, "application/pdf", $"{id}.pdf");
            }
            else
            {
                return NotFound("File not found.");
            }
        }
    }
}