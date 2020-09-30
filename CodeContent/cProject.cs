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
        private string fullSettingsFileName => Path.Combine(fullFolderName, $"Settings_{Name}.csv");
        public cBreakPoints BreakPoints { get; }
        public cFunctions Functions { get; }
        public bool Setting_GroupTypes { get; set; } = false;
    //_____________________________________________________________________________________________________________________________________________________________
    public cProject(string folder, string name) {
            fullFolderName = folder;
            Name = name;
            if (!File.Exists(fullProjectFileName)) using (File.Create(fullProjectFileName)) { }
            if (!File.Exists(fullSettingsFileName)) using (File.Create(fullSettingsFileName)) { }
            Scripts = new List<string>();
            BreakPoints = new cBreakPoints(fullFolderName, Name);
            Functions = new cFunctions(fullFolderName, Name);
            AddScripts(File.ReadAllLines(fullProjectFileName));
            Setting_GroupTypes = File.ReadAllText(fullSettingsFileName).Contains("GroupTypes:1");
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
            File.WriteAllText(fullSettingsFileName, $"GroupTypes:{(Setting_GroupTypes?1:0)}");
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
