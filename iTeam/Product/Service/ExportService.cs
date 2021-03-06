using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using System.Drawing;
using Microsoft.Office.Interop.Excel;
using Microsoft.Office.Interop.Word;
using System.IO;
using System.Diagnostics;
using System.Reflection;
using System.Data;
using Microsoft.Win32;
using ICSharpCode.SharpZipLib.Zip;
using ICSharpCode.SharpZipLib.Checksums;
using System.Collections;
using System.Runtime.InteropServices;
using System.Threading;
using System.Drawing.Imaging;

namespace OwLib
{
    /// <summary>
    /// 导出服务
    /// </summary>
    public class ExportService
    {
        #region 基本变量
        private static String tempDir;

        /// <summary>
        /// 获取或设置临时目录
        /// </summary>
        public static String TempDir
        {
            get { return ExportService.tempDir; }
            set { ExportService.tempDir = value; }
        }
        #endregion

        #region 设置公司标示
        /// <summary>
        /// 添加数据源标识
        /// </summary>
        /// <param name="worksheet">Worksheet</param>
        /// <param name="row">行</param>
        public void SetExcelDataSource(Worksheet worksheet, int row)
        {
            Microsoft.Office.Interop.Excel.Range range = worksheet.get_Range(worksheet.Cells[row + 6, 1], worksheet.Cells[row + 7, 1]);
            range.Font.ColorIndex = 3;
            range.Font.Bold = true;
            range.Cells[1, 1] = "数据来源：owchart";
        }
        #endregion

        #region 检查Office版本 ,1为03版本

        /// <summary>
        /// 检查Office版本 ,1为03版本
        /// </summary>
        /// <returns></returns>
        public int ExistsRegedit()
        {
            int ifused = 0;
            try
            {
                RegistryKey rk = Registry.LocalMachine;

                //查询Office2003
                RegistryKey f03 = rk.OpenSubKey(@"SOFTWARE\Microsoft\Office\11.0\Excel\InstallRoot\");

                String path2 = null;

                if (f03 != null)
                {
                    String path = f03.GetValue("Path").ToString();
                    path2 = Path.Combine(Directory.GetParent(path).FullName, @"Office12\Moc.exe");
                }

                //查询Office2007
                RegistryKey f07 = rk.OpenSubKey(@"SOFTWARE\Microsoft\Office\12.0\Excel\InstallRoot\");


                //查询Office2010
                RegistryKey f10 = rk.OpenSubKey(@"SOFTWARE\Microsoft\Office\14.0\Excel\InstallRoot\");


                //查询Office2003
                RegistryKey f03_64 = rk.OpenSubKey(@"SOFTWARE\Wow6432Node\Office\11.0\Excel\InstallRoot\");

                //查询Office2007
                RegistryKey f07_64 = rk.OpenSubKey(@"SOFTWARE\Wow6432Node\Office\12.0\Excel\InstallRoot\");


                //查询Office2010
                RegistryKey f10_64 = rk.OpenSubKey(@"SOFTWARE\Wow6432Node\Office\14.0\Excel\InstallRoot\");

                //检查本机是否安装Office2003
                if (f03 != null || f03_64!=null)
                {
                    //String file03 = f03.GetValue("Path").ToString();
                    //if (File.Exists(file03 + "Excel.exe")) ifused += 1;
                    ifused = 1;
                }

                if (path2 != null && File.Exists(path2))
                {
                    ifused = 2;
                }

                //检查本机是否安装Office2007

                if (f07 != null || f07_64!=null)
                {
                    //String file07 = akey.GetValue("Path").ToString();
                    //if (File.Exists(file07 + "Excel.exe")) ifused += 2;
                    ifused = 2;
                }

                //检查本机是否安装Office2010

                if (f10 != null || f10_64!=null)
                {
                    //String file07 = akey.GetValue("Path").ToString();
                    //if (File.Exists(file07 + "Excel.exe")) ifused += 2;
                    ifused = 3;
                }
            }
            catch
            {
                //Log.WriteLog("读取OFFICE注册表错误");
                return -1;
            }
            return ifused;
        }

        #endregion

        #region 打开Word，Excel

        /// <summary>
        /// 打开Word
        /// </summary>
        /// <param name="OpenFilePath">打开地址</param>
        private static void OpenWordFile(String OpenFilePath)
        {
            try
            {
                Process excel = new Process();
                excel.StartInfo.FileName = "word.exe";
                excel.StartInfo.FileName = OpenFilePath;
                excel.Start();
            }
            catch (Exception ex)
            {
            }
        }

        /// <summary>
        /// 打开Excel
        /// </summary>
        /// <param name="OpenFilePath">打开地址</param>
        public void OpenExcelFile(String OpenFilePath)
        {
            try
            {
                Process excel = new Process();
                excel.StartInfo.FileName = "excel.exe";
                excel.StartInfo.FileName = OpenFilePath;
                excel.Start();
            }
            catch(Exception ex)
            {
            }
        }

        #endregion

        #region 导出数据到剪切板

        public void AddClipboard(String head, String body)
        {
            Clipboard.SetDataObject(head + body + "\r\n数据来源：owchart");
        }

        #endregion

        #region 导出数据到DBF
        /// <summary>
        /// 导出数据到BDF
        /// </summary>
        /// <param name="dt">数据表</param>
        /// <param name="fileName">文件名</param>
        public void ExportDataTableToDBF(System.Data.DataTable dt, String fileName)
        {
            SaveFileDialog saveDialog = new SaveFileDialog();
            saveDialog.DefaultExt = "dbf";
            saveDialog.Filter = "DBF文件|*.dbf";
            if (!String.IsNullOrEmpty(fileName))
                saveDialog.FileName = fileName;
            if (saveDialog.ShowDialog() == DialogResult.OK)
            {
                fileName = saveDialog.FileName;
                DBFHelper.DataTableToDBFRealDataType(dt, fileName);
            }
        }

        #endregion

        #region 导出数据到txt

        /// <summary>
        /// 将Html导出为txt
        /// </summary>
        /// <param name="fileName">文件名</param>
        /// <param name="html">html</param>
        /// <returns>是否成功</returns>
        public bool ExportHtmlToTxt(String fileName, String html)
        {
            SaveFileDialog saveDialog = new SaveFileDialog();
            saveDialog.DefaultExt = "txt";
            saveDialog.Filter = "Txt文件|*.txt";
            saveDialog.FileName = fileName;
            if (saveDialog.ShowDialog() == DialogResult.OK)
            {
                fileName = saveDialog.FileName;
            }
            else
            {
                return false;
            }
            Dictionary<String, String> patterns = new Dictionary<String, String>();
            patterns["<br>"] = "\r\n";
            patterns["<BR>"] = "\r\n";
            patterns["<br/>"] = "\r\n";
            patterns["<BR/>"] = "\r\n";
            patterns["<P>"] = "\r\n";
            patterns["</P>"] = "";
            patterns["<p>"] = "\r\n";
            patterns["</p>"] = "";
            patterns["&nbsp;"] = " ";
            foreach (String pat in patterns.Keys)
            {
                if (html.IndexOf(pat) != -1)
                {
                    html = html.Replace(pat, patterns[pat]);
                }
            }
            FileStream fs = new FileStream(fileName, FileMode.Create);
            StreamWriter sw = new StreamWriter(fs);
            sw.Write(html);
            sw.Dispose();
            fs.Dispose();
            return true;
        }

        #endregion

        #region 导出到图片
        /// <summary>
        /// 导出图片
        /// </summary>
        /// <param name="fileName">文件名</param>
        /// <param name="image">图片</param>
        public void ExportImage(String fileName, Image image)
        {
            SaveFileDialog saveDialog = new SaveFileDialog();
            saveDialog.DefaultExt = "jpg";
            saveDialog.Filter = "JPG文件|*.jpg";
            saveDialog.FileName = fileName;
            if (saveDialog.ShowDialog() == DialogResult.OK)
            {
                fileName = saveDialog.FileName;
            }
            else
            {
                return;
            }
            image.Save(fileName);
            image.Dispose();
        }
        #endregion

        #region 导出数据到Word

        /// <summary>
        /// 导出DataView和图片到Word
        /// </summary>
        /// <param name="fileName">文件名</param>
        /// <param name="dataView">视图</param>
        /// <param name="image">图片</param>
        public void ExportDataViewToWord(String fileName, DataView dataView, Image image)
        {
            if (!codeboolisExcelInstalled())
            {
                MessageBox.Show("请先安装OFFICE！", "提示");
                return;
            }
            object oMissing = System.Reflection.Missing.Value;
            Microsoft.Office.Interop.Word.Application wordApp = new Microsoft.Office.Interop.Word.Application();
            String tempImagePath = tempDir + System.Guid.NewGuid().ToString();
            try
            {
                wordApp.Visible = false;
                Microsoft.Office.Interop.Word.Document wordDoc = wordApp.Documents.Add(ref oMissing, ref oMissing, ref oMissing, ref oMissing);
                Microsoft.Office.Interop.Word.Range tempRange = wordDoc.Application.Selection.Range;
                if (image != null)
                {
                    image.Save(tempImagePath);
                    InlineShape shape = wordDoc.Application.ActiveDocument.InlineShapes.AddPicture(tempImagePath, ref oMissing, ref oMissing, ref oMissing);
                    tempRange = shape.Range.Next(ref oMissing, ref oMissing);
                }
                if (dataView != null)
                {
                    int rowCount = dataView.Count;
                    int colCount = dataView.Table.Columns.Count;
                    object tableBehavior = null;
                    object autoFitBehavior = null;
                    //创建表格
                    Microsoft.Office.Interop.Word.Table oTable = wordDoc.Tables.Add(tempRange, rowCount, colCount, ref tableBehavior, ref autoFitBehavior);
                    oTable.Borders.InsideLineStyle = Microsoft.Office.Interop.Word.WdLineStyle.wdLineStyleSingle;
                    oTable.Borders.OutsideLineStyle = Microsoft.Office.Interop.Word.WdLineStyle.wdLineStyleSingle;
                    oTable.ApplyStyleHeadingRows = true;
                    oTable.Select();
                    //循环遍历行
                    for (int i = 1; i <= dataView.Count; i++)
                    {
                        //循环遍历列
                        for (int j = 1; j <= dataView.Table.Columns.Count; j++)
                        {
                            if (!(dataView[i - 1][j - 1] is DBNull))
                            {
                                oTable.Cell(i, j).Range.Text = dataView[i - 1][j - 1].ToString();
                            }
                        }
                    }
                    tempRange = oTable.Range.Next(ref oMissing, ref oMissing);
                }
                //object tableBehavior = null;
                //object autoFitBehavior = null;
                //wordDoc.Tables.Add(tempRange, 1, 1, ref tableBehavior, ref autoFitBehavior);
                object range = tempRange;
                Paragraph para = tempRange.Paragraphs.Add(ref range);
                para.Range.InsertParagraphAfter();
                para.Range.Text = "\r\n数据来源：owchart";
                para.Range.Font.ColorIndex = WdColorIndex.wdRed;
                para.Range.Font.Bold = 1;
                para.Range.Font.Size = 10;
                wordDoc.Application.Selection.Font.Size = 12;
                object combineDocNameObj = fileName;
                wordDoc.SaveAs2(ref combineDocNameObj, ref oMissing, ref oMissing, ref oMissing, ref oMissing, ref oMissing
                    , ref oMissing, ref oMissing, ref oMissing, ref oMissing, ref oMissing
                    , ref oMissing, ref oMissing, ref oMissing, ref oMissing, ref oMissing, ref oMissing);
                wordDoc.Close(ref oMissing, ref oMissing, ref oMissing);
            }
            finally
            {
                wordApp.Quit(ref oMissing, ref oMissing, ref oMissing);
                System.Runtime.InteropServices.Marshal.ReleaseComObject(wordApp);
                if (File.Exists(tempImagePath))
                {
                    File.Delete(tempImagePath);
                }
            }
            OpenWordFile(fileName);
        }

        /// <summary>
        /// 导出和图片到Word
        /// </summary>
        /// <param name="fileName">文件名</param>
        /// <param name="image">图片</param>
        public void ExportDataViewToWord2(String fileName, Image image)
        {
            SaveFileDialog saveDialog = new SaveFileDialog();
            saveDialog.DefaultExt = "doc";
            saveDialog.Filter = "Word文件|*.doc;*.docx";
            saveDialog.FileName = fileName;
            if (saveDialog.ShowDialog() == DialogResult.OK)
            {
                fileName = saveDialog.FileName;
            }
            else
            {
                return;
            }
            if (!codeboolisExcelInstalled())
            {
                MessageBox.Show("请先安装OFFICE！", "提示");
                return;
            }

            object oEndOfDoc = "\\endofdoc"; //指定编码
            object oMissing = System.Reflection.Missing.Value;
            Microsoft.Office.Interop.Word.Application wordApp = new Microsoft.Office.Interop.Word.Application();
            String tempImagePath = tempDir + System.Guid.NewGuid().ToString();
            try
            {
                wordApp.Visible = false;
                Microsoft.Office.Interop.Word.Document wordDoc = wordApp.Documents.Add(ref oMissing, ref oMissing, ref oMissing, ref oMissing);
                Microsoft.Office.Interop.Word.Range tempRange = wordDoc.Application.Selection.Range;
                if (image != null)
                {
                    image.Save(tempImagePath);
                    object LinkToFile = false;
                    object SaveWithDocument = true;
                    object Anchor = wordDoc.Application.Selection.Range;
                    InlineShape shape = wordDoc.Application.ActiveDocument.InlineShapes.AddPicture(tempImagePath, ref LinkToFile, ref SaveWithDocument, ref Anchor);
                    Microsoft.Office.Interop.Word.Shape s = wordDoc.Application.ActiveDocument.InlineShapes[1].ConvertToShape();
                    s.WrapFormat.Type = Microsoft.Office.Interop.Word.WdWrapType.wdWrapSquare;
                    tempRange = shape.Range.Next(ref oMissing, ref oMissing);
                }

                object range = tempRange;
                //oPara1 = wordDoc.Content.Paragraphs.Add(ref oMissing);
                //Paragraph para = tempRange.Paragraphs.Add(ref range);
                Paragraph para = wordDoc.Content.Paragraphs.Add(ref range);
                //para.Range.InsertParagraphAfter();
                para.Range.Text = "\r\n数据来源：owchart";
                para.Range.Font.ColorIndex = WdColorIndex.wdRed;
                para.Range.Font.Bold = 1;
                para.Range.Font.Size = 10;
                wordDoc.Application.Selection.Font.Size = 12;
                object combineDocNameObj = fileName;
                wordDoc.SaveAs2(ref combineDocNameObj, ref oMissing, ref oMissing, ref oMissing, ref oMissing, ref oMissing
                    , ref oMissing, ref oMissing, ref oMissing, ref oMissing, ref oMissing
                    , ref oMissing, ref oMissing, ref oMissing, ref oMissing, ref oMissing, ref oMissing);
                wordDoc.Close(ref oMissing, ref oMissing, ref oMissing);
            }
            finally
            {
                wordApp.Quit(ref oMissing, ref oMissing, ref oMissing);
                System.Runtime.InteropServices.Marshal.ReleaseComObject(wordApp);
                if (File.Exists(tempImagePath))
                {
                    File.Delete(tempImagePath);
                }
            }
            OpenWordFile(fileName);
        }

        /// <summary>
        /// 导出和图片到Word
        /// </summary>
        /// <param name="fileName">文件名</param>
        /// <param name="image">图片</param>
        public void ExportDataViewToWord(String fileName, Image image)
        {
            if (!codeboolisExcelInstalled())
            {
                MessageBox.Show("请先安装OFFICE！", "提示");
                return;
            }

            object oEndOfDoc = "\\endofdoc"; //指定编码
            object oMissing = System.Reflection.Missing.Value;
            Microsoft.Office.Interop.Word.Application wordApp = new Microsoft.Office.Interop.Word.Application();
            String tempImagePath = tempDir + System.Guid.NewGuid().ToString();
            try
            {
                wordApp.Visible = false;
                Microsoft.Office.Interop.Word.Document wordDoc = wordApp.Documents.Add(ref oMissing, ref oMissing, ref oMissing, ref oMissing);
                Microsoft.Office.Interop.Word.Range tempRange = wordDoc.Application.Selection.Range;
                if (image != null)
                {
                    image.Save(tempImagePath);
                    object LinkToFile = false;
                    object SaveWithDocument = true;
                    object Anchor = wordDoc.Application.Selection.Range;
                    InlineShape shape = wordDoc.Application.ActiveDocument.InlineShapes.AddPicture(tempImagePath, ref LinkToFile, ref SaveWithDocument, ref Anchor);
                    Microsoft.Office.Interop.Word.Shape s = wordDoc.Application.ActiveDocument.InlineShapes[1].ConvertToShape();
                    s.WrapFormat.Type = Microsoft.Office.Interop.Word.WdWrapType.wdWrapSquare;
                    tempRange = shape.Range.Next(ref oMissing, ref oMissing);
                }

                object range = tempRange;
                //oPara1 = wordDoc.Content.Paragraphs.Add(ref oMissing);
                //Paragraph para = tempRange.Paragraphs.Add(ref range);
                Paragraph para = wordDoc.Content.Paragraphs.Add(ref range);
                //para.Range.InsertParagraphAfter();
                para.Range.Text = "\r\n数据来源：owchart";
                para.Range.Font.ColorIndex = WdColorIndex.wdRed;
                para.Range.Font.Bold = 1;
                para.Range.Font.Size = 10;
                wordDoc.Application.Selection.Font.Size = 12;
                object combineDocNameObj = fileName;
                wordDoc.SaveAs2(ref combineDocNameObj, ref oMissing, ref oMissing, ref oMissing, ref oMissing, ref oMissing
                    , ref oMissing, ref oMissing, ref oMissing, ref oMissing, ref oMissing
                    , ref oMissing, ref oMissing, ref oMissing, ref oMissing, ref oMissing, ref oMissing);
                wordDoc.Close(ref oMissing, ref oMissing, ref oMissing);
            }
            finally
            {
                wordApp.Quit(ref oMissing, ref oMissing, ref oMissing);
                System.Runtime.InteropServices.Marshal.ReleaseComObject(wordApp);
                if (File.Exists(tempImagePath))
                {
                    File.Delete(tempImagePath);
                }
            }
            OpenWordFile(fileName);
        }

        /// <summary>
        /// 将Html导出为word
        /// </summary>
        /// <param name="fileName">文件名</param>
        /// <param name="html">html</param>
        /// <returns>是否成功</returns>
        public bool ExportHtmlToWord(String fileName, String html)
        {

            if (!codeboolisExcelInstalled())
            {
                MessageBox.Show("请先安装OFFICE！", "提示");
                return false;
            }
            object oMissing = System.Reflection.Missing.Value;
            object oSaveChanges = Microsoft.Office.Interop.Excel.XlSaveAction.xlDoNotSaveChanges;
            SaveFileDialog saveDialog = new SaveFileDialog();
            saveDialog.DefaultExt = "doc";
            saveDialog.Filter = "Word文件|*.doc;*.docx";
            saveDialog.FileName = fileName;
            if (saveDialog.ShowDialog() == DialogResult.OK)
            {
                fileName = saveDialog.FileName;
            }
            else
            {
                return false;
            }
            Dictionary<String, String> patterns = new Dictionary<String, String>();
            patterns["<br>"] = "\r\n";
            patterns["<BR>"] = "\r\n";
            patterns["<br/>"] = "\r\n";
            patterns["<BR/>"] = "\r\n";
            patterns["<P>"] = "\r\n";
            patterns["</P>"] = "";
            patterns["<p>"] = "\r\n";
            patterns["</p>"] = "";
            foreach (String pat in patterns.Keys)
            {
                if (html.IndexOf(pat) != -1)
                {
                    html = html.Replace(pat, patterns[pat]);
                }
            }
            Microsoft.Office.Interop.Word.Application wordApp = new Microsoft.Office.Interop.Word.Application();
            try
            {
                wordApp.Visible = false;
                Microsoft.Office.Interop.Word.Document wordDoc = wordApp.Documents.Add(ref oMissing, ref oMissing, ref oMissing, ref oMissing);

                //object start = 0;
                //object end = 0;
                Microsoft.Office.Interop.Word.Range tempRange = wordDoc.Application.Selection.Range;

                object range = tempRange;
                Paragraph para = tempRange.Paragraphs.Add(ref range);

                para.Range.InsertParagraphAfter();
                para.Range.Text = html;

                object combineDocNameObj = fileName;
                wordDoc.SaveAs2(ref combineDocNameObj, ref oMissing, ref oMissing, ref oMissing, ref oMissing, ref oMissing
                    , ref oMissing, ref oMissing, ref oMissing, ref oMissing, ref oMissing
                    , ref oMissing, ref oMissing, ref oMissing, ref oMissing, ref oMissing, ref oMissing);
                wordDoc.Close(ref oMissing, ref oMissing, ref oMissing);

            }
            finally
            {
                wordApp.Quit(ref oMissing, ref oMissing, ref oMissing);
                System.Runtime.InteropServices.Marshal.ReleaseComObject(wordApp);
            }
            OpenWordFile(fileName);
            return true;
        }
        #endregion

        #region 导出数据到Excel

        #region 弹出框

        public SaveFileDialog ShowDialog(String fileName)
        {
            SaveFileDialog dialog = new SaveFileDialog();
            String defaultExt = null;
            String otherExt = null;
            if (ExistsRegedit() == 1)
            {
                defaultExt = "xls";
                otherExt = "xlsx";
                NpoiManage.Type = excelType.xls;
            }
            else
            {
                defaultExt = "xlsx";
                otherExt = "xls";
                NpoiManage.Type = excelType.xlsx;
            }
            if (fileName == null || fileName.Trim().Length == 0)
            {
                dialog.FileName = String.Format("*.{0}", defaultExt);
            }
            else
            {
                dialog.FileName = Path.GetFileNameWithoutExtension(fileName);
            }
            dialog.DefaultExt = defaultExt;
            dialog.Filter = String.Format("Excel文件(*.{0})|*.{0}|Excel文件(*.{1})|*.{1}|All file(*.*)|*.*", defaultExt, otherExt);
            return dialog;
        }
        #endregion

        #region 内部类

        /// <summary>
        /// 行列式信息描述
        /// </summary>
        public class Determinant
        {
            /// <summary>
            /// 构造函数
            /// </summary>
            /// <param name="tdPos">TD索引</param>
            /// <param name="trPos">TR索引</param>
            /// <param name="row">单元格行号</param>
            /// <param name="col">单元格列号</param>
            public Determinant(int tdPos, int trPos, int row, int col)
            {
                this.tdPos = tdPos;
                this.trPos = trPos;
                this.row = row;
                this.col = col;
            }

            private int tdPos;

            /// <summary>
            /// 获取或设置TD索引
            /// </summary>
            public int TdPos
            {
                get { return tdPos; }
                set { tdPos = value; }
            }

            private int trPos;

            /// <summary>
            /// 获取或设置TR索引
            /// </summary>
            public int TrPos
            {
                get { return trPos; }
                set { trPos = value; }
            }

            private int row;

            /// <summary>
            /// 获取或设置单元格行号
            /// </summary>
            public int Row
            {
                get { return row; }
                set { row = value; }
            }

            private int col;

            /// <summary>
            /// 获取或设置单元格列号
            /// </summary>
            public int Col
            {
                get { return col; }
                set { col = value; }
            }

            /// <summary>
            /// 转换为字符串
            /// </summary>
            /// <returns></returns>
            public override String ToString()
            {
                return "tr:" + trPos.ToString() + "td:" + tdPos.ToString();
            }
        }
        #endregion

        #region 判断方法

        /// <summary>
        /// 判断是否存在Excel
        /// </summary>
        /// <returns></returns>
        private bool codeboolisExcelInstalled()
        {
            Type type = Type.GetTypeFromProgID("Excel.Application");
            return type != null;
        }
        #endregion

        #region 数据转换，数据收集
        /// <summary>
        /// ConvertDataViewToArray 
        /// </summary>
        /// <param name="dataView">DataView</param>
        /// <returns></returns>
        public object[,] ConvertDataViewToArray(System.Data.DataView dataView)
        {
            int rowCount = dataView.Count;
            //初始化二维数组
            object[,] array = new object[rowCount, dataView.Table.Columns.Count];
            //循环遍历表格，拼装数组
            for (int r = 0; r < dataView.Count; r++)
            {
                for (int c = 0; c < dataView.Table.Columns.Count; c++)
                {
                    array[r, c] = dataView[r].Row[c];
                }
            }
            return array;
        }

        /// <summary>
        /// 将DataView导出为二维数组
        /// </summary>
        /// <param name="dataView">DataView</param>
        /// <param name="titles">标题名</param>
        /// <returns></returns>
        public object[,] ConvertDataViewToArray(System.Data.DataView dataView, String[] titles)
        {
            int rowCount = titles == null ? dataView.Count : dataView.Count + 1;
            //初始化二维数组
            object[,] array = new object[rowCount, dataView.Table.Columns.Count];
            //拼装列头
            if (titles != null)
            {
                for (int c = 0; c < dataView.Table.Columns.Count; c++)
                {
                    array[0, c] = titles[c];
                }
            }
            //循环遍历表格，拼装数组
            for (int r = 0; r < dataView.Count; r++)
            {
                for (int c = 0; c < dataView.Table.Columns.Count; c++)
                {
                    if (titles != null)
                    {
                        array[r + 1, c] = dataView[r].Row[c];
                    }
                    else
                    {
                        array[r, c] = dataView[r].Row[c];
                    }
                }
            }
            return array;
        }

        /// <summary>
        /// 收集TR类型的html元素
        /// </summary>
        /// <param name="trElements">TR元素</param>
        /// <param name="element">HTML元素</param>
        public void CollectTrElements(List<HtmlElement> trElements, HtmlElement element)
        {
            if (element.TagName == "TR")
            {
                trElements.Add(element);
            }
            else
            {
                //递归搜集
                if (element.Children.Count > 0)
                {
                    for (int i = 0; i < element.Children.Count; i++)
                    {
                        CollectTrElements(trElements, element.Children[i]);
                    }
                }
            }
        }

        /// <summary>
        /// DataView转换为DataTable
        /// </summary>
        /// <param name="dgv"></param>
        /// <returns></returns>
        public System.Data.DataTable GetDgvToTable(System.Data.DataView dgv)
        {
            System.Data.DataTable dt = dgv.Table;
            return dt;
        }

        #endregion

        #region 数据填充

        /// <summary>
        /// 把DataTable填充到Excel中
        /// </summary>
        /// <param name="basePoint">基础Range</param>
        /// <param name="data">二维数组</param>
        /// <param name="format">格式</param>
        /// <param name="isAutoFit">是否自动布局</param>
        /// <param name="isFormula">是否使用公式</param>
        /// <returns></returns>
        public Microsoft.Office.Interop.Excel.Range FillRange(Microsoft.Office.Interop.Excel.Range basePoint, object[,] data, String format, bool isAutoFit, bool isFormula)
        {
            if (!codeboolisExcelInstalled())
            {
                MessageBox.Show("请先安装Excel！", "提示");
                return null;
            }
            int rows = data.GetLength(0);
            int columns = data.GetLength(1);
            if (rows > 0 && columns > 0)
            {
                Microsoft.Office.Interop.Excel.Range range = basePoint.get_Resize(rows, columns);
                if (isFormula)
                {
                    for (int rind = 0; rind < rows; rind++)
                    {
                        for (int cind = 0; cind < columns; cind++)
                        {
                            Microsoft.Office.Interop.Excel.Range currentRange = range.get_Item(rind + 1, cind + 1) as Microsoft.Office.Interop.Excel.Range;
                        }
                    }
                }
                else
                {
                    if (!String.IsNullOrEmpty(format))
                    {
                        range.Columns.NumberFormatLocal = format;
                    }
                    range.set_Value(XlRangeValueDataType.xlRangeValueDefault, data);
                    //range.Value = data;
                }
                if (isAutoFit)
                {
                    range.Columns.AutoFit();
                }
                basePoint = basePoint.get_Resize(1, columns);
                basePoint.WrapText = true;
                return range;
            }
            return basePoint;
        }

        #endregion

        #region Html导出到Excel

        /// <summary>
        /// 从Html导出到Excel
        /// </summary>
        /// <param name="tableElement">table元素</param>
        /// <param name="worksheet">WorkSheet对象</param>
        /// <param name="rowCountLimit">输出行数的限制</param>
        /// <param name="rowCount">行数</param>
        /// <returns></returns>
        public void ExportHtmlTableToExcel(HtmlElement tableElement, Microsoft.Office.Interop.Excel.Worksheet worksheet, int rowCountLimit, out int rowCount)
        {
            //Log.WriteLog("Excel导出日志：进入函数public void ExportHtmlTableToExcel(HtmlElement tableElement, Microsoft.Office.Interop.Excel.Worksheet worksheet, int rowCountLimit, out int rowCount)");
            //创建保存Tr元素的集合
            //Log.WriteLog("Excel导出日志：创建保存Tr元素的集合");
            List<HtmlElement> trElements = new List<HtmlElement>();

            //收集Tr元素
            //Log.WriteLog("Excel导出日志：收集Tr元素");
            CollectTrElements(trElements, tableElement);

            //创建行列式描述
            //Log.WriteLog("Excel导出日志：创建行列式描述");
            Dictionary<String, Determinant> determinants = new Dictionary<String, Determinant>();

            //初始化第一个单元格
            //Log.WriteLog("Excel导出日志：初始化第一个单元格");
            Determinant firstDeterminant = new Determinant(0, 0, 1, 1);
            determinants[firstDeterminant.ToString()] = firstDeterminant;
            int colCount = 1;

            //Log.WriteLog("Excel导出日志：循环遍历Tr");
            //循环遍历Tr
            rowCount = trElements.Count;
            int nextColStart = 1;
            int rCount = 0;
            for (int i = 0; i < trElements.Count; i++)
            {
                //行数限制
                //Log.WriteLog("Excel导出日志：行数限制");
                if (i > rowCountLimit - 1)
                {
                    break;
                }
                //获取TR元素
                //Log.WriteLog("Excel导出日志：获取TR元素");
                HtmlElement trElement = trElements[i];
                if (trElement.Children.Count > colCount)
                {
                    colCount = trElement.Children.Count;
                }
                if (rCount > 0)
                {
                    rCount--;
                    if (rCount == 0) nextColStart = 1;
                }
                if (trElement.Children.Count > 0)
                {
                    //循环遍历TR子元素
                    //Log.WriteLog("Excel导出日志：循环遍历TR子元素");
                    bool addflag = true;
                    for (int j = 0; j < trElement.Children.Count; j++)
                    {
                        //获取TD元素
                        //Log.WriteLog("Excel导出日志：获取TD元素");
                        HtmlElement tdElement = trElement.Children[j];

                        #region 把...中的数据加进去
                        try
                        {
                            if (tdElement.InnerText.Contains("..."))
                            {
                                HtmlElement temp = tdElement.Children[0];
                                if (tdElement != null)
                                {
                                    tdElement.InnerText = temp.GetAttribute("title");
                                }
                            }
                        }
                        catch
                        {
#if DEBUG
                            MessageBox.Show("导出网页到Excel错误!");
#endif
                        }
                        #endregion

                        if (tdElement.OuterHtml.Replace(" ", "").IndexOf("display:none", StringComparison.CurrentCultureIgnoreCase) != -1)
                        {
                            continue;
                        }
                        //获取ColSpan
                        //Log.WriteLog("Excel导出日志：获取ColSpan");
                        int colSpan = 1;
                        int rowSpan = 1;
                        String title = String.Empty;
                        try
                        {
                            //获取colSpan和rowSpan
                            //Log.WriteLog("Excel导出日志：获取colSpan和rowSpan");
                            colSpan = Convert.ToInt32(tdElement.GetAttribute("colSpan"));
                            rowSpan = Convert.ToInt32(tdElement.GetAttribute("rowSpan"));
                            //获取标题
                            //Log.WriteLog("Excel导出日志：获取标题");
                            title = tdElement.GetAttribute("sheetname");
                            if (title == null || title.Length == 0)
                            {
                                title = tdElement.GetAttribute("paraname");
                            }
                        }
                        catch (Exception err)
                        {
                            //MessageBox.Show(err.ToString());
                        }
                        if (colSpan == 1 && rowSpan == 1)
                        {
                            addflag = false;
                        }
                        //获取rowspan
                        //获取行列式
                        //Log.WriteLog("Excel导出日志：获取rowspan,获取行列式");
                        String identifier = "tr:" + i.ToString() + "td:" + j.ToString();
                        Determinant thisTd = null;
                        if (determinants.ContainsKey(identifier))
                        {
                            thisTd = determinants[identifier];
                        }
                        else
                        {
                            if (addflag)
                            {
                                thisTd = new Determinant(j, i, i + 1, nextColStart);
                            }
                            else
                            {
                                nextColStart = j + 1;
                                thisTd = new Determinant(j, i, i + 1, j + 1);
                            }
                            determinants[identifier] = thisTd;
                        }
                        int row = thisTd.Row;
                        int col = thisTd.Col;
                        if (j == 0)
                        {
                            col = nextColStart;
                            thisTd.Col = col;
                        }
                        //生成本行接下来一格的行列信息
                        //Log.WriteLog("Excel导出日志：生成本行接下来一格的行列信息");
                        Determinant thisColNext = new Determinant(j + 1, i, row, col + colSpan);
                        determinants[thisColNext.ToString()] = thisColNext;
                        //生成接下来受到rowSpan影响的几行的行列信息
                        //Log.WriteLog("Excel导出日志：生成接下来受到rowSpan影响的几行的行列信息");
                        for (int r = 1; r < rowSpan; r++)
                        {
                            Determinant rowNext = new Determinant(j, i + r, row + r, col);
                            if (j > 0)
                            {
                                if (trElement.Children[j - 1].GetAttribute("rowSpan") != "1")
                                {
                                    rowNext.Col = col + colSpan;
                                    rowNext.TdPos -= colSpan;
                                }
                            }
                            determinants[rowNext.ToString()] = rowNext;
                        }
                        //根据描述生成Excel
                        //Log.WriteLog("Excel导出日志：根据描述生成Excel");
                        if (colSpan > 1 || rowSpan > 1)
                        {
                            Microsoft.Office.Interop.Excel.Range range = worksheet.get_Range(worksheet.Cells[row, col],
                                worksheet.Cells[row + rowSpan - 1, col + colSpan - 1]);
                            range.get_Resize(rowSpan, colSpan).Merge(true);
                        }
                        if (tdElement.TagName == "TH")
                        {
                            Microsoft.Office.Interop.Excel.Range range = worksheet.get_Range(worksheet.Cells[row, col], worksheet.Cells[row + rowSpan - 1, col + colSpan - 1]);
                            //可能的错误
                            try
                            {
                                range.Columns.ColumnWidth = Convert.ToInt32(tdElement.DomElement.GetType().GetProperty("clientWidth").GetValue(tdElement.DomElement, null)) / 8.5 + 2;
                            }
                            catch
                            {

                            }
                            if (title != null && title.Length > 0)
                            {
                                worksheet.Cells[row, col] = (object)title;
                            }
                            else
                            {
                                if (tdElement.InnerText != null)
                                {
                                    worksheet.Cells[row, col] = (object)tdElement.InnerText.Trim();
                                }
                            }
                            range.Font.Bold = true;
                        }
                        else
                        {
                            if (title != null && title.Length > 0)
                            {
                                worksheet.Cells[row, col] = (object)title;
                            }
                            else
                            {
                                if (tdElement.InnerText != null)
                                {
                                    worksheet.Cells[row, col] = (object)tdElement.InnerText.Trim();
                                }
                            }
                        }
                        if (addflag)
                        {
                            if (rowSpan > 1)
                            {
                                nextColStart += colSpan;
                                rCount = rowSpan;
                            }
                        }
                        if (rowSpan <= 1)
                        {
                            addflag = false;
                        }
                        if (j == 0 && !addflag && rCount <= 0)
                        {
                            nextColStart = 1;
                        }
                    }
                }
            }
            //自动适应尺寸
            //Log.WriteLog("Excel导出日志：自动适应尺寸");
            Microsoft.Office.Interop.Excel.Range fullRange = worksheet.get_Range(worksheet.Cells[1, 1], worksheet.Cells[1 + trElements.Count, colCount + 1]);
            fullRange.Columns.AutoFit();
            determinants.Clear();
            trElements.Clear();
        }

        /// <summary>
        /// 从Html导出为Excel
        /// </summary>
        /// <param name="tableElement">table的html元素</param>
        /// <returns></returns>
        public bool ExportHtmlTableToExcel(HtmlElement tableElement)
        {
            String saveFileName = null;
            SaveFileDialog saveDialog = ShowDialog(null);
            if (saveDialog.ShowDialog() == DialogResult.OK)
            {
                saveFileName = saveDialog.FileName;
            }
            else
            {
                return false;
            }

            NpoiManage.SaveToFile(tableElement, saveFileName);
            OpenExcelFile(saveFileName);
            return true;
        }

        /// <summary>
        /// 导出Html的表格到Excel
        /// </summary>
        /// <param name="fileName">文件名</param>
        /// <param name="html">html</param>
        public void ExportHtmlTableToExcel(String fileName, String html)
        {
            try
            {
                String saveFileName = null;
                SaveFileDialog saveDialog = ShowDialog(fileName);
                if (saveDialog.ShowDialog() == DialogResult.OK)
                {
                    saveFileName = saveDialog.FileName;
                    NpoiManage.SaveToFile(html, saveFileName);
                    OpenExcelFile(saveFileName);                   
                }
            }
            catch (Exception ex)
            {
#if DEBUG
                MessageBox.Show("ERROR MSG:" + ex.Message);
#endif
            }
        }

        #endregion

        #region Image导出到Excel

        /// <summary>
        /// 从Image导出到Excel
        /// </summary>
        /// <param name="chartImage">图片</param>
        /// <param name="imageSize">图片尺寸</param>
        /// <param name="fileName">文件名</param>
        public void ExportImageToExcel(Image chartImage, Size imageSize, String fileName)
        {
            try
            {
                SaveFileDialog saveDialog = ShowDialog(fileName);
                if (saveDialog.ShowDialog() == DialogResult.OK)
                {
                    fileName = saveDialog.FileName;
                    NpoiManage.SaveToFile(chartImage, fileName);
                    OpenExcelFile(fileName);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("ERROR MSG:" + ex.Message);
            }
        }

        /// <summary>
        /// 从Image导出到Excel
        /// </summary>
        /// <param name="chartImage">图片</param>
        /// <param name="imageSize">图片尺寸</param>
        /// <param name="fileName">文件名</param>
        /// <param name="noDialog">无弹出框</param>
        public void ExportImageToExcel(Image chartImage, Size imageSize, String fileName, bool noDialog)
        {
            try
            {
                NpoiManage.SaveToFile(chartImage, fileName);
                OpenExcelFile(fileName);
            }
            catch (Exception ex)
            {
                MessageBox.Show("ERROR MSG:" + ex.Message);
            }
        }

        /// <summary>
        /// 从Image导出到Excel
        /// </summary>
        /// <param name="chartImage">图片</param>
        /// <param name="fileName">文件名</param>
        public void ExportImageToExcel(Image chartImage, String fileName)
        {
            ExportImageToExcel(chartImage, chartImage.Size, fileName);
        }

        #endregion

        #region DataView导出到Excel

        /// <summary>
        /// 从DataView导出到Excel
        /// </summary>
        /// <param name="dataView">视图</param>
        /// <param name="fileName">文件名</param>
        public void ExportDataViewToExcel(System.Data.DataView dataView, String fileName)
        {
            //MessageBox.Show("11");
            String[] titles = new String[dataView.Table.Columns.Count];
            for (int i = 0; i < dataView.Table.Columns.Count; i++)
            {
                titles[i] = dataView.Table.Columns[i].Caption;
            }
            Image image = null;
            ExportDataViewToExcel(dataView, titles, image, fileName);
        }

        /// <summary>
        /// 从DataView导出到Excel
        /// </summary>
        /// <param name="dataView"></param>
        /// <param name="headerHtml"></param>
        /// <param name="headerRowCount"></param>
        /// <param name="fileName"></param>
        public void ExportDataViewToExcel(DataView dataView, String headerHtml, int headerRowCount, String fileName)
        {
            try
            {
                SaveFileDialog saveDialog = ShowDialog(fileName);
                if (saveDialog.ShowDialog() == DialogResult.OK)
                {
                    fileName = saveDialog.FileName;
                    NpoiManage.SaveToFile(dataView, headerHtml, fileName);
                    OpenExcelFile(fileName);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("ERROR MSG:" + ex.Message);
            }
        }

        /// <summary>
        /// 从DataView导出到Excel
        /// </summary>
        /// <param name="dataView"></param>
        /// <param name="headerHtml"></param>
        /// <param name="headerRowCount"></param>
        /// <param name="fileName"></param>
        public void ExportDataViewToExcel(DataView dataView, String headerHtml, String fileName)
        {
            try
            {
                SaveFileDialog saveDialog = ShowDialog(fileName);
                if (saveDialog.ShowDialog() == DialogResult.OK)
                {
                    fileName = saveDialog.FileName;
                    NpoiManage.SaveToFile(dataView, headerHtml, fileName);
                    OpenExcelFile(fileName);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("ERROR MSG:" + ex.Message);
            }
        }

        /// <summary>
        /// 从DataView导出到Excel
        /// </summary>
        /// <param name="dataView">视图</param>
        /// <param name="titles">标题</param>
        /// <param name="fileName">文件名</param>
        public void ExportDataViewToExcel(System.Data.DataView dataView, String[] titles, String fileName)
        {
            Image image = null;
            ExportDataViewToExcel(dataView, titles, image, new Size(0, 0), fileName);
        }

        /// <summary>
        /// 从DataView导出到Excel
        /// </summary>
        /// <param name="dataView">视图</param>
        /// <param name="titles">标题</param>
        /// <param name="fileName">文件名</param>
        /// <param name="noDialog">无弹出框</param>
        public void ExportDataViewToExcel(System.Data.DataView dataView, String[] titles, String fileName, bool noDialog)
        {
            Image image = null;
            ExportDataViewToExcel(dataView, titles, image, new Size(0, 0), fileName, noDialog);
        }

        /// <summary>
        /// 从DataView导出到Excel
        /// </summary>
        /// <param name="dataView">视图</param>
        /// <param name="titles">标题</param>
        /// <param name="chartImage">图片</param>
        /// <param name="fileName">文件名</param>
        /// <returns></returns>
        public void ExportDataViewToExcel(System.Data.DataView dataView, String[] titles, Image chartImage, String fileName)
        {
            if (chartImage == null)
            {
                ExportDataViewToExcel(dataView, titles, chartImage, new Size(0, 0), fileName);
            }
            else
            {
                ExportDataViewToExcel(dataView, titles, chartImage, chartImage.Size, fileName);
            }
        }

        /// <summary>
        /// 从DataView导出到Excel
        /// </summary>
        /// <param name="dataView">视图</param>
        /// <param name="titles">标题</param>
        /// <param name="chartImage">图片</param>
        /// <param name="imageSize">图片大小</param>
        /// <param name="fileName">文件名</param>
        public void ExportDataViewToExcel(System.Data.DataView dataView, String[] titles, Image chartImage, Size imageSize, String fileName)
        {
            try
            {
                if (dataView == null)
                {
                    return;
                }
                if (dataView.Count <= 0)
                {
                    return;
                }
                SaveFileDialog saveDialog = ShowDialog(fileName);
                if (saveDialog.ShowDialog() == DialogResult.OK)
                {
                    fileName = saveDialog.FileName;
                    List<String[]> tempTitles = new List<String[]>();
                    tempTitles.Add(titles);
                    NpoiManage.SaveToFile(dataView, tempTitles, columnName.ColumnName, chartImage, fileName);
                    OpenExcelFile(fileName);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("ERROR MSG:" + ex.Message);
            }
        }

        /// <summary>
        /// 从DataView导出到Excel
        /// </summary>
        /// <param name="dataView">视图</param>
        /// <param name="titles">标题</param>
        /// <param name="chartImage">图片</param>
        /// <param name="imageSize">图片大小</param>
        /// <param name="fileName">文件名</param>
        /// <param name="noDialog">无弹出框</param>
        public void ExportDataViewToExcel(System.Data.DataView dataView, String[] titles, Image chartImage, Size imageSize, String fileName, bool noDialog)
        {
            try
            {
                List<String[]> tempTitles = new List<String[]>();
                tempTitles.Add(titles);
                NpoiManage.SaveToFile(dataView, tempTitles, columnName.ColumnName, chartImage, fileName);
                OpenExcelFile(fileName);
            }
            catch (Exception ex)
            {
                MessageBox.Show("ERROR MSG:" + ex.Message);
            }
        }


        public void ExportDataViewToExcel(List<DataView> dataViews, List<String> tableXmls, String fileName, bool noDialog)
        {
            try
            {
                SaveFileDialog saveDialog = ShowDialog(fileName);
                if (noDialog)
                {
                    if (saveDialog.ShowDialog() == DialogResult.OK)
                    {
                        fileName = saveDialog.FileName;
                        NpoiManage.SaveToFile(dataViews, tableXmls, fileName);
                        OpenExcelFile(fileName);
                    }
                }
                else
                {
                    NpoiManage.SaveToFile(dataViews, tableXmls, fileName);
                    OpenExcelFile(fileName);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("ERROR MSG:" + ex.Message);
            }
        }

        #endregion

        #region DataTable导出为Excel

        /// <summary>
        /// 将DataTable导出为Excel
        /// </summary>
        /// <param name="dataTable">DataTable对象</param>
        /// <param name="fileName">文件名</param>
        public void ExportDataTableToExcel(System.Data.DataTable dataTable, String fileName)
        {
            ExportDataTableToExcel(dataTable, null, fileName, columnName.ColumnName);
        }

        /// <summary>
        /// 将DataTable导出为Excel
        /// </summary>
        /// <param name="dataTable">DataTable对象</param>
        /// <param name="fileName">文件名</param>
        public void ExportDataTableToExcel(System.Data.DataTable dataTable, String fileName, columnName cn)
        {
            ExportDataTableToExcel(dataTable, null, fileName, cn);
        }

        /// <summary>
        /// 将DataTable导出为Excel
        /// </summary>
        /// <param name="dataTable">DataTable对象</param>
        /// <param name="fileName">文件名</param>
        public void ExportDataTableToExcel(System.Data.DataTable dataTable, System.Data.DataTable dataTable2, String fileName, columnName cn)
        {
            try
            {
                SaveFileDialog saveDialog = ShowDialog(fileName);
                if (saveDialog.ShowDialog() == DialogResult.OK)
                {
                    fileName = saveDialog.FileName;
                    List<System.Data.DataTable> lt = new List<System.Data.DataTable>();
                    lt.Add(dataTable);
                    lt.Add(dataTable2);
                    NpoiManage.SaveToFile(lt, cn, true, fileName);
                    OpenExcelFile(fileName);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("ERROR MSG:" + ex.Message);
            }
        }


        /// <summary>
        /// 将DataTable导出为Excel
        /// </summary>
        /// <param name="dataTables">DataTable对象</param>
        /// <param name="fileName">文件名</param>
        public void ExportDataTableToExcel(List<System.Data.DataTable> dataTables, String fileName, columnName cn)
        {
            try
            {
                SaveFileDialog saveDialog = ShowDialog(fileName);
                if (saveDialog.ShowDialog() == DialogResult.OK)
                {
                    fileName = saveDialog.FileName;
                    NpoiManage.SaveToFile(dataTables, cn, false, fileName);
                    OpenExcelFile(fileName);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("ERROR MSG:" + ex.Message);
            }
        }

        /// <summary>
        /// 将DataTable导出为Excel
        /// </summary>
        /// <param name="dataTables"></param>
        /// <param name="imgData"></param>
        /// <param name="fileName"></param>
        public void ExportDataTableToExcel(List<DataTableData> dataTables, List<ImageData> imgData, String fileName)
        {
            try
            {
                SaveFileDialog saveDialog = ShowDialog(fileName);
                if (saveDialog.ShowDialog() == DialogResult.OK)
                {
                    fileName = saveDialog.FileName;
                    NpoiManage.SaveToFile(dataTables, imgData, fileName);
                    OpenExcelFile(fileName);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("ERROR MSG:" + ex.Message);
            }
        }

        /// <summary>
        /// 将DataTable导出为Excel
        /// </summary>
        /// <param name="dataTables"></param>
        /// <param name="imgData"></param>
        /// <param name="nullString"></param>
        /// <param name="fileName"></param>
        public void ExportDataTableToExcel(List<DataTableData> dataTables, List<ImageData> imgData, String nullString, String fileName,bool colour)
        {
            try
            {
                SaveFileDialog saveDialog = ShowDialog(fileName);
                if (saveDialog.ShowDialog() == DialogResult.OK)
                {
                    fileName = saveDialog.FileName;
                    NpoiManage.SaveToFile(dataTables, imgData, nullString, fileName, colour);
                    OpenExcelFile(fileName);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("ERROR MSG:" + ex.Message);
            }
        }
        public void ExportDataTableToExcel(List<DataTableData> dataTables, List<ImageData> imgData, String nullString, String fileName)
        {
            ExportDataTableToExcel(dataTables, imgData, nullString, fileName, false);
        }

        /// <summary>
        /// 将DataTable导出为Excel
        /// </summary>
        /// <param name="dataView"></param>
        /// <param name="headerHtml"></param>
        /// <param name="headerRowCount"></param>
        /// <param name="fileName"></param>
        public void ExportDataTableToExcel(System.Data.DataTable dataTable, String headerHtml, String nullString, String fileName,bool colour)
        {
            try
            {
                String fileRoot = Path.GetPathRoot(fileName);
                if (String.IsNullOrEmpty(fileRoot))
                {
                    SaveFileDialog saveDialog = ShowDialog(fileName);
                    if (saveDialog.ShowDialog() == DialogResult.OK)
                    {
                        fileName = saveDialog.FileName;
                        NpoiManage.SaveToFile(dataTable, headerHtml, nullString, fileName, colour);
                        OpenExcelFile(fileName);
                    }
                }
                else
                {
                    NpoiManage.SaveToFile(dataTable, headerHtml, nullString, fileName, colour);
                    OpenExcelFile(fileName);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("ERROR MSG:" + ex.Message);
            }
        }

        public void ExportDataTableToExcel(DataTableData dataTable, String headerHtml, String nullString, String fileName, bool colour)
        {
            try
            {
                String fileRoot = Path.GetPathRoot(fileName);
                if (String.IsNullOrEmpty(fileRoot))
                {
                    SaveFileDialog saveDialog = ShowDialog(fileName);
                    if (saveDialog.ShowDialog() == DialogResult.OK)
                    {
                        fileName = saveDialog.FileName;
                        NpoiManage.SaveToFile(dataTable, headerHtml, nullString, fileName, colour);
                        OpenExcelFile(fileName);
                    }
                }
                else
                {
                    NpoiManage.SaveToFile(dataTable, headerHtml, nullString, fileName, colour);
                    OpenExcelFile(fileName);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("ERROR MSG:" + ex.Message);
            }
        }

        public void ExportDataTableToExcel(System.Data.DataTable dataTable, String headerHtml, String nullString, String fileName)
        {
            ExportDataTableToExcel(dataTable, headerHtml, nullString, fileName, false);
       
        }

        /// <summary>
        /// 将DataTable导出为Excel
        /// </summary>
        /// <param name="dataTable">包含有坐标信息的datatable</param>
        /// <param name="headerHtml">多列头描述</param>
        /// <param name="image">包含有坐标信息的图片</param>
        /// <param name="fileName">路径</param>
        public void ExportDataTableToExcel(DataTableData dataTable, String headerHtml, ImageData image, String fileName,bool colour)
        {
            try
            {
                String fileRoot = Path.GetPathRoot(fileName);
                if (String.IsNullOrEmpty(fileRoot))
                {
                    SaveFileDialog saveDialog = ShowDialog(fileName);
                    if (saveDialog.ShowDialog() == DialogResult.OK)
                    {
                        //dataTable.ExportType = ExportSourceType.Edb;
                        fileName = saveDialog.FileName;
                        NpoiManage.SaveToFile(dataTable, headerHtml, image, fileName, colour);
                        OpenExcelFile(fileName);
                    }
                }
                else
                {
                    NpoiManage.SaveToFile(dataTable, headerHtml, image, fileName, colour);
                    OpenExcelFile(fileName);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("ERROR MSG:" + ex.Message);
            }
        }

        public void ExportDataTableToExcel(DataTableData dataTable, String headerHtml, ImageData image, String fileName)
        {
            ExportDataTableToExcel(dataTable, headerHtml, image, fileName, false);
        }

        #endregion

        #region 数组导出到Excel

        /// <summary>
        /// 导出数组到Excel
        /// </summary>
        /// <param name="array">数组</param>
        /// <param name="rowCount">列数</param>
        /// <param name="colCount">列数</param>
        /// <param name="fileName">文件名称</param>
        public void ExportArrayToExcel(object[,] array, int rowCount, int colCount, String fileName)
        {
            try
            {
                if (array == null)
                {
                    return;
                }
                System.Data.DataTable dt = new System.Data.DataTable();
                int rows = array.GetLength(0);
                int columns = array.GetLength(1);
                for (int i = 0; i < columns; i++)
                {
                    dt.Columns.Add("", typeof(String));
                }
                for (int i = 0; i < rows; i++)
                {
                    DataRow dr = dt.NewRow();
                    for (int j = 0; j < columns; j++)
                    {
                        dr[j] = array[i, j].ToString();
                    }
                }
                ExportDataTableToExcel(dt, null, fileName, columnName.Nono);
                //if (!codeboolisExcelInstalled())
                //{
                //    IApp app = ServiceHelper.GetService<IApp>();
                //    app.ShowMessageBox("提示", "请先安装Excel！", MessageBoxButtonType.OK, MessageBoxFormType.Right);
                //    return;
                //}
                //SaveFileDialog saveDialog = ShowDialog(fileName);
                //if (saveDialog.ShowDialog() == DialogResult.OK)
                //{
                //    fileName = saveDialog.FileName;
                //    Microsoft.Office.Interop.Excel.Application xlApp = new Microsoft.Office.Interop.Excel.Application();
                //    xlApp.Visible = false;
                //    try
                //    {
                //        IApp app = ServiceHelper.GetService<IApp>();
                //        Microsoft.Office.Interop.Excel.Workbook workbook = xlApp.Application.Workbooks.Add(System.Type.Missing);
                //        Microsoft.Office.Interop.Excel.Worksheet worksheet = (Microsoft.Office.Interop.Excel.Worksheet)workbook.Worksheets[1];
                //        Microsoft.Office.Interop.Excel.Range range;
                //        range = worksheet.get_Range(worksheet.Cells[1, 1], worksheet.Cells[rowCount + 1, colCount + 1]);
                //        //填充数据
                //        FillRange(range, array, "@", true, false);
                //        SetExcelDataSource(worksheet, rowCount + 1);
                //        xlApp.ActiveWindow.DisplayGridlines = true;
                //        xlApp.DisplayAlerts = false;
                //        workbook.SaveAs(fileName, Missing.Value, Missing.Value, Missing.Value, Missing.Value, Missing.Value, XlSaveAsAccessMode.xlNoChange, Missing.Value, Missing.Value, Missing.Value, Missing.Value, Missing.Value);
                //        workbook.Close(Missing.Value, Missing.Value, Missing.Value);
                //    }
                //    finally
                //    {
                //        xlApp.Quit();
                //        System.Runtime.InteropServices.Marshal.ReleaseComObject(xlApp);
                //    }
                //    OpenExcelFile(fileName);
                //}
            }
            catch (Exception ex)
            {
                MessageBox.Show("ERROR MSG:" + ex.Message);
            }
        }

        #endregion

        #region OpenXml导出方式

        #region DataView导出Excel

        /// <summary>
        /// XML方式导出数据
        /// </summary>
        /// <param name="gridView">数据源</param>
        /// <param name="saveFileName">导出路径</param>
        public bool ExprotToExcel(System.Data.DataView gridView, String saveFileName)
        {
            try
            {
                System.Data.DataTable[] dts = new System.Data.DataTable[1];
                dts[0] = GetDgvToTable(gridView);
                ExcelFactory ef = new ExcelFactory(saveFileName, dts, null);
                ef.CreateXlsx();
                OpenExcelFile(saveFileName);
                return true;
            }
            catch (Exception err)
            {
                return false;
            }
        }

        /// <summary>
        /// XML方式导出数据(多表头)
        /// </summary>
        /// <param name="gridView">数据源</param>
        /// <param name="saveFileName">导出路径</param>
        /// <param name="head">表头</param>
        public bool ExprotToExcel(System.Data.DataView gridView, String saveFileName, String[][] head)
        {
            try
            {
                System.Data.DataTable[] dts = new System.Data.DataTable[1];
                dts[0] = GetDgvToTable(gridView);
                ExcelFactory ef = new ExcelFactory(saveFileName, dts, null);
                ef.CreateXlsx(head);
                OpenExcelFile(saveFileName);
                return true;
            }
            catch (Exception err)
            {
                return false;
            }
        }

        /// <summary>
        /// XML方式导出数据
        /// </summary>
        /// <param name="gridView">数据源</param>
        /// <param name="saveFileName">导出路径</param>
        public bool ExprotToExcel(System.Data.DataView gridView, String saveFileName, columnName cn)
        {
            try
            {
                System.Data.DataTable[] dts = new System.Data.DataTable[1];
                dts[0] = GetDgvToTable(gridView);
                ExcelFactory ef = new ExcelFactory(saveFileName, dts, null);
                ef.CreateXlsx(cn);
                OpenExcelFile(saveFileName);
                return true;
            }
            catch (Exception err)
            {
                return false;
            }
        }

        /// <summary>
        /// XML方式导出数据(多表头)
        /// </summary>
        /// <param name="gridView">数据源</param>
        /// <param name="saveFileName">导出路径</param>
        /// <param name="head">表头</param>
        public bool ExprotToExcel(System.Data.DataView gridView, String saveFileName, String[][] head, columnName cn)
        {
            try
            {
                System.Data.DataTable[] dts = new System.Data.DataTable[1];
                dts[0] = GetDgvToTable(gridView);
                ExcelFactory ef = new ExcelFactory(saveFileName, dts, null);
                ef.CreateXlsx(cn, head);
                OpenExcelFile(saveFileName);
                return true;
            }
            catch (Exception err)
            {
                return false;
            }
        }

        #endregion

        #region DataTable导出Excel

        /// <summary>
        /// XML方式导出数据
        /// </summary>
        /// <param name="table">数据源</param>
        /// <param name="saveFileName">导出路径</param>
        public bool ExprotToExcel(System.Data.DataTable table, String saveFileName)
        {
            try
            {
                System.Data.DataTable[] dts = new System.Data.DataTable[1];
                dts[0] = table;
                ExcelFactory ef = new ExcelFactory(saveFileName, dts, null);
                ef.CreateXlsx();
                OpenExcelFile(saveFileName);
                return true;
            }
            catch (Exception err)
            {
                return false;
            }
        }

        /// <summary>
        /// XML方式导出数据(多表头)
        /// </summary>
        /// <param name="table">数据源</param>
        /// <param name="saveFileName">导出路径</param>
        /// <param name="head">表头</param>
        public bool ExprotToExcel(System.Data.DataTable table, String saveFileName, String[][] head)
        {
            try
            {
                System.Data.DataTable[] dts = new System.Data.DataTable[1];
                dts[0] = table;
                ExcelFactory ef = new ExcelFactory(saveFileName, dts, null);
                ef.CreateXlsx(head);
                OpenExcelFile(saveFileName);
                return true;
            }
            catch (Exception err)
            {
                return false;
            }
        }

        /// <summary>
        /// XML方式导出数据
        /// </summary>
        /// <param name="table">数据源</param>
        /// <param name="saveFileName">导出路径</param>
        public bool ExprotToExcel(System.Data.DataTable table, String saveFileName, columnName cn)
        {
            try
            {
                System.Data.DataTable[] dts = new System.Data.DataTable[1];
                dts[0] = table;
                ExcelFactory ef = new ExcelFactory(saveFileName, dts, null);
                ef.CreateXlsx(cn);
                OpenExcelFile(saveFileName);
                return true;
            }
            catch (Exception err)
            {
                return false;
            }
        }

        /// <summary>
        /// XML方式导出数据(多表头)
        /// </summary>
        /// <param name="table">数据源</param>
        /// <param name="saveFileName">导出路径</param>
        /// <param name="head">表头</param>
        public bool ExprotToExcel(System.Data.DataTable table, String saveFileName, String[][] head, columnName cn)
        {
            try
            {
                System.Data.DataTable[] dts = new System.Data.DataTable[1];
                dts[0] = table;
                ExcelFactory ef = new ExcelFactory(saveFileName, dts, null);
                ef.CreateXlsx(cn, head);
                OpenExcelFile(saveFileName);
                return true;
            }
            catch (Exception err)
            {
                return false;
            }
        }

        /// <summary>
        /// XML方式导出数据(单表头)
        /// </summary>
        /// <param name="table">数据源</param>
        /// <param name="saveFileName">导出路径</param>
        /// <param name="head">表头</param>
        public bool ExprotToExcel(System.Data.DataTable table, String saveFileName, List<String> head, columnName cn, Image img, int h, int w)
        {
            try
            {
                System.Data.DataTable[] dts = new System.Data.DataTable[1];
                if (table != null)
                {
                    if (table.TableName.Trim() == "")
                    {
                        table.TableName = "sheet1";
                    }
                    dts[0] = table;
                }
                else
                {
                    System.Data.DataTable dtTemp = new System.Data.DataTable("sheet1");
                    DataColumn x0 = new DataColumn(" ", typeof(String));
                    dtTemp.Columns.Add(x0);
                    dts[0] = dtTemp;
                }

                Image img2 = null;
                Graphics gh = null;
                if (img != null)
                {
                    img2 = new Bitmap(w, h);
                    gh = Graphics.FromImage(img2);
                    gh.DrawImage(img, new System.Drawing.Rectangle(0, 0, w, h));
                }
                ExcelFactory ef = new ExcelFactory(saveFileName, dts, img2);
                if (head != null)
                {
                    List<String[]> s = new List<String[]>();
                    s.Add(head.ToArray());
                    ef.CreateXlsx(cn, s.ToArray());
                }
                else
                {
                    ef.CreateXlsx(cn, null);
                }
                if (gh != null)
                {
                    gh.Dispose();
                    gh = null;
                }
                OpenExcelFile(saveFileName);
                return true;
            }
            catch (Exception err)
            {
                return false;
            }
        }

        /// <summary>
        /// XML方式导出数据(多表头)
        /// </summary>
        /// <param name="table">数据源</param>
        /// <param name="saveFileName">导出路径</param>
        /// <param name="head">表头</param>
        public bool ExprotToExcel(System.Data.DataTable table, System.Data.DataTable table2, String saveFileName, String[][] head, columnName cn)
        {
            try
            {
                System.Data.DataTable[] dts = new System.Data.DataTable[2];
                dts[0] = table;
                dts[1] = table2;
                ExcelFactory ef = new ExcelFactory(saveFileName, dts, null);
                ef.CreateXlsx(true, cn, head);
                OpenExcelFile(saveFileName);
                return true;
            }
            catch (Exception err)
            {
                return false;
            }
        }

        /// <summary>
        /// XML方式导出数据(多表头)
        /// </summary>
        /// <param name="table">数据源</param>
        /// <param name="saveFileName">导出路径</param>
        public bool ExprotToExcel(System.Data.DataTable[] dts, String saveFileName, columnName cn)
        {
            try
            {
                ExcelFactory ef = new ExcelFactory(saveFileName, dts, null);
                ef.CreateXlsx(cn);
                return true;
            }
            catch (Exception err)
            {
                return false;
            }
        }

        #endregion

        #endregion

        #region 导出公式注释到Excel
        private Microsoft.Office.Interop.Excel.Application GetExcelInstance()
        {
            Microsoft.Office.Interop.Excel.Application instance = null;

            try
            {
                instance = (Microsoft.Office.Interop.Excel.Application)System.Runtime.InteropServices.Marshal.GetActiveObject("Excel.Application");
            }
            catch (System.Runtime.InteropServices.COMException ex)
            {
                instance = new Microsoft.Office.Interop.Excel.Application();
            }

            return instance;
        }

        private bool AddComent(Microsoft.Office.Interop.Excel.Application myExcel, object coment, int row, int column)
        {
            try
            {
                Microsoft.Office.Interop.Excel.Range range = myExcel.get_Range(myExcel.Cells[row, column], myExcel.Cells[row, column]);

                if (range.Comment != null)
                {
                    range.Comment.Delete();
                }
                range.AddComment(coment);
                return true;
            }
            catch
            {
                return false;
            }
        }
        #endregion

        #endregion

        #region 读取Excel到DataTable

        /// <summary>
        /// 读取Excel到DataSet
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        public DataSet ReadExcelToDataSet(String filePath)
        {
            DataSet ds = null;
            try
            {
                ds = NpoiManage.ReadExcelToDataSet(filePath);
            }
            catch (Exception ex)
            {
            }
            return ds;
        }

        /// <summary>
        /// 读取Excel到DataTable
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        public System.Data.DataTable ReadExcelToDataTable(String filePath)
        {
            DataSet ds = ReadExcelToDataSet(filePath);
            if (ds != null && ds.Tables.Count > 0)
            {
                return ds.Tables[0];
            }
            return null;
        }

        #endregion
    }

    #region OpenXml
    public class ExcelFactory
    {
        #region 汤文 2013/4/2

        #region 属性
        private String root;
        private String xlsxTemp;
        //外层文件夹
        private String _rels;
        private String docProps;
        private String xl;
        private String Content_Types;

        //_rels文件夹
        private String _rels_rels;
        //docProps文件夹
        private String docProps_app;
        private String docProps_core;
        //xl文件夹
        private String xl_rels;
        private String xl_theme;
        private String xl_worksheets;
        private String xl_printerSettings;
        private String xl_styles;
        private String xl_workbook;
        private String xl_sharedStrings;
        private String xl_drawings;
        private String xl_media;

        //xl中_rels文件夹
        private String xl_rels_workbook;
        //xl中theme文件夹
        private String xl_theme_theme1;
        //xl中worksheets文件夹
        private List<String> xl_worksheets_sheet;

        //xl中printerSettings文件夹
        private String xl_printerSettings_printerSettings1;

        //xl中的drawings文件夹
        private String xl_drawings_drawing1;
        //xl_drawings中的_rels文件夹
        private String xl_drawings_rels;
        private String xl_drawings_rels_drawing1;

        //xl中的media文件夹
        private String xl_media_image1;

        //xl中worksheets中_rels文件夹
        private String xl_worksheets_rels;
        //worksheets中sheet1.xml.rels
        private String xl_worksheets_sheet1_xml_rels;

        /// <summary>
        /// 需要创建的文件夹
        /// </summary>
        private List<String> createDirList;
        /// <summary>
        /// 列宽
        /// </summary>
        private List<int> colunmWith;
        /// <summary>
        /// 数据
        /// </summary>
        private System.Data.DataTable[] dts;


        /// <summary>
        /// 图片
        /// </summary>
        private Image image;
        #endregion

        #region 构造函数
        public ExcelFactory(String filePath, System.Data.DataTable[] dts, Image img)
        {
            Initialize(filePath, dts, img);
        }
        #endregion

        #region 初始化

        /// <summary>
        /// 初始化
        /// </summary>
        private void Initialize(String filePath, System.Data.DataTable[] dts, Image img)
        {
            InitializeData(dts, img);
            CheckTableNames();
            InitializePath(filePath);
            InitializeDir();
            Initialize_xl_media_image1(img);
            Initialize_Content_Types();
            Initialize_rels_rels();
            Initialize_docProps_app();
            Initialize_docProps_core();
            Initialize_xl_styles();
            Initialize_xl_workbook();
            Initialize_xl_sharedStrings();
            Initialize_xl_rels_workbook();
            Initialize_xl_theme_theme1();
            Initialize_xl_printerSettings_printerSettings1();
            Initialize_xl_worksheets_sheet1_xml_rels();
            Initialize_xl_worksheets_sheet();
            Initialize_xl_drawings_drawing1();
            Initialize_xl_drawings_rels_drawing1();
        }
        /// <summary>
        /// 是否有重复的名字
        /// </summary>
        /// <param name="tableNames"></param>
        /// <returns></returns>
        private void CheckTableNames()
        {
            Dictionary<String, byte> dy = new Dictionary<String, byte>();
            int num = 1;
            for (int i = 0; i < dts.Length; i++)
            {
                if (dy.ContainsKey(dts[i].TableName))
                {
                    dts[i].TableName = String.Format("{0}{1}", dts[i].TableName, num);
                    dy.Add(dts[i].TableName, 0);
                    num++;
                }
                else
                {
                    num = 1;
                    dy.Add(dts[i].TableName, 0);
                }
            }
        }
        /// <summary>
        /// 初始化数据
        /// </summary>
        /// <param name="dts"></param>
        private void InitializeData(System.Data.DataTable[] dts, Image img)
        {
            this.dts = dts;
            this.image = img;
        }
        /// <summary>
        /// 初始化路径
        /// </summary>
        /// <param name="filePath">文件路径</param>
        private void InitializePath(String filePath)
        {
            xlsxTemp = Path.Combine(Path.GetDirectoryName(filePath), "TempOpenXml");
            createDirList = new List<String>();
            xl_worksheets_sheet = new List<String>();
            root = filePath;
            createDirList.Add(xlsxTemp);
            //外层文件夹
            _rels = Path.Combine(xlsxTemp, "_rels");
            createDirList.Add(_rels);
            docProps = Path.Combine(xlsxTemp, "docProps");
            createDirList.Add(docProps);
            xl = Path.Combine(xlsxTemp, "xl");
            createDirList.Add(xl);
            Content_Types = Path.Combine(xlsxTemp, "[Content_Types].xml"); //

            //_rels文件夹
            _rels_rels = Path.Combine(_rels, ".rels");//
            //docProps文件夹
            docProps_app = Path.Combine(docProps, "app.xml");//
            docProps_core = Path.Combine(docProps, "core.xml");//

            //xl文件夹
            xl_rels = Path.Combine(xl, "_rels");
            createDirList.Add(xl_rels);
            xl_theme = Path.Combine(xl, "theme");
            createDirList.Add(xl_theme);
            xl_worksheets = Path.Combine(xl, "worksheets");
            createDirList.Add(xl_worksheets);
            xl_printerSettings = Path.Combine(xl, "printerSettings");
            createDirList.Add(xl_printerSettings);
            //xl文件夹中drawings文件夹
            xl_drawings = Path.Combine(xl, "drawings");
            createDirList.Add(xl_drawings);
            //drawings文件夹中_rels文件夹
            xl_drawings_rels = Path.Combine(xl_drawings, "_rels");
            createDirList.Add(xl_drawings_rels);
            xl_drawings_drawing1 = Path.Combine(xl_drawings, "drawing1.xml");
            xl_drawings_rels_drawing1 = Path.Combine(xl_drawings_rels, "drawing1.xml.rels");

            //xl文件夹中media文件夹
            xl_media = Path.Combine(xl, "media");
            createDirList.Add(xl_media);
            xl_media_image1 = Path.Combine(xl_media, "image1.jpg");

            xl_styles = Path.Combine(xl, "styles.xml"); //
            xl_workbook = Path.Combine(xl, "workbook.xml");//
            xl_sharedStrings = Path.Combine(xl, "sharedStrings.xml");//

            //xl中_rels文件夹
            xl_rels_workbook = Path.Combine(xl_rels, "workbook.xml.rels");//
            //xl中theme文件夹
            xl_theme_theme1 = Path.Combine(xl_theme, "theme1.xml");//
            //xl中worksheets文件夹
            if (dts != null)
            {
                for (int i = 0; i < dts.Length; i++)
                {
                    xl_worksheets_sheet.Add(Path.Combine(xl_worksheets, String.Format("sheet{0}.xml", i + 1)));
                }
            }

            //xl中printerSettings文件夹
            xl_printerSettings_printerSettings1 = Path.Combine(xl_printerSettings, "printerSettings1.bin");//;
            //xl中worksheets中_rels文件夹
            xl_worksheets_rels = Path.Combine(xl_worksheets, "_rels");
            createDirList.Add(xl_worksheets_rels);
            //worksheets中sheet1.xml.rels
            xl_worksheets_sheet1_xml_rels = Path.Combine(xl_worksheets_rels, "sheet1.xml.rels");//;
        }
        /// <summary>
        /// 初始化文件夹
        /// </summary>
        private void InitializeDir()
        {
            if (Directory.Exists(xlsxTemp))
            {
                Directory.Delete(xlsxTemp, true);
                Directory.CreateDirectory(xlsxTemp);
            }
            if (createDirList != null)
            {
                foreach (String str in createDirList)
                {
                    if (!Directory.Exists(str))
                    {
                        Directory.CreateDirectory(str);
                    }
                }
            }
        }
        /// <summary>
        /// Content_Types
        /// </summary>
        private void Initialize_Content_Types()
        {
            if (dts != null)
            {
                StringBuilder sb = new StringBuilder();
                sb.Append("<?xml version=\"1.0\" encoding=\"UTF-8\" standalone=\"yes\"?>");
                sb.Append("<Types xmlns=\"http://schemas.openxmlformats.org/package/2006/content-types\">");
                sb.Append("<Default Extension=\"bin\" ContentType=\"application/vnd.openxmlformats-officedocument.spreadsheetml.printerSettings\"/>");
                sb.Append("<Default Extension=\"rels\" ContentType=\"application/vnd.openxmlformats-package.relationships+xml\"/>");
                sb.Append("<Default Extension=\"xml\" ContentType=\"application/xml\"/>");
                sb.Append("<Default Extension=\"jpg\" ContentType=\"image/jpeg\"/>");
                sb.Append("<Override PartName=\"/xl/workbook.xml\" ContentType=\"application/vnd.openxmlformats-officedocument.spreadsheetml.sheet.main+xml\"/>");
                for (int i = 0; i < dts.Length; i++)
                {
                    sb.Append(String.Format("<Override PartName=\"/xl/worksheets/sheet{0}.xml\" ContentType=\"application/vnd.openxmlformats-officedocument.spreadsheetml.worksheet+xml\"/>", i + 1));
                    //sb.Append("<Override PartName=\"/xl/worksheets/sheet2.xml\" ContentType=\"application/vnd.openxmlformats-officedocument.spreadsheetml.worksheet+xml\"/>");
                    //sb.Append("<Override PartName=\"/xl/worksheets/sheet3.xml\" ContentType=\"application/vnd.openxmlformats-officedocument.spreadsheetml.worksheet+xml\"/>");
                }
                sb.Append("<Override PartName=\"/xl/theme/theme1.xml\" ContentType=\"application/vnd.openxmlformats-officedocument.theme+xml\"/>");
                sb.Append("<Override PartName=\"/xl/styles.xml\" ContentType=\"application/vnd.openxmlformats-officedocument.spreadsheetml.styles+xml\"/>");
                sb.Append("<Override PartName=\"/xl/sharedStrings.xml\" ContentType=\"application/vnd.openxmlformats-officedocument.spreadsheetml.sharedStrings+xml\"/>");
                sb.Append("<Override PartName=\"/xl/drawings/drawing1.xml\" ContentType=\"application/vnd.openxmlformats-officedocument.drawing+xml\"/>");
                sb.Append("<Override PartName=\"/docProps/core.xml\" ContentType=\"application/vnd.openxmlformats-package.core-properties+xml\"/>");
                sb.Append("<Override PartName=\"/docProps/app.xml\" ContentType=\"application/vnd.openxmlformats-officedocument.extended-properties+xml\"/>");
                sb.Append("</Types>");
                File.WriteAllText(Content_Types, Convert.ToString(sb));
            }
        }
        /// <summary>
        /// rels_rels
        /// </summary>
        private void Initialize_rels_rels()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("<?xml version=\"1.0\" encoding=\"UTF-8\" standalone=\"yes\"?>");
            sb.Append("<Relationships xmlns=\"http://schemas.openxmlformats.org/package/2006/relationships\">");
            sb.Append("<Relationship Id=\"rId3\" Type=\"http://schemas.openxmlformats.org/officeDocument/2006/relationships/extended-properties\" Target=\"docProps/app.xml\"/>");
            sb.Append("<Relationship Id=\"rId2\" Type=\"http://schemas.openxmlformats.org/package/2006/relationships/metadata/core-properties\" Target=\"docProps/core.xml\"/>");
            sb.Append("<Relationship Id=\"rId1\" Type=\"http://schemas.openxmlformats.org/officeDocument/2006/relationships/officeDocument\" Target=\"xl/workbook.xml\"/>");
            sb.Append("</Relationships>");
            File.WriteAllText(_rels_rels, Convert.ToString(sb));
        }
        /// <summary>
        /// docProps_app
        /// </summary>
        private void Initialize_docProps_app()
        {
            if (dts != null)
            {
                StringBuilder sb = new StringBuilder();
                sb.Append("<?xml version=\"1.0\" encoding=\"UTF-8\" standalone=\"yes\"?>");
                sb.Append("<Properties xmlns=\"http://schemas.openxmlformats.org/officeDocument/2006/extended-properties\" xmlns:vt=\"http://schemas.openxmlformats.org/officeDocument/2006/docPropsVTypes\">");
                sb.Append("<Application>Microsoft Excel</Application>");
                sb.Append("<DocSecurity>0</DocSecurity>");
                sb.Append("<ScaleCrop>false</ScaleCrop>");
                sb.Append("<HeadingPairs>");
                sb.Append("<vt:vector size=\"2\" baseType=\"variant\">");
                sb.Append("<vt:variant>");
                sb.Append("<vt:lpstr>工作表</vt:lpstr></vt:variant>");
                sb.Append("<vt:variant>");
                sb.Append(String.Format("<vt:i4>{0}</vt:i4>", dts.Length));
                sb.Append("</vt:variant>");
                sb.Append("</vt:vector>");
                sb.Append("</HeadingPairs>");
                sb.Append("<TitlesOfParts>");
                sb.Append(String.Format("<vt:vector size=\"{0}\" baseType=\"lpstr\">", dts.Length));
                for (int i = 0; i < dts.Length; i++)
                {
                    sb.Append(String.Format("<vt:lpstr>Sheet{0}</vt:lpstr>", i + 1));
                    //sb.Append("<vt:lpstr>Sheet2</vt:lpstr>");
                    //sb.Append("<vt:lpstr>Sheet3</vt:lpstr>");
                }
                sb.Append("</vt:vector>");
                sb.Append("</TitlesOfParts>");
                sb.Append("<Company></Company>");
                sb.Append("<LinksUpToDate>false</LinksUpToDate>");
                sb.Append("<SharedDoc>false</SharedDoc>");
                sb.Append("<HyperlinksChanged>false</HyperlinksChanged>");
                sb.Append("<AppVersion>14.0300</AppVersion>");
                sb.Append("</Properties>");
                File.WriteAllText(docProps_app, Convert.ToString(sb));
            }
        }
        /// <summary>
        /// docProps_core
        /// </summary>
        private void Initialize_docProps_core()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("<?xml version=\"1.0\" encoding=\"UTF-8\" standalone=\"yes\"?>");
            sb.Append("<cp:coreProperties xmlns:cp=\"http://schemas.openxmlformats.org/package/2006/metadata/core-properties\" xmlns:dc=\"http://purl.org/dc/elements/1.1/\" xmlns:dcterms=\"http://purl.org/dc/terms/\" xmlns:dcmitype=\"http://purl.org/dc/dcmitype/\" xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\">");
            sb.Append("<dc:creator></dc:creator>");
            sb.Append("<dcterms:created xsi:type=\"dcterms:W3CDTF\">2006-09-16T00:00:00Z</dcterms:created>");
            sb.Append("<dcterms:modified xsi:type=\"dcterms:W3CDTF\">2013-04-12T05:35:16Z</dcterms:modified>");
            sb.Append("</cp:coreProperties>");
            File.WriteAllText(docProps_core, Convert.ToString(sb));
        }
        /// <summary>
        /// xl_styles
        /// </summary>
        private void Initialize_xl_styles()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("<?xml version=\"1.0\" encoding=\"UTF-8\" standalone=\"yes\"?>");
            sb.Append("<styleSheet xmlns=\"http://schemas.openxmlformats.org/spreadsheetml/2006/main\" xmlns:mc=\"http://schemas.openxmlformats.org/markup-compatibility/2006\" mc:Ignorable=\"x14ac\" xmlns:x14ac=\"http://schemas.microsoft.com/office/spreadsheetml/2009/9/ac\">");
            sb.Append("<fonts count=\"4\" x14ac:knownFonts=\"1\">");
            sb.Append("<font>");
            sb.Append("<sz val=\"11\"/>");
            sb.Append("<color theme=\"1\"/>");
            sb.Append("<name val=\"宋体\"/><family val=\"2\"/>");
            sb.Append("<scheme val=\"minor\"/>");
            sb.Append("</font>");
            sb.Append("<font>");
            sb.Append("<sz val=\"9\"/>");
            sb.Append("<name val=\"宋体\"/><family val=\"3\"/>");
            sb.Append("<charset val=\"134\"/>");
            sb.Append("<scheme val=\"minor\"/>");
            sb.Append("</font>");
            sb.Append("<font>");
            sb.Append("<b/>");
            sb.Append("<sz val=\"11\"/>");
            sb.Append("<color theme=\"1\"/>");
            sb.Append("<name val=\"宋体\"/><family val=\"3\"/>");
            sb.Append("<charset val=\"134\"/>");
            sb.Append("<scheme val=\"minor\"/>");
            sb.Append("</font>");
            sb.Append("<font>");
            sb.Append("<b/>");
            sb.Append("<sz val=\"11\"/>");
            sb.Append("<color rgb=\"FFFF0000\"/>");
            sb.Append("<name val=\"宋体\"/><family val=\"3\"/>");
            sb.Append("<charset val=\"134\"/>");
            sb.Append("<scheme val=\"minor\"/>");
            sb.Append("</font>");
            sb.Append("</fonts>");
            sb.Append("<fills count=\"2\">");
            sb.Append("<fill>");
            sb.Append("<patternFill patternType=\"none\"/>");
            sb.Append("</fill>");
            sb.Append("<fill>");
            sb.Append("<patternFill patternType=\"gray125\"/>");
            sb.Append("</fill>");
            sb.Append("</fills>");
            sb.Append("<borders count=\"1\">");
            sb.Append("<border>");
            sb.Append("<left/>");
            sb.Append("<right/>");
            sb.Append("<top/>");
            sb.Append("<bottom/>");
            sb.Append("<diagonal/>");
            sb.Append("</border>");
            sb.Append("</borders>");
            sb.Append("<cellStyleXfs count=\"1\">");
            sb.Append("<xf numFmtId=\"0\" fontId=\"0\" fillId=\"0\" borderId=\"0\"/>");
            sb.Append("</cellStyleXfs>");
            sb.Append("<cellXfs count=\"4\">");
            sb.Append("<xf numFmtId=\"0\" fontId=\"0\" fillId=\"0\" borderId=\"0\" xfId=\"0\"/>");
            sb.Append("<xf numFmtId=\"0\" fontId=\"2\" fillId=\"0\" borderId=\"0\" xfId=\"0\" applyFont=\"1\"/>");
            sb.Append("<xf numFmtId=\"0\" fontId=\"3\" fillId=\"0\" borderId=\"0\" xfId=\"0\" applyFont=\"1\"/>");
            sb.Append("<xf numFmtId=\"0\" fontId=\"2\" fillId=\"0\" borderId=\"0\" xfId=\"0\" applyFont=\"1\" applyAlignment=\"1\">");
            sb.Append("<alignment wrapText=\"1\"/>");
            sb.Append("</xf>");
            sb.Append("</cellXfs>");
            sb.Append("<cellStyles count=\"1\">");
            sb.Append("<cellStyle name=\"常规\" xfId=\"0\" builtinId=\"0\"/>");
            sb.Append("</cellStyles>");
            sb.Append("<dxfs count=\"0\"/>");
            sb.Append("<tableStyles count=\"0\" defaultTableStyle=\"TableStyleMedium2\" defaultPivotStyle=\"PivotStyleMedium9\"/>");
            sb.Append("<extLst>");
            sb.Append("<ext uri=\"{EB79DEF2-80B8-43e5-95BD-54CBDDF9020C}\" xmlns:x14=\"http://schemas.microsoft.com/office/spreadsheetml/2009/9/main\">");
            sb.Append("<x14:slicerStyles defaultSlicerStyle=\"SlicerStyleLight1\"/>");
            sb.Append("</ext>");
            sb.Append("</extLst>");
            sb.Append("</styleSheet>");
            File.WriteAllText(xl_styles, Convert.ToString(sb));
        }
        /// <summary>
        /// xl_workbook
        /// </summary>
        private void Initialize_xl_workbook()
        {
            if (dts != null)
            {
                StringBuilder sb = new StringBuilder();
                sb.Append("<?xml version=\"1.0\" encoding=\"UTF-8\" standalone=\"yes\"?>");
                sb.Append("<workbook xmlns=\"http://schemas.openxmlformats.org/spreadsheetml/2006/main\" xmlns:r=\"http://schemas.openxmlformats.org/officeDocument/2006/relationships\">");
                sb.Append("<fileVersion appName=\"xl\" lastEdited=\"5\" lowestEdited=\"4\" rupBuild=\"9302\"/>");
                sb.Append("<workbookPr filterPrivacy=\"1\" defaultThemeVersion=\"124226\"/>");
                sb.Append("<bookViews>");
                sb.Append("<workbookView xWindow=\"240\" yWindow=\"105\" windowWidth=\"14805\" windowHeight=\"8010\"/>");
                sb.Append("</bookViews>");
                sb.Append("<sheets>");
                for (int i = 0; i < dts.Length; i++)
                {
                    sb.Append(String.Format("<sheet name=\"{0}\" sheetId=\"{1}\" r:id=\"rId{2}\"/>", dts[i].TableName, i + 1, i + 1));
                }
                sb.Append("</sheets>");
                sb.Append("<calcPr calcId=\"122211\"/>");
                sb.Append("</workbook>");
                File.WriteAllText(xl_workbook, Convert.ToString(sb.ToString()));
            }
        }
        /// <summary>
        /// xl_rels_workbook
        /// </summary>
        private void Initialize_xl_rels_workbook()
        {
            if (dts != null)
            {
                int num = 1;
                StringBuilder sb = new StringBuilder();
                sb.Append("<?xml version=\"1.0\" encoding=\"UTF-8\" standalone=\"yes\"?>");
                sb.Append("<Relationships xmlns=\"http://schemas.openxmlformats.org/package/2006/relationships\">");
                for (int i = 0; i < dts.Length; i++)
                {
                    sb.Append(String.Format("<Relationship Id=\"rId{0}\" Type=\"http://schemas.openxmlformats.org/officeDocument/2006/relationships/worksheet\" Target=\"worksheets/sheet{1}.xml\"/>", i + 1, i + 1));
                    num++;
                }
                sb.Append(String.Format("<Relationship Id=\"rId{0}\" Type=\"http://schemas.openxmlformats.org/officeDocument/2006/relationships/sharedStrings\" Target=\"sharedStrings.xml\"/>", num + 2));
                sb.Append(String.Format("<Relationship Id=\"rId{0}\" Type=\"http://schemas.openxmlformats.org/officeDocument/2006/relationships/styles\" Target=\"styles.xml\"/>", num + 1));
                sb.Append(String.Format("<Relationship Id=\"rId{0}\" Type=\"http://schemas.openxmlformats.org/officeDocument/2006/relationships/theme\" Target=\"theme/theme1.xml\"/>", num));
                sb.Append("</Relationships>");
                File.WriteAllText(xl_rels_workbook, Convert.ToString(sb));
            }
        }
        /// <summary>
        /// xl_sharedStrings
        /// </summary>
        private void Initialize_xl_sharedStrings()
        {

        }
        /// <summary>
        /// xl_theme_theme1
        /// </summary>
        private void Initialize_xl_theme_theme1()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("<?xml version=\"1.0\" encoding=\"UTF-8\" standalone=\"yes\"?>");
            sb.Append("<a:theme xmlns:a=\"http://schemas.openxmlformats.org/drawingml/2006/main\" name=\"Office 主题​​\">");
            sb.Append("<a:themeElements>");
            sb.Append("<a:clrScheme name=\"Office\">");
            sb.Append("<a:dk1>");
            sb.Append("<a:sysClr val=\"windowText\" lastClr=\"000000\"/>");
            sb.Append("</a:dk1>");
            sb.Append("<a:lt1>");
            sb.Append("<a:sysClr val=\"window\" lastClr=\"FFFFFF\"/>");
            sb.Append("</a:lt1>");
            sb.Append("<a:dk2>");
            sb.Append("<a:srgbClr val=\"1F497D\"/>");
            sb.Append("</a:dk2>");
            sb.Append("<a:lt2>");
            sb.Append("<a:srgbClr val=\"EEECE1\"/>");
            sb.Append("</a:lt2>");
            sb.Append("<a:accent1>");
            sb.Append("<a:srgbClr val=\"4F81BD\"/>");
            sb.Append("</a:accent1>");
            sb.Append("<a:accent2>");
            sb.Append("<a:srgbClr val=\"C0504D\"/>");
            sb.Append("</a:accent2>");
            sb.Append("<a:accent3>");
            sb.Append("<a:srgbClr val=\"9BBB59\"/>");
            sb.Append("</a:accent3>");
            sb.Append("<a:accent4>");
            sb.Append("<a:srgbClr val=\"8064A2\"/>");
            sb.Append("</a:accent4>");
            sb.Append("<a:accent5>");
            sb.Append("<a:srgbClr val=\"4BACC6\"/>");
            sb.Append("</a:accent5>");
            sb.Append("<a:accent6>");
            sb.Append("<a:srgbClr val=\"F79646\"/>");
            sb.Append("</a:accent6>");
            sb.Append("<a:hlink>");
            sb.Append("<a:srgbClr val=\"0000FF\"/>");
            sb.Append("</a:hlink>");
            sb.Append("<a:folHlink>");
            sb.Append("<a:srgbClr val=\"800080\"/>");
            sb.Append("</a:folHlink>");
            sb.Append("</a:clrScheme>");
            sb.Append("<a:fontScheme name=\"Office\">");
            sb.Append("<a:majorFont>");
            sb.Append("<a:latin typeface=\"Cambria\"/>");
            sb.Append("<a:ea typeface=\"\"/>");
            sb.Append("<a:cs typeface=\"\"/>");
            sb.Append("<a:font script=\"Jpan\" typeface=\"ＭＳ Ｐゴシック\"/>");
            sb.Append("<a:font script=\"Hang\" typeface=\"맑은 고딕\"/>");
            sb.Append("<a:font script=\"Hans\" typeface=\"宋体\"/>");
            sb.Append("<a:font script=\"Hant\" typeface=\"新細明體\"/>");
            sb.Append("<a:font script=\"Arab\" typeface=\"Times New Roman\"/>");
            sb.Append("<a:font script=\"Hebr\" typeface=\"Times New Roman\"/>");
            sb.Append("<a:font script=\"Thai\" typeface=\"Tahoma\"/>");
            sb.Append("<a:font script=\"Ethi\" typeface=\"Nyala\"/>");
            sb.Append("<a:font script=\"Beng\" typeface=\"Vrinda\"/>");
            sb.Append("<a:font script=\"Gujr\" typeface=\"Shruti\"/>");
            sb.Append("<a:font script=\"Khmr\" typeface=\"MoolBoran\"/>");
            sb.Append("<a:font script=\"Knda\" typeface=\"Tunga\"/>");
            sb.Append("<a:font script=\"Guru\" typeface=\"Raavi\"/>");
            sb.Append("<a:font script=\"Cans\" typeface=\"Euphemia\"/>");
            sb.Append("<a:font script=\"Cher\" typeface=\"Plantagenet Cherokee\"/>");
            sb.Append("<a:font script=\"Yiii\" typeface=\"Microsoft Yi Baiti\"/>");
            sb.Append("<a:font script=\"Tibt\" typeface=\"Microsoft Himalaya\"/>");
            sb.Append("<a:font script=\"Thaa\" typeface=\"MV Boli\"/>");
            sb.Append("<a:font script=\"Deva\" typeface=\"Mangal\"/>");
            sb.Append("<a:font script=\"Telu\" typeface=\"Gautami\"/>");
            sb.Append("<a:font script=\"Taml\" typeface=\"Latha\"/>");
            sb.Append("<a:font script=\"Syrc\" typeface=\"Estrangelo Edessa\"/>");
            sb.Append("<a:font script=\"Orya\" typeface=\"Kalinga\"/>");
            sb.Append("<a:font script=\"Mlym\" typeface=\"Kartika\"/>");
            sb.Append("<a:font script=\"Laoo\" typeface=\"DokChampa\"/>");
            sb.Append("<a:font script=\"Sinh\" typeface=\"Iskoola Pota\"/>");
            sb.Append("<a:font script=\"Mong\" typeface=\"Mongolian Baiti\"/>");
            sb.Append("<a:font script=\"Viet\" typeface=\"Times New Roman\"/>");
            sb.Append("<a:font script=\"Uigh\" typeface=\"Microsoft Uighur\"/>");
            sb.Append("<a:font script=\"Geor\" typeface=\"Sylfaen\"/>");
            sb.Append("</a:majorFont>");
            sb.Append("<a:minorFont>");
            sb.Append("<a:latin typeface=\"Calibri\"/>");
            sb.Append("<a:ea typeface=\"\"/>");
            sb.Append("<a:cs typeface=\"\"/>");
            sb.Append("<a:font script=\"Jpan\" typeface=\"ＭＳ Ｐゴシック\"/>");
            sb.Append("<a:font script=\"Hang\" typeface=\"맑은 고딕\"/>");
            sb.Append("<a:font script=\"Hans\" typeface=\"宋体\"/>");
            sb.Append("<a:font script=\"Hant\" typeface=\"新細明體\"/>");
            sb.Append("<a:font script=\"Arab\" typeface=\"Arial\"/>");
            sb.Append("<a:font script=\"Hebr\" typeface=\"Arial\"/>");
            sb.Append("<a:font script=\"Thai\" typeface=\"Tahoma\"/>");
            sb.Append("<a:font script=\"Ethi\" typeface=\"Nyala\"/>");
            sb.Append("<a:font script=\"Beng\" typeface=\"Vrinda\"/>");
            sb.Append("<a:font script=\"Gujr\" typeface=\"Shruti\"/>");
            sb.Append("<a:font script=\"Khmr\" typeface=\"DaunPenh\"/>");
            sb.Append("<a:font script=\"Knda\" typeface=\"Tunga\"/>");
            sb.Append("<a:font script=\"Guru\" typeface=\"Raavi\"/>");
            sb.Append("<a:font script=\"Cans\" typeface=\"Euphemia\"/>");
            sb.Append("<a:font script=\"Cher\" typeface=\"Plantagenet Cherokee\"/>");
            sb.Append("<a:font script=\"Yiii\" typeface=\"Microsoft Yi Baiti\"/>");
            sb.Append("<a:font script=\"Tibt\" typeface=\"Microsoft Himalaya\"/>");
            sb.Append("<a:font script=\"Thaa\" typeface=\"MV Boli\"/>");
            sb.Append("<a:font script=\"Deva\" typeface=\"Mangal\"/>");
            sb.Append("<a:font script=\"Telu\" typeface=\"Gautami\"/>");
            sb.Append("<a:font script=\"Taml\" typeface=\"Latha\"/>");
            sb.Append("<a:font script=\"Syrc\" typeface=\"Estrangelo Edessa\"/>");
            sb.Append("<a:font script=\"Orya\" typeface=\"Kalinga\"/>");
            sb.Append("<a:font script=\"Mlym\" typeface=\"Kartika\"/>");
            sb.Append("<a:font script=\"Laoo\" typeface=\"DokChampa\"/>");
            sb.Append("<a:font script=\"Sinh\" typeface=\"Iskoola Pota\"/>");
            sb.Append("<a:font script=\"Mong\" typeface=\"Mongolian Baiti\"/>");
            sb.Append("<a:font script=\"Viet\" typeface=\"Arial\"/>");
            sb.Append("<a:font script=\"Uigh\" typeface=\"Microsoft Uighur\"/>");
            sb.Append("<a:font script=\"Geor\" typeface=\"Sylfaen\"/>");
            sb.Append("</a:minorFont>");
            sb.Append("</a:fontScheme>");
            sb.Append("<a:fmtScheme name=\"Office\">");
            sb.Append("<a:fillStyleLst>");
            sb.Append("<a:solidFill>");
            sb.Append("<a:schemeClr val=\"phClr\"/>");
            sb.Append("</a:solidFill>");
            sb.Append("<a:gradFill rotWithShape=\"1\">");
            sb.Append("<a:gsLst>");
            sb.Append("<a:gs pos=\"0\">");
            sb.Append("<a:schemeClr val=\"phClr\">");
            sb.Append("<a:tint val=\"50000\"/>");
            sb.Append("<a:satMod val=\"300000\"/>");
            sb.Append("</a:schemeClr>");
            sb.Append("</a:gs>");
            sb.Append("<a:gs pos=\"35000\">");
            sb.Append("<a:schemeClr val=\"phClr\">");
            sb.Append("<a:tint val=\"37000\"/>");
            sb.Append("<a:satMod val=\"300000\"/>");
            sb.Append("</a:schemeClr>");
            sb.Append("</a:gs>");
            sb.Append("<a:gs pos=\"100000\">");
            sb.Append("<a:schemeClr val=\"phClr\">");
            sb.Append("<a:tint val=\"15000\"/>");
            sb.Append("<a:satMod val=\"350000\"/>");
            sb.Append("</a:schemeClr>");
            sb.Append("</a:gs>");
            sb.Append("</a:gsLst>");
            sb.Append("<a:lin ang=\"16200000\" scaled=\"1\"/>");
            sb.Append("</a:gradFill>");
            sb.Append("<a:gradFill rotWithShape=\"1\">");
            sb.Append("<a:gsLst>");
            sb.Append("<a:gs pos=\"0\">");
            sb.Append("<a:schemeClr val=\"phClr\">");
            sb.Append("<a:shade val=\"51000\"/>");
            sb.Append("<a:satMod val=\"130000\"/>");
            sb.Append("</a:schemeClr>");
            sb.Append("</a:gs>");
            sb.Append("<a:gs pos=\"80000\">");
            sb.Append("<a:schemeClr val=\"phClr\">");
            sb.Append("<a:shade val=\"93000\"/>");
            sb.Append("<a:satMod val=\"130000\"/>");
            sb.Append("</a:schemeClr>");
            sb.Append("</a:gs>");
            sb.Append("<a:gs pos=\"100000\">");
            sb.Append("<a:schemeClr val=\"phClr\">");
            sb.Append("<a:shade val=\"94000\"/>");
            sb.Append("<a:satMod val=\"135000\"/>");
            sb.Append("</a:schemeClr>");
            sb.Append("</a:gs>");
            sb.Append("</a:gsLst>");
            sb.Append("<a:lin ang=\"16200000\" scaled=\"0\"/>");
            sb.Append("</a:gradFill>");
            sb.Append("</a:fillStyleLst>");
            sb.Append("<a:lnStyleLst>");
            sb.Append("<a:ln w=\"9525\" cap=\"flat\" cmpd=\"sng\" algn=\"ctr\">");
            sb.Append("<a:solidFill>");
            sb.Append("<a:schemeClr val=\"phClr\">");
            sb.Append("<a:shade val=\"95000\"/>");
            sb.Append("<a:satMod val=\"105000\"/>");
            sb.Append("</a:schemeClr>");
            sb.Append("</a:solidFill>");
            sb.Append("<a:prstDash val=\"solid\"/>");
            sb.Append("</a:ln>");
            sb.Append("<a:ln w=\"25400\" cap=\"flat\" cmpd=\"sng\" algn=\"ctr\">");
            sb.Append("<a:solidFill>");
            sb.Append("<a:schemeClr val=\"phClr\"/>");
            sb.Append("</a:solidFill>");
            sb.Append("<a:prstDash val=\"solid\"/>");
            sb.Append("</a:ln>");
            sb.Append("<a:ln w=\"38100\" cap=\"flat\" cmpd=\"sng\" algn=\"ctr\">");
            sb.Append("<a:solidFill>");
            sb.Append("<a:schemeClr val=\"phClr\"/>");
            sb.Append("</a:solidFill>");
            sb.Append("<a:prstDash val=\"solid\"/>");
            sb.Append("</a:ln>");
            sb.Append("</a:lnStyleLst>");
            sb.Append("<a:effectStyleLst>");
            sb.Append("<a:effectStyle>");
            sb.Append("<a:effectLst>");
            sb.Append("<a:outerShdw blurRad=\"40000\" dist=\"20000\" dir=\"5400000\" rotWithShape=\"0\">");
            sb.Append("<a:srgbClr val=\"000000\">");
            sb.Append("<a:alpha val=\"38000\"/>");
            sb.Append("</a:srgbClr>");
            sb.Append("</a:outerShdw>");
            sb.Append("</a:effectLst>");
            sb.Append("</a:effectStyle>");
            sb.Append("<a:effectStyle>");
            sb.Append("<a:effectLst>");
            sb.Append("<a:outerShdw blurRad=\"40000\" dist=\"23000\" dir=\"5400000\" rotWithShape=\"0\">");
            sb.Append("<a:srgbClr val=\"000000\">");
            sb.Append("<a:alpha val=\"35000\"/>");
            sb.Append("</a:srgbClr>");
            sb.Append("</a:outerShdw>");
            sb.Append("</a:effectLst>");
            sb.Append("</a:effectStyle>");
            sb.Append("<a:effectStyle>");
            sb.Append("<a:effectLst>");
            sb.Append("<a:outerShdw blurRad=\"40000\" dist=\"23000\" dir=\"5400000\" rotWithShape=\"0\">");
            sb.Append("<a:srgbClr val=\"000000\">");
            sb.Append("<a:alpha val=\"35000\"/>");
            sb.Append("</a:srgbClr>");
            sb.Append("</a:outerShdw>");
            sb.Append("</a:effectLst>");
            sb.Append("<a:scene3d>");
            sb.Append("<a:camera prst=\"orthographicFront\">");
            sb.Append("<a:rot lat=\"0\" lon=\"0\" rev=\"0\"/>");
            sb.Append("</a:camera>");
            sb.Append("<a:lightRig rig=\"threePt\" dir=\"t\">");
            sb.Append("<a:rot lat=\"0\" lon=\"0\" rev=\"1200000\"/>");
            sb.Append("</a:lightRig>");
            sb.Append("</a:scene3d>");
            sb.Append("<a:sp3d>");
            sb.Append("<a:bevelT w=\"63500\" h=\"25400\"/>");
            sb.Append("</a:sp3d>");
            sb.Append("</a:effectStyle>");
            sb.Append("</a:effectStyleLst>");
            sb.Append("<a:bgFillStyleLst>");
            sb.Append("<a:solidFill>");
            sb.Append("<a:schemeClr val=\"phClr\"/>");
            sb.Append("</a:solidFill>");
            sb.Append("<a:gradFill rotWithShape=\"1\">");
            sb.Append("<a:gsLst>");
            sb.Append("<a:gs pos=\"0\">");
            sb.Append("<a:schemeClr val=\"phClr\">");
            sb.Append("<a:tint val=\"40000\"/>");
            sb.Append("<a:satMod val=\"350000\"/>");
            sb.Append("</a:schemeClr>");
            sb.Append("</a:gs>");
            sb.Append("<a:gs pos=\"40000\">");
            sb.Append("<a:schemeClr val=\"phClr\">");
            sb.Append("<a:tint val=\"45000\"/>");
            sb.Append("<a:shade val=\"99000\"/>");
            sb.Append("<a:satMod val=\"350000\"/>");
            sb.Append("</a:schemeClr>");
            sb.Append("</a:gs>");
            sb.Append("<a:gs pos=\"100000\">");
            sb.Append("<a:schemeClr val=\"phClr\">");
            sb.Append("<a:shade val=\"20000\"/>");
            sb.Append("<a:satMod val=\"255000\"/>");
            sb.Append("</a:schemeClr>");
            sb.Append("</a:gs>");
            sb.Append("</a:gsLst>");
            sb.Append("<a:path path=\"circle\">");
            sb.Append("<a:fillToRect l=\"50000\" t=\"-80000\" r=\"50000\" b=\"180000\"/>");
            sb.Append("</a:path>");
            sb.Append("</a:gradFill>");
            sb.Append("<a:gradFill rotWithShape=\"1\">");
            sb.Append("<a:gsLst>");
            sb.Append("<a:gs pos=\"0\">");
            sb.Append("<a:schemeClr val=\"phClr\">");
            sb.Append("<a:tint val=\"80000\"/>");
            sb.Append("<a:satMod val=\"300000\"/>");
            sb.Append("</a:schemeClr>");
            sb.Append("</a:gs>");
            sb.Append("<a:gs pos=\"100000\">");
            sb.Append("<a:schemeClr val=\"phClr\">");
            sb.Append("<a:shade val=\"30000\"/>");
            sb.Append("<a:satMod val=\"200000\"/>");
            sb.Append("</a:schemeClr>");
            sb.Append("</a:gs>");
            sb.Append("</a:gsLst>");
            sb.Append("<a:path path=\"circle\">");
            sb.Append("<a:fillToRect l=\"50000\" t=\"50000\" r=\"50000\" b=\"50000\"/>");
            sb.Append("</a:path>");
            sb.Append("</a:gradFill>");
            sb.Append("</a:bgFillStyleLst>");
            sb.Append("</a:fmtScheme>");
            sb.Append("</a:themeElements>");
            sb.Append("<a:objectDefaults/>");
            sb.Append("<a:extraClrSchemeLst/>");
            sb.Append("</a:theme>");
            File.WriteAllText(xl_theme_theme1, Convert.ToString(sb));
        }
        /// <summary>
        /// xl_printerSettings_printerSettings1
        /// </summary>
        private void Initialize_xl_printerSettings_printerSettings1()
        {
            File.WriteAllText(xl_printerSettings_printerSettings1, @"                                                                       	 ?4d   X  X   A 4                                                                                                           ");
        }
        /// <summary>
        /// xl_worksheets_sheet1_xml_rels
        /// </summary>
        private void Initialize_xl_worksheets_sheet1_xml_rels()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("<?xml version=\"1.0\" encoding=\"UTF-8\" standalone=\"yes\"?>");
            sb.Append("<Relationships xmlns=\"http://schemas.openxmlformats.org/package/2006/relationships\">");
            sb.Append("<Relationship Id=\"rId1\" Type=\"http://schemas.openxmlformats.org/officeDocument/2006/relationships/drawing\" Target=\"../drawings/drawing1.xml\"/>");
            sb.Append("</Relationships>");
            File.WriteAllText(xl_worksheets_sheet1_xml_rels, Convert.ToString(sb));
        }
        /// <summary>
        /// xl_worksheets_sheet1
        /// </summary>
        private void Initialize_xl_worksheets_sheet()
        {
            if (dts != null)
            {
                for (int i = 0; i < dts.Length; i++)
                {
                    StringBuilder sb = new StringBuilder();
                    sb.Append("<?xml version=\"1.0\" encoding=\"UTF-8\" standalone=\"yes\"?>");
                    sb.Append("<worksheet xmlns=\"http://schemas.openxmlformats.org/spreadsheetml/2006/main\" xmlns:r=\"http://schemas.openxmlformats.org/officeDocument/2006/relationships\" xmlns:mc=\"http://schemas.openxmlformats.org/markup-compatibility/2006\" mc:Ignorable=\"x14ac\" xmlns:x14ac=\"http://schemas.microsoft.com/office/spreadsheetml/2009/9/ac\">");
                    sb.Append("<dimension ref=\"A1\"/>");
                    sb.Append("<sheetViews>");
                    sb.Append("<sheetView workbookViewId=\"0\"/>");
                    sb.Append("</sheetViews>");
                    sb.Append("<sheetFormatPr defaultRowHeight=\"13.5\" x14ac:dyDescent=\"0.15\"/>");
                    sb.Append("<sheetData/>");
                    sb.Append("<phoneticPr fontId=\"1\" type=\"noConversion\"/>");
                    sb.Append("<pageMargins left=\"0.7\" right=\"0.7\" top=\"0.75\" bottom=\"0.75\" header=\"0.3\" footer=\"0.3\"/>");
                    sb.Append("</worksheet>");
                    File.WriteAllText(Path.Combine(xl_worksheets, String.Format("sheet{0}.xml", i + 1)), Convert.ToString(sb));
                }
            }
        }
        /// <summary>
        /// xl_drawings_drawing1
        /// </summary>
        private void Initialize_xl_drawings_drawing1()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("<?xml version=\"1.0\" encoding=\"UTF-8\" standalone=\"yes\"?>");
            sb.Append("<xdr:wsDr xmlns:xdr=\"http://schemas.openxmlformats.org/drawingml/2006/spreadsheetDrawing\" xmlns:a=\"http://schemas.openxmlformats.org/drawingml/2006/main\">");
            sb.Append("<xdr:twoCellAnchor editAs=\"oneCell\">");
            sb.Append("<xdr:from>");
            sb.Append("<xdr:col>0</xdr:col>");
            sb.Append("<xdr:colOff>457200</xdr:colOff>");
            sb.Append("<xdr:row>2</xdr:row>");
            sb.Append("<xdr:rowOff>19050</xdr:rowOff>");
            sb.Append("</xdr:from>");
            sb.Append("<xdr:to>");
            sb.Append("<xdr:col>6</xdr:col>");
            sb.Append("<xdr:colOff>469900</xdr:colOff>");
            sb.Append("<xdr:row>39</xdr:row>");
            sb.Append("<xdr:rowOff>25400</xdr:rowOff>");
            sb.Append("</xdr:to>");
            sb.Append("<xdr:pic>");
            sb.Append("<xdr:nvPicPr>");
            sb.Append("<xdr:cNvPr id=\"2\" name=\"图片 1\"/>");
            sb.Append("<xdr:cNvPicPr>");
            sb.Append("<a:picLocks noChangeAspect=\"1\"/>");
            sb.Append("</xdr:cNvPicPr>");
            sb.Append("</xdr:nvPicPr>");
            sb.Append("<xdr:blipFill>");
            sb.Append("<a:blip xmlns:r=\"http://schemas.openxmlformats.org/officeDocument/2006/relationships\" r:embed=\"rId1\">");
            sb.Append("<a:extLst>");
            sb.Append("<a:ext uri=\"{28A0092B-C50C-407E-A947-70E740481C1C}\">");
            sb.Append("<a14:useLocalDpi xmlns:a14=\"http://schemas.microsoft.com/office/drawing/2010/main\" val=\"0\"/>");
            sb.Append("</a:ext>");
            sb.Append("</a:extLst>");
            sb.Append("</a:blip>");
            sb.Append("<a:stretch>");
            sb.Append("<a:fillRect/>");
            sb.Append("</a:stretch>");
            sb.Append("</xdr:blipFill>");
            sb.Append("<xdr:spPr>");
            sb.Append("<a:xfrm>");
            sb.Append("<a:off x=\"457200\" y=\"361950\"/>");
            sb.Append("<a:ext cx=\"4127500\" cy=\"6350000\"/>");
            sb.Append("</a:xfrm>");
            sb.Append("<a:prstGeom prst=\"rect\">");
            sb.Append("<a:avLst/>");
            sb.Append("</a:prstGeom>");
            sb.Append("</xdr:spPr>");
            sb.Append("</xdr:pic>");
            sb.Append("<xdr:clientData/>");
            sb.Append("</xdr:twoCellAnchor>");
            sb.Append("</xdr:wsDr>");
            File.WriteAllText(xl_drawings_drawing1, Convert.ToString(sb));
        }
        /// <summary>
        /// xl_drawings_rels_drawing1
        /// </summary>
        private void Initialize_xl_drawings_rels_drawing1()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("<?xml version=\"1.0\" encoding=\"UTF-8\" standalone=\"yes\"?>");
            sb.Append("<Relationships xmlns=\"http://schemas.openxmlformats.org/package/2006/relationships\">");
            sb.Append("<Relationship Id=\"rId1\" Type=\"http://schemas.openxmlformats.org/officeDocument/2006/relationships/image\" Target=\"../media/image1.jpg\"/>");
            sb.Append("</Relationships>");
            File.WriteAllText(xl_drawings_rels_drawing1, Convert.ToString(sb));
        }
        /// <summary>
        /// xl_media_image1
        /// </summary>
        private void Initialize_xl_media_image1(Image img)
        {
            if (dts.Length == 1 && img != null)
            {
                img.Save(xl_media_image1, ImageFormat.Jpeg);
            }
        }
        #endregion

        #region 创建文件
        public void CreateXlsx()
        {
            CreateXlsx(false, columnName.ColumnName, null);
        }
        public void CreateXlsx(String[][] head)
        {
            CreateXlsx(false, columnName.ColumnName, head);
        }
        public void CreateXlsx(columnName cn)
        {
            CreateXlsx(false, cn, null);
        }
        public void CreateXlsx(columnName cn, String[][] head)
        {
            CreateXlsx(false, cn, head);
        }
        public void CreateXlsx(bool isCombine, columnName cn, String[][] head)
        {
            try
            {
                //写数据
                WriteData(isCombine, cn, head);
                ZipFileDictory(xlsxTemp, root, null);
                //SevenZipCompressor tmp = new SevenZipCompressor();
                //tmp.ArchiveFormat = OutArchiveFormat.Zip;
                //tmp.CompressDirectory(xlsxTemp, root);
            }
            finally
            {
                if (Directory.Exists(xlsxTemp))
                {
                    Directory.Delete(xlsxTemp, true);
                }
            }
        }
        #endregion

        #region 压缩文件夹
        /// <summary>
        /// 寻找文件夹（当前文件夹）
        /// </summary>
        /// <returns></returns>
        private String[] FindFolders(String FolderToZip)
        {
            if (Directory.Exists(FolderToZip))
            {
                return Directory.GetDirectories(FolderToZip);
            }
            return null;
        }

        /// <summary>
        /// 寻找文件（当前文件夹）
        /// </summary>
        /// <returns></returns>
        private String[] FindFiles(String FolderToZip)
        {
            if (Directory.Exists(FolderToZip))
            {
                return Directory.GetFiles(FolderToZip);
            }
            return null;
        }

        /// <summary>
        /// 压缩目录
        /// </summary>
        /// <param name="FolderToZip">待压缩的文件夹，全路径格式</param>
        /// <param name="ZipedFile">压缩后的文件名，全路径格式</param>
        /// <param name="Password">密码</param>
        /// <returns></returns>
        public bool ZipFileDictory(String FolderToZip, String ZipedFile, String Password)
        {
            if (!Directory.Exists(FolderToZip))
            {
                return false;
            }
            if (!FolderToZip.EndsWith("\\"))
            {
                FolderToZip += "\\";
            }
            ZipEntry entry = null;
            FileStream fs = null;
            String[] files;
            String[] folders;
            String virtualPath = null;
            Crc32 crc = new Crc32();
            ZipOutputStream s = new ZipOutputStream(File.Create(ZipedFile));
            s.SetLevel(6);
            s.Password = Password;
            try
            {
                files = FindFiles(FolderToZip);
                folders = FindFolders(FolderToZip);

                #region 压缩文件
                foreach (String file in files)
                {
                    virtualPath = file.Replace(FolderToZip, "");
                    //打开压缩文件
                    fs = File.OpenRead(file);
                    byte[] buffer = new byte[fs.Length];
                    fs.Read(buffer, 0, buffer.Length);
                    entry = new ZipEntry(virtualPath);
                    entry.DateTime = DateTime.Now;
                    entry.Size = fs.Length;
                    fs.Close();
                    crc.Reset();
                    crc.Update(buffer);
                    entry.Crc = crc.Value;
                    s.PutNextEntry(entry);
                    s.Write(buffer, 0, buffer.Length);
                }
                #endregion

                #region 压缩文件夹
                foreach (String folder in folders)
                {
                    virtualPath = folder.Replace(FolderToZip, "");
                    ZipFileDictory(folder, s, "", null);
                }
                #endregion

            }
            catch
            {
                return false;
            }
            finally
            {
                if (fs != null)
                {
                    fs.Close();
                    fs = null;
                }
                if (entry != null)
                {
                    entry = null;
                }
                GC.Collect();
                GC.Collect(1);
            }

            s.Finish();
            s.Close();
            return true;
        }

        /// <summary>
        /// 递归压缩文件夹方法
        /// </summary>
        /// <param name="FolderToZip">String</param>
        /// <param name="s">ZipOutputStream</param>
        /// <param name="ParentFolderName">String</param>
        /// <param name="dirFilters">String列表</param>
        private bool ZipFileDictory(String FolderToZip, ZipOutputStream s, String ParentFolderName, List<String> dirFilters)
        {
            bool res = true;
            String[] folders, filenames;
            ZipEntry entry = null;
            FileStream fs = null;
            Crc32 crc = new Crc32();
            try
            {
                //创建当前文件夹
                entry = new ZipEntry(Path.Combine(ParentFolderName, Path.GetFileName(FolderToZip) + "/"));  //加上 “/” 才会当成是文件夹创建
                s.PutNextEntry(entry);
                s.Flush();
                if (dirFilters == null)
                {
                    //先压缩文件，再递归压缩文件夹 
                    filenames = Directory.GetFiles(FolderToZip);
                    foreach (String file in filenames)
                    {
                        //打开压缩文件
                        fs = File.OpenRead(file);
                        byte[] buffer = new byte[fs.Length];
                        fs.Read(buffer, 0, buffer.Length);
                        entry = new ZipEntry(Path.Combine(ParentFolderName, Path.GetFileName(FolderToZip) + "/" + Path.GetFileName(file)));
                        entry.DateTime = DateTime.Now;
                        entry.Size = fs.Length;
                        fs.Close();
                        crc.Reset();
                        crc.Update(buffer);
                        entry.Crc = crc.Value;
                        s.PutNextEntry(entry);
                        s.Write(buffer, 0, buffer.Length);
                    }
                }
            }
            catch
            {
                res = false;
            }
            finally
            {
                if (fs != null)
                {
                    fs.Close();
                    fs = null;
                }
                if (entry != null)
                {
                    entry = null;
                }
                GC.Collect();
                GC.Collect(1);
            }
            folders = Directory.GetDirectories(FolderToZip);
            foreach (String folder in folders)
            {
                if (dirFilters != null && !dirFilters.Contains(folder))
                {
                    continue;
                }
                if (!ZipFileDictory(folder, s, Path.Combine(ParentFolderName, Path.GetFileName(FolderToZip)), null))
                {
                    return false;
                }
            }
            return res;
        }
        #endregion

        #region 注释
        //#region 读取模板

        ///// <summary>  
        ///// 复制文件夹  
        ///// </summary>  
        ///// <param name="sourceFolder">待复制的文件夹</param>  
        ///// <param name="destFolder">复制到的文件夹</param>  
        //private void CopyFolder(String sourceFolder, String destFolder)
        //{
        //    if (!Directory.Exists(destFolder))
        //    {
        //        Directory.CreateDirectory(destFolder);
        //    }
        //    String[] files = Directory.GetFiles(sourceFolder);
        //    foreach (String file in files)
        //    {
        //        String name = Path.GetFileName(file);
        //        String dest = Path.Combine(destFolder, name);
        //        File.Copy(file, dest);
        //    }
        //    String[] folders = Directory.GetDirectories(sourceFolder);
        //    foreach (String folder in folders)
        //    {
        //        if (!folder.ToLower().Contains(".svn"))
        //        {
        //            String name = Path.GetFileName(folder);
        //            String dest = Path.Combine(destFolder, name);
        //            CopyFolder(folder, dest);
        //        }
        //    }
        //}

        //#endregion
        #endregion

        #region 写数据

        /// <summary>
        /// 写数据(多表头)
        /// </summary>
        private void WriteData(bool isCombine, columnName cn, String[][] head)
        {
            if (dts == null)
            {
                return;
            }
            long index = 0;
            int tableNum = 0;
            using (StreamWriter sr = new StreamWriter(xl_sharedStrings, false, Encoding.UTF8))
            {
                long cout = 0;
                foreach (System.Data.DataTable dt in dts)
                {
                    cout += dt.Columns.Count * dt.Rows.Count;
                }
                String begin_sharedStrings = "<?xml version=\"1.0\" encoding=\"UTF-8\" standalone=\"yes\"?><sst xmlns=\"http://schemas.openxmlformats.org/spreadsheetml/2006/main\" count=\"" + cout + "\" uniqueCount=\"" + cout + "\">";
                sr.Write(begin_sharedStrings);
                foreach (System.Data.DataTable dt in dts)
                {
                    StringBuilder sb = new StringBuilder();
                    StringBuilder colunmWithSb = new StringBuilder();
                    int rowBegin = 1;
                    if (image != null)
                    {
                        rowBegin = image.Height / 10;
                    }
                    int row = rowBegin;
                    int asciiNum = 1;
                    String begin = "<?xml version=\"1.0\" encoding=\"UTF-8\" standalone=\"yes\"?><worksheet xmlns=\"http://schemas.openxmlformats.org/spreadsheetml/2006/main\" xmlns:r=\"http://schemas.openxmlformats.org/officeDocument/2006/relationships\" xmlns:mc=\"http://schemas.openxmlformats.org/markup-compatibility/2006\" mc:Ignorable=\"x14ac\" xmlns:x14ac=\"http://schemas.microsoft.com/office/spreadsheetml/2009/9/ac\"><dimension ref=\"A1:H1\"/><sheetViews><sheetView tabSelected=\"1\" workbookViewId=\"0\"><selection activeCell=\"H1\" sqref=\"H1\"/></sheetView></sheetViews><sheetFormatPr defaultRowHeight=\"13.5\" x14ac:dyDescent=\"0.15\"/>";
                    String end = "</sheetData><phoneticPr fontId=\"1\" type=\"noConversion\"/><pageMargins left=\"0.7\" right=\"0.7\" top=\"0.75\" bottom=\"0.75\" header=\"0.3\" footer=\"0.3\"/></worksheet>";
                    if (dts.Length == 1 && image != null)
                    {
                        end = "</sheetData><phoneticPr fontId=\"1\" type=\"noConversion\"/><pageMargins left=\"0.7\" right=\"0.7\" top=\"0.75\" bottom=\"0.75\" header=\"0.3\" footer=\"0.3\"/><drawing r:id=\"rId1\"/></worksheet>";
                    }
                    if (colunmWith == null)
                    {
                        colunmWith = new List<int>();
                        String[] array = null;
                        if (head != null && head[0] != null&&head[0].Length>0)
                        {
                            for (int i = 0; i < head[tableNum].Length; i++)
                            {
                                int num = 0;
                                //array = head[tableNum][i].ToString().Split(new char[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
                                //foreach (String str in array)
                                //{
                                //    if (num < str.Length)
                                //    {
                                //        num = str.Length;
                                //    }
                                //}
                                num = head[tableNum][i].Length;
                                if (num > 10)
                                {
                                    num = 10;
                                }
                                colunmWith.Add(num * 16);
                            }
                        }
                        else
                        {
                            for (int i = 0; i < dt.Columns.Count; i++)
                            {
                                int num = 0;
                                switch (cn)
                                {
                                    case columnName.Nono:
                                        {
                                            array = dt.Rows[0][i].ToString().Split(new char[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
                                        }
                                        break;
                                    case columnName.Caption:
                                        {
                                            array = dt.Columns[i].Caption.Split(new char[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
                                        };
                                        break;
                                    default:
                                        {
                                            array = dt.Columns[i].ColumnName.Split(new char[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
                                        };
                                        break;
                                }
                                foreach (String str in array)
                                {
                                    if (num < str.Length)
                                    {
                                        num = str.Length;
                                    }
                                }
                                colunmWith.Add(num * 16);
                            }
                        }
                    }
                    //列宽
                    colunmWithSb.Append("<cols>");
                    for (int i = 0; i < colunmWith.Count; i++)
                    {
                        colunmWithSb.Append("<col min=\"" + (i + 1) + "\" max=\"" + (i + 1) + "\" width=\"" + ((double)colunmWith[i] / 8.5 + 2) + "\" bestFit=\"1\" customWidth=\"1\"/>");
                    }
                    colunmWithSb.Append("</cols>");

                    using (StreamWriter st = new StreamWriter(xl_worksheets_sheet[tableNum], false, Encoding.UTF8))
                    {
                        st.Write(begin);
                        st.Write(colunmWithSb.ToString());
                        st.Write("<sheetData>");
                        if (head == null)
                        {
                            //换行数
                            int lineNumber = 1;
                            //表头
                            for (int i = 0; i < dt.Columns.Count; i++)
                            {
                                sb.Append("<c r=\"" + ToNumberSystem26(asciiNum) + "" + row + "\" s=\"3\" t=\"s\"><v>" + index + "</v></c>");
                                switch (cn)
                                {
                                    case columnName.Nono:
                                        {
                                            sr.Write("<si><t>" + dt.Rows[0][i] + "</t></si>");
                                            int num = (dt.Rows[0][i].ToString().Length - dt.Rows[0][i].ToString().Replace("\r\n", "").Length) / 2;
                                            if (num > lineNumber)
                                            {
                                                lineNumber = num;
                                            }
                                        }; break;                                    
										case columnName.Caption:
                                        {
                                            sr.Write("<si><t>" + dt.Columns[i].Caption + "</t></si>");
                                            int num = (dt.Columns[i].Caption.Length - dt.Columns[i].Caption.Replace("\r\n", "").Length) / 2;
                                            if (num > lineNumber)
                                            {
                                                lineNumber = num;
                                            }

                                        };
                                        break;
                                    default:
                                        {
                                            sr.Write("<si><t>" + dt.Columns[i].ColumnName + "</t></si>");
                                            int num = (dt.Columns[i].ColumnName.Length - dt.Columns[i].ColumnName.Replace("\r\n", "").Length) / 2;
                                            if (num > lineNumber)
                                            {
                                                lineNumber = num;
                                            }
                                        };
                                        break;
                                }
                                asciiNum++;
                                index++;
                            }
                            sb.Append("</row>");
                            //写出
                            st.Write("<row r=\"" + row + "\" spans=\"1:" + dt.Columns.Count.ToString() + "\" ht=\"" + lineNumber * 16 + "\" customHeight=\"1\" x14ac:dyDescent=\"0.15\">");
                            st.Write(sb.ToString());
                            row++;
                            asciiNum = 1;
                        }
                        else
                        {
                            for (int i = 0; i < head.Length; i++)
                            {
                                sb.Append("<row r=\"" + row + "\" spans=\"1:" + head[i].Length + "\" x14ac:dyDescent=\"0.15\">");
                                //表头
                                for (int j = 0; j < head[i].Length; j++)
                                {
                                    sb.Append("<c r=\"" + ToNumberSystem26(asciiNum) + "" + row + "\" s=\"1\" t=\"s\"><v>" + index + "</v></c>");
                                    sr.Write("<si><t>" + head[i][j] + "</t></si>");
                                    asciiNum++;
                                    index++;
                                }
                                sb.Append("</row>");
                                //写出
                                st.Write(sb.ToString());
                                sb = new StringBuilder();
                                row++;
                                asciiNum = 1;
                            }
                        }
                        bool run = false;
                        if (columnName.Nono == cn)
                        {
                            run = true;
                        }                        //数据
                        for (int i = 0; i < dt.Rows.Count; i++)
                        {
                            if (run)
                            {
                                run = false;
                                continue;
                            }                            sb = new StringBuilder();
                            sb.Append("<row r=\"" + row + "\" spans=\"1:" + dt.Columns.Count.ToString() + "\" x14ac:dyDescent=\"0.15\">");
                            for (int j = 0; j < dt.Columns.Count; j++)
                            {
                                switch (dt.Columns[j].DataType.Name.ToLower())
                                {
                                    case "int16":
                                    case "int32":
                                    case "int64":
                                    case "double":
                                    case "single":
                                    case "decimal":
                                        {
                                            sb.Append("<c r=\"" + ToNumberSystem26(asciiNum) + "" + row + "\"><v>" + dt.Rows[i][j] + "</v></c>");
                                        }; break;
                                    default:
                                        {
                                            String strTemp = dt.Rows[i][j].ToString().Replace("&", "&amp;").Replace("<", "&lt;").Replace(">", "&gt;");
                                            if (strTemp.Length < 50)
                                            {
                                                sb.Append("<c r=\"" + ToNumberSystem26(asciiNum) + "" + row + "\" t=\"s\"><v>" + index + "</v></c>");
                                                sr.Write("<si><t>" + strTemp + "</t></si>");
                                            }
                                            else
                                            {
                                                sb.Append("<c r=\"" + ToNumberSystem26(asciiNum) + "" + row + "\" t=\"s\"><v>" + index + "</v></c>");
                                                sr.Write("<si><t xml:space=\"preserve\">" + strTemp + "</t></si>");
                                            }
                                            index++;
                                        }; break;
                                }
                                asciiNum++;
                            }
                            sb.Append("</row>");
                            st.Write(sb.ToString());
                            row++;
                            asciiNum = 1;
                        }
                        if (isCombine)
                        {
                            for (int i = 1; i < dts.Length; i++)
                            {
                                System.Data.DataTable dt2 = dts[i];
                                //数据
                                for (int j = 0; j < dt2.Rows.Count; j++)
                                {
                                    sb = new StringBuilder();
                                    sb.Append("<row r=\"" + row + "\" spans=\"1:" + dt2.Columns.Count.ToString() + "\" x14ac:dyDescent=\"0.15\">");
                                    for (int k = 0; k < dt2.Columns.Count; k++)
                                    {
                                        switch (dt2.Columns[k].DataType.Name.ToLower())
                                        {
                                            case "int16":
                                            case "int32":
                                            case "int64":
                                            case "double":
                                            case "single":
                                            case "decimal":
                                                {
                                                    sb.Append("<c r=\"" + ToNumberSystem26(asciiNum) + "" + row + "\"><v>" + dt2.Rows[j][k] + "</v></c>");
                                                }; break;
                                            default:
                                                {
                                                    String strTemp = dt2.Rows[j][k].ToString().Replace("&", "&amp;").Replace("<", "&lt;").Replace(">", "&gt;");
                                                    if (strTemp.Length < 50)
                                                    {
                                                        sb.Append("<c r=\"" + ToNumberSystem26(asciiNum) + "" + row + "\" t=\"s\"><v>" + index + "</v></c>");
                                                        sr.Write("<si><t>" + strTemp + "</t></si>");
                                                    }
                                                    else
                                                    {
                                                        sb.Append("<c r=\"" + ToNumberSystem26(asciiNum) + "" + row + "\" t=\"s\"><v>" + index + "</v></c>");
                                                        sr.Write("<si><t xml:space=\"preserve\">" + strTemp + "</t></si>");
                                                    }
                                                    index++;
                                                }; break;
                                        }
                                        asciiNum++;
                                    }
                                    sb.Append("</row>");
                                    st.Write(sb.ToString());
                                    row++;
                                    asciiNum = 1;
                                }
                            }
                        }
                        sb = new StringBuilder();
                        if (!isCombine)
                        {
                            sb.Append("<row r=\"" + (rowBegin + dt.Rows.Count + 8) + "\" spans=\"1:" + dt.Columns.Count.ToString() + "\" x14ac:dyDescent=\"0.15\">");
                            sb.Append("<c r=\"" + ToNumberSystem26(asciiNum) + "" + (rowBegin + dt.Rows.Count + 8) + "\" s=\"2\" t=\"s\"><v>" + index + "</v></c>");
                        }
                        else
                        {
                            int num = 0;
                            for (int i = 1; i < dts.Length; i++)
                            {
                                num += dts[i].Rows.Count;
                            }
                            sb.Append("<row r=\"" + (rowBegin + dt.Rows.Count + num + 8) + "\" spans=\"1:" + dt.Columns.Count.ToString() + "\" x14ac:dyDescent=\"0.15\">");
                            sb.Append("<c r=\"" + ToNumberSystem26(asciiNum) + "" + (dt.Rows.Count + rowBegin + num + 8) + "\" s=\"2\" t=\"s\"><v>" + index + "</v></c>");
                        }
                        sb.Append("</row>");
                        sr.Write("<si><t>数据来源：owchart</t></si>");
                        st.Write(sb.ToString());
                        st.Write(end.ToString());
                        index++;
                        st.Close();
                        if (isCombine)
                        {
                            break;
                        }                    }
                    tableNum++;
                }
                sr.Write("</sst>");
                sr.Close();
            }
        }

        /// <summary>
        /// 将指定的自然数转换为26进制表示。映射关系：[1-26] ->[A-Z]。
        /// </summary>
        /// <param name="n">自然数（如果无效，则返回空字符串）。</param>
        /// <returns>26进制表示。</returns>
        private String ToNumberSystem26(int n)
        {
            String s = String.Empty;
            while (n > 0)
            {
                int m = n % 26;
                if (m == 0) m = 26;
                s = (char)(m + 64) + s;
                n = (n - m) / 26;
            }
            return s;
        }

        /// <summary>
        /// 将指定的26进制表示转换为自然数。映射关系：[A-Z] ->[1-26]。
        /// </summary>
        /// <param name="s">26进制表示（如果无效，则返回0）。</param>
        /// <returns>自然数。</returns>
        private int FromNumberSystem26(String s)
        {
            if (String.IsNullOrEmpty(s)) return 0;
            int n = 0;
            for (int i = s.Length - 1, j = 1; i >= 0; i--, j *= 26)
            {
                char c = Char.ToUpper(s[i]);
                if (c < 'A' || c > 'Z') return 0;
                n += ((int)c - 64) * j;
            }
            return n;
        }


        #endregion

        #region 转换

        ///// <summary>
        ///// GridView转换为DataTable
        ///// </summary>
        ///// <param name="dgv"></param>
        ///// <returns></returns>
        //public System.Data.DataTable GetDgvToTable(GridView dgv)
        //{
        //    System.Data.DataTable dt = new System.Data.DataTable();
        //    colunmWith = new List<int>();
        //    for (int count = 0; count < dgv.Columns.Count; count++)
        //    {
        //        DataColumn dc = new DataColumn(dgv.Columns[count].Caption, dgv.Columns[count].ColumnType);
        //        colunmWith.Add(dgv.Columns[count].Width);
        //        dt.Columns.Add(dc);
        //    }
        //    for (int count = 0; count < dgv.RowCount; count++)
        //    {
        //        DataRow dr = dt.NewRow();
        //        for (int countsub = 0; countsub < dgv.Columns.Count; countsub++)
        //        {
        //            dr[countsub] = Convert.ToString(dgv.GetRowCellValue(count, dgv.Columns[countsub]));
        //        }
        //        dt.Rows.Add(dr);
        //    }
        //    return dt;
        //}

        /// <summary>
        /// DataView转换为DataTable
        /// </summary>
        /// <param name="dgv"></param>
        /// <returns></returns>
        public System.Data.DataTable GetDgvToTable(System.Data.DataView dgv)
        {
            System.Data.DataTable dt = dgv.Table;
            colunmWith = null;
            ////获取列宽
            //for (int count = 0; count < dgv.Table.Columns.Count; count++)
            //{
            //    DataColumn dc = new DataColumn(dgv.Table.Columns[count].Caption);
            //    colunmWith.Add(dgv.Table.Columns[count].w);
            //    dt.Columns.Add(dc);
            //}

            return dt;
        }
        #endregion

        #region Office版本

        public static int ExistsRegedit()
        {
            int ifused = 0;
            try
            {
                RegistryKey rk = Registry.LocalMachine;

                //查询Office2003
                RegistryKey f03 = rk.OpenSubKey(@"SOFTWARE\Microsoft\Office\11.0\Excel\InstallRoot\");

                String path2 = null;

                if (f03 != null)
                {
                    String path = f03.GetValue("Path").ToString();
                    path2 = Path.Combine(Directory.GetParent(path).FullName, @"Office12\Moc.exe");
                }

                //查询Office2007
                RegistryKey f07 = rk.OpenSubKey(@"SOFTWARE\Microsoft\Office\12.0\Excel\InstallRoot\");


                //查询Office2010
                RegistryKey f10 = rk.OpenSubKey(@"SOFTWARE\Microsoft\Office\14.0\Excel\InstallRoot\");


                //检查本机是否安装Office2003
                if (f03 != null)
                {
                    //String file03 = f03.GetValue("Path").ToString();
                    //if (File.Exists(file03 + "Excel.exe")) ifused += 1;
                    ifused = 1;
                }

                if (path2 != null && File.Exists(path2))
                {
                    ifused = 2;
                }

                //检查本机是否安装Office2007

                if (f07 != null)
                {
                    //String file07 = akey.GetValue("Path").ToString();
                    //if (File.Exists(file07 + "Excel.exe")) ifused += 2;
                    ifused = 2;
                }

                //检查本机是否安装Office2010

                if (f10 != null)
                {
                    //String file07 = akey.GetValue("Path").ToString();
                    //if (File.Exists(file07 + "Excel.exe")) ifused += 2;
                    ifused = 3;
                }
            }
            catch
            {
                //Log.WriteLog("读取OFFICE注册表错误");
                throw;
            }
            return ifused;
        }
        #endregion

        #endregion
    }
    #endregion

    #region dbf

    /// <summary>
    /// DBF类
    /// </summary>
    public class DBFHelper
    {
        /// <summary>
        /// 默认最大的列长
        /// </summary>
        public static byte MaxLength = 20;
        /// <summary>
        /// 文件每行分隔符或单元格内容占位符: 32
        /// </summary>
        public const byte FileRowSplitOrEmptyContentChar = 32;
        /// <summary>
        /// 文件版本号: 3
        /// </summary>
        public static byte FileVersion = 3;
        /// <summary>
        /// 文件头和字段结束符（回车键）: 13
        /// </summary>
        public const byte FileHeaderEndChar = 13;
        /// <summary>
        /// 文件结束符: 26
        /// </summary>
        public const byte FileEndChar = 26;

        /// <summary>
        /// This is the file header for a DBF. We do this special layout with everything
        /// packed so we can read straight from disk into the structure to populate it
        /// </summary>
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
        private struct DBFHeader
        {
            public byte version;
            public byte updateYear;
            public byte updateMonth;
            public byte updateDay;
            /// <summary>
            /// 记录数
            /// </summary>
            public Int32 numRecords;
            /// <summary>
            /// 文件头大小
            /// </summary>
            public Int16 headerLen;
            /// <summary>
            /// 记录字节长度
            /// </summary>
            public Int16 recordLen;
            public Int16 reserved1;
            public byte incompleteTrans;
            public byte encryptionFlag;
            public Int32 reserved2;
            public Int64 reserved3;
            public byte MDX;
            public byte language;
            public Int16 reserved4;
        }

        // This is the field descriptor structure. There will be one of these for each column in the table.
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
        // 字段的长度也为32字节
        private struct FieldDescriptor
        {
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 11)]
            public String fieldName;
            public char fieldType;
            // 字段的起始位置
            public Int32 address;
            public byte fieldLen;
            public byte count;
            public Int16 reserved1;
            public byte workArea;
            public Int16 reserved2;
            public byte flag;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 7)]
            public byte[] reserved3;
            public byte indexFlag;
        }

        /// <summary>
        /// 文件头部长度
        /// </summary>
        public static int HeaderSize = Marshal.SizeOf(typeof(DBFHeader));
        /// <summary>
        /// 字段描述符长度
        /// </summary>
        public static int FieldDescSize = Marshal.SizeOf(typeof(FieldDescriptor));

        /// <summary>
        /// 读取DBF
        /// </summary>
        /// <param name="dbfFile">文件名称</param>
        /// <returns>数据表格</returns>
        public static System.Data.DataTable ReadDBF(String dbfFile)
        {
            System.Data.DataTable dt = new System.Data.DataTable();
            BinaryReader recReader;
            String number;
            String year;
            String month;
            String day;
            DataRow row;

            // If there isn't even a file, just return an empty DataTable
            if (false == File.Exists(dbfFile))
            {
                return dt;
            }

            BinaryReader br = null;
            try
            {
                // Read the header into a buffer
                byte[] buffer;
                DBFHeader header;
                ArrayList fields;
                // 读取文件头和字段描述信息
                ReadHeaderAndFieldDesc(dbfFile, ref br, out buffer, out header, out fields);

                // Read in the first row of records, we need this to help determine column types below
                ((FileStream)br.BaseStream).Seek(header.headerLen + 1, SeekOrigin.Begin);
                buffer = br.ReadBytes(header.recordLen);
                recReader = new BinaryReader(new MemoryStream(buffer));
                // 为DataTable创建DataColumn
                CreateDataColumns(dt, recReader, fields);

                // Skip past the end of the header. 
                ((FileStream)br.BaseStream).Seek(header.headerLen, SeekOrigin.Begin);

                // Read in all the records
                for (int counter = 0; counter <= header.numRecords - 1; counter++)
                {
                    // First we'll read the entire record into a buffer and then read each field from the buffer
                    // This helps account for any extra space at the end of each record and probably performs better
                    buffer = br.ReadBytes(header.recordLen);
                    recReader = new BinaryReader(new MemoryStream(buffer));

                    // All dbf field records begin with a deleted flag field. Deleted - 0x2A (asterisk) else 0x20 (space)
                    if (recReader.ReadChar() == '*')
                    {
                        continue;
                    }

                    // Loop through each field in a record
                    row = dt.NewRow();
                    foreach (FieldDescriptor field in fields)
                    {
                        switch (field.fieldType)
                        {
                            case 'N':  // Number
                                // If you port this to .NET 2.0, use the Decimal.TryParse method
                                number = Encoding.Default.GetString(recReader.ReadBytes(field.fieldLen));
                                if (IsNumber(number))
                                {
                                    if (number.IndexOf(".") > -1)
                                    {
                                        row[field.fieldName] = decimal.Parse(number);
                                    }
                                    else
                                    {
                                        row[field.fieldName] = int.Parse(number);
                                    }
                                }
                                else
                                {
                                    row[field.fieldName] = 0;
                                }

                                break;

                            case 'C': // String
                                byte[] be = recReader.ReadBytes(field.fieldLen);
                                row[field.fieldName] = Encoding.Default.GetString(be).Trim();
                                break;

                            case 'D': // Date (YYYYMMDD)
                                year = Encoding.Default.GetString(recReader.ReadBytes(4));
                                month = Encoding.Default.GetString(recReader.ReadBytes(2));
                                day = Encoding.Default.GetString(recReader.ReadBytes(2));
                                row[field.fieldName] = System.DBNull.Value;
                                try
                                {
                                    if (IsNumber(year) && IsNumber(month) && IsNumber(day))
                                    {
                                        if (Int32.Parse(year) > 1900)
                                        {
                                            row[field.fieldName] = new DateTime(Int32.Parse(year), Int32.Parse(month), Int32.Parse(day));
                                        }
                                    }
                                }
                                catch
                                { }

                                break;

                            case 'L': // Boolean (Y/N)
                                if ('Y' == recReader.ReadByte())
                                {
                                    row[field.fieldName] = true;
                                }
                                else
                                {
                                    row[field.fieldName] = false;
                                }

                                break;

                            case 'F':
                                number = Encoding.Default.GetString(recReader.ReadBytes(field.fieldLen));
                                if (IsNumber(number))
                                {
                                    row[field.fieldName] = double.Parse(number);
                                }
                                break;
                        }
                    }

                    recReader.Close();
                    dt.Rows.Add(row);
                }
            }

            catch
            {
                throw;
            }
            finally
            {
                if (null != br)
                {
                    br.Close();
                }
            }

            return dt;
        }

        /// <summary>
        /// 为DataTable创建DataColumn
        /// </summary>
        /// <param name="dt"></param>
        /// <param name="recReader"></param>
        /// <param name="fields"></param>
        private static void CreateDataColumns(System.Data.DataTable dt, BinaryReader recReader, ArrayList fields)
        {
            String number;
            // Create the columns in our new DataTable
            DataColumn col = null;
            foreach (FieldDescriptor field in fields)
            {
                number = Encoding.Default.GetString(recReader.ReadBytes(field.fieldLen));
                switch (field.fieldType)
                {
                    case 'N':
                        if (number.IndexOf(".") > -1)
                        {
                            col = new DataColumn(field.fieldName, typeof(decimal));
                        }
                        else
                        {
                            col = new DataColumn(field.fieldName, typeof(int));
                        }
                        break;
                    case 'C':
                        col = new DataColumn(field.fieldName, typeof(String));
                        break;
                    case 'D':
                        col = new DataColumn(field.fieldName, typeof(DateTime));
                        break;
                    case 'L':
                        col = new DataColumn(field.fieldName, typeof(bool));
                        break;
                    case 'F':
                        col = new DataColumn(field.fieldName, typeof(Double));
                        break;
                }
                dt.Columns.Add(col);
            }
        }

        /// <summary>
        /// 读取文件头和字段描述信息
        /// </summary>
        /// <param name="dbfFile"></param>
        /// <param name="br"></param>
        /// <param name="buffer"></param>
        /// <param name="header"></param>
        /// <param name="fields"></param>
        /// <returns></returns>
        private static BinaryReader ReadHeaderAndFieldDesc(String dbfFile, ref BinaryReader br, out byte[] buffer, out DBFHeader header, out ArrayList fields)
        {
            br = new BinaryReader(File.OpenRead(dbfFile));
            buffer = br.ReadBytes(HeaderSize); // Marshal.SizeOf(typeof(DBFHeader)

            // Marshall the header into a DBFHeader structure
            GCHandle handle = GCHandle.Alloc(buffer, GCHandleType.Pinned);
            header = (DBFHeader)Marshal.PtrToStructure(handle.AddrOfPinnedObject(), typeof(DBFHeader));
            handle.Free();

            // Read in all the field descriptors. Per the spec, 13 (0D) marks the end of the field descriptors
            fields = new ArrayList();
            while (FileHeaderEndChar != br.PeekChar())
            {
                buffer = br.ReadBytes(FieldDescSize); // Marshal.SizeOf(typeof(FieldDescriptor))
                handle = GCHandle.Alloc(buffer, GCHandleType.Pinned);
                fields.Add((FieldDescriptor)Marshal.PtrToStructure(handle.AddrOfPinnedObject(), typeof(FieldDescriptor)));
                handle.Free();
            }
            return br;
        }

        /// <summary>
        /// Simple function to test is a String can be parsed. There may be a better way, but this works
        /// If you port this to .NET 2.0, use the new TryParse methods instead of this
        /// </summary>
        /// <param name="numberString">String to test for parsing</param>
        /// <returns>true if String can be parsed</returns>
        public static bool IsNumber(String numberString)
        {
            char[] numbers = numberString.ToCharArray();

            foreach (char number in numbers)
            {
                if ((number < 48 || number > 57) && number != 46 && number != 32)
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// 根据文件头，以及相关信息，将字段信息进行序列化
        /// </summary>
        /// <param name="address"></param>
        /// <param name="fieldLen">字段长度</param>
        /// <param name="fieldName">字段名称</param>
        /// <param name="fieldType">自大类型，如“C”，表示字段串</param>
        /// <param name="fieldSize">字段描述符长度</param>
        /// <param name="myBinWtr">欲写入的流</param>
        public static void recordHead(int address, byte fieldLen, String fieldName, char fieldType, int fieldSize, ref BinaryWriter myBinWtr)
        {
            FieldDescriptor fiDes = new FieldDescriptor();
            fiDes.address = address;
            fiDes.fieldLen = fieldLen;
            fiDes.fieldName = fieldName;
            fiDes.fieldType = fieldType;
            byte[] buffer = new byte[fieldSize];
            IntPtr ps = Marshal.AllocHGlobal(fieldSize);
            Marshal.StructureToPtr(fiDes, ps, true);
            for (int i = 0; i < fieldSize; i++)
            {
                buffer[i] = Marshal.ReadByte(ps, i);
            }

            myBinWtr.Write(buffer);
        }

        /// <summary>
        /// 将Datatable文件写入DBF文件中
        /// </summary>
        /// <param name="dt"></param>
        /// <param name="filePathAndName"></param>
        /// <returns></returns>
        public static bool WriteDBF(System.Data.DataTable dt, String filePathAndName)
        {
            return WriteDBF(dt, filePathAndName, false, DateTime.Now.ToString("yyyyMMdd"), DateTime.Now.ToString("hh:mm"));
        }

        /// <summary>
        /// 将Datatable文件写入DBF文件中
        /// </summary>
        /// <param name="dt">DataTable</param>
        /// <param name="filePathAndName">文件名</param>
        /// <param name="bAddTimeRow">是否加入时间行</param>
        /// <param name="date">生成日期</param>
        /// <param name="time">生成时间</param>
        /// <returns></returns>
        public static bool WriteDBF(System.Data.DataTable dt, String filePathAndName, bool bAddTimeRow, String date, String time)
        {
            MemoryStream ms = new MemoryStream();
            BinaryWriter myBinWtr = new BinaryWriter(ms);
            bool result = true;
            try
            {
                byte year = Byte.Parse(DateTime.Now.ToString("yy"));
                byte month = (byte)DateTime.Now.Month;
                byte day = (byte)DateTime.Now.Day;
                int recordLen = 0;
                String targetFileName = filePathAndName;
                byte[] columnLength = new byte[dt.Columns.Count];
                // 计算所有列最大的总长度
                recordLen = GetSumColumnLength(dt, columnLength);
                recordLen += 1; // 再增加一个文件头和字段数据间空白符的长度

                DBFHeader header = GenerateHeaderData(dt, bAddTimeRow, year, month, day, recordLen);
                // 写入文件头相关数据写入流中
                WriteHeaderToStream(ref myBinWtr, header);
                // 写入字段相关数据
                WriteFieldDescriptorToStream(dt, ref myBinWtr, columnLength);

                myBinWtr.Write(FileHeaderEndChar);//文件头结束

                #region 是否需要日期和时间
                if (bAddTimeRow)
                {
                    WriteDateTimeRow(dt, date, time, ref myBinWtr, columnLength);
                }
                #endregion
                DataRow dr = null;
                //object ob = null;
                String sValue = "";
                //DataColumn dc;
                // 写入内容，各行之前，以空格符分隔
                for (int k = 0; k < dt.Rows.Count; k++)
                {
                    dr = dt.Rows[k];
                    //ob = dr.ItemArray;
                    myBinWtr.Write(FileRowSplitOrEmptyContentChar);

                    for (int i = 0; i < dt.Columns.Count; i++)
                    {
                        //dc = dt.Columns[i];
                        if (dr.IsNull(i))
                        {
                            sValue = String.Empty;
                        }
                        else
                        {
                            sValue = dr[i].ToString();
                        }
                        //if (dc.ColumnName.ToUpper() == "QUOTEDTIME")
                        //{
                        //    sValue = WhDataOperator.ReadDateTimeByName(dr, dc.ColumnName, DateTime.Now).ToString("HH:mm:ss");//.TimeOfDay.ToString();
                        //}
                        //else if (dc.ColumnName.ToUpper() == "QUOTEDDATE" || dc.ColumnName.ToUpper() == "DEALDATE")
                        //{
                        //    sValue = WhDataOperator.ReadDateTimeByName(dr, dc.ColumnName, DateTime.Now).ToString("yyyyMMdd");
                        //}
                        //else
                        //    sValue = WhDataOperator.ReadStringByName(dr, dc.ColumnName, "");

                        WriteCellContentToStream(sValue, columnLength[i], ref myBinWtr);
                    }
                }

                myBinWtr.Write(FileEndChar); // 写入结束标志符

                WriteDBF(ms, targetFileName);
            }
            catch
            {
                result = false;
            }
            finally
            {
                myBinWtr.Close();
                ms.Close();
            }

            return result;
        }

        private static void WriteDBF(MemoryStream ms, String targetFileName)
        {
            int index = 0;
            String mesage = "";
            for (index = 0; index < 5; index++)//最多重试5次
            {
                try
                {
                    using (FileStream writeFileStream = new FileStream(targetFileName, FileMode.Create, FileAccess.Write, FileShare.ReadWrite))
                    {
                        writeFileStream.Write(ms.ToArray(), 0, (int)ms.Length);
                    }

                    break;
                }
                catch (IOException e)
                {
                    mesage = e.Message;
                    Thread.Sleep(1000);
                    continue;
                }
            }
            if (index >= 5)
            {
                //log.Info(mesage);
            }
        }

        /// <summary>
        /// 写入时间行
        /// </summary>
        /// <param name="dt"></param>
        /// <param name="date"></param>
        /// <param name="time"></param>
        /// <param name="myBinWtr"></param>
        /// <param name="columnLength"></param>
        private static void WriteDateTimeRow(System.Data.DataTable dt, String date, String time, ref BinaryWriter myBinWtr, byte[] columnLength)
        {
            //d = 32;//空格
            myBinWtr.Write(FileRowSplitOrEmptyContentChar);
            String cellContent = String.Empty;

            for (int i = 0; i < dt.Columns.Count; i++)
            {
                if (i == 0)
                {
                    cellContent = date; // 写入日期列
                }
                else if (i == 1)
                {
                    cellContent = time;   // 写入时间列
                }
                else
                {
                    cellContent = String.Empty;
                }

                WriteCellContentToStream(cellContent, columnLength[i], ref myBinWtr);
            }
        }

        /// <summary>
        /// 根据每列最大长度, 填充某单元格第i行第j列的内容。少于该列最大长度的补充空白字节
        /// 假设某列最大长度为20个字节，实际为4个自节，则须要添加16个空白字符
        /// </summary>
        /// <param name="cellContent">单元格内容</param>
        /// <param name="columnMaxLength">列最大行</param>
        /// <param name="myBinWtr">二进制写入</param>
        private static void WriteCellContentToStream(String cellContent, int columnMaxLength, ref BinaryWriter myBinWtr)
        {
            // 写入日期列
            byte[] byteArray = System.Text.Encoding.Default.GetBytes(cellContent);
            byte[] MaxByteArray = new byte[columnMaxLength];
            // 根据每列最大长度填充第i行第j列的内容。不足长度的补充空白字节
            for (int j = 0; j < MaxByteArray.Length; j++)
            {
                if (j < byteArray.Length)
                {
                    MaxByteArray[j] = byteArray[j];
                }
                else
                {
                    MaxByteArray[j] = FileRowSplitOrEmptyContentChar;
                }
            }

            myBinWtr.Write(MaxByteArray);
        }

        /// <summary>
        /// 写入字段相关数据
        /// </summary>
        /// <param name="dt"></param>
        /// <param name="myBinWtr"></param>
        /// <param name="columnLength"></param>
        private static void WriteFieldDescriptorToStream(System.Data.DataTable dt, ref BinaryWriter myBinWtr, byte[] columnLength)
        {
            // 根据列，序列化字段描述信息，目录只支持字段串
            for (int i = 0; i < dt.Columns.Count; i++)
            {
                recordHead(columnLength[i] + 1, columnLength[i], dt.Columns[i].Caption, 'C', FieldDescSize, ref myBinWtr);
            }
        }

        /// <summary>
        /// 写入文件头相关数据写入流中
        /// </summary>
        /// <param name="myBinWtr">二进制写入</param>
        /// <param name="header">DBH类</param>
        private static void WriteHeaderToStream(ref BinaryWriter myBinWtr, DBFHeader header)
        {
            IntPtr ps = Marshal.AllocHGlobal(HeaderSize); // ps指向在非托管内存中为文件头分配的内存区域
            Marshal.StructureToPtr(header, ps, true); // 将DBF头部的托管数据封送到非托管内存中。
            // 相当于结构体的二进制的序列化
            byte[] buffer = new byte[HeaderSize];
            for (int i = 0; i < HeaderSize; i++)
            {
                buffer[i] = Marshal.ReadByte(ps, i);
            }
            // 写入文件头数据
            myBinWtr.Write(buffer);
        }

        /// <summary>
        /// 产生文件头部信息
        /// </summary>
        /// <param name="dt"></param>
        /// <param name="bAddTimeRow"></param>
        /// <param name="year"></param>
        /// <param name="month"></param>
        /// <param name="day"></param>
        /// <param name="recordLen"></param>
        /// <returns></returns>
        private static DBFHeader GenerateHeaderData(System.Data.DataTable dt, bool bAddTimeRow, byte year, byte month, byte day, int recordLen)
        {
            DBFHeader header = new DBFHeader();
            header.version = FileVersion;
            header.updateYear = year;
            header.updateMonth = month;
            header.updateDay = day;

            if (bAddTimeRow)
            {
                header.numRecords = dt.Rows.Count + 1;
            }
            else
            {
                header.numRecords = dt.Rows.Count;
            }
            header.recordLen = (short)recordLen;
            header.headerLen = (short)(HeaderSize + FieldDescSize * dt.Columns.Count + 1); // 文件头长度 * 字段长度 * 列个数 + 1

            return header;
        }

        /// <summary>
        /// 计算所有列最大的总长度
        /// </summary>
        /// <param name="dt">Datatable对象</param>
        /// <param name="columnLength">列长</param>
        /// <returns>最大的总长度</returns>
        private static int GetSumColumnLength(System.Data.DataTable dt, byte[] columnLength)
        {
            byte currentColumnLength = MaxLength;
            DataColumn column = null;
            int recordLen = 0;

            // 先计算所有最大列的长度
            for (int i = 0; i < dt.Columns.Count; i++)
            {
                column = dt.Columns[i];
                if (column.DataType == typeof(String) && column.MaxLength > 0)
                {
                    currentColumnLength = (byte)column.MaxLength;
                }

                columnLength[i] = currentColumnLength;
                recordLen += currentColumnLength;
            }

            return recordLen;
        }

        /// <summary>
        /// 将DataTable数据类型转成DBF数据类型
        /// </summary>
        /// <param name="dt"></param>
        /// <param name="filePathAndName"></param>
        public static void DataTableToDBFRealDataType(System.Data.DataTable dt, String filePathAndName)
        {
            String time = DateTime.Now.ToString("yy");
            byte year = Byte.Parse(time);
            byte month = (byte)DateTime.Now.Month;
            byte day = (byte)DateTime.Now.Day;
            int recordLen = 0;
            byte[] columnLength = new byte[dt.Columns.Count];
            for (int i = 0; i < dt.Columns.Count; i++)
            {

                if (dt.Columns[i].DataType == typeof(String))
                {
                    if (dt.Columns[i].MaxLength == -1)
                    {
                        columnLength[i] = 60;
                    }
                    else
                    {
                        columnLength[i] = (byte)dt.Columns[i].MaxLength;
                    }
                }
                else
                {
                    columnLength[i] = 20;
                }

                recordLen += columnLength[i];
            }
            recordLen += 1;
            DBFHeader header = new DBFHeader();
            header.version = 3;
            header.updateYear = year;
            header.updateMonth = month;
            header.updateDay = day;
            header.numRecords = dt.Rows.Count;
            header.recordLen = (short)recordLen;
            header.headerLen = (short)(32 + 32 * dt.Columns.Count + 1);
            IntPtr pnt = Marshal.AllocHGlobal(Marshal.SizeOf(header));

            int size = Marshal.SizeOf(typeof(DBFHeader));
            IntPtr ps = Marshal.AllocHGlobal(size);
            Marshal.StructureToPtr(header, ps, true);
            byte[] buffer = new byte[size];
            for (int i = 0; i < size; i++)
            {
                buffer[i] = Marshal.ReadByte(ps, i);
            }
            String targetFileName = filePathAndName;
            FileStream writeFileStream = new FileStream(targetFileName, FileMode.Create, FileAccess.Write);
            BinaryWriter myBinWtr = new BinaryWriter(writeFileStream);
            myBinWtr.Write(buffer);
            int fieldSize = Marshal.SizeOf(typeof(FieldDescriptor));
            for (int i = 0; i < dt.Columns.Count; i++)
            {
                char type = 'C';
                if (dt.Columns[i].DataType == typeof(String))
                    type = 'C';
                else if (dt.Columns[i].DataType == typeof(decimal) || dt.Columns[i].DataType == typeof(int))
                    type = 'N';
                else if (dt.Columns[i].DataType == typeof(DateTime))
                    type = 'C';
                else if (dt.Columns[i].DataType == typeof(bool))
                    type = 'L';
                else if (dt.Columns[i].DataType == typeof(Double))
                    type = 'F';

                recordHead(columnLength[i] + 1, columnLength[i], dt.Columns[i].Caption, type, fieldSize, ref myBinWtr);
            }


            byte d = 13;
            myBinWtr.Write(d);

            for (int k = 0; k < dt.Rows.Count; k++)
            {
                DataRow dr = dt.Rows[k];
                object ob = dr.ItemArray;

                d = 32;
                myBinWtr.Write(d);
                for (int i = 0; i < dt.Columns.Count; i++)
                {
                    byte[] byteArray = System.Text.Encoding.Default.GetBytes(dr[i].ToString());
                    byte[] MaxByteArray = new byte[columnLength[i]];
                    for (int j = 0; j < MaxByteArray.Length; j++)
                    {
                        if (j < byteArray.Length)
                        {
                            MaxByteArray[j] = byteArray[j];
                        }
                        else
                        {
                            MaxByteArray[j] = 32;
                        }
                    }
                    myBinWtr.Write(MaxByteArray);

                }
            }
            d = 26;
            myBinWtr.Write(d);
            myBinWtr.Close();
            writeFileStream.Close();

        }

    }

    #endregion
}
