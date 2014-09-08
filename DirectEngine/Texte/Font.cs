using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using SharpDX;
using SharpDX.Direct3D11;

namespace DirectEngine.Texte
{
    internal struct CharData
    {
        public float minU;
        public float maxU;
        public int width;

        internal CharData(float minU, float maxU, int width)
        {
            this.minU = minU;
            this.maxU = maxU;
            this.width = width;
        }
    }

    public class Font
    {
        public Game Game { protected set; get; }
        public int Height { protected set; get; }

        public string TextureName { private set; get; }
        private Dictionary<char, CharData> CharDataDict;

        private Font(Game game, string textureName)
        {
            Game = game;
            TextureName = textureName;
            game.LoadTexture2D(textureName);
            Height = (int)game.GetTexture2DSize(textureName).Y;
            CharDataDict = new Dictionary<char, CharData>();
        }

        public static Font LoadFont(Game game, string name)
        {
            string pathWithoutExt = "Content\\Fonts\\" + name;
            if (!File.Exists(pathWithoutExt + ".png") || !File.Exists(pathWithoutExt + ".font"))
                throw new IOException("Can't find the .font or the .png file associated with the name : " + name);

            Font result = new Font(game, pathWithoutExt + ".png");
            using (FileStream stream = File.Open(pathWithoutExt + ".font", FileMode.Open, FileAccess.Read))
            {
                StreamReader reader = new StreamReader(stream);
                string line;
                while (!String.IsNullOrWhiteSpace(line = reader.ReadLine()))
                {
                    char first = line[0];
                    string[] tab = line.Substring(1).Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                    System.Globalization.NumberFormatInfo info = new System.Globalization.NumberFormatInfo();
                    info.NumberDecimalSeparator = ".";
                    CharData data = new CharData(float.Parse(tab[0], info), float.Parse(tab[1], info), int.Parse(tab[2]));
                    result.CharDataDict.Add(first, data);
                }
                reader.Close();
                stream.Close();
            }

            return result;
        }

        public int GetCharWidth(char c)
        {
            if (CharDataDict.ContainsKey(c))
                return CharDataDict[c].width;
            return 0;
        }

        public float getMinU(char c)
        {
            if (CharDataDict.ContainsKey(c))
                return CharDataDict[c].minU;
            return 0;
        }
        public float getMaxU(char c)
        {
            if (CharDataDict.ContainsKey(c))
                return CharDataDict[c].maxU;
            return 0;
        }
    }
}
