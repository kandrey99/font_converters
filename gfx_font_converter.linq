<Query Kind="Program" />

void Main()
{
	var fontConverter = new FontConverter("test_16x16")
	{
		SymbolWidth = 16,
		SymbolHeight = 16,
		XAdvance = 20,
		FirstSymbolCode = 128,
		LastSymbolCode = 129,
		DstDir = @"G:\Screencast\gfx_symbol_test\gfx_symbol_test\src\fonts"
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
	public int XAdvance { get; set; } = 10;
	public byte FirstSymbolCode { get; set; } = 32;
	public byte LastSymbolCode { get; set; } = 42;
	public string SrcDir { get; set; } = @"g:\VOLUME\SOFT\Font editors\SG Bitmap Font Editor\fonts";
	public string DstDir { get; set; } = @"g:\VOLUME\PROGRAMMING\smarthome\mh-z19b\src\fonts";
	
	public void Convert()
	{
		int wb = System.Convert.ToInt32(Math.Ceiling(SymbolWidth / 8.0));

		byte[] bytes = File.ReadAllBytes($"{SrcDir}/{_fontName}");

		var result = new StringBuilder($"const uint8_t {_fontName}Bitmaps[] PROGMEM = {{\n");
		for (int code = FirstSymbolCode; code <= LastSymbolCode; code++)
		{
			result.Append("    ");
			var v = SymbolHeight * wb;
			for (int j = v * code; j < v * (code + 1); j++)
			{
				result.Append(ToHexString(bytes[j]) + ",");
			}
			result.Append($" // '{ System.Convert.ToChar(code) }'\n");
			result.Append(SymbolPic(v * code, bytes));
		}
		result.Append("};\n");

		result.Append($"const GFXglyph {_fontName}Glyphs[] PROGMEM = {{\n");
		result.Append("// bitmapOffset, width, height, xAdvance, xOffset, yOffset\n");
		for (int code = FirstSymbolCode; code <= LastSymbolCode; code++)
		{
			int offset = (code - FirstSymbolCode) * SymbolHeight * wb;
			int width = GetSymbolWidth(code);
			int height = SymbolHeight;
			char symbol = System.Convert.ToChar(code);
			result.Append($"    {{ {offset,5}, {width,4}, {height,4}, {XAdvance,4}, {0,4}, {SymbolHeight*(-1),4} }}, // '{ symbol }'\n");
		}
		result.Append("};\n");

		result.Append($"const GFXfont {_fontName} PROGMEM = {{ (uint8_t*){_fontName}Bitmaps, (GFXglyph*){_fontName}Glyphs, {ToHexString(FirstSymbolCode)}, {ToHexString(LastSymbolCode)}, {SymbolHeight} }};");

		Console.WriteLine(result.ToString());
		File.WriteAllText($"{DstDir}/{_fontName}.h", result.ToString());
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