﻿//_____________________________________________________________________________________________________________________________________________________________
namespace IseAddons {
    //_____________________________________________________________________________________________________________________________________________________________
    public class cBreakPoint {
        //_____________________________________________________________________________________________________________________________________________________________
        public string FullName { get; set; }
        //_____________________________________________________________________________________________________________________________________________________________
        public int Line { get; set; }
        //_____________________________________________________________________________________________________________________________________________________________
        public bool Enabled { get; set; }
        //_____________________________________________________________________________________________________________________________________________________________
        public bool Updated { get; set; } = false;
        //_____________________________________________________________________________________________________________________________________________________________
        public string SerializedString => $"{FullName};{Line};{Enabled}";
        }
    }
