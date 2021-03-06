﻿using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Newtonsoft.Json;
using ME3Explorer;

namespace ME2Explorer
{
    public static class ME2TalkFiles
    {
        public static List<TalkFile> tlkList = new List<TalkFile>();

        public static readonly string LoadedTLKsPath = App.AppDataFolder + "ME2LoadedTLKs.JSON";

        public static void LoadTlkData(string fileName)
        {
            if (File.Exists(fileName))
            {
                TalkFile tlk = new TalkFile();
                tlk.LoadTlkData(fileName);
                tlkList.Add(tlk);
            }
        }
        public static string findDataById(int strRefID, bool withFileName = false)
        {
            string s = "No Data";
            foreach (TalkFile tlk in tlkList)
            {
                s = tlk.findDataById(strRefID, withFileName);
                if (s != "No Data")
                {
                    return s;
                }
            }
            return s;
        }

        public static void LoadSavedTlkList()
        {
            if (File.Exists(LoadedTLKsPath))
            {
                List<string> files = JsonConvert.DeserializeObject<List<string>>(File.ReadAllText(LoadedTLKsPath));
                foreach (string filePath in files)
                {
                    LoadTlkData(filePath);
                }
            }
            else
            {
                string tlkPath = ME2Directory.cookedPath + "BIOGame_INT.tlk";
                LoadTlkData(tlkPath);
            }
        }

        public static void SaveTLKList()
        {
            File.WriteAllText(LoadedTLKsPath, JsonConvert.SerializeObject(tlkList.Select(x => x.path)));
        }

        internal static void ClearLoadedTlks()
        {
            tlkList.Clear();
        }
    }
}
