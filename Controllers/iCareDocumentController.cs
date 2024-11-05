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
            ViewBag.Patients = new SelectList(_context.Patient, "PatientId", "Name");
            ViewBag.Drugs = _context.Drugs.Select(d => new { d.DrugId, d.DrugName }).ToList();
            return View();
        }

        // POST: iCareDocument/RegisterDocument
        [HttpPost]
        public async Task<IActionResult> RegisterDocument(int patientId, string filePath, string fileType, int createdBy, string description, int drugId)
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

            var newDocument = new iCareDocument
            {
                PatientId = patientId,
                FilePath = filePath,
                FileType = fileType,
                CreatedBy = createdBy,
                Description = description,
                CreationDate = DateTime.Now,
                LastModified = DateTime.Now,
                ModifiedBy = createdBy
            };

            _context.iCareDocuments.Add(newDocument);
            await _context.SaveChangesAsync();

            try
            {
                string pdfFilePath = Path.Combine(_hostingEnvironment.WebRootPath, "documents", $"{newDocument.iCareDocumentId}.pdf");
                using (var stream = new FileStream(pdfFilePath, FileMode.Create))
                {
                    Document pdfDoc = new Document(PageSize.A4);
                    PdfWriter.GetInstance(pdfDoc, stream);
                    pdfDoc.Open();

                    // Add content to the PDF
                    pdfDoc.Add(new Paragraph("iCare Document"));
                    pdfDoc.Add(new Paragraph($"Patient ID: {newDocument.PatientId}"));
                    pdfDoc.Add(new Paragraph($"File Path: {newDocument.FilePath}"));
                    pdfDoc.Add(new Paragraph($"File Type: {newDocument.FileType}"));
                    pdfDoc.Add(new Paragraph($"Created By: {newDocument.CreatedBy}"));
                    pdfDoc.Add(new Paragraph($"Creation Date: {newDocument.CreationDate}"));
                    pdfDoc.Add(new Paragraph($"Last Modified: {newDocument.LastModified}"));
                    pdfDoc.Add(new Paragraph($"Modified By: {newDocument.ModifiedBy}"));
                    pdfDoc.Add(new Paragraph($"Description: {newDocument.Description}"));
                    pdfDoc.Add(new Paragraph("Drug Information:"));
                    pdfDoc.Add(new Paragraph($"Drug ID: {selectedDrug.DrugId}"));
                    pdfDoc.Add(new Paragraph($"Drug Name: {selectedDrug.DrugName}"));
                    pdfDoc.Add(new Paragraph($"Drug Description: {selectedDrug.Description}"));

                    pdfDoc.Close();
                }

                newDocument.FilePath = pdfFilePath;
                _context.Update(newDocument);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Error generating PDF: " + ex.Message);
                return View("CreateDocument");
            }
            return RedirectToAction("Index");
        }

        // GET: iCareDocument/EditDocument/5
        public async Task<IActionResult> EditDocument(int id)
        {
            var document = await _context.iCareDocuments.FindAsync(id);
            if (document == null)
            {
                return NotFound();
            }

            ViewBag.Patients = new SelectList(_context.Patient, "PatientId", "Name", document.PatientId);
            ViewBag.Drugs = _context.Drugs.Select(d => new { d.DrugId, d.DrugName }).ToList();

            return View(document);
        }

        // POST: iCareDocument/EditDocument/5
        [HttpPost]
        public async Task<IActionResult> EditDocument(int iCareDocumentId, int patientId, string filePath, string fileType, int createdBy, DateTime creationDate, int modifiedBy, DateTime lastModified, string description, int drugId, IFormFile[] imageFiles)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Patients = new SelectList(_context.Patient, "PatientId", "Name", patientId);
                ViewBag.Drugs = _context.Drugs.Select(d => new { d.DrugId, d.DrugName }).ToList();
                return View(new iCareDocument
                {
                    iCareDocumentId = iCareDocumentId,
                    PatientId = patientId,
                    FilePath = filePath,
                    FileType = fileType,
                    CreatedBy = createdBy,
                    CreationDate = creationDate,
                    ModifiedBy = modifiedBy,
                    LastModified = lastModified,
                    Description = description
                });
            }

            var existingDocument = await _context.iCareDocuments.FindAsync(iCareDocumentId);
            if (existingDocument == null)
            {
                return NotFound();
            }

            // Update document details
            existingDocument.PatientId = patientId;
            existingDocument.FilePath = filePath;
            existingDocument.FileType = fileType;
            existingDocument.Description = description;
            existingDocument.LastModified = DateTime.Now;
            existingDocument.ModifiedBy = modifiedBy;

            // Create a new PDF with updated information and include multiple images
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

                // Add each uploaded image to the PDF
                foreach (var imageFile in imageFiles)
                {
                    if (imageFile != null && imageFile.Length > 0)
                    {
                        using (var memoryStream = new MemoryStream())
                        {
                            await imageFile.CopyToAsync(memoryStream);
                            var imgBytes = memoryStream.ToArray();
                            var img = iTextSharp.text.Image.GetInstance(imgBytes);
                            img.ScaleToFit(PageSize.A4.Width - 50, PageSize.A4.Height - 50); // Scale image 
                            img.Alignment = Element.ALIGN_CENTER;
                            pdfDoc.Add(img);
                        }
                    }
                }

                pdfDoc.Close();
            }

            existingDocument.FilePath = pdfFilePath;
            _context.Update(existingDocument);
            await _context.SaveChangesAsync();

            return RedirectToAction("Index");
        }

        // POST: iCareDocument/DeleteSelected
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

                    // Remove document from database
                    _context.iCareDocuments.Remove(document);
                }
            }

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