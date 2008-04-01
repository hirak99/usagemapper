using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Threading;

namespace UsageMapper
{
    public class FileCollection
    {
        public class FileStructure
        {
            public bool LoadInfo(string FileName) {
                Name = FileName;
                if (Directory.Exists(FileName)) {
                    SubFileNames = new List<string>();
                    try {
                        foreach (string dir in Directory.GetDirectories(FileName))
                            SubFileNames.Add(dir);
                        foreach (string file in Directory.GetFiles(FileName))
                            SubFileNames.Add(file);
                    }
                    catch (Exception) { }
                    return true;
                }
                else if (File.Exists(FileName)) {
                    SubFileNames = null;
                    FileInfo fi = new FileInfo(FileName);
                    Size = fi.Length;
                    return true;
                }
                else return false;
            }
            public string Name;
            public long Size;
            public int SubCount;
            public int SubFileCount, SubDirCount;
            public List<string> SubFileNames;
            public List<FileStructure> SubFiles;
            public bool IsDirectory {
                get { return SubFileNames != null; }
            }
            public override string ToString() {
                return "\"" + Name + (IsDirectory ? "\\" : "") + "\" Size: " + Size;
            }
            public string LastName {
                get {
                    string result = Name.Substring(Name.LastIndexOf('\\') + 1);
                    if (IsDirectory) result += '\\';
                    return result;
                }
            }
        }

        private Dictionary<string, FileStructure> map = new Dictionary<string, FileStructure>();
        private bool CaseSensativeFileNames = false;
        private object locker = new object();
        private int RecursionDepth = 0;
        private bool CancelProcessingFlag;
        public bool IsWorking { get {
                if (Monitor.TryEnter(locker)) {
                    Monitor.Exit(locker);
                    return false;
                }
                else return true;
        } }
        public void CancelProcessing() {
            CancelProcessingFlag = true;
            // To wait till lock is available
            lock (locker) { }
            CancelProcessingFlag = false;
        }
        public delegate void ScanUpdateDelegate(object Sender, string ScanningWhat);
        public event ScanUpdateDelegate ScanUpdate;
        private void FireScanningNow(string Dir) {
            if (ScanUpdate != null) ScanUpdate(this, Dir);
        }
        public FileStructure GetFileByName(string FileName) {
            if (IsWorking) CancelProcessing();
            lock (locker) {
                if (FileName == null || FileName.Length == 0) return null;
                if (FileName[FileName.Length - 1] == '\\') FileName = FileName.Substring(0, FileName.Length - 1);
                if (FileName.Length <= 3) FileName += '\\';
                FileStructure result;
                try {
                    result = map[CaseSensativeFileNames ? FileName : FileName.ToLower()];
                    return result;
                }
                catch (KeyNotFoundException) { }
                result = new FileStructure();
                if (!result.LoadInfo(FileName)) return null;
                if (result.IsDirectory) {
                    FireScanningNow(result.Name);
                    result.SubFiles = new List<FileStructure>();
                    foreach (string sf in result.SubFileNames) {
                        ++RecursionDepth;
                        FileStructure subFile = GetFileByName(sf);
                        --RecursionDepth;
                        if (CancelProcessingFlag) return null;
                        result.Size += subFile.Size;
                        result.SubCount += 1 + subFile.SubCount;
                        result.SubDirCount += subFile.SubDirCount + (subFile.IsDirectory ? 1 : 0);
                        result.SubFileCount += subFile.SubFileCount + (subFile.IsDirectory ? 0 : 1);
                        result.SubFiles.Add(subFile);
                    }
                    result.SubFiles.Sort(delegate(FileStructure fs1, FileStructure fs2) {
                        if (fs1.Size > fs2.Size) return -1;
                        else if (fs1.Size < fs2.Size) return 1;
                        else return 0;
                    });
                }
                map[CaseSensativeFileNames ? result.Name : result.Name.ToLower()] = result;
                return result;
            }
        }
        public void ForgetFolder(string FolderName) {
            lock (locker) {
                FolderName = FolderName.TrimEnd(new char[] { '\\' });
                if (!CaseSensativeFileNames) FolderName = FolderName.ToLower();
                // First store in a temporary list since cannot modify collection while enumerating
                List<String> toDelete = new List<string>();
                foreach (string folder in map.Keys)
                    if (folder.StartsWith(FolderName)) toDelete.Add(folder);
                foreach (string folder in toDelete)
                    map.Remove(folder);
            }
        }
    }
}
