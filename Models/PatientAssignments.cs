﻿namespace iCareWebApplication.Models
{
    public class PatientAssignments
    {
        public int AssignmentId {  get; set; }
        public int PatientId { get; set; }
        public int WorkerId { get; set; }
        public DateTime DateAssigned { get; set; }
        public bool Active { get; set; }

    }
}
