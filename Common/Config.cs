using System;
using System.Collections.Generic;
using System.IO;

namespace BattleshipsAda
{
    public static class Config
    {
        private const string CONFIG_FILE = "adaship_config.ini";
        private const string BOARD = "board";
        private const string BOAT = "boat";
        private const string SPACE = " ";
        private const char   COLON = ':';
        private const char   X_CHAR = 'x';
        private const char   COMMA = ',';

        private static Tuple<int, int> _boardSize;
        private static List<ShipInfo> _shipDict;
        private static DateTime _modifiedTime;

        private static bool ValidateConfig() {
            if (!File.Exists(CONFIG_FILE)) return false;                                                           // Ensure file exists at path
            
            using var fileStream = File.OpenRead(CONFIG_FILE);
            if (fileStream.CanRead) {                                                                                   // Ensure file is readable
                return fileStream.Length > 0;                                                                           // Ensure file is not empty
            }
            return false;
        }

        private static void Parse() {
            _boardSize = new Tuple<int, int>(10, 10);
            _shipDict = new List<ShipInfo>();
            
            var lines = File.ReadAllLines(CONFIG_FILE);
            foreach (var line in lines) {
                var splitLine = line
                    .ToLower()
                    .Replace(SPACE, string.Empty)
                    .Split(COLON);

                var key = splitLine[0];                                                                           // Config (should be) formatted as `key: value`
                var value = splitLine[1];
                switch (key) {
                    case BOAT: {
                        var boatInfo = value.Split(COMMA);
                        int.TryParse(boatInfo[1], out var shipSize);
                        _shipDict.Add(new ShipInfo(boatInfo[0], shipSize));
                        break;
                    }
                    case BOARD: {
                        var xySize = value.Split(X_CHAR);
                    
                        int.TryParse(xySize[0], out var xSize);
                        int.TryParse(xySize[1], out var ySize);
                        _boardSize = new Tuple<int, int>(xSize, ySize);
                        break;
                    }
                }
            }
            _modifiedTime = File.GetLastWriteTimeUtc(CONFIG_FILE);
        }
        
        // Check if the file has been updated otherwise return the cached values
        public static Configuration LoadConfiguration() {
            if (ValidateConfig()) {                                                                                     // Validate file integrity
                var fileModTime = File.GetLastWriteTimeUtc(CONFIG_FILE);
                if (_boardSize == null || _shipDict == null || _modifiedTime.CompareTo(fileModTime) < 0) {
                    Parse();
                }
            }
            return new Configuration(_boardSize, _shipDict);
        }

        public class Configuration
        {
            public readonly Tuple<int, int> BoardSize;
            public readonly List<ShipInfo> ShipTypes;
            
            public Configuration(Tuple<int, int> boardSize, List<ShipInfo> shipTypes) {
                BoardSize = boardSize;
                ShipTypes = shipTypes;
            }
        }
    }
}