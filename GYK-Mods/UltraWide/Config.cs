﻿using System.IO;
using System.Text;

namespace UltraWide
{
    public class Config
    {
        private static Options _options;
        private const string Path = "./QMods/UltraWide/config.txt";

        public class Options
        {
            public int Width = GameSettings.me.res_x;
            public int Height = GameSettings.me.res_y;
        }

        private static void SaveDefaultOptions()
        {
            var defaultOptions = new[] { _options.Width.ToString(), _options.Height.ToString() };
            File.WriteAllLines(Path, defaultOptions, Encoding.Default);
        }

        public static Options GetOptions()
        {
            if (_options != null) return _options;
            _options = new Options();
            if (File.Exists(Path))
            {
                var config = File.ReadAllLines(Path);
                if (config.Length > 0)
                {
                    var isWidthNo = int.TryParse(config[0], out var tempW);
                    _options.Width = isWidthNo ? tempW : GameSettings.me.res_x;
                    var isHeightNo = int.TryParse(config[1], out var tempH);
                    _options.Height = isHeightNo ? tempH : GameSettings.me.res_y;
                }
                else
                {
                    SaveDefaultOptions();
                    return _options;
                }
            }
            else
            {
                SaveDefaultOptions();
                return _options;
            }
            return _options;
        }
    }
}
