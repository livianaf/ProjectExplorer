using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Management.Automation;
using System.Windows.Forms;

//_____________________________________________________________________________________________________________________________________________________________
namespace IseAddons {
    //_____________________________________________________________________________________________________________________________________________________________
    class PSSyntaxHelper {
        //_____________________________________________________________________________________________________________________________________________________________
        private static Dictionary<string, string> tokenColours = new Dictionary<string, string>(){
                                    {"Attribute","#FFADD8E6"},
                                    {"Command","#FF0000FF"},
                                    {"CommandArgument","#FF8A2BE2"},
                                    {"CommandParameter","#FF000080"},
                                    {"Comment","#FF006400"},
                                    {"GroupEnd","#FF000000"},
                                    {"GroupStart","#FF000000"},
                                    {"Keyword","#FF00008B"},
                                    {"LineContinuation","#FF000000"},
                                    {"LoopLabel","#FF00008B"},
                                    {"Member","#FF000000"},
                                    {"NewLine","#FF000000"},
                                    {"Number","#FF800080"},
                                    {"Operator","#FFA9A9A9"},
                                    {"Position","#FF000000"},
                                    {"StatementSeparator","#FF000000"},
                                    {"String","#FF8B0000"},
                                    {"Type","#FF008080"},
                                    {"Unknown","#FF000000"},
                                    {"Variable","#FFFF4500"}};
        private static int currentLine = 1;
        private static int currentColumn = 1;
        //_____________________________________________________________________________________________________________________________________________________________
        private static void AppendRtfSpan( RichTextBox cTxt, string block, string tokenColor, int lineNum, int colNum, string word) {
            if (tokenColor.iEquals("NewLine")) { currentLine++; currentColumn = 1; }
            if ((tokenColor.iEquals("NewLine")) || (tokenColor.iEquals("LineContinuation"))) {
                if (tokenColor.iEquals("LineContinuation")) cTxt.AppendText("`");
                cTxt.AppendText("\r\n");
                }
            else {
                string htmlColor = tokenColours[tokenColor].ToString().Replace("#FF", "#");
                if (tokenColor.iEquals("String")) {
                    string[] lines = block.Split(new string[] { "\r\n" }, StringSplitOptions.None);
                    block = "";

                    bool multipleLines = false;

                    foreach (string line in lines) {
                        if (multipleLines) {
                            block += "\r\n";
                            }

                        string newText = line.TrimStart();
                        newText = (new String(' ', line.Length - newText.Length)) + newText;
                        block += newText;
                        multipleLines = true;
                        }
                    }
                cTxt.SelectionStart = cTxt.TextLength;
                cTxt.SelectionLength = 0;
                block = block.Replace("\t", "   ");
                // if (currentLine != lineNum) cTxt.SelectionBackColor = Color.White;
                // else if(currentColumn <= colNum && currentColumn+block.Length>= colNum + word.Length) cTxt.SelectionBackColor = Color.LimeGreen;
                // else cTxt.SelectionBackColor = Color.MistyRose;
                cTxt.SelectionBackColor = currentLine != lineNum ? Color.White : (currentColumn <= colNum && currentColumn + block.Length >= colNum + word.Length) ? Color.LimeGreen : Color.MistyRose;
                cTxt.SelectionColor = ColorTranslator.FromHtml(htmlColor);
                cTxt.AppendText(block.Replace("\t", "   "));
                cTxt.SelectionColor = cTxt.ForeColor;
                currentColumn += (block.Replace("\t", "   ")).Length;
                }
            }
        //_____________________________________________________________________________________________________________________________________________________________
        public static void AddText( RichTextBox cTxt, string text, int lineNum, int colNum, string word ) {
            Collection<PSParseError> errors = new Collection<PSParseError>();
            currentLine = 1;
            currentColumn = 1;
            var tokens = PSParser.Tokenize(text, out errors);

            // Iterate over the tokens and set the colors appropriately.
            int position = 0;
            foreach (PSToken token in tokens) {
                string block, tokenColor;
                if (position < token.Start) {
                    block = text.Substring(position, (token.Start - position));
                    tokenColor = "Unknown";
                    AppendRtfSpan(cTxt, block, tokenColor, lineNum, colNum, word);
                    }

                block = text.Substring(token.Start, token.Length);
                tokenColor = token.Type.ToString();
                AppendRtfSpan(cTxt, block, tokenColor, lineNum, colNum, word);
                position = token.Start + token.Length;
                }
            }
        }
    }
