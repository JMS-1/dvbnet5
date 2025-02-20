﻿using System.Text;

namespace JMS.DVB.EPG.BBC;

/// <summary>
/// Meldet das Ergebnis einer Dekomprimierung.
/// </summary>
public class DecodingReportArgs : EventArgs
{
    /// <summary>
    /// Die komprimierten Werte.
    /// </summary>
    public byte[] CompressedData { get; set; } = null!;

    /// <summary>
    /// Bisher dekomprimierte Zeichen.
    /// </summary>
    public string Uncompressed { get; set; } = null!;

    /// <summary>
    /// Die verwendete Dekomprimierungstabelle.
    /// </summary>
    public int CodePage { get; set; }

    /// <summary>
    /// <see cref="CompressedData"/> in der Textrepräsentation der
    /// binären Form.
    /// </summary>
    private string m_Sequence = null!;

    /// <summary>
    /// Erzeugt eine neue Informationsinstanz.
    /// </summary>
    public DecodingReportArgs()
    {
    }

    /// <summary>
    /// Ermittelt die komprimierten Daten aus <see cref="CompressedData"/>
    /// als Bitfolge.
    /// </summary>
    public string Sequence
    {
        get
        {
            // Create once
            if (null == m_Sequence)
            {
                // Reset
                StringBuilder binDump = new();

                // Fill
                foreach (var hex in CompressedData)
                    for (int h = 8, r = hex; h-- > 0; r *= 2)
                        binDump.Append((0 == (r & 0x80)) ? '0' : '1');

                // Remember
                m_Sequence = binDump.ToString();
            }

            // Report
            return m_Sequence;
        }
    }
}

/// <summary>
/// Parameter zur Information über eine nicht erfolgte Auflösung.
/// </summary>
public class DecodingFailureArgs : DecodingReportArgs
{
    /// <summary>
    /// Die aktuelle Position bei der Auswertung der Daten.
    /// </summary>
    public int BitPosition { get; set; }

    /// <summary>
    /// Erzeugt eine neue Informationsinstanz.
    /// </summary>
    public DecodingFailureArgs()
    {
    }
}

/// <summary>
/// Klasse zum Dekodieren von komprimierten Textsequenzen.
/// </summary>
public class TextDecoder : Encoding, Section.ICustomEncoder
{
    /// <summary>
    /// Die vollen Informationen für die Komprimerungstabellen.
    /// </summary>
    private static readonly HuffmanPairTable[] CodePages = new HuffmanPairTable[2];

    /// <summary>
    /// Die Tabelle für die erste Komprimierungsart.
    /// </summary>
    private static readonly ushort[][] BinaryTables;

    /// <summary>
    /// Liest die <i>Huffman</i> Tabellen aus den Ressourcen.
    /// </summary>
    static TextDecoder()
    {
        // Create binary representation helper
        BinaryTables = new ushort[CodePages.Length][];

        // Attach to self
        var me = typeof(TextDecoder);

        // Load all
        for (var i = CodePages.Length; i > 0; --i)
            try
            {
                // Attach to resource
                using var stream = me.Assembly.GetManifestResourceStream(string.Format("{0}.BBC.CodePage{1}.xht", me.Assembly.ManifestModule.Name[..^4], i));

                LoadTable(i, HuffmanPairTable.Load(stream!));
            }
            catch
            {
                // Ignore any error - just do not decompress
            }

        // Check for overloads
        DynamicLoadTables();
    }

    /// <summary>
    /// Prüft, ob im Verzeichnis der Anwendung Dekomprimierungstabellen zu finden sind und
    /// verwendet diese dann.
    /// </summary>
    /// <returns>Liste aller erfolgreich geladenen Tabellen.</returns>
    public static int[] DynamicLoadTables()
    {
        // Reset
        List<int> tables = [];

        // Get the directory
        DirectoryInfo tableDirectory = RunTimeLoader.GetDirectory("Huffman Tables");

        // Check files
        for (int i = 0; i++ < CodePages.Length;)
            try
            {
                // Attach to file
                FileInfo table = new(Path.Combine(tableDirectory.FullName, string.Format("CodePage{0}.xht", i)));

                // Load it
                if (table.Exists)
                {
                    // Process
                    LoadTable(i, HuffmanPairTable.Load(table.FullName));

                    // Remember
                    tables.Add(i);
                }
            }
            catch
            {
                // Ignore any error
            }

        // Report
        return [.. tables];
    }

    /// <summary>
    /// Meldet eine Dekomprimierungstabelle.
    /// </summary>
    /// <param name="codepage">Die gewünschte Komprimierungsart.</param>
    /// <returns>Die angeforderte Table oder <i>null</i>, wenn keine gültig Tabelle aktiv ist.</returns>
    public static HuffmanPairTable GetTable(int codepage)
    {
        // Validate
        ArgumentOutOfRangeException.ThrowIfLessThan(codepage, 1);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(codepage, 2);

        // Report
        return CodePages[codepage - 1];
    }

    /// <summary>
    /// Assoziiert eine Dekomprimierungstabelle.
    /// </summary>
    /// <param name="codepage">Die betroffene Komprimierungsart.</param>
    /// <param name="table">Die zu verwendende Tabelle</param>
    /// <exception cref="ArgumentNullException">Es wurde keine Tabelle angegeben.</exception>
    /// <exception cref="ArgumentException">Die Komprimierungsart ist unzulässig.</exception>
    public static void LoadTable(int codepage, HuffmanPairTable table)
    {
        // Validate
        ArgumentNullException.ThrowIfNull(table);

        ArgumentOutOfRangeException.ThrowIfLessThan(codepage, 1);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(codepage, 2);

        // Try to compile the table
        var compiled = table.CreateBinary().GetTable();

        // Remember the full table information
        CodePages[codepage - 1] = table;

        // Remember the binary representation
        BinaryTables[codepage - 1] = compiled;
    }

    /// <summary>
    /// Meldet die Dekodierung im DVB.NET 4.0 EPG Parser an.
    /// </summary>
    internal static void RegisterWithEPG() => Section.RegisterEncoding(0x1f, new TextDecoder());

    /// <summary>
    /// Wird aufgerufen, wenn eine Dekompromierung fehlgeschlagen ist.
    /// </summary>
    public static event EventHandler<DecodingFailureArgs>? OnFailure;

    /// <summary>
    /// Wird aufgerufen, wenn eine Decomprimierung erfolgreich war.
    /// </summary>
    public static event EventHandler<DecodingReportArgs>? OnSuccess;

    /// <summary>
    /// Meldet die maximale Anzahl von verschlüsselten Bytes für einen Zeichenkette
    /// fester Länge.
    /// </summary>
    /// <param name="charCount">Die Länge der Zeichenkette.</param>
    /// <returns>Die maximale Anzahl der benötigten Bytes.</returns>
    public override int GetMaxByteCount(int charCount) => throw new NotImplementedException();

    /// <summary>
    /// Meldet die maximale Anzahl von Zeichen, die aus einer komprimierten
    /// Sequenz entstehen kann.
    /// </summary>
    /// <param name="byteCount">Die Anzahl der komprimierten Bytes.</param>
    /// <returns>Die maximale Anzahl von Zeichen.</returns>
    public override int GetMaxCharCount(int byteCount) => throw new NotImplementedException();

    /// <summary>
    /// Komprimiert eine Zeichensequenz.
    /// </summary>
    /// <param name="bytes">Die komprimierten Daten.</param>
    /// <param name="byteIndex">Laufende Nummer des ersten zu nutzenden Wertes.</param>
    /// <param name="byteCount">Anzahl der zu nutzenden Werte.</param>
    /// <param name="chars">Resultierende Zeichenkette.</param>
    /// <param name="charIndex">Laufende Nummer des ersten Zeichens in der resultierenden Zeichenkette.</param>
    /// <returns>Die Anzahl der dekomprimierten Zeichen.</returns>
    public override int GetChars(byte[] bytes, int byteIndex, int byteCount, char[] chars, int charIndex) => throw new NotImplementedException();

    /// <summary>
    /// Meldet die Anzahl der Zeichen, die bei einer Dekomprimierung entstehen würden.
    /// </summary>
    /// <param name="bytes">Die komprimierten Daten.</param>
    /// <param name="index">Laufende Nummer des ersten zu nutzenden Wertes.</param>
    /// <param name="count">Anzahl der zu nutzenden Werte.</param>
    /// <returns>Die Anzahl der dekomprimierten Zeichen.</returns>
    public override int GetCharCount(byte[] bytes, int index, int count) => throw new NotImplementedException();

    /// <summary>
    /// Komprimiert eine Zeichenkette.
    /// </summary>
    /// <param name="chars">Die Zeichenkette.</param>
    /// <param name="charIndex">Die laufende Nummer des ersten zu komprimierenden Zeichens.</param>
    /// <param name="charCount">Die Anzahl der zu komprimierenden Zeichen.</param>
    /// <param name="bytes">Ergebnisfeld für die komprimierten Werte.</param>
    /// <param name="byteIndex">Die laufende Nummer des ersten Wertes im Ergebnisfeld.</param>
    /// <returns>Die Anzahl der erzeugten Werte.</returns>
    public override int GetBytes(char[] chars, int charIndex, int charCount, byte[] bytes, int byteIndex) => throw new NotImplementedException();

    /// <summary>
    /// Ermittelt, wieviele Werte für eine Komprimierung einer Zeichenkette notwendig sind.
    /// </summary>
    /// <param name="chars">Die Zeichenkette.</param>
    /// <param name="index">Die laufende Nummer des ersten zu komprimierenden Zeichens.</param>
    /// <param name="count">Die Anzahl der zu komprimierenden Zeichen.</param>
    /// <returns>Die Anzahl der benötigten Werte.</returns>
    public override int GetByteCount(char[] chars, int index, int count) => throw new NotImplementedException();

    /// <summary>
    /// Dekomprimiert eine Zeichenkette.
    /// </summary>
    /// <param name="bytes">Feld mit den dekomprimierten Werten.</param>
    /// <param name="index">Laufende Nummer des ersten zu dekomprimierenden Wertes.</param>
    /// <param name="count">Die Anzahl der zur Dekomprimierung zu verwenden Werte.</param>
    /// <returns>Die dekomprimierte Zeichenketten - bei Fehlern wird ein <i>...</i> angehängt.</returns>
    /// <exception cref="ArgumentException">Mindestens ein Parameter ist unzulässig.</exception>
    public override string GetString(byte[] bytes, int index, int count)
    {
        // Validate
        if (count < 1)
            return string.Empty;

        // Get the table
        ushort[] table = null!;
        if (1 == bytes[index])
            table = BinaryTables[0];
        else if (2 == bytes[index])
            table = BinaryTables[1];

        // Not table
        if (table == null)
            return Section.DefaultEncoding.GetString(bytes, index, count);

        // Advance index
        ++index;
        --count;

        // Nothing in it
        if (count < 1)
            return string.Empty;

        // Remember
        int initialIndex = index, initialCount = count;

        // Constructor
        List<byte> builder = [];

        // Set current processing index
        var tableIndex = table[0];

        // Current index shift
        var indexShift = 0;

        // Previous pattern and width
        uint? previous = null;

        // Last checkpoint
        var lastCharPosition = 0;

        // Process
        for (; ; )
        {
            // Check for regular stop
            if (tableIndex == 0)
            {
                // Load the initial check pattern
                var check = (byte)(0xff >> indexShift);

                // All bytes must be zero
                for (; count-- > 0; check = 0xff)
                    if ((bytes[index++] & check) != 0)
                        break;

                // Found something bad
                if (count >= 0)
                    break;

                // Load result
                var decompressed = UTF8.GetString([.. builder]);

                // Load client
                var successHandler = OnSuccess;

                // Report
                if (successHandler != null)
                {
                    // Create full data
                    var compressed = new byte[initialCount];

                    // Fill it
                    Array.Copy(bytes, initialIndex, compressed, 0, compressed.Length);

                    // Report
                    successHandler(this, new()
                    {
                        CodePage = (table == BinaryTables[0]) ? 1 : 2,
                        Uncompressed = decompressed,
                        CompressedData = compressed
                    });
                }

                // Report
                return decompressed;
            }

            // Unexpected end of input
            if (count < 1)
                break;

            // Check for leaf
            if (tableIndex <= 127)
            {
                // At this position
                lastCharPosition = 8 * (index - initialIndex) + indexShift;

                // Append
                builder.Add((byte)('\x0001' + tableIndex - 1));

                // Restart
                tableIndex = table[tableIndex];
            }

            // In error
            if (tableIndex == TableCreator.ErrorIndicator)
                break;

            // Check for escape mode
            var inEscape = tableIndex == TableCreator.EscapeIndicator;

            // Get the number of bits to process
            var bitWidth = inEscape ? (ushort)8 : table[tableIndex++];

            // Check limitation
            if (bitWidth > 24)
                break;

            // Not possible
            if (bitWidth > 8 * count)
                break;

            // The pattern
            uint pattern;

            // Quick load pattern
            if (previous.HasValue)
            {
                // Use
                pattern = previous.Value;
            }
            else
            {
                // The pattern
                pattern = bytes[index + 0];

                // Append 
                pattern <<= 8;
                if (count > 1)
                    pattern |= bytes[index + 1];

                // Append 
                pattern <<= 8;
                if (count > 2)
                    pattern |= bytes[index + 2];

                // Append 
                pattern <<= 8;
                if (count > 3)
                    pattern |= bytes[index + 3];

                // Remember
                previous = pattern;
            }

            // Correct it
            pattern <<= indexShift;

            // Adjust shift
            indexShift += bitWidth;

            // Adjust index
            if (indexShift >= 8)
            {
                // Shift bytes
                int bytesEaten = indexShift / 8;

                // Adjust all
                index += bytesEaten;
                count -= bytesEaten;
                indexShift %= 8;

                // Clear cache
                previous = null;
            }

            // Check for escape mode
            if (inEscape)
            {
                // At this position
                lastCharPosition = 8 * (index - initialIndex) + indexShift;

                // Get character
                pattern >>= 24;

                // See if this is it
                if (pattern < 128)
                    if (0 == pattern)
                        tableIndex = 0;
                    else
                        tableIndex = (ushort)(1 + (pattern - 1));
                else
                    builder.Add((byte)pattern);

                // Next
                continue;
            }

            // Get the first link position
            var linkIndex = table[tableIndex++];

            // Translate the pattern
            var patternIndex = pattern >> (32 - bitWidth);

            // Check mode
            if (linkIndex == TableCreator.EndIndicator)
            {
                // Attach to the entry
                tableIndex = table[tableIndex + patternIndex];
            }
            else
            {
                // Try to find
                do
                {
                    // Test pattern - upper half
                    uint test = table[linkIndex - 4];

                    // Test pattern - lower half
                    test <<= 16;
                    test |= table[linkIndex - 3];

                    // Next table
                    tableIndex = table[linkIndex - 2];

                    // Did it
                    if (test == patternIndex)
                        break;

                    // Advance
                    linkIndex = table[linkIndex - 1];
                }
                while (linkIndex != TableCreator.EndIndicator);

                // In error
                if (linkIndex == TableCreator.EndIndicator)
                    break;
            }
        }

        // Load client
        var handler = OnFailure;

        // Report
        if (handler != null)
        {
            // Create full data
            var compressed = new byte[initialCount];

            // Fill it
            Array.Copy(bytes, initialIndex, compressed, 0, compressed.Length);

            // Report
            handler(this, new()
            {
                Uncompressed = UTF8.GetString([.. builder]),
                CodePage = (table == BinaryTables[0]) ? 1 : 2,
                BitPosition = lastCharPosition,
                CompressedData = compressed
            });
        }

        // Report
        return UTF8.GetString([.. builder]) + "....";
    }
}
