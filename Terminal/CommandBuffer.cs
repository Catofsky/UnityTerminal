namespace TerminalSystem {

    public class CommandBuffer {

        private string[] lines;
        private int position = -1;

        public CommandBuffer(int size) {
            lines = new string[size];
        }

        public string GetLine() {
            if (position == -1) return "";
            else return lines[position];
        }

        public void Reset() {
            position = -1;
        }

        public void Add(string text) {
            if (string.IsNullOrEmpty(text)) return;
            for (int i = lines.Length - 2; i > 0; i--) {
                lines[i] = lines[i - 1];
            }
            lines[0] = text;
        }

        public void Up() {
            if (position < lines.Length && !string.IsNullOrEmpty(lines[position + 1]))
                position++;
            else return;
        }

        public void Down() {
            if (position > 0)
                position--;
            else return;
        }

    }
}
