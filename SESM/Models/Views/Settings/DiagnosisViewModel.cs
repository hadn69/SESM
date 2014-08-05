using System.Collections.Generic;
using System.Configuration;

namespace SESM.Models.Views.Settings
{
    public class DiagnosisViewModel
    {
        public DiagnosisElement DatabaseConnexion { get; set; }
        public DiagnosisElement SuperAdmin { get; set; }
        public DiagnosisElement ArchMatch { get; set; }
        public DiagnosisElement Binariesx86 { get; set; }
        public DiagnosisElement Binariesx64 { get; set; }
        public DiagnosisElement ServiceCreation { get; set; }
        public DiagnosisElement ServiceDeletion { get; set; }
        public DiagnosisElement FileCreation { get; set; }
        public DiagnosisElement FileDeletion { get; set; }
        

        public List<DiagnosisElement> ElementList { get; set; }

        public DiagnosisViewModel()
        {
            ElementList = new List<DiagnosisElement>();

            DatabaseConnexion = new DiagnosisElement();
            DatabaseConnexion.Name = "Database Connexion";
            ElementList.Add(DatabaseConnexion);

            SuperAdmin = new DiagnosisElement();
            SuperAdmin.Name = "Super Administrator";
            ElementList.Add(SuperAdmin);

            ArchMatch = new DiagnosisElement();
            ArchMatch.Name = "Architecture";
            ElementList.Add(ArchMatch);

            Binariesx86 = new DiagnosisElement();
            Binariesx86.Name = "32 Bits Binaries";
            ElementList.Add(Binariesx86);

            Binariesx64 = new DiagnosisElement();
            Binariesx64.Name = "64 Bits Binaries";
            ElementList.Add(Binariesx64);

            ServiceCreation = new DiagnosisElement();
            ServiceCreation.Name = "Windows Service Creation";
            ElementList.Add(ServiceCreation);

            ServiceDeletion = new DiagnosisElement();
            ServiceDeletion.Name = "Windows Service Deletion";
            ElementList.Add(ServiceDeletion);

            FileCreation = new DiagnosisElement();
            FileCreation.Name = "File Creation";
            ElementList.Add(FileCreation);

            FileDeletion = new DiagnosisElement();
            FileDeletion.Name = "File Deletion";
            ElementList.Add(FileDeletion);

            
        }
    }
}