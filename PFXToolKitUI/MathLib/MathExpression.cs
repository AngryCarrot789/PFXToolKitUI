// 
// Copyright (c) 2024-2025 REghZy
// 
// This file is part of PFXToolKitUI.
// 
// This program is free software; you can redistribute it and/or
// modify it under the terms of the GNU Lesser General Public
// License as published by the Free Software Foundation; either
// version 3 of the License, or (at your option) any later version.
// 
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
// Lesser General Public License for more details.
// 
// You should have received a copy of the GNU Lesser General Public
// License along with PFXToolKitUI. If not, see <https://www.gnu.org/licenses/>.
// 

namespace PFXToolKitUI.MathLib;

public class MathExpression {
    public static void Main() {
        const string expression = "(25 * (int) (sin(45) / 3)) < x && (x & 15) >= abs(x)";
        MathExpression maths = Parse(expression, null, null);
        Console.Write("ok");
    }

    public static MathExpression Parse(string expression, Dictionary<string, double>? variablesD64, Dictionary<string, long>? variablesL64) {
        MathTokenizer tokenizer = new MathTokenizer(expression);
        List<MathToken> tokens = new List<MathToken>();
        while (tokenizer.MoveNext()) {
            tokens.Add(tokenizer.CurrentToken!);
        }

        ParsingContext context = new ParsingContext(tokens, variablesD64, variablesL64);
        context.Parse();
        return null;
    }

    private static void ParseToken(ParsingContext context, MathToken token) {
    }

    private class ParsingContext {
        private readonly IDictionary<string, double> doubleVars;
        private readonly IDictionary<string, long> longVars;
        private readonly List<MathToken> tokens;
        private int position;

        public ParsingContext(List<MathToken> tokens, IDictionary<string, double>? variablesD64, IDictionary<string, long>? variablesL64) {
            this.doubleVars = variablesD64 ?? new Dictionary<string, double>();
            this.longVars = variablesL64 ?? new Dictionary<string, long>();
            this.tokens = tokens;
        }

        public void Parse() {
            for (this.position = 0; this.position < this.tokens.Count; this.position++) {
                MathToken token = this.tokens[this.position];
                switch (token.TokenType) {
                    case TokenType.WhiteSpace:
                        continue;
                    case TokenType.OpenBracketRound:
                        this.ParseParenOpen();
                        break;
                    case TokenType.CloseBracketRound:   
                        throw new InvalidOperationException("Unexpected token: " + token.TokenType);
                    case TokenType.Literal:      break;
                    case TokenType.ArgSeparator: break;
                    case TokenType.Operator:     break;
                    default:                     throw new ArgumentOutOfRangeException();
                }
            }
        }

        public void ParseParenOpen() {
            
        }
    }
}