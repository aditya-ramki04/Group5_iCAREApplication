using iCareWebApplication.Data;
using iCareWebApplication.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace iCareWebApplication.Controllers
    {
    public class PatientController : Controller
    {
        private readonly iCareContext _context;

        public PatientController(iCareContext context)
        {
            _context = context;
        }

        // GET: /Patient/Index - Fetch all patient records
        public async Task<IActionResult> Index()
        {
            var patients = await _context.Patient.ToListAsync();
            return View(patients);
        }

        // GET: /Patient/Create - Display the form to create a new patient
        public IActionResult Create()
        {
            ViewData["Title"] = "Create Patient"; // Ensure this line is included
            return View(new Patient());
        }

        // POST: /Patient/Create - Handle form submission to create a new patient record
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Patient patient)
        {
            if (ModelState.IsValid)
            {
                _context.Patient.Add(patient);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["Title"] = "Create Patient";
            return View(patient);
        }

        // GET: /Patient/Edit/5 - Display the form to edit an existing patient
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var patient = await _context.Patient.FindAsync(id);
            if (patient == null)
            {
                return NotFound();
            }

            return View(patient);
        }

        // POST: /Patient/Edit/5 - Update the patient record in the database
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Patient patient)
        {
            if (id != patient.PatientId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(patient);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.Patient.Any(e => e.PatientId == id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }

                return RedirectToAction(nameof(Index));
            }
            return View(patient);
        }


        public async Task<IActionResult> FilterByLocation(int? geoCodeId)
        {
            var patients = geoCodeId.HasValue
                           ? await _context.Patient.Where(p => p.GeoCodeId == geoCodeId).ToListAsync()
                           : await _context.Patient.ToListAsync();

            return View("Index", patients); // Reuse the Index view to display patients
        }

    }
}

