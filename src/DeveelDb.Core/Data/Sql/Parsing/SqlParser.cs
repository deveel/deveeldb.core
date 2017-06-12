using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using Antlr4.Runtime;

using Deveel.Data.Sql.Expressions;
using Deveel.Data.Sql.Statements;
using Deveel.Data.Sql.Types;

namespace Deveel.Data.Sql.Parsing {
    public sealed class SqlParser : ISqlParser {
        private PlSqlParser plSqlParser;
        private PlSqlLexer lexer;
        private List<SqlParseMessage> messages;

        public SqlParser() {
            MakeParser(String.Empty, message => messages.Add(message));
            messages = new List<SqlParseMessage>(12);
        }

        string ISqlParser.Dialect => "SQL-99";

        private void MakeParser(string input, Action<SqlParseMessage> messageReceiver) {
            using (var reader = new StringReader(input)) {
                var inputStream = new AntlrInputStream(reader);
                lexer = new PlSqlLexer(inputStream);

                var commonTokenStream = new CommonTokenStream(lexer);

                plSqlParser = new PlSqlParser(commonTokenStream);
                plSqlParser.RemoveErrorListeners();
                plSqlParser.AddErrorListener(new ErrorHandler(messageReceiver));
            }
        }

        public SqlParseResult Parse(string sql) {
            throw new NotImplementedException();
        }

        private void SetInput(string inputString) {
            plSqlParser.SetInputStream(new BufferedTokenStream(new PlSqlLexer(new AntlrInputStream(inputString))));
            messages.Clear();
        }

        public SqlExpressionParseResult ParseExpression(string text) {
            SetInput(text);

            //var plSqlParser = MakeParser(text, null);
            var parseResult = plSqlParser.expressionUnit();

            var visitor = new SqlExpressionVisitor();
            var result = visitor.Visit(parseResult);

            var errors = messages.Where(x => x.Level == SqlParseMessageLevel.Error).Select(x => x.Message).ToArray();
            if (errors.Length > 0)
                return SqlExpressionParseResult.Fail(errors);

            return SqlExpressionParseResult.Success(result);
        }

        public SqlTypeResolveInfo ParseType(string s) {
            SetInput(s);

            //var plSqlParser = MakeParser(s, null);
            var parseResult = plSqlParser.datatype();

            return  SqlTypeParser.GetResolveInfo(parseResult);
        }



        private class ErrorHandler : BaseErrorListener {
            private Action<SqlParseMessage> receiveMessage;

            public ErrorHandler(Action<SqlParseMessage> receiveMessage) {
                this.receiveMessage = receiveMessage;
            }

            public override void SyntaxError(IRecognizer recognizer, IToken offendingSymbol, int line, int charPositionInLine, string msg, RecognitionException e) {
                if (receiveMessage != null)
                    receiveMessage(new SqlParseMessage(msg, SqlParseMessageLevel.Error, new LocationInfo(line, charPositionInLine)));
            }

        }
    }
}