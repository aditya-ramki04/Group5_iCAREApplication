﻿namespace iCareWebApplication.Models
{
    public class ModificationHistory
    {
        public int ModificationHistoryId { get; set; }
        public int DocumentId { get; set; }
        public int ModifiedBy { get; set; }
        public DateTime ModificationDate { get; set; }
        public string ModificationDetails { get; set; }

    }
}
