//using iCareWebApplication.Data;
//using iCareWebApplication.Models;
//using iTextSharp.text;
//using iTextSharp.text.pdf;
//using Microsoft.AspNetCore.Mvc.Rendering;
//using Microsoft.AspNetCore.Mvc;
//using System.IO;
//using System.Reflection.Metadata;
//using Document = iTextSharp.text.Document;

//namespace iCareWebApplication.Controllers
//{
//    public class DocumentController : Controller
//    {
//        private readonly iCareContext _context;

//        public DocumentController(iCareContext context)
//        {
//            _context = context;
//        }

//        // GET: Document/Create
//        public IActionResult CreateDocument()
//        {
//            ViewBag.Patients = new SelectList(_context.Patient, "PatientId", "Name");
//            return View();
//        }

//        // POST: Document/Create
//        [HttpPost]
//        public async Task<IActionResult> CreateDocument(iCareDocument model)
//        {
//            if (ModelState.IsValid)
//            {
//                // Generate the PDF file from the provided content
//                var pdfFilePath = Path.Combine("wwwroot/documents", $"{Guid.NewGuid()}.pdf");
//                using (var fs = new FileStream(pdfFilePath, FileMode.Create, FileAccess.Write, FileShare.None))
//                {
//                    Document document = new Document();
//                    PdfWriter writer = PdfWriter.GetInstance(document, fs);
//                    document.Open();
//                    document.Add(new Paragraph(model.FilePath)); // FilePath will temporarily store the text content
//                    document.Close();
//                }

//                // Set additional metadata and save to the database
//                model.FilePath = pdfFilePath;
//                model.FileType = "PDF";
//                model.CreationDate = DateTime.Now;
//                model.CreatedBy = // Fetch current user's ID here
//             //   model.LastModified = DateTime.Now;
//                model.ModifiedBy = model.CreatedBy;

//                _context.iCareDocuments.Add(model);
//                await _context.SaveChangesAsync();

//                return RedirectToAction("Index", "Document");
//            }

//            ViewBag.Patients = new SelectList(_context.Patient, "PatientId", "Name");
//            return View(model);
//        }
//        public IActionResult Download(int id)
//        {
//            var document = _context.iCareDocuments.FirstOrDefault(d => d.iCareDocumentId == id);
//            if (document == null || !System.IO.File.Exists(document.FilePath))
//                return NotFound();

//            var fileBytes = System.IO.File.ReadAllBytes(document.FilePath);
//            return File(fileBytes, "application/pdf", Path.GetFileName(document.FilePath));
//        }

//    }
//}
// Import necessary namespaces
//using iCareWebApplication.Data;
//using iCareWebApplication.Models;
//using iTextSharp.text;
//using iTextSharp.text.pdf;
//using Microsoft.AspNetCore.Mvc;
//using Microsoft.AspNetCore.Mvc.Rendering;
//using Microsoft.EntityFrameworkCore;
//using System;
//using System.IO;
//using System.Threading.Tasks;

//namespace iCareWebApplication.Controllers
//{
//    public class iCareDocumentController : Controller
//    {
//        private readonly iCareContext _context;

//        public iCareDocumentController(iCareContext context)
//        {
//            _context = context;
//        }

//        // GET: iCareDocument/Index
//        public async Task<IActionResult> Index()
//        {
//            var documents = await _context.iCareDocuments.ToListAsync();
//            return View(documents);
//        }

//        [HttpPost]
//        public async Task<IActionResult> CreateDocument(iCareDocument model)
//        {
//            if (ModelState.IsValid)
//            {
//                // Fetch the patient information using the provided PatientId
//                var patient = await _context.Patient.FindAsync(model.PatientId);
//                if (patient == null)
//                {
//                    ModelState.AddModelError("", "Patient not found.");
//                    ViewBag.Patients = new SelectList(_context.Patient, "PatientId", "Name");
//                    return View(model);
//                }

//                // Generate a unique file name for the PDF
//                var pdfFileName = $"{Guid.NewGuid()}.pdf"; // Creates a new GUID for the file name
//                var pdfFilePath = Path.Combine("wwwroot/documents", pdfFileName);

//                // Create the PDF file
//                using (var fs = new FileStream(pdfFilePath, FileMode.Create, FileAccess.Write, FileShare.None))
//                {
//                    Document document = new Document();
//                    PdfWriter writer = PdfWriter.GetInstance(document, fs);
//                    document.Open();

//                    // Add patient information and document metadata to the PDF
//                    document.Add(new Paragraph($"Document ID: {model.iCareDocumentId}"));
//                    document.Add(new Paragraph($"Patient ID: {patient.PatientId}"));
//                    document.Add(new Paragraph($"Patient Name: {patient.Name}"));
//                    document.Add(new Paragraph($"Description: {model.Description}"));
//                    document.Add(new Paragraph($"Date of Creation: {DateTime.Now}"));
//                    // Add other metadata as needed

//                    document.Close();
//                }

//                // Save the document metadata to the database
//                model.FilePath = pdfFilePath; // Store the path of the newly created PDF
//                model.FileType = "PDF"; // Assuming you're storing the type as PDF
//                model.CreationDate = DateTime.Now;
//             //   model.CreatedBy = /* Fetch current user's ID here */;
//                model.LastModified = DateTime.Now; // Set last modified date
//                model.ModifiedBy = model.CreatedBy; // Set modified by as creator initially

//                _context.iCareDocuments.Add(model);
//                await _context.SaveChangesAsync();

//                return RedirectToAction("Index", "Document");
//            }

//            ViewBag.Patients = new SelectList(_context.Patient, "PatientId", "Name");
//            return View(model);
//        }




//        // GET: iCareDocument/Download
//        public IActionResult Download(int id)
//        {
//            var document = _context.iCareDocuments.Find(id);
//            if (document == null || string.IsNullOrEmpty(document.FilePath))
//            {
//                return NotFound();
//            }

//            // Return the file as a download
//            var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", document.FilePath.TrimStart('/'));
//            return PhysicalFile(filePath, "application/pdf", Path.GetFileName(filePath));
//        }
//        public IActionResult CreateDocument()
//        {
//            // Populate the ViewBag with patients for the dropdown
//            ViewBag.Patients = new SelectList(_context.Patient, "PatientId", "Name");

//            return View();
//        }
//    }
//}
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

        public iCareDocumentController(iCareContext context)
        {
            _context = context;
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
            // Validate file type and patient ID (or other criteria as needed)
            if (string.IsNullOrEmpty(fileType) || patientId <= 0)
            {
                ModelState.AddModelError("", "Invalid file type or patient ID.");
                return View("RegisterDocument");
            }

            // Create a new iCareDocument object with the provided data
            var newDocument = new iCareDocument
            {
                PatientId = patientId,
                FilePath = filePath,
                FileType = fileType,
                CreatedBy = createdBy,
                Description = description,
                CreationDate = DateTime.Now,
                LastModified = DateTime.Now, // Initially set as the creation date
                ModifiedBy = createdBy // Initially, the creator is also the modifier
            };

            // Add the document to the database
            _context.iCareDocuments.Add(newDocument);
            await _context.SaveChangesAsync();

            // Generate PDF
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

                // Update the document's FilePath with the new PDF path if needed
                newDocument.FilePath = pdfFilePath;
                _context.Update(newDocument);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                // Log error or handle exceptions
                ModelState.AddModelError("", "Error generating PDF: " + ex.Message);
                return View("RegisterDocument");
            }

            // Redirect to a confirmation or list page upon successful registration
            return RedirectToAction("Index");
        }



    }
}
