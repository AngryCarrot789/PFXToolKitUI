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

using System.Globalization;
using PFXToolKitUI.Utils;

namespace PFXToolKitUI.MathLib;

/// <summary>
/// Tokenizes a simple math expression
/// </summary>
public class MathTokenizer {
    private readonly string expression;
    private int position;

    /// <summary>
    /// Gets the current token parsed as a result of a call to <see cref="MoveNext"/>
    /// </summary>
    public MathToken? CurrentToken { get; private set; }

    private bool HasMoreChars => this.position < this.expression.Length;

    public MathTokenizer(string expression) {
        this.expression = expression;
    }

    public bool MoveNext() {
        MathToken? lastToken = this.CurrentToken;
        this.CurrentToken = null;

        if (this.position >= this.expression.Length) {
            return false;
        }

        return (this.CurrentToken = this.MoveNextInternal(lastToken)) != null;
    }

    private static bool IsRawTokenChar(char ch) {
        switch (ch) {
            case ')':
            case '(':
            case ',':
            case ' ':
                return true;
            default: return false;
        }
    }

    private static bool IsOperatorChar(char ch) {
        switch (ch) {
            case '+':
            case '-':
            case '*':
            case '/':
            case '%':
            case '<':
            case '>':
            case '=':
            case '!':
            case '&':
            case '|':
            case '^':
            case '~':
                return true;
            default: return false;
        }
    }

    private static bool IsValidHexChar(char ch) {
        return char.IsBetween(ch, '0', '9') || char.IsBetween(char.ToUpperInvariant(ch), 'A', 'F');
    }

    private MathToken? MoveNextInternal(MathToken? lastToken) {
        char chTok = this.expression[this.position];
        if (chTok == ' ') {
            int firstPosition = this.position, count = 1;
            for (this.position++; this.HasMoreChars && this.expression[this.position] == ' '; this.position++)
                count++;
            return new MathTokenWhiteSpace(count, firstPosition);
        }

        if (!this.HasMoreChars)
            return null;

        // position is currently at chTok, so increment to next char
        int idxChTok = this.position;
        this.position++;
        switch (chTok) {
            case '(': return new MathToken(TokenType.OpenBracketRound, idxChTok);
            case ')': return new MathToken(TokenType.CloseBracketRound, idxChTok);
            case ',': return new MathToken(TokenType.ArgSeparator, idxChTok);
            case '+': return new MathTokenOperator(OperatorType.Add, idxChTok);
            case '-': return new MathTokenOperator(OperatorType.Sub, idxChTok);
            case '*': return new MathTokenOperator(OperatorType.Mul, idxChTok);
            case '/': return new MathTokenOperator(OperatorType.Div, idxChTok);
            case '%': return new MathTokenOperator(OperatorType.Mod, idxChTok);
            case '<': {
                if (this.HasMoreChars) {
                    switch (this.expression[this.position++]) {
                        case '=': return new MathTokenOperator(OperatorType.LThanEquals, idxChTok);
                        case '<': return new MathTokenOperator(OperatorType.Lshift, idxChTok);
                    }

                    this.position--;
                }

                return new MathTokenOperator(OperatorType.LThan, idxChTok);
            }
            case '>': {
                if (this.HasMoreChars) {
                    switch (this.expression[this.position++]) {
                        case '=': return new MathTokenOperator(OperatorType.GThanEquals, idxChTok);
                        case '>': return new MathTokenOperator(OperatorType.Rshift, idxChTok);
                    }

                    this.position--;
                }

                return new MathTokenOperator(OperatorType.GThan, idxChTok);
            }
            case '=':
            case '!': {
                if (!this.HasMoreChars)
                    throw new Exception("End of expression");
                if (this.expression[this.position] != '=')
                    throw new Exception("Invalid operator: =" + this.expression[this.position]);

                this.position++;
                return new MathTokenOperator(chTok == '=' ? OperatorType.Equals : OperatorType.NotEquals, idxChTok);
            }
            case '&': {
                if (this.HasMoreChars && this.expression[this.position] == '&') {
                    this.position++;
                    return new MathTokenOperator(OperatorType.ConditionalAnd, idxChTok);
                }

                return new MathTokenOperator(OperatorType.LogicalAnd, idxChTok);
            }
            case '|': {
                if (this.HasMoreChars && this.expression[this.position] == '|') {
                    this.position++;
                    return new MathTokenOperator(OperatorType.ConditionalOr, idxChTok);
                }

                return new MathTokenOperator(OperatorType.LogicalOr, idxChTok);
            }
            case '^': return new MathTokenOperator(OperatorType.Xor, idxChTok);
            case '~': return new MathTokenOperator(OperatorType.BitComplement, idxChTok);
            default: {
                // Try parse hexadecimal literal, e.g. 0x45
                if (chTok == '0' && char.ToUpperInvariant(this.expression[this.position]) == 'X') {
                    int i = ++this.position; // incr to index of first literal character, assign to 'i'
                    if (!this.HasMoreChars)
                        throw new Exception("End of expression (parsing hexadecimal literal)");

                    char chNextLit; // for debugging
                    while (IsValidHexChar(chNextLit = this.expression[this.position]))
                        this.position++;

                    // check that literal isn't something like '0x245V'
                    if (!IsRawTokenChar(chNextLit) && !IsOperatorChar(chNextLit))
                        throw new Exception($"Unexpected character in hex literal: '{chNextLit}' at position {this.position}");

                    string literal = this.expression.Substring(i, this.position - i);
                    if (!long.TryParse(literal, NumberStyles.HexNumber, null, out _))
                        throw new Exception($"Hexadecimal literal is too long: '{literal}' at position {i}");

                    return new MathTokenLiteral(literal, idxChTok);
                }
                else if (char.IsBetween(chTok, '0', '9')) {
                    // try parse decimal literal, e.g. 23, 412.26 or just 0 
                    if (!this.HasMoreChars)
                        return new MathTokenLiteral(chTok.ToString(), idxChTok);

                    bool isDouble = false;
                    int i = idxChTok;
                    char chNextLit = this.expression[this.position];
                    while (char.IsDigit(chNextLit) || chNextLit == '.') {
                        this.position++;
                        isDouble |= chNextLit == '.';
                        if (!this.HasMoreChars)
                            break;
                        chNextLit = this.expression[this.position];
                    }

                    // check that literal isn't something like '23B'
                    if (!char.IsDigit(chNextLit) && chNextLit != '.' && !IsRawTokenChar(chNextLit) && !IsOperatorChar(chNextLit))
                        throw new Exception($"Unexpected character in decimal literal: '{chNextLit}' at position {this.position}");

                    string literal = this.expression.Substring(i, this.position - i);
                    if (!(isDouble ? double.TryParse(literal, out _) : long.TryParse(literal, out _)))
                        throw new Exception($"Literal is too long: '{literal}' at position {i}");

                    return new MathTokenLiteral(literal, idxChTok);
                }
                else {
                    if (!this.HasMoreChars)
                        return new MathTokenLiteral(chTok.ToString(), idxChTok);

                    // try parse keyword, e.g. 'int' or 'double', or a function name like 'abs' 
                    int i = idxChTok;
                    char chNextLit = this.expression[this.position];
                    while (!IsOperatorChar(chNextLit) && !IsRawTokenChar(chNextLit)) {
                        this.position++;
                        if (!this.HasMoreChars)
                            break;
                        chNextLit = this.expression[this.position];
                    }

                    string literal = this.expression.Substring(i, this.position - i);
                    return new MathTokenLiteral(literal, idxChTok);
                }
            }
        }
    }
}

public class MathToken {
    public TokenType TokenType { get; }

    public int Position { get; }

    public MathToken(TokenType tokenType, int position) {
        this.TokenType = tokenType;
        this.Position = position;
    }

    public override string ToString() {
        switch (TokenType) {
            case TokenType.OpenBracketRound:    return "(";
            case TokenType.CloseBracketRound:   return ")";
            case TokenType.ArgSeparator: return ",";
        }

        return base.ToString() ?? "";
    }
}

public class MathTokenWhiteSpace : MathToken {
    public int Count { get; }

    public MathTokenWhiteSpace(int count, int position) : base(TokenType.WhiteSpace, position) {
        this.Count = count;
    }

    public override string ToString() => StringUtils.Repeat(' ', this.Count);
}

public class MathTokenLiteral : MathToken {
    public string Content { get; }

    public MathTokenLiteral(string content, int position) : base(TokenType.Literal, position) {
        this.Content = content;
    }
    
    public override string ToString() {
        return $"'{this.Content}'";
    }
}

public class MathTokenOperator : MathToken {
    public OperatorType OperatorType { get; }

    public MathTokenOperator(OperatorType operatorType, int position) : base(TokenType.Operator, position) {
        this.OperatorType = operatorType;
    }
    
    public override string ToString() {
        switch (this.OperatorType) {
            case OperatorType.Add:            return "+";
            case OperatorType.Sub:            return "-";
            case OperatorType.Mul:            return "*";
            case OperatorType.Div:            return "/";
            case OperatorType.Mod:            return "%";
            case OperatorType.LThan:          return "<";
            case OperatorType.LThanEquals:    return "<=";
            case OperatorType.GThan:          return ">";
            case OperatorType.GThanEquals:    return ">=";
            case OperatorType.Equals:         return "==";
            case OperatorType.NotEquals:      return "!=";
            case OperatorType.Lshift:         return "<<";
            case OperatorType.Rshift:         return ">>";
            case OperatorType.LogicalAnd:     return "&";
            case OperatorType.LogicalOr:      return "|";
            case OperatorType.Xor:            return "^";
            case OperatorType.ConditionalAnd: return "&&";
            case OperatorType.ConditionalOr:  return "||";
            case OperatorType.BitComplement:  return "~";
            default:                          throw new ArgumentOutOfRangeException();
        }
    }
}

public enum TokenType {
    /// <summary>A white space, which any amount of repeated spaces</summary>
    WhiteSpace, // ' '
    /// <summary>An opening round bracket character '('</summary>
    OpenBracketRound, // '('
    /// <summary>A closing round bracket character ')'</summary>
    CloseBracketRound, // ')'
    /// <summary>Any kind of literal, including keywords, numbers, function names, etc.</summary>
    Literal, // '45', '0x255'
    /// <summary>An argument separator in a function</summary>
    ArgSeparator, // ','
    /// <summary>An operator, such as '+', '>>', '~', etc. Some operators may be unary, so placed before an expression</summary>
    Operator,
}

public enum OperatorType {
    Add, // '+'
    Sub, // '-'
    Mul, // '*'
    Div, // '/'
    Mod, // '%'
    LThan, // '<'
    LThanEquals, // '<='
    GThan, // '>'
    GThanEquals, // '>='
    Equals, // '=='
    NotEquals, // '!='
    Lshift, // '<<'
    Rshift, // '>>'
    LogicalAnd, // '&'
    LogicalOr, // '|'
    Xor, // '^'
    ConditionalAnd, // '&&'
    ConditionalOr, // '||'
    BitComplement, // '~'
}