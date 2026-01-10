using MigraDoc.DocumentObjectModel;
using MigraDoc.DocumentObjectModel.Tables;
using MigraDoc.Rendering;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Media;

var sfd = new Microsoft.Win32.SaveFileDialog
{
    Filter = "PDF File|*.pdf",
    Title = "Save DataGrid as PDF"
};
if (sfd.ShowDialog() == true)
{
    var columns = new List<(string, string, double, TextAlignment, string, Func<object, SolidColorBrush>, Func<object, SolidColorBrush>)>
    {
        ("ID", "Id", 50, TextAlignment.Left,null, null, (o)=>Brushes.Blue),
        ("Name", "Name", 150, TextAlignment.Left,null, cellBackground, cellForeround),
        ("CountryCode", "Country code", 100, TextAlignment.Left,null, null, null),
        ("Population","Population", 100, TextAlignment.Right,"N0", null, null),
        ("District", "District", 150, TextAlignment.Left,null, null, null)
    };
    if (ExportToPdfManual(Cities, columns, sfd.FileName, rowBackground))
    {
        Process.Start(new ProcessStartInfo
        {
            FileName = sfd.FileName,
            UseShellExecute = true
        });
    }

}

private SolidColorBrush rowBackground(object obj)
{
    if (obj is City city && city.IsMoreThanMillion)
        return Brushes.LightSalmon;
    return Brushes.Transparent;
}

private SolidColorBrush cellBackground(object obj)
{
    if (obj is City city && city.IsMoreThanMillion)
        return Brushes.Green;
    return Brushes.Transparent;
}

private SolidColorBrush cellForeround(object obj)
{
    if (obj is City city && city.IsMoreThanMillion)
        return Brushes.White;
    return Brushes.Black;
}

private ParagraphAlignment getParagrphAlignment(TextAlignment alignment = TextAlignment.Left)
{
    return alignment switch
    {
        TextAlignment.Left => ParagraphAlignment.Left,
        TextAlignment.Center => ParagraphAlignment.Center,
        TextAlignment.Right => ParagraphAlignment.Right,
        TextAlignment.Justify => ParagraphAlignment.Justify,
        _ => ParagraphAlignment.Left,
    };
}

private MigraDoc.DocumentObjectModel.Color getMigraDocColor(System.Windows.Media.SolidColorBrush brush) => brush == null ? MigraDoc.DocumentObjectModel.Colors.Transparent : MigraDoc.DocumentObjectModel.Color.FromArgb(brush.Color.A, brush.Color.R, brush.Color.G, brush.Color.B);
private bool ExportToPdfManual(IEnumerable<object> items, List<(string, string, double, TextAlignment, string, Func<object, SolidColorBrush>, Func<object, SolidColorBrush>)> columns, string pdfPath, Func<object, SolidColorBrush> rowBackgroundFunc = null)
{
    try
    {
        var document = new Document();
        var section = document.AddSection();

        // === This is the key sequence that usually fixes it ===
        PageSetup ps = document.DefaultPageSetup.Clone();
        ps.ResetPageSize();                    // ← Prevents old/fixed PageWidth/Height from blocking the change

        ps.PageFormat = PageFormat.A4;         // or A3, Letter, etc.
        ps.Orientation = Orientation.Landscape;

        // Optional: tighter margins for extra width
        ps.LeftMargin = Unit.FromCentimeter(1.2);
        ps.RightMargin = Unit.FromCentimeter(1.2);
        ps.TopMargin = Unit.FromCentimeter(1.5);
        ps.BottomMargin = Unit.FromCentimeter(1.5);

        // Assign the prepared PageSetup to the section
        section.PageSetup = ps;

        var table = section.AddTable();
        table.Borders.Visible = true;
        // At table creation time (once)
        table.LeftPadding = Unit.FromMillimeter(1.5);
        table.RightPadding = Unit.FromMillimeter(1.5);
        table.TopPadding = Unit.FromMillimeter(1);
        table.BottomPadding = Unit.FromMillimeter(1);

        table.Borders.Color = MigraDoc.DocumentObjectModel.Colors.DimGray;
        table.Borders.Width = Unit.FromPoint(0.5);

        foreach (var col in columns)
        {
            table.AddColumn(Unit.FromPoint(col.Item3));
        }

        // Header
        var header = table.AddRow();
        header.Shading.Color = MigraDoc.DocumentObjectModel.Colors.LightGray;
        for (int i = 0; i < columns.Count; i++)
        {
            header.Cells[i].AddParagraph(columns[i].Item2);
            header.Cells[i].Format.Alignment = ParagraphAlignment.Center;
        }
        foreach (Cell cell in header.Cells)
        {
            cell.Format.Font.Bold = true;
            cell.Format.Font.Color = MigraDoc.DocumentObjectModel.Colors.Black;
        }

        foreach (var item in items)
        {
            var row = table.AddRow();

            row.Height = Unit.FromMillimeter(6);      // or whatever looks good
            row.HeightRule = RowHeightRule.AtLeast;   // allows content to grow if needed

            row.Shading.Color = rowBackgroundFunc == null
                ? MigraDoc.DocumentObjectModel.Colors.Transparent
                : getMigraDocColor(rowBackgroundFunc(item));

            for (var i = 0; i < columns.Count; i++)
            {
                var column = columns[i];
                var prop = item.GetType().GetProperty(columns[i].Item1);
                if (prop == null) continue;

                var value = prop.GetValue(item);
                var text = value?.ToString() ?? string.Empty;

                MigraDoc.DocumentObjectModel.Paragraph p;
                if (decimal.TryParse(text, out var decValue) && !string.IsNullOrEmpty(column.Item5))
                    p = row.Cells[i].AddParagraph(decValue.ToString(column.Item5));
                else if (DateTime.TryParse(text, out var dateValue) && !string.IsNullOrEmpty(column.Item5))
                    p = row.Cells[i].AddParagraph(dateValue.ToString(column.Item5));
                else
                    p = row.Cells[i].AddParagraph(text);

                // Alignment – better to set on paragraph instead of cell (more predictable)
                p.Format.Alignment = getParagrphAlignment(column.Item4);

                // Font color
                p.Format.Font.Color = column.Item7 != null
                    ? getMigraDocColor(column.Item7(item))
                    : MigraDoc.DocumentObjectModel.Colors.Black;

                // Cell background – only when explicitly provided
                if (column.Item6 != null)
                {
                    row.Cells[i].Shading.Color = getMigraDocColor(column.Item6(item));
                }
            }
        }

        var renderer = new PdfDocumentRenderer
        {
            Document = document
        };
        renderer.RenderDocument();
        renderer.PdfDocument.Save(pdfPath);
        return true;
    }
    catch (Exception)
    {
        return false;
    }
}
