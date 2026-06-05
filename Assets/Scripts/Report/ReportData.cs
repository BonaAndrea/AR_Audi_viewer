using System.Collections.Generic;

[System.Serializable]
public class ReportData
{
    public string AppCode;
    public string Codice;
    public string Marca;
    public string Veicolo;
    public string Tecnico1;
    public string Tecnico2;
    public string Data;

    public List<DefectRow> Rows = new();
}

[System.Serializable]
public class DefectRow
{
    public string CodDifetto;
    public string Elem;
    public string Descrizione;

    public bool Visto;
    public bool Estensione02;
    public bool Estensione05;
    public bool Estensione1;
    public bool Intensita02;
    public bool Intensita05;
    public bool Intensita1;
    public bool PS;
    public bool NA;
    public bool NR;
    public bool NP;
    public bool FOTO;
    public string Note;

    public int G;
}