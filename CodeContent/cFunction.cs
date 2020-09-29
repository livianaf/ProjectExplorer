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
        public string Class { get; set; }
        public bool IsPrpty { get; set; }
        public bool IsCfg { get; set; }
        public bool IsWF { get; set; }
        public string Alias { get; set; }
        public string ShortAlias { get; set; }
        //_____________________________________________________________________________________________________________________________________________________________
        public cFunction(string fullPath, string name, int line, int position, string className = null, bool isPrpty = false, bool isCfg = false, bool isWF = false) {
            FullName = fullPath;
            Name = name;
            Line = line;
            Position = position;
            Class = className;
            IsPrpty = isPrpty;
            IsCfg = isCfg;
            IsWF = isWF;
            Alias = (className!=null)? (className != name ? $"{className}.{name}{(isPrpty?"":"()")}": $"{className}.<constructor>()"):(isCfg? $"[Cfg] {name}": (isWF ? $"[Wf] {name}" : $"{name}()"));
            ShortAlias = (className != null) ? (className != name ? $".{name}{(isPrpty ? "" : "()")}" : $".<constructor>()") : (isCfg ? $"{name}" : (isWF ? $"{name}" : $"{name}()"));
            }
        //_____________________________________________________________________________________________________________________________________________________________
        public string SerializedString => $"{FullName};{Name};{Line};{Position}";
        }
    }
