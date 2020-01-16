<Query Kind="Program" />

void Main()
{
	var fontConverter = new FontConverter("test_16x16")
	{
		SymbolWidth = 16,
		SymbolHeight = 16,
		FirstSymbolCode = 0,
		LastSymbolCode = 1
	};
	fontConverter.Convert();
}

class FontConverter
{
	private readonly string _fontName;

	public FontConverter(string fontName)
	{
		_fontName = fontName;
	}
	
	public int SymbolWidth { get; set; } = 8;
	public int SymbolHeight { get; set; } = 8;
	public byte FirstSymbolCode { get; set; } = 32;
	public byte LastSymbolCode { get; set; } = 42;
	public string SrcDir { get; set; } = "g:/VOLUME/SOFT/Font editors/SG Bitmap Font Editor/fonts";
	public string DstDir { get; set; } = "g:/VOLUME/PROGRAMMING/smarthome/mh-z19b/src/fonts";
	
	public void Convert()
	{
		int wb = System.Convert.ToInt32(Math.Ceiling(SymbolWidth / 8.0));

		byte[] bytes = File.ReadAllBytes($"{SrcDir}/{_fontName}");

		var result = $"const uint8_t {_fontName}Bitmaps[] PROGMEM = {{\n";
		for (int code = FirstSymbolCode; code <= LastSymbolCode; code++)
		{
			var v = SymbolHeight * wb;
			result += "    ";
			for (int j = v * code; j < v * (code + 1); j++)
			{
				result += ToHexString(bytes[j]) + ",";
			}
			result += $" // '{ System.Convert.ToChar(code) }'\n";
			result += SymbolPic(code * v, bytes);
		}
		result += "};\n";

		result += $"const GFXglyph {_fontName}Glyphs[] PROGMEM = {{\n";
		result += "// bitmapOffset, width, height, xAdvance, xOffset, yOffset\n";
		for (int code = FirstSymbolCode; code <= LastSymbolCode; code++)
		{
			int offset = (code - FirstSymbolCode) * SymbolHeight * wb;
			int width = GetSymbolWidth(code);
			int height = SymbolHeight;
			char symbol = System.Convert.ToChar(code);
			result += $"    {{ {offset,5}, {width,4}, {height,4}, {SymbolWidth+2,4}, {0,4}, {SymbolHeight*(-1),4} }}, // '{ symbol }'\n";			
		}
		result += "};\n";

		result += $"const GFXfont {_fontName} PROGMEM = {{ (uint8_t*){_fontName}Bitmaps, (GFXglyph*){_fontName}Glyphs, {ToHexString(FirstSymbolCode)}, {ToHexString(LastSymbolCode)}, {SymbolHeight} }};";

		Console.WriteLine(result);
		File.WriteAllText($"{DstDir}/{_fontName}.h", result);

	}

	private string ToHexString(int code)
	{
		return string.Format($"0x{code:X2}");
	}

	private int GetSymbolWidth(int code)
	{
		//if (new[] { 32, 46 }.Any(c => c == code)) return 2;
		return SymbolWidth;
	}

	string SymbolPic(int bitmapOffset, byte[] bytes)
	{
		var sb = new StringBuilder();
		int w = SymbolWidth;
		int h = SymbolHeight;
		for (int yy = 0; yy < h; yy++)
		{
			sb.Append("//  ");
			byte bit = 0;
			byte bits = 0;
			for (int xx = 0; xx < w; xx++)
			{
				if (bit == 0)
				{
					bits = bytes[bitmapOffset++];
					bit = 0x80;
				}
				if ((bit & bits) > 0)
					sb.Append('X');
				else
					sb.Append('*');
				bit >>= 1;
			}
			sb.Append('\n');
		}
		return sb.ToString();
	}
}