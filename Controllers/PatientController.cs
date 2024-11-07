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

        public async Task<IActionResult> AssignablePatients()
        {
            int? workerId = HttpContext.Session.GetInt32("UserId");

            if (workerId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            // Get the role ID of the logged-in worker
            var currentUser = await _context.User.FindAsync(workerId);
            if (currentUser == null)
            {
                return RedirectToAction("Login", "Account");
            }

            int nurseRoleId = 3; // Assuming 3 is the RoleID for Nurse
            int doctorRoleId = 6; // Assuming 6 is the RoleID for Doctor

            // Get the list of assignable patients based on the role of the logged-in worker
            IQueryable<Patient> assignablePatientsQuery = _context.Patient
                .Where(p => !_context.PatientAssignment
                    .Any(pa => pa.PatientId == p.PatientId && pa.WorkerId == workerId && pa.Active));

            if (currentUser.RoleID == doctorRoleId)
            {
                // If the user is a doctor:
                // 1. Ensure patients already have at least one nurse assigned.
                // 2. Ensure no other doctor is assigned to the patient.

                assignablePatientsQuery = assignablePatientsQuery
                    .Where(p =>
                        // Check that there is at least one active nurse assigned to the patient
                        _context.PatientAssignment
                            .Where(pa => pa.PatientId == p.PatientId && pa.Active)
                            .Join(_context.User, pa => pa.WorkerId, u => u.UserId, (pa, u) => u)
                            .Any(u => u.RoleID == nurseRoleId)
                        &&
                        // Check that there are no active doctors assigned to the patient
                        !_context.PatientAssignment
                            .Where(pa => pa.PatientId == p.PatientId && pa.Active)
                            .Join(_context.User, pa => pa.WorkerId, u => u.UserId, (pa, u) => u)
                            .Any(u => u.RoleID == doctorRoleId)
                    );
            }
            else if (currentUser.RoleID == nurseRoleId)
            {
                // If the user is a nurse, apply only the nurse assignment limit constraint
                assignablePatientsQuery = assignablePatientsQuery
                    .Where(p =>
                        // Ensure fewer than 3 active nurses are assigned to the patient
                        _context.PatientAssignment
                            .Where(pa => pa.PatientId == p.PatientId && pa.Active)
                            .Join(_context.User, pa => pa.WorkerId, u => u.UserId, (pa, u) => u)
                            .Count(u => u.RoleID == nurseRoleId) < 3
                    );
            }

            // Execute the query and convert to a list
            var assignablePatients = await assignablePatientsQuery.ToListAsync();

            return View(assignablePatients);
        }




        public async Task<IActionResult> AssignByGeoLocation(int? geoCodeId)
        {
            // Get the logged-in user's ID (e.g., doctor or worker's ID)
            int? workerId = HttpContext.Session.GetInt32("UserId");

            // Ensure the user is logged in
            if (workerId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            // Fetch the list of patients filtered by GeoCodeId if provided; otherwise, get all patients
            var patients = geoCodeId.HasValue && geoCodeId.Value > 0
                ? await _context.Patient.Where(p => p.GeoCodeId == geoCodeId).ToListAsync()
                : await _context.Patient.ToListAsync();

            ViewBag.SelectedGeoCodeId = geoCodeId; // Pass the selected value to the view
            return View("AssignablePatients", patients); // This should match your view's name
        }

    }
}

