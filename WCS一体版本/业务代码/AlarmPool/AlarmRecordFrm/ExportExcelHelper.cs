using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;

namespace ZHQXC.AlarmPool
{
    public static class ExportExcelHelper
    {
        const string excel_xml_temp = @"<?xml version=""1.0""?>
<?mso-application progid=""Excel.Sheet""?>
<Workbook xmlns=""urn:schemas-microsoft-com:office:spreadsheet""
 xmlns:o=""urn:schemas-microsoft-com:office:office""
 xmlns:x=""urn:schemas-microsoft-com:office:excel""
 xmlns:ss=""urn:schemas-microsoft-com:office:spreadsheet""
 xmlns:html=""http://www.w3.org/TR/REC-html40"">
 <DocumentProperties xmlns=""urn:schemas-microsoft-com:office:office"">
  <Author>paopao</Author>
  <LastAuthor>paopao</LastAuthor>
  <Created>2014-03-26T00:54:41Z</Created>
  <Company>Microsoft</Company>
  <Version>14.00</Version>
 </DocumentProperties>
 <OfficeDocumentSettings xmlns=""urn:schemas-microsoft-com:office:office"">
  <AllowPNG/>
 </OfficeDocumentSettings>
 <ExcelWorkbook xmlns=""urn:schemas-microsoft-com:office:excel"">
  <WindowHeight>9405</WindowHeight>
  <WindowWidth>20475</WindowWidth>
  <WindowTopX>600</WindowTopX>
  <WindowTopY>90</WindowTopY>
  <ProtectStructure>False</ProtectStructure>
  <ProtectWindows>False</ProtectWindows>
 </ExcelWorkbook>
 <Styles>
  <Style ss:ID=""Default"" ss:Name=""Normal"">
   <Alignment ss:Vertical=""Center""/>
   <Borders/>
   <Font ss:FontName=""宋体"" x:CharSet=""134"" ss:Size=""11"" ss:Color=""#000000""/>
   <Interior/>
   <NumberFormat/>
   <Protection/>
  </Style>
 </Styles>
 <Worksheet ss:Name=""Sheet1"">
  <Table ss:ExpandedColumnCount=""255"" ss:ExpandedRowCount=""655350"" x:FullColumns=""1""
   x:FullRows=""1"" ss:DefaultColumnWidth=""54"" ss:DefaultRowHeight=""13.5"">
  
  </Table>
  <WorksheetOptions xmlns=""urn:schemas-microsoft-com:office:excel"">
   <PageSetup>
    <Header x:Margin=""0.3""/>
    <Footer x:Margin=""0.3""/>
    <PageMargins x:Bottom=""0.75"" x:Left=""0.7"" x:Right=""0.7"" x:Top=""0.75""/>
   </PageSetup>
   <Selected/>
   <Panes>
    <Pane>
     <Number>3</Number>
     <ActiveRow>3</ActiveRow>
     <ActiveCol>3</ActiveCol>
    </Pane>
   </Panes>
   <ProtectObjects>False</ProtectObjects>
   <ProtectScenarios>False</ProtectScenarios>
  </WorksheetOptions>
 </Worksheet>
 <Worksheet ss:Name=""Sheet2"">
  <Table ss:ExpandedColumnCount=""1"" ss:ExpandedRowCount=""1"" x:FullColumns=""1""
   x:FullRows=""1"" ss:DefaultColumnWidth=""54"" ss:DefaultRowHeight=""13.5"">
  </Table>
  <WorksheetOptions xmlns=""urn:schemas-microsoft-com:office:excel"">
   <PageSetup>
    <Header x:Margin=""0.3""/>
    <Footer x:Margin=""0.3""/>
    <PageMargins x:Bottom=""0.75"" x:Left=""0.7"" x:Right=""0.7"" x:Top=""0.75""/>
   </PageSetup>
   <ProtectObjects>False</ProtectObjects>
   <ProtectScenarios>False</ProtectScenarios>
  </WorksheetOptions>
 </Worksheet>
 <Worksheet ss:Name=""Sheet3"">
  <Table ss:ExpandedColumnCount=""1"" ss:ExpandedRowCount=""1"" x:FullColumns=""1""
   x:FullRows=""1"" ss:DefaultColumnWidth=""54"" ss:DefaultRowHeight=""13.5"">
  </Table>
  <WorksheetOptions xmlns=""urn:schemas-microsoft-com:office:excel"">
   <PageSetup>
    <Header x:Margin=""0.3""/>
    <Footer x:Margin=""0.3""/>
    <PageMargins x:Bottom=""0.75"" x:Left=""0.7"" x:Right=""0.7"" x:Top=""0.75""/>
   </PageSetup>
   <ProtectObjects>False</ProtectObjects>
   <ProtectScenarios>False</ProtectScenarios>
  </WorksheetOptions>
 </Worksheet>
</Workbook>";

       static XmlDocument CreateDocument(DataGridView dagaGridView, String sheetName = "Sheet1")
        {

            XmlDocument doc = new XmlDocument();
            doc.LoadXml(excel_xml_temp);
            XmlNamespaceManager nsp = new XmlNamespaceManager(doc.NameTable);
            nsp.AddNamespace("o", "urn:schemas-microsoft-com:office:office");
            nsp.AddNamespace("x", "urn:schemas-microsoft-com:office:excel");
            nsp.AddNamespace("ss", "urn:schemas-microsoft-com:office:spreadsheet");
            nsp.AddNamespace("html", "http://www.w3.org/TR/REC-html40");

            var tableElement = doc.SelectSingleNode("ss:Workbook/ss:Worksheet[@ss:Name='" + sheetName + "']/ss:Table", nsp);

            var columnsRowElement = doc.CreateElement("Row", "urn:schemas-microsoft-com:office:spreadsheet");
            foreach (DataGridViewColumn column in dagaGridView.Columns)
            {
                var colElement = doc.CreateElement("Cell", "urn:schemas-microsoft-com:office:spreadsheet");
                var dataElement = doc.CreateElement("Data", "urn:schemas-microsoft-com:office:spreadsheet");
                var dataTypeAttribute = doc.CreateAttribute("ss", "Type","urn:schemas-microsoft-com:office:spreadsheet");
                dataTypeAttribute.Value = "String";
                dataElement.Attributes.Append(dataTypeAttribute);
                dataElement.InnerText = column.HeaderText;
                colElement.AppendChild(dataElement);
                columnsRowElement.AppendChild(colElement);
            }
            tableElement.AppendChild(columnsRowElement);

            foreach (DataGridViewRow valueRow in dagaGridView.Rows)
            {
                var rowElement = doc.CreateElement("Row", "urn:schemas-microsoft-com:office:spreadsheet");
                foreach (DataGridViewCell cell in valueRow.Cells)
                {
                    var cellElement = doc.CreateElement("Cell", "urn:schemas-microsoft-com:office:spreadsheet");
                    var dataElement = doc.CreateElement("Data", "urn:schemas-microsoft-com:office:spreadsheet");
                    var dataTypeAttribute = doc.CreateAttribute("ss", "Type", "urn:schemas-microsoft-com:office:spreadsheet");
                    dataTypeAttribute.Value = "String";
                    dataElement.Attributes.Append(dataTypeAttribute);
                    if (String.IsNullOrWhiteSpace(Convert.ToString(cell.Value))){
                        dataElement.InnerText = Convert.ToString(cell.FormattedValue) + "";
                    }
                    else {
                        dataElement.InnerText = Convert.ToString(cell.Value) + "";
                    }
                    cellElement.AppendChild(dataElement);
                    rowElement.AppendChild(cellElement);
                }

                tableElement.AppendChild(rowElement);
            }

            return doc;
        }

        /// <summary>
        /// 导出当前显示的内容为 excel 格式
        /// </summary>
        /// <param name="dataGridView"></param>
        public static void ExportAsExcel(this DataGridView dataGridView,string fileName)
        {
            var doc = CreateDocument(dataGridView);
            doc.Save(fileName);
        }

    }
}
