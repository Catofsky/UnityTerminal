using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;

namespace TerminalSystem {

    public class FileController {

        private const int MAX_DIR_LEN = 8;
        private const int MAX_FILE_LEN = 12;

        public string dataPath = Application.dataPath + "/Terminal";
        public string invite = "/";
        private List<string> listDirs = new List<string>();
        private List<string> listFiles = new List<string>();
        private TerminalConsole console;

        public FileController(TerminalConsole console) {
            this.console = console;
        }

        public Result OpenFile(string path) {
            Result res = new Result();
            if (File.Exists(path)) {
                try {
                    FileStream fs = new FileStream(path, FileMode.Open, FileAccess.Read);
                    byte[] bts = new byte[fs.Length];
                    int num = (int)fs.Length;
                    int numb = 0;
                    while (num > 0) {
                        int i = fs.Read(bts, numb, num);
                        if (i == 0)
                            break;
                        num -= i;
                        numb += i;
                    }
                    string fileData = Encoding.UTF8.GetString(bts);
                    res.outLines.AddRange(fileData.Split('\n'));
                    fs.Close();
                }
                catch {
                    res.errors.Add("Cannot open file");
                }
            }
            else {
                res.errors.Add("File is not exists");
            }
            return res;
        }

        public Result CreateFile(string data, string name) {
            string path = new StringBuilder().Append(dataPath).Append(invite).Append(name).ToString();
            Result res = new Result();
            if (name.Length > MAX_FILE_LEN) {
                res.errors.Add("Name is too long");
                return res;
            }
            try {
                FileStream fs = new FileStream(path, FileMode.Create);
                byte[] bts = Encoding.UTF8.GetBytes(data);
                fs.Write(bts, 0, bts.Length);
                fs.Close();
                res.outLines.Add("File created");
            }
            catch {
                res.errors.Add("Can't create file");
            }
            UpdateFileList();
            return res;
        }

        public Result DeleteFile(string name) {
            string path = new StringBuilder().Append(dataPath).Append(invite).ToString();
            Result res = new Result();
            if (name.IndexOf("..") >= 0) {
                res.errors.Add("Command error");
                return res;
            }
            string[] fileList = Directory.GetFiles(path, name);
            int count = 0;
            try {
                foreach (string file in fileList) {
                    if (File.Exists(file)) {
                        File.Delete(file);
                        count++;
                    }
                }
                res.outLines.Add(string.Format("Deleted: {0} files", count));
            }
            catch {
                res.errors.Add("Can't delete files");
            }
            UpdateFileList();
            return res;
        }

        public Result CopyFile(string name, string destination) {
            Result res = new Result();
            if (name.IndexOf("..") >= 0 || destination.IndexOf("..") >= 0) {
                res.errors.Add("Command error");
                return res;
            }
            string from = new StringBuilder().Append(dataPath).Append(invite).ToString();
            string[] fileList = Directory.GetFiles(from, name);
            int count = 0;
            try {
                foreach (string file in fileList) {
                    string[] names = file.Split('/');
                    File.Copy(file, from + destination + '/' + names[names.Length - 1]);
                    count++;
                }
                res.outLines.Add(string.Format("Copied: {0} files", count));
            }
            catch {
                res.errors.Add("Can't copy files");
                foreach (string file in fileList) {
                    string[] names = file.Split('/');
                }
            }
            UpdateFileList();
            return res;
        }

        public Result RenameFile(string name, string newName) {
            Result res = new Result();
            if (name.IndexOf("..") >= 0 || newName.IndexOf("..") >= 0) {
                res.errors.Add("Command error");
                return res;
            }
            string path = new StringBuilder().Append(dataPath).Append(invite).ToString();
            try {
                if (File.Exists(path + name)) {
                    File.Move(path + name, path + newName);
                    res.outLines.Add("Renamed");
                }
                else res.errors.Add("File is not exist");
            }
            catch {
                res.errors.Add("Can't rename file");
            }
            UpdateFileList();
            return res;
        }

        public Result List(string[] args) {
            Result res = new Result();
            bool filesOnly = false;
            bool dirsOnly = false;
            if (args.Length > 0) {
                foreach (string arg in args) {
                    filesOnly = arg.IndexOf('f') >= 0 || filesOnly;
                    dirsOnly = arg.IndexOf('d') >= 0 || dirsOnly;
                }
            }
            string[] list;
            if (filesOnly) {
                list = Directory.GetFiles(dataPath + invite);
            }
            else if (dirsOnly) {
                list = Directory.GetDirectories(dataPath + invite);
            }
            else {
                string[] dirs = Directory.GetDirectories(dataPath + invite);
                string[] files = Directory.GetFiles(dataPath + invite);
                list = dirs.Concat(files).ToArray();
            }

            for (int i = 0; i < list.Length; i++) {
                string file = list[i];
                list[i] = file.Substring(file.LastIndexOf("/") + 1);
            }

            if (list.Length == 0) {
                res.outLines.Add(invite);
                res.outLines.Add("Directory is empty");
            }
            else {
                res.outLines.Add(invite);
                string[] outList = new string[list.Length / 2 + list.Length % 2];
                bool even = list.Length % 2 == 0;
                int sep = (DisplaySystem.COLS - console.Typer.borderWidth * 2 - 2) / 2;
                int j = 0;
                for (int i = 0; i < outList.Length - 1; i++) {
                    outList[i] = list[j].PadRight(sep) + list[j + 1].PadRight(sep);
                    j += 2;
                }
                if (even) {
                    outList[outList.Length - 1] = list[j].PadRight(sep) + list[j + 1].PadRight(sep);
                }
                else {
                    outList[outList.Length - 1] = list[j].PadRight(sep) + " ".PadRight(sep);
                }
                res.outLines.AddRange(outList);
            }
            return res;
        }

        public void UpdateDirsList() {
            listDirs.Clear();
            listDirs.AddRange(Directory.GetDirectories(dataPath + invite));
            for (int i = 0; i < listDirs.Count; i++) {
                string file = listDirs[i];
                listDirs[i] = file.Substring(file.LastIndexOf("/") + 1);
            }
        }

        public void UpdateFileList() {
            listFiles.Clear();
            string[] list = Directory.GetFiles(dataPath + invite);
            for (int i = 0; i < list.Length; i++) {
                string file = list[i];
                list[i] = file.Substring(file.LastIndexOf("/") + 1);
            }
            listFiles.AddRange(list);
        }

        public string Autocomplete(string start, int index, bool files) {
            string[] s = start.Split(' ');
            if (s.Length < 1) return start;
            List<string> list = files ? listFiles : listDirs;
            foreach (string l in list) {
                if (l.StartsWith(s[1])) {
                    if (index != 0) {
                        index--;
                        continue;
                    }
                    return s[0] + ' ' + l;
                }
            }
            return start;
        }

        public Result CreateDirectory(string name) {
            StringBuilder sb = new StringBuilder();
            Result res = new Result();
            if (name.Length > MAX_DIR_LEN) {
                res.errors.Add("Name is too long");
                return res;
            }
            if (name.IndexOf('.') >= 0) {
                res.errors.Add("Name can not contain '.'");
                return res;
            }
            sb.Append(dataPath).Append(invite).Append(name);
            string path = sb.ToString();
            try {
                if (Directory.Exists(path)) {
                    res.errors.Add("Directory is already exists");
                    return res;
                }
                Directory.CreateDirectory(path);
                res.outLines.Add("Directory has been created");
            }
            catch {
                res.errors.Add("Can't create directory");
            }
            UpdateDirsList();
            return res;
        }

        public Result ChangeDirectory(string newDir) {
            Result res = new Result();
            if (newDir == ".") {
                res.outLines.Add(invite);
                return res;
            }
            else if (newDir == "..") {
                if (invite != "/" && invite.Length > 1) {
                    invite = invite.Substring(0, invite.Length - 1);
                    invite = invite.Substring(0, invite.LastIndexOf("/") + 1);
                    res.outLines.Add(invite);
                }
            }
            else if (newDir.StartsWith("/")) {
                invite = "/";
                res.outLines.Add(invite);
            }
            else {
                string path = new StringBuilder().Append(dataPath).Append(invite).Append(newDir).ToString();
                if (Directory.Exists(path)) {
                    invite = invite + newDir.ToLower() + "/";
                    res.outLines.Add(invite);
                }
                else {
                    res.errors.Add(string.Format("Directory {0} is not exist", newDir));
                }
            }
            UpdateDirsList();
            UpdateFileList();
            return res;
        }

        public Result DeleteDirectory(string name) {
            Result res = new Result();
            string path = new StringBuilder().Append(dataPath).Append(invite).Append(name).ToString();
            try {
                Directory.Delete(path, true);
                res.outLines.Add("Directory has been deleted");
            }
            catch {
                res.errors.Add("Can't delete directory");
            }
            UpdateDirsList();
            return res;
        }

        public Result CopyDirectory(string source, string destination, bool copySubDirs) {
            Result res = new Result();
            try {
                DirectoryInfo dir = new DirectoryInfo(source);
                DirectoryInfo[] dirs = dir.GetDirectories();
                if (!dir.Exists) {
                    res.errors.Add(string.Format("Directory {0} is not exist", source));
                    return res;
                }
                if (!Directory.Exists(destination)) {
                    Directory.CreateDirectory(destination);
                }

                FileInfo[] files = dir.GetFiles();
                foreach (FileInfo file in files) {
                    string temppath = Path.Combine(destination, file.Name);
                    file.CopyTo(temppath, false);
                }

                if (copySubDirs) {
                    foreach (DirectoryInfo subdir in dirs) {
                        string temppath = Path.Combine(destination, subdir.Name);
                        CopyDirectory(subdir.FullName, temppath, copySubDirs);
                    }
                }
                res.outLines.Add("Directory copied");
            }
            catch {
                res.errors.Add("Can't copy directory");
                return res;
            }
            UpdateDirsList();
            return res;
        }

    }

}