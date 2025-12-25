using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Kawazu;

namespace SaturnEdit.Utilities;

public class ReadingConverter
{
        /// <summary>
    /// Returns the "reading" string for a specified title.
    /// </summary>
    public static async Task<string> Convert(string title)
    {
        string result = title;

        foreach (KeyValuePair<char, char> pair in FullWidthDict)
        {
            result = result.Replace(pair.Key, pair.Value);
        }

        KawazuConverter converter = new();
        result = await converter.Convert(result);

        result = new(result.Where(c => ValidCharacters.Contains(c)).ToArray());

        return result;
    }
    
    private static readonly HashSet<char> ValidCharacters =
    [
        'あ', 'い', 'う', 'え', 'お',
        'か', 'き', 'く', 'け', 'こ',
        'さ', 'し', 'す', 'せ', 'そ',
        'た', 'ち', 'つ', 'て', 'と',
        'な', 'に', 'ぬ', 'ね', 'の',
        'は', 'ひ', 'ふ', 'へ', 'ほ',
        'ま', 'み', 'む', 'め', 'も',
        'や', 'ゆ', 'よ',
        'ら', 'り', 'る', 'れ', 'ろ',
        'わ', 'ゐ', 'ゑ', 'を',
        'が', 'ぎ', 'ぐ', 'げ', 'ご',
        'ざ', 'じ', 'ず', 'ぜ', 'ぞ',
        'だ', 'ぢ', 'づ', 'で', 'ど',
        'ば', 'び', 'ぶ', 'べ', 'ぼ',
        'ぱ', 'ぴ', 'ぷ', 'ぺ', 'ぽ',
        'ん','っ',
        
        'ぁ','ぃ','ぅ','ぇ','ぉ',
        'ゃ','ゅ','ょ',

        '１', '２', '３', '４', '５', '６', '７', '８', '９', '０',
        'Ａ', 'Ｂ', 'Ｃ', 'Ｄ', 'Ｅ', 'Ｆ', 'Ｇ', 'Ｈ', 'Ｉ', 'Ｊ', 'Ｋ', 'Ｌ', 'Ｍ', 'Ｎ', 'Ｏ', 'Ｐ', 'Ｑ', 'Ｒ', 'Ｓ', 'Ｔ', 'Ｕ', 'Ｖ', 'Ｗ', 'Ｘ', 'Ｙ', 'Ｚ',
    ];

    private static readonly Dictionary<char, char> FullWidthDict = new()
    {
        ['A'] = 'Ａ', ['a'] = 'Ａ',
        ['B'] = 'Ｂ', ['b'] = 'Ｂ',
        ['C'] = 'Ｃ', ['c'] = 'Ｃ',
        ['D'] = 'Ｄ', ['d'] = 'Ｄ',
        ['E'] = 'Ｅ', ['e'] = 'Ｅ',
        ['F'] = 'Ｆ', ['f'] = 'Ｆ',
        ['G'] = 'Ｇ', ['g'] = 'Ｇ',
        ['H'] = 'Ｈ', ['h'] = 'Ｈ',
        ['I'] = 'Ｉ', ['i'] = 'Ｉ',
        ['J'] = 'Ｊ', ['j'] = 'Ｊ',
        ['K'] = 'Ｋ', ['k'] = 'Ｋ',
        ['L'] = 'Ｌ', ['l'] = 'Ｌ',
        ['M'] = 'Ｍ', ['m'] = 'Ｍ',
        ['N'] = 'Ｎ', ['n'] = 'Ｎ',
        ['O'] = 'Ｏ', ['o'] = 'Ｏ',
        ['P'] = 'Ｐ', ['p'] = 'Ｐ',
        ['Q'] = 'Ｑ', ['q'] = 'Ｑ',
        ['R'] = 'Ｒ', ['r'] = 'Ｒ',
        ['S'] = 'Ｓ', ['s'] = 'Ｓ',
        ['T'] = 'Ｔ', ['t'] = 'Ｔ',
        ['U'] = 'Ｕ', ['u'] = 'Ｕ',
        ['V'] = 'Ｖ', ['v'] = 'Ｖ',
        ['W'] = 'Ｗ', ['w'] = 'Ｗ',
        ['X'] = 'Ｘ', ['x'] = 'Ｘ',
        ['Y'] = 'Ｙ', ['y'] = 'Ｙ',
        ['Z'] = 'Ｚ', ['z'] = 'Ｚ',
        
        ['1'] = '１', 
        ['2'] = '２', 
        ['3'] = '３', 
        ['4'] = '４', 
        ['5'] = '５', 
        ['6'] = '６', 
        ['7'] = '７', 
        ['8'] = '８', 
        ['9'] = '９', 
        ['0'] = '０',
    };
}