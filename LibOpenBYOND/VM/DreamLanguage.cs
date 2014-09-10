﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Irony.Parsing;
using Irony.Interpreter;
using Irony.Ast;
using Irony.Interpreter.Ast;

/**************************************************************************
 * WARNING: HERE BE DRAGONS.
 * 
 * THIS FILE DEFINES THE GRAMMAR FOR THE DREAM PROGRAMMING LANGUAGE.
 * 
 * IF YOU HAVEN'T READ ANYTHING ABOUT COMPILERS OR PROGRAMMING LANGUAGES,
 * FLEE NOW AND ONLY RETURN AFTER YOU KNOW WHAT THE FUCK YOU'RE DOING.
 * 
 * THE SMALLEST OF CHANGES CAN PRODUCE BUGS AND REGRESSIONS.
 * 
 * CONSULT N3X15 BEFORE MODIFYING. nexisentertainment@gmail.com
 ***************************************************************************/

namespace OpenBYOND.VM
{
    /// <summary>
    /// We're going to go ahead and call our butchered implementation of the BYOND programming language "Dream".  
    /// I don't know if BYOND already calls it that, but I've only heard it referenced several different ways 
    /// and it's getting annoying calling it shit like "BYOND Programming Language" or something vague like "DM".
    /// 
    /// "Nightmare" would be more honest, but doesn't do too well in the marketting department. If BYOND pulls a 
    /// trademark card, we'll just switch to that.
    /// </summary>
    [Language("Dream", "0.1", "WIP re-implementation of the BYOND game development language.")]
    public class DreamGrammar : InterpretedLanguageGrammar
    {
        // ROADMAP:
        // 1. Basic object-tree shit (atoms)
        // 2. Proc support
        // 3. Preprocessor
        public DreamGrammar()
            : base(caseSensitive: true)
        {
            // Thankfully, we get built-in pythonic numbers, which is exactly what we need.
            var number = TerminalFactory.CreatePythonNumber("number");
            var identifier = TerminalFactory.CreatePythonIdentifier("identifier");
            var comma = ToTerm(",");
            var slash = ToTerm("/");

            /////////////////
            // COMMENTS
            // Dream uses both line comments and block comments.
            var lineComment = new CommentTerminal("lineComment", "//", "\n", "\r");

            // May or may not work, since BYOND permits nesting and most other languages don't.
            var blockComment = new CommentTerminal("blockComment", "/*", "*/");

            // From MiniPython example:
            // //comment must to be added to NonGrammarTerminals list; it is not used directly in grammar rules,
            // // so we add it to this list to let Scanner know that it is also a valid terminal. 
            // However, with Dream, we may need to make them grammar terminals so we can handle nesting properly. - N3X
            base.NonGrammarTerminals.Add(lineComment);
            base.NonGrammarTerminals.Add(blockComment);

            /////////////////
            // BNF rules

            // Blocks
            var procblock = new NonTerminal("procblock", "proc declaration");
            var procchildren = new NonTerminal("procchildren", "proc children");

            var atomblock = new NonTerminal("atomblock", "atom declaration");
            var atomchildren = new NonTerminal("atomchildren", "atom declaration");

            // Path stuff
            var path = new NonTerminal("path");
            var abspath = new NonTerminal("abspath", "absolute path");
            var relpath = new NonTerminal("relpath", "relative path");

            // Parameters
            var paramlist = new NonTerminal("paramlist", "parameter list", typeof(ParamListNode));

            var script = new NonTerminal("script", "script root", typeof(StatementListNode));
            var declblocks = new NonTerminal("declblocks", "declarative blocks");

            // <atomblock> ::= <path> INDENT <atomchildren> DEDENT
            atomblock.Rule = path + Indent + atomchildren + Dedent;

            // <procblock> ::= <path> '(' <paramlist> ')' INDENT <procchildren> DEDENT
            procblock.Rule = path + "(" + paramlist + ")" + Indent + procchildren + Dedent;

            // <script> ::= <declblocks>*
            script.Rule = MakeStarRule(script, declblocks);

            // <declblocks> ::= <procblock>
            //                | <atomblock>
            declblocks.Rule = atomblock
                      | procblock;

            // <paramlist> ::= <parameter>*
            // TODO: Make parameter rule, using IDENTIFIER as placeholder
            paramlist.Rule = MakeStarRule(paramlist, comma, identifier);
        }

        public override void CreateTokenFilters(LanguageData language, TokenFilterList filters)
        {
            var outlineFilter = new CodeOutlineFilter(language.GrammarData,
              OutlineOptions.ProduceIndents | OutlineOptions.CheckBraces, ToTerm(@"\")); // "\" is continuation symbol
            filters.Add(outlineFilter);
        }
    }
}
