using System;
using System.IO;
using System.IO.Compression;
using System.Xml;
using UnityEngine;

public static class ReportSaver
{
    public static string Save(ReportData report, string originalPath)
    {
        string savePath = Path.Combine(
            Application.persistentDataPath,
            "Report_" + DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".xlsx"
        );

        File.Copy(originalPath, savePath, overwrite: true);

        using var archive = ZipFile.Open(savePath, ZipArchiveMode.Update);
        var sheetEntry = archive.GetEntry("xl/worksheets/sheet1.xml");
        if (sheetEntry == null)
        {
            Debug.LogError("sheet1.xml non trovato nel file salvato");
            return savePath;
        }

        // Leggi XML corrente
        string xmlContent;
        using (var stream = sheetEntry.Open())
        using (var reader = new StreamReader(stream))
            xmlContent = reader.ReadToEnd();

        var doc = new XmlDocument();
        doc.LoadXml(xmlContent);

        var ns = new XmlNamespaceManager(doc.NameTable);
        ns.AddNamespace("x", "http://schemas.openxmlformats.org/spreadsheetml/2006/main");

        // Aggiorna celle
        int row = 7;
        foreach (var defect in report.Rows)
        {
            SetCell(doc, ns, row, 5, defect.Visto ? "1" : "0", "b");
            SetCell(doc, ns, row, 7, defect.Estensione02 ? "1" : "0", "b");
            SetCell(doc, ns, row, 8, defect.Estensione05 ? "1" : "0", "b");
            SetCell(doc, ns, row, 9, defect.Estensione1 ? "1" : "0", "b");
            SetCell(doc, ns, row, 10, defect.Intensita02 ? "1" : "0", "b");
            SetCell(doc, ns, row, 11, defect.Intensita05 ? "1" : "0", "b");
            SetCell(doc, ns, row, 12, defect.Intensita1 ? "1" : "0", "b");
            SetCell(doc, ns, row, 13, defect.PS ? "1" : "0", "b");
            SetCell(doc, ns, row, 14, defect.NA ? "1" : "0", "b");
            SetCell(doc, ns, row, 15, defect.NR ? "1" : "0", "b");
            SetCell(doc, ns, row, 16, defect.NP ? "1" : "0", "b");
            SetCell(doc, ns, row, 17, defect.FOTO ? "1" : "0", "b");
            if (!string.IsNullOrEmpty(defect.Note))
                SetCell(doc, ns, row, 18, defect.Note, "str");
            row++;
        }

        // Sovrascrivi XML nel file
        sheetEntry.Delete();
        var newEntry = archive.CreateEntry("xl/worksheets/sheet1.xml");
        using (var stream = newEntry.Open())
        using (var writer = new StreamWriter(stream))
            writer.Write(doc.OuterXml);

        return savePath;
    }

    private static void SetCell(XmlDocument doc, XmlNamespaceManager ns,
        int row, int col, string value, string type)
    {
        string cellRef = ColName(col) + row;
        var cell = doc.SelectSingleNode($"//x:c[@r='{cellRef}']", ns);

        if (cell == null) return;

        // Imposta tipo
        if (cell.Attributes["t"] == null)
        {
            var attr = doc.CreateAttribute("t");
            cell.Attributes.Append(attr);
        }
        cell.Attributes["t"].Value = type;

        var vNode = cell.SelectSingleNode("x:v", ns);
        if (vNode == null)
        {
            vNode = doc.CreateElement("v", "http://schemas.openxmlformats.org/spreadsheetml/2006/main");
            cell.AppendChild(vNode);
        }
        vNode.InnerText = value;
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