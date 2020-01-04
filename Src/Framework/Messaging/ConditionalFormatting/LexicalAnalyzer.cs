#region Copyright (C) 2004-2012 Zabaleta Asociados SRL
//
// Trx Framework - <http://www.trxframework.org/>
// Copyright (C) 2004-2012  Zabaleta Asociados SRL
//
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU Affero General Public License as
// published by the Free Software Foundation, either version 3 of the
// License, or (at your option) any later version.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Affero General Public License for more details.
//
// You should have received a copy of the GNU Affero General Public License
// along with this program.  If not, see <http://www.gnu.org/licenses/>.
//
#endregion

namespace Trx.Messaging.ConditionalFormatting
{
    using System;
    /*
     * This file was generated by C#Lex, after doing that you must:
     */
    public class LexicalAnalyzer {
        private const int YY_BUFFER_SIZE = 512;
        private const int YY_F = -1;
        private const int YY_NO_STATE = -1;
        private const int YY_NOT_ACCEPT = 0;
        private const int YY_START = 1;
        private const int YY_END = 2;
        private const int YY_NO_ANCHOR = 4;
        private const int YY_BOL = 65536;
        private const int YY_EOF = 65537;

        //private IdentificatorFactory _identificatorFactory = null;
        private int _currentTokenIndex = 0;
        private int _lastRead = 0;
        internal int CurrentTokenIndex {
            get {
                return _currentTokenIndex + yy_buffer_start + 1;
            }
        }
        // This yy_advance() function must remain.
        private int yy_advance() {
            int next_read;
            int i;
            int j;
            if ( yy_buffer_index < yy_buffer_read ) {
                return yy_buffer[yy_buffer_index++];
            }
            if ( 0 != yy_buffer_start ) {
                i = yy_buffer_start;
                j = 0;
                while ( i < yy_buffer_read ) {
                    yy_buffer[j] = yy_buffer[i];
                    ++i;
                    ++j;
                }
                yy_buffer_end = yy_buffer_end - yy_buffer_start;
                yy_buffer_start = 0;
                yy_buffer_read = j;
                yy_buffer_index = j;
                next_read = yy_reader.Read( yy_buffer,
                    yy_buffer_read,
                    yy_buffer.Length - yy_buffer_read );
                if ( 0 == next_read ) {
                    if ( yy_buffer_end > 0 ) {
                        _currentTokenIndex += _lastRead - yy_buffer_end;
                    }
                    _lastRead = 0;
                    return YY_EOF;
                }
                _currentTokenIndex += _lastRead;
                _lastRead = next_read;
                yy_buffer_read = yy_buffer_read + next_read;
            }
            while ( yy_buffer_index >= yy_buffer_read ) {
                if ( yy_buffer_index >= yy_buffer.Length ) {
                    yy_buffer = yy_double( yy_buffer );
                }
                next_read = yy_reader.Read( yy_buffer,
                    yy_buffer_read,
                    yy_buffer.Length - yy_buffer_read );
                if ( 0 == next_read ) {
                    return YY_EOF;
                }
                _currentTokenIndex += _lastRead;
                _lastRead = next_read;
                yy_buffer_read = yy_buffer_read + next_read;
            }
            return yy_buffer[yy_buffer_index++];
        }
        private System.IO.TextReader yy_reader;
        private int yy_buffer_index;
        private int yy_buffer_read;
        private int yy_buffer_start;
        private int yy_buffer_end;
        private char[] yy_buffer;
        private bool yy_at_bol;
        private int yy_lexical_state;

        public LexicalAnalyzer( System.IO.TextReader yy_reader1 )
            : this() {
            if ( null == yy_reader1 ) {
                throw ( new System.Exception( "Error: Bad input stream initializer." ) );
            }
            yy_reader = yy_reader1;
        }

        private LexicalAnalyzer() {
            yy_buffer = new char[YY_BUFFER_SIZE];
            yy_buffer_read = 0;
            yy_buffer_index = 0;
            yy_buffer_start = 0;
            yy_buffer_end = 0;
            yy_at_bol = true;
            yy_lexical_state = YYINITIAL;
        }

        private const int YYINITIAL = 0;
        private static readonly int[] yy_state_dtrans = new int[] {
		0
	};
        private void yybegin( int state ) {
            yy_lexical_state = state;
        }
        private void yy_move_end() {
            if ( yy_buffer_end > yy_buffer_start &&
                '\n' == yy_buffer[yy_buffer_end - 1] )
                yy_buffer_end--;
            if ( yy_buffer_end > yy_buffer_start &&
                '\r' == yy_buffer[yy_buffer_end - 1] )
                yy_buffer_end--;
        }
        private void yy_mark_start() {
            yy_buffer_start = yy_buffer_index;
        }
        private void yy_mark_end() {
            yy_buffer_end = yy_buffer_index;
        }
        private void yy_to_mark() {
            yy_buffer_index = yy_buffer_end;
            yy_at_bol = ( yy_buffer_end > yy_buffer_start ) &&
                        ( '\r' == yy_buffer[yy_buffer_end - 1] ||
                         '\n' == yy_buffer[yy_buffer_end - 1] ||
                         2028/*LS*/ == yy_buffer[yy_buffer_end - 1] ||
                         2029/*PS*/ == yy_buffer[yy_buffer_end - 1] );
        }
        private string yytext() {
            return ( new string( yy_buffer,
                yy_buffer_start,
                yy_buffer_end - yy_buffer_start ) );
        }
        private int yylength() {
            return yy_buffer_end - yy_buffer_start;
        }
        private char[] yy_double( char[] buf ) {
            int i;
            char[] newbuf;
            newbuf = new char[2 * buf.Length];
            for ( i = 0; i < buf.Length; ++i ) {
                newbuf[i] = buf[i];
            }
            return newbuf;
        }
        private const int YY_E_INTERNAL = 0;
        private const int YY_E_MATCH = 1;
        private string[] yy_error_string = {
		"Error: Internal error.\n",
		"Error: Unmatched input.\n"
	};
        private void yy_error( int code, bool fatal ) {
            System.Console.Write( yy_error_string[code] );
            System.Console.Out.Flush();
            if ( fatal ) {
                throw new System.Exception( "Fatal Error.\n" );
            }
        }
        private static int[][] unpackFromString( int size1, int size2, string st ) {
            int colonIndex = -1;
            string lengthString;
            int sequenceLength = 0;
            int sequenceInteger = 0;

            int commaIndex;
            string workString;

            int[][] res = new int[size1][];
            for ( int i = 0; i < size1; i++ )
                res[i] = new int[size2];
            for ( int i = 0; i < size1; i++ ) {
                for ( int j = 0; j < size2; j++ ) {
                    if ( sequenceLength != 0 ) {
                        res[i][j] = sequenceInteger;
                        sequenceLength--;
                        continue;
                    }
                    commaIndex = st.IndexOf( ',' );
                    workString = ( commaIndex == -1 ) ? st :
                        st.Substring( 0, commaIndex );
                    st = st.Substring( commaIndex + 1 );
                    colonIndex = workString.IndexOf( ':' );
                    if ( colonIndex == -1 ) {
                        res[i][j] = System.Int32.Parse( workString );
                        continue;
                    }
                    lengthString =
                        workString.Substring( colonIndex + 1 );
                    sequenceLength = System.Int32.Parse( lengthString );
                    workString = workString.Substring( 0, colonIndex );
                    sequenceInteger = System.Int32.Parse( workString );
                    res[i][j] = sequenceInteger;
                    sequenceLength--;
                }
            }
            return res;
        }
        private int[] yy_acpt = {
		/* 0 */ YY_NOT_ACCEPT,
		/* 1 */ YY_NO_ANCHOR,
		/* 2 */ YY_NO_ANCHOR,
		/* 3 */ YY_NO_ANCHOR,
		/* 4 */ YY_NO_ANCHOR,
		/* 5 */ YY_NO_ANCHOR,
		/* 6 */ YY_NO_ANCHOR,
		/* 7 */ YY_NO_ANCHOR,
		/* 8 */ YY_NO_ANCHOR,
		/* 9 */ YY_NO_ANCHOR,
		/* 10 */ YY_NO_ANCHOR,
		/* 11 */ YY_NO_ANCHOR,
		/* 12 */ YY_NO_ANCHOR,
		/* 13 */ YY_NO_ANCHOR,
		/* 14 */ YY_NO_ANCHOR,
		/* 15 */ YY_NO_ANCHOR,
		/* 16 */ YY_NO_ANCHOR,
		/* 17 */ YY_NO_ANCHOR,
		/* 18 */ YY_NO_ANCHOR,
		/* 19 */ YY_NO_ANCHOR,
		/* 20 */ YY_NOT_ACCEPT,
		/* 21 */ YY_NO_ANCHOR,
		/* 22 */ YY_NOT_ACCEPT,
		/* 23 */ YY_NO_ANCHOR,
		/* 24 */ YY_NOT_ACCEPT,
		/* 25 */ YY_NO_ANCHOR,
		/* 26 */ YY_NOT_ACCEPT,
		/* 27 */ YY_NO_ANCHOR,
		/* 28 */ YY_NOT_ACCEPT,
		/* 29 */ YY_NO_ANCHOR,
		/* 30 */ YY_NOT_ACCEPT,
		/* 31 */ YY_NO_ANCHOR,
		/* 32 */ YY_NOT_ACCEPT,
		/* 33 */ YY_NO_ANCHOR,
		/* 34 */ YY_NOT_ACCEPT,
		/* 35 */ YY_NO_ANCHOR,
		/* 36 */ YY_NOT_ACCEPT,
		/* 37 */ YY_NOT_ACCEPT,
		/* 38 */ YY_NOT_ACCEPT,
		/* 39 */ YY_NOT_ACCEPT
	};
        private int[] yy_cmap = unpackFromString( 1, 65538,
    "21:9,24:2,21,24:2,21:18,24,21:6,20,7,8,21:2,13,21,12,21,14:10,21:3,11,21:3," +
    "3,22:2,5,16,22,21,23,18,21:3,17,4,1,15,21,2,19,6,21:6,9,21,10,21:3,3,22:2,5" +
    ",16,22,21,23,18,21:3,17,4,1,15,21,2,19,6,21:65419,0:2" )[0];

        private int[] yy_rmap = unpackFromString( 1, 40,
    "0,1,2,1:7,3,1:9,4,1,5,6,7,8,9,4,10,11,12,13,14,15,16,14,17,18,19,20" )[0];

        private int[][] yy_nxt = unpackFromString( 21, 25,
    "1,2,21,23,25,27,21,3,4,5,6,7,8,9,10,29,27,31,33,21,35,21,27,21,11,-1:27,12," +
    "-1:25,20,-1,20,-1:8,10,-1,20,-1:5,20,13,-1:4,20,-1,20,-1:8,20,-1,20,-1:5,20" +
    ",13,-1:6,15,-1:22,20,22,20,-1:8,20,-1,20,-1:5,20,13,-1:7,16,-1:19,24,-1:25," +
    "34,-1:40,17,-1:9,26,-1:40,39,-1:11,28,-1:19,32:19,14,32:4,-1:19,30,-1:21,36" +
    ",-1:12,38,-1:26,18,-1:24,19,-1:34,37,-1:8" );

        public Yytoken yylex() {
            int yy_lookahead;
            int yy_anchor = YY_NO_ANCHOR;
            int yy_state = yy_state_dtrans[yy_lexical_state];
            int yy_next_state = YY_NO_STATE;
            int yy_last_accept_state = YY_NO_STATE;
            bool yy_initial = true;
            int yy_this_accept;

            yy_mark_start();
            yy_this_accept = yy_acpt[yy_state];
            if ( YY_NOT_ACCEPT != yy_this_accept ) {
                yy_last_accept_state = yy_state;
                yy_mark_end();
            }
            while ( true ) {
                if ( yy_initial && yy_at_bol )
                    yy_lookahead = YY_BOL;
                else
                    yy_lookahead = yy_advance();
                yy_next_state = YY_F;
                yy_next_state = yy_nxt[yy_rmap[yy_state]][yy_cmap[yy_lookahead]];
                if ( YY_EOF == yy_lookahead && true == yy_initial ) {
                    return null;
                }
                if ( YY_F != yy_next_state ) {
                    yy_state = yy_next_state;
                    yy_initial = false;
                    yy_this_accept = yy_acpt[yy_state];
                    if ( YY_NOT_ACCEPT != yy_this_accept ) {
                        yy_last_accept_state = yy_state;
                        yy_mark_end();
                    }
                }
                else {
                    if ( YY_NO_STATE == yy_last_accept_state ) {
                        throw ( new System.Exception( "Lexical Error: Unmatched Input." ) );
                    }
                    else {
                        yy_anchor = yy_acpt[yy_last_accept_state];
                        if ( 0 != ( YY_END & yy_anchor ) ) {
                            yy_move_end();
                        }
                        yy_to_mark();
                        switch ( yy_last_accept_state ) {
                            case 1:
                                break;
                            case -2:
                                break;
                            case 2: {
                                    return new Yytoken( Token.Error, CurrentTokenIndex,
                                        "Illegal character: " + yytext() );
                                }
                            case -3:
                                break;
                            case 3: {
                                    return new Yytoken( Token.LeftParenthesis, CurrentTokenIndex );
                                }
                            case -4:
                                break;
                            case 4: {
                                    return new Yytoken( Token.RightParenthesis, CurrentTokenIndex );
                                }
                            case -5:
                                break;
                            case 5: {
                                    return new Yytoken( Token.LeftSquareBracket, CurrentTokenIndex );
                                }
                            case -6:
                                break;
                            case 6: {
                                    return new Yytoken( Token.RightSquareBracket, CurrentTokenIndex );
                                }
                            case -7:
                                break;
                            case 7: {
                                    return new Yytoken( Token.EqualsOperator, CurrentTokenIndex );
                                }
                            case -8:
                                break;
                            case 8: {
                                    return new Yytoken( Token.IdentificatorSeparator, CurrentTokenIndex );
                                }
                            case -9:
                                break;
                            case 9: {
                                    return new Yytoken( Token.ParameterSeparator, CurrentTokenIndex );
                                }
                            case -10:
                                break;
                            case 10: {
                                    return new Yytoken( Token.Natural, CurrentTokenIndex, Convert.ToInt32( yytext() ) );
                                }
                            case -11:
                                break;
                            case 11: {
                                    /* Ignore fillers. */
                                    break;
                                }
                            case -12:
                                break;
                            case 12: {
                                    return new Yytoken( Token.Or, CurrentTokenIndex );
                                }
                            case -13:
                                break;
                            case 13: {
                                    return new Yytoken( Token.BinaryConstant, CurrentTokenIndex,
                                        new BinaryConstantExpression( yytext().Substring( 0, yytext().Length - 1 ) ) );
                                }
                            case -14:
                                break;
                            case 14: {
                                    return new Yytoken( Token.StringConstant, CurrentTokenIndex,
                                        new StringConstantExpression( yytext().Substring( 1, yytext().Length - 2 ) ) );
                                }
                            case -15:
                                break;
                            case 15: {
                                    return new Yytoken( Token.And, CurrentTokenIndex );
                                }
                            case -16:
                                break;
                            case 16: {
                                    return new Yytoken( Token.Not, CurrentTokenIndex );
                                }
                            case -17:
                                break;
                            case 17: {
                                    return new Yytoken( Token.Mti, CurrentTokenIndex );
                                }
                            case -18:
                                break;
                            case 18: {
                                    return new Yytoken( Token.IsSet, CurrentTokenIndex );
                                }
                            case -19:
                                break;
                            case 19: {
                                    return new Yytoken( Token.Parent, CurrentTokenIndex );
                                }
                            case -20:
                                break;
                            case 21: {
                                    return new Yytoken( Token.Error, CurrentTokenIndex,
                                        "Illegal character: " + yytext() );
                                }
                            case -21:
                                break;
                            case 23: {
                                    return new Yytoken( Token.Error, CurrentTokenIndex,
                                        "Illegal character: " + yytext() );
                                }
                            case -22:
                                break;
                            case 25: {
                                    return new Yytoken( Token.Error, CurrentTokenIndex,
                                        "Illegal character: " + yytext() );
                                }
                            case -23:
                                break;
                            case 27: {
                                    return new Yytoken( Token.Error, CurrentTokenIndex,
                                        "Illegal character: " + yytext() );
                                }
                            case -24:
                                break;
                            case 29: {
                                    return new Yytoken( Token.Error, CurrentTokenIndex,
                                        "Illegal character: " + yytext() );
                                }
                            case -25:
                                break;
                            case 31: {
                                    return new Yytoken( Token.Error, CurrentTokenIndex,
                                        "Illegal character: " + yytext() );
                                }
                            case -26:
                                break;
                            case 33: {
                                    return new Yytoken( Token.Error, CurrentTokenIndex,
                                        "Illegal character: " + yytext() );
                                }
                            case -27:
                                break;
                            case 35: {
                                    return new Yytoken( Token.Error, CurrentTokenIndex,
                                        "Illegal character: " + yytext() );
                                }
                            case -28:
                                break;
                            default:
                                yy_error( YY_E_INTERNAL, false );
                                break;
                        }
                        yy_initial = true;
                        yy_state = yy_state_dtrans[yy_lexical_state];
                        yy_next_state = YY_NO_STATE;
                        yy_last_accept_state = YY_NO_STATE;
                        yy_mark_start();
                        yy_this_accept = yy_acpt[yy_state];
                        if ( YY_NOT_ACCEPT != yy_this_accept ) {
                            yy_last_accept_state = yy_state;
                            yy_mark_end();
                        }
                    }
                }
            }
        }
    }
}