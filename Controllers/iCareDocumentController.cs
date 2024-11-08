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

        // GET: iCareDocument/Index - Fetch and display all documents
        public async Task<IActionResult> Index()
        {
            var documents = await _context.iCareDocuments.ToListAsync();
            return View(documents);
        }

        // GET: iCareDocument/CreateDocument - Display form to create a new document
        public IActionResult CreateDocument()
        {
            int? currentUserId = HttpContext.Session.GetInt32("UserId");
            if (currentUserId == null)
            {
                return RedirectToAction("Login", "Account"); // Redirect if user is not logged in
            }

            // Fetch patients assigned to the current user
            var assignedPatients = _context.PatientAssignment
                .Where(pa => pa.WorkerId == currentUserId && pa.Active)
                .Join(_context.Patient, pa => pa.PatientId, p => p.PatientId, (pa, p) => p)
                .ToList();

            // Populate ViewBag with assigned patients and available drugs
            ViewBag.Patients = new SelectList(assignedPatients, "PatientId", "Name");
            ViewBag.Drugs = _context.Drugs.Select(d => new { d.DrugId, d.DrugName }).ToList();

            return View();
        }

        // POST: iCareDocument/RegisterDocument - Save a new document and generate PDF
        [HttpPost]
        public async Task<IActionResult> RegisterDocument(
            int patientId, string fileType, int createdBy, string description, int drugId, IFormFile[] imageFiles)
        {
            // Validate fileType and patientId
            if (string.IsNullOrEmpty(fileType) || patientId <= 0)
            {
                ModelState.AddModelError("", "Invalid file type or patient ID.");
                return View("CreateDocument");
            }

            // Fetch the selected drug from the database
            var selectedDrug = await _context.Drugs.FirstOrDefaultAsync(d => d.DrugId == drugId);
            if (selectedDrug == null)
            {
                ModelState.AddModelError("", "Selected drug not found.");
                return View("CreateDocument");
            }

            try
            {
                // Create a new document entry in the database
                var newDocument = new iCareDocument
                {
                    PatientId = patientId,
                    FileType = fileType,
                    CreatedBy = createdBy,
                    Description = description,
                    CreationDate = DateTime.Now,
                    LastModified = DateTime.Now,
                    ModifiedBy = createdBy,
                    FilePath = "placeholder" // Placeholder until file path is set
                };

                _context.iCareDocuments.Add(newDocument);
                await _context.SaveChangesAsync(); // Save to get auto-generated ID

                // Define the path for the PDF file
                string pdfFilePath = Path.Combine(_hostingEnvironment.WebRootPath, "documents", $"{newDocument.iCareDocumentId}.pdf");

                // Generate and save the PDF
                using (var stream = new FileStream(pdfFilePath, FileMode.Create))
                {
                    var pdfDoc = new Document(PageSize.A4);
                    PdfWriter.GetInstance(pdfDoc, stream);
                    pdfDoc.Open();

                    // Add document information to the PDF
                    pdfDoc.Add(new Paragraph("iCare Document"));
                    pdfDoc.Add(new Paragraph($"Patient ID: {newDocument.PatientId}"));
                    pdfDoc.Add(new Paragraph($"File Type: {newDocument.FileType}"));
                    pdfDoc.Add(new Paragraph($"Created By: {newDocument.CreatedBy}"));
                    pdfDoc.Add(new Paragraph($"Creation Date: {newDocument.CreationDate}"));
                    pdfDoc.Add(new Paragraph($"Last Modified: {newDocument.LastModified}"));
                    pdfDoc.Add(new Paragraph($"Modified By: {newDocument.ModifiedBy}"));
                    pdfDoc.Add(new Paragraph($"Description: {newDocument.Description}"));

                    // Add drug information to the PDF
                    pdfDoc.Add(new Paragraph("Drug Information:"));
                    pdfDoc.Add(new Paragraph($"Drug ID: {selectedDrug.DrugId}"));
                    pdfDoc.Add(new Paragraph($"Drug Name: {selectedDrug.DrugName}"));
                    pdfDoc.Add(new Paragraph($"Drug Description: {selectedDrug.Description}"));

                    // Handle image files and add them to the PDF
                    string imageDirectory = Path.Combine(_hostingEnvironment.WebRootPath, "images", newDocument.iCareDocumentId.ToString());
                    if (!Directory.Exists(imageDirectory))
                    {
                        Directory.CreateDirectory(imageDirectory);
                    }

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

                            // Add the image to the PDF
                            var img = iTextSharp.text.Image.GetInstance(filePathToSave);
                            img.ScaleToFit(PageSize.A4.Width - 50, PageSize.A4.Height - 50);
                            img.Alignment = Element.ALIGN_CENTER;
                            pdfDoc.Add(img);
                        }
                    }

                    pdfDoc.Close();
                }

                // Update document record with the PDF path and save
                newDocument.FilePath = pdfFilePath;
                _context.Update(newDocument);

                // Update DocumentCount for the patient
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

        // GET: iCareDocument/EditDocument/5 - Display form to edit an existing document
        public async Task<IActionResult> EditDocument(int id)
        {
            int? currentUserId = HttpContext.Session.GetInt32("UserId");
            if (currentUserId == null)
            {
                return RedirectToAction("Login", "Account"); // Redirect if user is not logged in
            }

            var document = await _context.iCareDocuments.FindAsync(id);
            if (document == null)
            {
                return NotFound();
            }

            // Fetch assigned patients for the current user
            var assignedPatients = await _context.PatientAssignment
                .Where(pa => pa.WorkerId == currentUserId && pa.Active)
                .Join(_context.Patient, pa => pa.PatientId, p => p.PatientId, (pa, p) => p)
                .ToListAsync();

            // Populate ViewBag with assigned patients and available drugs
            ViewBag.Patients = new SelectList(assignedPatients, "PatientId", "Name", document.PatientId);
            ViewBag.Drugs = _context.Drugs.Select(d => new { d.DrugId, d.DrugName }).ToList();

            // Get image paths associated with the document
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
            // Check if the model state is valid
            if (!ModelState.IsValid)
            {
                // Re-populate dropdown lists if the model state is invalid
                ViewBag.Patients = new SelectList(_context.Patient, "PatientId", "Name", patientId);
                ViewBag.Drugs = _context.Drugs.Select(d => new { d.DrugId, d.DrugName }).ToList();
                return View(await _context.iCareDocuments.FindAsync(iCareDocumentId)); // Return the form with the current data
            }

            // Find the existing document in the database
            var existingDocument = await _context.iCareDocuments.FindAsync(iCareDocumentId);
            if (existingDocument == null)
            {
                return NotFound(); // Return 404 if document not found
            }

            // Remove the existing PDF file if it exists
            var existingFilePath = Path.Combine(_hostingEnvironment.WebRootPath, "documents", $"{existingDocument.iCareDocumentId}.pdf");
            if (System.IO.File.Exists(existingFilePath))
            {
                System.IO.File.Delete(existingFilePath);
            }

            // Update document properties with new values
            existingDocument.PatientId = patientId;
            existingDocument.FilePath = filePath;
            existingDocument.FileType = fileType;
            existingDocument.Description = description;
            existingDocument.LastModified = DateTime.Now; // Set the last modified date to now
            existingDocument.ModifiedBy = modifiedBy; // Set the user who modified the document

            // Define the directory for storing images
            var imageDirectory = Path.Combine(_hostingEnvironment.WebRootPath, "images", iCareDocumentId.ToString());
            if (!Directory.Exists(imageDirectory))
            {
                Directory.CreateDirectory(imageDirectory); // Create directory if it doesn't exist
            }

            // Remove selected images from the filesystem
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

            // Save new images uploaded by the user
            foreach (var imageFile in imageFiles)
            {
                if (imageFile.Length > 0)
                {
                    var imageFileName = $"{Guid.NewGuid()}_{imageFile.FileName}";
                    var filePathToSave = Path.Combine(imageDirectory, imageFileName);

                    using (var stream = new FileStream(filePathToSave, FileMode.Create))
                    {
                        await imageFile.CopyToAsync(stream); // Save image to filesystem
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

                // Add basic document information to the PDF
                pdfDoc.Add(new Paragraph("iCare Document"));
                pdfDoc.Add(new Paragraph($"Patient ID: {existingDocument.PatientId}"));
                pdfDoc.Add(new Paragraph($"File Type: {existingDocument.FileType}"));
                pdfDoc.Add(new Paragraph($"Created By: {existingDocument.CreatedBy}"));
                pdfDoc.Add(new Paragraph($"Creation Date: {existingDocument.CreationDate}"));
                pdfDoc.Add(new Paragraph($"Last Modified: {existingDocument.LastModified}"));
                pdfDoc.Add(new Paragraph($"Modified By: {existingDocument.ModifiedBy}"));
                pdfDoc.Add(new Paragraph($"Description: {existingDocument.Description}"));

                // Add selected drug information to the PDF
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
                    pdfDoc.Add(img); // Add each image to the PDF
                }

                pdfDoc.Close(); // Close the PDF document
            }

            // Update the document's file path in the database
            existingDocument.FilePath = pdfFilePath;
            _context.Update(existingDocument);
            await _context.SaveChangesAsync(); // Save changes to the database

            return RedirectToAction("Index"); // Redirect to the index view
        }

        // POST: iCareDocument/DeleteSelected - Delete selected documents and their files
        [HttpPost]
        public async Task<IActionResult> DeleteSelected(int[] selectedDocumentIds)
        {
            foreach (var docId in selectedDocumentIds)
            {
                // Find each document by ID
                var document = await _context.iCareDocuments.FindAsync(docId);
                if (document != null)
                {
                    // Delete associated PDF file if it exists
                    var filePath = Path.Combine(_hostingEnvironment.WebRootPath, "documents", $"{docId}.pdf");
                    if (System.IO.File.Exists(filePath))
                    {
                        System.IO.File.Delete(filePath);
                    }

                    // Remove images associated with the document
                    var imageDirectory = Path.Combine(_hostingEnvironment.WebRootPath, "images", docId.ToString());
                    if (Directory.Exists(imageDirectory))
                    {
                        Directory.Delete(imageDirectory, true); // Delete directory and its contents
                    }

                    // Update DocumentCount for the associated patient
                    var patient = await _context.Patient.FindAsync(document.PatientId);
                    if (patient != null)
                    {
                        // Ensure DocumentCount is not negative
                        patient.DocumentCount = Math.Max(0, patient.DocumentCount - 1);
                        _context.Update(patient);
                    }

                    // Remove document record from the database
                    _context.iCareDocuments.Remove(document);
                }
            }

            // Save changes to the database
            await _context.SaveChangesAsync();

            return RedirectToAction("Index"); // Redirect to the index view
        }

        // GET: iCareDocument/Download - Download the specified PDF document
        public IActionResult Download(int id)
        {
            // Locate the PDF file by its path
            string filePath = Path.Combine(_hostingEnvironment.WebRootPath, "documents", $"{id}.pdf");

            // Return the file if it exists, or NotFound if it doesn't
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