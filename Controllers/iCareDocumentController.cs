using iCareWebApplication.Data;
using iCareWebApplication.Models;
using iTextSharp.text;
using iTextSharp.text.pdf;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System;
using System.IO;
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

        // GET: iCareDocument/CreateDocumentForm
        public IActionResult CreateDocument()
        {
            ViewBag.Patients = new SelectList(_context.Patient, "PatientId", "Name");
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> RegisterDocument(int patientId, string filePath, string fileType, int createdBy, string description)
        {
            if (string.IsNullOrEmpty(fileType) || patientId <= 0)
            {
                ModelState.AddModelError("", "Invalid file type or patient ID.");
                return View("RegisterDocument");
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
                string pdfFilePath = Path.Combine("wwwroot", "documents", $"{newDocument.iCareDocumentId}.pdf");
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

                    pdfDoc.Close();
                }

                newDocument.FilePath = pdfFilePath;
                _context.Update(newDocument);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Error generating PDF: " + ex.Message);
                return View("RegisterDocument");
            }
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
                // Handle file not found case
                return NotFound("File not found.");
            }
        }



    }
}
