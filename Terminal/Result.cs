using System.Collections.Generic;

namespace TerminalSystem {

    public class Result {

        public List<string> outLines;
        public List<string> errors;

        public Result(string[] lines, string[] errors) {
            outLines = new List<string>();
            this.errors = new List<string>();

            outLines.AddRange(lines);
            this.errors.AddRange(errors);
        }

        public Result(string text) {
            outLines = new List<string>();
            errors = new List<string>();
            outLines.Add(text);
        }

        public Result() {
            outLines = new List<string>();
            errors = new List<string>();
        }

    }

}