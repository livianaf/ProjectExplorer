using System.Collections.Generic;
using System.IO;
using System.Linq;

//_____________________________________________________________________________________________________________________________________________________________
namespace IseAddons {
    //_____________________________________________________________________________________________________________________________________________________________
    class cBreakPoints {
        //_____________________________________________________________________________________________________________________________________________________________
        private string Name { get; set; }
        private string fullFolderName { get; }
        private string fullBreakPointsFileName => Path.Combine(fullFolderName, $"BreakPoints_{Name}.csv");
        public List<cBreakPoint> Items { set; get; } = new List<cBreakPoint>();
        public bool PendingUpdate => Items.Count(b => b.Updated == false) > 0;
        //_____________________________________________________________________________________________________________________________________________________________
        public cBreakPoints(string folder, string name) {
            fullFolderName = folder;
            Name = name;
            if (!File.Exists(fullBreakPointsFileName)) using (File.Create(fullBreakPointsFileName)) { }
            Import();
            }
        //_____________________________________________________________________________________________________________________________________________________________
        private void Import() {
            foreach (string s in File.ReadAllLines(fullBreakPointsFileName)) {
                string[] litems = s.Split(';');
                Items.Add(new cBreakPoint() { FullName = litems[0], Line = int.Parse(litems[1]), Enabled = bool.Parse(litems[2]) });
                }
            }
        //_____________________________________________________________________________________________________________________________________________________________
        public void Export() { File.WriteAllLines(fullBreakPointsFileName, Items.Select(f=>f.SerializedString).ToArray()); }
        //_____________________________________________________________________________________________________________________________________________________________
        public void Remove() { cProject.DeleteIfExists(fullBreakPointsFileName); }
        //_____________________________________________________________________________________________________________________________________________________________
        public void Rename(string name) { Remove(); Name = name; Export(); }
        }
    }
