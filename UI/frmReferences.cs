using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Linq;
using System.Drawing;
//_____________________________________________________________________________________________________________________________________________________________
namespace IseAddons {
    //_____________________________________________________________________________________________________________________________________________________________
    public partial class frmReferences : Form {
        //_____________________________________________________________________________________________________________________________________________________________
        public string Command { set { this.Text = $"References {value}"; } }
        public List<cScriptLocation> Locations { set { ShowLocations(value); } }
        public long LocationsCount { get { return cLst.Items.Count; } }
        public cScriptLocation SelectedLocation { get { if (cLst.SelectedItems.Count != 1) return null; return (cScriptLocation)cLst.SelectedItems[0].Tag; } }
        public bool ReferenceSelected { get; private set;} = false;
        //_____________________________________________________________________________________________________________________________________________________________
        public frmReferences() { InitializeComponent(); cContext.BackColor = Color.White; cContext.ReadOnly = true; }
        //_____________________________________________________________________________________________________________________________________________________________
        public void SelectDefaultLocation(string fileName, int line) {
            foreach (ListViewItem i in cLst.Items) {
                cScriptLocation f = (cScriptLocation)i.Tag;
                if (!f.fileName.iEquals(fileName)) continue;
                if (f.line != line) continue;
                i.Selected = true;
                break;
                }
            }
        //_____________________________________________________________________________________________________________________________________________________________
        private void ShowLocations(List<cScriptLocation> locations) {
            foreach (cScriptLocation l in locations) {
                ListViewGroup g = cLst.Groups.Cast<ListViewGroup>().FirstOrDefault(x => x.ToString().iEquals(l.fileName));
                if (g == null) g = new ListViewGroup(l.fileName);
                if (!cLst.Groups.Contains(g)) cLst.Groups.Add(g);
                ListViewItem i = new ListViewItem($"{l.line}, {l.position}", g) { UseItemStyleForSubItems = false, Font = cContext.Font, Tag = l };
                i.SubItems.Add(l.functionName);    i.SubItems[1].ForeColor = Color.DarkCyan; i.SubItems[1].Font = cContext.Font;
                i.SubItems.Add(l.lineText.Trim()); i.SubItems[2].ForeColor = Color.Blue;     i.SubItems[2].Font = cContext.Font;
                cLst.Items.Add(i);
                }
            if (cLst.Items.Count > 0) cLst.Items[0].Selected = true;
            foreach (ColumnHeader c in cLst.Columns) c.Width = -2;
            }
        //_____________________________________________________________________________________________________________________________________________________________
        private void cLst_DoubleClick(object sender, EventArgs e) { if (cLst.SelectedItems.Count != 1) return; ReferenceSelected = true; Hide(); }
        //_____________________________________________________________________________________________________________________________________________________________
        private void bGoto_Click(object sender, EventArgs e) { if (cLst.SelectedItems.Count != 1) return; ReferenceSelected = true; Hide(); }
        //_____________________________________________________________________________________________________________________________________________________________
        private void BClose_Click(object sender, EventArgs e) { ReferenceSelected = false; Hide(); }
        //_____________________________________________________________________________________________________________________________________________________________
        private void cLst_SelectedIndexChanged( object sender, EventArgs e ) {
            if (cLst.SelectedItems.Count != 1) return;
            cContext.Text = "";
            cScriptLocation f = (cScriptLocation)cLst.SelectedItems[0].Tag;
            PSSyntaxHelper.AddText(cContext, string.Join("\r\n", f.lineContext), f.ContextSize, f.position, f.word);
            }
        }
    }
