using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

//_____________________________________________________________________________________________________________________________________________________________
namespace IseAddons {
    //_____________________________________________________________________________________________________________________________________________________________
    class cProjects {
        //_____________________________________________________________________________________________________________________________________________________________
        public List<string> Names { set; get; } = new List<string>();
        private Dictionary<string, cProject> Projects { set; get; } = new Dictionary<string, cProject>();
        private string selectedName { set; get; } = null;
        private static string folderName { get; } = Path.GetFileNameWithoutExtension(Assembly.GetCallingAssembly().ManifestModule.Name);
        private static string fullFolderName { get; } = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), folderName);
        private string fullProjectFileName(string name) => Path.Combine(fullFolderName, $"project_{name}.csv");
        //_____________________________________________________________________________________________________________________________________________________________
        public cProjects() {
            if (!Directory.Exists(fullFolderName)) Directory.CreateDirectory(fullFolderName);
            Names = Directory.EnumerateFiles(fullFolderName, "project_*.csv").Select(file => Path.GetFileNameWithoutExtension(file).Substring(8)).ToList();
            foreach (string name in Names) Projects.Add(name, new cProject(fullFolderName, name));
            }
        //_____________________________________________________________________________________________________________________________________________________________
        public cProjects Add(string name) {
            Names.Add(name);
            Projects.Add(name, new cProject(fullFolderName, name));
            return this;
            }
        //_____________________________________________________________________________________________________________________________________________________________
        public cProjects Select(string name) { selectedName = name; return this; }
        //_____________________________________________________________________________________________________________________________________________________________
        public cProject Project { get { return string.IsNullOrEmpty(selectedName) ? null : Projects[selectedName]; } }
        //_____________________________________________________________________________________________________________________________________________________________
        public cProjects Remove(string name) { Projects[name].Remove(); Names.Remove(name); Projects.Remove(name); return this; }
        //_____________________________________________________________________________________________________________________________________________________________
        public cProjects Rename(string oldName, string newName) {
            Projects[oldName].Rename(newName);
            Names.Remove(oldName);
            Names.Add(newName);
            cProject prj = Projects[oldName];
            Projects.Remove(oldName);
            Projects.Add(newName, prj);
            return this;
            }
        //_____________________________________________________________________________________________________________________________________________________________
        public bool ManagingFiles { get; set; } = false;
        //_____________________________________________________________________________________________________________________________________________________________
        public string GenerateUniqueName(string proposedName) {
            proposedName = proposedName.RemoveInvalidChars();
            if (!Names.Any(s => s.iEquals(proposedName))) return proposedName;
            int i = 1;
            string newName = $"{proposedName}_{i}";
            while (Names.Any(s => s.iEquals(newName))) newName = $"{proposedName}_{++i}";
            return newName;
            }
        }
    }
