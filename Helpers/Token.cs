using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Management.Automation;
using System.Text.RegularExpressions;

//_____________________________________________________________________________________________________________________________________________________________
namespace IseAddons {
    //_____________________________________________________________________________________________________________________________________________________________
    class Token {
        //_____________________________________________________________________________________________________________________________________________________________
        public Token(PSToken t) {
            Content = t.Content;
            Type = t.Type;
            Start = t.Start;
            Length = t.Length;
            StartLine = t.StartLine;
            StartColumn = t.StartColumn;
            EndLine = t.EndLine;
            EndColumn = t.EndColumn;
            this.t = t;
            }
        private PSToken t { set; get; }
        public string Content { get; set; }
        public PSTokenType Type { get; set; }
        public int Start { get; set; }
        public int Length { get; set; }
        public int StartLine { get; set; }
        public int StartColumn { get; set; }
        public int EndLine { get; set; }
        public int EndColumn { get; set; }
        //_____________________________________________________________________________________________________________________________________________________________
        public static void GetTokensFromStringToken(PSToken tknToSearch, string txt, Queue<Token> tokenQueue, PSToken token) { GetTokensFromStringToken(tknToSearch, txt, tokenQueue, new Token(token)); }
        //_____________________________________________________________________________________________________________________________________________________________
        public static void GetTokensFromStringToken(PSToken tknToSearch, string txt, Queue<Token> tokenQueue, Token token) {
            LogHelper.Add("GetTokensFromStringToken");
            if (token.Type != PSTokenType.String) return;
            if (tknToSearch!=null && !token.Content.ToLower().Contains(tknToSearch.Content.ToLower())) return;
            if (txt.Split('\n')[token.StartLine - 1][token.StartColumn - 1] != '"') return;
            GetVariableTokensFromStringToken(tknToSearch, txt, tokenQueue, token);
            GetParenthesisTokensFromStringToken(tknToSearch, txt, tokenQueue, token);
            }
        //_____________________________________________________________________________________________________________________________________________________________
        public static PSToken GetPSTokenInLocation(string txt, PSToken token, int CaretColumn) {
            LogHelper.Add("GetPSTokenInLocation");
            Queue<Token> tokenQueue = new Queue<Token>();
            GetTokensFromStringToken(null, txt, tokenQueue, token);
            foreach (Token t in tokenQueue)
                if (t.StartColumn <= CaretColumn && t.EndColumn >= CaretColumn) return t.t;
            return null;
            }
        //_____________________________________________________________________________________________________________________________________________________________
        private static void GetVariableTokensFromStringToken(PSToken tknToSearch, string txt, Queue<Token> tokenQueue, Token token) {
            LogHelper.Add("GetVariableTokensFromStringToken");
            if (!token.Content.Contains("$")) return;
            string newStr = Regex.Replace(token.Content, "[^$_:0-9a-zA-Z]", " ");
            newStr = newStr.Replace("$ ", "  ");
            string pat = @"(\$\w+[:\w]+)\s+";
            Regex r = new Regex(pat, RegexOptions.IgnoreCase);
            Match m = r.Match(newStr);
            Collection<PSParseError> errors = new Collection<PSParseError>();
            while (m.Success) {
                Group g = m.Groups[1];
                CaptureCollection cc = g.Captures;
                for (int j = 0; j < cc.Count; j++) {
                    Capture c = cc[j];
                    //System.Console.WriteLine("Capture" + j + "='" + c + "', Position=" + c.Index);
                    ScriptBlock scriptBlock = ScriptBlock.Create(g.ToString());
                    foreach (PSToken pst in PSParser.Tokenize(new object[] { scriptBlock }, out errors)) {
                        if (pst.Type != PSTokenType.Variable) continue;
                        Token t = new Token(pst);
                        t.StartLine += (token.StartLine - 1);
                        t.StartColumn += (token.StartColumn) + c.Index;
                        t.EndColumn += (token.StartColumn) + c.Index;
                        tokenQueue.Enqueue(t);
                        }
                    }
                m = m.NextMatch();
                }
            }
        //_____________________________________________________________________________________________________________________________________________________________
        private static void GetParenthesisTokensFromStringToken(PSToken tknToSearch, string txt, Queue<Token> tokenQueue, Token token) {
            LogHelper.Add("GetParenthesisTokensFromStringToken");
            if (!token.Content.Contains("$(")) return;
            
            List<Tuple<int, int>> lst = ListOfParenthesis(token.Content);
            foreach (Tuple<int, int> tpl in lst) {
                string script = token.Content.Substring(tpl.Item1, tpl.Item2-tpl.Item1+1);
                Collection<PSParseError> errors = new Collection<PSParseError>();
                ScriptBlock scriptBlock = ScriptBlock.Create(script);
                foreach (PSToken pst in PSParser.Tokenize(new object[] { scriptBlock }, out errors)) {
                    Token t = new Token(pst);
                    t.StartLine += (token.StartLine - 1);
                    t.StartColumn += (token.StartColumn) + tpl.Item1;
                    t.EndColumn += (token.StartColumn) + tpl.Item1;
                    tokenQueue.Enqueue(t);
                    GetTokensFromStringToken(tknToSearch, txt, tokenQueue, t);
                    }
                }
            }
        //_____________________________________________________________________________________________________________________________________________________________
        private static List<Tuple<int,int>> ListOfParenthesis(string txt) {
            int pos = 0;
            int max = txt.Length;
            int num = 0;
            var stack = new Stack<Tuple<int,bool,int>>();
            List<Tuple<int, int>> lst = new List<Tuple<int, int>>();
            while (pos < max) {
                if (pos > 0 && txt[pos] == '(' && txt[pos - 1] == '$') stack.Push(new Tuple<int, bool, int>(pos - 1, true, num++));
                else if (txt[pos] == '(') stack.Push(new Tuple<int, bool, int>(pos, false, 0));
                if (txt[pos] == ')' && stack.Count >= 1 && stack.Peek().Item2 && stack.Peek().Item3 == 0) { lst.Add(new Tuple<int, int>(stack.Peek().Item1, pos)); num = 0; }
                if (txt[pos] == ')' && stack.Count >= 1) stack.Pop();
                pos++;
                }
            return lst;
            }
        }
    }
