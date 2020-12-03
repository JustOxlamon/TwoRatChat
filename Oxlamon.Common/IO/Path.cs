using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Oxlamon.Common.IO {
    public static class PathEx {
        public static void CreateFolderPath( string folderPath ) {
            string[] data = folderPath.Split( new string[] { "\\" }, StringSplitOptions.RemoveEmptyEntries );
            string folder = data[0];
            for (int j = 1; j < data.Length; ++j) {
                folder += "\\" + data[j];
                Directory.CreateDirectory( folder );
            }
        }
    }
}
