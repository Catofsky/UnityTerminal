using System.Collections.Generic;
using System;
using UnityEngine;

public static class Utils {

    public static bool init = false;

    private static Dictionary<char, int> characters = new Dictionary<char, int>();

    public static void InitCharsValues() {
        characters.Add(' ', 0);

        characters.Add('0', 16);
        characters.Add('1', 17);
        characters.Add('2', 18);
        characters.Add('3', 19);
        characters.Add('4', 20);
        characters.Add('5', 21);
        characters.Add('6', 22);
        characters.Add('7', 23);
        characters.Add('8', 24);
        characters.Add('9', 25);

        characters.Add('a', 26);
        characters.Add('b', 27);
        characters.Add('c', 28);
        characters.Add('d', 29);
        characters.Add('e', 30);
        characters.Add('f', 31);
        characters.Add('g', 32);
        characters.Add('h', 33);
        characters.Add('i', 34);
        characters.Add('j', 35);
        characters.Add('k', 36);
        characters.Add('l', 37);
        characters.Add('m', 38);
        characters.Add('n', 39);
        characters.Add('o', 40);
        characters.Add('p', 41);
        characters.Add('q', 42);
        characters.Add('r', 43);
        characters.Add('s', 44);
        characters.Add('t', 45);
        characters.Add('u', 46);
        characters.Add('v', 47);
        characters.Add('w', 48);
        characters.Add('x', 49);
        characters.Add('y', 50);
        characters.Add('z', 51);

        characters.Add('A', 52);
        characters.Add('B', 53);
        characters.Add('C', 54);
        characters.Add('D', 55);
        characters.Add('E', 56);
        characters.Add('F', 57);
        characters.Add('G', 58);
        characters.Add('H', 59);
        characters.Add('I', 60);
        characters.Add('J', 61);
        characters.Add('K', 62);
        characters.Add('L', 63);
        characters.Add('M', 64);
        characters.Add('N', 65);
        characters.Add('O', 66);
        characters.Add('P', 67);
        characters.Add('Q', 68);
        characters.Add('R', 69);
        characters.Add('S', 70);
        characters.Add('T', 71);
        characters.Add('U', 72);
        characters.Add('V', 73);
        characters.Add('W', 74);
        characters.Add('X', 75);
        characters.Add('Y', 76);
        characters.Add('Z', 77);

        characters.Add('~', 78);
        characters.Add('!', 79);
        characters.Add('@', 80);
        characters.Add('#', 81);
        characters.Add('$', 82);
        characters.Add('%', 83);
        characters.Add('^', 84);
        characters.Add('&', 85);
        characters.Add('*', 86);
        characters.Add('(', 87);
        characters.Add(')', 88);
        characters.Add('_', 89);
        characters.Add('+', 90);
        characters.Add('`', 91);
        characters.Add('-', 92);
        characters.Add('=', 93);
        characters.Add('[', 94);
        characters.Add(']', 95);
        characters.Add(';', 96);
        characters.Add('\'', 97);
        characters.Add(',', 98);
        characters.Add('.', 99);
        characters.Add('/', 100);
        characters.Add('\\', 101);
        characters.Add('}', 102);
        characters.Add('{', 103);
        characters.Add(':', 104);
        characters.Add('"', 105);
        characters.Add('|', 106);
        characters.Add('<', 107);
        characters.Add('>', 108);
        characters.Add('?', 109);

        init = true;
    }

    public static int GetCharNumber(char c) {
        int res;
        if (!characters.TryGetValue(c, out res)) res = 0;
        return res;
    }

    public static bool CompareColors(Color a, Color b) {
        return Math.Round(a.r, 2) == Math.Round(b.r, 2) &&
               Math.Round(a.g, 2) == Math.Round(b.g, 2) &&
               Math.Round(a.b, 2) == Math.Round(b.b, 2);
    }

    public static string StringToCenter(string text, int len) {
        return text.PadLeft(((len - text.Length) / 2) + text.Length).PadRight(len);
    }

    public static string StringToRight(string text, int len) {
        return text.PadLeft(len);
    }

    public static string[] Split(string text, int chunk) {
        int size = text.Length / chunk;
        string[] list = new string[size + 1];
        for (int i = 0; i < size; i++) {
            list[i] = text.Substring(i * chunk, chunk);
        }
        list[size] = text.Substring(size * chunk);
        return list;
    }

    public static float IntColorToFloat(int num) {
        if (num >= 255) return 1f;
        if (num <= 0) return 0f;
        return num / 255f;
    }

    public static int FloatColorToInt(float num) {
        if (num >= 1f) return 255;
        if (num <= 0f) return 0;
        return (int) (255 / num);
    }

    public static Color ParseColor(string a, string b, string c) {
        return new Color(Utils.IntColorToFloat(Int32.Parse(a)), 
                         Utils.IntColorToFloat(Int32.Parse(b)), 
                         Utils.IntColorToFloat(Int32.Parse(c)));
    }

    public static Color ParseColor(int a, int b, int c) {
        return new Color(Utils.IntColorToFloat(a),
                         Utils.IntColorToFloat(b),
                         Utils.IntColorToFloat(c));
    }

}
