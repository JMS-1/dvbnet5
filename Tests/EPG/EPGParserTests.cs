using JMS.DVB.EPG;
using JMS.DVB.EPG.Descriptors;
using JMS.DVB.EPG.Tables;

namespace DVBNETTests.EPG;

[TestFixture]
public class EPGParserTests
{
    class ParseFromTestData : IDisposable
    {
        private readonly byte[] m_buffer = new byte[100000];

        private readonly Stream m_stream;

        private readonly Parser m_parser = new();

        public readonly List<EIT> Tables = [];

        public ParseFromTestData(string name)
        {
            m_stream = new FileStream(Path.Combine($"TestData/{name}.bin"), FileMode.Open, FileAccess.Read, FileShare.ReadWrite, m_buffer.Length);

            m_parser.SectionFound += SectionFound;
        }

        private void SectionFound(Section section)
        {
            if ((section == null) || !section.IsValid) return;

            if (section.Table is EIT table && table.IsValid)
                Tables.Add(table);
        }

        public void Dispose()
        {
            m_stream.Dispose();
        }

        public void Process()
        {
            for (int n; (n = m_stream.Read(m_buffer, 0, m_buffer.Length)) > 0;)
                m_parser.OnData(m_buffer, 0, n);
        }
    }

    [Test]
    public void Can_Parse_German_EPG()
    {
        using var parser = new ParseFromTestData("epg");

        parser.Process();

        Assert.That(parser.Tables, Has.Count.EqualTo(336));
        Assert.That(((ShortEvent)parser.Tables[0].Entries[0].Descriptors[0]).Text, Is.EqualTo("Jingle Bells - Eine Familie zum Fest Komödie, USA 2004 Altersfreigabe: Ohne Altersbeschränkung (WH vom Dienstag, 26.12.2023, 22:15 Uhr)"));
    }

    [Test]
    public void Can_Parse_UK_EPG()
    {
        using var parser = new ParseFromTestData("epg-uk");

        parser.Process();

        Assert.That(parser.Tables, Has.Count.EqualTo(61169));
        Assert.That(((ShortEvent)parser.Tables[49].Entries[1].Descriptors[2]).Text, Is.EqualTo("Latest news from the world of entertainment. [HD]"));
    }

    [Test]
    public void Can_Parse_UK_EPG_2()
    {
        using var parser = new ParseFromTestData("epg-uk-2");

        parser.Process();

        Assert.That(parser.Tables, Has.Count.EqualTo(38856));

        var shortEvent = (ShortEvent)parser.Tables[356].Entries[3].Descriptors[1];

        Assert.That(shortEvent.Name, Is.EqualTo("Crùnluath"));

        foreach (var row in parser.Tables)
            foreach (var ent in row.Entries)
                foreach (var descr in ent.Descriptors)
                    if (descr is ShortEvent se)
                        Console.WriteLine(se.Name);
    }
}
