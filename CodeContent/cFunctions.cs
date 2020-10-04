using System.Collections.Generic;
using System.IO;
using System.Linq;

//_____________________________________________________________________________________________________________________________________________________________
namespace IseAddons {
    //_____________________________________________________________________________________________________________________________________________________________
    class cFunctions {
        //_____________________________________________________________________________________________________________________________________________________________
        private string Name { get; set; }
        private string fullFolderName { get; }
        private string fullFunctionsFileName => Path.Combine(fullFolderName, $"Functions_{Name}.csv");
        public List<cFunction> Items { get; set; } = new List<cFunction>();
        //_____________________________________________________________________________________________________________________________________________________________
        public cFunctions(string folder, string name) {
            fullFolderName = folder;
            Name = name;
            if(!File.Exists(fullFunctionsFileName)) using (File.Create(fullFunctionsFileName)) { }
            }
        //_____________________________________________________________________________________________________________________________________________________________
        public List<cFunction> GetFunctionByName(string Name) => Items.Where(f => f.Name.iEquals(Name)).ToList();
        //_____________________________________________________________________________________________________________________________________________________________
        public List<cFunction> GetFunctionByFileAndName(string fullPath, string Name) => Items.Where(f => f.Name.iEquals(Name) && f.FullName.iEquals(fullPath)).ToList();
        //_____________________________________________________________________________________________________________________________________________________________
        public List<cScriptLocation> GetScriptLocations(List<cFunction> functions) { 
            List<cScriptLocation> l = new List<cScriptLocation>();
            foreach (cFunction function in functions) l.Add(new cScriptLocation(function.FullName, function.Line, function.Position, Name));
            return l; 
            }
        //_____________________________________________________________________________________________________________________________________________________________
        public cFunction GetFunctionsByLine(string fullPath, int line) {
            List<cFunction> l = Items.Where(f => f.Line <= line && f.FullName.iEquals(fullPath)).Select(f => f).ToList();
            if (l.Count == 0) return null;
            return l.OrderBy(m => m.Line).Last();
            }
        //_____________________________________________________________________________________________________________________________________________________________
        public void RemoveFunctionByFile(string fullPath) => GetFunctionsByFile(fullPath).ForEach(f => Items.Remove(f));
        //_____________________________________________________________________________________________________________________________________________________________
        public List<cFunction> GetFunctionsByFile(string fullPath) => Items.Where(f => f.FullName.iEquals(fullPath)).Select(f => f).ToList();
        //_____________________________________________________________________________________________________________________________________________________________
        public void Export() { File.WriteAllLines(fullFunctionsFileName, Items.Select(f => f.SerializedString).ToArray()); }
        //_____________________________________________________________________________________________________________________________________________________________
        public void Remove() { cProject.DeleteIfExists(fullFunctionsFileName); }
        //_____________________________________________________________________________________________________________________________________________________________
        public void Rename(string name) { Remove(); Name = name; Export(); }
        }
    }
