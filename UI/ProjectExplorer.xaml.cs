﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Management.Automation;
using System.Runtime.ExceptionServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using Microsoft.PowerShell.Host.ISE;

//_____________________________________________________________________________________________________________________________________________________________
namespace IseAddons {
    //_____________________________________________________________________________________________________________________________________________________________
    public partial class ProjectExplorer : UserControl, IAddOnToolHostObject, IComponentConnector {
        private cProjects mProjects = new cProjects();
        private ObjectModelRoot hostObject;
        private bool PrjSelected => LastProjectSelected != null;
        private Stack<Tuple<string, int, int>> BackList { get; } = new Stack<Tuple<string, int, int>>();
        private int LastProjectIndexSelected { set; get; } = -1;
        private string LastProjectSelected { set; get; } = null;
        //_____________________________________________________________________________________________________________________________________________________________
        public ProjectExplorer() {
            LogHelper.Add("ProjectExplorer");
            InitializeComponent();
            ShowProjectDetail(false);
            AppDomain.CurrentDomain.DomainUnload += new EventHandler(CurrentDomain_ProcessExit);
            AppDomain.CurrentDomain.ProcessExit += new EventHandler(CurrentDomain_ProcessExit);
            AppDomain.CurrentDomain.FirstChanceException += new EventHandler<FirstChanceExceptionEventArgs>(CurrentDomain_FirstChanceException);
            LogHelper.CleanOldLogs();
            }
        //_____________________________________________________________________________________________________________________________________________________________
        public ObjectModelRoot HostObject {
            get { return hostObject; }
            set {
                LogHelper.Add("HostObject assigned");
                hostObject = value;
                hostObject.CurrentPowerShellTab.PropertyChanged += new PropertyChangedEventHandler(CurrentPowerShellTab_PropertyChanged);
                OpenProjects();
                }
            }
        #region Events
        //_____________________________________________________________________________________________________________________________________________________________
        private void cLstPrj_MouseDoubleClick(object sender, MouseButtonEventArgs e) {
            LogHelper.Add("cLstPrj_MouseDoubleClick");
            if (cLstPrj.SelectedItems.Count != 1) return;
            ShowProjectDetail();
            if ("Busy" == (string)cLstPrj.Tag) return;
            cLstPrj.Tag = "Busy";
            ISEFileCollection files = hostObject.CurrentPowerShellTab.Files;
            foreach (ISEFile file in files) {
                if (!file.IsSaved) {
                    MessageBoxResult r = MessageBox.Show($"Do you want to save {file.FullPath}?", "Project Explorer", MessageBoxButton.YesNoCancel, MessageBoxImage.Question);
                    if (r == MessageBoxResult.Cancel) { cLstPrj.SelectedIndex = LastProjectIndexSelected; cLstPrj.Tag = "Ready"; return; }
                    if (r == MessageBoxResult.Yes) file.Save();
                    }
                }
            SaveBreakPoints();
            if (PrjSelected) mProjects.Project.Export();
            CloseFiles();
            SelectNewProject();
            OpenFiles();
            UpdateTreeView();
            ImportBreakpoints();
            cLstPrj.Tag = "Ready";
            }
        //_____________________________________________________________________________________________________________________________________________________________
        private void bSaveNewProject_Click(object sender, RoutedEventArgs e) {
            LogHelper.Add("bSaveNewProject_Click");
            if (string.IsNullOrWhiteSpace(cNamePrj.Text.Trim())) return;
            ShowProjectDetail();
            cNamePrj.Text = mProjects.GenerateUniqueName(cNamePrj.Text);
            if (((string)bSaveNewProject.Tag).iStartsWith("RENAME")) {
                string oldName = ((string)bSaveNewProject.Tag).Split('|')[1];
                mProjects.Rename(oldName, cNamePrj.Text);
                for (int i = 0; i < cLstPrj.Items.Count; i++) if (cLstPrj.Items[i].Equals(oldName)) cLstPrj.Items[i] = cNamePrj.Text;
                bSaveNewProject.Tag = "ADD";
                bSaveNewProject.Content = "Add";
                return;
                }
            LastProjectSelected = cNamePrj.Text;
            lSelected.Text = LastProjectSelected;
            string[] f = hostObject.CurrentPowerShellTab.Files.Where(file => !file.IsUntitled).Select(file => file.FullPath).ToArray();
            mProjects.Add(LastProjectSelected).Select(LastProjectSelected).Project.AddScripts(f);
            OpenProjects();
            LastProjectIndexSelected = (cLstPrj.Items.Count - 1);
            mProjects.Select(LastProjectSelected);
            OpenFiles();
            SaveProject();
            }
        //_____________________________________________________________________________________________________________________________________________________________
        private void CurrentDomain_FirstChanceException( object sender, FirstChanceExceptionEventArgs e ) { LogHelper.AddException(e.Exception, "CurrentDomain_FirstChanceException", $"Type: {e.GetType()}"); }
        //_____________________________________________________________________________________________________________________________________________________________
        private void CurrentDomain_ProcessExit( object sender, EventArgs e ) { LogHelper.Add("CurrentDomain_ProcessExit"); SaveProject(); }
        //_____________________________________________________________________________________________________________________________________________________________
        private void mProjectDelete_Click( object sender, RoutedEventArgs e ) {
            LogHelper.Add("mProjectDelete_Click");
            if (cLstPrj.SelectedItem == null) return;
            string sPrjToBeDeleted = cLstPrj.SelectedItem.ToString();
            if (LastProjectSelected != null && LastProjectSelected.iEquals(sPrjToBeDeleted)) {
                MessageBox.Show("You must close the project before deletion", "Project Explorer", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                return;
                }
            if (MessageBox.Show($"Delete {sPrjToBeDeleted}?", "Project Explorer", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.No) return;
            mProjects.Remove(cLstPrj.SelectedItem.ToString());
            cLstPrj.Items.Remove(cLstPrj.SelectedItem);
            }
        //_____________________________________________________________________________________________________________________________________________________________
        private void mProjectRename_Click(object sender, RoutedEventArgs e) {
            LogHelper.Add("mProjectDelete_Click");
            if (cLstPrj.SelectedItem == null) return;
            string sPrjToBeRename = cLstPrj.SelectedItem.ToString();
            if (LastProjectSelected != null && LastProjectSelected.iEquals(sPrjToBeRename)) {
                MessageBox.Show("You must close the project before rename", "Project Explorer", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                return;
                }
            if (MessageBox.Show($"Write below the new name for '{sPrjToBeRename}' and click Rename.", "Project Explorer", MessageBoxButton.OKCancel, MessageBoxImage.Information) == MessageBoxResult.Cancel) return;
            cNamePrj.Text = sPrjToBeRename;
            bSaveNewProject.Content = "Rename";
            bSaveNewProject.Tag = $"RENAME|{sPrjToBeRename}";
            }
        //_____________________________________________________________________________________________________________________________________________________________
        private void CurrentPowerShellTab_PropertyChanged( object sender, PropertyChangedEventArgs e ) {
            LogHelper.Add($"CurrentPowerShellTab_PropertyChanged({e.PropertyName})");
            if (!(e.PropertyName.iEquals("CanInvoke") | e.PropertyName.iEquals("LastEditorWithFocus")) || !hostObject.CurrentPowerShellTab.CanInvoke) return;
            if (!PrjSelected) return;
            if (mProjects.ManagingFiles) return;

            if (mProjects.Project.BreakPoints.Items.Count > 0 && mProjects.Project.BreakPoints.PendingUpdate) ImportBreakpoints();

            if (mProjects.Project.Scripts.Count != hostObject.CurrentPowerShellTab.Files.Count)
                mProjects.Project.AddScripts(hostObject.CurrentPowerShellTab.Files.ForEach(f => f.FullPath).ToArray());

            if (mProjects.Project.Scripts.Count != cTvFunctions.Items.Count) SaveProject();

            string fullPath = hostObject.CurrentPowerShellTab.Files.SelectedFile?.FullPath;
            if (string.IsNullOrWhiteSpace(fullPath)) return;
            string name = Path.GetFileName(fullPath);
            string folderName = Path.GetDirectoryName(fullPath);

            foreach (TreeViewItem i in cTvFunctions.Items) {
                if (i.Header.ToString() != name) continue;
                //necesario para que BringIntoView coloque el TvItem al principio de la lista. No vale con llamar a BringIntoView del último hijo antes del padre.
                Rect rect = new Rect(-1000, 0, cTvFunctions.ActualWidth + 1000, cTvFunctions.ActualHeight);
                if (i.IsExpanded == false) InvokeCmd($"CD {folderName}");
                i.IsExpanded = true;
                i.IsSelected = true;
                // expande todos los niveles
                foreach (TreeViewItem n1 in i.Items) { n1.IsExpanded = true; foreach (TreeViewItem n2 in n1.Items) n2.IsExpanded = true; }
                i.BringIntoView(rect);
                break;// si no hay else se puede salir directamente.
                //else i.IsExpanded = false; // colapsa todo el árbol menos la rama que interesa.
                }
            }
        //_____________________________________________________________________________________________________________________________________________________________
        private void bUpdate_Click( object sender, RoutedEventArgs e ) { LogHelper.Add("bUpdate_Click"); SaveProject(); }
        //_____________________________________________________________________________________________________________________________________________________________
        private void cTvFunctions_MouseDoubleClick( object sender, MouseButtonEventArgs e ) {
            LogHelper.Add("cTvFunctions_MouseDoubleClick");
            TreeViewItem item = (TreeViewItem)((TreeView)sender).SelectedItem;
            if (item == null) return;
            cFunction tag = item.Tag as cFunction;
            if (tag == null) {// doubleclick in file node
                if (item.GetType().Name != "FileTreeViewItem") return;
                try {
                    hostObject.CurrentPowerShellTab.Files.SetSelectedFile(hostObject.CurrentPowerShellTab.Files.Where(file => Path.GetFileName(file.FullPath).iEquals(item.Header.ToString())).FirstOrDefault());
                    hostObject.CurrentPowerShellTab.Files.SelectedFile.Editor.SelectCaretLine();
                    }
                catch (Exception ex) { LogHelper.AddException(ex, "cTvFunctions_MouseDoubleClick", "selected file"); }
                }
            else {// doubleclick in function node
                try {
                    hostObject.CurrentPowerShellTab.Files.SetSelectedFile(hostObject.CurrentPowerShellTab.Files.Where(file => file.FullPath.iEquals(tag.FullName)).FirstOrDefault());
                    hostObject.CurrentPowerShellTab.Files.SelectedFile.Editor.SetCaretPosition(tag.Line, tag.Position);
                    hostObject.CurrentPowerShellTab.Files.SelectedFile.Editor.Select(tag.Line, tag.Position, tag.Line, tag.Position + tag.Name.Length);
                    }
                catch (Exception ex) { LogHelper.AddException(ex, "cTvFunctions_MouseDoubleClick", "selected function"); }
                }
            }
        //_____________________________________________________________________________________________________________________________________________________________
        private void UserControl_Loaded( object sender, RoutedEventArgs e ) { LogHelper.Add("UserControl_Loaded"); }
        //_____________________________________________________________________________________________________________________________________________________________
        private void cWriteLog_Click(object sender, RoutedEventArgs e) { LogHelper.Enabled = (cWriteLog.IsChecked == true); }
        //_____________________________________________________________________________________________________________________________________________________________
        private void cGroupTypes_Click(object sender, RoutedEventArgs e) { bUpdate_Click(sender, e); }
        #endregion
        #region Utility Methods
        //_____________________________________________________________________________________________________________________________________________________________
        public void OpenProjects() {
            LogHelper.Add("OpenProjects");
            cLstPrj.Items.Clear();
            foreach (string name in mProjects.Names) cLstPrj.Items.Add(name);
            }
        //_____________________________________________________________________________________________________________________________________________________________
        public void OpenFiles() {
            LogHelper.Add("OpenFiles");
            mProjects.ManagingFiles = true;
            foreach (string file in mProjects.Project.Scripts)
                try { hostObject.CurrentPowerShellTab.Files.Add(file); }
                catch (Exception ex) { LogHelper.AddException(ex, "OpenFiles", file); }
            mProjects.ManagingFiles = false;
            }
        //_____________________________________________________________________________________________________________________________________________________________
        public void CloseFiles() {
            LogHelper.Add("CloseFiles");
            mProjects.ManagingFiles = true;
            while (hostObject.CurrentPowerShellTab.Files.Count > 0) hostObject.CurrentPowerShellTab.Files.Remove(hostObject.CurrentPowerShellTab.Files[0], true);
            mProjects.ManagingFiles = false;
            }
        //_____________________________________________________________________________________________________________________________________________________________
        private void InvokeCmd(string cmd) {
            LogHelper.Add($"InvokeCmd [{cmd}]");
            try { hostObject.CurrentPowerShellTab.Invoke(cmd); }
            catch (Exception ex) { LogHelper.AddException(ex, "Invoke", cmd); }
            }
        //_____________________________________________________________________________________________________________________________________________________________
        private void SelectNewProject() {
            LogHelper.Add("SelectNewProject");
            LastProjectIndexSelected = cLstPrj.SelectedIndex;
            LastProjectSelected = cLstPrj.SelectedItem.ToString();
            lSelected.Text = LastProjectSelected;
            mProjects.Select(LastProjectSelected);
            cGroupTypes.IsChecked = mProjects.Project.Setting_GroupTypes;
            }
        //_____________________________________________________________________________________________________________________________________________________________
        private bool FileIsOpen(string path) {
            LogHelper.Add("FileIsOpen");
            foreach (ISEFile file in hostObject.CurrentPowerShellTab.Files)
                if (file.FullPath.Equals(path, StringComparison.OrdinalIgnoreCase)) return true;
            return false;
            }
        //_____________________________________________________________________________________________________________________________________________________________
        private void ImportBreakpoints() {
            LogHelper.Add("ImportBreakpoints");
            StringBuilder stringBuilder = new StringBuilder();
            foreach (cBreakPoint breakPoint in mProjects.Project.BreakPoints.Items) {
                if (breakPoint.Updated) continue;
                // show only bk in opened files
                if (!FileIsOpen(breakPoint.FullName)) continue;
                stringBuilder.AppendLine($"$null = Set-PSBreakpoint -Line {breakPoint.Line} -Script {breakPoint.FullName}"+(breakPoint.Enabled?"": " | Disable-PSBreakpoint;"));
                breakPoint.Updated = true;
                }
            InvokeCmd(stringBuilder.ToString());
            }
        //_____________________________________________________________________________________________________________________________________________________________
        private void SaveBreakPoints() {
            LogHelper.Add("SaveBreakPoints");
            if (!PrjSelected) return;
            try { if (!hostObject.CurrentPowerShellTab.CanInvoke) return; } catch (Exception) { return; }
            mProjects.Project.BreakPoints.Items.Clear();
            List<cBreakPoint> lst = new List<cBreakPoint>();
            try {
                lst.AddRange(hostObject.CurrentPowerShellTab.InvokeSynchronous("Get-PSBreakPoint").Select(pso => pso.BaseObject).Cast<LineBreakpoint>().ForEach(b => new cBreakPoint() {
                FullName = b.Script,
                Line = b.Line,
                Enabled = b.Enabled
                }));
                }
            catch (Exception ex) { LogHelper.AddException(ex, "InvokeSynchronous", "Get-PSBreakPoint"); }
            // add only bk in opened files
            foreach (cBreakPoint b in lst) if (FileIsOpen(b.FullName)) mProjects.Project.AddBreakPoint(b);
            }
        //_____________________________________________________________________________________________________________________________________________________________
        private void UpdateTreeView() {
            LogHelper.Add("UpdateTreeView");
            int visibleFunctions = 0;
            try {
                LogHelper.Add("UpdateTreeView->AddScripts");
                cTvFunctions.Dispatcher.Invoke((Action)(() => cTvFunctions.Items.Clear()));
                mProjects.Project.Scripts.Clear();
                mProjects.Project.AddScripts(hostObject.CurrentPowerShellTab.Files.Where(file => !file.IsUntitled).Select(file => file.FullPath).ToArray());
                }
            catch (Exception ex) { LogHelper.AddException(ex, "UpdateTreeView", null); }
            foreach (ISEFile file in hostObject.CurrentPowerShellTab.Files) {
                LogHelper.Add($"UpdateTreeView->Analyze {file.FullPath}");
                bool isFunction = false;
                bool isConfiguration = false;
                bool isWorkflow = false;
                bool inclass = false;
                bool isNewLine = false;
                int countClassLevel = 0;
                int countBracketLevel = 0;
                int countParentLevel = 0;
                int possibleErrLine = -1;
                string className = "";
                mProjects.Project.Functions.RemoveFunctionByFile(file.FullPath);
                FileTreeViewItem scriptItem = new FileTreeViewItem(file.FullPath);
                scriptItem.Header = Path.GetFileName(file.FullPath);
                if (file == hostObject.CurrentPowerShellTab.Files.SelectedFile)
                    scriptItem.IsExpanded = true;
                LogHelper.Add($"UpdateTreeView->Search tokens");
                PSTokenType[] allowedLst = { PSTokenType.GroupStart, PSTokenType.GroupEnd, PSTokenType.Type, PSTokenType.NewLine, PSTokenType.Keyword, PSTokenType.CommandArgument, PSTokenType.Variable };
                foreach (PSToken psToken in PSParser.Tokenize(file.Editor.Text, out Collection<PSParseError> errors).Where(t => allowedLst.Contains(t.Type))) {
                    if ("&{@{}".Contains(psToken.Content) && (psToken.Type == PSTokenType.GroupStart || psToken.Type == PSTokenType.GroupEnd)) countBracketLevel += psToken.Type == PSTokenType.GroupStart ? 1 : -1; 
                    if ("$(@()".Contains(psToken.Content) && (psToken.Type == PSTokenType.GroupStart || psToken.Type == PSTokenType.GroupEnd)) countParentLevel += psToken.Type == PSTokenType.GroupStart ? 1 : -1;
                    if (!inclass && psToken.Type == PSTokenType.Type) continue;
                    if (!inclass && psToken.Type == PSTokenType.NewLine) continue;
                    if (psToken.Content.iEquals("class") && psToken.Type == PSTokenType.Keyword) {
                        if (countBracketLevel + countParentLevel != 0 && possibleErrLine < 0) possibleErrLine = psToken.StartLine;
                        inclass = true;
                        }
                    else if (inclass && className == "" && psToken.Type == PSTokenType.Type) className = psToken.Content;
                    else if (inclass && className != "" && "&{@{}$(@()".Contains(psToken.Content) && (psToken.Type == PSTokenType.GroupStart || psToken.Type == PSTokenType.GroupEnd)) { countClassLevel += psToken.Type == PSTokenType.GroupStart ? 1 : -1; if (countClassLevel == 0) { inclass = false; className = ""; } }
                    else if (inclass && className != "" && countClassLevel == 1 && psToken.Type == PSTokenType.CommandArgument) {
                        if (countBracketLevel != 1 && countParentLevel != 0 && possibleErrLine < 0) possibleErrLine = psToken.StartLine;
                        mProjects.Project.Functions.Items.Add(new cFunction(file.FullPath, psToken.Content, psToken.StartLine, psToken.StartColumn, className));
                        }
                    else if (inclass && className != "" && countClassLevel == 1 && psToken.Type == PSTokenType.NewLine) isNewLine = true;
                    else if (inclass && className != "" && countClassLevel == 1 && psToken.Type == PSTokenType.Variable && isNewLine) {
                        if (countBracketLevel != 1 && countParentLevel != 0 && possibleErrLine < 0) possibleErrLine = psToken.StartLine;
                        mProjects.Project.Functions.Items.Add(new cFunction(file.FullPath, psToken.Content, psToken.StartLine, psToken.StartColumn + 1, className, isPrpty: true)); isNewLine = false;
                        }
                    else if (psToken.Type == PSTokenType.Keyword && psToken.Content.iEquals("function")) isFunction = true;
                    else if (psToken.Type == PSTokenType.Keyword && psToken.Content.iEquals("configuration")) isConfiguration = true;
                    else if (psToken.Type == PSTokenType.Keyword && psToken.Content.iEquals("workflow")) isWorkflow = true;
                    else if (isFunction && psToken.Type == PSTokenType.CommandArgument) {
                        if (countBracketLevel + countParentLevel != 0 && possibleErrLine < 0) possibleErrLine = psToken.StartLine;
                        mProjects.Project.Functions.Items.Add(new cFunction(file.FullPath, psToken.Content, psToken.StartLine, psToken.StartColumn));
                        isFunction = false;
                        }
                    else if (isConfiguration && psToken.Type == PSTokenType.CommandArgument) {
                        if (countBracketLevel + countParentLevel != 0 && possibleErrLine < 0) possibleErrLine = psToken.StartLine;
                        mProjects.Project.Functions.Items.Add(new cFunction(file.FullPath, psToken.Content, psToken.StartLine, psToken.StartColumn, isCfg: true));
                        isConfiguration = false;
                        }
                    else if (isWorkflow && psToken.Type == PSTokenType.CommandArgument) {
                        if (countBracketLevel + countParentLevel != 0 && possibleErrLine < 0) possibleErrLine = psToken.StartLine;
                        mProjects.Project.Functions.Items.Add(new cFunction(file.FullPath, psToken.Content, psToken.StartLine, psToken.StartColumn, isWF: true));
                        isWorkflow = false;
                        }
                    }
                if(countBracketLevel != 0 || countParentLevel != 0) {
                    LogHelper.Add($"{file.FullPath}: Error matching brackets and or parenthesis");
                    scriptItem.Header += "  * review matching ";
                    scriptItem.Header += countBracketLevel != 0 ? "{}" : "";
                    if(!scriptItem.Header.ToString().EndsWith(" ") && countParentLevel != 0) scriptItem.Header += " and ";
                    scriptItem.Header += countParentLevel != 0 ? "()" : "";
                    if (possibleErrLine > 0) scriptItem.Header += $" before line {possibleErrLine}";
                    scriptItem.Foreground = Brushes.Red;
                    }
                LogHelper.Add($"UpdateTreeView->Add functions");
                if (cGroupTypes.IsChecked == true) {
                    visibleFunctions += AddItemsToList(scriptItem, "Worflows", mProjects.Project.Functions.GetFunctionsByFile(file.FullPath).Where(t => t.IsWF).OrderBy(f => f.Alias));
                    visibleFunctions += AddItemsToList(scriptItem, "DSC", mProjects.Project.Functions.GetFunctionsByFile(file.FullPath).Where(t => t.IsCfg).OrderBy(f => f.Alias));
                    visibleFunctions += AddItemsToList(scriptItem, "Functions", mProjects.Project.Functions.GetFunctionsByFile(file.FullPath).Where(t => t.Class == null).OrderBy(f => f.Alias));
                    visibleFunctions += AddItemsToList(scriptItem, "Classes", mProjects.Project.Functions.GetFunctionsByFile(file.FullPath).Where(t => t.Class != null).OrderBy(f => f.Alias));
                    }
                else foreach (cFunction functionDefinition in mProjects.Project.Functions.GetFunctionsByFile(file.FullPath).OrderBy(f => f.Alias)) AddItemToList(scriptItem, functionDefinition);

                LogHelper.Add($"UpdateTreeView->Add item");
                cTvFunctions.Dispatcher.Invoke((Action)(() => cTvFunctions.Items.Add(scriptItem)));
                }
            lOpened.Text = $"{mProjects.Project.Scripts.Count} / {visibleFunctions}";
            LogHelper.Add($"UpdateTreeView->End");
            }
        //_____________________________________________________________________________________________________________________________________________________________
        private int AddItemsToList(TreeViewItem scriptItem, string group, IOrderedEnumerable<cFunction> functions) {
            int total = 0;
            if (functions.Count() == 0) return total;
            if (group != "Classes") {
                TreeViewItem treeViewItem = new TreeViewItem() { Header = group, Tag = null, IsExpanded = true };
                foreach (cFunction functionDefinition in functions) {
                    total += AddItemToList(treeViewItem, functionDefinition);
                    }
                scriptItem.Items.Add(treeViewItem);
                }
            else {
                string className = "";
                foreach (cFunction functionDefinition in functions) {
                    if (className == functionDefinition.Class) continue;
                    TreeViewItem treeViewItem = new TreeViewItem();
                    className = functionDefinition.Class;
                    treeViewItem.Header = $"{className} class";
                    treeViewItem.IsExpanded = true;
                    treeViewItem.Tag = null;
                    scriptItem.Items.Add(treeViewItem);
                    total += AddItemsToList(treeViewItem, "Properties", functions.Where(t => t.Class == className && t.IsPrpty).OrderBy(f => f.Alias));
                    total += AddItemsToList(treeViewItem, "Methods", functions.Where(t => t.Class == className && !t.IsPrpty).OrderBy(f => f.Alias));
                    }
                }
            return total;
            }
        //_____________________________________________________________________________________________________________________________________________________________
        private int AddItemToList(TreeViewItem scriptItem, cFunction functionDefinition) {
            TreeViewItem treeViewItem = new TreeViewItem();
            treeViewItem.Header = CreateChildNode(functionDefinition);
            treeViewItem.Tag = functionDefinition;
            scriptItem.Items.Add(treeViewItem);
            return 1;
            }
        //_____________________________________________________________________________________________________________________________________________________________
        private object CreateChildNode(cFunction f) {
            LogHelper.Add($"CreateChildNode({f.SerializedString})");
            DockPanel dp = new DockPanel() { HorizontalAlignment = HorizontalAlignment.Left, LastChildFill = true };
            DockPanel.SetDock(dp, Dock.Right);
            TextBlock txt = new TextBlock() { Background = Brushes.Transparent, HorizontalAlignment = HorizontalAlignment.Left, Foreground = Brushes.Black, Text = cGroupTypes.IsChecked == true ? f.ShortAlias : f.Alias };
            TextBlock elip = new TextBlock() { Background = Brushes.Transparent, HorizontalAlignment = HorizontalAlignment.Left, Foreground = Brushes.Gray, Text = $" / {f.Line}" };
            DockPanel.SetDock(elip, Dock.Right);
            dp.Children.Add(elip);
            dp.Children.Add(txt);
            return dp;
            }
        //_____________________________________________________________________________________________________________________________________________________________
        private void SaveProject() {
            LogHelper.Add("SaveProject");
            if (!PrjSelected) return;
            try { SaveBreakPoints(); UpdateTreeView(); mProjects.Project.Setting_GroupTypes = (cGroupTypes.IsChecked == true); mProjects.Project.Export(); }
            catch (Exception ex) { LogHelper.AddException(ex, "SaveProject", null); }
            }
        //_____________________________________________________________________________________________________________________________________________________________
        private List<cScriptLocation> GetAllReferenceLocations(PSToken token) {
            LogHelper.Add("GetAllReferenceLocations");
            List<cScriptLocation> l = new List<cScriptLocation>();
            ISEFile file = hostObject.CurrentPowerShellTab.Files.SelectedFile;
            GetReferencesFromScriptText(token, l, file.FullPath, file.Editor.Text);
            foreach (ISEFile f in hostObject.CurrentPowerShellTab.Files) {
                if (file.FullPath.iEquals(f.FullPath)) continue;
                GetReferencesFromScriptText(token, l, f.FullPath, f.Editor.Text);
                }
            return l;
            }
        //_____________________________________________________________________________________________________________________________________________________________
        private void GetReferencesFromScriptText(PSToken token, List<cScriptLocation> l, string FullPath, string txt) {
            if (!GetReferencesFromScriptTextWithParser(token, l, FullPath, txt)) GetReferencesFromScriptTextWithRegEx(token, l, FullPath, txt);
            }
        //_____________________________________________________________________________________________________________________________________________________________
        private void GetReferencesFromScriptTextWithRegEx(PSToken token, List<cScriptLocation> l, string FullPath, string txt) {
            LogHelper.Add("GetReferencesFromScriptTextWithRegEx");
            string pattern = $"[\\W\\b]+({(Regex.Escape(token.Content))})[\\W\\b]+";
            if (!Regex.IsMatch(txt, pattern, RegexOptions.IgnoreCase)) return;
            string[] ltxt = txt.Replace("\r", "").Split('\n');
            for (int i = 0; i < ltxt.Length; i++) {
                foreach (Match m in Regex.Matches($" {ltxt[i]} ", pattern, RegexOptions.IgnoreCase)) {
                    if (token.Type == PSTokenType.Variable)
                        l.Add(new cScriptLocation(Path.GetFileName(FullPath), token.Type, i + 1, m.Groups[1].Index - 1, ltxt, $"${token.Content}", mProjects.Project.Functions.GetFunctionsByLine(FullPath, i + 1)?.Name));
                    else if (token.Type == PSTokenType.String)
                        l.Add(new cScriptLocation(Path.GetFileName(FullPath), token.Type, i + 1, m.Groups[1].Index - 1, ltxt, $"\"{token.Content}\"", mProjects.Project.Functions.GetFunctionsByLine(FullPath, i + 1)?.Name));
                    else
                        l.Add(new cScriptLocation(Path.GetFileName(FullPath), token.Type, i + 1, m.Groups[1].Index, ltxt, token.Content, mProjects.Project.Functions.GetFunctionsByLine(FullPath, i + 1)?.Name));
                    }
                }
            }
        //_____________________________________________________________________________________________________________________________________________________________
        private bool GetReferencesFromScriptTextWithParser(PSToken token, List<cScriptLocation> l, string FullPath, string txt) {
            LogHelper.Add("GetReferencesFromScriptTextWithParser");
            Queue<Token> tokenQueue = new Queue<Token>();
            ScriptBlock scriptBlock;

            try { scriptBlock = ScriptBlock.Create(txt); } catch (Exception ex) { LogHelper.Add($"GetReferencesFromScriptText->Create ERROR: {ex.Message}"); return false; }

            string[] ltxt = txt.Replace("\r", "").Split('\n');
            foreach (PSToken t in PSParser.Tokenize(new object[] { scriptBlock }, out Collection<PSParseError> errors)) {
                tokenQueue.Enqueue(new Token(t));
                if (token.Type != PSTokenType.String) Token.GetTokensFromStringToken(token, txt, tokenQueue, t);
                }
            foreach (Token t in tokenQueue) {
                if (t.Content == null) continue;
                if (!t.Content.iEquals(token.Content)) continue;
                if ((token.Type == PSTokenType.CommandArgument && t.Type == PSTokenType.Command) ||
                    (token.Type == PSTokenType.CommandArgument && t.Type == PSTokenType.CommandArgument) ||
                    (token.Type == PSTokenType.Command && t.Type == PSTokenType.CommandArgument) ||
                    (token.Type == PSTokenType.Command && t.Type == PSTokenType.Command) ||
                    (token.Type == PSTokenType.CommandParameter && t.Type == PSTokenType.CommandParameter) ||
                    (token.Type == PSTokenType.Comment && t.Type == PSTokenType.Comment) ||
                    (token.Type == PSTokenType.Keyword && t.Type == PSTokenType.Keyword) ||
                    (token.Type == PSTokenType.Member && t.Type == PSTokenType.Member) ||
                    (token.Type == PSTokenType.Member && t.Type == PSTokenType.CommandArgument))
                    l.Add(new cScriptLocation(Path.GetFileName(FullPath), t.Type, t.StartLine, t.StartColumn, ltxt, token.Content, mProjects.Project.Functions.GetFunctionsByLine(FullPath, t.StartLine)?.Name));

                else if (token.Type == PSTokenType.Variable && t.Type == PSTokenType.Variable)
                    l.Add(new cScriptLocation(Path.GetFileName(FullPath), t.Type, t.StartLine, t.StartColumn, ltxt, $"${token.Content}", mProjects.Project.Functions.GetFunctionsByLine(FullPath, t.StartLine)?.Name));

                else if (token.Type == PSTokenType.String && t.Type == PSTokenType.String)
                    l.Add(new cScriptLocation(Path.GetFileName(FullPath), t.Type, t.StartLine, t.StartColumn, ltxt, $"\"{token.Content}\"", mProjects.Project.Functions.GetFunctionsByLine(FullPath, t.StartLine)?.Name));
                }
            return true;
            }
        //_____________________________________________________________________________________________________________________________________________________________
        private cFunction GetSelectedFileName(List<cFunction> functions, string currentFile, ISEEditor editor) {
            if (functions.Count == 1) return functions[0];
            List<cScriptLocation> locations = mProjects.Project.Functions.GetScriptLocations(functions);
            using (frmReferences f = new frmReferences()) {
                f.Locations = locations;
                f.SelectDefaultLocation(Path.GetFileName(currentFile), editor.CaretLine);
                f.Command = "Found more than one candidate. Select one of them.";
                f.ShowDialog(new WindowWrapper(WindowWrapper.GetSafeHandle()));
                if (!f.ReferenceSelected) return null;
                return functions.Where(func => func.FullName == f.SelectedLocation.fileName && func.Line == f.SelectedLocation.line).FirstOrDefault();
                }
            }
        //_____________________________________________________________________________________________________________________________________________________________
        private void ShowProjectDetail(bool option = true) {
            cFunctionsGrid.RowDefinitions[0].Height = new GridLength(option ? 30 : 0);
            cFunctionsGrid.RowDefinitions[1].Height = new GridLength(option ? 1 : 0, GridUnitType.Star);
            cFunctionsGrid.RowDefinitions[2].Height = new GridLength(option ? 30 : 0);
            cFunctionsGrid.RowDefinitions[3].Height = new GridLength(option ? 0 : 1, GridUnitType.Star);
            }
        #endregion
        #region Menu Actions
        //_____________________________________________________________________________________________________________________________________________________________
        public void GoBack() {
            LogHelper.Add("GoBack");
            if (BackList.Count == 0) return;
            Tuple<string, int, int> pos = BackList.Pop();
            try {
                hostObject.CurrentPowerShellTab.Files.SetSelectedFile(hostObject.CurrentPowerShellTab.Files.Where(file => file.FullPath.iEquals(pos.Item1)).FirstOrDefault());
                hostObject.CurrentPowerShellTab.Files.SelectedFile.Editor.SetCaretPosition(pos.Item2, pos.Item3);
                }
            catch (Exception ex) { LogHelper.AddException(ex, "GetReferences", null); }
            }
        //_____________________________________________________________________________________________________________________________________________________________
        public void GetReferences() {
            LogHelper.Add("GetReferences");
            ISEEditor editor = hostObject.CurrentPowerShellTab.Files.SelectedFile.Editor;
            string caretLineText = editor.CaretLineText;
            string currentFile = hostObject.CurrentPowerShellTab.Files.SelectedFile.FullPath;
            Tuple<string, int, int> pos = new Tuple<string, int, int>(currentFile, editor.CaretLine, editor.CaretColumn);
            List<PSToken> list = PSParser.Tokenize(caretLineText, out Collection<PSParseError> errors).Where(t => t.Type == PSTokenType.Command && t.StartColumn <= editor.CaretColumn && t.EndColumn >= editor.CaretColumn).ToList();
            if (list.Count == 0) list = PSParser.Tokenize(caretLineText, out errors).Where(t => t.Type == PSTokenType.CommandParameter && t.StartColumn <= editor.CaretColumn && t.EndColumn >= editor.CaretColumn).ToList();
            if (list.Count == 0) list = PSParser.Tokenize(caretLineText, out errors).Where(t => t.Type == PSTokenType.CommandArgument && t.StartColumn <= editor.CaretColumn && t.EndColumn >= editor.CaretColumn).ToList();
            if (list.Count == 0) list = PSParser.Tokenize(caretLineText, out errors).Where(t => t.StartColumn <= editor.CaretColumn && t.EndColumn >= editor.CaretColumn).ToList();
            if (list.Count == 0) return;
            PSToken tkn = list[0];
            List<cScriptLocation> lst = GetAllReferenceLocations(tkn);
            string title = $"{tkn.Content} ({tkn.Type}) [{lst.Count}]";
            if (tkn.Type == PSTokenType.String) {// if tkn is a string with less than 2 results, try to searh into the string for additional tokens 
                PSToken tknsubstr = Token.GetPSTokenInLocation(caretLineText, tkn, editor.CaretColumn);
                if (tknsubstr != null) {
                    List<cScriptLocation> lstsubstr = GetAllReferenceLocations(tknsubstr);
                    if (lstsubstr.Count > 0) {
                        lst.AddRange(lstsubstr);// is string found more tokens replace the list with the new result
                        title+= $" / {tknsubstr.Content} ({tknsubstr.Type}) [{lstsubstr.Count}]";
                        }
                    }
                }
            if (lst.Count == 0) return;
            using (frmReferences f = new frmReferences()) {
                f.Locations = lst;
                f.SelectDefaultLocation(Path.GetFileName(currentFile), editor.CaretLine);
                f.Command = $"References {title}";
                f.ShowDialog(new WindowWrapper(WindowWrapper.GetSafeHandle()));
                if (!f.ReferenceSelected) return;
                cScriptLocation l = f.SelectedLocation;
                try {
                    hostObject.CurrentPowerShellTab.Files.SetSelectedFile(hostObject.CurrentPowerShellTab.Files.Where(file => Path.GetFileName(file.FullPath).iEquals(l.fileName)).FirstOrDefault());
                    hostObject.CurrentPowerShellTab.Files.SelectedFile.Editor.SetCaretPosition(l.line, l.position);
                    hostObject.CurrentPowerShellTab.Files.SelectedFile.Editor.Select(l.line, l.position, l.line, l.position + l.word.Length);
                    BackList.Push(pos);
                    }
                catch (Exception ex) { LogHelper.AddException(ex, "GetReferences", null); }
                }
            }
        //_____________________________________________________________________________________________________________________________________________________________
        public bool GoToDefinition() {
            LogHelper.Add("GoToDefinition");
            try {
                ISEEditor editor = hostObject.CurrentPowerShellTab.Files.SelectedFile.Editor;
                string currentFile = hostObject.CurrentPowerShellTab.Files.SelectedFile.FullPath;
                string caretLineText = editor.CaretLineText;
                Tuple<string, int, int> pos = new Tuple<string, int, int>(currentFile, editor.CaretLine, editor.CaretColumn);
                List<PSToken> list = PSParser.Tokenize(caretLineText, out Collection<PSParseError> errors).Where(t => (t.Type == PSTokenType.Command || t.Type == PSTokenType.Member) && t.StartColumn <= editor.CaretColumn && t.EndColumn >= editor.CaretColumn).ToList();

                List<cFunction> functions = mProjects.Project.Functions.GetFunctionByFileAndName(currentFile, list[0].Content);
                if (functions.Count == 0) functions = mProjects.Project.Functions.GetFunctionByName(list[0].Content);
                if (functions.Count == 0) return false;

                cFunction function = GetSelectedFileName(functions, currentFile, editor);

                ISEFile file = hostObject.CurrentPowerShellTab.Files.Where(x => x.FullPath.iEquals(function.FullName)).FirstOrDefault();
                if (file != null) hostObject.CurrentPowerShellTab.Files.SetSelectedFile(file);
                else {
                    try { hostObject.CurrentPowerShellTab.Files.Add(function.FullName); }
                    catch (Exception ex) { LogHelper.AddException(ex, "GoToDefinition", null); return false; }
                    }
                hostObject.CurrentPowerShellTab.Files.SelectedFile.Editor.SetCaretPosition(functions[0].Line, 1);
                hostObject.CurrentPowerShellTab.Files.SelectedFile.Editor.Select(function.Line, function.Position, function.Line, function.Position + function.Name.Length);
                BackList.Push(pos);
                return true;
                }
            catch (Exception ex) { LogHelper.AddException(ex, "GoToDefinition", null); return false; }
            }
        #endregion
        }
    }
