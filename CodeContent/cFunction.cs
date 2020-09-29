using System;

//_____________________________________________________________________________________________________________________________________________________________
namespace IseAddons {
    //_____________________________________________________________________________________________________________________________________________________________
    [Serializable]
    public class cFunction {
        //_____________________________________________________________________________________________________________________________________________________________
        public string FullName { get; set; }
        public int Line { get; set; }
        public int Position { get; set; }
        public string Name { get; set; }
        public string Alias { get; set; }
        //_____________________________________________________________________________________________________________________________________________________________
        public cFunction(string fullPath, string name, int line, int position, string className = null, bool isPrpty = false, bool isCfg = false, bool isWF = false) {
            FullName = fullPath;
            Name = name;
            Line = line;
            Position = position;
            Alias = (className!=null)? (className != name ? $"{className}.{name}{(isPrpty?"":"()")}": $"{className}.<constructor>()"):(isCfg? $"[Cfg] {name}": (isWF ? $"[Wf] {name}" : $"{name}()"));
            }
        //_____________________________________________________________________________________________________________________________________________________________
        public string SerializedString => $"{FullName};{Name};{Line};{Position}";
        }
    }
