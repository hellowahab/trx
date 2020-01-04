// created by jay 0.7 (c) 1998 Axel.Schreiner@informatik.uni-osnabrueck.de

#line 1 "Expressions.jay"

using System;

using System.Runtime.Serialization;

namespace Trx.Messaging.ConditionalFormatting {

public class SemanticParser {
	
#line 14 "-"

  /** simplified error message.
      @see <a href="#yyerror(java.lang.String, java.lang.String[])">yyerror</a>
    */
  public void yyerror (string message) {
    yyerror(message, null);
  }

  /** (syntax) error message.
      Can be overwritten to control message format.
      @param message text to be displayed.
      @param expected vector of acceptable tokens, if available.
    */
  public void yyerror (string message, string[] expected) {
    if ((expected != null) && (expected.Length  > 0)) {
      System.Console.Write ( message+", expecting");
      for (int n = 0; n < expected.Length; ++ n)
        System.Console.Write (" "+expected[n]);
        System.Console.WriteLine ();
    } else
      System.Console.WriteLine (message);
  }

  /** debugging support, requires the package jay.
      Set to null to suppress debugging messages.
    */
  protected yyDebug debug;

  protected static  int yyFinal = 7;
  public static  string [] yyRule = {
    "$accept : LogicalExpression",
    "LogicalExpression : LogicalExpression Or LogicalExpression",
    "LogicalExpression : LogicalExpression And LogicalExpression",
    "LogicalExpression : Not LogicalExpression",
    "LogicalExpression : LogicalFactor",
    "LogicalFactor : LeftParenthesis LogicalExpression RightParenthesis",
    "LogicalFactor : IsSetExpression",
    "LogicalFactor : RelationalExpression",
    "RelationalExpression : EqualsExpression",
    "EqualsExpression : FieldExpression EqualsOperator StringExpression",
    "EqualsExpression : FieldExpression LeftSquareBracket Natural ParameterSeparator Natural RightSquareBracket EqualsOperator StringExpression",
    "EqualsExpression : FieldExpression EqualsOperator BinaryExpression",
    "EqualsExpression : FieldExpression LeftSquareBracket Natural ParameterSeparator Natural RightSquareBracket EqualsOperator BinaryExpression",
    "EqualsExpression : MtiExpression EqualsOperator Natural",
    "FieldExpression : Natural",
    "FieldExpression : SubFieldFactor",
    "FieldExpression : ParentFactor",
    "MtiExpression : Mti",
    "MtiExpression : SubMtiFactor",
    "MtiExpression : ParentMtiFactor",
    "SubMtiFactor : Natural IdentificatorSeparator Mti",
    "SubMtiFactor : Natural IdentificatorSeparator SubMtiFactor",
    "ParentMtiFactor : Parent IdentificatorSeparator Mti",
    "ParentMtiFactor : Parent IdentificatorSeparator SubMtiFactor",
    "ParentMtiFactor : Parent IdentificatorSeparator ParentMtiFactor",
    "IsSetExpression : IsSet LeftParenthesis Natural RightParenthesis",
    "IsSetExpression : IsSet LeftParenthesis SubFieldFactor RightParenthesis",
    "IsSetExpression : IsSet LeftParenthesis ParentFactor RightParenthesis",
    "SubFieldFactor : Natural IdentificatorSeparator Natural",
    "SubFieldFactor : Natural IdentificatorSeparator SubFieldFactor",
    "ParentFactor : Parent IdentificatorSeparator Natural",
    "ParentFactor : Parent IdentificatorSeparator SubFieldFactor",
    "ParentFactor : Parent IdentificatorSeparator ParentFactor",
    "StringExpression : StringConstant",
    "BinaryExpression : BinaryConstant",
  };
  protected static  string [] yyName = {    
    "end-of-file",null,null,null,null,null,null,null,null,null,null,null,
    null,null,null,null,null,null,null,null,null,null,null,null,null,null,
    null,null,null,null,null,null,null,null,null,null,null,null,null,null,
    null,null,null,null,null,null,null,null,null,null,null,null,null,null,
    null,null,null,null,null,null,null,null,null,null,null,null,null,null,
    null,null,null,null,null,null,null,null,null,null,null,null,null,null,
    null,null,null,null,null,null,null,null,null,null,null,null,null,null,
    null,null,null,null,null,null,null,null,null,null,null,null,null,null,
    null,null,null,null,null,null,null,null,null,null,null,null,null,null,
    null,null,null,null,null,null,null,null,null,null,null,null,null,null,
    null,null,null,null,null,null,null,null,null,null,null,null,null,null,
    null,null,null,null,null,null,null,null,null,null,null,null,null,null,
    null,null,null,null,null,null,null,null,null,null,null,null,null,null,
    null,null,null,null,null,null,null,null,null,null,null,null,null,null,
    null,null,null,null,null,null,null,null,null,null,null,null,null,null,
    null,null,null,null,null,null,null,null,null,null,null,null,null,null,
    null,null,null,null,null,null,null,null,null,null,null,null,null,null,
    null,null,null,null,null,null,null,null,null,null,null,null,null,null,
    null,null,null,null,null,null,null,"Error","Or","And","Not",
    "EqualsOperator","LeftParenthesis","RightParenthesis","Natural",
    "Parent","StringConstant","BinaryConstant","IdentificatorSeparator",
    "LeftSquareBracket","RightSquareBracket","ParameterSeparator","IsSet",
    "Mti",
  };

  /** index-checked interface to yyName[].
      @param token single character or %token value.
      @return token name or [illegal] or [unknown].
    */
  public static string GetTokenName( int token) {
    if ((token < 0) || (token > yyName.Length)) return "[illegal]";
    string name;
    if ((name = yyName[token]) != null) return name;
    return "[unknown]";
  }

  /** computes list of expected tokens on error by tracing the tables.
      @param state for which to compute the list.
      @return list of token names.
    */
  protected string[] yyExpecting (int state) {
    int token, n, len = 0;
    bool[] ok = new bool[yyName.Length];

    if ((n = yySindex[state]) != 0)
      for (token = n < 0 ? -n : 0;
           (token < yyName.Length) && (n+token < yyTable.Length); ++ token)
        if (yyCheck[n+token] == token && !ok[token] && yyName[token] != null) {
          ++ len;
          ok[token] = true;
        }
    if ((n = yyRindex[state]) != 0)
      for (token = n < 0 ? -n : 0;
           (token < yyName.Length) && (n+token < yyTable.Length); ++ token)
        if (yyCheck[n+token] == token && !ok[token] && yyName[token] != null) {
          ++ len;
          ok[token] = true;
        }

    string [] result = new string[len];
    for (n = token = 0; n < len;  ++ token)
      if (ok[token]) result[n++] = yyName[token];
    return result;
  }

  /** the generated parser, with debugging messages.
      Maintains a state and a value stack, currently with fixed maximum size.
      @param yyLex scanner.
      @param yydebug debug message writer implementing yyDebug, or null.
      @return result of the last reduction, if any.
      @throws ExpressionCompileException on irrecoverable parse error.
    */
  public Object yyparse (yyInput yyLex, Object yyd)
				 {
    this.debug = (yyDebug)yyd;
    return yyparse(yyLex);
  }

  /** initial size and increment of the state/value stack [default 256].
      This is not final so that it can be overwritten outside of invocations
      of yyparse().
    */
  protected int yyMax;

  /** executed at the beginning of a reduce action.
      Used as $$ = yyDefault($1), prior to the user-specified action, if any.
      Can be overwritten to provide deep copy, etc.
      @param first value for $1, or null.
      @return first.
    */
  protected Object yyDefault (Object first) {
    return first;
  }

  /** the generated parser.
      Maintains a state and a value stack, currently with fixed maximum size.
      @param yyLex scanner.
      @return result of the last reduction, if any.
      @throws ExpressionCompileException on irrecoverable parse error.
    */
  public Object yyparse (yyInput yyLex)
				{
    if (yyMax <= 0) yyMax = 256;			// initial size
    int yyState = 0;                                   // state stack ptr
    int [] yyStates = new int[yyMax];	                // state stack 
    Object yyVal = null;                               // value stack ptr
    Object [] yyVals = new Object[yyMax];	        // value stack
    int yyToken = -1;					// current input
    int yyErrorFlag = 0;				// #tks to shift

    int yyTop = 0;
    goto skip;
    yyLoop:
    yyTop++;
    skip:
    for (;; ++ yyTop) {
      if (yyTop >= yyStates.Length) {			// dynamically increase
        int[] i = new int[yyStates.Length+yyMax];
        System.Array.Copy(yyStates, i, 0);
        yyStates = i;
        Object[] o = new Object[yyVals.Length+yyMax];
        System.Array.Copy(yyVals, o, 0);
        yyVals = o;
      }
      yyStates[yyTop] = yyState;
      yyVals[yyTop] = yyVal;
      if (debug != null) debug.push(yyState, yyVal);

      yyDiscarded: for (;;) {	// discarding a token does not change stack
        int yyN;
        if ((yyN = yyDefRed[yyState]) == 0) {	// else [default] reduce (yyN)
          if (yyToken < 0) {
            yyToken = yyLex.advance() ? yyLex.token() : 0;
            if (debug != null)
              debug.lex(yyState, yyToken, GetTokenName( yyToken), yyLex.value());
          }
          if ((yyN = yySindex[yyState]) != 0 && ((yyN += yyToken) >= 0)
              && (yyN < yyTable.Length) && (yyCheck[yyN] == yyToken)) {
            if (debug != null)
              debug.shift(yyState, yyTable[yyN], yyErrorFlag-1);
            yyState = yyTable[yyN];		// shift to yyN
            yyVal = yyLex.value();
            yyToken = -1;
            if (yyErrorFlag > 0) -- yyErrorFlag;
            goto yyLoop;
          }
          if ((yyN = yyRindex[yyState]) != 0 && (yyN += yyToken) >= 0
              && yyN < yyTable.Length && yyCheck[yyN] == yyToken)
            yyN = yyTable[yyN];			// reduce (yyN)
          else
            switch (yyErrorFlag) {
  
            case 0:
              yyerror("syntax error", yyExpecting(yyState));
              if (debug != null) debug.error("syntax error");
              goto case 1;
            case 1: case 2:
              yyErrorFlag = 3;
              do {
                if ((yyN = yySindex[yyStates[yyTop]]) != 0
                    && (yyN += Token.yyErrorCode) >= 0 && yyN < yyTable.Length
                    && yyCheck[yyN] == Token.yyErrorCode) {
                  if (debug != null)
                    debug.shift(yyStates[yyTop], yyTable[yyN], 3);
                  yyState = yyTable[yyN];
                  yyVal = yyLex.value();
                  goto yyLoop;
                }
                if (debug != null) debug.pop(yyStates[yyTop]);
              } while (-- yyTop >= 0);
              if (debug != null) debug.reject();
				try {
					yyVal = yyLex.value();
				} catch {
					throw new ExpressionCompileException( "Syntax error.", yyLex.LastParsedTokenIndex);
				}
				throw new ExpressionCompileException( string.Format( "Syntax error: {0}", yyVal ),
					yyLex.LastParsedTokenIndex );
  
            case 3:
              if (yyToken == 0) {
                if (debug != null) debug.reject();
				  try {
					  yyVal = yyLex.value();
				  } catch {
					  throw new ExpressionCompileException( "Syntax error at the end of the file.",
						 yyLex.LastParsedTokenIndex);
				  }
				  throw new ExpressionCompileException( string.Format( "Syntax error at the end of the file: {0}", yyVal ),
					yyLex.LastParsedTokenIndex);
              }
              if (debug != null)
                debug.discard(yyState, yyToken, GetTokenName( yyToken),
  							yyLex.value());
              yyToken = -1;
              goto yyDiscarded;		// leave stack alone
            }
        }
        int yyV = yyTop + 1-yyLen[yyN];
        if (debug != null)
          debug.reduce(yyState, yyStates[yyV-1], yyN, yyRule[yyN], yyLen[yyN]);
        yyVal = yyDefault(yyV > yyTop ? null : yyVals[yyV]);
        switch (yyN) {
case 1:
#line 41 "Expressions.jay"
  {
		yyVal = new ConditionalOrOperator( yyVals[-2+yyTop] as IBooleanExpression, yyVals[0+yyTop] as IBooleanExpression);
	  }
  break;
case 2:
#line 45 "Expressions.jay"
  {
		yyVal = new ConditionalAndOperator( yyVals[-2+yyTop] as IBooleanExpression, yyVals[0+yyTop] as IBooleanExpression);
	  }
  break;
case 3:
#line 49 "Expressions.jay"
  {
		yyVal = new NegationOperator( yyVals[0+yyTop] as IBooleanExpression);
	  }
  break;
case 5:
#line 57 "Expressions.jay"
  {
		yyVal = yyVals[-1+yyTop];
	  }
  break;
case 9:
#line 71 "Expressions.jay"
  {
		yyVal = new FieldValueEqualsStringOperator( yyVals[-2+yyTop] as IMessageExpression,
			yyVals[0+yyTop] as StringConstantExpression );
	  }
  break;
case 10:
#line 76 "Expressions.jay"
  {
		yyVal = new MidEqualsStringOperator( yyVals[-7+yyTop] as IMessageExpression,
			yyVals[0+yyTop] as StringConstantExpression, ( int )yyVals[-5+yyTop],  ( int )yyVals[-3+yyTop] );
	  }
  break;
case 11:
#line 81 "Expressions.jay"
  {
		yyVal = new FieldValueEqualsBinaryOperator( yyVals[-2+yyTop] as IMessageExpression,
			yyVals[0+yyTop] as BinaryConstantExpression );
	  }
  break;
case 12:
#line 86 "Expressions.jay"
  {
		yyVal = new MidEqualsBinaryOperator( yyVals[-7+yyTop] as IMessageExpression,
			yyVals[0+yyTop] as BinaryConstantExpression, ( int )yyVals[-5+yyTop],  ( int )yyVals[-3+yyTop] );
	  }
  break;
case 13:
#line 91 "Expressions.jay"
  {
		yyVal = new MtiEqualsExpression( ( int )yyVals[0+yyTop], yyVals[-2+yyTop] as IMessageExpression );
	  }
  break;
case 14:
#line 99 "Expressions.jay"
  {
		yyVal = new MessageExpression( ( int )yyVals[0+yyTop] );
	  }
  break;
case 17:
#line 109 "Expressions.jay"
  {
		yyVal = new MessageExpression();
	  }
  break;
case 20:
#line 118 "Expressions.jay"
  {
		yyVal = new SubMessageExpression( ( int )yyVals[-2+yyTop], new MessageExpression() );
	  }
  break;
case 21:
#line 122 "Expressions.jay"
  {
		yyVal = new SubMessageExpression( ( int )yyVals[-2+yyTop], yyVals[0+yyTop] as SubMessageExpression );
	  }
  break;
case 22:
#line 129 "Expressions.jay"
  {
		yyVal = new ParentMessageExpression( new MessageExpression() );
	  }
  break;
case 23:
#line 133 "Expressions.jay"
  {
		yyVal = new ParentMessageExpression( yyVals[0+yyTop] as SubMessageExpression );
	  }
  break;
case 24:
#line 137 "Expressions.jay"
  {
		yyVal = new ParentMessageExpression( yyVals[0+yyTop] as ParentMessageExpression );
	  }
  break;
case 25:
#line 145 "Expressions.jay"
  {
		yyVal = new IsSetExpression( new MessageExpression( ( int )yyVals[-1+yyTop] ) );
	  }
  break;
case 26:
#line 149 "Expressions.jay"
  {
		yyVal = new IsSetExpression( yyVals[-1+yyTop] as SubMessageExpression );
	  }
  break;
case 27:
#line 153 "Expressions.jay"
  {
		yyVal = new IsSetExpression( yyVals[-1+yyTop] as ParentMessageExpression );
	  }
  break;
case 28:
#line 160 "Expressions.jay"
  {
		yyVal = new SubMessageExpression( ( int )yyVals[-2+yyTop], new MessageExpression( ( int )yyVals[0+yyTop] ) );
	  }
  break;
case 29:
#line 164 "Expressions.jay"
  {
		yyVal = new SubMessageExpression( ( int )yyVals[-2+yyTop], yyVals[0+yyTop] as SubMessageExpression );
	  }
  break;
case 30:
#line 171 "Expressions.jay"
  {
		yyVal = new ParentMessageExpression( new MessageExpression( ( int )yyVals[0+yyTop] ) );
	  }
  break;
case 31:
#line 175 "Expressions.jay"
  {
		yyVal = new ParentMessageExpression( yyVals[0+yyTop] as SubMessageExpression );
	  }
  break;
case 32:
#line 179 "Expressions.jay"
  {
		yyVal = new ParentMessageExpression( yyVals[0+yyTop] as ParentMessageExpression );
	  }
  break;
#line 433 "-"
        }
        yyTop -= yyLen[yyN];
        yyState = yyStates[yyTop];
        int yyM = yyLhs[yyN];
        if (yyState == 0 && yyM == 0) {
          if (debug != null) debug.shift(0, yyFinal);
          yyState = yyFinal;
          if (yyToken < 0) {
            yyToken = yyLex.advance() ? yyLex.token() : 0;
            if (debug != null)
               debug.lex(yyState, yyToken,GetTokenName( yyToken), yyLex.value());
          }
          if (yyToken == 0) {
            if (debug != null) debug.accept(yyVal);
            return yyVal;
          }
          goto yyLoop;
        }
        if (((yyN = yyGindex[yyM]) != 0) && ((yyN += yyState) >= 0)
            && (yyN < yyTable.Length) && (yyCheck[yyN] == yyState))
          yyState = yyTable[yyN];
        else
          yyState = yyDgoto[yyM];
        if (debug != null) debug.shift(yyStates[yyTop], yyState);
	 goto yyLoop;
      }
    }
  }

   static  short [] yyLhs  = {              -1,
    0,    0,    0,    0,    1,    1,    1,    3,    4,    4,
    4,    4,    4,    5,    5,    5,    8,    8,    8,   11,
   11,   12,   12,   12,    2,    2,    2,    9,    9,   10,
   10,   10,    6,    7,
  };
   static  short [] yyLen = {           2,
    3,    3,    2,    1,    3,    1,    1,    1,    3,    8,
    3,    8,    3,    1,    1,    1,    1,    1,    1,    3,
    3,    3,    3,    3,    4,    4,    4,    3,    3,    3,
    3,    3,    1,    1,
  };
   static  short [] yyDefRed = {            0,
    0,    0,    0,    0,    0,   17,    0,    4,    6,    7,
    8,    0,    0,   15,   16,   18,   19,    3,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    5,    0,   20,
   29,   21,    0,   22,   31,   32,   23,   24,    0,    0,
    0,    0,    0,    2,   33,   34,    9,   11,    0,   13,
   25,    0,    0,   26,   27,    0,    0,    0,    0,    0,
    0,   10,   12,
  };
  protected static  short [] yyDgoto  = {             7,
    8,    9,   10,   11,   12,   47,   48,   13,   14,   15,
   16,   17,
  };
  protected static  short [] yySindex = {         -251,
 -251, -251, -268, -256, -229,    0, -218,    0,    0,    0,
    0, -244, -225,    0,    0,    0,    0,    0, -220, -254,
 -249, -219, -251, -251, -217, -222, -211,    0, -268,    0,
    0,    0, -268,    0,    0,    0,    0,    0, -236, -224,
 -209, -208, -203,    0,    0,    0,    0,    0, -214,    0,
    0, -206, -213,    0,    0, -205, -207, -207, -210, -199,
 -217,    0,    0,
  };
  protected static  short [] yyRindex = {            0,
    0,    0, -243,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0, -241,    0,
    0,    0, -238,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    1,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0, -200, -198,    0,    0,
    0,    0,    0,
  };
  protected static  short [] yyGindex = {            6,
    0,    0,    0,    0,    0,    3,    5,    0,  -18,  -16,
   27,   46,
  };
  protected static  short [] yyTable = {            20,
    1,   31,   35,   41,   36,   42,   18,   19,    1,   29,
    2,   21,    3,    4,   33,    4,   25,   14,   30,   28,
    5,    6,   30,   34,   26,   14,   51,   28,   43,   44,
   30,   52,   22,   31,   35,   27,   36,   23,   24,   23,
   24,   49,   28,   53,   39,   40,   32,   37,   45,   46,
   58,   40,   50,   54,   55,   24,   56,   57,   59,   60,
   52,   61,   28,   62,   30,   63,   38,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    1,    0,
    0,    0,    0,    1,
  };
  protected static  short [] yyCheck = {           268,
    0,   20,   21,   22,   21,   22,    1,    2,  260,  264,
  262,  268,  264,  265,  264,  265,  261,  261,  273,  261,
  272,  273,  261,  273,  269,  269,  263,  269,   23,   24,
  269,  268,  262,   52,   53,  261,   53,  258,  259,  258,
  259,  264,  263,  268,  264,  265,   20,   21,  266,  267,
  264,  265,  264,  263,  263,  259,  271,  264,  264,  270,
  268,  261,  263,   61,  263,   61,   21,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,  258,   -1,
   -1,   -1,   -1,  263,
  };

#line 194 "Expressions.jay"

}
#line 572 "-"
	 public interface yyDebug {
		 void push (int state, Object value);
		 void lex (int state, int token, string name, Object value);
		 void shift (int from, int to, int errorFlag);
		 void pop (int state);
		 void discard (int state, int token, string name, Object value);
		 void reduce (int from, int to, int rule, string text, int len);
		 void shift (int from, int to);
		 void accept (Object value);
		 void error (string message);
		 void reject ();
	 }
	 
	 class yyDebugSimple : yyDebug {
		 void println (string s){
			 Console.WriteLine (s);
		 }
		 
		 public void push (int state, Object value) {
			 println ("push\tstate "+state+"\tvalue "+value);
		 }
		 
		 public void lex (int state, int token, string name, Object value) {
			 println("lex\tstate "+state+"\treading "+name+"\tvalue "+value);
		 }
		 
		 public void shift (int from, int to, int errorFlag) {
			 switch (errorFlag) {
			 default:				// normally
				 println("shift\tfrom state "+from+" to "+to);
				 break;
			 case 0: case 1: case 2:		// in error recovery
				 println("shift\tfrom state "+from+" to "+to
					     +"\t"+errorFlag+" left to recover");
				 break;
			 case 3:				// normally
				 println("shift\tfrom state "+from+" to "+to+"\ton error");
				 break;
			 }
		 }
		 
		 public void pop (int state) {
			 println("pop\tstate "+state+"\ton error");
		 }
		 
		 public void discard (int state, int token, string name, Object value) {
			 println("discard\tstate "+state+"\ttoken "+name+"\tvalue "+value);
		 }
		 
		 public void reduce (int from, int to, int rule, string text, int len) {
			 println("reduce\tstate "+from+"\tuncover "+to
				     +"\trule ("+rule+") "+text);
		 }
		 
		 public void shift (int from, int to) {
			 println("goto\tfrom state "+from+" to "+to);
		 }
		 
		 public void accept (Object value) {
			 println("accept\tvalue "+value);
		 }
		 
		 public void error (string message) {
			 println("error\t"+message);
		 }
		 
		 public void reject () {
			 println("reject");
		 }
		 
	 }
// %token constants
 class Token {
  public const int Error = 257;
  public const int Or = 258;
  public const int And = 259;
  public const int Not = 260;
  public const int EqualsOperator = 261;
  public const int LeftParenthesis = 262;
  public const int RightParenthesis = 263;
  public const int Natural = 264;
  public const int Parent = 265;
  public const int StringConstant = 266;
  public const int BinaryConstant = 267;
  public const int IdentificatorSeparator = 268;
  public const int LeftSquareBracket = 269;
  public const int RightSquareBracket = 270;
  public const int ParameterSeparator = 271;
  public const int IsSet = 272;
  public const int Mti = 273;
  public const int yyErrorCode = 256;
 }

  /** must be implemented by a scanner object to supply input to the parser.
    */
  public interface yyInput {
    /** move on to next token.
        @return false if positioned beyond tokens.
        @throws IOException on input error.
      */
    bool advance (); // throws java.io.IOException;
    /** classifies current token.
        Should not be called if advance() returned false.
        @return current %token or single character.
      */
    int token ();

		/// <summary>
		/// It returns the index of the last parsed token.
		/// </summary>
		int LastParsedTokenIndex {

			get;
		}
    /** associated with current token.
        Should not be called if advance() returned false.
        @return value for token().
      */
    Object value ();
  }
} // close outermost namespace, that MUST HAVE BEEN opened in the prolog
