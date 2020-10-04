//_____________________________________________________________________________________________________________________________________________________________
using System;
using System.IO;
using System.Management.Automation;

//_____________________________________________________________________________________________________________________________________________________________
namespace IseAddons {
    //_____________________________________________________________________________________________________________________________________________________________
    public class cScriptLocation {
        //_____________________________________________________________________________________________________________________________________________________________
        public string fileName { get; }
        public string functionName { get; }
        public PSTokenType type { get; }
        public int line { get; }
        public int position { get; }
        public string lineText { get; }
        public string[] lineContext { get; }
        public string word { get; }
        private const int contextSize = 4;
        public int ContextSize { get; private set; }
        //_____________________________________________________________________________________________________________________________________________________________
        public cScriptLocation(string fileName, int line, int position, string functionName) : this(fileName, PSTokenType.CommandArgument, line, position, File.ReadAllLines(fileName), functionName, functionName) { }
        //_____________________________________________________________________________________________________________________________________________________________
        public cScriptLocation(string fileName, PSTokenType type, int line, int position, string[] fileContent, string word, string functionName) {
            this.fileName = fileName; this.type = type; this.line = line; this.position = position; this.word = word; this.functionName = functionName;
            lineText = (line > fileContent.Length ? $"...{word}..." : fileContent[-1 + line]);
            if ((line - contextSize) >= fileContent.Length) return;
            int startContext = Math.Max((line - contextSize),0);
            ContextSize = (line - contextSize)>0? contextSize : contextSize + (line - contextSize);
            int endContext = Math.Min((line + contextSize), fileContent.Length);
            lineContext = new string[endContext - startContext];
            Array.Copy(fileContent, startContext, lineContext, 0, (endContext - startContext));
            }
        }
    }
