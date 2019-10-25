﻿using System.IO;
using System.Windows.Controls;

//_____________________________________________________________________________________________________________________________________________________________
namespace IseAddons {
    //_____________________________________________________________________________________________________________________________________________________________
    public class FileTreeViewItem : TreeViewItem {
        //_____________________________________________________________________________________________________________________________________________________________
        public string FileName { get { return Path.GetFileName(this.FullName); } }
        //_____________________________________________________________________________________________________________________________________________________________
        public string FullName { get; }
        //_____________________________________________________________________________________________________________________________________________________________
        public FileTreeViewItem(string fullName) { FullName = fullName; }
        }
    }