grammar Sql;

// ====================================================================================
// Parser Rules
// ====================================================================================

root
    : statement
    ;

statement
    : insert_statement
    | select_statement
    | create_table_statement
    | drop_table_statement
    ;

insert_statement
    : INSERT INTO table_name=ID '(' column_list ')' VALUES '(' value_list ')' ';'?
    ;

select_statement
    : SELECT column_list FROM table_source
      (where_clause)?
      (order_by_clause)?
      (limit_clause)?
      ';'?
    ;

table_source
    : table_name=ID (join_clause)*
    ;

join_clause
    : (join_operator)? JOIN join_table=ID ON expression
    ;

join_operator
    : LEFT | RIGHT | INNER
    ;

create_table_statement
    : CREATE TABLE table_name=ID '(' column_definitions ')' ';'?
    ;

drop_table_statement
    : DROP TABLE (IF EXISTS)? table_name=ID ';'?
    ;

where_clause
    : WHERE expression
    ;

order_by_clause
    : ORDER BY column_name=ID (direction=ASC | direction=DESC)?
    ;

limit_clause
    : LIMIT amount=NUMBER (OFFSET offset=NUMBER)?
    ;

// Expression rules rewritten to remove left recursion
expression
    : left=expression op=OR right=boolean_expression   #LogicalOr
    | boolean_expression                             #NestedBoolean
    ;

boolean_expression
    : left=boolean_expression op=AND right=comparison_expression #LogicalAnd
    | comparison_expression                                      #NestedComparison
    ;

comparison_expression
    : left=additive_expression op=(EQUAL | GREATER_THAN | LESS_THAN | GTE | LTE) right=additive_expression #Comparison
    | additive_expression                                                                               #NestedAdditive
    ;

additive_expression
    : left=additive_expression op=(PLUS | MINUS) right=multiplicative_expression #Add
    | multiplicative_expression                                                  #NestedMultiplicative
    ;

multiplicative_expression
    : left=multiplicative_expression op=(ASTERISK | SLASH) right=primary_expression #Mul
    | primary_expression                                                            #NestedPrimary
    ;

primary_expression
    : LPAREN expression RPAREN #ParenthesizedExpression
    | value                    #Literal
    | ID                       #Column
    ;

column_definitions
    : column_def (',' column_def)*
    ;

column_def
    : column_name=ID data_type=ID
    ;

column_list
    : ASTERISK
    | ID (',' ID)*
    ;

value_list
    : value (',' value)*
    ;

value
    : STRING_LITERAL
    | NUMBER
    ;

// ====================================================================================
// Lexer Rules
// ====================================================================================

// Keywords
WHERE: 'WHERE';
ORDER: 'ORDER';
BY: 'BY';
ASC: 'ASC';
DESC: 'DESC';
CREATE: 'CREATE';
TABLE: 'TABLE';
DROP: 'DROP';
SELECT: 'SELECT';
FROM: 'FROM';
INSERT: 'INSERT';
INTO: 'INTO';
VALUES: 'VALUES';
ASTERISK: '*';
IF: 'IF';
EXISTS: 'EXISTS';
LIMIT: 'LIMIT';
OFFSET: 'OFFSET';
AND: 'AND';
OR: 'OR';
JOIN: 'JOIN';
ON: 'ON';
LEFT: 'LEFT';
RIGHT: 'RIGHT';
INNER: 'INNER';

// Operators
EQUAL: '=';
GREATER_THAN: '>';
LESS_THAN: '<';
GTE: '>=';
LTE: '<=';
PLUS: '+';
MINUS: '-';
SLASH: '/';

ID: [a-zA-Z_][a-zA-Z0-9_]*;
STRING_LITERAL: '\'' (~['])* '\'';
NUMBER: [0-9]+;

COMMA: ',';
LPAREN: '(';
RPAREN: ')';
SEMI: ';';

WS: [ \t\r\n]+ -> skip;
COMMENT: '--' .*? ('\r'? '\n' | EOF) -> skip;