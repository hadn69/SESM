namespace SESM.Models.Views.Settings
{
    public class DiagnosisViewModel
    {
        public bool DatabaseConnexion { get; set; }
        public bool ArchMatch { get; set; }
        public bool Binariesx86 { get; set; }
        public bool Binariesx64 { get; set; }
        public bool ServiceCreation { get; set; }
        public bool ServiceDeletion { get; set; }
        public bool FileCreation { get; set; }
        public bool FileDeletion { get; set; }
        public bool SuperAdmin { get; set; }

        public DiagnosisViewModel()
        {
            DatabaseConnexion = false;
            ArchMatch = false;
            Binariesx86 = false;
            Binariesx64 = false; 
            ServiceCreation = false; 
            ServiceDeletion = false; 
            FileCreation = false; 
            FileDeletion = false;
            SuperAdmin = false; 

        }
    }
}