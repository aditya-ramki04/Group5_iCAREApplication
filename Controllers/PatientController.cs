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

        // Constructor to initialize the PatientController with a database context.
        // Parameters:
        //   context: The database context used to interact with the data source.
        public PatientController(iCareContext context)
        {
            _context = context;
        }

        // GET: /Patient/Index
        // Fetches all patient records and displays them in the Index view.
        // Parameters: None
        // Returns: An IActionResult that renders the Index view with a list of all patients.
        public async Task<IActionResult> Index()
        {
            var patients = await _context.Patient.ToListAsync();
            return View(patients);
        }

        // GET: /Patient/Create
        // Displays the form to create a new patient.
        // Parameters: None
        // Returns: An IActionResult that renders the Create view with an empty Patient model.
        public IActionResult Create()
        {
            ViewData["Title"] = "Create Patient";
            return View(new Patient());
        }

        // POST: /Patient/Create
        // Handles form submission to create a new patient record in the database.
        // Parameters:
        //   patient: The Patient object containing the new patient's information.
        // Returns: An IActionResult that redirects to the Index action if successful, or returns to the Create view if there are validation errors.
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

        // GET: /Patient/Edit/5
        // Displays the form to edit an existing patient.
        // Parameters:
        //   id: The ID of the patient to edit.
        // Returns: An IActionResult that renders the Edit view with the patient's data if found, or returns NotFound if the patient ID is invalid.
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

        // POST: /Patient/Edit/5
        // Updates the patient record in the database.
        // Parameters:
        //   id: The ID of the patient to edit.
        //   patient: The updated Patient object.
        // Returns: An IActionResult that redirects to the Index action if successful, or returns to the Edit view if there are validation errors.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Patient patient)
        {
            // Check if the provided ID matches the patient's ID from the form data
            if (id != patient.PatientId)
            {
                // If the IDs don't match, return a 404 Not Found response
                return NotFound();
            }

            // Verify if the model data is valid (e.g., required fields are provided and correctly formatted)
            if (ModelState.IsValid)
            {
                try
                {
                    // Update the patient record in the database context
                    _context.Update(patient);
                    // Save changes asynchronously to the database
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    // Handle potential concurrency issues during the update

                    // Check if the patient with the given ID still exists in the database
                    if (!_context.Patient.Any(e => e.PatientId == id))
                    {
                        // If the patient no longer exists, return a 404 Not Found response
                        return NotFound();
                    }
                    else
                    {
                        // If another concurrency error occurred, rethrow the exception
                        throw;
                    }
                }

                // Redirect to the Index view after a successful update
                return RedirectToAction(nameof(Index));
            }

            // If the model data is invalid, re-display the form with validation errors
            return View(patient);
        }


        // Filters patients by GeoCodeId and displays the filtered list.
        // Parameters:
        //   geoCodeId: The GeoCodeId to filter patients by.
        // Returns: An IActionResult that renders the Index view with the filtered list of patients.
        public async Task<IActionResult> FilterByLocation(int? geoCodeId)
        {
            var patients = geoCodeId.HasValue
                           ? await _context.Patient.Where(p => p.GeoCodeId == geoCodeId).ToListAsync()
                           : await _context.Patient.ToListAsync();

            return View("Index", patients);
        }

        // Gets a list of patients assignable to the logged-in worker based on the worker's role.
        // Parameters: None
        // Returns: An IActionResult that renders the AssignablePatients view with a filtered list of patients.
        public async Task<IActionResult> AssignablePatients()
        {
            // Retrieve the logged-in user's ID from the session
            int? workerId = HttpContext.Session.GetInt32("UserId");

            // If the user ID is not found in the session, redirect to the login page
            if (workerId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            // Retrieve the current user's details from the database using their ID
            var currentUser = await _context.User.FindAsync(workerId);

            // If the current user is not found in the database, redirect to the login page
            if (currentUser == null)
            {
                return RedirectToAction("Login", "Account");
            }

            // Define role IDs for Nurse and Doctor roles for later comparisons
            int nurseRoleId = 3;
            int doctorRoleId = 6;

            // Initialize a query to select all patients who are not actively assigned to the current worker
            IQueryable<Patient> assignablePatientsQuery = _context.Patient
                .Where(p => !_context.PatientAssignment
                    .Any(pa => pa.PatientId == p.PatientId && pa.WorkerId == workerId && pa.Active));

            // If the current user is a doctor, apply additional filters for doctors
            if (currentUser.RoleID == doctorRoleId)
            {
                assignablePatientsQuery = assignablePatientsQuery
                    .Where(p =>
                        // Ensure that there is at least one active nurse assigned to the patient
                        _context.PatientAssignment
                            .Where(pa => pa.PatientId == p.PatientId && pa.Active)
                            .Join(_context.User, pa => pa.WorkerId, u => u.UserId, (pa, u) => u)
                            .Any(u => u.RoleID == nurseRoleId)
                        &&
                        // Ensure that no other doctor is currently assigned to the patient
                        !_context.PatientAssignment
                            .Where(pa => pa.PatientId == p.PatientId && pa.Active)
                            .Join(_context.User, pa => pa.WorkerId, u => u.UserId, (pa, u) => u)
                            .Any(u => u.RoleID == doctorRoleId)
                    );
            }
            // If the current user is a nurse, apply constraints specific to nurse assignments
            else if (currentUser.RoleID == nurseRoleId)
            {
                assignablePatientsQuery = assignablePatientsQuery
                    .Where(p =>
                        // Ensure that fewer than 3 active nurses are assigned to the patient
                        _context.PatientAssignment
                            .Where(pa => pa.PatientId == p.PatientId && pa.Active)
                            .Join(_context.User, pa => pa.WorkerId, u => u.UserId, (pa, u) => u)
                            .Count(u => u.RoleID == nurseRoleId) < 3
                    );
            }

            // Execute the query and retrieve the list of assignable patients
            var assignablePatients = await assignablePatientsQuery.ToListAsync();

            // Pass the list of assignable patients to the view
            return View(assignablePatients);
        }


        // Filters assignable patients by GeoCodeId based on the logged-in worker's role.
        // Parameters:
        //   geoCodeId: The GeoCodeId to filter patients by.
        // Returns: An IActionResult that renders the AssignablePatients view with the filtered list of patients.
        public async Task<IActionResult> AssignByGeoLocation(int? geoCodeId)
        {
            // Retrieve the logged-in user's ID from the session
            int? workerId = HttpContext.Session.GetInt32("UserId");

            // If no user is logged in, redirect to the login page
            if (workerId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            // Retrieve the current user's details from the database using their ID
            var currentUser = await _context.User.FindAsync(workerId);

            // If the current user is not found in the database, redirect to the login page
            if (currentUser == null)
            {
                return RedirectToAction("Login", "Account");
            }

            // Define role IDs for Nurse and Doctor roles
            int nurseRoleId = 3;
            int doctorRoleId = 6;

            // Initialize a query to select patients based on their GeoCodeId and exclude patients actively assigned to the worker
            IQueryable<Patient> assignablePatientsQuery = _context.Patient
                .Where(p =>
                    // Filter by GeoCodeId if specified
                    (!geoCodeId.HasValue || p.GeoCodeId == geoCodeId)
                    // Exclude patients already assigned to the current worker
                    && !_context.PatientAssignment.Any(pa => pa.PatientId == p.PatientId && pa.WorkerId == workerId && pa.Active)
                );

            // If the current user is a doctor, apply additional filters specific to doctors
            if (currentUser.RoleID == doctorRoleId)
            {
                assignablePatientsQuery = assignablePatientsQuery
                    .Where(p =>
                        // Ensure that there is at least one active nurse assigned to the patient
                        _context.PatientAssignment
                            .Where(pa => pa.PatientId == p.PatientId && pa.Active)
                            .Join(_context.User, pa => pa.WorkerId, u => u.UserId, (pa, u) => u)
                            .Any(u => u.RoleID == nurseRoleId)
                        &&
                        // Ensure that no other doctor is actively assigned to the patient
                        !_context.PatientAssignment
                            .Where(pa => pa.PatientId == p.PatientId && pa.Active)
                            .Join(_context.User, pa => pa.WorkerId, u => u.UserId, (pa, u) => u)
                            .Any(u => u.RoleID == doctorRoleId)
                    );
            }
            // If the current user is a nurse, apply constraints specific to nurse assignments
            else if (currentUser.RoleID == nurseRoleId)
            {
                assignablePatientsQuery = assignablePatientsQuery
                    .Where(p =>
                        // Ensure that fewer than 3 active nurses are assigned to the patient
                        _context.PatientAssignment
                            .Where(pa => pa.PatientId == p.PatientId && pa.Active)
                            .Join(_context.User, pa => pa.WorkerId, u => u.UserId, (pa, u) => u)
                            .Count(u => u.RoleID == nurseRoleId) < 3
                    );
            }

            // Execute the query to get the list of assignable patients
            var assignablePatients = await assignablePatientsQuery.ToListAsync();

            // Pass the selected GeoCodeId to the view for display purposes
            ViewBag.SelectedGeoCodeId = geoCodeId;

            // Return the assignable patients list to the "AssignablePatients" view
            return View("AssignablePatients", assignablePatients);
        }

    }
}
