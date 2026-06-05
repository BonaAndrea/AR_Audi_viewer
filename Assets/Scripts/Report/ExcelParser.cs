using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Xml;
using UnityEngine;

public static class ExcelParser
{
    public static ReportData Parse(string filePath)
    {
        var report = new ReportData();
        var sharedStrings = new List<string>();
        var cellValues = new Dictionary<string, string>();

        using var archive = ZipFile.OpenRead(filePath);

        // Leggi shared strings
        var ssEntry = archive.GetEntry("xl/sharedStrings.xml");
        if (ssEntry != null)
        {
            using var stream = ssEntry.Open();
            var doc = new XmlDocument();
            doc.Load(stream);
            var nodes = doc.GetElementsByTagName("t");
            foreach (XmlNode node in nodes)
                sharedStrings.Add(node.InnerText);
        }

        // Leggi sheet1
        var sheetEntry = archive.GetEntry("xl/worksheets/sheet1.xml");
        if (sheetEntry == null)
        {
            Debug.LogError("sheet1.xml non trovato");
            return report;
        }

        using var sheetStream = sheetEntry.Open();
        var sheetDoc = new XmlDocument();
        sheetDoc.Load(sheetStream);

        var ns = new XmlNamespaceManager(sheetDoc.NameTable);
        ns.AddNamespace("x", "http://schemas.openxmlformats.org/spreadsheetml/2006/main");

        var rows = sheetDoc.SelectNodes("//x:row", ns);
        foreach (XmlNode row in rows)
        {
            var rowIndex = int.Parse(row.Attributes["r"].Value);
            var cells = row.SelectNodes("x:c", ns);
            foreach (XmlNode cell in cells)
            {
                string cellRef = cell.Attributes["r"].Value;
                string type = cell.Attributes["t"]?.Value ?? "";
                var vNode = cell.SelectSingleNode("x:v", ns);
                if (vNode == null) continue;

                string value;
                if (type == "s")
                    value = int.TryParse(vNode.InnerText, out int idx) && idx < sharedStrings.Count
                        ? sharedStrings[idx] : "";
                else if (type == "b")
                    value = vNode.InnerText == "1" ? "TRUE" : "FALSE";
                else
                    value = vNode.InnerText;

                cellValues[cellRef] = value;
            }
        }

        // Helper
        string Get(int row, int col) =>
            cellValues.TryGetValue(ColName(col) + row, out var v) ? v : "";

        bool GetBool(int row, int col) =>
            Get(row, col).ToUpper() == "TRUE";

        // Header
        report.AppCode = Get(1, 3);
        report.Codice = Get(2, 4);
        report.Marca = Get(2, 5);
        report.Veicolo = Get(2, 7);
        report.Tecnico1 = Get(2, 10);
        report.Tecnico2 = Get(3, 10);
        report.Data = Get(3, 18);

        // Righe difetti dalla riga 7
        for (int r = 7; r <= 100; r++)
        {
            string codice = Get(r, 2);
            string descrizione = Get(r, 4);

            if (string.IsNullOrEmpty(codice) && string.IsNullOrEmpty(descrizione)) continue;
            if (descrizione.StartsWith("Note")) break;

            var defect = new DefectRow
            {
                CodDifetto = codice,
                Elem = Get(r, 3),
                Descrizione = descrizione,
                Visto = GetBool(r, 5),
                G = int.TryParse(Get(r, 6), out int g) ? g : 0,
                Estensione02 = GetBool(r, 7),
                Estensione05 = GetBool(r, 8),
                Estensione1 = GetBool(r, 9),
                Intensita02 = GetBool(r, 10),
                Intensita05 = GetBool(r, 11),
                Intensita1 = GetBool(r, 12),
                PS = GetBool(r, 13),
                NA = GetBool(r, 14),
                NR = GetBool(r, 15),
                NP = GetBool(r, 16),
                FOTO = GetBool(r, 17),
                Note = Get(r, 18)
            };

            report.Rows.Add(defect);
        }

        return report;
    }

    private static string ColName(int col)
    {
        string name = "";
        while (col > 0)
        {
            col--;
            name = (char)('A' + col % 26) + name;
            col /= 26;
        }
        return name;
    }
}