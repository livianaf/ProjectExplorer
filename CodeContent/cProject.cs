using System.Collections.Generic;
using System.IO;

//_____________________________________________________________________________________________________________________________________________________________
namespace IseAddons {
    //_____________________________________________________________________________________________________________________________________________________________
    class cProject {
        //_____________________________________________________________________________________________________________________________________________________________
        private string Name { get; set; }
        public List<string> Scripts { get; private set; } = new List<string>();
        private string fullFolderName { get; }
        private string fullProjectFileName => Path.Combine(fullFolderName, $"Project_{Name}.csv");
        public cBreakPoints BreakPoints { get; }
        public cFunctions Functions { get; }
        //_____________________________________________________________________________________________________________________________________________________________
        public cProject(string folder, string name) {
            fullFolderName = folder;
            Name = name;
            if (!File.Exists(fullProjectFileName)) using (File.Create(fullProjectFileName)) { }
            Scripts = new List<string>();
            BreakPoints = new cBreakPoints(fullFolderName, Name);
            Functions = new cFunctions(fullFolderName, Name);
            AddScripts(File.ReadAllLines(fullProjectFileName));
            }
        //_____________________________________________________________________________________________________________________________________________________________
        private void AddScript(string name) {
            if (!File.Exists(name) || Scripts.Contains(name)) return;
            Scripts.Add(name);
            File.WriteAllLines(fullProjectFileName, Scripts.ToArray());
            }
        //_____________________________________________________________________________________________________________________________________________________________
        public void AddScripts(string[] names) {
            Scripts.Clear();
            foreach (string name in names) AddScript(name);
            }
        //_____________________________________________________________________________________________________________________________________________________________
        public void AddBreakPoint(cBreakPoint bp) {
            foreach (cBreakPoint name in BreakPoints.Items)
                if (bp.SerializedString.iEquals(name.SerializedString)) return;
            BreakPoints.Items.Add(bp);
            }
        //_____________________________________________________________________________________________________________________________________________________________
        public void Export() {
            File.WriteAllLines(fullProjectFileName, Scripts.ToArray());
            BreakPoints.Export();
            Functions.Export();
            }
        //_____________________________________________________________________________________________________________________________________________________________
        public void Remove() {
            DeleteIfExists(fullProjectFileName);
            BreakPoints.Remove();
            Functions.Remove();
            }
        //_____________________________________________________________________________________________________________________________________________________________
        public void Rename(string name) {
            DeleteIfExists(fullProjectFileName);
            BreakPoints.Rename(name);
            Functions.Rename(name);
            Name = name;
            File.WriteAllLines(fullProjectFileName, Scripts.ToArray());
            }
        //_____________________________________________________________________________________________________________________________________________________________
        public static void DeleteIfExists(string name) { if (File.Exists(name)) File.Delete(name); }
        }
    }
